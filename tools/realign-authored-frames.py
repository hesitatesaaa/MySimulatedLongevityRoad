"""Reapply explicitly authorized normalization to preserved independent originals."""
from pathlib import Path
import json
import subprocess
import sys
import argparse

root = Path(__file__).resolve().parents[1]
work = root / "output/actor-redraw"
records = sorted((work / "frames").glob("*/main/*.provenance.json"))
records += sorted((work / "icons").glob("*.provenance.json"))
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--actor")
args = parser.parse_args()
if args.actor:
    records = [p for p in records if json.loads(p.read_text(encoding="utf-8"))["actorId"] == args.actor]
for path in records:
    record = json.loads(path.read_text(encoding="utf-8"))
    prompt_file = work / "realignment-prompts" / record["actorId"] / f"{record['action']}_{record['index']}.txt"
    prompt_file.parent.mkdir(parents=True, exist_ok=True)
    prompt_file.write_text(record["prompt"], encoding="utf-8")
    subprocess.run([sys.executable, str(root / "tools/actor-redraw.py"), "normalize",
                    "--actor", record["actorId"], "--action", record["action"], "--index", str(record["index"]),
                    "--source", str(root / record["sourceFile"]), "--prompt-file", str(prompt_file), "--realign"], check=True)
print(f"Reprocessed {len(records)} independently drawn assets from preserved originals.")
