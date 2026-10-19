"""Check the actual installed NML's recursive C# source selection before packaging."""
import argparse
import json
import os
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
# Verified against ModCompileLoadService lambdas and Paths in the installed NML.
IGNORED = {'bin', 'obj', 'Properties', 'packages', 'packages.config', 'packages-lock.json', 'packages-lock.xml'}


def selected_sources(root):
    result = set()
    for directory, folders, names in os.walk(root):
        folders[:] = [name for name in folders if not name.startswith('.') and name not in IGNORED]
        for name in names:
            if name.endswith('.cs') and not name.startswith('.'):
                result.add((Path(directory) / name).resolve())
    return result


def validate(root):
    root = root.resolve()
    actual = selected_sources(root)
    expected = {path.resolve() for path in (root / 'code').rglob('*.cs')} | {root / 'InterestingTrait.cs'}
    extras, missing = actual - expected, expected - actual
    if extras or missing:
        raise ValueError(json.dumps(dict(
            unexpected=[str(p.relative_to(root)) for p in sorted(extras)[:12]], unexpectedCount=len(extras),
            missing=[str(p.relative_to(root)) for p in sorted(missing)[:12]]), ensure_ascii=True))
    assert expected and all(path.is_file() for path in expected)
    return sorted(actual)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=ROOT)
    parser.add_argument('--json', action='store_true')
    args = parser.parse_args()
    sources = validate(args.root)
    print(json.dumps(dict(sourceCount=len(sources), sources=[str(p) for p in sources]) if args.json
                     else dict(sourceCount=len(sources), developerToolsExcluded=True), ensure_ascii=True))
