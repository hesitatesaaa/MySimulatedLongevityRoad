"""Install explicitly reviewed stage atlases; runtime QA remains pending."""
import argparse, hashlib, importlib.util, json, shutil
from datetime import datetime, timezone
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('inspection',ROOT/'tools/beast-resource-inspection.py')
inspection=importlib.util.module_from_spec(spec);spec.loader.exec_module(inspection)
p=argparse.ArgumentParser();p.add_argument('--ids',nargs='+',required=True);p.add_argument('--revision',type=int,required=True)
a=p.parse_args();path=ROOT/'resources/beast-spell-effects.manifest.json'
m=json.loads(path.read_text(encoding='utf-8-sig'));ready=[];seen=set()
for entry in m['assets']:
 if (ROOT/entry['path']).exists():seen.add(inspection.inspect_png(ROOT/entry['path'],(entry['width'],entry['height']),entry['frames']))
for identity in a.ids:
 entry=next(x for x in m['assets'] if x['id']==identity)
 source=ROOT/f'output/beast-overhaul/spell-candidates/{identity}-v{a.revision}.png'
 target=ROOT/entry['path']
 if target.exists():raise FileExistsError(target)
 digest=inspection.inspect_png(source,(entry['width'],entry['height']),entry['frames'])
 if digest in seen:raise ValueError('Duplicate visible stage: '+identity)
 seen.add(digest)
 receipt=json.loads((ROOT/f'output/beast-overhaul/receipts/{identity}-v{a.revision}.json').read_text())
 ready.append((entry,source,target,receipt))
now=datetime.now(timezone.utc).isoformat()
for entry,source,target,receipt in ready:
 target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target)
 entry['status']='approved';entry['sha256']=hashlib.sha256(target.read_bytes()).hexdigest()
 entry['review']=dict(kind='offline-stage-atlas-review',result='passed',reviewedAtUtc=now,
                      checked=['spell-identity','phase-sequence','cell-boundaries','direction','pivot','alpha'],unityValidation='pending')
 entry['provenance']={key:receipt[key] for key in ('tool','source','sourceSha256','prompt','references','revision')}
 for field in ('parts','assembly'):
  if field in receipt:entry['provenance'][field]=receipt[field]
 entry['provenance']['conversion']=dict(tool='System.Drawing',frameSize=192,columns=4,paddingPixels=16,scale='fixed-whole-cell')
 entry['playback']=dict(mode='once-over-phase-duration',pivot=[0.5,0.5],frameOrder='left-to-right-top-to-bottom')
path.write_text(json.dumps(m,ensure_ascii=True,indent=2),encoding='utf-8')
sp=ROOT/'resources/beast-overhaul-status.json';s=json.loads(sp.read_text(encoding='utf-8-sig'))
s['installedSpellStageAtlases']=sum((ROOT/x['path']).exists() and x.get('status')=='approved' for x in m['assets'])
s['missingSpellStageAtlases']=sum(not (ROOT/x['path']).exists() for x in m['assets'])
s['generatedSpellStageCandidates']=sum((ROOT/f"output/beast-overhaul/generated-spell-originals/{x['id']}.png").exists() or any((ROOT/'output/beast-overhaul/generated-spell-originals').glob(x['id']+'-v*.png')) for x in m['assets'])
s['generatedSpellStageRevisions']=len(list((ROOT/'output/beast-overhaul/generated-spell-originals').glob('*-v*.png')))
s['generatedImages']=s['generatedItemImages']+s['generatedSpellStageCandidates'];s['releaseReady']=False
sp.write_text(json.dumps(s,ensure_ascii=True,indent=2),encoding='utf-8')
print(json.dumps(dict(installed=a.ids,remaining=s['missingSpellStageAtlases'],unityValidation='pending')))
