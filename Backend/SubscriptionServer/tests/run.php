<?php
declare(strict_types=1);
// Requires a disposable localhost MySQL/MariaDB account with CREATE DATABASE.
if(getenv('APP_ENV')!=='test'||!in_array(getenv('DB_HOST'),['127.0.0.1','localhost'],true)) { fwrite(STDERR,"Set APP_ENV=test and a localhost disposable database connection.\n");exit(1); }
$root=dirname(__DIR__);$name='subscription_test_'.bin2hex(random_bytes(5));$checks=0;$server=null;$admin=null;
$artifacts=sys_get_temp_dir().'/'.$name;mkdir($artifacts,0700,true);
function check(bool $condition,string $message): void {global $checks;if(!$condition)throw new RuntimeException('FAILED: '.$message);$checks++;echo "PASS $message\n";}
function command(string $file,array $args=[]): string {
    global $root;
    $proc=proc_open(array_merge([PHP_BINARY,$root.'/'.$file],$args),[0=>['pipe','r'],1=>['pipe','w'],2=>['pipe','w']],$pipes,$root,null,['bypass_shell'=>true]);
    fclose($pipes[0]);$out=stream_get_contents($pipes[1]);$error=stream_get_contents($pipes[2]);fclose($pipes[1]);fclose($pipes[2]);
    if(proc_close($proc)!==0)throw new RuntimeException('CLI failed: '.$file.' '.$error);return $out;
}
function request(string $path,array|string|null $body=null,string $credential='',int $expected=200,?string $cookies=null,array $extra=[]): array {
    global $base;
    $headers=['Accept: application/json'];if($credential!=='')$headers[]='Authorization: Bearer '.$credential;
    $headers=array_merge($headers,$extra);$responseHeaders=[];
    $curl=curl_init(str_starts_with($path,'http')?$path:$base.'/'.$path);
    $options=[CURLOPT_RETURNTRANSFER=>true,CURLOPT_FOLLOWLOCATION=>false,CURLOPT_HTTPHEADER=>$headers,CURLOPT_TIMEOUT=>20,CURLOPT_HEADERFUNCTION=>function($c,$line)use(&$responseHeaders){if(str_contains($line,':')){[$k,$v]=explode(':',$line,2);$responseHeaders[strtolower(trim($k))]=trim($v);}return strlen($line);}];
    if($body!==null){$options[CURLOPT_POST]=true;$options[CURLOPT_POSTFIELDS]=$body;}
    if($cookies!==null){$options[CURLOPT_COOKIEFILE]=$cookies;$options[CURLOPT_COOKIEJAR]=$cookies;}
    curl_setopt_array($curl,$options);$content=curl_exec($curl);$status=(int)curl_getinfo($curl,CURLINFO_RESPONSE_CODE);$error=curl_error($curl);curl_close($curl);
    if($content===false)throw new RuntimeException('HTTP test connection failed: '.$error);
    check($status===$expected,basename(parse_url($path,PHP_URL_PATH)).' HTTP '.$expected);
    return ['body'=>$content,'json'=>json_decode($content,true),'headers'=>$responseHeaders];
}
function fixture(callable $change): void { $file=getenv('TEST_PROVIDER_FILE');$state=json_decode(file_get_contents($file),true,512,JSON_THROW_ON_ERROR);$change($state);file_put_contents($file,json_encode($state,JSON_THROW_ON_ERROR)); }
function event(string $id,int $offset=0,bool $bad=false): void {
    global $customer;
    $body=json_encode(['id'=>$id,'object'=>'event','api_version'=>'2025-03-31.basil','type'=>'customer.subscription.updated','livemode'=>false,'data'=>['object'=>['customer'=>$customer,'status'=>'canceled']]],JSON_THROW_ON_ERROR);
    $timestamp=time()+$offset;$signature=hash_hmac('sha256',$timestamp.'.'.$body,'whsec_test_local');
    request('stripeWebhook.php',$body,'',$bad||$offset<-300?400:200,null,['Content-Type: application/json','Stripe-Signature: t='.$timestamp.',v1='.($bad?'bad':$signature)]);
}
try {
    $admin=new PDO('mysql:host='.getenv('DB_HOST').';port='.(getenv('DB_PORT')?:3306).';charset=utf8mb4',getenv('DB_USERNAME'),getenv('DB_PASSWORD'),[PDO::ATTR_ERRMODE=>PDO::ERRMODE_EXCEPTION]);
    $admin->exec('CREATE DATABASE `'.$name.'` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci');putenv('DB_DATABASE='.$name);putenv('BILLING_NAMESPACE='.$name);
    $socket=stream_socket_server('tcp://127.0.0.1:0',$errno,$error);$address=stream_socket_get_name($socket,false);fclose($socket);$port=(int)substr(strrchr($address,':'),1);
    $base='http://127.0.0.1:'.$port.'/subscriptions';putenv('PUBLIC_URL='.$base);putenv('TEST_PUBLIC_PREFIX=/subscriptions');
    putenv('STRIPE_SECRET_KEY=sk_test_local_fixture');putenv('STRIPE_WEBHOOK_SECRET=whsec_test_local');putenv('MAIL_TRANSPORT=log');
    putenv('TEST_PROVIDER_FILE='.$artifacts.'/provider.json');file_put_contents(getenv('TEST_PROVIDER_FILE'),'{}');
    putenv('MAIL_LOG_FILE='.$artifacts.'/mail.log');
    require $root.'/src/bootstrap.php';
    command('bin/migrate.php');command('bin/migrate.php');
    $catalog=['Test Game'=>['name'=>'Test Game','portal_configuration'=>'bpc_test_game','plans'=>['monthly'=>['name'=>'Monthly','description'=>'Test monthly','price_id'=>'price_monthly','display_price'=>'Test only','active'=>true],'yearly'=>['name'=>'Yearly','description'=>'Test yearly','price_id'=>'price_yearly','display_price'=>'Test only','active'=>true]]],'Other Game'=>['name'=>'Other Game','portal_configuration'=>'bpc_other','plans'=>[]]];
    file_put_contents($artifacts.'/catalog.php','<?php return '.var_export($catalog,true).';');
    // GAME_CATALOG accepts an absolute private file for isolated tests/deployments.
    putenv('GAME_CATALOG='.$artifacts.'/catalog.php'); command('bin/seed.php');
    $server=proc_open([PHP_BINARY,'-S','127.0.0.1:'.$port,'-t',$root.'/public',$root.'/tests/router.php'],[0=>['pipe','r'],1=>['file',$artifacts.'/server.log','a'],2=>['file',$artifacts.'/server.log','a']],$pipes,$root,null,['bypass_shell'=>true]);fclose($pipes[0]);
    $ready=false;for($i=0;$i<40;$i++){ $probe=@fsockopen('127.0.0.1',$port,$errno,$error,.1);if($probe){fclose($probe);$ready=true;break;}usleep(100000);}check($ready,'local test server started');
    request('health.php');
    $credentials=['_name'=>'Alice Test','_email'=>'alice@example.invalid','_notel'=>'0123456789','_password'=>'test-password-123','_game_name'=>'Test Game','_device_name'=>'Offline device'];
    $bad=$credentials;$bad['_password']='short';request('insertUser.php',$bad,'',422);
    check(request('insertUser.php',$credentials)['body']==='Successful','registration compatibility response');
    check(request('insertUser.php',$credentials)['body']==='email already registered','duplicate email blocked');
    $duplicate=$credentials;$duplicate['_email']='different@example.invalid';check(request('insertUser.php',$duplicate)['body']==='phone number already registered','duplicate phone blocked');
    $other=$credentials;$other['_game_name']='Other Game';request('insertUser.php',$other);
    $wrong=$credentials;$wrong['_password']='incorrect-test-password';check(request('loginVerify.php',$wrong)['body']==='password is not verify','incorrect password rejected');
    $session=request('loginVerify.php',$credentials)['json'];$access=$session['session_token'];$userId=$session['id_user'];
    check($session['login_logs']===0&&strlen($access)===64,'accepted login issues credential with legacy zero flag');
    check(\SubscriptionServer\query('SELECT token_hash FROM sessions WHERE user_id=?',[$userId])->fetchColumn()!==$access,'raw session token is not stored');
    request('getinfouser.php',['id_user'=>$userId],'',401);
    request('getinfouser.php',['id_user'=>$userId+100],$access,403);
    request('getinfo.php',['_email'=>'wrong@example.invalid'],$access,403);
    request('getinfouser.php',['_game_name'=>'Other Game'],$access,403);
    check(request('getinfouser.php',['id_user'=>$userId],$access)['json']['login_logs']===1,'authenticated poll returns active flag');
    check(request('getinfo.php',[],$access)['body']==='no result','confirmed missing subscription');
    $conflict=request('loginVerify.php',$credentials)['json'];check($conflict['login_logs']===1&&!isset($conflict['session_token']),'device conflict does not issue full credential');
    request('getinfouser.php',[],$conflict['session_action_token'],401);
    request('logout.php',['id_user'=>$userId],'',401);
    request('logout.php',['id_user'=>$userId,'action_token'=>$conflict['session_action_token']]);
    request('getinfouser.php',[],$access,401);
    request('logout.php',['id_user'=>$userId,'action_token'=>$conflict['session_action_token']],'',401);
    $access=request('loginVerify.php',$credentials)['json']['session_token'];
    request('createBillingLink.php',['action'=>'choose'],$access,403);
    request('sendVerification.php',['_email'=>'wrong@example.invalid'],$access,403);
    request('sendVerification.php',[],$access);
    $mail=\SubscriptionServer\query('SELECT body FROM mail_outbox WHERE user_id=? AND delivered_at IS NULL',[$userId])->fetchColumn();preg_match('/token=([a-f0-9]{64})/',$mail,$match);$verification=$match[1];
    \SubscriptionServer\query('UPDATE verification_tokens SET expires_at=? WHERE user_id=?',[time()-1,$userId]);request('verifyEmail.php?token='.$verification,null,'',400);
    request('sendVerification.php',[],$access);$mail=\SubscriptionServer\query('SELECT body FROM mail_outbox WHERE user_id=? AND delivered_at IS NULL',[$userId])->fetchColumn();preg_match('/token=([a-f0-9]{64})/',$mail,$match);
    request('verifyEmail.php?token='.$match[1]);request('verifyEmail.php?token='.$match[1],null,'',400);
    check(request('getinfouser.php',[],$access)['json']['confirmation']==='1','email verification updates legacy field');
    request('createBillingLink.php',['action'=>'unknown'],$access,400);
    $link=request('createBillingLink.php',['action'=>'choose'],$access)['json']['url'];$cookie=$artifacts.'/cookies.txt';
    $exchange=request($link,null,'',303,$cookie);check($exchange['headers']['location']===$base.'/pilihplan.php','handoff redirects to clean subfolder URL');
    check(str_contains($exchange['headers']['set-cookie'],'HttpOnly')&&str_contains($exchange['headers']['set-cookie'],'path=/subscriptions/'),'browser cookie is HttpOnly and scoped to hosting folder');
    request($link,null,'',401,$artifacts.'/other-cookies.txt');
    $page=request('pilihplan.php',null,'',200,$cookie);preg_match('/name="_csrf" value="([a-f0-9]+)"/',$page['body'],$match);$csrf=$match[1];
    request('createCheckout.php',['plan'=>'monthly','_csrf'=>$csrf],'',401);
    request('createCheckout.php',['plan'=>'monthly','_csrf'=>'wrong'],'',403,$cookie);
    request('createCheckout.php',['plan'=>'unknown','_csrf'=>$csrf],'',400,$cookie);
    $first=request('createCheckout.php',['plan'=>'monthly','_csrf'=>$csrf],'',303,$cookie)['headers']['location'];
    $second=request('createCheckout.php',['plan'=>'monthly','_csrf'=>$csrf],'',303,$cookie)['headers']['location'];
    check($first===$second,'duplicate checkout reuses unfinished session');
    $customer='cus_test_'.$userId;
    fixture(function(&$s){$s['fail_after_create']=true;});
    request('createCheckout.php',['plan'=>'yearly','_csrf'=>$csrf],'',503,$cookie);
    request('createCheckout.php',['plan'=>'yearly','_csrf'=>$csrf],'',303,$cookie);
    $provider=json_decode(file_get_contents(getenv('TEST_PROVIDER_FILE')),true);check(count($provider['checkouts'])===2,'interrupted provider response does not create duplicate checkout');
    $subscription=['id'=>'sub_test','customer'=>$customer,'status'=>'active','start_date'=>time()-100,'cancel_at_period_end'=>false,'items'=>['data'=>[['price'=>['id'=>'price_yearly','product'=>'prod_test'],'current_period_end'=>time()+3600]]]];
    fixture(function(&$s)use($customer,$subscription){$s['subscriptions'][$customer]=[$subscription];});
    event('evt_wrong',0,true);event('evt_stale',-1000);event('evt_current');event('evt_current');
    check((int)\SubscriptionServer\query('SELECT COUNT(*) FROM webhook_events WHERE event_id=?',['evt_current'])->fetchColumn()===1,'duplicate webhook receipt is idempotent');
    command('bin/processWebhooks.php');$summary=request('getinfo.php',[],$access)['json'];check($summary['status']==='active'&&$summary['access_until']>time(),'reconciliation activates valid paid period');
    request('createCheckout.php',['plan'=>'monthly','_csrf'=>$csrf],'',409,$cookie);
    $link=request('createBillingLink.php',['action'=>'manage'],$access)['json']['url'];request($link,null,'',303,$cookie);$page=request('manageSub.php?stripe_id=cus_attacker',null,'',200,$cookie);preg_match('/name="_csrf" value="([a-f0-9]+)"/',$page['body'],$match);
    request('createPortal.php',['_csrf'=>$match[1],'stripe_id'=>'cus_attacker'],'',303,$cookie);
    $provider=json_decode(file_get_contents(getenv('TEST_PROVIDER_FILE')),true);check($provider['portal_customer']===$customer&&$provider['portal_configuration']==='bpc_test_game','portal derives customer and game catalog from authenticated ownership');
    fixture(function(&$s)use($customer){$s['subscriptions'][$customer][0]['cancel_at_period_end']=true;});event('evt_older_payload');command('bin/processWebhooks.php');$summary=request('getinfo.php',[],$access)['json'];
    check($summary['status']==='active'&&$summary['cancel_at_period_end'],'old event payload cannot overwrite current paid access');
    fixture(function(&$s)use($customer){$s['subscriptions'][$customer][0]['items']['data'][0]['current_period_end']=time()-1;});\SubscriptionServer\synchronize($userId);$summary=request('getinfo.php',[],$access)['json'];check($summary['status']==='expired'&&$summary['access_until']===0,'expired paid period denies access');
    fixture(function(&$s)use($customer){$s['subscriptions'][$customer][0]['status']='canceled';$s['subscriptions'][$customer][0]['items']['data'][0]['current_period_end']=time()+3600;});\SubscriptionServer\synchronize($userId);check(request('getinfo.php',[],$access)['json']['status']==='canceled','immediate cancellation is not active');
    fixture(function(&$s){$s['fail_sync']=true;});event('evt_outage');request('getinfo.php',[],$access,503);command('bin/processWebhooks.php');check((int)\SubscriptionServer\query('SELECT attempts FROM webhook_events WHERE event_id=?',['evt_outage'])->fetchColumn()===1,'worker failure records a retry');
    fixture(function(&$s){$s['fail_sync']=false;});\SubscriptionServer\query('UPDATE webhook_events SET available_at=? WHERE event_id=?',[time(),'evt_outage']);command('bin/processWebhooks.php');
    request('paymentSuccess.php?activate=1');check(request('getinfo.php',[],$access)['json']['status']==='canceled','success page cannot grant access');
    \SubscriptionServer\query('UPDATE sessions SET expires_at=? WHERE user_id=?',[time()-1,$userId]);request('getinfo.php',[],$access,401);request('createPortal.php',['_csrf'=>$match[1]],'',401,$cookie);
    $children=[];for($i=0;$i<2;$i++){ $p=proc_open([PHP_BINARY,$root.'/tests/login_worker.php'],[0=>['pipe','r'],1=>['pipe','w'],2=>['file',$artifacts.'/worker.log','a']],$pips,$root,null,['bypass_shell'=>true]);fclose($pips[0]);$children[]=[$p,$pips[1]]; }
    $flags=[];foreach($children as [$p,$pipe]){$data=json_decode(stream_get_contents($pipe),true);fclose($pipe);proc_close($p);$flags[]=$data['login_logs']??-1;}sort($flags);check($flags===[0,1],'concurrent login admits exactly one active session');
    command('bin/processMail.php');command('bin/cleanup.php');
    $row=\SubscriptionServer\query('SELECT id FROM users WHERE id=?',[$userId])->fetch();check($row!==false,'maintenance preserves account records');
    echo "\n$checks backend assertions passed against an isolated database. No live Stripe calls made.\n";
} catch(Throwable $error) { fwrite(STDERR,$error->getMessage()."\n");exitCode(1); }
finally { if(is_resource($server)){proc_terminate($server);proc_close($server);} if($admin)$admin->exec('DROP DATABASE IF EXISTS `'.$name.'`'); }
function exitCode(int $value): void { $GLOBALS['failed_exit']=$value; }
if(isset($GLOBALS['failed_exit']))exit($GLOBALS['failed_exit']);
