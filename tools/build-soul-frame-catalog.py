"""Generate exact soul frame paths, preserving the previous loader's ordering."""
import argparse
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ACTIONS = {'idle': ['idle', 'Idle'], 'breathing': ['breathing', 'Breathing'],
           'run': ['run', 'walk', 'Walk'], 'attack': ['attack', 'BasicAtk'],
           'hit': ['hit', 'Hit'], 'death': ['death', 'Dead']}

def build(root=ROOT):
    resources = root / 'GameResources'
    available = {p.relative_to(resources).with_suffix('').as_posix()
                 for p in (resources / 'actors/Souls').rglob('*.png')}
    lookups = 0
    def first(paths):
        nonlocal lookups
        for path in paths:
            lookups += 1
            if path in available:
                return path
    def load(folder, aliases):
        base = 'actors/Souls/' + (folder + '/' if folder else '')
        for action in aliases:
            frames = []
            if folder:
                lower = action.lower()
                upper = lower[0].upper() + lower[1:]
                for i in range(128):
                    frame = first([base + folder + '_' + a + '_' + str(i).zfill(n)
                                   for a in (lower, upper) for n in (3, 2, 1)])
                    if frame:
                        frames.append(frame)
                    elif i > 0 and frames:
                        break
                if frames:
                    return frames
            for i in range(1, 129):
                frame = first([base + action + '/' + action + '_' + str(i).zfill(2),
                               base + action + '/' + action + '_' + str(i),
                               base + action + '/' + str(i).zfill(2), base + action + '/' + str(i)])
                if frame:
                    frames.append(frame)
            if frames:
                return frames
        return []
    folders = [''] + sorted(p.name for p in (resources / 'actors/Souls').iterdir() if p.is_dir())
    groups = {folder: {name: load(folder, aliases) for name, aliases in ACTIONS.items()}
              for folder in folders}
    return dict(schemaVersion=1, groups=groups), dict(previousCandidateLookups=lookups,
        catalogLookups=sum(len(frames) for actions in groups.values() for frames in actions.values()),
        groups=len(groups), runtimeFrames=len(available), unityStartupMeasured=False)

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    catalog, stats = build()
    path = ROOT / 'resources/world-soul-frames.manifest.json'
    data = json.dumps(catalog, ensure_ascii=True, indent=2) + '\n'
    if args.check:
        assert path.read_text(encoding='utf-8') == data, 'Soul catalog is stale; regenerate before packaging'
    else:
        path.write_text(data, encoding='utf-8')
    print(json.dumps(stats))
