"""Copy the owned font, refresh tutorial page textures and hash source captures.

Run after the Unity capture command and after regenerating the tutorial PDFs.
Requires Pillow and PyMuPDF. No network, login or payment service is used.
"""
from pathlib import Path
import hashlib
import json
import shutil
import pymupdf
from PIL import Image

HERE = Path(__file__).resolve().parents[1]
ROOT = HERE.parents[1]


def main():
    fonts = HERE / 'public/fonts'
    screens = HERE / 'public/screens'
    fonts.mkdir(parents=True, exist_ok=True)
    screens.mkdir(parents=True, exist_ok=True)
    shutil.copy2(ROOT / 'Assets/TextMesh Pro/Fonts/LiberationSans.ttf', fonts / 'LiberationSans.ttf')
    shutil.copy2(ROOT / 'Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt', fonts / 'LICENSE.txt')
    for name in ['game-ui-customization', 'account-flow-customization']:
        with pymupdf.open(ROOT / f'Docs/tutorial/{name}.pdf') as document:
            document[0].get_pixmap(matrix=pymupdf.Matrix(1.8, 1.8)).save(screens / f'{name}.png')
    files = {}
    for path in screens.glob('*.png'):
        with Image.open(path) as image:
            files[path.name] = {'sha256': hashlib.sha256(path.read_bytes()).hexdigest(), 'size': list(image.size)}
    manifest = {'unityCaptureSize': [3840, 2160], 'logicalSize': [1920, 1080], 'sampleData': True,
                'liveRequests': False, 'method': 'Unity snapshots with reconstructed pointer/state animation',
                'files': files, 'buttonCenters': {'entryLogin': [960, 560], 'entryRegistration': [960, 690],
                                                 'profile': [960, 486], 'subscription': [960, 580]}}
    (HERE / 'public/capture-manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf8')
    print('Prepared font, current PDF textures and source manifest.')


if __name__ == '__main__':
    main()
