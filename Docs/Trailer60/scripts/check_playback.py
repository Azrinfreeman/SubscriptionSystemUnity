"""Run the delivered preview through a real browser from start to finish, muted.

Requires Python Playwright, installed Edge and a local HTTP server for this folder
on port 3031. This checks media playback, not subjective sound quality.
"""
from pathlib import Path
import json
import shutil
from playwright.sync_api import sync_playwright

HERE = Path(__file__).resolve().parents[1]
QA = HERE / 'out/qa'
PREVIEW = HERE.parents[1] / 'output/video/Subscription_Template_Trailer_60s_Preview.mp4'


def main():
    QA.mkdir(parents=True, exist_ok=True)
    shutil.copy2(PREVIEW, QA / 'playback-preview.mp4')
    (QA / 'player.html').write_text('<!doctype html><html lang="en"><meta charset="utf-8">'
                                  '<title>Trailer playback check</title><body style="margin:0;background:#203645">'
                                  '<video controls muted style="width:100%" src="playback-preview.mp4"></video></body></html>', encoding='utf8')
    with sync_playwright() as runtime:
        browser = runtime.chromium.launch(executable_path='C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', headless=True)
        page = browser.new_page(viewport={'width': 1280, 'height': 800})
        errors = []
        page.on('pageerror', lambda error: errors.append(str(error)))
        page.goto('http://127.0.0.1:3031/out/qa/player.html')
        page.locator('video').evaluate('(video) => video.play()')
        page.wait_for_function('document.querySelector("video").ended', timeout=80000)
        result = page.locator('video').evaluate('(video) => ({ended:video.ended,duration:video.duration,currentTime:video.currentTime,width:video.videoWidth,height:video.videoHeight,error:video.error,quality:video.getVideoPlaybackQuality().toJSON?.() || {dropped:video.getVideoPlaybackQuality().droppedVideoFrames,total:video.getVideoPlaybackQuality().totalVideoFrames}})')
        result['pageErrors'] = errors
        result['muted'] = True
        assert result['ended'] and abs(result['duration'] - 60) < .025 and not result['error'] and not errors
        page.screenshot(path=str(QA / 'playback-ended.png'))
        (QA / 'playback.json').write_text(json.dumps(result, indent=2), encoding='utf8')
        print(json.dumps(result, indent=2))
        browser.close()


if __name__ == '__main__':
    main()
