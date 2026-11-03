<?php
declare(strict_types=1);
require __DIR__.'/common.php';
runCommand(function(){
    if(in_array('--retry-failed',$GLOBALS['argv'],true)) \SubscriptionServer\query('UPDATE webhook_events SET attempts=0,available_at=? WHERE processed_at IS NULL AND attempts>=10',[time()]);
    return \SubscriptionServer\processWebhooks();
});
