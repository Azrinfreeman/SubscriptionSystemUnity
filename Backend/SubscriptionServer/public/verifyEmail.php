<?php
declare(strict_types=1);
require __DIR__.'/../src/bootstrap.php';
\SubscriptionServer\handle(static function(): void {
    if($_SERVER["REQUEST_METHOD"]!=="GET") throw new \SubscriptionServer\ApiError("method_not_allowed","Use GET.",405); \SubscriptionServer\verifyEmail();
});
