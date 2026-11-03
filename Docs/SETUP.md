# Set up and customize the template

Start with the offline Unity checks, then configure the included PHP server and connect the two original scenes. Online registration, email and billing need your own authorized development services. The shipped backend URL is deliberately empty.

## Contents

- [Check prerequisites](#check-prerequisites)
- [Confirm the template works offline](#confirm-the-template-works-offline)
- [Prepare the server](#prepare-the-server)
- [Connect Unity](#connect-unity)
- [Exercise the online flow](#exercise-the-online-flow)
- [Find the right editing point](#find-the-right-editing-point)
- [Connect gameplay and build](#connect-gameplay-and-build)
- [Troubleshoot by symptom](#troubleshoot-by-symptom)

## Check prerequisites

Open this folder with Unity 2022.3.62f3, the version in `ProjectSettings/ProjectVersion.txt`. Install Windows build support for the included validation build, or support for your intended target. Keep the project's existing uGUI, TextMeshPro and GPM WebView packages.

For online use, prepare HTTPS hosting, MySQL/MariaDB, compatible PHP 8.2+ with the extensions listed in the [server installation reference](../Backend/SubscriptionServer/README.md#install), Composer, SMTP and Stripe test-mode access. The server reference owns deployment commands and private configuration details. Unity does not run the PHP server.

Current instructions live here and in the linked references. The PDF under `output/pdf/` and the trailer contain historical UI screenshots; they are not the starting point for the current menus.

## Confirm the template works offline

1. Open `Assets/Scenes/LogMasuk.unity`. Save scene edits and leave Play mode.
2. Run `Tools > Subscription System > Validate Current Scene`. Repeat for `Assets/Scenes/LamanUtama.unity`. An empty URL should produce `Set an HTTPS backend base URL on SubscriptionSettings.` Fix missing references, scripts or button events separately.
3. Run `Tools > Subscription System > Run Offline Regression Checks`.
4. With the backend URL still empty, run `Tools > Subscription System > Run Offline Scene Checks`. Wait for the Console result and return to Edit mode before changing scripts or scenes.

The scene suite uses an editor-only request transport in the existing scenes. It saves and restores account/motion preferences and the previously open scene. It does not contact email or payment services. A watchdog stops stalled runs. Save work first; do not stop a run halfway through to edit its fixtures.

## Prepare the server

Follow [Install](../Backend/SubscriptionServer/README.md#install), then [Email](../Backend/SubscriptionServer/README.md#email), [Stripe](../Backend/SubscriptionServer/README.md#stripe) and [Workers and operations](../Backend/SubscriptionServer/README.md#workers-and-operations), in that order.

The setup has four checkpoints:

| Checkpoint | Expected result |
| --- | --- |
| Public/private separation | Only `Backend/SubscriptionServer/public/` is served. Private configuration, dependencies, tests and logs cannot be fetched over HTTP. |
| Configuration and catalog | `.env` and `config/games.php` are private. The catalog has your exact game key, configured test prices and a stable installation namespace. |
| Database and process | Migration/seed and `php bin/checkConfig.php` succeed. `/health.php` returns `status: ok`; this alone does not verify the database, email or payment provider. |
| Workers | Mail, webhook processing and reconciliation run on schedule. Development log mail stays private; actual SMTP delivery is a separate check. |

Run installation commands from `Backend/SubscriptionServer`, not from the Unity root. Do not paste server secrets into Unity assets. The backend README contains the authoritative commands, webhook event list and worker schedule so those instructions have one maintained source.

## Connect Unity

Choose `Tools > Subscription System > Select Settings`, which selects `Assets/Settings/SubscriptionSettings.asset`. Both scenes' `Domains` components must reference this same asset.

| Inspector field | Enter or keep |
| --- | --- |
| Backend Base URL | Your HTTPS public folder, for example `https://example.com/subscriptions/`. No PHP filename, credentials, query or fragment. |
| Game Identifier | The exact key in private `config/games.php`; the default is `Your Game`. This is separate from the menu title. |
| Request Timeout Seconds | Default 20; valid range 1–120. |
| Refresh Interval Seconds | Default 30; minimum 5. |
| Login Scene / Home Scene | `LogMasuk` / `LamanUtama`. Keep both enabled in Build Settings. |
| Enforce Server Login Status | Keep enabled for the included server's single-device session policy. |
| API endpoints and billing pages | Keep the included relative PHP paths unless your server routes intentionally differ. |

Validate both scenes again. The empty-URL message should disappear after valid configuration. Never disable authentication or grant access from cached flags to get past a configuration error.

The included server returns `login_logs=0` when issuing a login credential and `login_logs=1` on valid profile refresh. A competing-device response contains a restricted action token for revoking that session; sign in again afterward. Protected calls require the bearer credential, which stays in memory. Restarting the app requires another login. See [the contract](BACKEND_CONTRACT.md) for the wire fields and ticket lifetimes.

## Exercise the online flow

Use an authorized development deployment and Stripe test mode.

1. Register from SignUp. Check invalid input, duplicate email/phone and retry after a failed request. The server enforces Malaysian mobile numbers and passwords of 12–128 bytes; the client currently checks password presence and displays server rejection messages.
2. Sign in and inspect the profile in LamanUtama. Confirm another account cannot see the previous account's cached details.
3. Request verification. Process the mail queue, follow the single-use link, then reopen the profile or resume the app. Development log delivery and real mailbox receipt are different checks.
4. Open subscription details. Try Subscribe, change/manage actions, cancellation and refresh with test prices. Billing opens an authenticated single-use ticket URL, not an email/customer query string.
5. Return to the app and wait for server refresh. Browser return alone never grants access. Test unavailable network/provider state and a pending webhook as well as confirmed access.
6. Sign out, restart and sign in again. Unrelated game preferences should survive logout; bearer sessions should not survive process restart.

Android/iOS use the existing GPM WebView. Editor and desktop open the system browser. Closing the WebView or resuming the app requests bounded refreshes; if webhook processing is delayed, use Muat semula again. This template does not add native store billing.

## Find the right editing point

| Change | Authoritative source | How to verify |
| --- | --- | --- |
| Backend address, game key, refresh timing | `Assets/Settings/SubscriptionSettings.asset` | Validate both scenes, then inspect the authorized online flow. |
| Colors and animation durations | `Assets/UI/Settings/DefaultGameUITheme.asset` | Inspect both menus and recheck contrast/reduced motion. |
| Button appearance | `Assets/UI/Prefabs/MenuButtonFace.prefab` | Inspect instances without overwriting action-specific label overrides. |
| Layout and labels | The original scenes under `Canvas/SafeArea` | Check pointer, Tab/Back and narrow viewports. |
| Form rules and responses | `Assets/LoginController.cs`, `Assets/RegisterUserController.cs`; server `src/Accounts.php` | Check accepted/rejected input on both sides. |
| Profile display | `Assets/loadDetails.cs`, `Assets/loadNama.cs` | Trigger `StateChanged`, close/reopen the panel and switch accounts. |
| Subscription state and access | `Assets/PlayerManager.cs`; server `src/Billing.php` | Run offline state checks and isolated backend regressions. |
| HTTP transport and parsing | `Assets/Player.cs`, `Assets/SubscriptionSettings.cs` | Check failed requests, response validation and stale 401 handling. |

Follow [Customize the game menus](tutorial/game-ui-customization.md) for theme, label and motion edits. Follow [Customize the account flow](tutorial/account-flow-customization.md) for ownership, a display-only row and coordinated phone validation.

Keep existing component names and public UnityEvent methods when adapting the template. Serialized references make routine layout edits possible, but replacement controls still need assigning in the Inspector. One-time migration helpers such as `GameUIRevampTools.Apply` are not customization generators.

## Connect gameplay and build

Use `PlayerManager.CanAccessPremium()` before opening premium content. It requires a valid session, fresh profile/subscription results, `active` status and a future server-provided `access_until`. During refresh it may return false; show a loading/retry state rather than permanently locking the action.

```csharp
var manager = PlayerManager.instance;
if (manager != null && manager.CanAccessPremium())
{
    // Open your premium content.
}
else
{
    // Show a loading, retry or subscription prompt for the current state.
}
```

Subscribe to `PlayerManager.StateChanged` for display updates and unsubscribe on disable/destroy. Protected server resources must authorize requests on the server independently of this local UI hook. There is no profile-edit endpoint in this client; its profile fields are read-only.

For Windows verification, the existing editor helper `SubscriptionTemplateTools.BuildWindowsValidation()` writes `Build/SubscriptionValidation/SubscriptionSystem.exe`. For a release, use normal Build Settings, set identifiers/icons/signing in Player Settings, include your gameplay scenes, and build to a fresh directory. Keep the account scenes enabled. Published binaries under `Publish/` are historical.

Test the built player on target devices, including keyboard/touch, safe area, pause/resume and WebView behavior. Current evidence and unperformed device/service checks are in [VERIFICATION.md](VERIFICATION.md).

## Troubleshoot by symptom

| Symptom | Check and editing point |
| --- | --- |
| Empty URL or configuration failure | Select SubscriptionSettings; enter the public HTTPS folder and run scene validation. |
| Unknown game or wrong catalog | Compare Game Identifier with `config/games.php`, then follow the server's seed instructions. |
| Protected 401 | Sign in again; check session expiry, game ownership and Authorization forwarding at the host. Do not use an ID as a credential. |
| No email | Check queue worker results and private development mail log, or SMTP delivery settings. A health response does not prove delivery. |
| Payment returned but access is unavailable | Inspect webhook/reconciliation workers, provider result and paid-through time. Refresh again after processing. |
| Missing script, field or button target | Reassign the existing controller/loader reference in the Inspector and validate that scene. |
| Invisible panel or blocked clicks | Check panel active state, CanvasGroup, legacy Animator, EventSystem and decorative Raycast Target flags. |
| HTML tutorial is stale | Run `python Docs/tutorial/export_game_ui_tutorial.py` from the root; edit Markdown or the exporter, not generated HTML. |
