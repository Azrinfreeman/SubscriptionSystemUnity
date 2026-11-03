<?php
declare(strict_types=1);
require __DIR__.'/common.php';
runCommand(function(){
    \SubscriptionServer\publicUrl(); \SubscriptionServer\query('SELECT 1 FROM games LIMIT 1');
    $issues=[];
    if(\SubscriptionServer\env('BILLING_NAMESPACE')===''||str_contains(\SubscriptionServer\env('BILLING_NAMESPACE'),'replace-'))$issues[]='Set a unique BILLING_NAMESPACE for this installation.';
    if(!defined('PASSWORD_ARGON2ID'))$issues[]='PHP Argon2id support is required.';
    if(\SubscriptionServer\env('MAIL_TRANSPORT')==='smtp'&&(\SubscriptionServer\env('SMTP_HOST')===''||\SubscriptionServer\env('SMTP_USERNAME')===''))$issues[]='Configure SMTP delivery.';
    if(\SubscriptionServer\env('APP_ENV')==='production'&&\SubscriptionServer\env('MAIL_TRANSPORT')!=='smtp')$issues[]='Production requires SMTP.';
    if(\SubscriptionServer\env('STRIPE_SECRET_KEY')===''||\SubscriptionServer\env('STRIPE_WEBHOOK_SECRET')==='')$issues[]='Configure Stripe keys and webhook secret.';
    if(\SubscriptionServer\query("SELECT 1 FROM plans WHERE active=1 AND price_id LIKE '%REPLACE%' LIMIT 1")->fetchColumn())$issues[]='Replace example Stripe price IDs.';
    if((int)\SubscriptionServer\query('SELECT COUNT(*) FROM games WHERE active=1')->fetchColumn()>1&&\SubscriptionServer\query("SELECT 1 FROM games WHERE active=1 AND portal_configuration='' LIMIT 1")->fetchColumn())$issues[]='Set a portal configuration for each game.';
    if($issues){foreach($issues as $issue)fwrite(STDERR,$issue.PHP_EOL);exit(1);}
    return 'Configuration checks passed. No secrets printed. Now test SMTP, webhook delivery and a test-mode purchase.';
});
