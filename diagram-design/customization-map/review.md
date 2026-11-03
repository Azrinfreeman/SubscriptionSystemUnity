# Editing map review

Reviewed 3 November 2026 against `brief.md`, the original Mermaid and the repository paths in the account tutorial.

The original Mermaid passes source lint (five nodes, four edges). Its wide four-column arrangement makes labels too small at 600 px. The first browser-generated SVG also contained foreignObject nodes; that export is rejected as a deliverable. The original source is preserved. The editable revision is `Docs/diagrams/customization-map-r1.svg`, which uses only static SVG text, paths and rectangles.

## Analyze the revision

The 1200 px and 600 px renders show five primary boxes and four destination arrows. One shared branch line connects the question to the destinations. The map tells the reader where to edit a particular kind of change.

Visible text, transcribed from the render:

1. Choose the source that owns the change
2. What are you / changing?
3. Configure backend / and game key
4. SubscriptionSettings.asset
5. Tune colors and / motion duration
6. DefaultGameUITheme.asset
7. Edit layout and labels
8. Scenes and / MenuButtonFace.prefab
9. Change validated / application rules
10. Account scripts and / PHP source

The four destinations and their directions match the brief. No service, dependency or measured quantity was added. The question has a teal field and white text; labels and lines use the existing dark ink on white.

## Verdict: PASS

Static SVG lint: zero errors, one advisory warning. The lint counts dark ink and teal as two accent hues; dark ink is the documented text/line neutral, while teal marks only the question. White on teal is 7.11:1 and ink on white is 12.54:1. Both delivery and half-width renders keep labels legible, with no clipping or crossing lines. The SVG includes title and description metadata, uses one font family, and preserves the original source-and-editing purpose.

The same revision is embedded as Figure 1 in the account PDF. PDF pages were rasterized and inspected separately; the figure remains readable at its printed width. Review renders and scripts are local verification artifacts under `Build/DocumentationReview/`.
