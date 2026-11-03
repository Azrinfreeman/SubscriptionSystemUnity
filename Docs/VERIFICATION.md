# Verification of the in-place refactor

Verified on 2026-10-28 in the connected **SubscriptionSystemUnity** project using **Unity 2022.3.62f3**.

## Completed checks

| Check | Result |
| --- | --- |
| Offline data regression checks | 24 passed |
| Offline Play-mode scene checks | 26 passed |
| Unity console after the final scene run | No errors or warnings |
| Actual-project Windows x64 build | Succeeded; 0 errors, 0 warnings |
| Existing scene references and button events | Valid; only the intentionally empty backend URL needs configuration |
| Existing scene layout comparison | Same transforms and GameObject counts: LogMasuk 130, LamanUtama 123 |
| Original scene GUIDs and build list | Preserved |
| Visual inspection | Original entry screen and dashboard rendered from the existing scenes |

The validation build is under ignored `Build/SubscriptionValidation`. Its `build-result.txt` records the Unity build summary. Historical Publish binaries were not overwritten. The scene-check harness restores the original background-player setting as well as account preferences.

## What the checks exercise

The data checks cover JSON validation, the required login-status field, modern email suffixes, phone length, HTTPS configuration, endpoint joining, encoded payment query values, date-key mapping, stale-cache access denial, account cleanup, preference preservation, and one reusable Player component.

The scene checks exercise the original SignIn/SignUp and dashboard flows with an **editor-only** request transport. They cover duplicate login/registration clicks, successful response sequencing, persistent-manager duplication, slow and overlapping refreshes, unavailable network state, late responses after account cleanup, registration recovery, notice dismissal, verification cancellation when closing the profile, local logout despite remote failure, captured logout credentials, unrelated preferences, authenticated navigation, memory-only credentials and obsolete-401 protection.

These checks do not contact any external backend, send verification emails, open a payment page, or create a charge. Account preferences are restored after the checks, and the previously open scene is restored. No new application scene or runtime mock service was introduced.

## Fixes verified by code and scene inspection

- Removed fixed-delay session decisions and repeated runtime AddComponent calls.
- Corrected start/end date preference reads and consistent profile/subscription state application.
- Centralized account cleanup without PlayerPrefs.DeleteAll.
- Removed unused UnityEditor/Visual Scripting imports from the application's runtime scripts.
- Replaced runtime GameObject.Find/hierarchy-index lookups and per-frame UI refreshes with assigned references and state events.
- Repaired two refresh events targeting the missing CheckIfUserLogin component.
- Repaired the subscription button's missing Animator target using the existing SubscriptionDetails animator.
- Added shared, validated configuration to the existing Domains objects in both scenes.
- Added bounded billing-return refreshes, payment query encoding, and desktop/Editor browser fallback.

## Rebuilt backend validation

- 86 backend assertions passed against an isolated disposable MariaDB 10.4.32 database using PHP 8.2.12. No production database was used.
- Tests use a fake payment provider and the actual Stripe SDK signature verifier. They cover authentication, ownership, handoff replay/CSRF, session expiry, concurrent login, interrupted/duplicate checkout, out-of-order events, scheduled cancellation and provider failures.
- PHP syntax checks passed. Composer audit reported no security advisories in the locked dependencies at validation time.
- 24 Unity data checks and 26 offline scene checks passed after the credential integration. Tests use the existing LogMasuk/LamanUtama scenes and send no external requests.
- Windows player build succeeded with zero errors and warnings. Editor console was clean. The build is under ignored Build/SubscriptionValidation, leaving historical Publish output alone.
- Consumer PDF: 20 pages rendered and visually inspected, including the final revised registration page. Text extraction, copyable commands, 44 link annotations and 20 outline entries checked. Reproducible source and sample scene images are included.

## Still requires your environment

Deploy to your own HTTPS host and verify private/public folder separation, Authorization forwarding, actual SMTP receipt, a real Stripe test purchase, Customer Portal changes/cancellation, webhook retries and scheduled workers. Test Android/iOS WebView, keyboard/touch, resume and screen sizes on real devices. No live payment, hosting deployment, customer migration or native store billing certification has occurred.

The included backend now authenticates account operations with server-issued bearer tokens. IDs, emails and local preferences alone cannot authorize them. CanAccessPremium requires a future server-provided access_until as well as current valid state.

## Re-run

Use Tools > Subscription System > Run Offline Regression Checks outside Play mode, then Run Offline Scene Checks. Save your scene changes first. The scene suite temporarily enables background execution and restores its settings and account preferences afterward. Backend test requirements and commands are in the server README; use a localhost disposable database account only.

The PDF source includes a rebuild command. Every delivered PDF page must be rendered and visually inspected after content changes. Hosting/Stripe/settings diagrams are labeled illustrations; existing Unity screen captures use sample data.

Existing user package, editor-version and ignore-file changes were preserved. No separate game or startup scene was added.
## UI revamp verification — 3 November 2026

The original scenes now contain the game-menu redesign. Fresh runs passed 24 offline data checks and 30 offline scene checks, including focus inside forms, interrupted panel transitions and reduced-motion opening. Final Windows validation build: `Succeeded | errors=0 | warnings=0`. Both scene validators reported only the intentionally empty HTTPS backend URL; no missing serialized references or invalid button events were reported.

Offscreen UI renders cover entry, forms, device conflict, home, profile, subscription and logout prompt at 1920x1080, 1280x720, 1024x768, 2560x1080 and 720x1280. Captures: `Docs/UIRevamp/after/`. These are layout previews, not device screenshots. The tutorial HTML was rendered and visually inspected with no browser console messages. Theme primary-color customization was applied programmatically, verified across loaded bindings and restored.

Measured contrast: primary/white 7.11:1; ink/field 11.13:1; ink/white 12.54:1; error/white 6.56:1; gold/primary 4.18:1. Manual pointer/keyboard/gamepad review, real mobile keyboard and safe-area testing, hosted SMTP and Stripe/WebView transactions remain open. Existing setup PDF/video screenshots depict the previous UI. See [customization and testing steps](tutorial/game-ui-customization.md).

---



## Maintainability refactor

Recorded 3 November 2026. The current source retains both original scenes, public action methods, serialized field names/types, script GUIDs, shared theme values and button-face prefab. This pass clarified source code and documentation; it did not change the authentication, payment or entitlement policy.

| Check | Fresh result |
| --- | --- |
| Unity data regressions | 27 passed, including three added cache-mapping/null-value assertions. Existing preferences restored. |
| Unity offline scenes | 30 passed in the original scenes; no external requests. One run was interrupted during editor recompilation; the fresh run after compilation settled completed successfully. |
| Scene validators | Both scenes report only the intentionally empty backend URL; no missing references/scripts or invalid button actions. |
| Windows player build | Succeeded, zero errors and zero warnings, using the existing validation build helper. |
| Backend | 86 assertions passed before and after refactoring, using PHP 8.2.12 and a separate disposable MariaDB 10.4.32 instance on localhost. No real email or Stripe calls. Changed PHP files pass syntax checks. |
| Source compatibility | AST comparison of 23 client files preserves public fields/actions/properties, serialized field names/types and public enum numeric values. Hash comparison found no changes to 889 protected meta/scene/prefab/settings/package/project files. |
| Visual regression | Default entry/home renders inspected at 1920x1080, 1280x720, 1024x768, 2560x1080 and 720x1280. At 1920x1080 both scenes match the baseline pixel-for-pixel. These are offscreen renders. |
| Menu customization | Programmatically applied/restored Primary #286C55 and feedback duration 0.18 in both scenes; verified 4 login and 8 home primary bindings. Applied/restored the documented login label override. |
| Account-row recipe | Temporarily compiled the documented field/render extension, assigned a duplicated real profile row, verified read-only IDs 9/12 and clearing on account cleanup, then restored source, preferences and scenes. No extra profile field is shipped. |
| Phone recipe | Six proposed international-format fixtures pass in C# and PHP. The shipped Malaysian policy is unchanged; no existing accounts were migrated. |
| Tutorial exporter | Seven focused tests pass for escaped code, semantic tables, list numbering, companion links and rejected unsupported/unsafe constructs. Both companions regenerate deterministically. |
| Tutorial presentation | Desktop and narrow HTML previews inspected, including code samples and lower sections. Fixed narrow overflow from unbroken source paths. Programmatic focus shows the authored outline; design scanner reports zero findings for both exports. |
| Mermaid | Embedded and standalone editing map agree; lint has zero errors/warnings (five nodes, four edges). Parser/render not performed because the renderer is not installed. Visual diagram review remains incomplete. |

The HTML checks used the headless preview after collaborative browser snapshots timed out twice. Browser-native keyboard traversal was not completed; programmatic focus checks are narrower evidence. The full human Inspector walkthrough, real gamepad/touch keyboard/safe-area behavior, mobile WebView, hosted SMTP and real Stripe test transactions remain unperformed. Existing historical limits above still apply.

Re-run the Unity menu checks as described in [setup](SETUP.md#confirm-the-template-works-offline) and the backend suite using [its test prerequisites](../Backend/SubscriptionServer/README.md#verification-and-release). Run tutorial tests with `python -m unittest discover -s Docs/tutorial -p test_tutorial_export.py`, then regenerate companions with `python Docs/tutorial/export_game_ui_tutorial.py`.

The local recovery snapshot and raw run artifacts are under ignored `Build/MaintainabilityBaseline-20261103/`; the Windows player is under `Build/SubscriptionValidation/`. These are local outputs, not editable source or required repository dependencies. The disposable test server and tutorial preview server are stopped after validation.

## Tutorial PDF companions

Recorded 3 November 2026. The menu tutorial exports to five Letter pages and the account tutorial to six. Markdown remains authoritative; both screen HTML and static print HTML regenerate from it. The README links both PDFs. Rebuild with `python Docs/tutorial/export_tutorial_pdfs.py`; prerequisites and style sources are documented in the tutorials and `tutorial/DOC.md`.

The PDF exporter ran successfully using installed Edge and Python Playwright. It checked text completeness before export and rejected horizontal/vertical page overflow. PyMuPDF 1.26.7 then rasterized every page and checked page dimensions, text bounds, missing replacement glyphs and valid destinations for all six contents links in each PDF. Georgia, Segoe UI and Consolas are embedded. All pages were visually inspected, including the closing sections, repeated table headers, code samples and diagram. This verifies document presentation; it does not rerun historical Unity/backend checks.

Eight parser tests pass, including the added local-illustration/remote-source guard. Both final print HTML files have zero findings in the avoid-ai-design scanner. Repository links in the Markdown resolve. The original Mermaid still passes source lint; the print SVG revision passes static/security lint with one documented palette advisory and visual review at 1200/600 px. See [the diagram review](../diagram-design/customization-map/review.md).

Print production uses `tutorial-print.css`, `paginate-tutorial.js` and `export_tutorial_pdfs.py`. The PDFs are tagged and have outlines, but assistive-technology reading order was not tested. Review scripts and raster previews are retained locally in ignored `Build/DocumentationReview/`; PyMuPDF is needed only to rerun those review scripts, not to export the PDFs.
