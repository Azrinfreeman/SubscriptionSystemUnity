"""Reproducible motion-graphics trailer, original project captures + original audio.
Run from anywhere: python Docs/Trailer/render_trailer.py [--previews]
FFMPEG_BINARY can select an installed encoder; otherwise imageio-ffmpeg supplies it.
"""
from pathlib import Path
import os,sys,json,math,subprocess,wave,argparse
import numpy as np
from PIL import Image,ImageDraw,ImageFont,ImageFilter

HERE=Path(__file__).resolve().parent
ROOT=HERE.parent.parent
DATA=json.loads((HERE/'timeline.json').read_text(encoding='utf-8'))
W,H,FPS=DATA['width'],DATA['height'],DATA['fps']
OUT=ROOT/'output/video';TMP=ROOT/'Temp/TrailerProduction'
OUT.mkdir(parents=True,exist_ok=True);TMP.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(ROOT/'Temp/TrailerTools'))
FONTDIR=Path(os.environ.get('TRAILER_FONT_DIR','C:/Windows/Fonts'))
WHITE='#f6f3eb';MUTED='#a3b6c7';ORANGE='#ffae64';TEAL='#74d9c0';NAVY='#101c2b';PANEL='#1a2b3f'
FONTS={}
def font(n,bold=False,mono=False):
    key=(n,bold,mono)
    if key not in FONTS:FONTS[key]=ImageFont.truetype(str(FONTDIR/('consola.ttf' if mono else 'seguisb.ttf' if bold else 'segoeui.ttf')),n)
    return FONTS[key]
def text(im,xy,s,n=28,color=WHITE,bold=False,mono=False):ImageDraw.Draw(im).text(xy,s,font=font(n,bold,mono),fill=color)
def rr(im,box,fill=PANEL,r=24,outline=None,width=1):ImageDraw.Draw(im).rounded_rectangle(tuple(map(int,box)),r,fill,outline,width)
def ease(x):x=max(0,min(1,x));return 1-(1-x)**3
def center(im,x,y,s,n=28,color=WHITE,bold=False):
    f=font(n,bold);d=ImageDraw.Draw(im);width=d.textlength(s,font=f);d.text((x-width/2,y),s,font=f,fill=color)
def chip(im,x,y,label,color=TEAL):
    width=int(ImageDraw.Draw(im).textlength(label,font=font(20,True)))+34
    rr(im,(x,y,x+width,y+39),'#24394c',12);text(im,(x+17,y+5),label,20,color,True)
def line(im,a,b,fill=TEAL,width=3):ImageDraw.Draw(im).line([a,b],fill,width)
def arrow(im,a,b):
    line(im,a,b);angle=math.atan2(b[1]-a[1],b[0]-a[0]);d=ImageDraw.Draw(im)
    d.polygon([b,(b[0]-12*math.cos(angle-.5),b[1]-12*math.sin(angle-.5)),(b[0]-12*math.cos(angle+.5),b[1]-12*math.sin(angle+.5))],fill=TEAL)

yy,xx=np.mgrid[0:H,0:W];base=np.zeros((H,W,3),dtype=np.float32);base[:]=[12,23,37]
for cx,cy,col,spread in [(1650,100,[24,65,72],850),(150,1030,[62,35,31],800)]:
    g=np.exp(-((xx-cx)**2+(yy-cy)**2)/(spread**2))[...,None];base+=g*np.array(col)
BG=Image.fromarray(np.clip(base,0,255).astype('uint8'));del base,xx,yy
ASSETS={}
for key in ['login','dashboard']:
    ASSETS[key]=Image.open(ROOT/f'Docs/SetupGuide/assets/{key}.png').convert('RGB')
for key,number in [('cover',1),('route',2),('unity',15)]:
    path=HERE/f'assets/guide-{key}.png'
    if path.exists():ASSETS[key]=Image.open(path).convert('RGB')

def screenshot(im,key,x,y,width,p=0,label='ORIGINAL UNITY SCENE / SAMPLE DATA'):
    src=ASSETS[key];height=round(width*src.height/src.width)
    # Window frame and subtle motion keep screen content intact and readable.
    y=int(y-10*math.sin(p*math.pi));rr(im,(x-6,y-47,x+width+6,y+height+6),'#0b1421',19,'#385065',2)
    for i,col in enumerate(['#f5917b','#e4c47f','#80c5ae']):ImageDraw.Draw(im).ellipse((x+17+i*22,y-30,x+27+i*22,y-20),fill=col)
    text(im,(x+100,y-35),label,17,MUTED,True)
    im.paste(src.resize((int(width),height),Image.Resampling.LANCZOS),(int(x),int(y)))

def heading(im,shot,p,x=110,y=275,size=73):
    shift=int(28*(1-ease(p*5)))
    text(im,(x,y-70+shift),shot['eyebrow'],22,TEAL,True)
    for i,s in enumerate(shot['title']):text(im,(x,y+i*(size+12)+shift),s,size,WHITE,True)

def footer(im,t):
    text(im,(110,1008),'UNITY SUBSCRIPTION SYSTEM TEMPLATE',18,MUTED,True)
    text(im,(1603,1008),'PROJECT TEASER',18,MUTED,True)
    line(im,(110,975),(1810,975),'#2b4052',2)
    line(im,(110,975),(110+1700*t/45,975),ORANGE,3)

def card(im,x,y,w,h,title,detail,index=None):
    rr(im,(x,y,x+w,y+h),PANEL,24,'#385166',2)
    if h<=100:
        text(im,(x+26,y+12),title,28,WHITE,True)
        text(im,(x+26,y+53),detail,23,MUTED)
        return
    if index is not None:chip(im,x+25,y+24,f'0{index}',ORANGE)
    text(im,(x+26,y+(82 if index is not None else 25)),title,32,WHITE,True)
    text(im,(x+26,y+(133 if index is not None else 79)),detail,23,MUTED)

def frame(t):
    shot=next((s for s in DATA['shots'] if s['start']<=t<s['end']),DATA['shots'][-1]);p=(t-shot['start'])/(shot['end']-shot['start'])
    im=BG.copy();d=ImageDraw.Draw(im)
    # Very restrained orbit and corner marks add depth without visual noise.
    for i in range(3):
        cx=1650+i*34;cy=165+i*24;r=145+i*45
        d.arc((cx-r,cy-r,cx+r,cy+r),int(t*3+i*50),int(t*3+i*50+155),fill='#294651',width=2)
    kind=shot['kind']
    if kind=='hero':
        heading(im,shot,p,y=275,size=96)
        text(im,(114,535),shot['subtitle'],31,MUTED)
        chip(im,114,607,'ORIGINAL SCENES',ORANGE);chip(im,366,607,'REUSABLE FOUNDATION')
        screenshot(im,'dashboard',830,270,950,p)
        text(im,(834,855),'Your project. A connected foundation.',27,MUTED)
    elif kind in ('accounts','dashboard'):
        heading(im,shot,p,y=270,size=68)
        lines={'accounts':['Registration. Login.','Email verification.'],'dashboard':['Your original dashboard.','A reusable foundation.']}[kind]
        for i,s in enumerate(lines):text(im,(114,465+43*i),s,29,MUTED)
        screenshot(im,'login' if kind=='accounts' else 'dashboard',740,265,1050,p)
        labels=['SIGN UP','LOG IN','VERIFY EMAIL'] if kind=='accounts' else ['PROFILE','SUBSCRIPTION','ACCOUNT']
        for i,label in enumerate(labels):
            if p>(i*.14):chip(im,750+i*345,890,label,ORANGE if i==int(p*3)%3 else TEAL)
    elif kind=='billing':
        heading(im,shot,p,y=230,size=78)
        text(im,(114,454),shot['subtitle'],29,MUTED)
        chip(im,114,520,'ILLUSTRATED BILLING FLOW',ORANGE)
        labels=[('Choose a plan','Server-owned price catalog'),('Stripe Checkout','Hosted subscription billing'),('Refresh access','Reconcile provider state')]
        for i,(a,b) in enumerate(labels):
            x=114+i*573;card(im,x,655,525,203,a,b,i+1)
            if i<2:arrow(im,(x+530,756),(x+568,756))
        for i in range(3):
            if p>i*.22:line(im,(140+i*573,879),(603+i*573,879),ORANGE,4)
    elif kind=='control':
        heading(im,shot,p,y=230,size=77)
        text(im,(114,452),'Use the existing account and subscription controls.',29,MUTED)
        screenshot(im,'dashboard',830,260,955,p)
        for i,(a,b) in enumerate([('Plan details','See the current plan'),('Access status','Know the access end date'),('Manage subscription','Open the billing management flow')]):
            if p>i*.12:card(im,114,560+i*115,640,100,a,b)
    elif kind=='settings':
        heading(im,shot,p,y=260,size=72)
        for i,s in enumerate(['One shared settings asset.','Your server. Your game.']):text(im,(114,461+i*43),s,29,MUTED)
        chip(im,114,584,'CONFIGURATION EXAMPLE',ORANGE)
        rr(im,(830,227,1795,850),'#1a2b3f',26,'#3c5668',2)
        text(im,(866,260),'Subscription Settings',35,WHITE,True)
        rows=[('Backend Base URL','https://example.com/subscriptions/'),('Game Identifier','Your Game'),('Login Scene','LogMasuk'),('Home Scene','LamanUtama'),('Billing Link Endpoint','createBillingLink.php')]
        for i,(a,b) in enumerate(rows):
            y=340+i*94;text(im,(866,y),a,22,MUTED)
            rr(im,(866,y+32,1758,y+80),'#263e51',9)
            text(im,(884,y+39),b,25,TEAL if i==int(p*5)%5 else WHITE)
    elif kind=='architecture':
        heading(im,shot,p,y=200,size=82)
        text(im,(114,428),shot['subtitle'],29,MUTED)
        for i,(a,b) in enumerate([('Unity','Existing scenes + account UI'),('PHP','Authentication + verification'),('Stripe','Subscription reconciliation')]):
            x=114+i*573;card(im,x,570,525,190,a,b,i+1)
            if i<2:arrow(im,(x+530,670),(x+568,670))
        chip(im,114,825,'INCLUDED BACKEND',ORANGE)
        text(im,(430,829),'Migrations  /  Configuration examples  /  Maintenance jobs',28,MUTED)
    elif kind=='guide':
        heading(im,shot,p,y=278,size=82)
        text(im,(114,490),'An illustrated 20-page guide is included.',29,MUTED)
        chip(im,114,568,'STEP-BY-STEP CHECKPOINTS',ORANGE)
        for i,key in enumerate(['route','unity','cover']):
            src=ASSETS[key];w=345;h=int(w*src.height/src.width);x=830+i*297;y=215+(1-i%2)*30-int(10*p)
            rr(im,(x-6,y-6,x+w+6,y+h+6),'#415562',9);im.paste(src.resize((w,h),Image.Resampling.LANCZOS),(x,y))
    elif kind=='end':
        center(im,960,230,shot['eyebrow'],28,TEAL,True)
        center(im,960,350,'Make it yours.',118,WHITE,True)
        center(im,960,535,shot['subtitle'],31,MUTED)
        for i,label in enumerate(['UNITY CLIENT','PHP BACKEND','VISUAL GUIDE']):
            x=465+i*350;rr(im,(x,655,x+300,725),PANEL,20,'#3b5668',2);center(im,x+150,673,label,25,ORANGE,True)
        center(im,960,815,'Your Game. Connected.',37,WHITE,True)
    footer(im,t)
    # Brief dip at cuts, plus an opening/ending fade. No flashing or long black gaps.
    age=t-shot['start'];remaining=shot['end']-t
    opacity=min(1,age/.20,remaining/.18)
    if t>44:opacity=min(opacity,45-t)
    if opacity<1:im=Image.blend(Image.new('RGB',(W,H),NAVY),im,max(0,opacity))
    return im

def music():
    sr=48000;N=45*sr;rng=np.random.default_rng(20261028);out=np.zeros((N,2),np.float64);beat=60/110
    def add(start,sound,gain=1,pan=0):
        k=round(start*sr)
        if k>=N:return
        n=min(len(sound),N-k)
        out[k:k+n,0]+=sound[:n]*gain*math.sqrt((1-pan)/2);out[k:k+n,1]+=sound[:n]*gain*math.sqrt((1+pan)/2)
    def tone(midi,duration,kind='pluck'):
        t=np.arange(int(duration*sr))/sr;freq=440*2**((midi-69)/12)
        if kind=='pad':env=np.minimum(t/.15,1)*np.minimum((duration-t)/.35,1);return (np.sin(2*np.pi*freq*t)+.18*np.sin(2*np.pi*freq*2.003*t))*env
        env=(1-np.exp(-t*180))*np.exp(-t*7);return (np.sin(2*np.pi*freq*t)+.27*np.sin(2*np.pi*freq*2*t)+.1*np.sin(2*np.pi*freq*3*t))*env
    chords=[[57,60,64],[53,57,60],[48,52,55],[55,59,62]]
    for bar in range(21):
        start=bar*beat*4;chord=chords[bar%4]
        for note in chord:add(start,tone(note,beat*4+.3,'pad'),.045,(-.3 if note%2 else .3))
        for j in range(8):
            note=chord[[0,1,2,1,0,2,1,2][j]]+12
            add(start+j*beat/2,tone(note,.65),.105 if bar>1 else .07,(-.35 if j%2 else .35))
        for j in range(4):
            at=start+j*beat
            add(at,tone(chord[0]-12,.45),.21)
            t=np.arange(int(.23*sr))/sr;kick=np.sin(2*np.pi*(48*t+65*.025*(1-np.exp(-t/.025))))*np.exp(-t*22);add(at,kick,.25)
            if j%2:
                t=np.arange(int(.16*sr))/sr;noise=rng.normal(0,1,len(t));snare=(noise*.55+np.sin(2*np.pi*185*t)*.3)*np.exp(-t*32);add(at,snare,.095)
            for off in [0,.5]:
                t=np.arange(int(.065*sr))/sr;noise=rng.normal(0,1,len(t));high=np.r_[0,np.diff(noise)]*.4*np.exp(-t*75);add(at+off*beat,high,.042,.2)
    # Original melodic signature at the opening and closing phrase.
    for start in [0,8*beat,32*beat,64*beat,76*beat]:
        for i,note in enumerate([76,79,81,79,76,74,72]):add(start+i*beat/2,tone(note,.9),.11,math.sin(i)*.2)
    env=np.minimum(np.arange(N)/sr/.7,1)*np.minimum((N-1-np.arange(N))/sr/1.15,1)
    out*=env[:,None];out=np.tanh(out*1.2);out*=.82/max(np.max(np.abs(out)),.82)
    path=TMP/'original_score.wav'
    with wave.open(str(path),'wb') as w:w.setnchannels(2);w.setsampwidth(2);w.setframerate(sr);w.writeframes((out*32767).astype('<i2').tobytes())
    return path

def srt():
    def stamp(s):return f'00:00:{int(s):02d},000'
    lines=[]
    for i,s in enumerate(DATA['shots'],1):lines.extend([str(i),f"{stamp(s['start'])} --> {stamp(s['end'])}",' '.join(s['title']),s['subtitle'],''])
    (OUT/'Subscription_Template_Trailer_45s.srt').write_text('\n'.join(lines),encoding='utf-8')

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--previews',action='store_true');args=ap.parse_args();srt()
    for i,s in enumerate(DATA['shots']):frame((s['start']+s['end'])/2).save(TMP/f'shot-{i+1:02}.jpg',quality=90)
    frame(2).save(OUT/'Subscription_Template_Poster.png')
    if args.previews:print('Preview frames ready.',flush=True);return
    ff=os.environ.get('FFMPEG_BINARY')
    if not ff:
        import imageio_ffmpeg
        ff=imageio_ffmpeg.get_ffmpeg_exe()
    audio=music();output=OUT/'Subscription_Template_Trailer_45s.mp4'
    cmd=[ff,'-hide_banner','-loglevel','warning','-y','-f','rawvideo','-vcodec','rawvideo','-pix_fmt','rgb24','-s',f'{W}x{H}','-r',str(FPS),'-i','pipe:0','-i',str(audio),'-map','0:v:0','-map','1:a:0','-c:v','libx264','-preset','fast','-crf','19','-pix_fmt','yuv420p','-c:a','aac','-b:a','192k','-ar','48000','-af','loudnorm=I=-16:TP=-1:LRA=9','-t','45','-movflags','+faststart','-metadata','title=Your Game. Connected.','-metadata','comment=Original project screenshots, sample data, illustrated flows and original procedural score.',str(output)]
    with (TMP/'encode.log').open('w') as log:
        proc=subprocess.Popen(cmd,stdin=subprocess.PIPE,stderr=log)
        try:
            for n in range(45*FPS):
                proc.stdin.write(frame(n/FPS).tobytes())
                if n%150==0:print(f'Rendered {n}/{45*FPS} frames',flush=True)
            proc.stdin.close();code=proc.wait()
        except BaseException:proc.kill();raise
    if code:raise RuntimeError('Encoder failed; inspect Temp/TrailerProduction/encode.log')
    print(f'Created {output}',flush=True)
if __name__=='__main__':main()
