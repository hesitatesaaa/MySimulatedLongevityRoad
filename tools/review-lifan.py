"""Record the offline review performed on the independent-frame contact previews."""
import hashlib
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
actor = "mclsl_named_lifan"
frames = {}
for file in (root / "output/actor-redraw/frames" / actor / "main").glob("*.png"):
    frames[file.stem] = {
        "sha256": hashlib.sha256(file.read_bytes()).hexdigest(),
        "identity": True, "pixelEdges": True, "anchor": True, "continuity": True,
    }
if len(frames) != 42:
    raise ValueError("LiFan must have all 42 independently drawn frames")
icon = root / "output/actor-redraw/icons" / (actor + ".png")
record = {
    "actorId": actor, "offlinePassed": True,
    "reviewMethod": "manual inspection of every independent PNG and all five labeled contact previews",
    "notes": [
        "Same navy-white-gold costume and tied hair across the five actions.",
        "Idle and walk retain recognizable silhouette; feet remain at the ground anchor.",
        "Attack shows gathering, release and recovery. Hit shows recoil and recovery.",
        "Death poses 3, 4, 5, 6 and 8 were independently redrawn after continuity defects.",
        "Authorized alpha cleanup removes only source alpha below 16/255; death frame 2 now aligns without its former one-pixel offset.",
        "No real-game size, UI occupancy, sorting, motion or performance approval is claimed.",
    ],
    "frames": frames,
    "icon": {"sha256": hashlib.sha256(icon.read_bytes()).hexdigest(), "offlinePassed": True,
             "bodyLongestEdgePixels": 94, "independentlyDrawn": True},
    "verifiedInGame": False,
}
destination = root / "resources/actor-reviews" / (actor + ".json")
destination.parent.mkdir(parents=True, exist_ok=True)
destination.write_text(json.dumps(record, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(destination)
