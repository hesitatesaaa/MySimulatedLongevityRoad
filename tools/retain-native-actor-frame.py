"""Retain one built-in generated PNG without resampling or re-encoding."""
import argparse
import hashlib
import json
import shutil
from datetime import datetime, timezone
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'output' / 'actor-redraw'

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', required=True)
    parser.add_argument('--job-index', type=int, required=True)
    parser.add_argument('--jobs', default='balanced-resume-after-quota.jobs.json')
    args = parser.parse_args()
    jobs_path = (WORK / args.jobs).resolve()
    jobs_path.relative_to(WORK.resolve())
    jobs = json.loads(jobs_path.read_text(encoding='utf-8-sig'))
    job = jobs[args.job_index]
    source = Path(args.source).resolve()
    output = Path(job['output']).resolve()
    output.relative_to(WORK.resolve())
    original = WORK / 'originals' / job['actor'] / f"{job['action']}_{job['index']}.png"
    proof = output.with_suffix('.provenance.json')
    if any(p.exists() for p in (output, original, proof)):
        raise ValueError('Existing frame/original/provenance: refuse overwrite')
    with Image.open(source) as check:
        check.verify()
    with Image.open(source) as im:
        if im.format != 'PNG' or 'A' not in im.getbands():
            raise ValueError('Expected transparent PNG with alpha')
        canvas = list(im.size)
        if im.getchannel('A').getextrema()[0] != 0:
            raise ValueError('PNG lacks transparent pixels')
    prompt = (Path(job['promptFile']).read_text(encoding='utf-8-sig') if job.get('promptFile') else job['prompt']).strip()
    if prompt != job['prompt'].strip():
        raise ValueError('Queued prompt differs from retained prompt file')
    record = dict(actorId=job['actor'], action=job['action'], index=job['index'],
        artDirectionRevision=job['artDirectionRevision'], method='built-in-imagegen-independent-frame',
        sourceFile=original.relative_to(ROOT).as_posix(), sourceSha256=digest(source),
        outputFile=output.relative_to(ROOT).as_posix(), outputSha256=digest(source),
        prompt=prompt, references=job['references'], createdAtUtc=datetime.now(timezone.utc).isoformat(),
        normalization=dict(resampling='none', sourceCanvas=canvas, canvas=canvas, scale=1,
            cropped=False, spritesheetSlicing=False, byteIdenticalToGeneratedSource=True),
        nativeResolution=True, pendingNativeRuntimeIntegration=True,
        review=dict(identity=False, pixelEdges=False, anchor=False, continuity=False))
    for destination in (original, output):
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, destination)
        if digest(destination) != record['sourceSha256']:
            raise ValueError('Copied frame hash differs from generated source')
    proof.write_text(json.dumps(record, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(output=str(output), canvas=canvas, sha256=digest(output),
        resampling='none', installed=False), ensure_ascii=False))

if __name__ == '__main__':
    main()
