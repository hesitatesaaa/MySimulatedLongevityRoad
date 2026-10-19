"""Recover the newest source-bound, non-rejected repair without claiming visual review."""
import argparse
import copy
import json
import shutil
from pathlib import Path
from release_assets import ROOT, WORK, indices, png, read, sha, within, write

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--source-root', required=True)
parser.add_argument('--apply', action='store_true')
args = parser.parse_args()
old = Path(args.source_root).resolve()
targets = {f['id']: f for f in read(ROOT / 'resources/actor-redraw.targets.json')['forms']}
selected = {}
allowed = {'recommended-offline', 'recommended-for-install', 'candidate-not-integrated', 'candidate-pending-review'}
for receipt in (old / 'output/actor-redraw/repair-candidates').rglob('*.provenance.json'):
    record = read(receipt)
    if record.get('status') not in allowed or 'rejected' in receipt.parts:
        continue
    actor = record.get('actorId') or record.get('formId')
    action = record.get('action') or str(record['frame']).split('_')[0]
    index = int(str(record['frame']).split('_')[-1])
    if actor not in targets or action not in targets[actor]['actions']:
        continue
    spec = targets[actor]['actions'][action]
    if spec.get('generationRequired') is False or index not in indices(spec):
        continue
    candidate = within(old, record.get('candidateFile') or record['outputPath'])
    if sha(candidate) != (record.get('candidateSha256') or record['outputSha256']).lower():
        raise ValueError('Repair receipt hash mismatch: ' + str(receipt))
    key = (actor, action, index)
    if key not in selected or candidate.stat().st_mtime > selected[key][0].stat().st_mtime:
        selected[key] = candidate, receipt, record

repairs = []
for (actor, action, index), (candidate, receipt, record) in selected.items():
    staged = WORK / 'frames' / actor / 'main' / f'{action}_{index}.png'
    proof_path = staged.with_suffix('.provenance.json')
    previous = read(proof_path)
    digest = sha(candidate)
    if previous['sourceSha256'] == digest:
        continue
    expected_old = record.get('originalSha256') or record.get('sourceSha256')
    malformed_receipt_hash = bool(expected_old and len(expected_old) != 64)
    if expected_old and expected_old.lower() != previous['sourceSha256']:
        old_proof = read(old / 'output/actor-redraw/frames' / actor / 'main' / f'{action}_{index}.provenance.json')
        bound_original = within(old, record.get('originalFile') or record.get('sourceFile') or record.get('originalPath', ''))
        if (not malformed_receipt_hash or sha(bound_original) != previous['sourceSha256']
                or old_proof['sourceSha256'] != previous['sourceSha256']):
            raise ValueError('Repair refers to a different original: ' + str(receipt))
    source = within(ROOT, previous['sourceFile'])
    if sha(source) != previous['sourceSha256'] or sha(staged) != previous['outputSha256']:
        raise ValueError('Current source changed during recovery')
    canvas, _ = png(candidate)
    if canvas != png(source)[0] or not record.get('prompt'):
        raise ValueError('Repair canvas or prompt does not match native source: ' + str(receipt))
    row = dict(actorId=actor, frame=f'{action}_{index}', oldSha256=previous['sourceSha256'],
               sourceSha256=digest, reviewStatus='waived-by-user', candidate=str(candidate))
    if malformed_receipt_hash:
        row['malformedOriginalReceiptHash'] = expected_old
        row['originalReboundByPathAndVerifiedProvenance'] = True
    if args.apply:
        archive = WORK / 'replaced-sources-0.6.0' / actor / row['frame']
        archive.mkdir(parents=True, exist_ok=False)
        shutil.copyfile(source, archive / 'original.png')
        shutil.copyfile(staged, archive / 'staged.png')
        shutil.copyfile(proof_path, archive / 'staged.provenance.json')
        original = WORK / 'originals' / actor / (row['frame'] + '.png')
        original.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(candidate, original)
        shutil.copyfile(candidate, staged)
        proof = copy.deepcopy(previous)
        proof.update(sourceFile=original.relative_to(ROOT).as_posix(), sourceSha256=digest,
                     outputFile=staged.relative_to(ROOT).as_posix(), outputSha256=digest,
                     prompt=record['prompt'], references=record.get('references', record.get('referencePaths', [])),
                     nativeResolution=True, pendingNativeRuntimeIntegration=True,
                     reviewStatus='waived-by-user', review=dict(identity=False, pixelEdges=False, anchor=False, continuity=False),
                     normalization=dict(resampling='none', sourceCanvas=canvas, canvas=canvas, scale=1,
                                        cropped=False, spritesheetSlicing=False, byteIdenticalToGeneratedSource=True))
        proof.pop('staticReview', None)
        proof.pop('anatomyReview', None)
        recovered_receipt = ROOT / 'resources/actor-provenance' / actor / 'native' / (row['frame'] + '.repair.json')
        write(recovered_receipt, dict(record, recovery=row, visualReviewWaived=True))
        proof['repair'] = dict(replacedSourceSha256=previous['sourceSha256'],
                              candidateProvenance=recovered_receipt.relative_to(ROOT).as_posix())
        write(proof_path, proof)
    repairs.append(row)
audit_path = WORK / 'workspace-sync-audit.json'
audit = read(audit_path) if audit_path.exists() else {}
audit['recoveredNativeRepairs'] = repairs
audit['repairsApplied'] = args.apply
write(audit_path, audit)
print(json.dumps(dict(applied=args.apply, repairs=repairs)))
