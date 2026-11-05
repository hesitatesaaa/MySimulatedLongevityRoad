"""Normalize imagegen art and document each icon against its actual spell effect."""
import argparse
import hashlib
import json
import shutil
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'build/icon-upgrade'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--install', action='store_true')
    args = parser.parse_args()
    records = sorted(json.loads((WORK / 'final-generations.json').read_text(encoding='utf-8-sig')), key=lambda x: (not x['id'].startswith('S'), x['id']))
    baseline = json.loads((WORK / 'baseline.json').read_text(encoding='utf-8-sig'))
    output = WORK / 'normalized'
    originals = WORK / 'originals'
    sources = WORK / 'imagegen-originals'
    for directory in (output, originals, sources):
        directory.mkdir(exist_ok=True)
    font = ImageFont.truetype('C:/Windows/Fonts/msyh.ttc', 20)
    small = ImageFont.truetype('C:/Windows/Fonts/msyh.ttc', 17)
    overview = Image.new('RGB', (1200, 5 * 190), (28, 31, 39))
    overview_draw = ImageDraw.Draw(overview)
    assets = []
    pages = []
    for index, record in enumerate(records):
        sid = record['id']
        source = Path(record['path'])
        source_copy = sources / (sid + '.png')
        shutil.copyfile(source, source_copy)
        image = Image.open(source).convert('RGBA')
        pixels = np.array(image)
        # Remove only low-alpha generated halo; all visible artwork is imagegen's.
        pixels[:, :, 3] = np.where(pixels[:, :, 3] >= 128, 255, 0)
        image = Image.fromarray(pixels)
        box = image.getbbox()
        image = image.crop(box)
        image.thumbnail((52, 52), Image.Resampling.NEAREST)
        normalized = Image.new('RGBA', (64, 64))
        normalized.alpha_composite(image, ((64-image.width)//2, (64-image.height)//2))
        normalized = normalized.quantize(colors=64, method=Image.Quantize.FASTOCTREE).convert('RGBA')
        target = output / (sid + '.png')
        normalized.save(target)
        old_path = ROOT / record['target']
        old_copy = originals / (sid + '.png')
        if not old_copy.exists():
            shutil.copyfile(old_path, old_copy)
        old = Image.open(old_copy).convert('RGBA')
        x, y = index % 6 * 200, index // 6 * 190
        overview_draw.text((x+6, y+5), sid if sid.startswith('S') else '特质', font=small, fill='white')
        overview_draw.text((x+6, y+29), record['name'], font=font, fill='white')
        enlarged = normalized.resize((112,112), Image.Resampling.NEAREST)
        overview.paste(enlarged, (x+42,y+65), enlarged)
        page_index, slot = divmod(index, 8)
        if slot == 0:
            pages.append(Image.new('RGB', (1400, 4*230), (28,31,39)))
        page = pages[page_index]
        draw = ImageDraw.Draw(page)
        x, y = slot%2*700, slot//2*230
        draw.text((x+8,y+5),sid+' '+record['name'],font=font,fill='white')
        for offset, title in ((20,'原图标'),(165,'实际特效'),(545,'新版图标')):
            draw.text((x+offset,y+40), title if sid.startswith('S') or offset!=165 else '特质含义',font=small,fill=(185,196,210))
        old.thumbnail((96,96))
        page.paste(old,(x+20,y+95),old)
        effect_path = WORK / 'effect-references' / (sid+'.png')
        if sid.startswith('S'):
            effect = Image.open(effect_path).convert('RGBA')
            effect.thumbnail((360,120))
            page.paste(effect,(x+150,y+85),effect)
        else:
            themes = {'MclslTiandiChuiqing':'天光赐佑 · 青山承泽','MclslPathHuaShen':'紫金神魂 · 化神天资','MclslPathHeDao':'阴阳归一 · 合道圣胎','MclslPathChangSheng':'生机道种 · 长生不息'}
            draw.text((x+165,y+125),themes[sid],font=font,fill=(205,213,228))
        page.paste(enlarged,(x+550,y+80),enlarged)
        assets.append({
            'id':sid,'name':record['name'],'path':record['target'],
            'sha256':sha(target),'previousSha256':baseline['targets'][record['target']],
            'width':64,'height':64,'status':'offline-reviewed',
            'effectReference':str(effect_path.relative_to(ROOT)).replace('\\','/') if sid.startswith('S') else None,
            'effectReferenceSha256':sha(effect_path) if sid.startswith('S') else None,
            'provenance':{'tool':'built-in-imagegen','source':str(source_copy.relative_to(ROOT)).replace('\\','/'),'sourceSha256':sha(source_copy),'prompt':record['prompt'],
                          'processing':'alpha >=128; tight crop; nearest-neighbor fit 52x52; centered 64x64; 64-color quantization'},
            'gameVerified':False,
        })
        if args.install:
            shutil.copyfile(target,old_path)
    overview.save(WORK/'new-icons-overview.png')
    for i,page in enumerate(pages,1):
        page.save(WORK/f'comparison-{i}.png')
    protected = {p:sha(ROOT/p)==value for p,value in baseline['protected'].items()}
    assert all(protected.values()), [p for p,unchanged in protected.items() if not unchanged]
    assert len(assets)==30 and len({a['sha256'] for a in assets})==30
    if args.install:
        for asset in assets:
            assert sha(ROOT/asset['path'])==asset['sha256']
            assert asset['sha256']!=asset['previousSha256']
        (ROOT/'resources/matched-cultivation-icons.manifest.json').write_text(json.dumps({'schemaVersion':1,'assets':assets},ensure_ascii=False,indent=2)+'\n',encoding='utf8')
    report={'installed':args.install,'icons':len(assets),'spellIcons':26,'traitIcons':4,'protectedAssetsUnchanged':len(protected),'gameVerified':False}
    (WORK/'verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
    print(json.dumps(report))


if __name__ == '__main__':
    main()
