"""Apply explicit, hash-bound human/model inspection decisions; never discovers approvals."""
import argparse
import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser()
parser.add_argument("--reviews", required=True)
args = parser.parse_args()
file = (ROOT / args.reviews).resolve()
if not file.is_relative_to((ROOT / 'output/actor-redraw').resolve()):
    raise ValueError('Inspection decision file outside authoring workspace')
rows = json.loads(file.read_text(encoding='utf-8-sig'))
for row in rows:
    if row.get('scope') != 'idle_0-character-standard-only' or not row.get('inspected'):
        raise ValueError('Only explicit standard inspection decisions are accepted')
    subprocess.run([sys.executable, str(ROOT / 'tools/record-detailed-standard-review.py'),
                    '--actor', row['actor'], '--sha256', row['sha256'], '--note', row['note']], check=True)
print(json.dumps({'recordedStandards': len(rows)}))
