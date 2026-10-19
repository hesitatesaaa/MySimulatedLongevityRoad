"""Archive only the six source-bound high-knee frames rejected by the user."""
import hashlib
import json
import shutil
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'output/actor-redraw'

def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))

def write(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')

policy_path = WORK / 'human-walk-revisions-20261015.json'
revision = read(policy_path)
if any(x['status'] != 'queued-after-current-batch' for x in revision['items']):
    raise ValueError('Revision already activated; continue the saved missing queue instead')
old_queue = read(WORK / 'balanced-resume-after-quota.before-20261015-resume.jobs.json')
base_jobs = {(j['actor'], j['action'], j['index']): j for j in old_queue}
plans = []
for item in revision['items']:
    actor, index = item['actorId'], item['index']
    output = Path(item['output'])
    proof_path = output.with_suffix('.provenance.json')
    proof = read(proof_path)
    source = ROOT / proof['sourceFile']
    archive = WORK / 'rejected/20261015-human-low-stride' / actor / f'walk_{index}'
    if archive.exists():
        raise FileExistsError(archive)
    pairs = [(output, archive / f'candidate/walk_{index}.png'),
             (proof_path, archive / f'candidate/walk_{index}.provenance.json'),
             (source, archive / f'originals/walk_{index}.png')]
    for src, dst in pairs:
        if not src.resolve().is_relative_to(ROOT) or not dst.resolve().is_relative_to(WORK / 'rejected'):
            raise ValueError((src, dst))
        if not src.exists():
            raise FileNotFoundError(src)
        if src.suffix == '.png' and hashlib.sha256(src.read_bytes()).hexdigest() != item['sourceSha256']:
            raise ValueError(f'Source changed: {src}')
    key = actor, 'walk', index
    job = dict(base_jobs[key])
    job['references'] = [str(WORK / 'originals' / actor / 'idle_0.png'),
                         str(WORK / 'originals' / actor / 'walk_0.png')]
    job['repairOfSourceSha256'] = item['sourceSha256']
    job['rejectionRecord'] = str(archive / 'archive.json')
    job['prompt'] += (' Image 2 is a broad low-stride pose reference. Newly draw this phase with '
        + ('the NEAR leg forward and FAR leg behind.' if index == 1 else 'the FAR leg forward and NEAR leg behind.')
        + ' Keep the near leg on the foreground layer and the far leg behind it while alternating their forward reach.'
        + ' Keep both boots near the horizontal ground line, with wide front/back separation.'
        + ' Do not lift a knee high, tuck a boot beneath the pelvis or copy the reference bitmap.')
    plans.append((item, key, job, archive, pairs))

queue_path = WORK / 'balanced-resume-after-quota.jobs.json'
queue = read(queue_path)
keys = {(j['actor'], j['action'], j['index']) for j in queue}
if any(key in keys for _, key, _, _, _ in plans):
    raise ValueError('Repair already queued')
evidence_path = WORK / 'child-tasks/completed-queue-items-20261015.json'
evidence = read(evidence_path)
now = datetime.now(timezone.utc).isoformat()
for item, key, job, archive, pairs in plans:
    for src, dst in pairs:
        dst.parent.mkdir(parents=True, exist_ok=True)
        shutil.move(str(src), str(dst))
    record = dict(actorId=item['actorId'], action='walk', index=item['index'],
        sourceSha256=item['sourceSha256'], reason=item['reason'], archivedAtUtc=now,
        files=[{'from': str(src), 'to': str(dst)} for src, dst in pairs], installed=False)
    write(archive / 'archive.json', record)
    item.update(status='archived-queued-for-redraw', archiveRecord=str(archive / 'archive.json'))
    rejected = [x for x in evidence['removedCompletedItems'] if (x['actor'], x['action'], x['index']) == key]
    evidence.setdefault('rejectedCompletedItems', []).extend(rejected)
    evidence['removedCompletedItems'] = [x for x in evidence['removedCompletedItems']
        if (x['actor'], x['action'], x['index']) != key]
queue = [job for _, _, job, _, _ in plans] + queue
write(queue_path, queue)
evidence['remainingMissingJobs'] = len(queue)
write(evidence_path, evidence)
revision['activatedAtUtc'] = now
write(policy_path, revision)
p = WORK / 'progress.json'
progress = read(p)
progress['generatedFrames'] -= len(plans)
progress['observedAtUtc'] = now
write(p, progress)
p = WORK / 'pipeline.control.json'
control = read(p)
control['remainingMissingJobs'] = len(queue)
write(p, control)
print(json.dumps(dict(archived=len(plans), queued=len(plans), remainingMissingJobs=len(queue),
    progress=progress), ensure_ascii=True))
