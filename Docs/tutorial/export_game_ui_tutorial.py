"""Export the maintained tutorials to HTML using only Python's standard library.

Run from any directory: python Docs/tutorial/export_game_ui_tutorial.py
Markdown is authoritative; unsupported constructs fail instead of losing structure.
"""
from html import escape
from pathlib import Path
import re

TUTORIALS = ("game-ui-customization.md", "account-flow-customization.md")
DIRECTORY = Path(__file__).resolve().parent

# Presentation belongs here, never in the generated HTML companions.
STYLES = """
:root{color-scheme:light;--ink:#203645;--primary:#205f75;--field:#eaf3f6;--rule:#cad9df}
*{box-sizing:border-box}body{max-width:72ch;margin:48px auto;padding:0 24px;color:var(--ink);background:#fff;font:17px/1.5 'Segoe UI',system-ui,sans-serif;overflow-wrap:anywhere}
h1{font-size:2.1rem;line-height:1.15;margin:0 0 1em}h2{font-size:1.45rem;line-height:1.25;margin:2em 0 .7em}h3{font-size:1.12rem;margin:1.6em 0 .5em}
p{margin:0 0 1em}a{color:var(--primary);text-underline-offset:3px}a:focus-visible{outline:3px solid var(--primary);outline-offset:3px}
code{font-family:Consolas,'Courier New',monospace;font-size:.88em;overflow-wrap:anywhere}pre{padding:16px;background:var(--field);overflow-x:auto;line-height:1.5}pre code{white-space:pre-wrap;overflow-wrap:anywhere}
table{border-collapse:collapse;width:100%;font-size:.88em;margin:1.2em 0}th,td{padding:10px 8px;border-bottom:1px solid var(--rule);text-align:left;vertical-align:top;overflow-wrap:anywhere}th{font-weight:600}li{margin:0 0 .6em}ul,ol{padding-left:1.5em}footer{margin-top:3em;padding-top:1em;border-top:1px solid var(--rule);font-size:.85em}
figure{margin:1.5em 0}figure img{display:block;width:100%;height:auto}figcaption{font-size:.85em;margin-top:.8em}
@media(max-width:480px){body{margin:24px auto;padding:0 16px;font-size:16px}h1{font-size:1.8rem}th,td{padding:8px 5px}table{table-layout:fixed}}
@media print{body{margin:0;max-width:none;font-size:11pt}h2,h3{break-after:avoid}tr,pre{break-inside:avoid}a{color:var(--ink)}}
"""


def inline(text):
    """Escape source first; recognize only inline constructs we actually support."""
    tokens = re.compile(r"`([^`]+)`|\[([^\]]+)\]\(([^\s)]+)\)|\*\*([^*]+)\*\*")
    output = []
    offset = 0
    for match in tokens.finditer(text):
        output.append(escape(text[offset:match.start()]))
        code, label, target, strong = match.groups()
        if code is not None:
            output.append("<code>" + escape(code) + "</code>")
        elif label is not None:
            if re.match(r"[a-zA-Z][\w+.-]*:", target) and not target.startswith(("https://", "http://")):
                raise ValueError("Unsupported link scheme: " + target)
            # Only convert companions exported by this script, leaving repository links alone.
            path, separator, fragment = target.partition("#")
            if path in TUTORIALS:
                path = str(Path(path).with_suffix(".html"))
                target = path + (separator + fragment if separator else "")
            output.append('<a href="' + escape(target, quote=True) + '">' + escape(label) + '</a>')
        else:
            output.append("<strong>" + escape(strong) + "</strong>")
        offset = match.end()
    output.append(escape(text[offset:]))
    return "".join(output)


def heading_id(text):
    return re.sub(r"[^\w\- ]", "", text.lower()).replace(" ", "-")


def render_markdown(source):
    lines = source.splitlines()
    output, paragraph = [], []
    list_kind = None
    title = None
    headings = set()
    index = 0

    def flush_paragraph():
        if paragraph:
            output.append("<p>" + inline(" ".join(paragraph)) + "</p>")
            paragraph.clear()

    def close_list():
        nonlocal list_kind
        if list_kind:
            output.append(f"</{list_kind}>")
            list_kind = None

    while index < len(lines):
        line = lines[index]
        index += 1
        if not line.strip():
            flush_paragraph()
            close_list()
            continue
        if line.startswith("```"):
            flush_paragraph()
            close_list()
            language = line[3:]
            if not re.fullmatch(r"[a-zA-Z0-9_-]*", language) or language == "mermaid":
                raise ValueError("Unsupported code fence: " + line)
            code = []
            while index < len(lines) and lines[index] != "```":
                code.append(lines[index])
                index += 1
            if index == len(lines):
                raise ValueError("Unclosed code fence")
            index += 1
            attribute = f' class="language-{language}"' if language else ""
            output.append("<pre><code" + attribute + ">" + escape("\n".join(code)) + "</code></pre>")
            continue
        heading = re.fullmatch(r"(#{1,6}) (.+)", line)
        if heading:
            flush_paragraph()
            close_list()
            level, text = len(heading[1]), heading[2]
            anchor = heading_id(text)
            if anchor in headings:
                raise ValueError("Duplicate heading: " + text)
            headings.add(anchor)
            if level == 1:
                if title is not None:
                    raise ValueError("Use one document title")
                title = text
            output.append(f'<h{level} id="{anchor}">' + inline(text) + f'</h{level}>')
            continue
        if line.startswith("|"):
            flush_paragraph()
            close_list()
            rows = [line]
            while index < len(lines) and lines[index].startswith("|"):
                rows.append(lines[index])
                index += 1
            if len(rows) < 2 or not re.fullmatch(r"[| :\-]+", rows[1]):
                raise ValueError("Table needs a header separator")
            cells = lambda row: [cell.strip() for cell in row.strip().strip("|").split("|")]
            header = cells(rows[0])
            body = [cells(row) for row in rows[2:]]
            if any(len(row) != len(header) for row in body):
                raise ValueError("Table row has a different column count")
            output.append('<table><thead><tr>' + ''.join('<th scope="col">' + inline(cell) + '</th>' for cell in header) + '</tr></thead><tbody>')
            for row in body:
                output.append('<tr>' + ''.join('<td>' + inline(cell) + '</td>' for cell in row) + '</tr>')
            output.append('</tbody></table>')
            continue
        item = re.fullmatch(r"(?:(\d+)\. |(-) )(.+)", line)
        if item:
            flush_paragraph()
            kind = "ol" if item[1] else "ul"
            if list_kind != kind:
                close_list()
                start = f' start="{item[1]}"' if kind == "ol" and item[1] != "1" else ""
                output.append(f'<{kind}{start}>')
                list_kind = kind
            output.append('<li>' + inline(item[3]) + '</li>')
            continue
        illustration = re.fullmatch(r"!\[([^\]]+)\]\(([^\s)]+)\)", line)
        if illustration:
            flush_paragraph()
            close_list()
            caption, target = illustration.groups()
            if not re.fullmatch(r"[\w./-]+\.(?:svg|png|jpg)", target) or target.startswith('/'):
                raise ValueError("Use a relative local illustration: " + target)
            output.append('<figure><img src="' + escape(target, quote=True) + '" alt="' +
                          escape(caption, quote=True) + '"><figcaption>' + inline(caption) + '</figcaption></figure>')
            continue
        if re.match(r"\s|>|<|!\[|---$|\* ", line) or re.search(r"!\[|(?<!`)\[ \]", line):
            raise ValueError("Unsupported Markdown construct: " + line)
        close_list()
        paragraph.append(line)

    flush_paragraph()
    close_list()
    if title is None:
        raise ValueError("Document needs a title")
    return title, "\n".join(output)


def export(path):
    title, body = render_markdown(path.read_text(encoding="utf-8-sig"))
    html = ('<!doctype html><html lang="en"><head><meta charset="utf-8">'
            '<meta name="viewport" content="width=device-width,initial-scale=1">'
            '<title>' + escape(title) + '</title><style>' + STYLES + '</style></head><body>'
            '<main>' + body + '</main><footer><a href="' + escape(path.name) + '">Editable Markdown source</a>'
            '</footer></body></html>')
    path.with_suffix(".html").write_text(html, encoding="utf-8")
    return path.with_suffix(".html")


def main():
    for name in TUTORIALS:
        print(export(DIRECTORY / name))


if __name__ == "__main__":
    main()
