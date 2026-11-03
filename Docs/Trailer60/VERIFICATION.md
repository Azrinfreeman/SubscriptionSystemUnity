# Trailer delivery checks

Recorded 3 November 2026. The current export replaces the earlier UI in a new 60-second cut; the historical 45-second files remain intact. Machine-readable results are in `verification.json`. This record covers video production, not new Unity/backend validation.

Independent final review, round 3: **PASS; no mandatory fixes remain.** The [review report](REVIEW.md) independently confirms encoded text sizes, end-card clearance, exact durations, matching picture and the corrected SFX-only version. It notes partly cropped return controls in some screenshot close-ups. Subjective listening and exact Gallery dynamic-reference fidelity remain unverified.

| Check | Result |
| --- | --- |
| Duration | Both masters: video, audio and container 60.000000 seconds; 1,800 decoded frames at 30/1 fps. |
| Format | 1920 x 1080, H.264 High/yuv420p, AAC stereo at 48 kHz, fast-start MP4. The intermediate Edge render's full-range flag is converted once before delivery. |
| Audio variants | With-BGM master and no-BGM master share compressed-picture SHA-256 `db6c6ff5f2f18f9fb408917d48d80caca9966c2e8722b3396408921c7bfc00a1`. |
| No-BGM content | The known cue-free 6–9 second interval has decoded mono RMS 0.0. The source sound cues remain present; this check supplements picture-hash comparison. |
| Full decode | Both final MP4s decode to completion with no FFmpeg errors. |
| Music-master loudness | -16.21 LUFS integrated, -1.20 dBTP, 3.20 LU loudness range, measured from the encoded delivery. |
| Action-sound alignment | Cross-correlation at cue frames 345, 645 and 795 reports zero-sample lag in the encoded no-BGM file. No measured correction is needed for those probes. |
| Browser playback | The 720p preview played from start to end in installed Edge, muted: duration/currentTime 60; 1,800 presented frames, zero dropped frames, no media or page error. Evidence: `out/qa/playback.json`. This is an automated playback check, not subjective listening. |
| Picture review | All four each-second encoded contact sheets across the full minute and representative full-resolution Remotion stills were inspected. The verifier extracts 26 exact action/cut/readability frames. The final cut enlarges body/auxiliary text, uses short two-line captions and removes the end-card support line. The final feature phrase retains its settled hold. |
| Encoded text height | All 12 shots have two measured caption lines, 58–73 visible pixels each (minimum 56). Ticket, checkout, refresh, theme filename and feedback-timing probes measure 35 pixels (minimum 32). Results and isolated full-resolution regions are recorded in `verification.json`; this measures sampled text, not every screenshot label. |
| End-card clearance | Actual encoded frames 1740 and 1799 are extracted for review. The redundant support line is removed; product identity, screenshot group and final caption have separate space above/inside the y=880 caption band. |
| Captions and poster | Matching 12-cue SRT derives from the same TypeScript timeline; a full-resolution poster and 720p preview are delivered. |
| Source checks | `npm run check` passes; Python production scripts compile. The design scanner reports no P0/P1 and one P2 metadata-separator advisory. The separators distinguish sample/illustration disclosures and real timing values; no decorative counters were introduced. |
| Capture safety | Fresh 4K offscreen renders use fictional profile/plan values. No login, registration, mail, billing-ticket or Stripe request was submitted. The original LogMasuk scene and selected settings asset were restored; capture changes were not saved. |

Earlier independent reviews found music in the incorrectly labeled no-BGM export, undersized text and an overlapping end-card support line. The audio correction uses a separately resolved and checked `bgm:false` composition, guarded by a silent-interval regression check. The final picture was rerendered after increasing essential labels, shortening/enlarging captions and removing the support line. The verifier now measures both caption lines in all 12 shots and the previously undersized labels. Earlier reports are preserved as `REVIEW-r1.md` and `REVIEW-r2.md`; the latest independent verdict is in [REVIEW.md](REVIEW.md).

## Reproduction and limits

Run the commands in [the editing guide](README.md). `scripts/verify.py` performs the media checks and regenerates the encoded review sheets. `scripts/check_playback.py` repeats the optional muted browser test with a local HTTP server on port 3031. Generated review media is under ignored `out/qa/`.

The ShotCraft workbench is linked at `http://localhost:5199/?import=project`. The final session reused the existing Vite process for this same project after checking its command line; the launcher declines to restart existing Windows processes automatically. The collaborative browser snapshot confirms imported shot, caption, music and individual SFX tracks and an updating main preview. Several library thumbnails displayed warning icons during that check. The generic player adds a one-second tail; external-workbench export parity is not certified. The owned Remotion command is the authoritative exact-duration export.

The movie reconstructs pointer interaction and state changes from genuine screenshots. It does not establish successful hosted login/mail/Stripe operations. The diagram intentionally summarizes the ticket, checkout, webhook and refresh boundary. Exact matching to an unavailable dynamic Gallery sample, subjective listening quality, and a complete manual editing walkthrough remain unverified. Small screenshot controls and PDF body text are supporting visual evidence; the large English captions carry the explanation. No video was published or sent externally.
