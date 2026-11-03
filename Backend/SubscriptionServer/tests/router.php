<?php
declare(strict_types=1);
// Local PHP test server only; supports a hosting-subfolder fixture.
$path=parse_url($_SERVER['REQUEST_URI'],PHP_URL_PATH);
$prefix=getenv('TEST_PUBLIC_PREFIX')?:'';
if($prefix!==''&&!str_starts_with($path,$prefix.'/')){http_response_code(404);exit;}
$relative=substr($path,strlen($prefix)+1);
if(!preg_match('~^(?:[A-Za-z]+\.php|assets/style\.css)$~',$relative)){http_response_code(404);exit;}
$file=dirname(__DIR__).'/public/'.$relative;
if(!is_file($file)){http_response_code(404);exit;}
if(str_ends_with($file,'.css')){header('Content-Type: text/css');readfile($file);exit;}
require $file;
