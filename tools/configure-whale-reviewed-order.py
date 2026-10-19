"""Remove rejected redundant death poses; preserve total visual action duration."""
import importlib.util
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("authoring", ROOT / "tools/actor-redraw.py")
A = importlib.util.module_from_spec(spec)
spec.loader.exec_module(A)
path = A.RESOURCES / "actor-redraw.targets.json"
targets = A.load_json(path)
form = next(f for f in targets["forms"] if f["id"] == "mclsl_beast_liejiejing")
clip = form["actions"]["death"]
total_ms = clip["count"] * clip["durationMs"]
clip.update(count=6, durationMs=total_ms / 6, frameOrder=[0, 1, 3, 5, 4, 7])
targets["totalFrames"] = sum(c["count"] for f in targets["forms"] for c in f["actions"].values())
A.save_json(path, targets)
card_path = ROOT / form["referenceCard"]
card = A.load_json(card_path)
card["plannedActions"] = form["actions"]
A.save_json(card_path, card)
print("Liejiejing: six collapse frames, original total action duration preserved.")
