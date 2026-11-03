import React from 'react';
import {useCurrentFrame} from 'remotion';
import {theme} from '../theme';
import {ramp} from './Stage';

export const PaymentFlow:React.FC<{confirmation?:boolean}> = ({confirmation=false}) => {
  const local=useCurrentFrame();const f=local+(confirmation?150:0);
  const nodes=[{x:100,title:'Unity',detail:'Player menu'},{x:760,title:'PHP backend',detail:'Owns access'},{x:1420,title:'Stripe',detail:'Checkout'}];
  const stages=confirmation?['Signed webhook','Update state','Refresh access']:['Request billing','Issue ticket','Open checkout'];
  return <>
    <svg width="1920" height="1080" style={{position:'absolute',inset:0}}>
      {[0,1].map(i=><g key={i}><path d={`M${520+i*660} 590H${750+i*660}`} stroke={theme.rule} strokeWidth="4"/><path d={`M${520+i*660} 590H${750+i*660}`} stroke={theme.primary} strokeWidth="5" pathLength="1" strokeDasharray="1" strokeDashoffset={1-ramp(f,25+i*40,22)}/><path d={`M${736+i*660} 580L${750+i*660} 590L${736+i*660} 600`} fill="none" stroke={theme.primary} strokeWidth="4" opacity={ramp(f,40+i*40,10)}/></g>)}
      {confirmation&&<path d="M1630 715V800H970V715" fill="none" stroke={theme.primary} strokeWidth="4" pathLength="1" strokeDasharray="1" strokeDashoffset={1-ramp(local,18,28)}/>}
      {confirmation&&<><path d="M960 470V410H300V470" fill="none" stroke={theme.primary} strokeWidth="4" pathLength="1" strokeDasharray="1" strokeDashoffset={1-ramp(local,62,28)}/><path d="M290 455L300 470L310 455 M960 730L970 715L980 730" fill="none" stroke={theme.primary} strokeWidth="4" opacity={ramp(local,78,12)}/></>}
      {[0,1].map(i=>{
        const p=ramp(f,35+i*40,30);return <circle key={i} cx={520+i*660+p*230} cy={590} r={9} fill={theme.gold} opacity={p>0&&p<1?1:0}/>;
      })}
    </svg>
    {nodes.map((node,i)=>{
      const p=ramp(f,i*6,20);return <div key={node.title} style={{position:'absolute',left:node.x,top:470,width:400,height:235,padding:'42px 34px',background:theme.field,borderBottom:`7px solid ${theme.gold}`,opacity:p,transform:`translateY(${(1-p)*40}px)`}}>
        <div style={{fontSize:54,fontWeight:700,color:theme.primary}}>{node.title}</div><div style={{fontSize:theme.auxiliary,marginTop:20}}>{node.detail}</div>
      </div>;
    })}
    {!confirmation&&<><div style={{position:'absolute',left:470,top:340,width:320,fontSize:theme.auxiliary,lineHeight:1.2,textAlign:'center'}}>Authenticated<br/>ticket</div><div style={{position:'absolute',left:1130,top:340,width:320,fontSize:theme.auxiliary,lineHeight:1.2,textAlign:'center'}}>Hosted<br/>checkout</div></>}
    {confirmation&&<div style={{position:'absolute',left:1100,top:751,fontSize:theme.auxiliary,background:'white',padding:'0 20px'}}>Signed webhook</div>}
    {confirmation&&<div style={{position:'absolute',left:425,top:345,fontSize:theme.auxiliary,background:'white',padding:'0 20px',opacity:ramp(local,62,16)}}>Refresh current state</div>}
    <div style={{position:'absolute',left:100,top:815,display:'flex',gap:70,fontSize:theme.auxiliary,color:theme.primary}}>
      {stages.map((stage,i)=><span key={stage} style={{opacity:ramp(local,20+i*40,16),transform:`translateY(${(1-ramp(local,20+i*40,16))*15}px)`}}>{stage}</span>)}
    </div>
  </>;
};
