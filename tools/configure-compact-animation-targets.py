"""Use compact action phases for unfinished forms; keep already installed artwork intact."""
import importlib.util
from pathlib import Path

spec = importlib.util.spec_from_file_location("authoring", Path(__file__).with_name("actor-redraw.py"))
authoring = importlib.util.module_from_spec(spec)
spec.loader.exec_module(authoring)
file = authoring.RESOURCES / "actor-redraw.targets.json"
targets = authoring.load_json(file)
manifest = authoring.load_json(authoring.RESOURCES / "actor-frames.manifest.json")
installed = {g["actorId"] for g in manifest["groups"] if g.get("approved")}
counts = {"human": (4, 6, 8, 2, 6), "bird": (6, 8, 10, 2, 8),
          "dragon": (6, 8, 12, 4, 10), "quadruped": (4, 6, 8, 2, 6),
          "aquatic": (6, 8, 10, 2, 8), "heavy": (4, 6, 8, 2, 8),
          "special": (4, 6, 8, 2, 6)}
# Phase duration is preserved while redundant sample poses are removed.
before = targets["totalFrames"]
changes = []
for form in targets["forms"]:
    if form["id"] in installed:
        continue
    for action, count in zip(authoring.ACTIONS, counts[form["family"]]):
        clip = form["actions"][action]
        old = clip["count"]
        if old == count:
            continue
        authored = list((authoring.WORK / f"frames/{form['id']}/main").glob(action + "_*.png"))
        if any(int(p.stem.rsplit("_", 1)[1]) >= count for p in authored):
            raise ValueError("Existing authored frames need an explicit edit review: " + form["id"] + "/" + action)
        total_ms = old * clip["durationMs"]
        clip.update({"count": count, "durationMs": total_ms / count})
        clip.pop("frameOrder", None)
        changes.append({"actorId": form["id"], "action": action, "before": old, "after": count,
                        "clipDurationMs": total_ms})
targets["totalFrames"] = sum(c["count"] for f in targets["forms"] for c in f["actions"].values())
targets["authoringSchedule"] = {"mode": "compact-complete-action-phases", "reason": "User requested faster completion",
                               "singleFramePNG": True, "spritesheetSlicing": False,
                               "installedFormsPreserved": sorted(installed), "initialTargetFrames": 2746}
authoring.save_json(file, targets)
authoring.save_json(authoring.WORK / "compact-target-changes.json", {"before": before, "after": targets["totalFrames"],
                                                                   "changes": changes})
print(f"Updated uncompleted forms: {before} -> {targets['totalFrames']} frames. Installed forms preserved.")
