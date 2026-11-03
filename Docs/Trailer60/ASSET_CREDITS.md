# Asset sources

The trailer contains current project UI captures, its existing artwork, the current tutorial pages and original audio. There is no generated product UI, stock footage, sampled commercial track or voice impersonation.

| Asset | Source and handling |
| --- | --- |
| Unity menu/panel textures | Fresh 3840 x 2160 offscreen captures of `Assets/Scenes/LogMasuk.unity` and `LamanUtama.unity`. `scripts/capture-unity.cs.txt` records the capture method. No account requests were submitted. |
| Illustrated backgrounds | Captured from the same scenes with functional UI hidden temporarily. Original artwork remains in the Unity project. No separate editable illustration originals are claimed. |
| Account/plan text | Fictional sample data: Pemain Demo, player@example.com, 0123456789, Pelan demo, Contoh, Belum aktif. Samples are labeled. No customer records, passwords, session tokens or tickets appear. |
| PDF images | First pages of `Docs/tutorial/game-ui-customization.pdf` and `account-flow-customization.pdf`, rendered locally with PyMuPDF. |
| Typeface | The project's Liberation Sans, distributed under the SIL Open Font License. Font and license are under `public/fonts/`. |
| Camera math | Adapted from video-shotcraft's PageCam: center a page-space point in a viewport with `center - focus * scale`. The finished component is owned source in `src/Trailer.tsx`. |
| Feature filmstrip | Adapted motion grammar from `word-relay-filmstrip` and its exact TSX reference: 18-frame steps and delayed text entrances. Real UI replaces the demonstration's placeholder pages. |
| Music | Original deterministic 120 BPM composition in `scripts/compose_audio.py`. Sine-based instruments, seeded noise percussion and authored harmony/melody; no third-party music samples. |
| Action sounds | Original seeded noise/thump synthesis in the same script, driven by the timing cues in `src/timeline.ts`. These are editorial sounds, not sounds claimed to ship in the app. |

The Stripe section is an explanation of the implemented integration. It does not use a Stripe screenshot, brand logo, live checkout or claim a completed purchase. References to Unity and Stripe name the integrations, not an endorsement.
