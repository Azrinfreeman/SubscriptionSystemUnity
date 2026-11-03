import React from 'react';
import {AbsoluteFill, Easing, Img, interpolate, staticFile, useCurrentFrame} from 'remotion';
import {theme} from '../theme';

export const ease = Easing.bezier(.16,1,.3,1);
export const ramp = (f:number,start:number,length:number) => interpolate(f,[start,start+length],[0,1],{extrapolateLeft:'clamp',extrapolateRight:'clamp',easing:ease});

export const Art:React.FC<{home?:boolean;opacity?:number}> = ({home=false,opacity=.26}) => (
  <AbsoluteFill><Img src={staticFile(`screens/${home?'home':'entry'}-art.png`)} style={{width:'100%',height:'100%',opacity}}/></AbsoluteFill>
);

export const Title:React.FC<{text:string;copy?:string;color?:string;top?:number;size?:number;width?:number}> = ({text,copy,color=theme.ink,top=145,size=theme.title,width=650}) => {
  const frame=useCurrentFrame();const p=ramp(frame,0,18);
  return <div style={{position:'absolute',left:100,top,width,color,transform:`translateY(${(1-p)*30}px)`,opacity:p}}>
    <h1 style={{fontSize:size,lineHeight:1.06,letterSpacing:-2.2,whiteSpace:'pre-line',margin:0,fontWeight:700}}>{text}</h1>
    {copy&&<p style={{fontSize:theme.body,lineHeight:1.42,whiteSpace:'pre-line',margin:'30px 0 0',fontWeight:400}}>{copy}</p>}
  </div>;
};

export const Caption:React.FC<{text:string}> = ({text}) => <div style={{position:'absolute',left:0,bottom:0,width:1920,height:theme.captionHeight,display:'flex',alignItems:'center',padding:'16px 100px',background:theme.primary,color:theme.paper,fontSize:theme.caption,lineHeight:1.06,whiteSpace:'pre-line'}}>{text}</div>;

export const Screen:React.FC<{src:string;x?:number;y?:number;width?:number;children?:React.ReactNode;opacity?:number;scale?:number;rotate?:number}> = ({src,x=775,y=245,width=1040,children,opacity=1,scale=1,rotate=0}) => (
  <div style={{position:'absolute',left:x,top:y,width,height:width*9/16,opacity,transform:`scale(${scale}) rotate(${rotate}deg)`,transformOrigin:'50% 50%',outline:`4px solid ${theme.primary}`,boxShadow:`0 10px 0 ${theme.gold}`,overflow:'hidden',borderRadius:4}}>
    <Img src={staticFile(`screens/${src}`)} style={{position:'absolute',inset:0,width:'100%',height:'100%'}}/>{children}
  </div>
);

// Screenshot coordinates are logical 1920x1080 pixels; textures are 3840x2160.
export const CameraScreen:React.FC<{src:string;cx:number;cy:number;zoom:number;children?:React.ReactNode}> = ({src,cx,cy,zoom,children}) => (
  <div style={{position:'absolute',left:0,top:0,width:1920,height:1080,overflow:'hidden'}}>
    <div style={{position:'absolute',left:960-cx*zoom,top:540-cy*zoom,width:1920,height:1080,transform:`scale(${zoom})`,transformOrigin:'0 0'}}>
      <Img src={staticFile(`screens/${src}`)} style={{width:1920,height:1080}}/>{children}
    </div>
  </div>
);

export const Cursor:React.FC<{frame:number;to:[number,number];click:number;from?:[number,number];start?:number}> = ({frame,to,click,from=[1540,850],start=10}) => {
  const p=ramp(frame,start,click-start);const x=from[0]+(to[0]-from[0])*p;const y=from[1]+(to[1]-from[1])*p;
  const pulse=interpolate(frame,[click,click+16],[0,1],{extrapolateLeft:'clamp',extrapolateRight:'clamp'});
  return <div style={{position:'absolute',left:x,top:y,opacity:frame>click+32?1-ramp(frame,click+32,12):1}}>
    {frame>=click&&frame<click+16&&<div style={{position:'absolute',left:-32-pulse*15,top:-32-pulse*15,width:64+pulse*30,height:64+pulse*30,border:`4px solid ${theme.gold}`,borderRadius:'50%',opacity:1-pulse}}/>}
    <svg width="42" height="50" viewBox="0 0 42 50"><path d="M4 3L5 40L15 31L23 47L31 43L23 28L37 26Z" fill="white" stroke={theme.ink} strokeWidth="3" strokeLinejoin="round"/></svg>
  </div>;
};

export const Evidence:React.FC<{text?:string}> = ({text='UI walkthrough · sample data'}) => <div style={{position:'absolute',left:100,top:52,fontSize:theme.auxiliary,color:theme.primary}}>{text}</div>;
