<?php
declare(strict_types = 1);
namespace SubscriptionServer;

final class ApiError extends \RuntimeException {
    public function __construct(public string $errorCode, string $message, public int $status = 400) {
        parent::__construct($message);
    }
}

function env(string $key, string $default = ''): string {
    $value = getenv($key);
    return $value !== false?(string)$value:(string)($_ENV[$key] ?? $default);
}

function database(): \PDO {
    static $db;
    if ($db) return $db;
    $name = env('DB_DATABASE');
    if (!preg_match('/^[a-zA-Z0-9_]+$/', $name)) throw new ApiError('setup_required', 'Configure the server database.', 503);
    $connectionString = 'mysql:host='.env('DB_HOST', '127.0.0.1')
        .';port='.env('DB_PORT', '3306').';dbname='.$name.';charset=utf8mb4';
    $db = new \PDO($connectionString, env('DB_USERNAME'), env('DB_PASSWORD'), [
        \PDO::ATTR_ERRMODE => \PDO::ERRMODE_EXCEPTION,
        \PDO::ATTR_DEFAULT_FETCH_MODE => \PDO::FETCH_ASSOC,
        \PDO::ATTR_EMULATE_PREPARES => false,
    ]);
    $db->exec("SET time_zone = '+00:00'");
    return $db;
}

function query(string $sql, array $params = []): \PDOStatement {
    $statement = database()->prepare($sql);
    $statement->execute($params);
    return $statement;
}

function transaction(callable $action): mixed {
    database()->beginTransaction();
    try {
        $result = $action();
        database()->commit();
        return $result;
    }
    catch (\Throwable $error) {
        if (database()->inTransaction()) database()->rollBack();
        throw $error;
    }
}

function token(): string {
    return bin2hex(random_bytes(32));
}

function hashToken(string $value): string {
    return hash('sha256', $value);
}

function postOnly(): void {
    if (($_SERVER['REQUEST_METHOD'] ?? '') !== 'POST') throw new ApiError('method_not_allowed', 'Use POST.', 405);
}

function field(string $name, int $max = 254): string {
    $value = $_POST[$name] ?? '';
    if (!is_string($value) || strlen($value)>$max) throw new ApiError('invalid_input', 'Invalid field: '.$name);
    return $value;
}

function json(array $value, int $status = 200): never {
    http_response_code($status);
    header('Content-Type: application/json; charset=utf-8');
    echo json_encode($value, JSON_THROW_ON_ERROR);
    exit;
}

function plain(string $value, int $status = 200): never {
    http_response_code($status);
    header('Content-Type: text/plain; charset=utf-8');
    echo $value;
    exit;
}

function publicUrl(string $path = ''): string {
    $base = rtrim(env('PUBLIC_URL'), '/');
    $uri = parse_url($base);
    $test = env('APP_ENV') === 'test' && ($uri['host'] ?? '') === '127.0.0.1';
    if (!filter_var($base, FILTER_VALIDATE_URL) || (!$test && ($uri['scheme'] ?? '') !== 'https') || isset($uri['query']) || isset($uri['fragment']) || isset($uri['user'])) throw new ApiError('setup_required', 'Configure an HTTPS PUBLIC_URL including the hosting folder.', 503);
    return $base.($path !== ''?'/'.$path:'');
}

function game(string $key): array {
    $game = query('SELECT * FROM games WHERE game_key=? AND active=1', [$key])->fetch();
    if (!$game) throw new ApiError('unknown_game', 'The game identifier is not configured.', 403);
    return $game;
}

function rateLimit(string $scope, int $limit, int $seconds): void {
    $bucket = hashToken($scope.':'.($_SERVER['REMOTE_ADDR'] ?? 'cli'));
    $now = time();
    query('INSERT INTO rate_limits(bucket,started_at,hits) VALUES(?,?,1) ON DUPLICATE KEY UPDATE hits=IF(started_at<=?,1,hits+1),started_at=IF(started_at<=?,VALUES(started_at),started_at)', [$bucket, $now, $now-$seconds, $now-$seconds]);
    if ((int)query('SELECT hits FROM rate_limits WHERE bucket=?', [$bucket])->fetchColumn()>$limit) throw new ApiError('rate_limited', 'Too many attempts. Please wait and retry.', 429);
}

function userById(int $id): array {
    $user = query('SELECT u.*,g.game_key,g.portal_configuration FROM users u JOIN games g ON g.id=u.game_id WHERE u.id=? AND g.active=1', [$id])->fetch();
    if (!$user) throw new ApiError('unauthorized', 'Sign in again.', 401);
    return $user;
}

function validSession(array $session): bool {
    return !(bool)$session['revoked'] && (int)$session['expires_at']>time() && (int)$session['last_seen']>time()-(int)env('SESSION_IDLE_SECONDS', '1800');
}

function authenticated(): array {
    $header = $_SERVER['HTTP_AUTHORIZATION'] ?? '';
    if (!preg_match('/^Bearer ([a-f0-9]{64})$/', $header, $match)) {
        throw new ApiError('unauthorized', 'Your session expired. Sign in again.', 401);
    }
    $session = query('SELECT s.* FROM sessions s JOIN users u ON u.id=s.user_id JOIN games g ON g.id=u.game_id WHERE s.token_hash=? AND u.active_session_id=s.id AND g.active=1', [hashToken($match[1])])->fetch();
    if (!$session || !validSession($session)) {
        throw new ApiError('unauthorized', 'Your session expired. Sign in again.', 401);
    }
    $user = userById((int)$session['user_id']);
    // Posted identity is only a consistency check; the bearer session owns the account.
    if (field('_game_name', 100) !== '' && field('_game_name', 100) !== $user['game_key']) {
        throw new ApiError('account_mismatch', 'Game does not match this session.', 403);
    }
    if (field('id_user', 20) !== '' && (string)$user['id'] !== field('id_user', 20)) {
        throw new ApiError('account_mismatch', 'Account does not match this session.', 403);
    }
    if (field('_email') !== '' && strtolower(trim(field('_email'))) !== $user['email']) {
        throw new ApiError('account_mismatch', 'Email does not match this session.', 403);
    }
    query('UPDATE sessions SET last_seen=? WHERE id=? AND revoked=0', [time(), $session['id']]);
    $user['session_id'] = (int)$session['id'];
    return $user;
}

function profile(array $user, int $loginLogs = 1): array {
    return [
        'id_user' => (int)$user['id'],
        'name' => $user['name'],
        'email' => $user['email'],
        'notel' => $user['phone'],
        'confirmation' => $user['verified'] ? '1' : '0',
        'login_logs' => $loginLogs,
        'device_name' => $user['device_name'] ?? '',
    ];
}
