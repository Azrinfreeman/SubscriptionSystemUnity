import React from 'react';
import {Composition} from 'remotion';
import {Trailer} from './Trailer';
import {TOTAL,VIDEO} from './theme';
export const Root:React.FC=()=> <Composition id="SubscriptionTrailer60" component={Trailer} width={VIDEO.width} height={VIDEO.height} fps={VIDEO.fps} durationInFrames={TOTAL} defaultProps={{bgm:true}}/>;
