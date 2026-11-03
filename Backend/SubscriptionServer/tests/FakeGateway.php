<?php
declare(strict_types=1);
namespace SubscriptionServer;
// Test-only provider; bootstrap selects this only in localhost APP_ENV=test.
final class FakeGateway implements PaymentGateway {
    public function __construct(private string $file) {}
    private function state(callable $action): mixed {
        $file=fopen($this->file,'c+'); flock($file,LOCK_EX);
        try { $raw=stream_get_contents($file); $state=$raw!==''?json_decode($raw,true,512,JSON_THROW_ON_ERROR):[]; $result=$action($state); rewind($file);ftruncate($file,0);fwrite($file,json_encode($state,JSON_THROW_ON_ERROR));return $result; }
        finally { flock($file,LOCK_UN);fclose($file); }
    }
    public function customer(array $user): string { return 'cus_test_'.$user['id']; }
    public function checkout(array $user,array $attempt): array {
        $result=$this->state(function(&$state)use($user,$attempt){
            $key=$attempt['attempt_key'];
            if(isset($state['intents'][$key])) return ['session'=>$state['checkouts'][$state['intents'][$key]],'fail'=>false];
            $id='cs_test_'.(count($state['checkouts']??[])+1); $session=['id'=>$id,'status'=>'open','url'=>'https://checkout.stripe.com/test/'.$id,'customer'=>$user['customer_id']];
            $state['intents'][$key]=$id; $state['checkouts'][$id]=$session;
            $fail=($state['fail_after_create']??false); $state['fail_after_create']=false;
            return ['session'=>$session,'fail'=>$fail];
        });
        if($result['fail']) throw new \RuntimeException('Simulated interrupted provider response.');
        return $result['session'];
    }
    public function retrieveCheckout(string $id): array { return $this->state(fn(&$s)=>$s['checkouts'][$id]); }
    public function expireCheckout(string $id): void { $this->state(function(&$s)use($id){$s['checkouts'][$id]['status']='expired';return null;}); }
    public function portal(string $customer,string $configuration=''): string { $this->state(function(&$s)use($customer,$configuration){$s['portal_customer']=$customer;$s['portal_configuration']=$configuration;return null;});return 'https://billing.stripe.com/test/portal'; }
    public function subscriptions(string $customer): array {
        $state=json_decode(file_get_contents($this->file),true,512,JSON_THROW_ON_ERROR);
        if($state['fail_sync']??false) throw new \RuntimeException('Simulated provider outage.');
        return $state['subscriptions'][$customer]??[];
    }
    public function webhook(string $body,string $signature): array { return (new StripeGateway())->webhook($body,$signature); }
}
