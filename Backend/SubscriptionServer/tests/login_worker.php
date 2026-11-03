<?php
declare(strict_types=1);
$_SERVER['REQUEST_METHOD']='POST';$_SERVER['REMOTE_ADDR']='127.0.0.1';
$_POST=['_email'=>'alice@example.invalid','_password'=>'test-password-123','_device_name'=>'Concurrent test','_game_name'=>'Test Game'];
require dirname(__DIR__).'/public/loginVerify.php';
