"""Promote latest non-rejected, source-bound repairs under the explicit user policy."""
import argparse
import copy
import hashlib
import json
import re
import shutil
from datetime import datetime, timezone
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'output/actor-redraw'


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def within(path):
    result = (ROOT / path).resolve()
    result.relative_to(ROOT)
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--apply', action='store_true')
    args = parser.parse_args()
    assert read(ROOT / 'resources/0.6.0-release-policy.json')['candidateSelection'] == 'latest-not-rejected'
    audit = read(WORK / 'generated-images-audit.json')['files']
    generated = {x['sha256']: Path(x['file']) for x in audit}
    manifest = read(ROOT / 'resources/actor-frames.manifest.json')
    installed = {(g['actorId'], f['name']): f for g in manifest['groups'] for a in g['actions'].values() for f in a['frames']}
    options, rejected, skipped = {}, set(), []
    candidates = []
    for path in (WORK / 'repair-candidates').rglob('*.json'):
        value = read(path)
        status = str(value.get('status', '')).lower()
        if 'reject' in status or 'rejected' in path.parts:
            def reject(item):
                if isinstance(item, dict):
                    for k, v in item.items():
                        if k in ('candidateSha256', 'generatedSourceSha256') and isinstance(v, str):
                            rejected.add(v.lower())
                        elif isinstance(v, (dict, list)):
                            reject(v)
                elif isinstance(item, list):
                    for child in item:
                        reject(child)
            reject(value)
            continue
        actor = value.get('actorId') or value.get('formId')
        frame = str(value.get('frame', ''))
        if frame.isdigit():
            frame = str(value.get('action', '')) + '_' + frame
        digest = value.get('candidateSha256') or value.get('outputSha256')
        if (actor, frame) not in installed or not digest or digest not in generated:
            continue
        candidates.append(dict(actorId=actor, frame=frame, sha256=digest,
            file=str(generated[digest]), originalSha256=value.get('originalSha256') or value.get('sourceSha256'),
            prompt=value.get('prompt', ''), references=value.get('references') or value.get('referencePaths', []),
            record=path.relative_to(ROOT).as_posix()))
    for value in read(WORK / 'generated-frame-recovery.json')['recovered']:
        if len(value['actors']) != 1:
            continue
        actor = value['actors'][0]
        frames = value['frames'] or [f for f in value.get('referenceFrames', [])
                                    if re.search(r'\b' + f.split('_')[0] + r'\b', value['prompt'])]
        if not frames or (actor, frames[0]) not in installed:
            continue
        candidates.append(dict(actorId=actor, frame=frames[0], sha256=value['sha256'], file=value['file'],
            originalSha256=None, prompt=value['prompt'], references=re.findall(r'[A-Za-z]:[^"\n]+\.png', value['code']),
            record='output/actor-redraw/generated-frame-recovery.json', generationTimestamp=value['timestamp']))
    for value in candidates:
        key = (value['actorId'], value['frame'])
        digest = value['sha256']
        if digest in rejected or digest == installed[key]['sha256']:
            continue
        original = WORK / 'originals' / key[0] / (key[1] + '.png')
        proof = read(WORK / 'frames' / key[0] / 'main' / (key[1] + '.provenance.json'))
        if value['originalSha256'] and value['originalSha256'] != proof['sourceSha256']:
            skipped.append(dict(actorId=key[0], frame=key[1], reason='superseded-source', record=value['record']))
            continue
        assert sha(original) == proof['sourceSha256']
        candidate = Path(value['file'])
        assert sha(candidate) == digest
        generated_at = candidate.stat().st_mtime
        if not value['originalSha256'] and proof.get('repair') and proof.get('createdAtUtc'):
            if generated_at <= datetime.fromisoformat(proof['createdAtUtc'].replace('Z', '+00:00')).timestamp():
                continue
        value['generatedAt'] = generated_at
        if key not in options or generated_at > options[key]['generatedAt']:
            options[key] = value
    operations = []
    for key, value in sorted(options.items()):
        original = WORK / 'originals' / key[0] / (key[1] + '.png')
        staged = WORK / 'frames' / key[0] / 'main' / (key[1] + '.png')
        proof_file = staged.with_suffix('.provenance.json')
        proof = read(proof_file)
        old = proof['sourceSha256']
        with Image.open(value['file']) as candidate:
            candidate.verify()
        with Image.open(value['file']) as candidate, Image.open(original) as previous:
            assert candidate.format == 'PNG' and candidate.size == previous.size and candidate.mode == 'RGBA'
            assert candidate.getchannel('A').getextrema()[0] == 0
            canvas = list(candidate.size)
        proof = copy.deepcopy(proof)
        proof['integrationGeometry'] = {k: installed[key][k] for k in ('canvas', 'anchor', 'pixelsPerUnit')}
        proof.update(sourceFile=original.relative_to(ROOT).as_posix(),
            sourceSha256=value['sha256'], outputSha256=value['sha256'], nativeResolution=True,
            pendingNativeRuntimeIntegration=True, prompt=value['prompt'], references=value['references'],
            review=dict(identity=False, pixelEdges=False, anchor=False, continuity=False),
            createdAtUtc=datetime.now(timezone.utc).isoformat(),
            normalization=dict(resampling='none', sourceCanvas=canvas, canvas=canvas, scale=1,
                cropped=False, spritesheetSlicing=False, byteIdenticalToGeneratedSource=True),
            repair=dict(replacedSourceSha256=old, generatedSource=value['file'],
                candidateProvenance=value['record'], selection='latest-not-rejected-authorized-by-user'))
        for field in ('staticReview', 'anatomyReview', 'iconFinalReview'):
            proof.pop(field, None)
        operations.append((value, original, staged, proof_file, proof, old))
    result = dict(applied=args.apply, selected=[{k: v for k, v in x[0].items() if k not in ('prompt', 'references')} for x in operations],
                  skipped=skipped, rejectedHashes=len(rejected), visualReview='waived-by-user')
    if args.apply:
        for value, original, staged, proof_file, proof, old in operations:
            archive = WORK / 'rejected/pre-integration-20261019/generated-repairs' / value['actorId'] / (value['frame'] + '-' + old[:12])
            assert not archive.exists()
            archive.mkdir(parents=True)
            for source, name in ((original, 'original.png'), (staged, 'staged.png'), (proof_file, 'staged.provenance.json')):
                shutil.copy2(source, archive / name)
            for destination in (original, staged):
                shutil.copyfile(value['file'], destination)
                assert sha(destination) == value['sha256']
            proof_file.write_text(json.dumps(proof, ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
        record_file = WORK / 'generated-repairs-integration.json'
        previous = read(record_file)['selected'] if record_file.exists() else []
        merged = {(v['actorId'], v['frame']): v for v in previous + result['selected']}
        result['selected'] = list(merged.values())
        record_file.write_text(json.dumps(result, ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(applied=args.apply, selected=len(operations), frames=[v[0]['actorId'] + '/' + v[0]['frame'] for v in operations], rejectedHashes=len(rejected))))


if __name__ == '__main__':
    main()
