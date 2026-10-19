"""Read bounded job pages for the built-in tool orchestrator; performs no generation."""
import argparse
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser()
parser.add_argument("file")
parser.add_argument("--offset", type=int, default=0)
parser.add_argument("--count", type=int, default=8)
args = parser.parse_args()
path = (root / args.file).resolve()
if not path.is_relative_to((root / "output/actor-redraw").resolve()):
    raise ValueError("Job list outside authoring workspace")
jobs = json.loads(path.read_text(encoding="utf-8-sig"))
current = json.loads((root / "resources/actor-art-direction.json").read_text(encoding="utf-8-sig"))["revision"]
selected = jobs[args.offset:args.offset + args.count]
if any(j.get("artDirectionRevision") != current for j in selected):
    raise ValueError("Job page has superseded art direction")
print(json.dumps({"total": len(jobs), "jobs": selected}, ensure_ascii=True))
