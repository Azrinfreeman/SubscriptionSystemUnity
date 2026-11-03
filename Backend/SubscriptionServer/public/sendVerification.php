<?php
declare(strict_types=1);
require __DIR__.'/../src/bootstrap.php';
\SubscriptionServer\handle(static function(): void {
    \SubscriptionServer\postOnly(); $user=\SubscriptionServer\authenticated(); \SubscriptionServer\rateLimit("verify-mail-".$user["id"],3,600); \SubscriptionServer\queueVerification($user); \SubscriptionServer\plain("success");
});
