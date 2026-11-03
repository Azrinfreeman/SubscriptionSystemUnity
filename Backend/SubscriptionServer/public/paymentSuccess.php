<?php
declare(strict_types=1);
require __DIR__.'/../src/bootstrap.php';
\SubscriptionServer\handle(static function(): void {
    \SubscriptionServer\page("Return to your game","<p>Your billing changes may still be processing. Return to the game and refresh subscription details. This page does not activate access.</p>");
});
