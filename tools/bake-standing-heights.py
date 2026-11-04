"""Refresh offline standing-height metadata after changing native actor artwork."""
from pathlib import Path
import json
from PIL import Image
root=Path(__file__).resolve().parents[1]
p=root/'resources/actor-frames.manifest.json'
m=json.loads(p.read_text(encoding='utf-8-sig'));count=0
for group in m['groups']:
    native=group.get('nativeResourcePath')
    if not native:continue
    image=root/'NativeActorResources'/native/'main/idle_0.png'
    alpha=Image.open(image).convert('RGBA').getchannel('A')
    box=alpha.point(lambda a:255 if a>8 else 0).getbbox()
    assert box is not None,image
    group['standingVisibleHeight']=box[3]-box[1];count+=1
p.write_text(json.dumps(m,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Standing-height metadata updated:',count)