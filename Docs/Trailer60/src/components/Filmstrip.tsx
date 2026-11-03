import React from 'react';
import {Img,staticFile,useCurrentFrame,Easing,interpolate} from 'remotion';
import {theme} from '../theme';
import {ramp} from './Stage';

// Adapted from the reviewed word-relay-filmstrip demo: 18f steps, 7f delayed
// text entrance, old text exits first. Screens and copy use this game's identity.
const switches=[8,52,96];
const sources=['entry.png','SignIn.png','LogoutPanel.png'];
const words=['Motion preference','Keyboard focus','Device recovery'];
const notes=['Choose full or minimum animation.','Navigate usable controls with Tab.','Handle an account used on another device.'];
const swift=Easing.bezier(.2,0,.1,1);
const step=(frame:number) => switches.reduce((sum,start)=>sum+interpolate(frame,[start,start+18],[0,1],{easing:swift,extrapolateLeft:'clamp',extrapolateRight:'clamp'}),0);

export const Filmstrip:React.FC = () => {
  const f=useCurrentFrame();const scroll=step(f)*565;
  return <>
    <div style={{position:'absolute',left:80,top:135,width:960,height:760,overflow:'hidden'}}>
      {[-1,0,1,2,3].map(i=>{
        const top=115+(i+1)*565-scroll;const distance=Math.min(1.4,Math.abs(top-115)/565);
        const src=sources[Math.max(0,Math.min(2,i))];
        return <div key={i} style={{position:'absolute',left:10,top,width:940,height:529,transform:`scale(${1-.06*Math.min(1,distance)})`,overflow:'hidden',border:`2px solid ${theme.primary}`,filter:distance>.2?`blur(${Math.min(3,distance*3)}px)`:undefined}}>
          <Img src={staticFile(`screens/${src}`)} style={{width:'100%',height:'100%'}}/>
          <div style={{position:'absolute',inset:0,background:theme.ink,opacity:Math.min(.68,distance*.68)}}/>
        </div>;
      })}
    </div>
    <div style={{position:'absolute',left:1100,top:351,width:720}}>
      <div style={{fontSize:50,color:theme.primary}}>Small details.</div>
      <div style={{position:'relative',height:180,marginTop:16,overflow:'hidden'}}>
        {words.map((word,i)=>{
          const inAt=switches[i]+7;const outAt=switches[i+1];const out=outAt===undefined?0:ramp(f,outAt,8);
          if(f<inAt||out>=1)return null;
          return <div key={word} style={{position:'absolute',fontSize:80,fontWeight:700,lineHeight:1.02,letterSpacing:-2,transform:`translateY(${-out*175}px)`,opacity:1-out*.6}}>
            {word.split(' ').map((line,k)=><div key={k} style={{overflow:'hidden'}}>{Array.from(line).map((ch,j)=><span key={j} style={{display:'inline-block',transform:`translateY(${(1-ramp(f,inAt+j*1.2,16))*85}px)`}}>{ch}</span>)}</div>)}
          </div>;
        })}
      </div>
      {notes.map((note,i)=>{
        const start=switches[i]+7;const out=switches[i+1];
        const opacity=ramp(f,start+5,10)*(out===undefined?1:1-ramp(f,out,7));
        return <p key={note} style={{position:'absolute',fontSize:theme.auxiliary,lineHeight:1.3,width:680,opacity,marginTop:20}}>{note}</p>;
      })}
    </div>
  </>;
};
