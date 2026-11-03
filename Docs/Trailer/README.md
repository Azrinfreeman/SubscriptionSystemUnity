# Your Game. Connected. — 45-second trailer

The finished MP4, caption transcript and poster are under `../../output/video/`.

This is a motion-graphics teaser made from genuine screenshots of the original Unity scenes, PDF previews, animated titles and diagrams. It is not a recording of a live account/payment session. Sample-data screens, the illustrated billing flow and configuration example are labeled in the video. The original Unity scenes, runtime scripts and configuration are not changed by the renderer.

## Rebuild

Use Python 3.12+ and install `pip install -r Docs/Trailer/requirements.txt`. Then, from the project root:

```text
python Docs/Trailer/render_trailer.py --previews
python Docs/Trailer/render_trailer.py
```

The first command creates preview frames only. The second renders the 1,350-frame video, composes the original stereo score, normalizes audio and writes the final files. Intermediate media and logs go under ignored `Temp/TrailerProduction`. The final output is 1920×1080, constant 30 fps, H.264/yuv420p with 48 kHz AAC stereo. FFmpeg is selected through imageio-ffmpeg; set FFMPEG_BINARY to use your own installed encoder. The renderer also recognizes the task-local installation in Temp/TrailerTools.

`timeline.json` controls shot timing, text and ordering. `render_trailer.py` controls layouts, motion and the original music composition. `ASSET_CREDITS.md` documents provenance. Windows fonts default to C:/Windows/Fonts; override TRAILER_FONT_DIR with a folder containing licensed segoeui.ttf and seguisb.ttf alternatives. No font binaries or FFmpeg executable are included in the source delivery.

The SRT is a sidecar transcript of the burned-in main messages. Keep it disabled during normal playback if you do not want duplicate visible captions.

## Source assets

Screenshots are reused from Docs/SetupGuide/assets. PDF previews are included in this folder's assets directory. To update those previews, render pages 1, 2 and 15 of the final setup PDF with Poppler at approximately 1,100 pixels tall, named guide-cover.png, guide-route.png and guide-unity.png.

After edits, inspect every shot, transitions and safe margins, decode the final video to check for encoding errors, verify its duration/frame count/audio format, and inspect loudness. See VERIFICATION.md for the actual delivered checks and limits.
