<?php
declare(strict_types = 1);
namespace SubscriptionServer;

function customerFor(array $user): string {
    $existing = query('SELECT customer_id FROM billing_customers WHERE user_id=?', [$user['id']])->fetchColumn();
    if ($existing) return (string)$existing;
    $customer = gateway()->customer($user);
    query('INSERT INTO billing_customers(user_id,customer_id) VALUES(?,?)', [$user['id'], $customer]);
    return $customer;
}

function synchronize(int $userId): void {
    // One account lock spans provider retrieval and application of that snapshot.
    // An older concurrent request cannot apply after a newer reconciliation.
    transaction(function () use ($userId) {
        $user = query('SELECT * FROM users WHERE id=? FOR UPDATE', [$userId])->fetch();
        if (!$user) return;
        $customer = query('SELECT customer_id FROM billing_customers WHERE user_id=?', [$userId])->fetchColumn();
        $subscriptions = $customer?gateway()->subscriptions((string)$customer):[];
        $plans = query('SELECT * FROM plans WHERE game_id=?', [$user['game_id']])->fetchAll();
        $known = array_column($plans, null, 'price_id');
        query('DELETE FROM subscriptions WHERE user_id=?', [$userId]);
        foreach ($subscriptions as $sub) {
            if (($sub['customer'] ?? '') !== $customer) throw new ApiError('provider_mismatch', 'Subscription ownership mismatch.', 503);
            $chosen = null;
            foreach ($sub['items']['data'] ?? [] as $item) {
                if (!isset($known[$item['price']['id'] ?? ''])) continue;
                if ($chosen === null || (int)($item['current_period_end'] ?? 0)>(int)($chosen['current_period_end'] ?? 0)) $chosen = $item;
            }
            if ($chosen === null) continue;
            $product = $chosen['price']['product'] ?? '';
            if (is_array($product)) $product = $product['id'] ?? '';
            $method = $sub['default_payment_method'] ?? '';
            if (is_array($method)) $method = $method['id'] ?? '';
            query('INSERT INTO subscriptions(subscription_id,user_id,price_id,product_id,payment_method_id,status,started_at,period_end,cancel_at_period_end,canceled_at,synced_at) VALUES(?,?,?,?,?,?,?,?,?,?,?)', [ $sub['id'], $userId, $chosen['price']['id'], $product, $method, $sub['status'], (int)($sub['start_date'] ?? $sub['created'] ?? 0), (int)($chosen['current_period_end'] ?? $sub['current_period_end'] ?? 0), (int)($sub['cancel_at_period_end'] ?? false), (int)($sub['canceled_at'] ?? 0), time() ]);
        }
        query('INSERT INTO subscription_sync(user_id,synced_at) VALUES(?,?) ON DUPLICATE KEY UPDATE synced_at=VALUES(synced_at)', [$userId, time()]);
    });
}

function subscriptionSummary(array $user, bool $refresh = true): ?array {
    if ($refresh) {
        $synced = (int)query('SELECT synced_at FROM subscription_sync WHERE user_id=?', [$user['id']])->fetchColumn();
        $pending = query('SELECT 1 FROM webhook_events e JOIN billing_customers b ON b.customer_id=e.customer_id WHERE b.user_id=? AND e.processed_at IS NULL LIMIT 1', [$user['id']])->fetchColumn();
        if ($synced<time()-60 || $pending) synchronize((int)$user['id']);
    }
    $rows = query('SELECT s.*,p.name AS plan_name,p.description,b.customer_id FROM subscriptions s JOIN plans p ON p.price_id=s.price_id AND p.game_id=? JOIN billing_customers b ON b.user_id=s.user_id WHERE s.user_id=? ORDER BY (s.status=\'active\' AND s.period_end>?) DESC,s.period_end DESC,s.subscription_id', [$user['game_id'], $user['id'], time()])->fetchAll();
    if (!$rows) return null;
    $subscription = $rows[0];
    $status = $subscription['status'];
    if ($status === 'active' && (int)$subscription['period_end'] <= time()) {
        $status = 'expired';
    }
    $formatUtc = fn(int $timestamp): string => $timestamp > 0 ? gmdate('Y-m-d H:i:s', $timestamp).' UTC' : '';
    return [
        'stripe_id' => $subscription['customer_id'],
        'stripe_sub_id' => $subscription['subscription_id'],
        'stripe_pmc_id' => $subscription['payment_method_id'],
        'stripe_product_id' => $subscription['product_id'],
        'stripe_price_id' => $subscription['price_id'],
        'start_date' => $formatUtc((int)$subscription['started_at']),
        'end_date' => $formatUtc((int)$subscription['period_end']),
        'plan' => $subscription['plan_name'],
        'plan_desc' => $subscription['description'],
        'status' => $status,
        'stripe_phone' => $user['phone'],
        'access_until' => $status === 'active' ? (int)$subscription['period_end'] : 0,
        'cancel_at_period_end' => (bool)$subscription['cancel_at_period_end'],
    ];
}

function activeBrowserSession(array $user): void {
    $current = query('SELECT u.active_session_id,s.* FROM users u JOIN sessions s ON s.id=? WHERE u.id=?', [$user['session_id'], $user['id']])->fetch();
    if (!$current || (int)$current['active_session_id'] !== (int)$user['session_id'] || !validSession($current)) throw new ApiError('unauthorized', 'Your game session expired. Return to the game and sign in.', 401);
    if (!$user['verified']) throw new ApiError('verification_required', 'Verify your email before subscribing.', 403);
}

function checkout(array $user, string $planKey): string {
    synchronize((int)$user['id']);
    $attempt = transaction(function () use ($user, $planKey) {
        query('SELECT id FROM users WHERE id=? FOR UPDATE', [$user['id']]);
        activeBrowserSession($user);
        $plan = query('SELECT * FROM plans WHERE game_id=? AND plan_key=? AND active=1', [$user['game_id'], $planKey])->fetch();
        if (!$plan || str_contains($plan['price_id'], 'REPLACE')) throw new ApiError('unknown_plan', 'This plan is not configured.');
        $summary = subscriptionSummary($user, false);
        if (query("SELECT 1 FROM subscriptions WHERE user_id=? AND status IN ('active','trialing','past_due','unpaid','incomplete') LIMIT 1", [$user['id']])->fetchColumn()) throw new ApiError('already_subscribed', 'You have a subscription. Use Manage Subscription instead.', 409);
        $customer = customerFor($user);
        $attempt = query('SELECT * FROM checkout_attempts WHERE user_id=?', [$user['id']])->fetch();
        if ($attempt && $attempt['session_id']) {
            $session = gateway()->retrieveCheckout($attempt['session_id']);
            if (($session['status'] ?? '') === 'open' && $attempt['price_id'] === $plan['price_id']) return $attempt+['customer_id' => $customer];
            if (($session['status'] ?? '') === 'open') gateway()->expireCheckout($attempt['session_id']);
            if (($session['status'] ?? '') === 'complete' && (!$summary || !in_array($summary['status'], ['canceled', 'incomplete_expired', 'expired'], true))) throw new ApiError('payment_pending', 'Your previous checkout is processing. Return to the game and refresh access.', 409);
            $attempt = null;
        }
        // Keep the same durable intent after an interrupted provider call.
        if ($attempt && $attempt['session_id'] === null && $attempt['price_id'] !== $plan['price_id']) throw new ApiError('checkout_pending', 'Finish or retry the previous checkout first.', 409);
        if (!$attempt) {
            query('INSERT INTO checkout_attempts(user_id,attempt_key,price_id,return_url,expires_at) VALUES(?,?,?,?,?) ON DUPLICATE KEY UPDATE attempt_key=VALUES(attempt_key),session_id=NULL,price_id=VALUES(price_id),return_url=VALUES(return_url),expires_at=VALUES(expires_at)', [$user['id'], bin2hex(random_bytes(16)), $plan['price_id'], publicUrl(), time()+3600]);
            $attempt = query('SELECT * FROM checkout_attempts WHERE user_id=?', [$user['id']])->fetch();
        }
        return $attempt+['customer_id' => $customer];
    });
    // The intent is committed before this side effect. A network failure does not
    // lose the idempotency key; retrying cannot create a second checkout.
    if ($attempt['session_id']) return safePaymentUrl(gateway()->retrieveCheckout($attempt['session_id'])['url']);
    $user['customer_id'] = $attempt['customer_id'];
    $session = gateway()->checkout($user, $attempt);
    query('UPDATE checkout_attempts SET session_id=? WHERE user_id=? AND attempt_key=?', [$session['id'], $user['id'], $attempt['attempt_key']]);
    return safePaymentUrl($session['url']);
}

function safePaymentUrl(string $url): string {
    $parts = parse_url($url);
    if (($parts['scheme'] ?? '') !== 'https' || !in_array($parts['host'] ?? '', ['checkout.stripe.com', 'billing.stripe.com'], true) || isset($parts['user']) || isset($parts['pass']) || ($parts['port'] ?? 443) !== 443) throw new ApiError('provider_url', 'The payment service returned an unexpected URL.', 503);
    return $url;
}

function receiveWebhook(): void {
    postOnly();
    $body = file_get_contents('php://input', false, null, 0, 1048577);
    if (strlen($body)>1048576) throw new ApiError('request_too_large', 'Webhook too large.', 413);
    $event = gateway()->webhook($body, $_SERVER['HTTP_STRIPE_SIGNATURE'] ?? '');
    $object = $event['data']['object'] ?? [];
    $customer = $object['customer'] ?? null;
    if (is_array($customer)) $customer = $customer['id'] ?? null;
    $types = ['checkout.session.completed', 'checkout.session.async_payment_succeeded', 'checkout.session.async_payment_failed', 'customer.subscription.created', 'customer.subscription.updated', 'customer.subscription.deleted', 'invoice.paid', 'invoice.payment_failed'];
    query('INSERT IGNORE INTO webhook_events(event_id,customer_id,event_type,received_at,processed_at,available_at) VALUES(?,?,?,?,?,?)', [$event['id'], $customer, $event['type'], time(), in_array($event['type'], $types, true)?null:time(), time()]);
    json(['received' => true]);
}

function processWebhooks(): array {
    $done = 0;
    $failed = 0;
    $events = query('SELECT * FROM webhook_events WHERE processed_at IS NULL AND available_at<=? AND attempts<10 ORDER BY received_at,event_id LIMIT 100', [time()])->fetchAll();
    foreach ($events as $event) {
        if (!query('UPDATE webhook_events SET available_at=? WHERE event_id=? AND processed_at IS NULL AND available_at<=?', [time()+120, $event['event_id'], time()])->rowCount()) continue;
        try {
            $id = $event['customer_id']?query('SELECT user_id FROM billing_customers WHERE customer_id=?', [$event['customer_id']])->fetchColumn():false;
            if ($id) synchronize((int)$id);
            query('UPDATE webhook_events SET processed_at=?,last_error=NULL WHERE event_id=?', [time(), $event['event_id']]);
            $done++;
        }
        catch (\Throwable $error) {
            query('UPDATE webhook_events SET attempts=attempts+1,available_at=?,last_error=? WHERE event_id=?', [time()+300, 'reconciliation_failed', $event['event_id']]);
            $failed++;
        }
    }
    return ['processed' => $done, 'failed' => $failed];
}
