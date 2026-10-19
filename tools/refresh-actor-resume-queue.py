"""Remove verified existing outputs from the resume queue without generating images."""
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT/'output/actor-redraw'
path = WORK/'balanced-resume-after-quota.jobs.json'
jobs = json.loads(path.read_text(encoding='utf-8-sig'))
remaining = []
removed = []
for job in jobs:
    output = Path(job['output'])
    if not output.exists():
        remaining.append(job)
        continue
    proof = json.loads(output.with_suffix('.provenance.json').read_text(encoding='utf-8-sig'))
    if hashlib.sha256(output.read_bytes()).hexdigest() != proof['outputSha256']:
        raise ValueError('Candidate output hash mismatch: '+str(output))
    removed.append(dict(actor=job['actor'], action=job['action'], index=job['index']))
report = dict(updatedAtUtc=datetime.now(timezone.utc).isoformat(), remainingJobs=len(remaining),
    verifiedExistingOutputsRemoved=removed, newImagesGeneratedByThisCheck=0)
path.write_text(json.dumps(remaining, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
(WORK/'resume-refresh-20261013.json').write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(json.dumps(dict(remainingJobs=len(remaining), removed=len(removed))))
