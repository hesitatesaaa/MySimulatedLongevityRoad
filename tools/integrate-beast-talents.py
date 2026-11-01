"""Integrate authored ImageGen sheets; never synthesize or recolor artwork."""
import hashlib
import json
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / '.dev-tools/combat-art/beast-talents'

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    rows = []
    for prompt_path in sorted(SOURCE.glob('*.prompt.json')):
        prompt = json.loads(prompt_path.read_text(encoding='utf-8-sig'))
        species = prompt['species']
        source = SOURCE / (species + '.png')
        with Image.open(source) as raw:
            image = raw.convert('RGBA')
        assert image.getextrema()[3][0] == 0, f'{species}: no transparent background'
        atlas = Image.new('RGBA', (768, 1152))
        rects, frames = [], []
        for i in range(25):
            column, row = i % 5, i // 5
            rect = tuple(round(v) for v in (column * image.width / 5, row * image.height / 5,
                         (column + 1) * image.width / 5, (row + 1) * image.height / 5))
            cell = image.crop(rect)
            assert cell.getchannel('A').getbbox(), f'{species}: empty source cell {i}'
            if i < 24:
                cell = cell.resize((160, 160), Image.Resampling.LANCZOS)
                atlas.paste(cell, (i % 4 * 192 + 16, i // 4 * 192 + 16))
                frames.append(dict(index=i, sourceRect=rect, runtimeRect=[i % 4 * 192, (5-i // 4)*192,192,192]))
            else:
                icon = cell.resize((64, 64), Image.Resampling.LANCZOS)
            rects.append(rect)
        atlas_path = ROOT / f'GameResources/effects/BeastTalents/BT_{species}.png'
        icon_path = ROOT / f'GameResources/ui/BeastTalents/BT_{species}.png'
        atlas_path.parent.mkdir(parents=True, exist_ok=True)
        icon_path.parent.mkdir(parents=True, exist_ok=True)
        atlas.save(atlas_path)
        icon.save(icon_path)
        rows.append(dict(species=species, id='BT_'+species, name=prompt['name'], source=source.relative_to(ROOT).as_posix(),
                         sourceSHA256=sha(source), prompt=prompt['prompt'], originalSource=prompt['source'],
                         atlas=atlas_path.relative_to(ROOT).as_posix(), atlasSHA256=sha(atlas_path),
                         icon=icon_path.relative_to(ROOT).as_posix(), iconSHA256=sha(icon_path), frames=frames,
                         iconSourceRect=rects[24], sourceSize=list(image.size), review='pending-Unity'))
    manifest = dict(version=1, generatedWith='image_gen', transparentBackground=True,
                    frameCount=24, phaseFrames=dict(cast=[0,7],active=[8,15],hit=[16,19],dissipate=[20,23]),
                    authoredSpecies=len(rows), species=rows, releaseReady=False)
    (ROOT / 'resources/beast-talents.manifest.json').write_text(json.dumps(manifest, ensure_ascii=True, indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(species=len(rows),frames=len(rows)*24,icons=len(rows))))

if __name__ == '__main__':
    main()
