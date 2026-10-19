"""Archive an exact staged frame for independent redraw; never recursively delete."""
import argparse
import json
import shutil
from pathlib import Path

root = Path(__file__).resolve().parents[1]
work = root / 'output/actor-redraw'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--actor', required=True)
parser.add_argument('--action', choices=['idle', 'walk', 'attack', 'hit', 'death', 'icon'], required=True)
parser.add_argument('--index', type=int, required=True)
parser.add_argument('--reason', required=True)
args = parser.parse_args()
if args.index < 0 or not args.actor.replace('_', '').isalnum():
    raise ValueError('Invalid actor or frame index')
stem = f'{args.action}_{args.index}'
file = work / ('icons/' + args.actor + '.png' if args.action == 'icon'
               else f'frames/{args.actor}/main/{stem}.png')
proof = file.with_suffix('.provenance.json')
record = json.loads(proof.read_text(encoding='utf-8-sig'))
original = (root / record['sourceFile']).resolve()
original.relative_to(work.resolve())
if record['actorId'] != args.actor or record['action'] != args.action or record['index'] != args.index:
    raise ValueError('Mismatched provenance')
destination = work / f'rejected/{args.actor}/{stem}-v1'
revision = 1
while destination.exists():
    revision += 1
    destination = work / f'rejected/{args.actor}/{stem}-v{revision}'
destination.mkdir(parents=True)
for source, name in [(file, 'normalized.png'), (proof, 'provenance.json'), (original, 'original.png')]:
    source.resolve().relative_to(work.resolve())
    shutil.move(str(source), destination / name)
(destination / 'rejection.json').write_text(json.dumps({'actorId': args.actor, 'frame': stem,
    'reason': args.reason}, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(destination)
