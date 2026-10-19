"""Archive exact uninstalled standards superseded by a user art-direction change."""
import argparse
import importlib.util
import shutil
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("authoring", ROOT / "tools/actor-redraw.py")
A = importlib.util.module_from_spec(spec)
spec.loader.exec_module(A)
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--actor", action="append", required=True)
parser.add_argument("--reason", required=True)
args = parser.parse_args()
targets = {f["id"] for f in A.load_json(A.RESOURCES / "actor-redraw.targets.json")["forms"]}
installed = {g["actorId"] for g in A.load_json(A.RESOURCES / "actor-frames.manifest.json")["groups"] if g.get("approved")}
for actor in args.actor:
    if actor not in targets or actor in installed:
        raise ValueError("Only an uninstalled known form may be superseded: " + actor)
    destination = A.within(A.WORK, f"rejected/{actor}/direction-{datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%S%f')}")
    files = [A.WORK / f"frames/{actor}/main/idle_0.png",
             A.WORK / f"frames/{actor}/main/idle_0.provenance.json",
             A.WORK / f"frames/{actor}/normalization-profile.json",
             A.WORK / f"originals/{actor}/idle_0.png",
             A.WORK / f"prompts/{actor}/idle_0.txt",
             A.RESOURCES / f"actor-standard-reviews/{actor}.json"]
    for file in files:
        if not file.exists():
            continue
        if not file.resolve().is_relative_to(ROOT.resolve()):
            raise ValueError("Unexpected source outside project")
        target = A.within(destination, file.resolve().relative_to(ROOT.resolve()).as_posix())
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.move(str(file.resolve()), str(target))
    A.save_json(destination / "reason.json", {"actorId": actor, "reason": args.reason, "productionFilesTouched": False})
    print("Archived superseded uninstalled standard: " + actor)
