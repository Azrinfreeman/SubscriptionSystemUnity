"""Rebuild the consumer PDF. Requires reportlab and Pillow; see README.md."""
from pathlib import Path
import json, re
from xml.sax.saxutils import escape
from reportlab.pdfgen import canvas
from reportlab.lib import colors
from reportlab.lib.pagesizes import A4
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import Paragraph, Table, TableStyle
from reportlab.lib.styles import ParagraphStyle
from PIL import Image

HERE = Path(__file__).resolve().parent
ROOT = HERE.parent.parent
OUT = ROOT / 'output/pdf/Subscription_System_Setup_Guide.pdf'
DATA = json.loads((HERE / 'guide.json').read_text(encoding='utf-8'))
W, H = A4
M, CW = 44, A4[0] - 88
INK = colors.HexColor('#203248')
TEAL = colors.HexColor('#256f68')
MUTED = colors.HexColor('#596b7b')
PALE = colors.HexColor('#edf5f3')
LINE = colors.HexColor('#dbe5eb')
ORANGE = colors.HexColor('#ec9b51')

def register():
    fonts = [('Body', 'segoeui.ttf'), ('Bold', 'seguisb.ttf'), ('Code', 'consola.ttf')]
    for name, filename in fonts:
        path = Path('C:/Windows/Fonts') / filename
        if not path.exists():
            raise SystemExit(f'Missing font {path}; edit register() for your system.')
        pdfmetrics.registerFont(TTFont(name, str(path)))

register()
STYLES = {
    'body': ParagraphStyle('body', fontName='Body', fontSize=10.6, leading=15, textColor=INK),
    'intro': ParagraphStyle('intro', fontName='Body', fontSize=11.5, leading=16.2, textColor=MUTED),
    'caption': ParagraphStyle('caption', fontName='Body', fontSize=8.6, leading=12, textColor=MUTED),
    'cell': ParagraphStyle('cell', fontName='Body', fontSize=9.2, leading=13, textColor=INK),
    'cellhead': ParagraphStyle('cellhead', fontName='Bold', fontSize=9.2, leading=13, textColor=colors.white),
}

def clean(text):
    return text.replace('\u2011','-').replace('\u2013','-').replace('\u2014','-')

def para(c, text, y, style='body', width=CW, x=M, gap=10, markup=False):
    p = Paragraph(text if markup else escape(clean(text)), STYLES[style])
    _, height = p.wrap(width, H)
    p.drawOn(c, x, y-height)
    return y-height-gap

def box(c, text, y, label='CHECKPOINT', fill=PALE):
    p = Paragraph(escape(clean(text)), STYLES['body'])
    _, height = p.wrap(CW-28, H)
    total = height+43
    c.setFillColor(fill); c.roundRect(M, y-total, CW, total, 9, fill=1, stroke=0)
    c.setFillColor(TEAL); c.setFont('Bold',8.3);c.drawString(M+14,y-17,label)
    p.drawOn(c,M+14,y-29-height)
    return y-total-13

def code(c,text,y):
    lines=clean(text).splitlines()
    longest=max(pdfmetrics.stringWidth(line,'Code',9.1) for line in lines)
    size=min(9.1,9.1*(CW-26)/max(longest,1))
    if size < 7.6:
        raise RuntimeError('Code line too long; revise source instead of making it unreadable.')
    leading=size+3.5;height=len(lines)*leading+24
    c.setFillColor(colors.HexColor('#f3f5f8'));c.roundRect(M,y-height,CW,height,7,fill=1,stroke=0)
    c.setFillColor(INK);c.setFont('Code',size)
    for i,line in enumerate(lines):c.drawString(M+13,y-16-i*leading,line)
    return y-height-13

def table(c,rows,y):
    cells=[[Paragraph(escape(clean(value)),STYLES['cellhead' if i==0 else 'cell']) for value in row] for i,row in enumerate(rows)]
    t=Table(cells,colWidths=[CW*.29,CW*.71],hAlign='LEFT')
    t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,0),TEAL),('VALIGN',(0,0),(-1,-1),'TOP'),('LEFTPADDING',(0,0),(-1,-1),9),('RIGHTPADDING',(0,0),(-1,-1),9),('TOPPADDING',(0,0),(-1,-1),7),('BOTTOMPADDING',(0,0),(-1,-1),7),('ROWBACKGROUNDS',(0,1),(-1,-1),[colors.white,colors.HexColor('#f4f7f9')]),('LINEBELOW',(0,0),(-1,0),.5,TEAL)]))
    _,height=t.wrap(CW,H);t.drawOn(c,M,y-height);return y-height-15

def node(c,x,y,w,h,title,subtitle=''):
    c.setFillColor(PALE);c.setStrokeColor(LINE);c.roundRect(x,y,w,h,8,fill=1,stroke=1)
    p=Paragraph(escape(title),ParagraphStyle('node',fontName='Bold',fontSize=11,leading=14,textColor=TEAL,alignment=1))
    _,ph=p.wrap(w-12,h);p.drawOn(c,x+6,y+h-ph-12)
    if subtitle:
        p=Paragraph(escape(subtitle),ParagraphStyle('nodesub',fontName='Body',fontSize=8.4,leading=11,textColor=MUTED,alignment=1))
        _,ph=p.wrap(w-12,h);p.drawOn(c,x+6,y+9)

def arrow(c,x1,y1,x2,y2):
    c.setStrokeColor(TEAL);c.setLineWidth(1.1);c.line(x1,y1,x2,y2)
    import math
    a=math.atan2(y2-y1,x2-x1)
    p=c.beginPath();p.moveTo(x2,y2);p.lineTo(x2-6*math.cos(a-.45),y2-6*math.sin(a-.45));p.lineTo(x2-6*math.cos(a+.45),y2-6*math.sin(a+.45));p.close()
    c.setFillColor(TEAL);c.drawPath(p,fill=1,stroke=0)

def picture(c,name,y,max_height=220):
    path=HERE/'assets'/f'{name}.png'
    if not path.exists(): raise RuntimeError(f'Missing actual screenshot: {path}')
    with Image.open(path) as im:iw,ih=im.size
    scale=min(CW/iw,max_height/ih);w,h=iw*scale,ih*scale
    c.drawImage(str(path),M+(CW-w)/2,y-h,width=w,height=h)
    return y-h-10

def visual(c,kind,y):
    x=M
    if kind=='contents':
        for idx,page in enumerate(DATA['pages'],start=2):
            if page['id']=='contents':continue
            c.setFont('Body',10.4);c.setFillColor(INK);c.drawString(M,y-13,page['title']);c.setFont('Bold',10);c.setFillColor(TEAL);c.drawRightString(W-M,y-13,str(idx))
            c.linkRect('',page['id'],(M,y-22,W-M,y+2),relative=0,thickness=0)
            c.setStrokeColor(LINE);c.line(M,y-24,W-M,y-24);y-=29
        return y-4
    if kind=='roadmap':
        width=(CW-27)/4
        for i,(title,sub) in enumerate([('01','Prepare'),('02','Install'),('03','Connect'),('04','Verify')]):
            node(c,x+i*(width+9),y-82,width,72,title,sub)
        return y-100
    if kind=='architecture':
        node(c,x,y-117,123,65,'Unity game','Existing two scenes')
        node(c,x+176,y-117,126,65,'PHP backend','Owns account/session')
        arrow(c,x+123,y-84,x+176,y-84)
        for i,(title,sub) in enumerate([('Database','Accounts and state'),('SMTP','Verification mail'),('Stripe','Checkout and billing')]):
            yy=y-49-i*63;node(c,x+365,yy,142,53,title,sub);arrow(c,x+302,y-84,x+365,yy+26)
        return y-196
    if kind=='folders':
        c.setFillColor(PALE);c.roundRect(M,y-173,CW,173,8,fill=1,stroke=0)
        c.setFillColor(TEAL);c.setFont('Bold',10);c.drawString(M+14,y-21,'PRIVATE: SubscriptionServer/')
        for i,t in enumerate(['.env  |  src/  |  config/  |  vendor/','database/  |  bin/  |  storage/']):
            c.setFont('Code',10);c.setFillColor(INK);c.drawString(M+14,y-45-i*21,t)
        node(c,M+14,y-151,CW-28,56,'PUBLIC: public/','Your HTTPS URL maps here; PHP endpoints and assets only')
        return y-188
    if kind=='config':
        node(c,M,y-64,220,58,'Server PUBLIC_URL','https://example.com/subscriptions')
        node(c,W-M-220,y-64,220,58,'Unity Backend Base Url','Use exactly the same address')
        arrow(c,M+220,y-35,W-M-220,y-35)
        return y-79
    if kind=='stripe':
        c.setFont('Body',8.2);c.setFillColor(MUTED);c.drawString(M,y-8,'ILLUSTRATION - not a Stripe Dashboard screenshot')
        width=(CW-32)/3
        for i,(title,sub) in enumerate([('Product','Your game offering'),('Recurring price','price_... ID'),('Server plan','Allowed game mapping')]):
            xx=M+i*(width+16);node(c,xx,y-89,width,62,title,sub)
            if i<2:arrow(c,xx+width,y-58,xx+width+16,y-58)
        return y-105
    if kind=='webhook':
        c.setFont('Body',8.2);c.setFillColor(MUTED);c.drawString(M,y-8,'PAYMENT CONFIRMATION FLOW')
        width=(CW-36)/4
        for i,(title,sub) in enumerate([('Stripe','Signed event'),('PHP receiver','Durable receipt'),('Worker','Reconcile state'),('Unity','Refresh access')]):
            xx=M+i*(width+12);node(c,xx,y-93,width,66,title,sub)
            if i<3:arrow(c,xx+width,y-60,xx+width+12,y-60)
        return y-110
    if kind=='inspector':
        c.setFillColor(INK);c.roundRect(M,y-163,CW,163,8,fill=1,stroke=0)
        c.setFont('Bold',12);c.setFillColor(colors.white);c.drawString(M+16,y-25,'Subscription Settings')
        for i,(key,value) in enumerate([('Backend Base Url','https://example.com/subscriptions/'),('Game Identifier','Your Game'),('Login Scene','LogMasuk'),('Home Scene','LamanUtama')]):
            yy=y-52-i*27;c.setFont('Body',9);c.setFillColor(colors.HexColor('#ccd9e3'));c.drawString(M+16,yy,key)
            c.setFillColor(colors.HexColor('#31485d'));c.roundRect(M+160,yy-6,CW-177,20,3,fill=1,stroke=0)
            c.setFillColor(colors.white);c.drawString(M+168,yy,value)
        return para(c,'Illustrated settings example, based on the delivered SubscriptionSettings fields. The table below lists the values to enter.',y-177,'caption')
    if kind in ('login','dashboard'):
        y=picture(c,kind,y,210)
        return para(c,'Actual existing Unity screen with sample data; no replacement scene is used.',y,'caption')
    if kind=='checklist':
        for i,t in enumerate(['Test-mode flow completed','SMTP and workers monitored','Secrets remain outside public','Platform/device checks completed']):
            yy=y-21-i*22;c.setStrokeColor(TEAL);c.rect(M,yy-2,10,10,fill=0,stroke=1);c.setFont('Body',10);c.setFillColor(INK);c.drawString(M+20,yy,t)
        return y-108
    return y

def footer(c,num,total):
    c.setStrokeColor(LINE);c.line(M,42,W-M,42)
    c.setFont('Body',8);c.setFillColor(MUTED);c.drawString(M,28,'SUBSCRIPTION SYSTEM / SETUP GUIDE / V1.0')
    c.drawRightString(W-M,28,f'{num} / {total}')
    if num!=3:
        c.setFillColor(TEAL);c.drawCentredString(W/2,28,'Contents');c.linkRect('','contents',(W/2-24,22,W/2+24,37),thickness=0)

def main():
    OUT.parent.mkdir(parents=True,exist_ok=True)
    c=canvas.Canvas(str(OUT),pagesize=A4,pageCompression=1)
    c.setTitle(DATA['title']);c.setAuthor('Subscription System Template');c.setSubject('Consumer installation and configuration guide')
    total=len(DATA['pages'])+1
    c.setFillColor(TEAL);c.rect(0,H-16,W,16,fill=1,stroke=0)
    c.setFont('Bold',10);c.setFillColor(TEAL);c.drawString(M,H-65,'UNITY + PHP / VISUAL SETUP GUIDE')
    cover=Paragraph('Your subscription<br/>game, connected',ParagraphStyle('cover',fontName='Bold',fontSize=35,leading=42,textColor=INK))
    _,hh=cover.wrap(CW,H);cover.drawOn(c,M,H-100-hh)
    y=H-118-hh;y=para(c,DATA['subtitle'],y,'intro');y=para(c,DATA['version'],y,'caption')
    y=picture(c,'dashboard',y-18,250)
    y=box(c,'Keep your original Unity scenes. Install the PHP backend, connect your services, and verify the complete account-to-subscription flow.',y-12,'YOUR ROUTE')
    para(c,'Includes illustrated steps, copyable commands, setup checkpoints and troubleshooting. Hosting/Stripe diagrams are clearly marked as illustrations.',y,'body')
    c.bookmarkPage('cover');c.addOutlineEntry('Start here','cover',0);footer(c,1,total);c.showPage()
    for number,page in enumerate(DATA['pages'],start=2):
        c.bookmarkPage(page['id']);c.addOutlineEntry(page['title'],page['id'],0)
        c.setFillColor(TEAL);c.setFont('Bold',9);c.drawString(M,H-45,page['part'])
        title=Paragraph(escape(page['title']),ParagraphStyle('title',fontName='Bold',fontSize=25,leading=30,textColor=INK))
        _,hh=title.wrap(CW,H);title.drawOn(c,M,H-62-hh);y=H-76-hh
        y=para(c,page['intro'],y,'intro',gap=15)
        if page.get('visual'):y=visual(c,page['visual'],y)
        if page.get('table'):y=table(c,page['table'],y)
        if page.get('code'):
            y=code(c,page['code'],y)
            if page.get('code_caption'):y=para(c,page['code_caption'],y,'caption')
        for i,(title,text) in enumerate(page.get('steps',[]),1):
            y=para(c,f'<font name="Bold" color="#256f68">{i:02d}  {escape(title)}</font><br/>{escape(clean(text))}',y,markup=True,gap=12)
        if page.get('callout'):y=box(c,page['callout'],y,'NOTE',colors.HexColor('#fff3e5'))
        for label,url in page.get('links',[]):
            y=para(c,f'<link href="{escape(url)}" color="#256f68"><u>{escape(label)}</u></link>',y,'caption',markup=True,gap=6)
        if page.get('checkpoint'):y=box(c,page['checkpoint'],y-2)
        if y<54:raise RuntimeError(f'Page {number} ({page["id"]}) overflows: bottom={y:.1f}; revise layout/content.')
        footer(c,number,total);c.showPage()
    c.save();print(f'Created {OUT} ({total} pages)')

if __name__=='__main__':main()
