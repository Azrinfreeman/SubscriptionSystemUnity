// Evaluate the project's own compiled timing modules, then export editing companions.
import ts from 'typescript';
import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {createRequire} from 'node:module';
import {resolve,dirname} from 'node:path';
import {fileURLToPath} from 'node:url';
const project=resolve(dirname(fileURLToPath(import.meta.url)),'..');
const generated=resolve(project,'out/generated');mkdirSync(generated,{recursive:true});
for(const name of ['theme','timeline']){
  const source=readFileSync(resolve(project,`src/${name}.ts`),'utf8');
  writeFileSync(resolve(generated,`${name}.js`),ts.transpileModule(source,{compilerOptions:{module:ts.ModuleKind.CommonJS,target:ts.ScriptTarget.ES2020}}).outputText);
}
const require=createRequire(import.meta.url);
const {SHOTS,SFX}=require(resolve(generated,'timeline.js'));
const {TOTAL,VIDEO}=require(resolve(generated,'theme.js'));
const manifest={...VIDEO,totalFrames:TOTAL,shots:SHOTS,sfx:SFX};
writeFileSync(resolve(project,'timeline.json'),JSON.stringify(manifest,null,2));
const output=resolve(project,'../../output/video');mkdirSync(output,{recursive:true});
const stamp=frame=>{const ms=Math.round(frame/VIDEO.fps*1000);return `${String(Math.floor(ms/3600000)).padStart(2,'0')}:${String(Math.floor(ms/60000)%60).padStart(2,'0')}:${String(Math.floor(ms/1000)%60).padStart(2,'0')},${String(ms%1000).padStart(3,'0')}`;};
writeFileSync(resolve(output,'Subscription_Template_Trailer_60s.srt'),SHOTS.map((shot,i)=>`${i+1}\n${stamp(shot.from)} --> ${stamp(shot.from+shot.duration)}\n${shot.caption}\n`).join('\n'));
console.log(`Exported ${SHOTS.length} shots, ${TOTAL} frames and matching SRT.`);
