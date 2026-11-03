<?php
declare(strict_types=1);
namespace SubscriptionServer;

const ROOT = __DIR__.'/..';
if (!is_file(ROOT.'/vendor/autoload.php')) {
    if (PHP_SAPI === 'cli') throw new \RuntimeException('Run composer install in Backend/SubscriptionServer.');
    http_response_code(503); header('Content-Type: text/plain'); exit('Server setup required.');
}
require ROOT.'/vendor/autoload.php';
\Dotenv\Dotenv::createImmutable(ROOT)->safeLoad();
require __DIR__.'/Core.php';
require __DIR__.'/Accounts.php';
require __DIR__.'/Billing.php';
require __DIR__.'/Gateway.php';
require __DIR__.'/Web.php';
date_default_timezone_set('UTC');
ini_set('display_errors','0');
error_reporting(E_ALL);

function handle(callable $action): void {
    header('Cache-Control: no-store'); header('Referrer-Policy: no-referrer'); header('X-Content-Type-Options: nosniff');
    header("Content-Security-Policy: default-src 'self'; script-src 'none'; style-src 'self'; form-action 'self'; frame-ancestors 'none'");
    try {
        if ((int)($_SERVER['CONTENT_LENGTH']??0)>1048576) throw new ApiError('request_too_large','Request too large.',413);
        if (env('APP_ENV') === 'test' && !in_array($_SERVER['REMOTE_ADDR'] ?? 'cli', ['127.0.0.1','::1','cli'], true)) throw new ApiError('test_environment', 'Test mode is restricted to localhost.',503);
        $action();
    } catch (ApiError $error) {
        try { if (database()->inTransaction()) database()->rollBack(); } catch (\Throwable) {}
        if (str_contains($_SERVER['HTTP_ACCEPT'] ?? '', 'text/html')) page('Unable to continue', '<p>'.escape($error->getMessage()).'</p><p>Return to the game and retry.</p>', $error->status);
        else json(['code'=>$error->errorCode,'message'=>$error->getMessage()],$error->status);
    } catch (\Throwable $error) {
        try { if (database()->inTransaction()) database()->rollBack(); } catch (\Throwable) {}
        // Do not log raw provider messages, credentials, request bodies or URLs.
        error_log('SubscriptionServer failure: '.get_class($error));
        json(['code'=>'server_error','message'=>'The service is temporarily unavailable. Please retry.'],503);
    }
}
