"""Inspect real production images and render fixed-anchor previews; no mock game objects."""
import argparse
import hashlib
import json
import math
import re
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageChops, ImageStat

ROOT = Path(__file__).resolve().parents[1]


def audit(output):
    output.mkdir(parents=True, exist_ok=True)
    manifest = json.loads((ROOT / 'resources/actor-frames.manifest.json').read_text(encoding='utf-8'))
    groups = []
    for g in manifest['groups']:
        actions = {}
        for action, clip in g['actions'].items():
            entries = clip['frames']
            fallback = clip.get('visualFallback')
            if fallback:
                entries = [dict(g['actions'][fallback]['frames'][0], durationMs=clip['durationMs'])]
            actions[action] = dict(loop=clip['loop'], fallback=fallback, frames=[dict(
                path='NativeActorResources/' + g['nativeResourcePath'] + '/main/' + f['name'] + '.png',
                name=f['name'], durationMs=f['durationMs'], anchor=f['anchor'],
                ppu=f['pixelsPerUnit'], canvas=f['canvas'], sha256=f['sha256']) for f in entries])
        groups.append(dict(id=g['actorId'], source='actor-manifest', actions=actions))
    definitions = (ROOT / 'code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorTypes.cs').read_text(encoding='utf-8')
    for actor, folder in re.findall(r'new\("[^"]+", "(mclsl_[^"]+)", "([^"]+)"', definitions):
        actions = {}
        for file in sorted((ROOT / 'GameResources/actors/Souls' / folder).glob('*.png')):
            match = re.fullmatch(re.escape(folder) + r'_(\w+)_(\d+)', file.stem)
            if not match:
                raise ValueError('Unrecognized soul frame: ' + str(file))
            action = match[1].lower()
            with Image.open(file) as im:
                canvas = list(im.size)
            actions.setdefault(action, dict(loop=action in ('idle', 'breathing', 'run'), frames=[]))['frames'].append(dict(
                path=file.relative_to(ROOT).as_posix(), name=file.stem,
                durationMs={'run':100, 'attack':1000/15, 'hit':1000/15, 'death':1000/12}.get(action, 1000/7.5),
                anchor=[canvas[0]/2, canvas[1]/2], ppu=1, canvas=canvas,
                sha256=hashlib.sha256(file.read_bytes()).hexdigest()))
        for clip in actions.values():
            clip['frames'].sort(key=lambda f: int(f['name'].rsplit('_', 1)[1]))
        groups.append(dict(id=actor, source='world-soul-registration', folder=folder,
                           anchorStatus='preview uses centered pivot; native loader pivot requires Unity inspection', actions=actions))
    ids = {g['id'] for g in groups}
    catalog = (ROOT/'code/MySimulatedLongevityRoad/Data/MclslBeastCatalog.cs').read_text(encoding='utf-8')
    ordinary_text, named_text = catalog.split('internal static readonly MclslBeastDefinition[] Named =', 1)
    declared = set()
    for prefix, text in (('mclsl_beast_', ordinary_text), ('mclsl_named_', named_text)):
        for species in re.findall(r'new\("([a-z]+)",', text):
            declared.update((prefix+species,prefix+species+'_yuan'))
    characters=(ROOT/'code/MySimulatedLongevityRoad/Traits/MclslNamedCharacterRegistration.cs').read_text(encoding='utf-8')
    for species, beast in re.findall(r'new\("([a-z]+)", "[^"]+", "[^"]+"(, true)?\)', characters):
        if not beast: declared.add('mclsl_named_'+species)
    immortal=(ROOT/'code/MySimulatedLongevityRoad/Traits/MclslImmortalActorRegistration.cs').read_text(encoding='utf-8')
    declared.update(re.findall(r'const string \w+Id = "(mclsl_[^"]+)"',immortal))
    declared.update(re.findall(r'new\("[^"]+", "(mclsl_[^"]+)"', definitions))
    if declared != ids:
        raise ValueError('Registration/manifest mismatch: '+repr(declared^ids))
    # The generic avatar is registered but has no independently drawn animation.
    aliases = [dict(id='mclsl_world_soul_avatar', source='generic-avatar', animation='icon fallback when no elemental folder can be resolved')]
    expected = {f.resolve() for f in (ROOT/'NativeActorResources').rglob('*.png')}
    expected |= {f.resolve() for f in (ROOT/'GameResources/actors/Souls').rglob('*.png')}
    consumed, errors, candidates, all_metrics = set(), [], [], []
    sheets = []
    font = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 13)
    for g in groups:
        decoded = {}
        world_radius = 0
        for action, clip in g['actions'].items():
            for frame_number,f in enumerate(clip['frames']):
                file = (ROOT/f['path']).resolve()
                if not file.is_relative_to(ROOT) or not file.exists():
                    errors.append(dict(id=g['id'], path=f['path'], error='missing or illegal path')); continue
                consumed.add(file)
                raw = file.read_bytes()
                if hashlib.sha256(raw).hexdigest() != f['sha256']:
                    errors.append(dict(id=g['id'], path=f['path'], error='hash mismatch'))
                with Image.open(file) as im:
                    if 'A' not in im.getbands() or list(im.size) != f['canvas']:
                        errors.append(dict(id=g['id'], path=f['path'], error='canvas or alpha mismatch'))
                    image = im.convert('RGBA')
                alpha=image.getchannel('A'); box=alpha.point(lambda v:255 if v>=16 else 0).getbbox()
                if box is None:
                    if action=='death' and frame_number==len(clip['frames'])-1:
                        f['transparentTerminal']=True
                        box=(0,0,0,0)
                    else:
                        errors.append(dict(id=g['id'], path=f['path'], error='empty alpha')); continue
                decoded[f['path']] = image
                ax, ay=f['anchor']; ppu=f['ppu']
                # Native anchors are top-left pixel coordinates in this manifest.
                world_radius=max(world_radius, *(abs(v)/ppu for v in (box[0]-ax,box[2]-ax,box[1]-ay,box[3]-ay)))
                f['bounds']=list(box)
                f['edgeTouch']=not f.get('transparentTerminal') and (box[0]==0 or box[1]==0 or box[2]==image.width or box[3]==image.height)
        scale=58/max(world_radius,.001)
        framesets={}
        cell=144; cols=min(10,max(len(c['frames']) for c in g['actions'].values()))
        rows=sum(math.ceil(len(c['frames'])/cols) for c in g['actions'].values())
        sheet=Image.new('RGB',(max(800,cols*cell),rows*cell+30),(31,36,42)); d=ImageDraw.Draw(sheet)
        d.text((8,5),g['id'],font=font,fill='white')
        row=0
        for action,clip in g['actions'].items():
            tiles=[]; metrics=[]
            for col,f in enumerate(clip['frames']):
                if f['path'] not in decoded: continue
                im=decoded[f['path']]; r=scale/f['ppu']
                size=(max(1,round(im.width*r)),max(1,round(im.height*r)))
                scaled=im.resize(size,Image.Resampling.NEAREST)
                tile=Image.new('RGB',(cell,cell),(42,48,55)); td=ImageDraw.Draw(tile)
                td.line((0,78,cell,78),fill=(65,72,79));td.line((72,0,72,124),fill=(65,72,79))
                tile.paste(scaled,(round(72-f['anchor'][0]*r),round(78-f['anchor'][1]*r)),scaled)
                td.text((3,125),action+' '+str(col),font=font,fill='white')
                tiles.append(tile)
                sheet.paste(tile,((col%cols)*cell,(row+col//cols)*cell+30))
                if f.get('transparentTerminal'): continue
                x0,y0,x1,y1=f['bounds']; bx=(x0+x1)/2-f['anchor'][0];by=(y0+y1)/2-f['anchor'][1]
                metrics.append(dict(frame=f['name'],center=[bx/f['ppu'],by/f['ppu']],
                                    width=(x1-x0)/f['ppu'],height=(y1-y0)/f['ppu'],edgeTouch=f['edgeTouch']))
            if not tiles: continue
            gif=output/(g['id']+'-'+action+'.gif')
            options={'loop':0} if clip['loop'] else {}
            tiles[0].save(gif,save_all=True,append_images=tiles[1:],duration=[f['durationMs'] for f in clip['frames']],**options)
            framesets[action]=tiles
            row+=math.ceil(len(clip['frames'])/cols)
            pairs=list(zip(metrics,metrics[1:]))
            if clip['loop']:pairs+=list(zip(metrics[-1:],metrics[:1]))
            for a,b in pairs:
                shift=math.dist(a['center'],b['center'])
                ratio=max(a['height']/b['height'],b['height']/a['height'])
                if shift>max(a['height'],b['height'])*.22 or ratio>1.5:
                    candidates.append(dict(id=g['id'],action=action,first=a['frame'],second=b['frame'],
                                           shift=round(shift,4),heightRatio=round(ratio,3),status='visual-review-required'))
            all_metrics.append(dict(id=g['id'],action=action,frames=metrics))
        switch=[]; switch_times=[]
        for action in ('idle','walk','run','attack','hit','idle','death'):
            if action in framesets:
                switch+=framesets[action];switch_times += [f['durationMs'] for f in g['actions'][action]['frames']]
        switch[0].save(output/(g['id']+'-transitions.gif'),save_all=True,append_images=switch[1:],duration=switch_times,loop=0)
        sheet.save(output/(g['id']+'.png'))
        sheets.append((g['id'],sheet))
    for start in range(0,len(sheets),4):
        thumbw=1008; thumbs=[]
        for _,sheet in sheets[start:start+4]:
            t=sheet.copy(); t.thumbnail((thumbw,520));thumbs.append(t)
        page=Image.new('RGB',(thumbw*2,1040),(21,25,30))
        for i,t in enumerate(thumbs):page.paste(t,((i%2)*thumbw,(i//2)*520))
        page.save(output/('page-'+str(start//4+1).zfill(2)+'.png'))
    unreferenced=sorted(str(f.relative_to(ROOT)) for f in expected-consumed)
    report=dict(groups=groups,aliases=aliases,registeredAssets=len(ids)+len(aliases),
                uniqueImages=len(consumed),errors=errors,unreferencedImages=unreferenced,
                geometryCandidates=candidates,metrics=all_metrics,unityVerified=False,
                previewScope='Authored frame timing, not actual Unity playback. Soul ticks use simulation time at 60 ticks/second.',
                visualReview='pending; candidate geometry is not a pass/fail of anatomy')
    (output/'inventory.json').write_text(json.dumps(report,ensure_ascii=True,indent=2),encoding='utf-8')
    links='\n'.join('<h2>'+g['id']+'</h2><img width="100%" src="'+g['id']+'.png"><div>'+''.join('<figure style="display:inline-block"><img src="'+g['id']+'-'+a+'.gif"><figcaption>'+a+'</figcaption></figure>' for a in g['actions'])+'<img src="'+g['id']+'-transitions.gif"></div>' for g in groups)
    (output/'index.html').write_text('<!doctype html><meta charset="utf-8"><title>动画逐帧与循环审查</title><style>body{background:#15191e;color:#ddd;font:14px sans-serif;max-width:1500px;margin:auto}figure{margin:8px}h2{margin-top:40px}</style><h1>全部新增生物动画预览</h1><p>固定锚点；十字线是同一原点。循环/切换预览不等于Unity实测。特殊亚种共用其物种/形态对应动画。</p>'+links,encoding='utf-8')
    gallery='''<!doctype html><meta charset="utf-8"><title>逐组动画播放审查</title>
<style>body{background:#15191e;color:#ddd;font:16px sans-serif;margin:24px}figure{display:inline-block;margin:12px}img{image-rendering:pixelated;width:288px}select{font-size:16px;padding:8px}summary{cursor:pointer}</style>
<h1>逐组动画播放审查</h1><p>实际资源的离线播放；不代表Unity实测。十字线为同一锚点。</p>
<label>选择生物 <select id="actor" aria-label="选择生物"></select></label><div id="loops"></div><details><summary>查看全部关键帧</summary><img id="contact" style="width:100%;max-width:1440px"></details>
<script>const groups=DATA, names={idle:'静止',walk:'移动',run:'移动',breathing:'呼吸',attack:'攻击',hit:'受击',death:'死亡'};
const select=document.getElementById('actor');for(const g of groups){const o=document.createElement('option');o.value=g.id;o.textContent=g.id;select.append(o)}
function show(){const g=groups.find(x=>x.id===select.value);const area=document.getElementById('loops');area.replaceChildren();for(const a of [...g.actions,'transitions']){const f=document.createElement('figure'),im=document.createElement('img'),c=document.createElement('figcaption');im.src=g.id+'-'+a+'.gif';im.alt=a==='transitions'?'动作切换':names[a];c.textContent=im.alt;f.append(im,c);area.append(f)}document.getElementById('contact').src=g.id+'.png'}select.addEventListener('change',show);show();</script>'''
    gallery=gallery.replace('DATA',json.dumps([dict(id=g['id'],actions=list(g['actions'])) for g in groups]))
    (output/'gallery.html').write_text(gallery,encoding='utf-8')
    print(json.dumps({k:len(report[k]) if isinstance(report[k],list) else report[k] for k in ('registeredAssets','uniqueImages','errors','unreferencedImages','geometryCandidates')}))
    if errors or unreferenced: raise SystemExit(1)


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--output',required=True)
    audit(Path(parser.parse_args().output).resolve())
