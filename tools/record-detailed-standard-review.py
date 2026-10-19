"""Save an explicitly inspected standard hash; does not approve a full animation set."""
import argparse
import importlib.util
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("authoring", ROOT / "tools/actor-redraw.py")
A = importlib.util.module_from_spec(spec)
spec.loader.exec_module(A)
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--actor", required=True)
parser.add_argument("--sha256", required=True)
parser.add_argument("--note", required=True)
args = parser.parse_args()
form = next(f for f in A.load_json(A.RESOURCES / "actor-redraw.targets.json")["forms"] if f["id"] == args.actor)
file = A.WORK / f"frames/{args.actor}/main/idle_0.png"
info = A.image_info(file)
proof = A.load_json(file.with_suffix(".provenance.json"))
if args.sha256 != info["sha256"] or proof.get("outputSha256") != args.sha256 or proof.get("method") != "built-in-imagegen-independent-frame":
    raise ValueError("The inspected standard changed or lacks independent-source provenance")
if info["canvas"] != form["canvas"] or not info["bodyBounds"]:
    raise ValueError("Standard canvas differs from form plan")
A.check_source_edges(A.within(ROOT, proof["sourceFile"]))
A.save_json(A.RESOURCES / f"actor-standard-reviews/{args.actor}.json", {
    "actorId": args.actor, "sha256": info["sha256"], "sourceSha256": proof["sourceSha256"],
    "offlinePassed": True, "reviewScope": "idle_0-character-standard-only", "verifiedInGame": False,
    "artDirectionRevision": A.load_json(A.RESOURCES / "actor-art-direction.json")["revision"],
    "visualGender": form.get("visualGender"),
    "note": args.note, "reviewedAtUtc": datetime.now(timezone.utc).isoformat(),
})
print("Recorded inspected standard only: " + args.actor)
