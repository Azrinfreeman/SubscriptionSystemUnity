# A game account flow, ready to customize

Production mode: autonomous, explicitly selected by the owner. Target: an exact 60-second, 1920 x 1080, 30 fps trailer for developers evaluating the Unity subscription template. The previous 45-second trailer remains historical.

## Product and execution decisions

The product is a starter system, not a playable game. Show real menus, registration, login, profile, verification, subscription details and the server-owned access boundary. The benefit is an existing account/subscription foundation with editable Unity scenes, prefabs and settings.

Use English editorial captions over the existing Malay interface. Explain the UI language and centered layout in the opening ten seconds. No narration: a musical bed and concise captions avoid an artificial voice. No invented prices, sales metrics, purchase links, customer identities or successful-payment claims. Sample UI state is fictional and labeled. Stripe checkout/webhooks are a labeled explanation, not provider footage.

| Requirement | Execution |
| --- | --- |
| Introduce purpose, use and benefit | Opening identifies a Unity account/subscription starter and the existing foundation it provides. |
| Describe design, language and layout | Show the actual storybook artwork, teal/gold controls, Malay labels and centered actions. |
| Show how to use it | Pointer-led registration, login, profile and subscription sequences reconstructed from fresh Unity renders. |
| Explain why Stripe access works | Authenticated ticket, hosted checkout, signed webhook, then fresh server state. Browser return alone never grants access. |
| Mention supporting features | Reduced motion, keyboard focus, device-conflict handling, editable theme and current PDF tutorials. |
| Exactly 60 seconds | 1,800 frames, constant 30 fps; one timeline controls scenes, captions and cues. |
| Human ownership | Remotion project, local captures, source manifest, credits, captions, render and verification scripts. |

## Visual direction

Keep the game's illustrated warmth, using #203645 ink, #205F75 primary teal, #F2BF50 gold, #EAF3F6 field and white surfaces. Cream only comes from actual game artwork; do not introduce the unrelated cream/terracotta trend. Use the UI's Liberation Sans for video text when available, otherwise its installed metric-compatible Arial face. No generic dashboard shells, pill labels, decorative counters, glass panels or invented UI. Reuse only authentic artwork and screenshot crops. Large titles are sentence case; payment explanation uses the same restrained palette.

Motion: friendly, precise, readable. 18-frame eased arrivals, actual panel-style 7-frame opens, 18-frame filmstrip steps, deliberate cursor paths and steady camera pushes. Holds take priority over kinetic-film's fast-cut defaults. No camera shake, repeated full-frame impacts or strobing. Full product views remain front-facing; opening/end screens may use slight perspective. Custom opening and explanatory diagram are authored for this product; exact recipe matching is not claimed for them.

Final text layout uses 48px body/auxiliary type and 78px captions, with two explicit caption lines in a 200px band. Encoded-frame probes check visible glyph heights rather than inferring them from CSS. Payment labels sit above the nodes, guide captions use short names, and the end card omits the redundant support line to leave clearance above the bottom caption band.

## Recipe mapping

Gallery index checked locally. Use `word-relay-filmstrip` / `word-relay-filmstrip` for supporting-feature evidence near the end: real screenshots, an 18-frame step, aligned caption center and no continuous conveyor motion. Reference: `demos/typography/word-relay-filmstrip/WordRelayFilmstrip.tsx`. Preserve step/entrance grammar; the five-second adaptation switches at local frames 8, 52 and 96 so the last phrase has more than half a second fully settled. Replace graphite/serif styling with this game's tokens, remove decorative numbering, and use readable product evidence. Other shots adapt PageCam's focus/scale mathematics into a focused owned component, with custom deterministic cursor/state transitions. `ui-strip-away-outro` was considered and rejected: clicking a fictional Publish action would misrepresent this template. `spotlight-hero-card` was rejected because theatrical darkening and a 2.6x zoom would obscure the centered menus.

## Storyboard and beat grid

Original instrumental score at 120 BPM. Beat length is 15 frames; 120 beats give exactly 60 seconds. Every scene begins on a beat. A source-generated score has an exact grid, so no guessed BPM or drift correction is needed. Each five-second section contains ten beats; the mechanism gets two connected sections.

| Time | Goal and real evidence | Main motion / caption |
| --- | --- | --- |
| 0-5 | Entry screen; identify product and benefit | One screen settles; accounts and subscriptions already connected in a Unity starter. |
| 5-10 | Design and language | Close framing on buttons and panel layout; Malay labels, centered actions, illustrated game feel. |
| 10-15 | Registration | Pointer selects Daftar akaun; the real registration panel opens. |
| 15-20 | Login | Pointer selects Log masuk; real login form hands off to labeled sample player menu. |
| 20-25 | Profile and verification | Profile opens; real read-only rows and resend control. |
| 25-30 | Subscription controls | Langganan saya / Langgan; real detail panel, no invented purchase result. |
| 30-35 | Begin payment explanation | Unity requests an authenticated PHP ticket; browser opens Stripe-hosted checkout. |
| 35-40 | Explain server-owned access | Signed webhook updates server state; Unity refreshes. Browser return is not payment proof. |
| 40-45 | Show subscription/access presentation | Full menu/detail views; status is derived from current server state. |
| 45-50 | Supporting functions | Filmstrip: reduced motion, keyboard focus, device-conflict recovery. |
| 50-55 | Maintain and customize | Theme settings and two current PDF covers; editable scenes and practical guides. |
| 55-60 | Product end card | Genuine entry/profile/subscription views converge around the product name; hold the final identity. |

## Capture and delivery

Capture current scenes at 3840 x 2160 with the existing offscreen renderer. Do not save capture-state changes or submit requests. Restore the original scene/selection. Record sample field values and button coordinates in `public/capture-manifest.json`. Screenshots are authentic, while pointer movements and state transitions are editorial reconstructions, not a continuous screen recording.

Deliver BGM and SFX-only MP4s, a lightweight preview, SRT transcript and poster under `output/video/`. No live service calls, deployment or publishing. Autonomous mode authorizes production through final render; the preview-first checks are performed internally. Independent final review must inspect decoded frames, the spec, source and technical results before delivery.
