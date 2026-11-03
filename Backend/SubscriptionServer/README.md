# Subscription server

Rebuilt PHP backend for this project's existing LogMasuk and LamanUtama scenes. The original PHP source was unavailable; this is a new compatible implementation with authenticated sessions and server-verified billing. It does not recover the expired server's database.

Start with [the current setup path](../../Docs/SETUP.md), then use this reference for server commands and administration. The PDF under `output/pdf/` and its editable source in `Docs/SetupGuide/` remain historical illustrated references; their screenshots predate the menu redesign.

## Install

1. Prepare HTTPS hosting, a private MySQL/MariaDB database, SMTP and a Stripe test account. Use a currently supported PHP release compatible with PHP 8.2+ syntax, with PDO MySQL, cURL, OpenSSL, mbstring and Argon2id support. Composer installation may also require ZIP or an archive extractor. Use InnoDB and a UTC server clock.
2. Copy this entire folder to private server storage. Map **only `public/`** to your HTTPS URL, e.g. `https://example.com/subscriptions/`. Never expose `.env`, `config/`, `vendor/`, `storage/`, `tests/` or `bin/`. On shared hosting, place private files above public_html and map/copy only public entrypoints with their bootstrap paths adjusted to the private folder. Verify these private paths return 403/404 over HTTP. Apache needs Authorization forwarding (provided in public/.htaccess), with AllowOverride enabled; on Nginx pass `HTTP_AUTHORIZATION` to PHP-FPM and map PHP script paths to this public folder.
3. Run `composer install --no-dev --optimize-autoloader` from this folder. Keep composer.lock. Copy `.env.example` to `.env` and `config/games.example.php` to `config/games.php`. Protect private files and give the PHP/worker user write access only to private `storage/` as needed.
4. Set PUBLIC_URL to that exact HTTPS folder (no PHP filename). Set database credentials. Generate a unique installation ID using `php bin/generateInstanceId.php` and set BILLING_NAMESPACE once; preserve it with backups and never reuse it for another database.
5. Put your exact game key in config/games.php; it must match Unity's Game Identifier (`Your Game` initially). Set real recurring test-mode Stripe price IDs, plan titles, descriptions and display prices. Display text must match the prices in Stripe. Keep retired price mappings by setting active=false. A changed price needs a new plan key; seed refuses silently replacing an existing price mapping.
6. Run `php bin/migrate.php`, then `php bin/seed.php`. Migrations are repeatable. Use a migration-capable database account for installation; the runtime account should have only necessary application-table privileges. Re-run seed after catalog edits. GAME_CATALOG can point to an absolute private catalog file.
7. Configure mail and Stripe below, run `php bin/checkConfig.php`, and visit `/health.php` for `{"status":"ok","api_version":1}`. Health is a process check; it does not certify external services.

## Email

Development uses MAIL_TRANSPORT=log: run `php bin/processMail.php` and read the private `storage/mail.log` verification link. Treat that log as a credential: never publish it. Production requires MAIL_TRANSPORT=smtp, real SMTP_HOST/PORT/USERNAME/PASSWORD, SMTP_ENCRYPTION=tls (587) or ssl (465), MAIL_FROM and MAIL_FROM_NAME. Run the worker and verify receipt and the single-use link in a real mailbox. Verification expires after one hour. Failed delivery is retried through the outbox; watch attempts and worker logs. The worker erases the queued message body after delivery.

## Stripe

Use test credentials first: STRIPE_SECRET_KEY and STRIPE_WEBHOOK_SECRET. Set your recurring price IDs in the catalog. Enable the Customer Portal and configure permitted products/prices and cancellation behavior. Set STRIPE_PORTAL_CONFIGURATION for one game, or a distinct portal_configuration per game in the catalog when hosting multiple games. Each game's portal must expose only that game's prices.

Create a webhook for `PUBLIC_URL/stripeWebhook.php`, including checkout.session.completed, checkout.session.async_payment_succeeded, checkout.session.async_payment_failed, customer.subscription.created/updated/deleted, invoice.paid and invoice.payment_failed. Copy that endpoint's signing secret, using the matching test/live environment. The receiver verifies the raw body with the Stripe SDK and durably records accepted events; workers reconcile current Stripe subscriptions, so delayed or duplicate events do not blindly overwrite current state. The locked SDK uses API version 2025-03-31.basil.

Only current mapped `active` subscriptions with a future paid-through time grant access. Trials and overdue/unknown statuses do not. Scheduled cancellation keeps access until the paid-through timestamp. Redirects never grant access. Existing subscriptions in active/trialing/past_due/unpaid/incomplete states block a second checkout. An interrupted Checkout request reuses its persisted intent/idempotency key. If an ambiguous pending intent needs manual recovery, reconcile the customer and Checkout records with Stripe before clearing/replacing it; never blindly create another subscription.

## Workers and operations

Run these as the same private application user, using absolute paths to PHP and scripts. Capture errors in private logs and monitor failures. Suggested cron schedule:

```cron
* * * * * /usr/bin/php /srv/subscription/bin/processMail.php
* * * * * /usr/bin/php /srv/subscription/bin/processWebhooks.php
* * * * * /usr/bin/php /srv/subscription/bin/reconcileSubscriptions.php
15 3 * * * /usr/bin/php /srv/subscription/bin/cleanup.php
```

Windows hosts can run the same commands with Task Scheduler. Prevent overlapping maintenance runs using your scheduler or a process lock. Webhook processing uses event leases and retries. After fixing persistent webhook failures, run `php bin/processWebhooks.php --retry-failed`. Reconciliation batches linked customers; watch lag, mail attempts and pending/failed webhook rows. Cleanup prunes expired operational records; it is not an account-deletion policy. Set your own backup, privacy/retention and restore procedures. Back up the database, .env, catalog and stable namespace. Never ship logs, secrets, tokens or real customer data in the template.

## Unity connection

Keep the original two scenes. Tools > Subscription System > Select Settings selects the shared asset. Set Backend Base URL and the exact catalog game key. Keep the endpoint defaults, including createBillingLink.php. No database, SMTP or Stripe secret belongs in Unity. The client stores bearer credentials in memory and requires login again after restart. Existing local login flags cannot authenticate requests.

Registration currently validates Malaysian mobile numbers and 12-128 byte passwords. Customize validation and labels together if adapting to other countries. There is no password-reset, native store billing or historical-user import feature in this delivery.

## API reference

API operations use POST WWWForm and `_game_name`. Public registration/login use their original fields. Protected calls require `Authorization: Bearer <session_token>`; posted IDs/emails must also match the authenticated account. Authentication is scoped to the game. Tokens are returned only when issued and stored hashed on the server.

| Route | Fields / behavior |
| --- | --- |
| insertUser.php | `_name`, `_email`, `_notel`, `_password`, `_game_name`; Successful prefix or original duplicate literals |
| loginVerify.php | `_email`, `_password`, `_device_name`, `_game_name`; profile plus session_token and Unix session_expires_at on success (login_logs=0) |
| getinfouser.php | `id_user`, `_game_name`; authenticated profile, login_logs=1 for the active session |
| getinfo.php | `_email`, `_game_name`; authenticated subscription JSON or literal `no result` for confirmed absence |
| sendVerification.php | `id_user`, `_email`, `_game_name`; authenticated request, queues email |
| logout.php | bearer token, or password-authorized restricted `session_action_token` to revoke the conflicting session |
| createBillingLink.php | bearer, `_game_name`, `action` choose/change/manage; short-lived URL and expires_at |
| pilihplan.php / tukarplan.php / manageSub.php | exchange a single-use ticket for an action-restricted browser session |
| createCheckout.php / createPortal.php | browser POST only, CSRF protected; customer and prices resolved server-side |
| verifyEmail.php | single-use email token |
| stripeWebhook.php | signed raw Stripe event; no browser/session authentication |
| paymentSuccess.php / paymentCancel.php | informational return pages; no entitlement changes |
| health.php | process status; no secrets |

A password-confirmed conflict response has login_logs=1 and a session_action_token valid for five minutes, which can only revoke that exact conflicting session. Log in again afterward for a full credential. Ordinary sessions have a configurable idle timeout (default 30 minutes) and absolute lifetime (24 hours). Billing tickets last 90 seconds; browser billing sessions last 15 minutes and remain bound to the still-active API session. URLs no longer accept email/customer identity as authorization.

Subscription fields preserve stripe_id, stripe_sub_id, stripe_pmc_id, stripe_product_id, stripe_price_id, stripe_phone, start_date, end_date, plan, plan_desc and status, and add `cancel_at_period_end` and `access_until` (Unix seconds). end_date explicitly means UTC paid-through time. Only active/future access_until permits premium use. Provider outages produce an error, not a false `no result`. JSON errors contain error/message; protected 401 expires the current client session. Never log credentials or handoff/verification links.

## Verification and release

Local validation: 86 backend assertions using an isolated disposable MariaDB database, fake payment provider and actual SDK signature verification; PHP syntax checks; Composer audit with no advisories at validation time. Tests cover ownership, bearer expiry, replay, billing handoff/CSRF, duplicate/interrupted checkout, stale events, cancellation, provider outages and simultaneous login locking. They send no real mail or payments.

Run `tests/run.php` only with APP_ENV=test, localhost DB_HOST and test DB credentials permitted to CREATE/DROP a disposable database. It creates a random test database and removes that database on completion; it never uses your production schema. Tests require dev dependencies/runtime files and local PHP process spawning, and must stay outside the public directory.

Set these process environment variables in your local test shell before running the suite. The test harness overrides its own public URL, catalog, mail log and fake-provider settings; do not point it at deployed services.

| Variable | Test value |
| --- | --- |
| APP_ENV | `test` |
| DB_HOST | `127.0.0.1` or `localhost` |
| DB_PORT | Port of your disposable local MySQL/MariaDB instance |
| DB_USERNAME | Local test account allowed to create/drop disposable databases |
| DB_PASSWORD | That test account's password, provided privately in the process environment |

From `Backend/SubscriptionServer`, run:

```text
php tests/run.php
```

`composer test` invokes the same entry point. Keep the test password out of committed scripts and command history. Passing this suite verifies the fake-provider integration and local database behavior; it does not verify a real Stripe account or mailbox.

Before release verify public HTTPS mapping, Authorization forwarding, SMTP receipt, a real Stripe test purchase/cancel/change flow, webhook retries, worker scheduling, backups and target-device WebViews. For live mode use a separate production database and live prices/keys/webhook, APP_ENV=production, SMTP and ALLOW_LIVE_PAYMENTS=true. Review target-store billing requirements separately. No deployment, live charge or old-customer migration has been performed.

## Find the implementation

| Source | Responsibility |
| --- | --- |
| `src/Accounts.php` | Registration policy, login/session conflicts, verification and mail delivery |
| `src/Core.php` | Database/transaction helpers, request fields, authentication and profile mapping |
| `src/Billing.php` | Provider synchronization, subscription summaries, durable checkout intents and webhook processing |
| `src/Web.php` | Browser billing sessions, ticket exchange, forms and response pages |
| `src/Gateway.php` | Provider integration boundary |
| `config/games.example.php` | Editable catalog example; copy to the private `games.php` for installation |
| `bin/` | Installation checks, migrations, seeding and maintenance commands |
| `tests/run.php`, `tests/FakeGateway.php` | Isolated HTTP/database regressions and fake payment provider |

For a coordinated client/server validation edit, follow [the account customization tutorial](../../Docs/tutorial/account-flow-customization.md). Preserve transaction and locking boundaries when refactoring functions; moving provider calls across them can change concurrency behavior even if the returned JSON looks unchanged.
