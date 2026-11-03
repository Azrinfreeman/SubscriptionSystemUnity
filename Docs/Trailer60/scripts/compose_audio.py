"""Original 120 BPM instrumental and action sounds. No sampled songs or voices.

The picture's TypeScript cue list is authoritative. Rebuild with Python + numpy
and FFmpeg on PATH. Generated WAVs are assets, not the editable music source.
"""
from pathlib import Path
import json
import math
import re
import subprocess
import wave
import numpy as np

HERE = Path(__file__).resolve().parents[1]
SR = 48000
SECONDS = 60
BEAT = .5
N = SR * SECONDS
rng = np.random.default_rng(20261103)


def add(track, seconds, sound, gain=1, pan=0):
    start = round(seconds * SR)
    if start >= len(track):
        return
    count = min(len(sound), len(track) - start)
    track[start:start + count, 0] += sound[:count] * gain * math.sqrt((1 - pan) / 2)
    track[start:start + count, 1] += sound[:count] * gain * math.sqrt((1 + pan) / 2)


def note(midi, seconds, pad=False):
    t = np.arange(round(seconds * SR)) / SR
    frequency = 440 * 2 ** ((midi - 69) / 12)
    if pad:
        envelope = np.minimum(t / .25, 1) * np.minimum((seconds - t) / .45, 1)
        signal = np.sin(2 * np.pi * frequency * t) + .12 * np.sin(2 * np.pi * frequency * 2.002 * t)
    else:
        envelope = (1 - np.exp(-t * 140)) * np.exp(-t * 4.5)
        signal = sum(gain * np.sin(2 * np.pi * frequency * harmonic * t)
                     for harmonic, gain in [(1, 1), (2, .28), (3, .08), (4, .03)])
    return signal * envelope


def noise(seconds):
    return rng.normal(0, 1, round(seconds * SR))


def write(path, data):
    with wave.open(str(path), 'wb') as output:
        output.setnchannels(2)
        output.setsampwidth(2)
        output.setframerate(SR)
        output.writeframes((np.clip(data, -1, 1) * 32767).astype('<i2').tobytes())


def main():
    out = HERE / 'public/audio'
    out.mkdir(parents=True, exist_ok=True)
    music = np.zeros((N, 2), np.float64)
    # D major, G major, B minor, A major. Warm extended harmony, no game bleeps.
    chords = [[50, 57, 62, 66, 69], [43, 55, 59, 62, 66],
              [47, 54, 59, 62, 66], [45, 57, 61, 64, 69]]
    for bar in range(30):
        start = bar * 4 * BEAT
        chord = chords[(bar // 2) % len(chords)]
        middle = 15 <= bar < 20
        energy = .58 if bar < 3 else .72 if middle else 1
        for index, pitch in enumerate(chord[1:]):
            add(music, start, note(pitch, 2.3, True), .024, (index - 1.5) * .22)
        for i in range(8):
            pitch = chord[1 + [0, 2, 1, 3, 2, 1, 3, 1][i]] + 12
            add(music, start + i * BEAT / 2, note(pitch, .8), .045 * energy, (-.3 if i % 2 else .3))
        for beat in range(4):
            at = start + beat * BEAT
            t = np.arange(round(.25 * SR)) / SR
            kick = np.sin(2 * np.pi * (47 * t + 1.9 * (1 - np.exp(-t / .022)))) * np.exp(-t * 22)
            add(music, at, kick, .27 * energy)
            add(music, at, note(chord[0] - 12, .35), .13 * energy)
            if beat % 2 and bar >= 2:
                t = np.arange(round(.15 * SR)) / SR
                clap = noise(.15) * np.exp(-t * 40)
                clap = np.r_[0, np.diff(clap)] * .3
                add(music, at, clap, .105 * energy)
            for subdivision in [0, .5]:
                t = np.arange(round(.055 * SR)) / SR
                hat = np.r_[0, np.diff(noise(.055))] * np.exp(-t * 95)
                add(music, at + subdivision * BEAT, hat, .025 * energy, .25)
        if bar in [0, 4, 8, 12, 20, 24, 27]:
            for i, pitch in enumerate([74, 78, 81, 78, 76, 74, 73, 69]):
                add(music, start + i * BEAT / 2, note(pitch, .95), .055, math.sin(i) * .2)
    # Light room reflections widen the instruments without obscuring the action cues.
    dry = music.copy()
    for delay, gain in [(.09, .08), (.19, .05), (.31, .025)]:
        offset = round(delay * SR)
        music[offset:] += dry[:-offset, ::-1] * gain
    time = np.arange(N) / SR
    music *= (np.minimum(time / .65, 1) * np.clip((60 - time) / 1.6, 0, 1))[:, None]
    music = np.tanh(music)
    write(out / 'music-raw.wav', music)
    subprocess.run(['ffmpeg', '-y', '-hide_banner', '-loglevel', 'error', '-i', str(out / 'music-raw.wav'),
                    '-af', 'loudnorm=I=-18:TP=-3:LRA=9', '-ar', str(SR), str(out / 'music.wav')], check=True)
    sfx = np.zeros_like(music)
    source = (HERE / 'src/timeline.ts').read_text(encoding='utf8')
    cues = [{'frame': int(frame), 'kind': kind} for frame, kind in
            re.findall(r"\{from:(\d+),kind:'(\w+)'\}", source)]
    cue_directory = out / 'cues'
    cue_directory.mkdir(exist_ok=True)
    for index, cue in enumerate(cues):
        kind = cue['kind']
        duration = .13 if kind in ['click', 'tick', 'step'] else .42
        t = np.arange(round(duration * SR)) / SR
        air = noise(duration)
        if kind in ['click', 'tick', 'step']:
            sound = (np.r_[0, np.diff(air)] * .25 + np.sin(2 * np.pi * 155 * t) * .2) * np.exp(-t * 75)
            gain = .16
        elif kind == 'settle':
            sound = (np.sin(2 * np.pi * 70 * t) * .5 + air * .12) * np.exp(-t * 15)
            gain = .20
        else:
            sound = np.convolve(air, np.ones(10) / 10, mode='same') * np.sin(np.pi * t / duration) ** 2
            gain = .16 if kind == 'sweep' else .11
        add(sfx, cue['frame'] / 30, sound, gain)
        # Individual stems let the optional visual workbench expose each action
        # separately. The master still uses the exact same summed source track.
        stem = np.zeros((len(sound), 2), np.float64)
        add(stem, 0, sound, gain)
        write(cue_directory / f'{index:02d}-{kind}.wav', stem)
    write(out / 'sfx.wav', sfx)
    (out / 'cue-report.json').write_text(json.dumps({'bpm': 120, 'fps': 30, 'cues': cues}, indent=2), encoding='utf8')
    print('Created original 60-second music and SFX WAVs; cues:', len(cues))


if __name__ == '__main__':
    main()
