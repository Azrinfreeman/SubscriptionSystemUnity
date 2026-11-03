<?php
declare(strict_types=1);
require __DIR__.'/common.php';
runCommand(function(){
    $configured=\SubscriptionServer\env('GAME_CATALOG','config/games.php');
    $file=preg_match('~^(?:[A-Za-z]:[\\\\/]|/)~',$configured)?$configured:\SubscriptionServer\ROOT.'/'.$configured;
    if(!is_file($file)) throw new RuntimeException('Copy games.example.php to games.php first.');
    $catalog=require $file;
    \SubscriptionServer\transaction(function()use($catalog){
        foreach($catalog as $key=>$game) {
            if(!is_string($key)||$key===''||strlen($key)>100||empty($game['name'])) throw new RuntimeException('Invalid game catalog.');
            \SubscriptionServer\query('INSERT INTO games(game_key,name,portal_configuration) VALUES(?,?,?) ON DUPLICATE KEY UPDATE name=VALUES(name),portal_configuration=VALUES(portal_configuration)',[$key,$game['name'],$game['portal_configuration']??'']);
            $id=\SubscriptionServer\game($key)['id'];
            \SubscriptionServer\query('UPDATE plans SET active=0 WHERE game_id=?',[$id]);
            foreach($game['plans']??[] as $planKey=>$plan) {
                $old=\SubscriptionServer\query('SELECT price_id FROM plans WHERE game_id=? AND plan_key=?',[$id,$planKey])->fetchColumn();
                if($old&&$old!==$plan['price_id']) throw new RuntimeException('Use a new plan key for a changed price; preserve old subscription mappings.');
                \SubscriptionServer\query('INSERT INTO plans(game_id,plan_key,name,description,price_id,display_price,active) VALUES(?,?,?,?,?,?,?) ON DUPLICATE KEY UPDATE name=VALUES(name),description=VALUES(description),display_price=VALUES(display_price),active=VALUES(active)',[$id,$planKey,$plan['name'],$plan['description'],$plan['price_id'],$plan['display_price'],(int)($plan['active']??true)]);
            }
        }
    });
    return 'Game catalog seeded; retired price mappings retained.';
});
