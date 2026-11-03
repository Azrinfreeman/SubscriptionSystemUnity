"""Rebuild both PDF companions from Markdown, preserving the screen HTML exporter.

Requires Python Playwright and a local Edge/Chrome installation. No browser download
is performed. Use --browser PATH for another Chromium executable. See DOC.md for design.
"""
import argparse
import base64
from html import escape
from pathlib import Path
import re

from export_game_ui_tutorial import DIRECTORY, TUTORIALS, render_markdown


def find_browser(override):
    if override:
        candidates = [Path(override)]
    else:
        candidates = [Path('C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'),
                      Path('C:/Program Files/Google/Chrome/Application/chrome.exe')]
    for candidate in candidates:
        if candidate.is_file():
            return str(candidate)
    raise SystemExit('Install Edge/Chrome or pass --browser /path/to/chromium.')


def pdf_links(body):
    """Repository references must remain readable when the PDF leaves this checkout."""
    def replace(match):
        target, label = match.groups()
        if target.startswith(('#', 'https://', 'http://')):
            return match.group(0)
        return label + ' <span class="reference">(<code>' + target + '</code>)</span>'
    return re.sub(r'<a href="([^"]+)">(.*?)</a>', replace, body)


def embed_images(body):
    """Keep print HTML self-contained, using only checked repository images."""
    def replace(match):
        path = (DIRECTORY / match[1]).resolve()
        if not path.is_relative_to(DIRECTORY.parent) or not path.is_file():
            raise ValueError('Missing or out-of-scope illustration: ' + match[1])
        mime = {'.svg': 'image/svg+xml', '.png': 'image/png', '.jpg': 'image/jpeg'}[path.suffix]
        return 'src="data:' + mime + ';base64,' + base64.b64encode(path.read_bytes()).decode('ascii') + '"'
    return re.sub(r'src="([^"]+)"', replace, body)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--browser', help='Path to an installed Chromium browser')
    args = parser.parse_args()
    from playwright.sync_api import sync_playwright
    css = (DIRECTORY / 'tutorial-print.css').read_text(encoding='utf-8')
    script = (DIRECTORY / 'paginate-tutorial.js').read_text(encoding='utf-8')
    with sync_playwright() as runtime:
        browser = runtime.chromium.launch(executable_path=find_browser(args.browser), headless=True)
        try:
            for name in TUTORIALS:
                source = DIRECTORY / name
                title, body = render_markdown(source.read_text(encoding='utf-8-sig'))
                page = browser.new_page(viewport={'width': 900, 'height': 1200})
                page.emulate_media(media='print')
                page.set_content('<!doctype html><html lang="en"><head><meta charset="utf-8">'
                                 '<title>' + escape(title) + '</title><style>' + css +
                                 '</style></head><body><div id="source">' + embed_images(pdf_links(body)) +
                                 '</div></body></html>')
                page.locator('img').evaluate_all('(images) => Promise.all(images.map(image => image.decode()))')
                original_text = page.locator('#source').inner_text()
                page.add_script_tag(content=script)
                count = page.evaluate('([title, name]) => paginateTutorial(title, name)', [title, name])
                # Completeness ignores repeated table headers, which are intentional.
                final_text = page.locator('main').all_inner_texts()
                normalize = lambda text: re.sub(r'\s+', ' ', text).strip()
                actual = normalize(' '.join(final_text))
                for paragraph in re.split(r'\n\s*\n', original_text):
                    if normalize(paragraph) not in actual:
                        # Tables gain repeated headers between rows; check individual lines.
                        for line in paragraph.splitlines():
                            if normalize(line) not in actual:
                                raise RuntimeError('Lost content: ' + line)
                overflow = page.locator('.content').evaluate_all(
                    '(nodes) => nodes.some(n => n.scrollHeight > n.clientHeight + 1 || n.scrollWidth > n.clientWidth + 1)')
                if overflow:
                    raise RuntimeError('Page overflow in ' + name)
                print_html = source.with_suffix('.print.html')
                print_html.write_text(page.content(), encoding='utf-8')
                pdf = source.with_suffix('.pdf')
                page.pdf(path=str(pdf), prefer_css_page_size=True, print_background=True,
                         tagged=True, outline=True)
                page.close()
                print(f'{pdf}: {count} pages; complete text and no page overflow')
        finally:
            browser.close()


if __name__ == '__main__':
    main()
