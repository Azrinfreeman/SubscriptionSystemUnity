<?php
declare(strict_types=1);
namespace SubscriptionServer;

function escape(string $value): string { return htmlspecialchars($value,ENT_QUOTES|ENT_SUBSTITUTE,'UTF-8'); }
function page(string $title,string $body,int $status=200): never {
    http_response_code($status); header('Content-Type: text/html; charset=utf-8');
    echo '<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>'.escape($title).'</title><link rel="stylesheet" href="assets/style.css"></head><body><main><p class="eyebrow">GAME SUBSCRIPTIONS</p><h1>'.escape($title).'</h1>'.$body.'<footer>Return to your game to refresh access. Never share a billing link.</footer></main></body></html>'; exit;
}
function browserSession(): void {
    if (session_status()===PHP_SESSION_ACTIVE) return;
    $directory=ROOT.'/storage/sessions'; if(!is_dir($directory)) mkdir($directory,0700,true);
    ini_set('session.use_strict_mode','1'); session_save_path($directory); session_name('game_billing');
    $path=(string)(parse_url(publicUrl(),PHP_URL_PATH)??'');
    session_set_cookie_params(['lifetime'=>0,'path'=>rtrim($path,'/').'/','secure'=>env('APP_ENV')!=='test','httponly'=>true,'samesite'=>'Lax']); session_start();
}
function createBillingLink(): void {
    postOnly(); $user=authenticated(); rateLimit('billing-link-'.$user['id'],20,300);
    if (!$user['verified']) throw new ApiError('verification_required','Verify your email first.',403);
    $action=field('action',10); $paths=['choose'=>'pilihplan.php','change'=>'tukarplan.php','manage'=>'manageSub.php'];
    if (!isset($paths[$action])) throw new ApiError('invalid_action','Choose a valid billing action.');
    if ($action!=='choose'&&!query('SELECT customer_id FROM billing_customers WHERE user_id=?',[$user['id']])->fetchColumn()) throw new ApiError('no_subscription','Subscribe before opening management.',409);
    $ticket=token();
    transaction(function()use($user,$ticket,$action){
        query('SELECT id FROM users WHERE id=? FOR UPDATE',[$user['id']]); activeBrowserSession($user);
        query('INSERT INTO billing_handoffs(token_hash,user_id,session_id,action,expires_at) VALUES(?,?,?,?,?)',[hashToken($ticket),$user['id'],$user['session_id'],$action,time()+90]);
    });
    json(['url'=>publicUrl($paths[$action]).'?ticket='.rawurlencode($ticket),'expires_at'=>time()+90]);
}
function browserUser(array $actions, string $pageName=''): array {
    browserSession();
    if (isset($_GET['ticket'])) {
        $raw=$_GET['ticket'];
        if (!is_string($raw)||!preg_match('/^[a-f0-9]{64}$/',$raw)) throw new ApiError('invalid_link','This billing link is invalid or expired.',401);
        $handoff=transaction(function()use($raw,$actions){
            $ticket=query('SELECT * FROM billing_handoffs WHERE token_hash=? FOR UPDATE',[hashToken($raw)])->fetch();
            if (!$ticket||$ticket['consumed']||(int)$ticket['expires_at']<=time()||!in_array($ticket['action'],$actions,true)) throw new ApiError('invalid_link','Return to the game to request a new billing link.',401);
            query('UPDATE billing_handoffs SET consumed=1 WHERE token_hash=?',[$ticket['token_hash']]); return $ticket;
        });
        session_regenerate_id(true);
        $_SESSION=['user_id'=>(int)$handoff['user_id'],'session_id'=>(int)$handoff['session_id'],'action'=>$handoff['action'],'expires_at'=>time()+900,'csrf'=>token()];
        $user=userById($_SESSION['user_id']); $user['session_id']=$_SESSION['session_id']; activeBrowserSession($user);
        header('Location: '.publicUrl($pageName),true,303); exit;
    }
    if (!isset($_SESSION['user_id'])||($_SESSION['expires_at']??0)<=time()||!in_array($_SESSION['action']??'',$actions,true)) throw new ApiError('unauthorized','Open this page from your game using a fresh billing link.',401);
    $user=userById((int)$_SESSION['user_id']); $user['session_id']=(int)$_SESSION['session_id']; activeBrowserSession($user); return $user;
}
function csrf(): void {
    if (!isset($_SESSION['csrf'])||!hash_equals($_SESSION['csrf'],field('_csrf',64))) throw new ApiError('csrf_failed','The form expired. Reopen billing from the game.',403);
}
function planPage(): void {
    $user=browserUser(['choose'],'pilihplan.php');
    $plans=query('SELECT * FROM plans WHERE game_id=? AND active=1 ORDER BY id',[$user['game_id']])->fetchAll();
    $body='<p>Choose a plan for <strong>'.escape($user['game_key']).'</strong>. Signed in as '.escape($user['email']).'.</p><div class="plans">';
    foreach($plans as $plan) $body.='<section><h2>'.escape($plan['name']).'</h2><p class="price">'.escape($plan['display_price']).'</p><p>'.escape($plan['description']).'</p><form method="post" action="createCheckout.php"><input type="hidden" name="_csrf" value="'.escape($_SESSION['csrf']).'"><input type="hidden" name="plan" value="'.escape($plan['plan_key']).'"><button>Continue to secure checkout</button></form></section>';
    $body.='</div><p>Prices and renewal terms are confirmed on Stripe Checkout. Only complete payment if they match your selection.</p>';
    page('Choose your plan',$body);
}
function managementPage(bool $change): void {
    $action=$change?'change':'manage'; $user=browserUser([$action],$change?'tukarplan.php':'manageSub.php');
    $summary=subscriptionSummary($user); $description=$summary?'Current plan: '.escape($summary['plan']).' | Status: '.escape($summary['status']):'No mapped subscription found.';
    page($change?'Change your plan':'Manage subscription','<p>'.$description.'</p><p>Stripe will show the available changes, billing dates and any amount due before you confirm. Cancellation options follow the configured portal policy.</p><form method="post" action="createPortal.php"><input type="hidden" name="_csrf" value="'.escape($_SESSION['csrf']).'"><button>Open secure billing portal</button></form>');
}
function checkoutPage(): void {
    postOnly(); $user=browserUser(['choose']); csrf(); rateLimit('checkout-'.$user['id'],10,300);
    header('Location: '.checkout($user,field('plan',80)),true,303); exit;
}
function portalPage(): void {
    postOnly(); $user=browserUser(['change','manage']); csrf(); rateLimit('portal-'.$user['id'],10,300);
    $customer=query('SELECT customer_id FROM billing_customers WHERE user_id=?',[$user['id']])->fetchColumn();
    if (!$customer) throw new ApiError('no_subscription','No billing account exists yet.',409);
    if ($user['portal_configuration']===''&&(int)query('SELECT COUNT(*) FROM games WHERE active=1')->fetchColumn()>1) throw new ApiError('portal_not_configured','Configure a separate portal catalog for this game.',503);
    header('Location: '.safePaymentUrl(gateway()->portal((string)$customer,$user['portal_configuration'])),true,303); exit;
}
