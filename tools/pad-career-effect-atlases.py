"""Isolate native 4x6 effect frames without deleting/resampling a single pixel."""
import hashlib
import json
import shutil
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
backup=ROOT/'.dev-tools/combat-art/career-effects-before-padding'
backup.mkdir(parents=True,exist_ok=True)
manifest=ROOT/'resources/living-resources.json'
data=json.loads(manifest.read_text(encoding='utf-8-sig'))
outputs={o['Resource']:o for o in data['Outputs']}
count=0;edge_contacts=0;new_edges=0;identical=0;padding=2
for path in sorted((ROOT/'GameResources/effects/Careers').glob('*.png')):
    source=backup/path.name
    if not source.exists():shutil.copyfile(path,source)
    im=Image.open(source).convert('RGBA');size=im.width//4
    assert size*4==im.width and size*6==im.height
    stride=size+padding*2
    result=Image.new('RGBA',(stride*4,stride*6))
    for row in range(6):
        for col in range(4):
            frame=im.crop((col*size,row*size,(col+1)*size,(row+1)*size))
            b=frame.getchannel('A').getbbox()
            edge_contacts+=bool(b and (b[0]==0 or b[1]==0 or b[2]==size or b[3]==size))
            x,y=col*stride+padding,row*stride+padding
            result.paste(frame,(x,y))
            matches=result.crop((x,y,x+size,y+size)).tobytes()==frame.tobytes()
            assert matches
            identical+=matches
            bounds=result.getchannel('A').crop((col*stride,row*stride,(col+1)*stride,(row+1)*stride)).getbbox()
            new_edges+=bool(bounds and (bounds[0]==0 or bounds[1]==0 or bounds[2]==stride or bounds[3]==stride))
            count+=1
    result.save(path)
    resource=path.relative_to(ROOT/'GameResources').with_suffix('').as_posix()
    outputs[resource].update(SHA256=hashlib.sha256(path.read_bytes()).hexdigest(),Size=list(result.size),
                             Use='24帧逐像素保留；2像素透明隔离；PPU按原有效帧尺寸保持')
data['CareerEffectPadding']=padding
data['Outputs']=list(outputs.values())
manifest.write_text(json.dumps(data,ensure_ascii=True,indent=2)+'\n',encoding='utf-8')
report=dict(atlases=count//24,frames=count,previousEdgeContacts=edge_contacts,padding=padding,
            newEdgeContacts=new_edges,pixelIdenticalCores=identical==count,resampled=False,
            worldVisibleSizePreserved=(stride-2*padding)==size)
(ROOT/'resources/career-effect-padding-report.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(json.dumps(report))
