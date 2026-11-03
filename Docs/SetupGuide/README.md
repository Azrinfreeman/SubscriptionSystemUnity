# Editable consumer guide

Final PDF: `../../output/pdf/Subscription_System_Setup_Guide.pdf`.

`guide.json` contains the chapter text, copyable commands, tables and links. `build_guide.py` creates the A4 PDF, bookmarks and clickable contents using ReportLab. `assets/login.png` and `assets/dashboard.png` capture the existing Unity scenes with sample data. Settings, hosting and Stripe visuals are labeled illustrations, not captures of customer accounts.

## Rebuild

Install Python 3.12+ with `pip install -r requirements.txt`, then run:

```text
python Docs/SetupGuide/build_guide.py
```

Run from the project root. The generator uses Segoe UI, Segoe UI Semibold and Consolas from C:/Windows/Fonts; adapt register() for other systems using licensed equivalent fonts. Fonts are embedded into the PDF. It stops if a chapter overflows its page. Keep commands and UI labels synchronized with the delivered server/client.

Render every page using Poppler (`pdftoppm -r 120 -png input.pdf preview`), inspect every page for layout/readability, and verify text extraction, page count, links and contents with pypdf. Rebuild and repeat after edits. No external accounts are required to rebuild the guide.

Version 1.0: 20 pages. Local validation is documented in ../VERIFICATION.md. Public hosting, real SMTP, actual Stripe account transactions and target-device behavior require owner verification.
