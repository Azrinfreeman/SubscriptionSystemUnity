"""Probe, decode, measure and extract review frames from the delivered MP4s.

Requires FFmpeg/FFprobe, numpy and Pillow. Run from any directory after render.
The machine checks complement visual review; they do not certify subjective audio taste.
"""
from pathlib import Path
import hashlib
import json
import re
import subprocess
import wave
import numpy as np
from PIL import Image, ImageDraw

HERE = Path(__file__).resolve().parents[1]
OUTPUT = HERE.parents[1] / 'output/video'
QA = HERE / 'out/qa'


def command(arguments):
    result = subprocess.run(arguments, capture_output=True, check=True)
    return result.stdout.decode('utf8', errors='replace')


def probe(path):
    return json.loads(command(['ffprobe', '-v', 'error', '-count_frames', '-show_streams', '-show_format', '-of', 'json', str(path)]))


def measure_text_lines(path, bounds, minimum_height, light_text=False):
    """Measure visible glyph rows in an isolated, settled encoded text region.

    Bounds are full-resolution picture pixels, excluding arrows and screenshots.
    These probes guard the labels that previously failed the readability review;
    visual inspection still owns wrapping, contrast and the rest of the picture.
    """
    pixels = np.asarray(Image.open(path).convert('RGB').crop(bounds))
    mask = (pixels.min(axis=2) > 205) if light_text else (pixels.max(axis=2) < 180)
    rows = np.flatnonzero(mask.sum(axis=1) >= 3)
    groups = np.split(rows, np.flatnonzero(np.diff(rows) > 1) + 1)
    heights = [int(group[-1] - group[0] + 1) for group in groups if group.size]
    assert heights and min(heights) >= minimum_height, f'Text below minimum in {path.name}, {bounds}: {heights}'
    return {'bounds': bounds, 'lineHeights': heights, 'minimum': minimum_height, 'passed': True}


def main():
    QA.mkdir(parents=True, exist_ok=True)
    paths = [OUTPUT / 'Subscription_Template_Trailer_60s.mp4', OUTPUT / 'Subscription_Template_Trailer_60s_NoBGM.mp4']
    report = {'files': {}, 'reviewFrames': [], 'audioTiming': []}
    for path in paths:
        data = probe(path)
        video = next(s for s in data['streams'] if s['codec_type'] == 'video')
        audio = next(s for s in data['streams'] if s['codec_type'] == 'audio')
        assert (video['width'], video['height'], video['r_frame_rate']) == (1920, 1080, '30/1')
        assert video['codec_name'] == 'h264' and video['pix_fmt'] == 'yuv420p'
        assert int(video['nb_read_frames']) == 1800
        assert abs(float(video['duration']) - 60) < .001
        assert abs(float(data['format']['duration']) - 60) < .025
        assert audio['codec_name'] == 'aac' and audio['sample_rate'] == '48000' and audio['channels'] == 2
        command(['ffmpeg', '-v', 'error', '-i', str(path), '-f', 'null', '-'])
        video_hash = command(['ffmpeg', '-v', 'error', '-i', str(path), '-map', '0:v:0', '-c', 'copy', '-f', 'hash', '-hash', 'sha256', '-']).strip()
        report['files'][path.name] = {'duration': data['format']['duration'], 'videoFrames': video['nb_read_frames'],
                                     'videoCodec': video['codec_name'], 'pixelFormat': video['pix_fmt'],
                                     'audio': {k: audio[k] for k in ['codec_name', 'sample_rate', 'channels']},
                                     'bytes': path.stat().st_size, 'videoHash': video_hash, 'decode': 'passed'}
    assert len({item['videoHash'] for item in report['files'].values()}) == 1, 'Audio versions have different pictures'
    final = paths[0]
    measured = subprocess.run(['ffmpeg', '-hide_banner', '-i', str(final), '-af', 'loudnorm=I=-16:TP=-1.5:LRA=9:print_format=json', '-f', 'null', '-'], capture_output=True, check=True).stderr.decode('utf8', errors='replace')
    loudness = json.loads(re.findall(r'\{[^{}]*"input_i"[^{}]*\}', measured)[-1])
    report['loudness'] = loudness
    assert -18 <= float(loudness['input_i']) <= -14
    assert float(loudness['input_tp']) <= -1
    # Inspect actual encoded frames each second, plus frames adjacent to major actions.
    review = QA / 'decoded'
    review.mkdir(exist_ok=True)
    command(['ffmpeg', '-y', '-v', 'error', '-i', str(final), '-vf', 'fps=1,scale=480:270', str(review / 'second-%02d.png')])
    pictures = sorted(review.glob('second-*.png'))
    assert len(pictures) == 60
    sheet = Image.new('RGB', (1920, 305 * 15), '#dce5e8')
    for i, path in enumerate(pictures):
        image = Image.open(path).convert('RGB')
        assert np.asarray(image).mean() > 15, 'Unexpected dark gap'
        x, y = (i % 4) * 480, (i // 4) * 305
        sheet.paste(image, (x, y + 28))
        ImageDraw.Draw(sheet).text((x + 7, y + 5), f'{i:02d}s', fill='#203645')
    sheet.save(QA / 'encoded-contact-sheet.jpg', quality=92)
    # Four smaller sheets make every frame inspectable without image downscaling.
    for part in range(4):
        selected = pictures[part * 15:(part + 1) * 15]
        small = Image.new('RGB', (1440, 305 * 5), '#dce5e8')
        for i, path in enumerate(selected):
            x, y = (i % 3) * 480, (i // 3) * 305
            small.paste(Image.open(path), (x, y + 28))
            ImageDraw.Draw(small).text((x + 7, y + 5), f'{part * 15 + i:02d}s', fill='#203645')
        small.save(QA / f'encoded-part-{part + 1}.jpg', quality=92)
    for frame in [0, 60, 210, 344, 345, 352, 390, 494, 495, 555, 570, 645, 690, 795, 840, 990, 1095, 1140, 1290, 1366, 1416, 1464, 1495, 1590, 1740, 1799]:
        target = review / f'frame-{frame}.png'
        command(['ffmpeg', '-y', '-v', 'error', '-i', str(final), '-vf', fr'select=eq(n\,{frame})', '-frames:v', '1', str(target)])
        report['reviewFrames'].append(frame)
    report['readability'] = {}
    probes = [
        ('authenticatedTicket', 990, (465, 335, 795, 450), 32, False),
        ('hostedCheckout', 990, (1130, 335, 1450, 450), 32, False),
        ('refreshCurrentState', 1140, (425, 340, 945, 400), 32, False),
        ('themeAsset', 1590, (100, 525, 850, 595), 32, False),
        ('feedbackTiming', 1590, (100, 700, 850, 820), 32, False),
    ]
    for name, frame, bounds, minimum, light in probes:
        report['readability'][name] = measure_text_lines(review / f'frame-{frame}.png', bounds, minimum, light)
    for frame in [60, 210, 390, 570, 690, 840, 990, 1140, 1290, 1495, 1590, 1799]:
        measured = measure_text_lines(review / f'frame-{frame}.png', (90, 890, 1830, 1075), 56, True)
        assert len(measured['lineHeights']) == 2, f'Expected two caption lines at frame {frame}'
        report['readability'][f'caption-{frame}'] = measured
    raw_audio = subprocess.run(['ffmpeg', '-v', 'error', '-i', str(paths[1]), '-vn', '-f', 'f32le', '-ac', '1', '-ar', '48000', '-'], capture_output=True, check=True).stdout
    decoded = np.frombuffer(raw_audio, dtype='<f4')
    # This interval contains no authored SFX. It catches a mislabeled BGM export
    # even when picture hashes and cue correlations otherwise look correct.
    quiet = decoded[6 * 48000:9 * 48000]
    quiet_rms = float(np.sqrt(np.mean(quiet ** 2)))
    assert quiet_rms < .0001, 'NoBGM version contains sound during the known silent interval'
    report['noBgmSilentInterval'] = {'seconds': [6, 9], 'rms': quiet_rms, 'passed': True}
    with wave.open(str(HERE / 'public/audio/sfx.wav'), 'rb') as source:
        samples = np.frombuffer(source.readframes(source.getnframes()), dtype='<i2').reshape(-1, 2).mean(axis=1) / 32768
    for frame in [345, 645, 795]:
        start = frame * 1600
        template = samples[start:start + 2048]
        window = decoded[start - 4096:start + 4096 + 2048]
        lag = int(np.argmax(np.correlate(window, template, mode='valid'))) - 4096
        assert abs(lag) <= 4800, 'SFX exceeds three-frame timing tolerance'
        report['audioTiming'].append({'cueFrame': frame, 'lagSamples': lag, 'lagFrames': lag / 1600})
    (HERE / 'verification.json').write_text(json.dumps(report, indent=2), encoding='utf8')
    print(json.dumps(report, indent=2))


if __name__ == '__main__':
    main()
