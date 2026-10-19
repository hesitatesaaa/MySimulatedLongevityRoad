"""Batch preview an explicit form; no production drawing or approval."""
import argparse
import importlib.util
from pathlib import Path
from types import SimpleNamespace

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('authoring', ROOT / 'tools/actor-redraw.py')
A = importlib.util.module_from_spec(spec)
spec.loader.exec_module(A)
parser = argparse.ArgumentParser()
parser.add_argument('--actor', required=True)
args = parser.parse_args()
form = next(f for f in A.load_json(A.RESOURCES / 'actor-redraw.targets.json')['forms'] if f['id'] == args.actor)
for action, clip in form['actions'].items():
    indices = A.frame_indices(clip)
    complete = all((A.WORK / f'frames/{args.actor}/main/{action}_{i}.png').exists() for i in indices)
    if any((A.WORK / f'frames/{args.actor}/main/{action}_{i}.png').exists() for i in indices):
        A.preview(SimpleNamespace(actor=args.actor, action=action, partial=not complete))
