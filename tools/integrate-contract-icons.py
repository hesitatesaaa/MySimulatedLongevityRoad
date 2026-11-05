"""Install the five imagegen contract-skill icons and verify protected art."""
import hashlib
import json
import shutil
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'build/contract-rework'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    records = sorted(json.loads((WORK/'icon-generations.json').read_text(encoding='utf8')),key=lambda r:r['id'])
    baseline = json.loads((WORK/'baseline-assets.json').read_text(encoding='utf8'))
    for name in ('icon-originals','imagegen-originals'):
        (WORK/name).mkdir(exist_ok=True)
    board = Image.new('RGB',(1000,5*220),(28,31,39))
    draw = ImageDraw.Draw(board)
    font = ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',20)
    small = ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',17)
    assets=[]
    targets=set()
    for i,record in enumerate(records):
        sid=record['id'];target=ROOT/f'GameResources/ui/CareerSkills/{sid}.png'
        source=Path(record['path']);backup=WORK/'icon-originals'/f'{sid}.png'
        if not backup.exists():
            shutil.copyfile(target,backup)
        raw_copy=WORK/'imagegen-originals'/f'{sid}.png'
        shutil.copyfile(source,raw_copy)
        raw=np.array(Image.open(source).convert('RGBA'))
        raw[:,:,3]=np.where(raw[:,:,3]>=128,255,0)
        art=Image.fromarray(raw);art=art.crop(art.getbbox())
        art.thumbnail((52,52),Image.Resampling.NEAREST)
        icon=Image.new('RGBA',(64,64));icon.alpha_composite(art,((64-art.width)//2,(64-art.height)//2))
        icon=icon.quantize(colors=64,method=Image.Quantize.FASTOCTREE).convert('RGBA')
        icon.save(target)
        y=i*220
        draw.text((10,y+8),sid+' '+record['name'],font=font,fill='white')
        for x,label in ((15,'旧图标'),(190,'实际特效参考'),(790,'新版图标')):
            draw.text((x,y+42),label,font=small,fill=(193,204,216))
        old=Image.open(backup).convert('RGBA');old.thumbnail((112,112));board.paste(old,(15,y+90),old)
        reference=WORK/f'{sid}-effect-reference.png'
        effect=Image.open(reference).convert('RGBA');effect.thumbnail((540,145));board.paste(effect,(185,y+72),effect)
        preview=icon.resize((128,128),Image.Resampling.NEAREST);board.paste(preview,(790,y+78),preview)
        relative=target.relative_to(ROOT).as_posix();targets.add(relative)
        assets.append({'id':sid,'name':record['name'],'path':relative,'width':64,'height':64,'sha256':sha(target),
                       'previousSha256':baseline[relative],'status':'offline-reviewed','gameVerified':False,
                       'provenance':{'tool':'built-in-imagegen','source':raw_copy.relative_to(ROOT).as_posix(),
                       'sourceSha256':sha(raw_copy),'prompt':record['prompt'],'references':[f'build/contract-rework/{sid}-effect-reference.png','GameResources/trait/MclslCombatPassive_spirit_3.png'],
                       'processing':'alpha>=128; crop; nearest-neighbor fit52x52; center64x64;64-color quantization'}})
    unchanged=[p for p in baseline if p not in targets]
    assert all(sha(ROOT/p)==baseline[p] for p in unchanged)
    assert len(assets)==5 and len({a['sha256'] for a in assets})==5
    for a in assets:
        assert a['sha256']!=a['previousSha256']
    board.save(WORK/'active-icons-comparison.png')
    (ROOT/'resources/contract-skill-icons.manifest.json').write_text(json.dumps({'schemaVersion':1,'assets':assets},ensure_ascii=False,indent=2)+'\n',encoding='utf8')
    print(json.dumps({'activeIcons':5,'protectedAssetsUnchanged':len(unchanged),'passiveIconsUnchanged':True}))


if __name__=='__main__':
    main()
