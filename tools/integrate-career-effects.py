"""Normalize new imagegen career atlases, rejecting source-edge truncation."""
import argparse
import hashlib
import json
import shutil
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parents[1]
WORK=ROOT/'build/contract-rework/career-effects'
SIZE=192


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--install',action='store_true')
    args=parser.parse_args()
    records=sorted(json.loads((WORK/'generations.json').read_text(encoding='utf8')),key=lambda r:r['id'])
    for name in ('normalized','originals','imagegen-originals','previews'):
        (WORK/name).mkdir(exist_ok=True)
    report=[];assets=[]
    for record in records:
        sid=record['id'];source=Path(record['path'])
        original=Image.open(source).convert('RGBA')
        # Generation can add outer canvas padding; the documented grid itself
        # must retain its four-by-six square-cell aspect ratio.
        assert original.width*6==original.height*4,(sid,original.size)
        cell_size=original.width//4
        assert cell_size*4==original.width and cell_size*6==original.height,(sid,original.size)
        cells=[];bad=[];extent=1
        for j in range(24):
            cell=original.crop((j%4*cell_size,j//4*cell_size,(j%4+1)*cell_size,(j//4+1)*cell_size))
            pixels=np.array(cell);pixels[:,:,3]=np.where(pixels[:,:,3]<32,0,pixels[:,:,3]);cell=Image.fromarray(pixels)
            core=pixels[:,:,3]>=128
            edges=int(core[:2,:].sum()+core[-2:,:].sum()+core[:,:2].sum()+core[:,-2:].sum())
            if edges>2:bad.append({'frame':j,'opaqueSourceEdgePixels':edges})
            box=cell.getbbox()
            if box:
                extent=max(extent,*[abs(v-cell_size/2)*2 for v in box])
            cells.append(cell)
        report.append({'id':sid,'sourceSize':list(original.size),'sourceClipping':bad})
        if bad:continue
        # One uniform scale and exact cell-center pivot for the entire spell;
        # never recenter individual frames or crop to individual visible bounds.
        scale=min(SIZE/cell_size,132/extent)
        side=max(1,round(cell_size*scale));atlas=Image.new('RGBA',(768,1152));frames=[];margins=[]
        for j,cell in enumerate(cells):
            small=cell.resize((side,side),Image.Resampling.LANCZOS)
            frame=Image.new('RGBA',(SIZE,SIZE));frame.alpha_composite(small,((SIZE-side)//2,(SIZE-side)//2))
            box=frame.getbbox()
            if box:margins.append(min(box[0],box[1],SIZE-box[2],SIZE-box[3]))
            atlas.alpha_composite(frame,(j%4*SIZE,j//4*SIZE));frames.append(frame)
        assert margins and min(margins)>=27,(sid,margins)
        target=WORK/'normalized'/f'{sid}.png';atlas.save(target)
        resource=ROOT/f'GameResources/effects/Careers/{sid}.png';backup=WORK/'originals'/f'{sid}.png'
        if not backup.exists():shutil.copyfile(resource,backup)
        raw=WORK/'imagegen-originals'/f'{sid}.png';shutil.copyfile(source,raw)
        preview=[]
        for frame in frames:
            background=Image.new('RGBA',(SIZE,SIZE),(28,31,39,255));background.alpha_composite(frame);preview.append(background.convert('RGB'))
        preview[0].save(WORK/'previews'/f'{sid}.gif',save_all=True,append_images=preview[1:],duration=65,loop=0)
        assets.append({'id':sid,'name':record['name'],'path':resource.relative_to(ROOT).as_posix(),
                       'width':768,'height':1152,'frameSize':192,'columns':4,'frames':24,
                       'phases':{'cast':[0,8],'travelOrArea':[8,8],'hit':[16,4],'cancel':[20,4]},
                       'minimumTransparentMarginPixels':min(margins),'sha256':sha(target),'previousSha256':sha(backup),
                       'status':'offline-reviewed','gameVerified':False,
                       'provenance':{'tool':'built-in-imagegen','source':raw.relative_to(ROOT).as_posix(),'sourceSha256':sha(raw),
                       'prompt':record['prompt'],'processing':'alpha<32 cleared; fixed full-cell pivot; one uniform scale for all24frames; no per-frame crop/recenter'}})
    (WORK/'source-geometry-report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
    (WORK/'normalized-manifest.json').write_text(json.dumps({'schemaVersion':1,'assets':assets},ensure_ascii=False,indent=2),encoding='utf8')
    font=ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',18)
    for career in sorted({record['career'] for record in records}):
        page=Image.new('RGB',(1000,5*180),(28,31,39));draw=ImageDraw.Draw(page)
        for asset in [a for a in assets if a['id'].startswith('CP_'+career+'_')]:
            row=int(asset['id'].split('_')[-1][0])-1
            draw.text((8,row*180+5),asset['id']+' '+asset['name'],font=font,fill='white')
            atlas=Image.open(WORK/'normalized'/f"{asset['id']}.png").convert('RGBA')
            for k,j in enumerate([5,11,17,21]):
                frame=atlas.crop((j%4*SIZE,j//4*SIZE,(j%4+1)*SIZE,(j//4+1)*SIZE))
                frame.thumbnail((140,140));page.paste(frame,(k*220+50,row*180+35),frame)
        page.save(WORK/'previews'/f'{career}-new-effects.png')
    if args.install:
        assert len(assets)==30,'Do not ship a partial effect replacement'
        for asset in assets:
            shutil.copyfile(WORK/'normalized'/f"{asset['id']}.png",ROOT/asset['path'])
        (ROOT/'resources/career-effects.manifest.json').write_text(json.dumps({'schemaVersion':1,'assets':assets},ensure_ascii=False,indent=2)+'\n',encoding='utf8')
        manifest=ROOT/'resources/living-resources.json';data=manifest.read_text(encoding='utf8')
        data=data.replace('"CareerEffectPadding": 2','"CareerEffectPadding": 0')
        assert data.count('"CareerEffectPadding": 0')==1
        manifest.write_text(data,encoding='utf8')
    print(json.dumps({'generated':len(records),'accepted':len(assets),'rejected':[x['id'] for x in report if x['sourceClipping']],'installed':args.install}))


if __name__=='__main__':main()
