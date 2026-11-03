# PHP integration contract

The rebuilt implementation is in [Backend/SubscriptionServer](../Backend/SubscriptionServer/README.md). That reference documents routes, fields, credential lifetimes, browser handoff and deployment. All API calls use POST WWWForm over the configured HTTPS base folder, preserving the original field names and legacy success/duplicate literals.

Registration and login are public. All account/subscription operations require a server-issued `Authorization: Bearer <session_token>`, scoped to the exact game. Posted account IDs and emails must match it. Login returns `session_token` and Unix `session_expires_at` with `login_logs=0`; subsequent valid profile refreshes return `login_logs=1`. A password-confirmed competing login returns only a restricted `session_action_token` for revoking that exact session. Login again after revocation.

Credentials remain in client memory. Old PlayerPrefs login flags cannot restore a session. Logout clears account-specific state without deleting unrelated preferences. A protected 401 expires only the matching current session generation.

`getinfo.php` returns a subscription object or literal `no result` only for confirmed absence. Provider/network failures remain errors. `end_date` means UTC paid-through time; `access_until` is its Unix timestamp when the mapped subscription is active and unexpired. The client requires current authenticated profile/subscription state and future access_until to grant premium access. Scheduled cancellation retains paid-through access. Redirects and local preferences never prove payment.

`createBillingLink.php` takes authenticated action choose/change/manage and returns a one-use 90-second ticket URL. Original billing pages exchange it for a 15-minute browser session bound to the API session; mutations require POST and CSRF. Customer IDs and allowed prices come from server records. Email/customer query strings do not authorize billing.

The backend verifies Stripe webhook signatures and queues durable receipts, then reconciles current provider state. Background mail, webhook and reconciliation workers must run. The detailed [server reference](../Backend/SubscriptionServer/README.md) includes all 17 public filenames and operational commands.

Local gameplay gating is a convenience API; server-hosted protected resources must independently authorize every request. Historical account recovery, native store billing and password reset are outside this implementation.
