"""Audit only approved 0.6.0 art and build read-only action review sheets."""
import hashlib
import json
import math
from pathlib import Path
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'output/runtime-validation/art-review'
OUT.mkdir(parents=True,exist_ok=True)
manifest=json.loads((ROOT/'resources/actor-frames.manifest.json').read_text(encoding='utf-8'))
errors=[]; forms=[]; icons=[]; sourceOwners={}
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
def check(entry,path,owner):
    proof=json.loads((ROOT/entry['provenance']).read_text(encoding='utf-8-sig'))
    source=(ROOT/proof['sourceFile']).resolve(); source.relative_to(ROOT.resolve())
    assert sha(path)==entry['sha256']==proof['outputSha256'], 'Installed hash mismatch'
    assert sha(source)==proof['sourceSha256'], 'Original hash mismatch'
    assert proof['method']=='built-in-imagegen-independent-frame', 'Wrong independent source method'
    digest=proof['sourceSha256']
    if digest in sourceOwners: errors.append('Duplicate source: '+owner+' / '+sourceOwners[digest])
    sourceOwners[digest]=owner
    with Image.open(path) as im:
        im.load(); assert im.mode=='RGBA', 'Expected transparent RGBA PNG'
        bbox=im.getchannel('A').point(lambda v:255 if v>=128 else 0).getbbox()
        assert bbox, 'Empty subject'
        return dict(canvas=list(im.size),visibleBounds=list(bbox),
                    touchesEdge=bbox[0]==0 or bbox[1]==0 or bbox[2]==im.width or bbox[3]==im.height,
                    byteIdenticalToSource=sha(path)==digest,
                    normalization=proof.get('normalization'))
for group in manifest['groups']:
    if not group.get('approved'): continue
    aid=group['actorId']; actions={}; sheets=[]
    folder=ROOT/('NativeActorResources' if group.get('nativeResourcePath') else 'GameResources')/(group.get('nativeResourcePath') or group['resourcePath'])/'main'
    for action,clip in group['actions'].items():
        frames=[]
        for entry in clip['frames']:
            path=folder/(entry['name']+'.png')
            try: info=check(entry,path,aid+'/'+entry['name']); frames.append(dict(name=entry['name'],**info))
            except Exception as ex: errors.append(aid+'/'+entry['name']+': '+str(ex))
        actions[action]=frames
        if not group.get('nativeResourcePath') and frames:
            cw=max(f['canvas'][0] for f in frames); ch=max(f['canvas'][1] for f in frames)
            scale=max(1,256//max(cw,ch)); w=cw*scale; h=ch*scale
            cols=min(5,len(frames)); rows=math.ceil(len(frames)/cols)
            sheet=Image.new('RGB',(cols*(w+8),rows*(h+24)),(30,36,42)); draw=ImageDraw.Draw(sheet)
            for i,f in enumerate(frames):
                with Image.open(folder/(f['name']+'.png')) as im:
                    shown=im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST)
                    x=(i%cols)*(w+8); y=(i//cols)*(h+24)
                    sheet.paste(shown,(x,y+20),shown)
                    draw.text((x+4,y+4),f['name'],fill='white')
            target=OUT/(aid+'-'+action+'.png'); sheet.save(target)
            sheets.append(target.relative_to(ROOT).as_posix())
    forms.append(dict(actorId=aid,native=bool(group.get('nativeResourcePath')),actions=actions,reviewSheets=sheets,visualContinuityRechecked=False))
for entry in manifest['icons']:
    if not entry.get('approved'): continue
    try: icons.append(dict(actorId=entry['actorId'],**check(entry,ROOT/'GameResources'/(entry['resourcePath']+'.png'),entry['actorId']+'/icon')))
    except Exception as ex: errors.append(entry['actorId']+'/icon: '+str(ex))
report=dict(scope='approved-0.6.0-only',forms=forms,icons=icons,errors=errors,
            frameCount=sum(len(a) for f in forms for a in f['actions'].values()),
            iconCount=len(icons),legacy054Scanned=False,gameSizeVerified=False)
(OUT/'audit.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(dict(forms=len(forms),frames=report['frameCount'],icons=len(icons),errors=errors,sheets=sum(len(f['reviewSheets']) for f in forms))))
