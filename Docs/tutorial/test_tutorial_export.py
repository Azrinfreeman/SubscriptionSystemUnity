"""Focused regression checks for tutorial structure and safe HTML escaping."""
import unittest
from export_game_ui_tutorial import inline, render_markdown


class TutorialExportChecks(unittest.TestCase):
    def test_local_illustration_preserves_caption_and_rejects_remote_source(self):
        _, body = render_markdown('# Example\n\n![Editing map](../diagrams/map.svg)')
        self.assertIn('<figcaption>Editing map</figcaption>', body)
        self.assertIn('src="../diagrams/map.svg"', body)
        with self.assertRaises(ValueError):
            render_markdown('# Example\n\n![Map](https://example.com/map.svg)')

    def test_fenced_code_keeps_literal_markup(self):
        _, body = render_markdown('# Example\n\n```csharp\nif (x < 2) { label = "<tag>"; }\n```')
        self.assertIn('class="language-csharp"', body)
        self.assertIn('x &lt; 2', body)
        self.assertIn('&lt;tag&gt;', body)
        self.assertNotIn('<tag>', body)

    def test_table_has_real_headers_and_rows(self):
        _, body = render_markdown('# Example\n\n| Source | Edit |\n| --- | --- |\n| `theme` | Color |')
        self.assertIn('<th scope="col">Source</th>', body)
        self.assertIn('<td><code>theme</code></td>', body)

    def test_steps_resume_after_code(self):
        _, body = render_markdown('# Example\n\n1. Add field\n\n```text\nfield\n```\n\n2. Assign it')
        self.assertIn('<ol start="2">', body)

    def test_companion_links_keep_fragments(self):
        self.assertIn('href="account-flow-customization.html#verify-and-export"',
                      inline('[Steps](account-flow-customization.md#verify-and-export)'))
        self.assertIn('href="../SETUP.md"', inline('[Setup](../SETUP.md)'))

    def test_code_is_not_parsed_as_a_link(self):
        self.assertEqual(inline('`[label](path)`'), '<code>[label](path)</code>')

    def test_bad_structure_fails_instead_of_flattening(self):
        for source in ['# Example\n```text\nunclosed', '# Example\n\n> quote',
                       '# Example\n\n<div>html</div>', '# Example\n## Same\n## Same',
                       '# Example\n\n| A | B |\n| --- | --- |\n| one |']:
            with self.subTest(source=source), self.assertRaises(ValueError):
                render_markdown(source)

    def test_executable_link_scheme_is_rejected(self):
        with self.assertRaises(ValueError):
            inline('[Bad](javascript:alert)')


if __name__ == '__main__':
    unittest.main()
