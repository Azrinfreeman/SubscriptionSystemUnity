<?php
declare(strict_types = 1);
namespace SubscriptionServer;

function registerAccount(): void {
    postOnly();
    rateLimit('register', 10, 900);
    $game = game(field('_game_name', 100));
    $name = trim(field('_name', 100));
    $email = strtolower(trim(field('_email')));
    $phone = trim(field('_notel', 30));
    $password = field('_password', 128);
    // Keep this phone policy aligned with RegisterUserController.MatchPhoneNumberPattern.
    $validName = $name !== '';
    $validEmail = filter_var($email, FILTER_VALIDATE_EMAIL);
    $validPhone = preg_match('/^01[0-46-9][0-9]{7,8}$/', $phone);
    $validPassword = strlen($password) >= 12; // field() already enforces the 128-byte maximum.
    if (!$validName || !$validEmail || !$validPhone || !$validPassword) {
        throw new ApiError('invalid_registration', 'Use a name, valid email, Malaysian mobile number and a password of 12-128 bytes.', 422);
    }
    if (!defined('PASSWORD_ARGON2ID')) throw new ApiError('setup_required', 'PHP Argon2id support is required.', 503);
    if (query('SELECT id FROM users WHERE game_id=? AND email=?', [$game['id'], $email])->fetch()) plain('email already registered');
    if (query('SELECT id FROM users WHERE game_id=? AND phone=?', [$game['id'], $phone])->fetch()) plain('phone number already registered');
    try {
        $id = transaction(function () use ($game, $name, $email, $phone, $password) {
            query('INSERT INTO users(game_id,name,email,phone,password_hash,created_at) VALUES(?,?,?,?,?,?)', [$game['id'], $name, $email, $phone, password_hash($password, PASSWORD_ARGON2ID), time()]);
            return (int)database()->lastInsertId();
        });
    }
    catch (\PDOException $error) {
        if ($error->getCode() === '23000') plain(query('SELECT id FROM users WHERE game_id=? AND email=?', [$game['id'], $email])->fetch()?'email already registered':'phone number already registered');
        throw $error;
    }
    // Account creation remains successful if mail delivery is delayed.
    try {
        queueVerification(userById($id));
    }
    catch (\Throwable $error) {
        error_log('Verification enqueue failed: '.get_class($error));
    }
    plain('Successful');
}

function loginAccount(): void {
    postOnly();
    rateLimit('login', 20, 900);
    $game = game(field('_game_name', 100));
    $email = strtolower(trim(field('_email')));
    $password = field('_password', 128);
    $device = trim(field('_device_name', 200));
    $user = query('SELECT * FROM users WHERE game_id=? AND email=?', [$game['id'], $email])->fetch();
    $hash = $user['password_hash'] ?? '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2uheWG/igi';
    $correct = password_verify($password, $hash);
    if (!$user) plain('invalid email');
    if (!$correct) plain('password is not verify');
    $response = transaction(function () use ($user, $device) {
        // Serialize logins for this account before deciding whether to issue a token.
        $user = query('SELECT * FROM users WHERE id=? FOR UPDATE', [$user['id']])->fetch();
        $active = $user['active_session_id']?query('SELECT * FROM sessions WHERE id=?', [$user['active_session_id']])->fetch():false;
        if ($active && validSession($active)) {
            $action = token();
            query('INSERT INTO session_actions(token_hash,user_id,session_id,expires_at) VALUES(?,?,?,?)', [hashToken($action), $user['id'], $active['id'], time()+300]);
            $user['device_name'] = $active['device_name'];
            return profile($user, 1)+['session_action_token' => $action];
        }
        if ($active) query('UPDATE sessions SET revoked=1 WHERE id=?', [$active['id']]);
        $credential = token();
        $expires = time()+max(60, (int)env('SESSION_ABSOLUTE_SECONDS', '86400'));
        query('INSERT INTO sessions(user_id,token_hash,device_name,created_at,last_seen,expires_at) VALUES(?,?,?,?,?,?)', [$user['id'], hashToken($credential), $device !== ''?$device:'Unnamed device', time(), time(), $expires]);
        query('UPDATE users SET active_session_id=? WHERE id=?', [(int)database()->lastInsertId(), $user['id']]);
        return profile($user, 0)+['session_token' => $credential, 'session_expires_at' => $expires];
    });
    json($response);
}

function logoutAccount(): void {
    postOnly();
    rateLimit('logout', 30, 300);
    if (field('action_token', 64) !== '') {
        $action = query('SELECT * FROM session_actions WHERE token_hash=?', [hashToken(field('action_token', 64))])->fetch();
        if (!$action) throw new ApiError('unauthorized', 'Sign in again to authorize this action.', 401);
        transaction(function () use ($action) {
            $user = query('SELECT * FROM users WHERE id=? FOR UPDATE', [$action['user_id']])->fetch();
            $action = query('SELECT * FROM session_actions WHERE token_hash=? FOR UPDATE', [$action['token_hash']])->fetch();
            if ($action['consumed'] || (int)$action['expires_at'] <= time()) throw new ApiError('unauthorized', 'The session action has expired.', 401);
            if (field('id_user', 20) !== (string)$user['id']) throw new ApiError('account_mismatch', 'Account does not match this action.', 403);
            if ((int)$user['active_session_id'] !== (int)$action['session_id']) throw new ApiError('stale_action', 'The device session changed. Sign in again.', 409);
            query('UPDATE session_actions SET consumed=1 WHERE token_hash=?', [$action['token_hash']]);
            query('UPDATE sessions SET revoked=1 WHERE id=?', [$action['session_id']]);
            query('UPDATE users SET active_session_id=NULL WHERE id=?', [$user['id']]);
        });
    }
    else {
        $user = authenticated();
        transaction(function () use ($user) {
            $current = query('SELECT active_session_id FROM users WHERE id=? FOR UPDATE', [$user['id']])->fetchColumn();
            if ((int)$current !== (int)$user['session_id']) throw new ApiError('unauthorized', 'This session is no longer active.', 401);
            query('UPDATE sessions SET revoked=1 WHERE id=?', [$user['session_id']]);
            query('UPDATE users SET active_session_id=NULL WHERE id=?', [$user['id']]);
        });
    }
    plain('logout');
}

function queueVerification(array $user): void {
    if ($user['verified']) return;
    $raw = token();
    $link = publicUrl('verifyEmail.php').'?token='.rawurlencode($raw);
    transaction(function () use ($user, $raw, $link) {
        query('SELECT id FROM users WHERE id=? FOR UPDATE', [$user['id']]);
        query('DELETE FROM verification_tokens WHERE user_id=?', [$user['id']]);
        query('DELETE FROM mail_outbox WHERE user_id=? AND delivered_at IS NULL', [$user['id']]);
        query('INSERT INTO verification_tokens(token_hash,user_id,expires_at) VALUES(?,?,?)', [hashToken($raw), $user['id'], time()+3600]);
        query('INSERT INTO mail_outbox(user_id,recipient,body,available_at) VALUES(?,?,?,?)', [$user['id'], $user['email'], "Verify your game account by opening this link within one hour:\n".$link, time()]);
    });
}

function verifyEmail(): void {
    rateLimit('verify', 40, 900);
    $raw = $_GET['token'] ?? '';
    if (!is_string($raw) || !preg_match('/^[a-f0-9]{64}$/', $raw)) throw new ApiError('invalid_link', 'Verification link is invalid or expired.');
    transaction(function () use ($raw) {
        $record = query('SELECT * FROM verification_tokens WHERE token_hash=? FOR UPDATE', [hashToken($raw)])->fetch();
        if (!$record || (int)$record['expires_at'] <= time()) throw new ApiError('invalid_link', 'Verification link is invalid or expired.');
        query('UPDATE users SET verified=1 WHERE id=?', [$record['user_id']]);
        query('DELETE FROM verification_tokens WHERE user_id=?', [$record['user_id']]);
        query('DELETE FROM mail_outbox WHERE user_id=?', [$record['user_id']]);
    });
    page('Email verified', '<p>Return to your game and refresh your profile.</p>');
}

function processMail(): array {
    $sent = 0;
    $failed = 0;
    $ids = query('SELECT id FROM mail_outbox WHERE delivered_at IS NULL AND available_at<=? AND attempts<8 ORDER BY id LIMIT 30', [time()])->fetchAll();
    foreach ($ids as $entry) {
        try {
            transaction(function () use ($entry, &$sent) {
                $mail = query('SELECT * FROM mail_outbox WHERE id=? FOR UPDATE', [$entry['id']])->fetch();
                if (!$mail || $mail['delivered_at'] !== null) return;
                if (env('MAIL_TRANSPORT', 'log') === 'log') {
                    if (env('APP_ENV') === 'production') throw new \RuntimeException('Production log mail is disabled.');
                    if (!is_dir(ROOT.'/storage')) mkdir(ROOT.'/storage', 0700, true);
                    file_put_contents(env('MAIL_LOG_FILE', ROOT.'/storage/mail.log'), $mail['recipient']."\n".$mail['body']."\n", FILE_APPEND|LOCK_EX);
                }
                else {
                    if (env('MAIL_TRANSPORT') !== 'smtp') throw new \RuntimeException('Unknown mail transport.');
                    $smtp = new \PHPMailer\PHPMailer\PHPMailer(true);
                    $smtp->isSMTP();
                    $smtp->Host = env('SMTP_HOST');
                    $smtp->Port = (int)env('SMTP_PORT', '587');
                    $smtp->SMTPAuth = true;
                    $smtp->Username = env('SMTP_USERNAME');
                    $smtp->Password = env('SMTP_PASSWORD');
                    $smtp->SMTPSecure = env('SMTP_ENCRYPTION', 'tls');
                    $smtp->Timeout = 15;
                    $smtp->CharSet = 'UTF-8';
                    $smtp->setFrom(env('MAIL_FROM'), env('MAIL_FROM_NAME', 'Game accounts'));
                    $smtp->addAddress($mail['recipient']);
                    $smtp->Subject = 'Verify your game account';
                    $smtp->Body = $mail['body'];
                    $smtp->send();
                }
                query('UPDATE mail_outbox SET delivered_at=?,body=? WHERE id=?', [time(), '[delivered]', $mail['id']]);
                $sent++;
            });
        }
        catch (\Throwable) {
            $failed++;
            query('UPDATE mail_outbox SET attempts=attempts+1,available_at=? WHERE id=?', [time()+300, $entry['id']]);
        }
    }
    return ['sent' => $sent, 'failed' => $failed];
}
