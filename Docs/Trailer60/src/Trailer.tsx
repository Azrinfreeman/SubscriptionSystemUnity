import React,{useEffect,useState} from 'react';
import {AbsoluteFill,Audio,continueRender,delayRender,Img,Sequence,staticFile,useCurrentFrame} from 'remotion';
import {theme,TOTAL} from './theme';
import {SHOTS,Shot} from './timeline';
import {Art,Caption,Cursor,Evidence,ramp,Screen,Title} from './components/Stage';
import {Filmstrip} from './components/Filmstrip';
import {PaymentFlow} from './components/PaymentFlow';

export type TrailerProps={bgm:boolean};

const ProductPanel:React.FC<{base:string;panel:string;click?:number;target?:[number,number];initialOpen?:boolean;after?:string}> = ({base,panel,click=45,target=[960,565],initialOpen=false,after}) => {
  const f=useCurrentFrame();const opening=initialOpen?1:ramp(f,click,7);
  const move=initialOpen?1:ramp(f,click+7,30);const scale=.52+.31*move;
  const afterOpacity=after?ramp(f,110,10):0;
  return <div style={{position:'absolute',left:930,top:125,width:850,height:800,overflow:'hidden',border:`3px solid ${theme.primary}`,borderRadius:4}}>
    <div style={{position:'absolute',left:425-960*scale,top:400-540*scale,width:1920,height:1080,transform:`scale(${scale})`,transformOrigin:'0 0'}}>
      <Img src={staticFile(`screens/${base}`)} style={{position:'absolute',width:1920,height:1080}}/>
      <Img src={staticFile(`screens/${panel}`)} style={{position:'absolute',width:1920,height:1080,opacity:opening*(1-afterOpacity)}}/>
      {after&&<Img src={staticFile(`screens/${after}`)} style={{position:'absolute',width:1920,height:1080,opacity:afterOpacity}}/>}
      {!initialOpen&&<Cursor frame={f} to={target} click={click}/>}
    </div>
  </div>;
};

export const TrailerShot:React.FC<{shot:Shot;title?:string;copy?:string;duration?:number}> = ({shot,title=shot.title,copy=shot.copy,duration=shot.duration}) => {
  const f=useCurrentFrame();const id=shot.id;
  return <AbsoluteFill style={{background:theme.paper,overflow:'hidden'}}>
    {['intro','design','outro'].includes(id)&&<Art opacity={id==='design'?.18:.30}/>}
    {id==='intro'&&<>
      <Evidence text="Unity subscription template"/>
      <Title text={title} copy={copy} top={200} size={92} width={690}/>
      <Screen src="entry.png" x={800} y={270} width={1000} scale={.94+.06*ramp(f,0,90)} rotate={-2+2*ramp(f,0,90)}/>
    </>}
    {id==='design'&&<>
      <Evidence text="Designed like a game menu"/><Title text={title} copy={copy} top={165} width={700}/>
      <Screen src="entry.png" x={800} y={190} width={1000}>
        <div style={{position:'absolute',left:0,top:0,width:'100%',height:'100%',overflow:'hidden'}}>
          <Img src={staticFile('screens/entry.png')} style={{position:'absolute',width:'100%',height:'100%',transform:`scale(${1+ .28*ramp(f,20,75)})`,transformOrigin:'50% 56%'}}/>
        </div>
      </Screen>
      <div style={{position:'absolute',left:105,top:700,display:'flex',alignItems:'center',gap:20,fontSize:theme.auxiliary}}><span style={{width:50,height:50,background:theme.primary}}/>Actions<span style={{width:50,height:50,background:theme.gold,marginLeft:25}}/>Focus</div>
    </>}
    {['register','login','profile','subscribe','access'].includes(id)&&<>
      <Evidence/><Title text={title} copy={copy} top={190} width={735} size={id==='login'?72:80}/>
      {id==='register'&&<ProductPanel base="entry.png" panel="SignUp.png" target={[960,690]}/>}
      {id==='login'&&<ProductPanel base="entry.png" panel="SignIn.png" after="home.png" target={[960,560]}/>}
      {id==='profile'&&<ProductPanel base="home.png" panel="Profile.png" target={[960,486]}/>}
      {id==='subscribe'&&<ProductPanel base="home.png" panel="SubscriptionDetails.png" target={[960,580]}/>}
      {id==='access'&&<ProductPanel base="home.png" panel="SubscriptionDetails.png" initialOpen/>}
      {id==='access'&&<div style={{position:'absolute',left:105,top:670,width:735,fontSize:theme.auxiliary,lineHeight:1.3,color:theme.primary}}>UI shows current status.<br/>Server confirms access.</div>}
    </>}
    {['payment','confirmation'].includes(id)&&<>
      <Evidence text="Illustrated payment flow · no live transaction"/>
      <Title text={title} top={125} size={86} width={1500}/>
      <PaymentFlow confirmation={id==='confirmation'}/>
    </>}
    {id==='extras'&&<><Evidence text="Supporting features"/><Filmstrip/></>}
    {id==='customize'&&<>
      <Evidence text="Editable source, practical guides"/><Title text={title} copy={copy} top={170} width={730}/>
      <div style={{position:'absolute',left:105,top:530,fontSize:theme.auxiliary,lineHeight:1.3}}>
        <div style={{color:theme.primary,marginBottom:20}}>DefaultGameUITheme.asset</div>
        <div style={{display:'flex',alignItems:'center',gap:20}}><span style={{width:46,height:46,background:theme.primary}}/>Primary color</div>
        <div style={{marginTop:20}}>Feedback 0.12 s<br/>Panels 0.22 s</div>
      </div>
      {['game-ui-customization','account-flow-customization'].map((src,i)=>{
        const p=ramp(f,8+i*12,28);return <div key={src} style={{position:'absolute',left:930+i*430,top:145+i*95,width:365,transform:`translateY(${(1-p)*120}px) rotate(${i===0?-4:4}deg)`,opacity:p}}>
          <Img src={staticFile(`screens/${src}.png`)} style={{width:365,border:`1px solid ${theme.rule}`,boxShadow:'0 8px 18px rgba(32,54,69,.15)'}}/>
          <div style={{fontSize:theme.auxiliary,textAlign:'center',marginTop:22,color:theme.primary}}>{i===0?'Menu guide':'Account guide'}</div>
        </div>;
      })}
    </>}
    {id==='outro'&&<>
      <div style={{position:'absolute',top:105,left:380,width:1160,textAlign:'center',fontSize:100,fontWeight:700,letterSpacing:-3,lineHeight:1.05,whiteSpace:'pre-line'}}>{title}</div>
      {[['entry.png',95,505,-5],['Profile.png',665,420,0],['SubscriptionDetails.png',1235,505,5]].map(([src,x,y,rot],i)=>{
        const p=ramp(f,i*6,30);return <Screen key={String(src)} src={String(src)} x={Number(x)+(i-1)*(1-p)*250} y={Number(y)+(1-p)*150} width={570} rotate={Number(rot)*p} opacity={p}/>;
      })}
    </>}
    {/* Shot duration is an editable clip prop; the exit follows its end. */}
    <div style={{position:'absolute',inset:0,background:theme.paper,opacity:ramp(f,duration-5,5)*.10,pointerEvents:'none'}}/>
  </AbsoluteFill>;
};

export const Trailer:React.FC<TrailerProps> = ({bgm=true}) => {
  const [handle]=useState(()=>delayRender('Load game typeface'));
  useEffect(()=>{
    const face=new FontFace('GameSans',`url(${staticFile('fonts/LiberationSans.ttf')})`);
    face.load().then(loaded=>{document.fonts.add(loaded);continueRender(handle);}).catch(error=>{throw error;});
  },[handle]);
  return <AbsoluteFill style={{fontFamily:theme.font,color:theme.ink,background:theme.paper}}>
    {SHOTS.map(shot=><Sequence key={shot.id} from={shot.from} durationInFrames={shot.duration} name={shot.id}>
      <TrailerShot shot={shot}/><Caption text={shot.caption}/>
    </Sequence>)}
    {bgm&&<Audio src={staticFile('audio/music.wav')} volume={1}/>}
    <Audio src={staticFile('audio/sfx.wav')} volume={1}/>
  </AbsoluteFill>;
};
