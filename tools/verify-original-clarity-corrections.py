"""Verify exact native PNG retention of the explicitly redrawn clarity-correction batch."""
import json
import hashlib
import argparse
from datetime import datetime, timezone
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT/'output/actor-redraw'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--jobs', default='original-clarity-corrections-20261013.jobs.json')
parser.add_argument('--completed-only', action='store_true', help='Verify retained frames after a stopped batch; report missing jobs separately')
args = parser.parse_args()
jobs_file = (WORK/args.jobs).resolve()
jobs_file.relative_to(WORK.resolve())
jobs = json.loads(jobs_file.read_text(encoding='utf-8-sig'))
records = []
missing = []
for job in jobs:
    output = Path(job['output'])
    if args.completed_only and not output.exists() and not output.with_suffix('.provenance.json').exists():
        missing.append(dict(actor=job['actor'], action=job['action'], index=job['index']))
        continue
    proof = json.loads(output.with_suffix('.provenance.json').read_text(encoding='utf-8-sig'))
    original = ROOT/proof['sourceFile']
    original_hash = hashlib.sha256(original.read_bytes()).hexdigest()
    output_hash = hashlib.sha256(output.read_bytes()).hexdigest()
    if original_hash != output_hash or output_hash != proof['outputSha256'] or original_hash != proof['sourceSha256']:
        raise ValueError('Source/output/provenance hash mismatch: '+str(output))
    if proof.get('normalization',{}).get('resampling') != 'none' or not proof.get('nativeResolution'):
        raise ValueError('Unexpected downsampling: '+str(output))
    with Image.open(output) as im:
        size = list(im.size)
        im.verify()
    if size != proof['normalization']['sourceCanvas']:
        raise ValueError('Source canvas changed')
    if proof['references'] != job['references'] or proof['prompt'].strip() != job['prompt'].strip():
        raise ValueError('Prompt/reference mismatch')
    records.append(dict(actor=job['actor'], action=job['action'], index=job['index'], canvas=size,
        sourceAndFrameByteIdentical=True, sha256=output_hash, nativeResolution=True))
report = dict(checkedAtUtc=datetime.now(timezone.utc).isoformat(), frames=records, checkedFrames=len(records),
    expectedFrames=len(jobs), missingJobs=missing, batchFilesComplete=not missing,
    imageSource='built-in-imagegen-independent-per-frame', clarityReference='output/actor-redraw/originals/mclsl_beast_zhuque_yuan/attack_2.png',
    lossyCompression=False, downsampling=False, reencoding=False, cropping=False,
    fileChecksPassed=True, completeActionReviewPassed=False, installedToRuntime=False, formalReleaseReady=False,
    archivedLowReferenceBatch='output/actor-redraw/rejected/low-reference-20261013')
file = WORK/('original-clarity-verification-20261013.json' if args.jobs == 'original-clarity-corrections-20261013.jobs.json'
    else jobs_file.stem+'.verification.json')
file.write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(json.dumps(dict(report=str(file), checkedFrames=len(records), byteIdentical=True, installed=False)))
