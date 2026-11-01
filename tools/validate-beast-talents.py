"""Validate independent talent assets and authored provenance; offline only."""
import hashlib
import json
import re
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def validate():
    catalog=(ROOT/'code/MySimulatedLongevityRoad/Data/MclslBeastTalentCatalog.cs').read_text(encoding='utf-8')
    expected=set(re.findall(r'new\("([a-z]+)",',catalog))
    manifest=json.loads((ROOT/'resources/beast-talents.manifest.json').read_text(encoding='utf-8'))
    assert len(expected)==21 and len(manifest['species'])==21
    assert {r['species'] for r in manifest['species']}==expected
    shared=set()
    for folder in ('GameResources/effects/Spells','GameResources/effects/Careers','GameResources/ui/Spells','GameResources/ui/CareerSkills'):
        shared.update(sha(p) for p in (ROOT/folder).rglob('*.png'))
    unique=set()
    for row in manifest['species']:
        assert row['prompt'] and row['sourceSHA256']
        source=ROOT/row['source']
        if source.exists():
            assert sha(source)==row['sourceSHA256']
        for kind,size in (('atlas',(768,1152)),('icon',(64,64))):
            path=ROOT/row[kind]
            digest=sha(path)
            assert digest==row[kind+'SHA256'] and digest not in shared and digest not in unique
            unique.add(digest)
            with Image.open(path) as image:
                assert image.size==size and image.convert('RGBA').getextrema()[3][0]==0
                if kind=='atlas':
                    for i in range(24):
                        cell=image.crop((i%4*192,i//4*192,(i%4+1)*192,(i//4+1)*192)).convert('RGBA')
                        bounds=cell.getchannel('A').getbbox()
                        assert bounds and min(bounds[:2])>0 and max(bounds[2:])<192
        assert len(row['frames'])==24 and len({tuple(f['sourceRect']) for f in row['frames']})==24
    print(json.dumps(dict(species=21,frames=504,icons=21,uniqueArtwork=True,offlineOnly=True,unityRetested=False)))

if __name__=='__main__':
    validate()
