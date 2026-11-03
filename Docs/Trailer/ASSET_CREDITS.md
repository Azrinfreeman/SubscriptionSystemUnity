# Asset and audio provenance

- Login and dashboard images: this Unity project's existing LogMasuk and LamanUtama scenes, captured during its offline validation with sample data. Source images: ../SetupGuide/assets/login.png and dashboard.png. Existing project artwork retains its original ownership/licensing; this trailer does not establish new rights to redistribute third-party project assets.
- PDF previews: pages 1, 2 and 15 of the delivered Subscription System Setup Guide, rendered with Poppler. Included under assets/ for reproducibility.
- Titles, background, diagrams and settings illustration: created specifically for this trailer using the included Python/Pillow renderer. The settings example and billing diagram are labeled illustrations. No Stripe logo, stock footage or fabricated checkout screen is used.
- Music: **Connected / Original Procedural Score**, newly composed for this trailer in render_trailer.py using sine-based instruments, seeded synthetic percussion and an original arrangement at 110 BPM. No recordings, samples, commercial songs or downloaded music are used. The score can be recreated from the source; no third-party music attribution or purchased music license is needed.
- Typography: Segoe UI / Segoe UI Semibold supplied by Windows. Fonts are rasterized in the video and are not redistributed as font files. Use fonts licensed for your own production environment when rebuilding.
- Encoder: FFmpeg 7.1 supplied through imageio-ffmpeg 0.6.0, installed in ignored Temp/TrailerTools. It is a production tool, not bundled with the trailer/source deliverables. Refer to its distribution license if distributing the encoder separately.

All account data visible in the trailer is sample data. The billing sequence illustrates the implemented integration; no live purchase or external account dashboard was recorded.
