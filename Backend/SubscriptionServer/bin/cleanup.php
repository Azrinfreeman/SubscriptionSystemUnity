<?php
declare(strict_types=1);
require __DIR__.'/common.php';
runCommand(function(){
    $now=time();
    \SubscriptionServer\query('DELETE FROM session_actions WHERE consumed=1 OR expires_at<=?',[$now]);
    \SubscriptionServer\query('DELETE FROM billing_handoffs WHERE consumed=1 OR expires_at<=?',[$now]);
    \SubscriptionServer\query('DELETE FROM verification_tokens WHERE expires_at<=?',[$now]);
    \SubscriptionServer\query('DELETE FROM mail_outbox WHERE (delivered_at IS NOT NULL AND delivered_at<?) OR (delivered_at IS NULL AND available_at<?)',[$now-86400,$now-86400]);
    \SubscriptionServer\query('DELETE FROM webhook_events WHERE processed_at IS NOT NULL AND processed_at<?',[$now-2592000]);
    \SubscriptionServer\query('DELETE FROM rate_limits WHERE started_at<?',[$now-86400]);
    \SubscriptionServer\query('UPDATE users u JOIN sessions s ON s.id=u.active_session_id SET u.active_session_id=NULL WHERE s.revoked=1 OR s.expires_at<=? OR s.last_seen<=?',[$now,$now-(int)\SubscriptionServer\env('SESSION_IDLE_SECONDS','1800')]);
    \SubscriptionServer\query('DELETE FROM sessions WHERE (revoked=1 OR expires_at<=?) AND last_seen<?',[$now,$now-2592000]);
    return 'Expired operational records cleaned.';
});
