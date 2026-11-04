"""Inspect real atlas geometry; edge contact is a review flag, not proof of bleed."""
import argparse
import json
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]

def audit():
    data=json.loads((ROOT/'resources/living-resources.json').read_text(encoding='utf-8-sig'))
    failures=[];edges=[];frames=0;files=set();buildings=[]
    for group in data['Groups']:
        file=ROOT/'GameResources'/(group['Atlas']+'.png');files.add(str(file))
        with Image.open(file) as raw:
            image=raw.convert('RGBA')
        rects=[]
        for f in group['Frames']:
            frames+=1
            x,y,w,h=[f[k] for k in ('X','Y','Width','Height')]
            rect=(x,image.height-y-h,x+w,image.height-y)
            key=group['Path']+'/'+f['Name']
            if not (0<=x<x+w<=image.width and 0<=y<y+h<=image.height):failures.append([key,'outside-atlas'])
            if not (0<=f['PivotX']<=w and 0<=f['PivotY']<=h and f['PixelsPerUnit']>0):failures.append([key,'invalid-pivot-ppu'])
            a=image.crop(rect).getchannel('A');box=a.getbbox()
            if box is None:failures.append([key,'empty-frame'])
            elif box[0]==0 or box[1]==0 or box[2]==w or box[3]==h:edges.append(key)
            for other,name in rects:
                if max(rect[0],other[0])<min(rect[2],other[2]) and max(rect[1],other[1])<min(rect[3],other[3]):failures.append([key,'overlapping-rect',name])
            rects.append((rect,f['Name']))
        if group['Path'].startswith('buildings/'):
            buildings.append(dict(path=group['Path'],frames=len(rects),edgeContacts=sum(k.startswith(group['Path']+'/') for k in edges)))
    effect_frames=0
    for p in (ROOT/'GameResources/effects/Careers').glob('*.png'):
        with Image.open(p) as im:
            a=im.convert('RGBA').getchannel('A');w=im.width//4;h=im.height//6
            if im.width%4 or im.height%6 or w!=h:failures.append([p.name,'invalid-effect-grid'])
            for row in range(6):
                for col in range(4):
                    effect_frames+=1;b=a.crop((col*w,row*h,(col+1)*w,(row+1)*h)).getbbox()
                    if not b:failures.append([p.name,row,col,'empty-effect-frame'])
                    elif b[0]==0 or b[1]==0 or b[2]==w or b[3]==h:edges.append(f'{p.name}/{row}/{col}')
    # Decode every current runtime PNG, including independent actor frames and effects.
    pngs=list((ROOT/'GameResources').rglob('*.png'))+list((ROOT/'NativeActorResources').rglob('*.png'))
    for p in pngs:
        with Image.open(p) as im:im.load()
    return dict(runtimeImagesDecoded=len(pngs),atlases=len(files),mappedFrames=frames,effectFrames=effect_frames,
                geometryErrors=failures,edgeContactsForReview=edges,buildings=buildings,
                scope='Offline geometry and PNG integrity only; no Unity visual/performance claim')

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--output',type=Path,required=True);args=parser.parse_args()
    result=audit();args.output.parent.mkdir(parents=True,exist_ok=True)
    args.output.write_text(json.dumps(result,ensure_ascii=True,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({k:len(v) if isinstance(v,list) else v for k,v in result.items() if k!='buildings'}))
    raise SystemExit(bool(result['geometryErrors']))
