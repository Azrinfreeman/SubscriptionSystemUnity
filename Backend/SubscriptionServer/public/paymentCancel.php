<?php
declare(strict_types=1);
require __DIR__.'/../src/bootstrap.php';
\SubscriptionServer\handle(static function(): void {
    \SubscriptionServer\page("Checkout canceled","<p>No access is granted by this page. Return to your game, or request a new billing link to try again.</p>");
});
