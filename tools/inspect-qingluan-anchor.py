from PIL import Image
from pathlib import Path
import statistics
import json
import sys

root = Path(__file__).resolve().parents[1]
source = root / "output/actor-redraw/originals/mclsl_beast_qingluan/idle_0.png"
image = Image.open(source).convert("RGBA")
solid = image.getchannel("A").point(lambda a: 255 if a >= 128 else 0).getbbox()
feet = []
for y in range(solid[3] - 40, solid[3]):
    for x in range(solid[0], solid[2]):
        r, g, b, a = image.getpixel((x, y))
        if a >= 128 and r > 130 and g > 85 and b < 110:
            feet.append(x)
print(json.dumps({"opaqueBounds": solid, "goldenFootMedianX": statistics.median(feet),
                  "goldenFootMinX": min(feet), "goldenFootMaxX": max(feet)}))
if "--apply" in sys.argv:
    path = root / "output/actor-redraw/frames/mclsl_beast_qingluan/normalization-profile.json"
    profile = json.loads(path.read_text(encoding="utf-8"))
    profile.update({"sourceAnchorX": statistics.median(feet), "sourceAnchorY": solid[3],
                    "anchorMeasurement": "median of opaque golden feet pixels in bottom 40 source rows"})
    path.write_text(json.dumps(profile, indent=2) + "\n", encoding="utf-8")
