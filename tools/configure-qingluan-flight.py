"""Record reviewed flight layout and the two needed recovery poses."""
from pathlib import Path
import json

root = Path(__file__).resolve().parents[1]
target_path = root / "resources/actor-redraw.targets.json"
targets = json.loads(target_path.read_text(encoding="utf-8"))
form = next(f for f in targets["forms"] if f["id"] == "mclsl_beast_qingluan")
form.update({"canvas": [160, 160], "anchor": [80, 128],
             "authoringAdjustment": "Add transparent margins for full wingbeat; fixed body pixel scale remains unchanged."})
form["actions"]["walk"].update({"count": 14, "durationMs": 80,
                                "frameOrder": [0, 1, 9, 4, 3, 2, 5, 6, 7, 12, 13, 8, 10, 11]})
targets["totalFrames"] = sum(sum(a["count"] for a in f["actions"].values()) for f in targets["forms"])
target_path.write_text(json.dumps(targets, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
profile_path = root / "output/actor-redraw/frames/mclsl_beast_qingluan/normalization-profile.json"
profile = json.loads(profile_path.read_text(encoding="utf-8"))
profile["frameOffsets"] = {"walk_1": [-1,-1], "walk_2": [6,9], "walk_3": [10,9], "walk_4": [4,6],
                           "walk_5": [10,9], "walk_6": [12,9], "walk_7": [1,9], "walk_8": [-2,1],
                           "walk_9": [0,-2], "walk_10": [1,-2]}
profile["frameOffsetReview"] = "Head landmark matches inspected against all 12 independently drawn frames; canvas alignment only."
profile_path.write_text(json.dumps(profile, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"canvas": form["canvas"], "anchor": form["anchor"], "flightFrames": 14,
                  "totalTargetFrames": targets["totalFrames"]}))
