<?php
declare(strict_types=1);
require __DIR__.'/../src/bootstrap.php';
\SubscriptionServer\handle(static function(): void {
    \SubscriptionServer\postOnly(); $user=\SubscriptionServer\authenticated(); $data=\SubscriptionServer\subscriptionSummary($user); if($data===null) \SubscriptionServer\plain("no result"); \SubscriptionServer\json($data);
});
