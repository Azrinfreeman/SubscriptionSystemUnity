<?php
declare(strict_types=1);
namespace SubscriptionServer;

interface PaymentGateway {
    public function customer(array $user): string;
    public function checkout(array $user,array $attempt): array;
    public function retrieveCheckout(string $id): array;
    public function expireCheckout(string $id): void;
    public function portal(string $customer,string $configuration=''): string;
    public function subscriptions(string $customer): array;
    public function webhook(string $body,string $signature): array;
}
final class StripeGateway implements PaymentGateway {
    private \Stripe\StripeClient $stripe;
    public function __construct() {
        $key=env('STRIPE_SECRET_KEY');
        if ($key===''||(str_starts_with($key,'sk_live_')&&(env('APP_ENV')!=='production'||env('ALLOW_LIVE_PAYMENTS')!=='true'))) throw new ApiError('billing_unavailable','Billing is not configured for this environment.',503);
        if (env('BILLING_NAMESPACE')===''||str_contains(env('BILLING_NAMESPACE'),'replace-')) throw new ApiError('setup_required','Configure a unique BILLING_NAMESPACE for this installation.',503);
        $http=new \Stripe\HttpClient\CurlClient(); $http->setTimeout(15); $http->setConnectTimeout(5); \Stripe\ApiRequestor::setHttpClient($http);
        $this->stripe=new \Stripe\StripeClient(['api_key'=>$key,'stripe_version'=>'2025-03-31.basil']);
    }
    public function customer(array $user): string {
        return $this->stripe->customers->create(['email'=>$user['email'],'name'=>$user['name'],'phone'=>$user['phone'],'metadata'=>['user_id'=>(string)$user['id'],'game_id'=>(string)$user['game_id'],'installation'=>env('BILLING_NAMESPACE')]],['idempotency_key'=>env('BILLING_NAMESPACE').'-customer-'.$user['id']])->id;
    }
    public function checkout(array $user,array $attempt): array {
        $session=$this->stripe->checkout->sessions->create(['mode'=>'subscription','customer'=>$user['customer_id'],'line_items'=>[['price'=>$attempt['price_id'],'quantity'=>1]],'expires_at'=>(int)$attempt['expires_at'],'success_url'=>$attempt['return_url'].'/paymentSuccess.php','cancel_url'=>$attempt['return_url'].'/paymentCancel.php','metadata'=>['game_id'=>(string)$user['game_id']],'subscription_data'=>['metadata'=>['user_id'=>(string)$user['id'],'game_id'=>(string)$user['game_id']]]],['idempotency_key'=>$attempt['attempt_key']]);
        return ['id'=>$session->id,'status'=>$session->status,'url'=>$session->url];
    }
    public function retrieveCheckout(string $id): array { return $this->stripe->checkout->sessions->retrieve($id)->toArray(); }
    public function expireCheckout(string $id): void { $this->stripe->checkout->sessions->expire($id); }
    public function portal(string $customer,string $configuration=''): string {
        $data=['customer'=>$customer,'return_url'=>publicUrl('paymentSuccess.php')];
        $configuration=$configuration!==''?$configuration:env('STRIPE_PORTAL_CONFIGURATION');
        if ($configuration!=='') $data['configuration']=$configuration;
        return $this->stripe->billingPortal->sessions->create($data)->url;
    }
    public function subscriptions(string $customer): array {
        $list=[];
        foreach($this->stripe->subscriptions->all(['customer'=>$customer,'status'=>'all','limit'=>100])->autoPagingIterator() as $sub) $list[]=$sub->toArray();
        return $list;
    }
    public function webhook(string $body,string $signature): array {
        if (env('STRIPE_WEBHOOK_SECRET')==='') throw new ApiError('setup_required','Webhook is not configured.',503);
        try { $event=\Stripe\Webhook::constructEvent($body,$signature,env('STRIPE_WEBHOOK_SECRET'),300)->toArray(); }
        catch(\UnexpectedValueException|\Stripe\Exception\SignatureVerificationException) { throw new ApiError('invalid_signature','Invalid webhook signature.'); }
        if ((bool)$event['livemode']!==str_starts_with(env('STRIPE_SECRET_KEY'),'sk_live_')) throw new ApiError('wrong_environment','Webhook environment mismatch.');
        return $event;
    }
}
function gateway(): PaymentGateway {
    static $gateway;
    if ($gateway) return $gateway;
    if (env('APP_ENV')==='test'&&env('TEST_PROVIDER_FILE')!=='') { require_once ROOT.'/tests/FakeGateway.php'; return $gateway=new FakeGateway(env('TEST_PROVIDER_FILE')); }
    return $gateway=new StripeGateway();
}
