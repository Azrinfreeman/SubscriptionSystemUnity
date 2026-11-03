<?php
declare(strict_types=1);
require __DIR__.'/common.php';
runCommand(function(){
    \SubscriptionServer\query('CREATE TABLE IF NOT EXISTS schema_migrations(version VARCHAR(100) PRIMARY KEY,applied_at BIGINT NOT NULL) ENGINE=InnoDB');
    foreach(glob(__DIR__.'/../database/migrations/*.sql') as $file) {
        $version=basename($file);
        if(\SubscriptionServer\query('SELECT version FROM schema_migrations WHERE version=?',[$version])->fetchColumn()) continue;
        foreach(explode(';',file_get_contents($file)) as $sql) if(trim($sql)!=='') \SubscriptionServer\database()->exec($sql);
        \SubscriptionServer\query('INSERT INTO schema_migrations VALUES(?,?)',[$version,time()]);
    }
    return 'Database migrations complete.';
});
