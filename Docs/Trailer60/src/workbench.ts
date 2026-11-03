import React from 'react';
import {Trailer,TrailerShot} from './Trailer';
import {Caption} from './components/Stage';
import {SHOTS,SFX} from './timeline';
import {theme,TOTAL,VIDEO} from './theme';

// Same shot/caption data as the original composition; no second timing table.
export const WORKBENCH={
  name:'Unity subscription template · 60 seconds',
  revision:'20261103-r3',
  fps:VIDEO.fps,width:VIDEO.width,height:VIDEO.height,total:TOTAL,background:theme.paper,
  shots:SHOTS.map(shot=>({id:shot.id,label:shot.narrativeGoal,from:shot.from,duration:shot.duration,
    component:TrailerShot,props:{shot,title:shot.title,copy:shot.copy,duration:shot.duration},durationProp:'duration',
    cardId:shot.id,cardName:shot.narrativeGoal,
    schema:[{type:'textarea',key:'title',label:'Title',default:shot.title},{type:'textarea',key:'copy',label:'Copy',default:shot.copy}]})),
  captions:SHOTS.map(shot=>({id:`caption-${shot.id}`,label:shot.caption,from:shot.from,duration:shot.duration,
    component:Caption,props:{text:shot.caption},schema:[{type:'textarea',key:'text',label:'Caption',default:shot.caption}]})),
  transitions:[],overlays:[],
  sfx:SFX.map((cue,index)=>({from:cue.from,duration:Math.ceil((['click','tick','step'].includes(cue.kind)?.13:.42)*VIDEO.fps),src:`audio/cues/${String(index).padStart(2,'0')}-${cue.kind}.wav`,volume:1})),
  bgm:[{from:0,duration:TOTAL,src:'audio/music.wav',volume:1}],
  order:['captions'],original:Trailer,
};
