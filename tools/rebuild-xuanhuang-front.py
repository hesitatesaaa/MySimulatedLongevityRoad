"""Import front-facing originals into isolated padded native building frames."""
import argparse
import hashlib
import json
import math
import shutil
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
IDS = ['tent_human'] + [f'house_human_{i}' for i in range(6)] + [f'hall_human_{i}' for i in range(3)] + [
    'library_human','market_human','barracks_human','windmill_human_0','windmill_human_1',
    'docks_human','fishing_docks_human','watch_tower_human','temple_human','bonfire',
    'statue','well','stockpile','mine','training_dummy']

# The generated source grid has detached fragments above these three poses.
# Clip only the transparent top margin plus those fragments when defining the
# Unity sprite rectangle. Keep the atlas pixels and the bottom-based pivot.
SPRITE_TOP_CROP = {
    ('watch_tower_human', 'ruin_0'): 44,
    ('temple_human', 'construction_0'): 57,
    ('temple_human', 'ruin_0'): 57,
}

def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def cells(path):
    image = Image.open(path).convert('RGBA')
    # Reviewed source grid: generated art has unequal row gutters. Explicit
    # rectangles avoid assuming width/5 by height/5 is an authored frame.
    # Individual reviewed bounds: AI atlas gutters are not a cutting grid.
    # In particular column four starts before width*4/5, and the watchtower
    # roof starts above the nominal row boundary. Uniform slicing clips it
    # and imports neighboring pillars into two house frames.
    bounds=[
        (0,0,280,220),(280,0,560,220),(560,0,830,220),(830,0,1095,220),(1095,0,1402,220),
        (0,220,285,447),(285,220,570,447),(570,220,830,447),(830,220,1095,447),(1095,220,1402,447),
        (0,447,283,660),(283,447,570,660),(570,447,845,647),(900,447,1090,660),(1160,447,1402,665),
        (0,660,283,895),(283,660,575,906),(610,647,805,895),(805,647,1135,895),(1135,665,1402,895),
        (0,895,285,1122),(285,906,555,1122),(555,930,805,1122),(805,895,1135,1122),(1135,895,1402,1122)]
    result = []
    for i in range(25):
        rect=tuple(round(v*(image.width/1402 if j%2==0 else image.height/1122)) for j,v in enumerate(bounds[i]))
        cell=image.crop(rect)
        box=cell.getchannel('A').getbbox()
        if not box: raise ValueError(f'Empty source cell {i}: {path}')
        result.append(cell.crop(box))
    return result

def fit(image,w,h):
    ratio=min(w/image.width,h/image.height)
    return image.resize((max(1,round(image.width*ratio)),max(1,round(image.height*ratio))),Image.Resampling.NEAREST)

def blade(draw,cx,cy,radius,angle):
    for k in range(4):
        a=angle+k*math.pi/2
        def point(r,t):
            return (round(cx+math.cos(a)*r-math.sin(a)*t),round(cy+math.sin(a)*r+math.cos(a)*t))
        draw.polygon([point(3,-1),point(radius,-3),point(radius,2),point(3,2)],fill='#d8c191',outline='#604a31')
        draw.line([point(0,0),point(radius,0)],fill='#a77d44',width=1)
    draw.ellipse((cx-2,cy-2,cx+2,cy+2),fill='#bd9142',outline='#503c2b')

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--main',required=True,type=Path)
    parser.add_argument('--ruins',required=True,type=Path)
    parser.add_argument('--construction',required=True,type=Path)
    args=parser.parse_args()
    manifest=ROOT/'resources/living-resources.json'
    data=json.loads(manifest.read_text(encoding='utf-8-sig'))
    backup=ROOT/'.dev-tools/combat-art/xuanhuang-before-front'
    if not backup.exists():
        backup.mkdir()
        shutil.copyfile(manifest,backup/'living-resources.json')
        for p in (ROOT/'GameResources/buildings/Xuanhuang').glob('*.png'): shutil.copyfile(p,backup/p.name)
    original=json.loads((backup/'living-resources.json').read_text(encoding='utf-8-sig'))
    native=data['NativeArchitecture']
    groups={g['Path']:g for g in data['Groups']}
    outputs={o['Resource']:o for o in data['Outputs']}
    pictures={key:cells(value) for key,value in vars(args).items()}
    contact=Image.new('RGBA',(800,850),'#263439')
    inventory=[]
    for i,id in enumerate(IDS):
        building=next(b for b in native['buildings'] if b['id']==id)
        if id in ('bonfire','stockpile'):
            resource='buildings/Xuanhuang/'+id
            groups.pop('buildings/civ_main/mclsl_xuanhuang/'+id,None)
            outputs.pop(resource,None)
            file=ROOT/'GameResources'/(resource+'.png')
            if file.exists():shutil.move(str(file),str(backup/(id+'-unused-front.png')))
            inventory.append(dict(id=id,redrawn=False,new=False,previouslyShared=True,sharedByUserRequest=True,
                                  upgrade=building['upgrade'],previous=building['previous'],path=building['path']))
            continue
        source=next(r for r in native['resources'] if r['path']==building['path'])
        path='buildings/civ_main/mclsl_xuanhuang/'+id
        old=next((g for g in original['Groups'] if g['Path']==path),None)
        reference=next(f for f in source['frames'] if f['name'].startswith('main'))
        # Match native physical footprint. Extra margins belong to the rect,
        # and are added equally to its pivot (world placement does not shift).
        w=round(reference['width']*4)+16;h=round(reference['height']*4)+16
        pivot_x=reference['pivotX']*4+8;pivot_y=reference['pivotY']*4+8
        frames=[];images=[]
        main=fit(pictures['main'][i],w-16,h-round(pivot_y)-8)
        base=Image.new('RGBA',(w,h))
        base.alpha_composite(main,((w-main.width)//2,h-round(pivot_y)-main.height))
        for f in source['frames']:
            name=f['name'];frame=Image.new('RGBA',(w,h))
            if name.startswith('main') or name.startswith('disabled'):
                frame=base.copy()
                if id.startswith('windmill') and name.startswith('main'):
                    d=ImageDraw.Draw(frame)
                    # Native four-frame loop spans one quarter revolution of
                    # four identical sails. 0,22.5,45,67.5 -> 90=0: no reverse jump.
                    blade(d,w//2,h-round(pivot_y)-round(main.height*.58),min(w//2-10,round(main.height*.36)),int(name.rsplit('_',1)[-1])*math.pi/8)
                elif id=='bonfire' and name.startswith('main'):
                    d=ImageDraw.Draw(frame);k=int(name.rsplit('_',1)[-1]);cx=w//2;cy=h-round(pivot_y)-round(main.height*.65)
                    d.polygon([(cx-7,cy),(cx-4,cy-10-k%2*2),(cx,cy-6),(cx+3,cy-15+k%2*2),(cx+7,cy)],fill='#f6a734')
            elif name.startswith('mini'):
                mini=fit(pictures['main'][i],max(4,round(f['width']*4)),max(4,round(f['height']*4)))
                frame.alpha_composite(mini,((w-mini.width)//2,h-round(pivot_y)-mini.height))
            else:
                kind='ruins' if name.startswith('ruin') else 'construction'
                pose=fit(pictures[kind][i],w-16,h-round(pivot_y)-8)
                frame.alpha_composite(pose,((w-pose.width)//2,h-round(pivot_y)-pose.height))
            images.append(frame)
            frames.append(dict(Name=name,X=(len(images)-1)*w,Y=0,Width=w,Height=h-SPRITE_TOP_CROP.get((id,name),0),
                               PivotX=pivot_x,PivotY=pivot_y,PixelsPerUnit=4))
        atlas=Image.new('RGBA',(w*len(images),h))
        for j,im in enumerate(images): atlas.alpha_composite(im,(j*w,0))
        resource='buildings/Xuanhuang/'+id
        file=ROOT/'GameResources'/(resource+'.png');atlas.save(file)
        groups[path]=dict(Path=path,Atlas=resource,Frames=frames)
        outputs[resource]=dict(Resource=resource,SHA256=sha(file),Size=list(atlas.size),Use='正向独立建筑；原生帧名；固定枢轴；透明隔离边界')
        shown=next(images[j] for j,f in enumerate(frames) if f['Name'].startswith('main'))
        shown.thumbnail((150,135));contact.alpha_composite(shown,(i%5*160+(160-shown.width)//2,i//5*170))
        ImageDraw.Draw(contact).text((i%5*160+3,i//5*170+140),id,fill='white')
        inventory.append(dict(id=id,redrawn=old is not None,new=old is None,previouslyShared=old is None,
                              upgrade=building['upgrade'],previous=building['previous'],path=path,frames=[f['Name'] for f in frames]))
    data['Groups']=list(groups.values());data['Outputs']=list(outputs.values())
    data['FrontBuildingSources']={key:dict(Path=value.resolve().relative_to(ROOT).as_posix(),SHA256=sha(value)) for key,value in vars(args).items()}
    manifest.write_text(json.dumps(data,ensure_ascii=True,indent=2)+'\n',encoding='utf-8')
    (ROOT/'resources/xuanhuang-building-inventory.json').write_text(json.dumps(inventory,ensure_ascii=True,indent=2)+'\n',encoding='utf-8')
    contact.save(ROOT/'build/xuanhuang-front-review.png')
    print(json.dumps(dict(buildings=len(inventory),redrawn=sum(x['redrawn'] for x in inventory),added=sum(x['new'] for x in inventory))))

if __name__=='__main__':main()
