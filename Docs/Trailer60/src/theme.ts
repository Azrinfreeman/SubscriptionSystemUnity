export const VIDEO = {width: 1920, height: 1080, fps: 30, bpm: 120, beats: 120};
export const beatF = (beat: number) => Math.round(beat * VIDEO.fps * 60 / VIDEO.bpm);
export const TOTAL = beatF(VIDEO.beats);
export const theme = {
  ink: '#203645', primary: '#205F75', gold: '#F2BF50',
  field: '#EAF3F6', paper: '#FFFFFF', rule: '#CAD9DF',
  font: 'GameSans, sans-serif',
  // Liberation Sans needs this size for at least 32px of visible auxiliary text.
  margin: 100, title: 82, body: 48, auxiliary: 48, caption: 78, captionHeight: 200,
};
