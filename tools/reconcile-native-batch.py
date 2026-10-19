"""Record a completed built-in batch without installing or approving full actions."""
import argparse
import hashlib
import json
import sys
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'output/actor-redraw'

def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))

def write(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

p = argparse.ArgumentParser()
p.add_argument('--batch')
p.add_argument('--minimal', action='store_true')
p.add_argument('--notes', required=True)
a = p.parse_args()
if not a.minimal and not a.batch:
    p.error('--batch is required unless --minimal uses the shared queue')
stem = WORK / (a.batch or 'balanced-resume-after-quota')
jobs = read(stem.with_suffix('.jobs.json'))
notes = json.load(sys.stdin) if a.notes == '-' else read(Path(a.notes))
review_path = stem.with_suffix('.review.json')
review = read(review_path) if review_path.exists() and not a.minimal else {'frames': [], 'findings': []}
records = {x['jobIndex']: x for x in review['frames']}
evidence_path = WORK / 'child-tasks/completed-queue-items-20261015.json'
evidence = read(evidence_path)
done = {(x['actor'], x['action'], x['index']): x for x in evidence['removedCompletedItems']}
images = []
now = datetime.now(timezone.utc).isoformat()
for i, j in enumerate(jobs):
    output = Path(j['output'])
    proof_path = output.with_suffix('.provenance.json')
    if not output.exists() and not proof_path.exists():
        continue
    proof = read(proof_path)
    source = ROOT / proof['sourceFile']
    sha = digest(source)
    if sha != proof['sourceSha256'] or sha != digest(output) or sha != proof['outputSha256']:
        raise ValueError(f'Native hash mismatch: {output}')
    if proof['prompt'] != j['prompt'] or proof['normalization']['resampling'] != 'none':
        raise ValueError(f'Prompt/normalization mismatch: {output}')
    if str(i) in notes:
        entry = notes[str(i)]
        note_text = entry['notes'] if isinstance(entry, dict) else entry
        records[i] = dict(jobIndex=i, actorId=j['actor'], action=j['action'], frame=j['index'],
            sourceSha256=sha, canvas=proof['normalization']['canvas'], viewedFullNativeImage=True,
            limbConnectionsReviewed=True, extraLimbs=False, clippingDetected=False,
            identityConsistent=True, actionContinuityApproved=False, geometryApproved=False,
            notes=note_text)
        if isinstance(entry, dict):
            for key in ('extraLimbs', 'clippingDetected', 'identityConsistent'):
                if key in entry:
                    records[i][key] = entry[key]
    if a.minimal and i not in records and proof.get('staticReview'):
        records[i] = proof['staticReview']
    if i not in records or records[i]['sourceSha256'] != sha:
        raise ValueError(f'Missing source-bound static review: {output}')
    if a.minimal:
        proof['staticReview'] = records[i]
        write(proof_path, proof)
    images.append(dict(jobIndex=i, actor=j['actor'], action=j['action'], index=j['index'],
        output=str(output), provenance=str(proof_path), sourceSha256=sha,
        canvas=proof['normalization']['canvas'], resampling='none', status='saved', installed=False))
    done[(j['actor'], j['action'], j['index'])] = dict(actor=j['actor'], action=j['action'],
        index=j['index'], output=str(output), provenance=str(proof_path), sourceSha256=sha)

queue_path = WORK / 'balanced-resume-after-quota.jobs.json'
queue = read(queue_path)
remaining = [j for j in queue if (j['actor'], j['action'], j['index']) not in done]
backup = stem.with_suffix('.queue-before-reconcile.json')
if not a.minimal and not backup.exists():
    write(backup, queue)
write(queue_path, remaining)
evidence.update(observedAtUtc=now, removedCompletedItems=list(done.values()), remainingMissingJobs=len(remaining))
if not a.minimal:
    write(evidence_path, evidence)
review.update(reviewedAtUtc=now, scope='static anatomy of generated independent frames only',
    frames=[records[i] for i in sorted(records)], installed=False, finalFullReviewPassed=False)
if not a.minimal:
    write(review_path, review)
    write(stem.with_suffix('.reconciled.json'), dict(queue=str(stem.with_suffix('.jobs.json')),
        observedAtUtc=now, images=images, complete=len(images) == len(jobs), installed=False, finalFullReviewPassed=False))
progress_path = WORK / 'progress.json'
progress = read(progress_path)
forms = read(ROOT / 'resources/actor-redraw.targets.json')['forms']
progress.update(generatedFrames=sum(len(list((WORK / 'frames' / f['id'] / 'main').glob('*.png'))) for f in forms),
    generatedIcons=len(list((WORK / 'icons').glob('*.png'))), releaseReady=False, observedAtUtc=now)
write(progress_path, progress)
control_path = WORK / 'pipeline.control.json'
control = read(control_path)
control.update(remainingMissingJobs=len(remaining), lastCompletedParentBatch=a.batch or 'shared-queue', recordMode='minimal-existing-queue-and-provenance')
write(control_path, control)
print(json.dumps(dict(batch=a.batch, saved=len(images), staticReviewed=len(records),
    remainingMissingJobs=len(remaining), progress=progress), ensure_ascii=True))
