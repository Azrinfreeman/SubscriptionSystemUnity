# Customize and rebuild the trailer

Edit `Docs/Trailer60/src/timeline.ts` to change the 60-second trailer's copy, and use Remotion Studio to preview the result. The video is a separate authored artifact; changing it does not change the Unity account or billing system.

## Open the editing source

The [trailer guide](../Trailer60/README.md) lists the source files, prerequisites and rebuild commands. `src/Trailer.tsx` owns the screen compositions, `src/components/` owns the cursor/diagram/shared presentation, and `src/theme.ts` owns the palette and output settings. Genuine screenshots and generated audio live in `public/`. MP4s, poster, SRT and generated HTML are outputs.

## Change the opening title

1. Open `Docs/Trailer60/src/timeline.ts` and find the entry with `id:'intro'`.
2. Replace its `title` with a short product-specific headline. For example, `title:'Player accounts.\nStripe subscriptions.\nYour game.'` uses three explicit lines.
3. From `Docs/Trailer60`, run `npm run check` and `npm run stills`. Inspect `out/qa/frame-60.png` for wrapping and overlap. Keep all three lines visible beside the screenshot.
4. Run `npm run studio` and scrub the introduction at `http://localhost:3030`. The title should settle while the entry menu remains readable.
5. Run `npm run render`, then `python scripts/verify.py`. Inspect the decoded contact sheets. Both delivery versions should remain 1,800 frames at 30 fps and show the new title.

This is a representative edit, not a claim that every replacement headline will fit. Long product names need a smaller title size or fewer words. Preserve the actual Malay UI labels in screenshots unless you also update and recapture the application.

## Keep the payment explanation accurate

The illustrated flow names the Unity menu, PHP backend and Stripe-hosted checkout. A signed webhook updates server state, and Unity refreshes it. Preserve the distinction between a browser return and proof of payment. Label fictional screen values and provider illustrations. Never insert real credentials, tickets or customer details into captures.

The [account guide](account-flow-customization.md) explains the implemented boundaries; it remains the source for application customization. The trailer's text and diagram explain those boundaries but do not implement them.

## Verify and export

The [verification record](../Trailer60/VERIFICATION.md) identifies completed technical and visual checks and remaining limits. The original movie contains music and SFX; the companion removes BGM while retaining identical picture and action sounds. There is no voice-over.

To rebuild this readable HTML companion from the root, run:

```text
python Docs/Trailer60/scripts/export_tutorial.py
```

The tutorial's Markdown is authoritative. The supplied capture tool also supports a normal Unity Editor menu; its manual installation instructions are in the trailer guide. A full external-workbench editing/export walkthrough is not certified, so use the documented Remotion command for the exact-duration master.
