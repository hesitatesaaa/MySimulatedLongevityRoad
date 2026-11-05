from pathlib import Path
from PIL import Image
import json
root=Path(__file__).resolve().parents[1]
m=json.loads((root/'resources/living-resources.json').read_text(encoding='utf-8-sig'))
frames=[]
for group in m['Groups']:
 if not group['Path'].startswith('actors/species/civs/mclsl_xuanhuang/'): continue
 im=Image.open(root/'GameResources'/(group['Atlas']+'.png')).convert('RGBA')
 for f in group['Frames']:
  if f['Name'].endswith(('_head','_item')): continue
  cell=im.crop((f['X'],im.height-f['Y']-f['Height'],f['X']+f['Width'],im.height-f['Y']))
  b=cell.getchannel('A').getbbox()
  assert b, (group['Path'],f['Name'])
  frames.append((b[0]-f['PivotX'],f['Height']-b[3]-f['PivotY'],b[2]-f['PivotX'],f['Height']-b[1]-f['PivotY']))
x0=min(b[0] for b in frames);y0=min(b[1] for b in frames);x1=max(b[2] for b in frames);y1=max(b[3] for b in frames)
scale=51/max(x1-x0,y1-y0);cx=(x0+x1)/2;cy=(y0+y1)/2
for b in frames:
 assert max(abs((b[0]-cx)*scale),abs((b[2]-cx)*scale),abs((b[1]-cy)*scale),abs((b[3]-cy)*scale))<=25.50001
print(f'PASS {len(frames)} frames; shared visible union {(x0,y0,x1,y1)}; scale {scale:.6f}; all body bounds within 85% of native 60px frame')
