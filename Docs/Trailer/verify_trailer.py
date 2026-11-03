"""Decode the complete export, measure audio, and extract a one-second review grid."""
from pathlib import Path
import sys,subprocess,json,re
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'Temp/TrailerTools'))
import imageio_ffmpeg
ff=imageio_ffmpeg.get_ffmpeg_exe();movie=ROOT/'output/video/Subscription_Template_Trailer_45s.mp4';tmp=ROOT/'Temp/TrailerProduction'
count,duration=imageio_ffmpeg.count_frames_and_secs(str(movie))
assert count==1350,(count,duration)
assert abs(duration-45)<.04,duration
meta=subprocess.run([ff,'-hide_banner','-i',str(movie)],capture_output=True,text=True).stderr
assert '1920x1080' in meta and '30 fps' in meta and 'yuv420p' in meta
assert '48000 Hz, stereo' in meta and 'Audio: aac' in meta
(tmp/'media-info.txt').write_text(meta,encoding='utf-8')
decode=subprocess.run([ff,'-hide_banner','-v','error','-i',str(movie),'-f','null','-'],capture_output=True,text=True)
assert decode.returncode==0 and not decode.stderr.strip(),decode.stderr
audio=subprocess.run([ff,'-hide_banner','-i',str(movie),'-vn','-af','loudnorm=I=-16:TP=-1:LRA=9:print_format=json','-f','null','-'],capture_output=True,text=True)
match=re.search(r'\{\s*"input_i".*?\}',audio.stderr,re.S)
assert match,audio.stderr
measures=json.loads(match.group(0));assert -17<float(measures['input_i'])<-15,measures
assert float(measures['input_tp'])<=-1,measures
(tmp/'audio-measurements.json').write_text(json.dumps(measures,indent=2),encoding='utf-8')
subprocess.run([ff,'-hide_banner','-loglevel','error','-y','-i',str(movie),'-vf',r'select=eq(mod(n\,30)\,15),scale=480:270','-fps_mode','vfr','-frames:v','45',str(tmp/'review-%02d.png')],check=True)
files=sorted(tmp.glob('review-[0-9][0-9].png'));assert len(files)==45,len(files)
for offset in range(0,len(files),9):
    sheet=Image.new('RGB',(1440,870),'#edf2f6');d=ImageDraw.Draw(sheet)
    for i,path in enumerate(files[offset:offset+9]):
        x=(i%3)*480;y=(i//3)*290;d.text((x+8,y+3),f'{offset+i+.5:.1f} seconds',fill='#101c2b');sheet.paste(Image.open(path),(x,y+20))
    sheet.save(tmp/f'review-grid-{offset//9+1}.jpg',quality=93)
report={'frames':count,'duration_seconds':duration,'resolution':'1920x1080','fps':30,'video':'H.264 / yuv420p','audio':'AAC stereo / 48 kHz','full_decode':'passed, no errors','audio_measured_lufs':float(measures['input_i']),'audio_measured_true_peak_dbtp':float(measures['input_tp']),'bytes':movie.stat().st_size,'review_frames':len(files)}
(ROOT/'Docs/Trailer/validation.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print(json.dumps(report,indent=2))
