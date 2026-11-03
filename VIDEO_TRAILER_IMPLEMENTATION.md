# 45-Second Project Trailer — Implementation Plan

Status: Implemented following approval. Final trailer, captions and poster are under `output/video/`. Editable source, credits and verification are under `Docs/Trailer/`. The plan below records the approved scope; the delivered edit uses genuine project screenshots with motion graphics and labeled illustrative flows.

## 1. Purpose and creative direction

Create a polished teaser for the existing **Unity Subscription System Template**, aimed at developers and template purchasers. Present the current project as a reusable foundation for account and subscription flows, using its original LogMasuk and LamanUtama scenes and existing artwork.

This is a product trailer for the delivered template. Its footage will show the features actually present in this project. No new gameplay, replacement scenes or invented game mechanics are needed.

Working title: **Your Game. Connected.**

Style: upbeat, welcoming and clear. Use the project's playful visual identity, orange accents and rounded interface elements, with restrained motion graphics around the captured screens. Keep the original Malay interface text readable; add short English captions to explain it to purchasers.

Default audience language: English. Default format: landscape, suitable for a product listing, YouTube or a presentation. These choices can be revised before production if you prefer Malay or a vertical social-media version.

## 2. Final deliverables

| File | Purpose |
| --- | --- |
| `output/video/Subscription_Template_Trailer_45s.mp4` | Main trailer: 1920 × 1080, 30 fps, H.264 video, AAC audio, exactly 45 seconds |
| `output/video/Subscription_Template_Trailer_45s.srt` | Timed on-screen message/caption transcript |
| `output/video/Subscription_Template_Poster.png` | Cover image from the trailer |
| `Docs/Trailer/` | Editable storyboard, timing manifest, render instructions and asset/audio credits |

Provide a reproducible edit using local video tools where available. If FFmpeg is used, retain the timing manifest, title assets and render script rather than delivering only an exported MP4. Temporary recordings and intermediate renders belong in an ignored workspace folder; include only the material needed to reproduce the finished edit in the delivery folder.

The existing PDF setup guide is featured in the video; it remains the detailed setup resource.

## 3. Exact 45-second storyboard

Times below are positions on the final timeline. Transitions fit inside these intervals so they do not increase the total duration.

| Time | Visual and motion | Main on-screen message |
| --- | --- | --- |
| 00:00–00:04 | Open on the original project artwork, then reveal the existing entry screen with a gentle push-in. Establish the working title. | **Your Game. Connected.** |
| 00:04–00:09 | Show the existing SIGNUP and LOGIN entry points. Cut to a short registration/login interaction using sample credentials. Avoid lingering on form typing. | **Accounts, built into your game.** |
| 00:09–00:14 | Show a verified sample profile, then transition into the original dashboard greeting and account controls. Use a subtle cursor highlight to guide attention. | **One place for your players.** |
| 00:14–00:20 | Highlight LANGGAN / Subscribe. Show the delivered plan-selection page and a brief verified Stripe test-mode Checkout capture if available. Otherwise use the actual local billing page with a labeled billing-flow diagram. | **Connect subscriptions with Stripe.** |
| 00:20–00:25 | Open the original subscription panel. Highlight the plan, status and access-end fields. Include management footage only if it can be captured in a working test environment. | **Keep players in control.** |
| 00:25–00:31 | Show the shared SubscriptionSettings asset. Highlight Backend Base URL and Game Identifier, then briefly show a private-example game catalog with no credentials. | **Configure it for your project.** |
| 00:31–00:37 | Animate a small Unity → PHP → Stripe diagram over genuine project imagery, then reveal the included backend folder. Keep code and filenames secondary to the message. | **Unity client. PHP backend.** |
| 00:37–00:41 | Show the setup PDF cover and two readable page previews: setup route and Unity configuration. | **Follow the visual setup guide.** |
| 00:41–00:45 | End card using the same visual identity. Hold the product name and final call to action long enough to read. Fade audio and image together. | **Unity Subscription System Template** / **Make it yours.** |

Only one main message appears at a time. Preserve breathing room around the screens and avoid covering buttons, status fields or form labels with captions.

## 4. Footage and demonstration rules

- Record the existing Unity project through its connected editor or a fresh validation build. Preserve original scene files, layout and artwork.
- Use sample accounts and a disposable development setup. Never show passwords, session tokens, verification links, billing tickets, secret keys, customer details or private server paths.
- Use existing editor-only fixtures where appropriate for dashboard/profile shots. Mark simulated account or subscription state with a small readable **Sample data** label.
- Stripe-hosted pages require an accessible authorized test environment. Label genuine test-mode footage **Stripe test mode**. Do not imply a live purchase or published deployment.
- If actual hosted billing footage cannot be captured, use the delivered local billing UI and an explicitly labeled **Illustrated billing flow**. Do not fabricate a provider screen or a payment-success claim.
- Show a configured example URL such as `https://example.com/subscriptions/` in settings. Keep the shipped settings asset unchanged after capture.
- Use screenshots of the delivered PDF directly. Capture at sufficient resolution so its headings remain readable in the final composition.
- Do not add a website address, purchase link, price, company logo, testimonial or availability claim unless supplied or already authorized by the user. The default end card needs no external URL.

## 5. Audio and captions

Default: instrumental background music, light transition accents and on-screen messages; no voice-over. This keeps the existing UI and product purpose easy to follow without requiring a recorded narrator.

Use original or suitably licensed music whose terms permit promotional distribution. Prefer a subtle upbeat electronic track around 105–120 BPM. Record the source, author and license in `Docs/Trailer/ASSET_CREDITS.md`. Do not use a commercial song or assume an existing project sound is licensed for advertising.

If suitable licensed music is unavailable, compose a simple original instrumental bed locally rather than downloading unlicensed material. Confirm usable audio/render tools during production before choosing that method.

Keep effects quiet, avoid abrupt loudness changes, and fade out over the last second. Aim for approximately −16 LUFS integrated with a true peak no higher than −1 dBTP, then verify by listening. Burn in the main messages and export matching SRT timings. Optional narration or an alternate language version is outside the default cut and can be planned separately.

## 6. Production workflow after approval

1. **Verify capture and editing tools.** Inspect the connected Unity editor, available video capture options and local encoding tools. Select a practical capture/render workflow before recording. This plan does not assume a dedicated AI video-generation service is connected.
2. **Prepare a shot list.** Convert the storyboard into a machine-readable timing manifest. Identify which clips use real interactions, sample state, PDF pages or vector motion graphics.
3. **Prepare sample state.** Use a disposable account/backend or the existing editor-only fixture. Record and restore settings, selection, scene and account preferences affected by capture. Do not modify the project's production behavior for the trailer.
4. **Capture source material.** Record at 1080p or better where possible. Capture a few seconds of handles around each action for editing. Prefer actual moving UI footage; use deliberate slow pans/zooms for static settings and documentation shots.
5. **Create title and diagram assets.** Match the project's palette and use licensed fonts with readable contrast. Keep explanatory graphics separate from the Unity scene files.
6. **Assemble the first cut.** Place shots, captions and restrained transitions on the exact 45-second timeline. Avoid fast flashes, excessive zooms and cuts that make the forms unreadable.
7. **Add and mix audio.** Align the main cuts with the music where practical, preserve caption reading time and check the ending fade.
8. **Review the complete cut.** Watch from beginning to end, inspect each transition and proofread every caption. Confirm all visible product claims match the implementation.
9. **Render and verify.** Export MP4, captions and poster, then validate the encoded file as described below. Keep the editable source and rendering instructions together.

If the available tools support only static capture, report that limitation and build a motion-graphics teaser from genuine project screenshots and document previews. Do not describe it as recorded interactive footage. Preserve the 45-second duration and the same factual storyboard.

## 7. Technical export and quality checks

| Check | Acceptance criterion |
| --- | --- |
| Duration | 1,350 video frames at constant 30 fps; 45.000-second target. Audio is trimmed/padded to the same timeline; allow only codec/container rounding in metadata. |
| Playback | MP4 plays fully with no missing media, black gaps, corrupt frames or interrupted audio. |
| Compatibility | H.264, yuv420p, 1920 × 1080, AAC stereo at 48 kHz, fast-start metadata for web playback. |
| Image quality | Original aspect ratio retained; no stretched UI, clipped captions or unreadable key labels. |
| Caption placement | Keep essential captions inside roughly 5% safe margins. Use readable type, high contrast and sufficient hold time. |
| Fidelity | Original scenes/UI remain recognizable. Mock/sample state and illustrative billing segments are labeled accurately. |
| Privacy | Inspect frames for credentials, account data, private desktop content and browser address-bar tickets. |
| Audio | No clipping, unintended silence, abrupt cuts or distracting volume jumps. Licensing/source recorded. |
| Review | Watch the final encoded file, inspect contact sheets across the full timeline and verify metadata with a media probe. |
| Reproduction | Timing manifest, caption source, credits and render instructions correspond to the delivered export. |

## 8. Scope boundaries

This task produces a promotional trailer and its editing source. It does not redesign the application, add gameplay, deploy the backend, configure a live Stripe account, buy stock media, publish to social platforms or send the video to others.

Existing user changes and historical builds remain intact. Any temporary capture state must be restored. A vertical/social cut, translated edition, custom logo or narrated version can be added later if requested.

## 9. Completion criteria

The work is complete when the 45-second MP4, caption file, poster and reproducible source are delivered; the entire exported video has been reviewed; audio provenance is documented; visible sample/test state is labeled; and the original Unity project still opens with its existing scenes and configuration preserved.

Implementation completed. The exported file is exactly 45 seconds at 1080p/30 fps with original instrumental audio. See `Docs/Trailer/VERIFICATION.md` for actual checks and limitations.

## Current 60-second cut

The new owner-authorized cut uses the redesigned menus and a Remotion project under `Docs/Trailer60/`. Its 12-shot timeline explains purpose, Malay UI/layout, account actions, Stripe handoff/webhooks, server-owned access, supporting controls and customization. The older plan and 45-second output above remain historical.

Current outputs are `output/video/Subscription_Template_Trailer_60s.mp4` and its `NoBGM`/`Preview` companions, with SRT and poster. Start with [the editing guide](Docs/Trailer60/README.md), [production tutorial](Docs/tutorial/trailer-production.md) and [current evidence](Docs/Trailer60/VERIFICATION.md). Delivery is complete: both versions are exactly 60 seconds, decoding/readability/audio-variant checks and automated muted playback pass, and [independent final review](Docs/Trailer60/REVIEW.md) passes with no mandatory fixes. The report records screenshot-cropping advisories and unverified subjective listening/dynamic-reference fidelity.
