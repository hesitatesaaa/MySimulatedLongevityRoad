"""Record one built-in imagegen result and run only approved single-frame normalization."""
import argparse
import importlib.util
import json
import subprocess
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("authoring", ROOT / "tools/actor-redraw.py")
A = importlib.util.module_from_spec(spec)
spec.loader.exec_module(A)
parser = argparse.ArgumentParser()
parser.add_argument("--jobs", required=True)
parser.add_argument("--index", required=True, type=int)
parser.add_argument("--source", required=True)
args = parser.parse_args()
jobs_file = A.within(A.WORK, args.jobs)
job = A.load_json(jobs_file)[args.index]
revision = A.load_json(A.RESOURCES / "actor-art-direction.json")["revision"]
record = {"jobIndex": args.index, "actor": job["actor"], "action": job["action"],
          "index": job["index"], "artDirectionRevision": job["artDirectionRevision"],
          "source": args.source, "completedAtUtc": datetime.now(timezone.utc).isoformat()}
control = A.load_json(A.WORK / "pipeline.control.json")
record["continueDispatch"] = (not control.get("stopDispatch", False)
                              and control.get("artDirectionRevision") == job["artDirectionRevision"]
                              and revision == job["artDirectionRevision"])
try:
    if job["artDirectionRevision"] != revision:
        raise ValueError("Art direction changed; preserve result without installing it")
    prompt_file = A.within(ROOT, job["promptFile"])
    if prompt_file.read_text(encoding="utf-8-sig").strip() != job["prompt"].strip():
        raise ValueError("Prompt changed since dispatch; result requires explicit reconciliation")
    command = [str(Path(__import__('sys').executable)), str(ROOT / "tools/actor-redraw.py"), "normalize",
               "--actor", job["actor"], "--action", job["action"], "--index", str(job["index"]),
               "--source", args.source, "--prompt-file", str(prompt_file)]
    for reference in job["references"]:
        command.extend(["--reference", reference])
    result = subprocess.run(command, capture_output=True, text=True, encoding="utf-8")
    record.update(exitCode=result.returncode, output=result.stdout.strip(), error=result.stderr.strip())
except Exception as error:
    record.update(exitCode=1, error=str(error))
destination = A.WORK / "batch-results" / jobs_file.stem / f"{args.index:05d}.json"
A.save_json(destination, record)
print(json.dumps(record, ensure_ascii=True))
raise SystemExit(record["exitCode"])
