// Normalize ImageGen's authored cells; no anatomy edits, mirroring or recoloring.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const sharp = require(process.env.MCLSL_SHARP || 'sharp');
const root = path.resolve(__dirname, '..');
const out = path.join(root, 'output/beast-optimization');
fs.mkdirSync(out, { recursive: true });
const read = p => JSON.parse(fs.readFileSync(path.join(root, p), 'utf8'));
const write = (p, v) => fs.writeFileSync(path.join(root, p), JSON.stringify(v, null, 2) + '\n');
const hash = b => crypto.createHash('sha256').update(b).digest('hex');
const sources = [
  ['fenghuang', process.argv[2], [0, 345, 650, 975, 1254]],
  ['dafeng', process.argv[3], [0, 320, 610, 920, 1254]],
];
const prompt = 'User reference phoenix/Dafeng: six folded-wing idle poses and eight coordinated two-wing flying poses, right-facing WorldBox pixel art, true alpha, no third wing or wing ghosts. Generated with built-in image_gen; see retained source sheet.';

async function bounds(input) {
  const { data, info } = await sharp(input).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
  let x0=info.width, y0=info.height, x1=0, y1=0;
  for(let y=0;y<info.height;y++) for(let x=0;x<info.width;x++)
    if(data[(y*info.width+x)*4+3]>=16) { x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x+1);y1=Math.max(y1,y+1); }
  if(x1<=x0 || y1<=y0) throw Error('Empty authored cell');
  return { left:x0, top:y0, width:x1-x0, height:y1-y0 };
}

async function main() {
  const manifest=read('resources/actor-frames.manifest.json');
  const policy=read('resources/actor-size-policy.json');
  if(policy.birdAnimationRevision) throw Error('Already integrated; refuse destructive re-normalization');
  const report=[];
  for(const [species,source,rows] of sources) {
    const sheet=fs.readFileSync(source), meta=await sharp(sheet).metadata();
    if(meta.width!==1254 || meta.height!==1254 || !meta.hasAlpha) throw Error('Unexpected sheet geometry');
    const sourcePath='resources/bird-animation-sources/'+species+'.png';
    fs.mkdirSync(path.join(root,path.dirname(sourcePath)),{recursive:true});
    fs.copyFileSync(source,path.join(root,sourcePath));
    const group=manifest.groups.find(g=>g.actorId==='mclsl_beast_'+species);
    const spec=policy.forms.find(g=>g.actorId===group.actorId), edge=group.canvas[0];
    const oldIdle=fs.readFileSync(path.join(root,'NativeActorResources',group.nativeResourcePath,'main/idle_0.png'));
    const oldHeight=(await bounds(oldIdle)).height;
    const cells=[];
    for(let i=0;i<14;i++) {
      const col=i%4, row=Math.floor(i/4), left=Math.round(col*1254/4), right=Math.round((col+1)*1254/4);
      const rect={left,top:rows[row],width:right-left,height:rows[row+1]-rows[row]};
      const bytes=await sharp(sheet).extract(rect).png().toBuffer();
      cells.push({bytes,rect,bounds:await bounds(bytes)});
    }
    const ratio=edge*.78/Math.max(...cells.map(c=>Math.max(c.bounds.width,c.bounds.height)));
    const before=[], after=[];
    let newIdleHeight;
    for(const action of ['idle','walk']) {
      // Recover after the downstroke instead of jumping from low to the apex.
      const order=action==='idle'?[0,1,2,3,4,5]:[6,7,8,9,10,11,13,12];
      for(let i=0;i<order.length;i++) {
        const frame=group.actions[action].frames[i], cell=cells[order[i]];
        const dest='NativeActorResources/'+group.nativeResourcePath+'/main/'+frame.name+'.png';
        const old=fs.readFileSync(path.join(root,dest));
        fs.mkdirSync(path.join(out,'before',path.dirname(dest)),{recursive:true});
        fs.writeFileSync(path.join(out,'before',dest),old);
        const width=Math.round(cell.bounds.width*ratio), height=Math.round(cell.bounds.height*ratio);
        const content=await sharp(cell.bytes).extract(cell.bounds).resize(width,height,{kernel:'nearest'}).png().toBuffer();
        const left=Math.round((edge-width)/2), top=Math.round(group.anchor[1]-height);
        if(top<1 || top+height>=edge) throw Error('Insufficient transparent margin');
        const result=await sharp({create:{width:edge,height:edge,channels:4,background:{r:0,g:0,b:0,alpha:0}}})
          .composite([{input:content,left,top}]).png().toBuffer();
        if(action==='idle' && i===0)newIdleHeight=(await bounds(result)).height;
        frame.sha256=hash(result);
        frame.origin='reference-redrawn-sheet-cell';
        fs.writeFileSync(path.join(root,dest),result);
        const proof=read(frame.provenance);
        proof.previousSourceSha256=proof.sourceSha256;
        proof.method='built-in-imagegen-reviewed-sheet-cell';
        proof.sourceSha256=hash(sheet);proof.sourceSheet=sourcePath;proof.sourceCell=cell.rect;
        proof.sourceRetention='full generated sheet retained in resources/bird-animation-sources';
        proof.prompt=prompt;proof.outputSha256=frame.sha256;proof.createdAtUtc=new Date().toISOString();
        proof.normalization={resampling:'nearest',canvas:[edge,edge],sourceCanvas:[cell.rect.width,cell.rect.height],
          scale:ratio,cropped:false,transparentPaddingTrimmed:true,spritesheetSlicing:true,colorQuantized:false,
          resizePasses:1,visibleBoundsAlpha:16,placement:[left,top]};
        delete proof.repair;
        proof.integrationStatus='offline-integration-awaiting-Unity-review';
        write(frame.provenance,proof);
        before.push(old);after.push(result);
      }
    }
    // Preserve previous apparent resting height after changing authored padding.
    const ppu=group.pixelsPerUnit*newIdleHeight/oldHeight;
    for(const action of ['idle','walk']) for(const frame of group.actions[action].frames) frame.pixelsPerUnit=ppu;
    group.pixelsPerUnit=ppu;
    group.calibration.measurementStatus='render-ratio-and-redrawn-birds-awaiting-Unity';
    const composites=[];
    for(let i=0;i<14;i++) {
      composites.push({input:await sharp(before[i]).resize(160,160).png().toBuffer(),left:i%7*160,top:Math.floor(i/7)*320});
      composites.push({input:await sharp(after[i]).resize(160,160).png().toBuffer(),left:i%7*160,top:Math.floor(i/7)*320+160});
    }
    const review='output/beast-optimization/'+species+'-before-after.png';
    await sharp({create:{width:1120,height:640,channels:4,background:'#242a30'}}).composite(composites).png().toFile(path.join(root,review));
    report.push({species,frames:14,source:sourcePath,sourceSha256:hash(sheet),review,
      idleFolded:'offline-inspected',movingWings:'offline-inspected-two-wings',
      bodyAnchor:'bounds-aligned; movement continuity requires Unity review',unityVerified:false});
  }
  policy.birdAnimationRevision='20261020';
  write('resources/actor-frames.manifest.json',manifest);
  write('resources/actor-size-policy.json',policy);
  write('resources/bird-animation-review.json',{revision:'20261020',records:report,unityVerified:false,
    limitations:['Attack, hit and death artwork retained.','Direction uses existing native flip; not yet observed in Unity.',
      'Bounds alignment cannot certify body stability during state transitions.']});
  console.log(JSON.stringify({installedFrames:28,sourceSheets:2,unityVerified:false}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});
