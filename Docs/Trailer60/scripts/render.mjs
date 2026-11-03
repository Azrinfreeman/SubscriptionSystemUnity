import {bundle} from '@remotion/bundler';
import {openBrowser,renderMedia,renderStill,selectComposition} from '@remotion/renderer';
import {existsSync,mkdirSync,copyFileSync,writeFileSync} from 'node:fs';
import {dirname,resolve} from 'node:path';
import {fileURLToPath} from 'node:url';
import {spawnSync} from 'node:child_process';

const project=resolve(dirname(fileURLToPath(import.meta.url)),'..');
const output=resolve(project,'../../output/video');
const qa=resolve(project,'out/qa');
mkdirSync(output,{recursive:true});mkdirSync(qa,{recursive:true});
const manifest=spawnSync(process.execPath,[resolve(project,'scripts/export-timeline.mjs')],{stdio:'inherit'});
if(manifest.status!==0)throw Error('Timeline export failed.');
const browserExecutable=process.env.TRAILER_BROWSER || 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe';
if(!existsSync(browserExecutable)) throw Error('Set TRAILER_BROWSER to an installed Chrome/Edge executable.');
const serveUrl=await bundle({entryPoint:resolve(project,'src/index.ts'),publicDir:resolve(project,'public')});
const browser=await openBrowser('chrome',{browserExecutable});
try {
  const composition=await selectComposition({serveUrl,id:'SubscriptionTrailer60',puppeteerInstance:browser,inputProps:{bgm:true}});
  // Write copy/caption manifest from the actual compiled composition's source data.
  const frames=[60,210,390,570,690,840,990,1140,1290,1395,1445,1495,1590,1740,1799];
  for(const frame of frames){
    await renderStill({serveUrl,composition,frame,output:resolve(qa,`frame-${frame}.png`),imageFormat:'png',puppeteerInstance:browser,inputProps:{bgm:true}});
  }
  copyFileSync(resolve(qa,'frame-60.png'),resolve(output,'Subscription_Template_60s_Poster.png'));
  console.log('Representative stills complete.');
  if(!process.argv.includes('--stills')){
    const raw=resolve(project,'out/trailer-raw.mp4');let last=-1;
    await renderMedia({serveUrl,composition,inputProps:{bgm:true},puppeteerInstance:browser,codec:'h264',outputLocation:raw,
      pixelFormat:'yuv420p',crf:17,x264Preset:'fast',audioBitrate:'192k',sampleRate:48000,concurrency:3,
      onProgress:({progress})=>{const step=Math.floor(progress*10);if(step!==last){last=step;console.log(`Picture ${step*10}%`);}}});
    // Edge's intermediate H.264 can be flagged full-range (yuvj420p). Convert
    // once to ordinary limited-range yuv420p, shared by both delivery versions.
    const compatible=resolve(project,'out/picture-compatible.mp4');
    run(['-i',raw,'-an','-vf','scale=in_range=full:out_range=tv,format=yuv420p','-c:v','libx264','-preset','fast','-crf','17','-pix_fmt','yuv420p','-color_range','tv','-level:v','4.1',compatible]);
    const final=resolve(output,'Subscription_Template_Trailer_60s.mp4');
    run(['-i',compatible,'-i',raw,'-map','0:v:0','-map','1:a:0','-af','loudnorm=I=-16:TP=-1.5:LRA=9','-c:v','copy','-c:a','aac','-b:a','192k','-ar','48000','-movflags','+faststart',final]);
    // Audio-only render applies bgm=false to the identical composition/timeline.
    const sfxOnly=resolve(project,'out/sfx-only.wav');
    const noBgmComposition=await selectComposition({serveUrl,id:'SubscriptionTrailer60',puppeteerInstance:browser,inputProps:{bgm:false}});
    if(noBgmComposition.props.bgm!==false)throw Error('NoBGM composition resolved with music enabled.');
    await renderMedia({serveUrl,composition:noBgmComposition,inputProps:{bgm:false},puppeteerInstance:browser,codec:'wav',outputLocation:sfxOnly,concurrency:2,sampleRate:48000});
    run(['-i',compatible,'-i',sfxOnly,'-map','0:v:0','-map','1:a:0','-c:v','copy','-c:a','aac','-b:a','192k','-ar','48000','-movflags','+faststart',resolve(output,'Subscription_Template_Trailer_60s_NoBGM.mp4')]);
    run(['-i',final,'-vf','scale=1280:720','-c:v','libx264','-preset','fast','-crf','24','-pix_fmt','yuv420p','-color_range','tv','-c:a','aac','-b:a','128k','-movflags','+faststart',resolve(output,'Subscription_Template_Trailer_60s_Preview.mp4')]);
    console.log('Final, SFX-only and preview MP4s written to '+output);
  }
} finally {await browser.close({silent:true});}

function run(args){
  // AAC can carry a padded tail from the intermediate render. Trim audio/container
  // to the picture's 1,800-frame duration; never use -shortest to discard frames.
  const result=spawnSync('ffmpeg',['-y','-hide_banner','-loglevel','error',...args.slice(0,-1),'-t','60',args.at(-1)],{stdio:'inherit'});
  if(result.status!==0)throw Error('FFmpeg failed with exit '+result.status);
}
