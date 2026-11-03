<?php
declare(strict_types=1);
if (PHP_SAPI!=='cli') { http_response_code(403); exit; }
require __DIR__.'/../src/bootstrap.php';
function runCommand(callable $action): void {
    try { $result=$action(); echo is_string($result)?$result.PHP_EOL:json_encode($result,JSON_THROW_ON_ERROR|JSON_PRETTY_PRINT).PHP_EOL; }
    catch(Throwable $error) { fwrite(STDERR,'Command failed ('.get_class($error).'). Check configuration and private server logs.'.PHP_EOL); error_log('CLI failure: '.get_class($error)); exit(1); }
}
