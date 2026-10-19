"""Persist one independent generated image immediately; no art edits or approval."""
import argparse,hashlib,json,shutil
from pathlib import Path
from datetime import datetime,timezone
ROOT=Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser();p.add_argument('--id',required=True);p.add_argument('--source',type=Path,required=True);p.add_argument('--jobs',required=True)
a=p.parse_args();jobs=json.loads((ROOT/a.jobs).read_text(encoding='utf-8-sig'));job=next(x for x in jobs if x['id']==a.id)
if not a.id.replace('_','').isalnum():raise ValueError('Invalid resource ID')
original=ROOT/'output/beast-overhaul/generated-originals'/(a.id+'.png')
original.parent.mkdir(parents=True,exist_ok=True)
if original.exists():raise FileExistsError(original)
shutil.copy2(a.source,original)
receipt=dict(id=a.id,tool='built-in-imagegen',source=str(original.relative_to(ROOT)).replace('\\','/'),sourceSha256=hashlib.sha256(original.read_bytes()).hexdigest(),prompt=job['prompt'],generatedAtUtc=datetime.now(timezone.utc).isoformat(),status='candidate-unreviewed')
receipts=ROOT/'output/beast-overhaul/receipts';receipts.mkdir(exist_ok=True)
(receipts/(a.id+'.json')).write_text(json.dumps(receipt,ensure_ascii=True,indent=2),encoding='utf-8')
print(json.dumps(dict(id=a.id,saved=True,status='candidate-unreviewed'),ensure_ascii=True))
