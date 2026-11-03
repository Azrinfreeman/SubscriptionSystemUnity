<?php
declare(strict_types=1);
require __DIR__.'/common.php';
runCommand(function(){
    $ok=0;$failed=0;
    $users=\SubscriptionServer\query('SELECT b.user_id FROM billing_customers b LEFT JOIN subscription_sync s ON s.user_id=b.user_id ORDER BY COALESCE(s.synced_at,0),b.user_id LIMIT 100')->fetchAll();
    foreach($users as $user) try{\SubscriptionServer\synchronize((int)$user['user_id']);$ok++;}catch(Throwable){$failed++;}
    return ['reconciled'=>$ok,'failed'=>$failed];
});
