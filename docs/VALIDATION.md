# Portfolio documentation review

Review date: **6 October 2026**, Asia/Kuala_Lumpur.
Source baseline: `24104d7f66dd81b0d1737e6ec4fe89f3f099818e` (7 August 2024).

The owner clarified the project goal on 6 October: integrate subscription payments into a Unity game so selected parts stay locked until the player subscribes. The README uses **Unity Subscription Unlock** and documents that intended workflow. After the owner explicitly approved the rename, the GitHub repository was renamed from `SubscriptionSystemUnity` to `Unity-Subscription-Unlock` on the same date; its repository identity and public visibility were preserved. The clone URL was updated. Account/status UI was inspected; no complete host-game entitlement gate or locked-content demo was verified. Application code was not changed.

## Checked for this update

- Unity and package versions against `ProjectSettings/ProjectVersion.txt` and `Packages/manifest.json`.
- Login/home scene names and order against `ProjectSettings/EditorBuildSettings.asset` and the tracked file tree.
- Registration, login, refresh, email-verification, logout, and hosted-page descriptions against the 12 top-level application C# files.
- GPM WebView version against its bundled `GpmWebView.cs`; native Android/iOS plugin paths and the existing GPM license notice against tracked files.
- No PHP backend or SQL schema was found in the tracked inventory. The historical Windows player files are present under `Publish/`, but were not executed.
- Local Markdown links, whitespace, and the final changed-file list.
- No GitHub release was listed and no application-owned test suite was found at review time.

A limited common-credential-pattern scan of the 12 top-level application scripts found no candidates. That check did not cover Git history, scenes, bundled plugins, or compiled player files and is not a complete credential/security audit.

Only the root README and this review document changed. Application scripts, domain configuration, scenes, plugins, package versions, and player files remain unchanged.

## Not verified

No Unity import, compilation, Play-mode run, device/native-WebView test, or build was performed. No configured backend request, account creation, email send, payment, subscription change, or login with real data was performed. Backend authorization, provider event handling, cancellation, and entitlement enforcement are outside the supplied source and were not assessed.

## Existing implementation observations

- `Domains.Awake` hard-codes an existing hosted URL. A controlled demo requires a separate local-copy domain change and a compatible test backend.
- `PlayerManager` starts a four-second repeating login check and requests account/subscription data for a cached login. No bundled offline service was found.
- `PlayerManager.AssignInformation` reads `start_date` and `end_date` from the `stripe_id` preference. The separate subscription view reads the date keys directly. This was documented without altering code.
- The client caches identifying/subscription fields in PlayerPrefs, sends identifiers in hosted-page query strings, and logs some responses. Logout clears all PlayerPrefs. Review these behaviors when integrating it into another application.
- WebView callbacks alone do not establish a completed payment. The external backend and provider configuration need a separate test-mode review before any end-to-end demo claim.
