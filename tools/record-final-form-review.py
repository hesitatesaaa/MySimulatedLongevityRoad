"""Record an explicitly completed per-frame native visual review in the existing final record."""
import argparse
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'output' / 'actor-redraw'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--actor', required=True)
    parser.add_argument('--reviewer', required=True)
    parser.add_argument('--frame-count', type=int, required=True)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    targets = json.loads((ROOT / 'resources/actor-redraw.targets.json').read_text(encoding='utf-8-sig'))
    form = next(f for f in targets['forms'] if f['id'] == args.actor)
    review_path = WORK / 'final-visual-review.json'
    review = json.loads(review_path.read_text(encoding='utf-8-sig'))
    assert args.check or not any(f['actorId'] == args.actor for f in review['reviewedForms']), 'Form already recorded'
    sources = []
    actions = []
    for action, spec in form['actions'].items():
        if spec.get('generationRequired') is False:
            continue
        actions.append(action)
        for index in spec.get('frameOrder', spec.get('indices', list(range(spec['count'])))):
            name = f'{action}_{index}'
            proof_path = WORK / 'frames' / args.actor / 'main' / (name + '.provenance.json')
            proof = json.loads(proof_path.read_text(encoding='utf-8-sig'))
            source = (ROOT / proof['sourceFile']).resolve()
            source.relative_to(ROOT.resolve())
            assert proof['actorId'] == args.actor and proof['action'] == action and proof['index'] == index
            assert sha(source) == proof['sourceSha256'], 'Changed original: ' + name
            assert sha(WORK / 'originals' / args.actor / (name + '.png')) == proof['sourceSha256'], 'Viewed original differs: ' + name
            with Image.open(source) as image:
                bounds = image.getchannel('A').point(lambda v: 255 if v >= 128 else 0).getbbox()
                assert bounds and bounds[0] > 0 and bounds[1] > 0 and bounds[2] < image.width and bounds[3] < image.height, 'Visible source touches edge: ' + name
            sources.append((name, proof['sourceSha256']))
    inventory = '\n'.join(name + ':' + digest for name, digest in sorted(sources))
    assert len(sources) == args.frame_count, 'Reviewer frame count differs from target'
    if args.check:
        print(json.dumps(dict(actorId=args.actor, frameCount=len(sources),
            sourceInventorySha256=hashlib.sha256(inventory.encode()).hexdigest())))
        return
    review['reviewedForms'].append(dict(actorId=args.actor, frameCount=len(sources),
        sourceInventorySha256=hashlib.sha256(inventory.encode()).hexdigest(),
        actionsReviewed=actions, reviewer=args.reviewer, confirmedDefects=[], verifiedInGame=False))
    review['updatedAtUtc'] = datetime.now(timezone.utc).isoformat()
    review_path.write_text(json.dumps(review, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(actorId=args.actor, frameCount=len(sources), reviewer=args.reviewer)))


if __name__ == '__main__':
    main()
