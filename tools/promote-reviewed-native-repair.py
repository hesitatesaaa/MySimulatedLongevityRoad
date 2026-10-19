"""Promote one reviewed native repair while retaining its previous source and proof."""
import argparse
import hashlib
import json
import shutil
from datetime import datetime, timezone
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'output' / 'actor-redraw'
ARCHIVE = WORK / 'rejected' / 'final-review-20261018'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def within(path, parent):
    path = Path(path).resolve()
    path.relative_to(parent.resolve())
    return path


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--candidate', required=True, help='Saved independent PNG in repair-candidates')
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    candidate = within(args.candidate, WORK / 'repair-candidates')
    receipt = candidate.with_suffix('.provenance.json')
    repair = json.loads(receipt.read_text(encoding='utf-8-sig'))
    actor = repair.get('actorId') or repair.get('formId')
    frame = str(repair['frame'])
    action = repair.get('action') or frame.split('_')[0]
    index = int(frame.split('_')[-1])
    name = f'{action}_{index}'
    assert actor and actor.replace('_', '').isalnum() and action in ('idle', 'walk', 'death', 'attack', 'hit')
    if action in ('attack', 'hit'):
        targets = json.loads((ROOT / 'resources/actor-redraw.targets.json').read_text(encoding='utf-8-sig'))
        form = next(f for f in targets['forms'] if f['id'] == actor)
        assert form['actions'][action].get('generationRequired') is not False, 'Cancelled combat action'
    assert repair.get('status') in ('recommended-for-install', 'recommended-offline', 'candidate-not-integrated')
    original = WORK / 'originals' / actor / (name + '.png')
    staged = WORK / 'frames' / actor / 'main' / (name + '.png')
    within(original, WORK); within(staged, WORK)
    proof_path = staged.with_suffix('.provenance.json')
    proof = json.loads(proof_path.read_text(encoding='utf-8-sig'))
    old_sha = repair.get('originalSha256') or repair.get('sourceSha256')
    new_sha = repair.get('candidateSha256') or repair.get('outputSha256')
    assert proof['actorId'] == actor and proof['action'] == action and proof['index'] == index
    assert proof['sourceSha256'] == old_sha == sha(original)
    assert proof['outputSha256'] == sha(staged)
    assert new_sha == sha(candidate) and new_sha != old_sha
    assert within(repair.get('originalFile') or repair.get('sourceFile') or repair['originalPath'], ROOT) in (original.resolve(), staged.resolve())
    assert within(repair.get('candidateFile') or repair.get('outputPath'), ROOT) == candidate
    with Image.open(original) as old, Image.open(candidate) as new:
        old.load(); new.load()
        assert old.format == new.format == 'PNG' and old.mode == new.mode == 'RGBA'
        assert old.size == new.size and new.getchannel('A').getextrema()[0] == 0
        canvas = list(new.size)
    destination = ARCHIVE / actor / name
    assert not destination.exists(), 'Existing rollback archive: refuse second promotion'
    if args.check:
        print(json.dumps(dict(actor=actor, frame=name, oldSha256=old_sha, newSha256=new_sha, canvas=canvas)))
        return
    destination.mkdir(parents=True)
    for source, filename in ((original, 'original.png'), (staged, 'staged.png'),
                             (proof_path, 'staged.provenance.json')):
        shutil.copy2(source, destination / filename)
    for target in (original, staged):
        temporary = target.with_suffix('.repair.tmp')
        shutil.copyfile(candidate, temporary)
        temporary.replace(target)
        assert sha(target) == new_sha
    proof.update(sourceFile=original.relative_to(ROOT).as_posix(),
                 sourceSha256=new_sha, outputSha256=new_sha, prompt=repair['prompt'],
                 references=repair.get('references') or repair.get('referencePaths'),
                 createdAtUtc=datetime.now(timezone.utc).isoformat(),
                 nativeResolution=True, pendingNativeRuntimeIntegration=True,
                 normalization=dict(resampling='none', sourceCanvas=canvas, canvas=canvas,
                                    scale=1, cropped=False, spritesheetSlicing=False,
                                    byteIdenticalToGeneratedSource=True),
                 review=dict(identity=False, pixelEdges=False, anchor=False, continuity=False),
                 repair=dict(replacedSourceSha256=old_sha,
                             candidateProvenance=receipt.relative_to(ROOT).as_posix(),
                             rollbackArchive=destination.relative_to(ROOT).as_posix()))
    proof.pop('staticReview', None)
    proof.pop('anatomyReview', None)
    proof_path.write_text(json.dumps(proof, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    final_path = WORK / 'final-visual-review.json'
    final = json.loads(final_path.read_text(encoding='utf-8-sig'))
    final['reviewedForms'] = [f for f in final['reviewedForms'] if f['actorId'] != actor]
    final['unresolvedDefects'] = [f for f in final['unresolvedDefects'] if f.get('sourceSha256') != old_sha]
    final['repairs'].append(dict(actorId=actor, frame=name, sourceSha256=new_sha))
    final.update(passed=False, releaseReady=False, updatedAtUtc=datetime.now(timezone.utc).isoformat())
    final_path.write_text(json.dumps(final, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(actor=actor, frame=name, sourceSha256=new_sha,
                          rollbackArchive=destination.relative_to(ROOT).as_posix())))


if __name__ == '__main__':
    main()
