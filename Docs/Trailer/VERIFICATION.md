# Trailer verification

Final output: `output/video/Subscription_Template_Trailer_45s.mp4` at the project root.

| Check | Result |
| --- | --- |
| Video duration | 45.000 seconds, 1,350 decoded frames |
| Video format | 1920×1080, constant 30 fps, H.264, yuv420p |
| Audio format | AAC stereo, 48 kHz |
| Integrated loudness | −16.0 LUFS measured from the encoded audio |
| True peak | −1.42 dBTP measured from the encoded audio |
| Complete-file decode | Passed with no video/audio decode errors |
| Visual review | Nine storyboard previews and 45 decoded frames covering every second inspected; a full-resolution encoded frame also checked for clarity |
| Captions and poster | SRT sidecar and 1080p poster delivered; main messages also burned into the video |
| File size | 3,563,386 bytes |

Review frames and encoder logs are in ignored Temp/TrailerProduction. Machine-readable results are in validation.json. Re-run `python Docs/Trailer/verify_trailer.py` to decode the complete file, measure encoded audio, extract exact half-second-offset review frames and regenerate the report. Inspect the resulting contact sheets after each edit.

The video is a motion-graphics teaser from genuine project screenshots, with simulated data and illustrated billing/configuration segments identified on screen. It does not show a real Stripe transaction, a live deployment or recorded gameplay. The audio is an original procedural composition; source and provenance are included. Audio levels were measured computationally; subjective listening quality is not certified by those measurements.

No Unity scripts, scenes, settings or customer accounts were changed for production. Unity MCP was used only to read connected-editor readiness. No publishing, external sharing or live payment actions were performed.
