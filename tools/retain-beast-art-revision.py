"""Retain an independently generated, versioned art candidate and its prompt."""
import argparse, hashlib, json, shutil
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
p = argparse.ArgumentParser()
p.add_argument('--id', required=True)
p.add_argument('--source', type=Path, required=True)
p.add_argument('--jobs', required=True)
p.add_argument('--revision', type=int, required=True)
a = p.parse_args()
job = next(x for x in json.loads((ROOT/a.jobs).read_text(encoding='utf-8-sig')) if x['id']==a.id)
if not a.id.replace('_','').isalnum() or a.revision < 1:
    raise ValueError('Invalid revision identity')
folder = 'generated-originals' if job['kind']=='icon' else 'generated-spell-originals'
target = ROOT/'output/beast-overhaul'/folder/f'{a.id}-v{a.revision}.png'
if target.exists():
    raise FileExistsError(target)
target.parent.mkdir(parents=True, exist_ok=True)
shutil.copy2(a.source, target)
receipt = dict(id=a.id, revision=a.revision, kind=job['kind'], tool='built-in-imagegen',
               source=str(target.relative_to(ROOT)).replace('\\','/'),
               sourceSha256=hashlib.sha256(target.read_bytes()).hexdigest(),
               prompt=job['prompt'], references=job['references'],
               generatedAtUtc=datetime.now(timezone.utc).isoformat(), status='candidate-unreviewed')
receipt_path = ROOT/'output/beast-overhaul/receipts'/f'{a.id}-v{a.revision}.json'
receipt_path.write_text(json.dumps(receipt, ensure_ascii=True, indent=2), encoding='utf-8')
print(json.dumps(dict(id=a.id, revision=a.revision, saved=True)))
