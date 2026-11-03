# Maintainability checkpoint ? 3 November 2026

The approved maintainability/refactoring work is implemented. Source ownership remains in the existing Unity components and PHP functions. Public/serialized interfaces and scene/prefab/settings values were preserved. README now leads to the current setup path and two practical Markdown/HTML customization tutorials.

Fresh verification: 27 Unity data checks, 30 offline scene checks, 86 isolated backend assertions, seven tutorial-exporter tests; Windows validation build succeeded with zero errors/warnings. Source contracts and protected file hashes match the baseline. Scripted theme/label/duration/account-row examples were applied and restored; proposed international-phone fixtures pass in C# and PHP without changing the shipped policy.

Current evidence and limitations: [Docs/VERIFICATION.md](Docs/VERIFICATION.md#maintainability-refactor). Human Inspector/input, real devices, hosted SMTP/Stripe and Mermaid parser/render checks remain open. Local snapshot/raw artifacts: ignored `Build/MaintainabilityBaseline-20261103/`. No deployment or new dependency was introduced. Earlier checkpoints below are historical.

# Project checkpoint

Saved: 28 October 2026 (Asia/Kuala_Lumpur).

Workspace: `D:/UnityProject/SubscriptionSystemUnity`

## Scope and decisions

Continue improving this existing project in place. Preserve its LogMasuk and LamanUtama scenes, artwork, UI, scene GUIDs and existing workflows. Do not replace it with a new game or scene architecture. The user requests an implementation Markdown first before starting a new substantial task, then explicitly approves execution.

## Completed work

- Refactored the existing Unity account/subscription flows and added shared SubscriptionSettings configuration.
- Rebuilt the missing PHP backend under `Backend/SubscriptionServer`, including authentication, email verification, database migrations, Stripe subscriptions, browser billing handoffs and maintenance workers.
- Integrated memory-only bearer sessions and authenticated billing links into the existing Unity client.
- Delivered the illustrated, reproducible 20-page PDF setup guide.
- Delivered the 45-second, 1080p/30 fps promotional trailer, original instrumental score, captions, poster and editable rendering source. The video uses genuine project screenshots with sample data and labeled illustrated billing/configuration flows.
- Provided YouTube title, description, thumbnail and audience suggestions in conversation. No YouTube fields were edited or video published by the assistant.

## Important files

| File/folder | Purpose |
| --- | --- |
| `README.md` | Project overview |
| `IMPLEMENTATION.md` | Original in-place refactor plan |
| `PHP_IMPLEMENTATION.md` | Approved PHP/PDF scope and completion notes |
| `VIDEO_TRAILER_IMPLEMENTATION.md` | Approved trailer scope and completion notes |
| `Backend/SubscriptionServer/README.md` | Backend installation, configuration and API reference |
| `Docs/SETUP.md` | Unity configuration and customization |
| `Docs/BACKEND_CONTRACT.md` | Authentication and subscription contract |
| `Docs/VERIFICATION.md` | Unity/backend validation record and limits |
| `Docs/SetupGuide/` | Editable PDF source and source images |
| `output/pdf/Subscription_System_Setup_Guide.pdf` | Consumer setup guide |
| `Docs/Trailer/` | Editable trailer timeline, renderer, credits and validation |
| `output/video/Subscription_Template_Trailer_45s.mp4` | Final trailer |
| `output/video/Subscription_Template_Trailer_45s.srt` | Caption transcript |
| `output/video/Subscription_Template_Poster.png` | YouTube thumbnail/poster |

## Last verified results

- Backend: 86 assertions passed in an isolated disposable MariaDB database; project PHP syntax checks passed. Composer audit reported no advisories at validation time.
- Unity: 24 offline data checks and 26 offline scene checks passed. Windows build succeeded with zero errors and warnings using Unity 2022.3.62f3.
- PDF: 20 pages rendered and visually inspected, text extraction and navigation checked.
- Trailer: 45.000 seconds, 1,350 frames, H.264/yuv420p, 1920×1080, constant 30 fps; AAC stereo at 48 kHz. Full-file decode passed. Measured −16.0 LUFS and −1.42 dBTP. Preview frames spanning every second visually inspected. No subjective listening certification is claimed.

These are the recorded validation results, not newly rerun checks during this checkpoint.

## Configuration and remaining owner setup

The shipped backend URL remains empty, and Game Identifier defaults to `Your Game`. Configure private server environment/catalog, HTTPS hosting, database, SMTP, Stripe test credentials/prices/Portal/webhook and scheduled workers using the included guide. Do not place server secrets in Unity.

Real hosting deployment, actual SMTP receipt, Stripe test transactions, mobile WebView/device behavior and live release have not been verified. Historical server data was not recovered. No live charges or customer migration occurred. The isolated validation database was stopped after testing.

## Latest YouTube draft

Title: **Unity Subscription System Template | PHP + Stripe | Official Trailer**

Use the delivered poster for the thumbnail. The trailer promotes developer tooling; the suggested audience choice was “No, it's not made for kids,” with no adult age restriction. Add a product/download link only when the owner supplies one. The latest screenshot showed a private upload draft; publication status after that screenshot is unknown.

## Save status and resuming

Project sources and deliverables exist on disk. Working-tree modifications and new files remain uncommitted; no reset, rollback or commit was performed for this checkpoint. Preserve pre-existing package/editor-version changes and all user edits.

Unity MCP reported `no_unity_session` while making this checkpoint. Therefore saving any currently unsaved in-editor scene/asset changes could not be confirmed. Reconnect/open this exact Unity project, inspect the active scene and save pending editor changes before further work. Do not assume a temporary MCP helper, encoder installation, validation build or other ignored Temp/Build content survives editor restarts; use the delivered source and setup instructions to recreate them as needed.

This older checkpoint is superseded by the UI revamp record at the top of this file.
## UI revamp checkpoint — 3 November 2026

User-authorized UI revamp implemented in the two existing scenes. Shared theme/button-face prefab and feedback, panel motion, reduced-motion preference, safe area, labelled navigation, inline notices and access summary are under Assets/UI. Existing account and billing ownership is preserved. Read Docs/tutorial/game-ui-customization.md for editing and verification.

Fresh validation: 24 offline data checks, 30 offline scene checks, Windows validation build succeeded with zero errors/warnings. Five viewport layouts rendered and inspected. Device/gamepad/live email and Stripe checks remain open. Scene validation reports only the intentionally empty backend URL. Older PDF/trailer captures remain historical. All earlier working-tree changes remain uncommitted and preserved. The one-time GameUIRevampTools.Apply migration is guarded; edit the scenes, prefab and theme directly for routine maintenance.

