<?php
declare(strict_types=1);
require __DIR__.'/../src/bootstrap.php';
\SubscriptionServer\handle(static function(): void {
    \SubscriptionServer\query("SELECT 1 FROM games LIMIT 1"); \SubscriptionServer\json(["api_version"=>1,"status"=>"ok"]);
});
