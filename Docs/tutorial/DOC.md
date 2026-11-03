# Tutorial presentation

FORMALITY: 4 | DENSITY: 5 | ORNAMENT: 1

Audience: the Unity/PHP developer maintaining the template. Markdown tutorials are authoritative. Lead with the owning asset or script, explain one practical change, then its expected result and verification. Link setup/backend details instead of duplicating them.

Signature element: every example names its edit destination and a check that proves its effect. Keep this consistent across menu and account tutorials.

HTML uses Segoe UI/system sans serif, Consolas/system monospace code, a 72-character reading measure, 1.5 line height, white paper and the menu's teal accent. PDF companions use verified local Georgia headings and Segoe UI body text at 11pt/1.45 on Letter pages with 0.8-inch side margins. Ink #203645, primary #205F75, field #EAF3F6 and rule #CAD9DF inherit the menu palette. Use real table headers, horizontal rules and visible keyboard focus. No cards, gradients, decorative numbering or repeated summary endings. Code must wrap legibly or scroll locally; tables must fit narrow screens.

The shared standard-library exporter handles the documented Markdown subset and fails on unsupported constructs. Regenerate both HTML companions whenever their Markdown or the exporter changes. Record executed checks separately from manual/device workflows.

## PDF production

Use `export_tutorial_pdfs.py` with the installed Playwright package and an installed Edge/Chrome browser. It reuses the Markdown parser, `tutorial-print.css` and `paginate-tutorial.js`, writes static `.print.html` companions, and exports PDFs beside the Markdown. No remote fonts or runtime libraries are used in the documents. The signature remains the source-and-check editing workflow; pagination and restrained teal headings support it rather than adding another decorative device.

The first page carries the opening action and linked contents, without a separate cover. Running headers identify the guide; footers identify the editable source and page count. Split tables repeat their headers, split steps retain their numbers, and headings stay with following content. Relative repository links remain useful in the HTML; the PDF shows their destinations as text because local checkout links are not portable. Internal contents links remain interactive.

Verification: inspect every rasterized PDF page, check text completeness and page bounds, run the existing parser tests and scan HTML for design defaults. Review the existing editing map separately against its brief. Historical runtime checks are cited as prior evidence, not rerun as part of PDF production.
