"""Export this feature tutorial through the existing owned Markdown renderer."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / 'Docs/tutorial'))
from export_game_ui_tutorial import export

if __name__ == '__main__':
    print(export(ROOT / 'Docs/tutorial/trailer-production.md'))
