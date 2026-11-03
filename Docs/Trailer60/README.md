# Rebuild the 60-second trailer

The finished trailer uses the current Unity menus, English captions over the Malay UI, original instrumental music and an illustrated Stripe flow. It is exactly 1,800 frames at 30 fps. The account screens use labeled fictional data. Pointer movement and panel transitions are reconstructed from genuine 4K Unity renders; the film is not continuous gameplay or a live payment recording.

## Watch and edit

- [Final MP4 with music](../../output/video/Subscription_Template_Trailer_60s.mp4)
- [SFX-only MP4](../../output/video/Subscription_Template_Trailer_60s_NoBGM.mp4)
- [720p preview](../../output/video/Subscription_Template_Trailer_60s_Preview.mp4)
- [Caption transcript](../../output/video/Subscription_Template_Trailer_60s.srt)
- [Poster](../../output/video/Subscription_Template_60s_Poster.png)

These links are relative to this folder. The authoritative editing files are listed below. Generated media does not own the design.

| Source | Change here |
| --- | --- |
| `src/timeline.ts` | Titles, explanatory copy, captions, shot order and sound cue frames |
| `src/theme.ts` | Output dimensions, frame rate, beat grid and named colors/type sizes |
| `src/Trailer.tsx` | Product walkthroughs, screenshot camera, title composition and end card |
| `src/components/Stage.tsx` | Shared caption, cursor, screenshot frame and text styles |
| `src/components/PaymentFlow.tsx` | Ticket, checkout, webhook and state-refresh illustration |
| `src/components/Filmstrip.tsx` | Supporting-feature sequence and its tuned step/word timing |
| `scripts/compose_audio.py` | Authored score, instrumentation and action sounds |
| `scripts/capture-unity.cs.txt` | Editor capture tool source; does not submit account requests |
| `src/workbench.ts` | Visual-workbench shot/caption/media mapping, derived from the same timeline |
| `public/` | Actual screenshots, font/license and generated audio used by Remotion |

## Rebuild

Use an installed Node.js/npm, Python with numpy and Pillow, FFmpeg/FFprobe on PATH, and Edge or Chrome. The checked-in npm lockfile pins the aligned Remotion packages. The render uses a local browser and does not download one. PyMuPDF is needed only to refresh PDF page images.

From this folder:

```powershell
npm ci
node scripts/export-timeline.mjs
python scripts/compose_audio.py
npm run check
npm run stills
npm run studio
```

Studio opens at `http://localhost:3030`. Scrub each shot and inspect `out/qa/frame-*.png` before rendering. When satisfied, run:

```powershell
npm run render
python scripts/verify.py
```

`TRAILER_BROWSER` can point to another installed Chromium executable. The full render writes both audio versions, the preview and poster to `output/video/` at the repository root. Exported `timeline.json` and SRT derive from the TypeScript timeline. Music must be rebuilt after changing audio cues. Keep the 60-second/120 BPM grid unless intentionally producing a different cut; a changed duration or tempo also requires revising the music arrangement.

The verification script probes exact frame counts, dimensions and codec/audio format, decodes both files, compares their picture hashes, measures loudness and estimates action-sound timing from the encoded audio. It creates decoded contact sheets under `out/qa/`. Inspect those images after every picture change. SRT repeats the burned-in captions; leave the sidecar disabled during normal playback to avoid duplicate text.

Readable text uses `theme.body` and `theme.auxiliary` at 48px, with 78px captions. These are source sizes: the final pixels are what matter. The verifier measures isolated encoded ticket, checkout, refresh, theme and timing labels against a 32px visible-height minimum, plus both caption lines in all 12 shots against 56px. Inspect the other labels and 480px contact sheets manually after changing the font, wording or layout. The caption occupies y=880–1079; keep end-card content above it. The final card uses the product title and caption without a separate support line.

## Refresh the Unity screenshots

Save your work, stop Play mode and open only one saved scene. Copy `scripts/capture-unity.cs.txt` to `Assets/Editor/SubscriptionTrailerCapture.cs`. After compilation, run **Tools > Subscription System > Capture 60-second trailer screens**. It uses the existing `GameUIRevampTools.Capture` helper, opens the two original scenes, fills only fictional display values, writes 4K screenshots and restores the original scene and selected asset without saving capture changes.

This command does not sign in, send verification mail, create a billing ticket or call Stripe. Remove the temporary Editor script when no longer needed. The ordinary menu wrapper is supplied for manual use; the capture body was executed through the connected Editor during production. The copy-into-Editor menu workflow itself was not separately exercised.

After capture or a tutorial-PDF update, run `python scripts/prepare_assets.py`. It copies the owned font/license, rasterizes page one of the current tutorial PDFs and refreshes `public/capture-manifest.json` with dimensions, hashes and logical button coordinates. This optional preparation step needs PyMuPDF and Pillow. Keep Unity UI textures at 3840 x 2160 so close framing remains readable.

## Make a routine change

To replace the opening title, edit the `intro` entry's `title` in `src/timeline.ts`. Use short explicit lines separated by `\n`, then run `npm run stills`. Inspect `out/qa/frame-60.png`: the complete headline should fit left of the entry-screen image, with the explanatory copy below it. Run `npm run render` and the verification script to rebuild all deliverables. For a color change, edit the semantic values in `src/theme.ts`; changing the trailer palette does not change the Unity theme asset.

The visual ShotCraft workbench is available locally at `http://localhost:5199/?import=project` in this production session. It imports shot, caption, music and SFX tracks with editable text/copy fields. Its generic player adds a one-second tail to the imported project; use the owned Remotion render command above as the authority for the exact 60-second delivery. Full external-workbench export parity has not been certified. The simpler Remotion Studio workflow remains independent of that optional tool.

## Evidence and limits

See [verification](VERIFICATION.md), [independent review](REVIEW.md), [creative decisions](DESIGN.md) and [asset sources](ASSET_CREDITS.md). The film does not certify hosted SMTP delivery, Stripe test purchases, physical-device input or a published release. Narration is omitted. Audio measurements do not replace a human listening pass on the destination speakers.

Optional sharing details: credit for the shot skill is voluntary, using [X](https://x.com/VincentWei93), [Douyin](https://www.douyin.com/user/MS4wLjABAAAAK1pkjBxilk2Oi_9h_vFyD-lTAu9CTlvhmOtkosDvvxg) or [Xiaohongshu](https://xhslink.cn/m/At9iP2d5C1V). You can submit the film to the [showcase](https://vincentwei1021.github.io/video-shotcraft/showcase.html) using [the submission form](https://github.com/Vincentwei1021/video-shotcraft/issues/new?template=showcase.yml). No video was published or sent to anyone. A CapCut/Jianying editing project can be exported separately if requested.
