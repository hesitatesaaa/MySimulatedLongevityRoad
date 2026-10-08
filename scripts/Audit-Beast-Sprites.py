"""Validate every active actor animation frame used by the 0.5.4 registrations."""
import hashlib
import json
import subprocess
import sys
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
ACTORS = ROOT / "GameResources" / "actors"
MANIFEST = ROOT / "scripts" / "beast_sprite_sources" / "manifest.json"
COUNTS = {"idle":4,"attack":6,"walk":4,"hit":4,"death":4}

def forms():
    manifest = json.loads(MANIFEST.read_text(encoding="utf8"))
    if len(manifest["beasts"]) != 21 or len(manifest["named"]) != 16:
        raise AssertionError("catalogue count")
    for name in manifest["beasts"]:
        for form in ("Beast","Human"):
            yield ACTORS/"Beasts"/name/form
    for name in manifest["named"]:
        if name == "BaiXianSheng":
            continue
        if name in ("DiYi","DiSanMo","QingShenLong"):
            for form in ("Beast","Human"):
                yield ACTORS/"Named"/name/form
        else:
            yield ACTORS/"Named"/name
    yield ACTORS/"Immortals"/"Bai"
    yield ACTORS/"Immortals"/"Chuanfa"

def audit(folder):
    hashes = {}
    canvas_size = None
    feet = []
    icon_path = folder/"spawn_icon.png"
    with Image.open(icon_path) as loaded:
        icon = loaded.convert("RGBA")
    icon_box = icon.getchannel("A").getbbox()
    if not icon_box or icon.width != icon.height:
        raise AssertionError(f"empty/non-square spawn icon: {icon_path}")
    icon_occupancy = max((icon_box[2]-icon_box[0])/icon.width,
                         (icon_box[3]-icon_box[1])/icon.height)
    if not .70 <= icon_occupancy <= .90:
        raise AssertionError(f"spawn icon too small or clipped: {icon_path} {icon_occupancy:.2f}")
    if icon_box[0] < 1 or icon_box[1] < 1 or icon_box[2] >= icon.width or icon_box[3] >= icon.height:
        raise AssertionError(f"spawn icon touches canvas edge: {icon_path}")
    for action,count in COUNTS.items():
        images = []
        for index in range(count):
            path = folder/"main"/f"{action}_{index}.png"
            with Image.open(path) as loaded:
                image = loaded.convert("RGBA")
            if image.width != image.height or not 64 <= image.width <= 512:
                raise AssertionError(f"invalid canvas size: {path} {image.size}")
            if canvas_size is None:
                canvas_size = image.size
            if image.size != canvas_size or image.getpixel((0,0))[3] != 0:
                raise AssertionError(f"canvas/alpha: {path}")
            box = image.getchannel("A").getbbox()
            if not box:
                raise AssertionError(f"empty frame: {path}")
            margin = min(box[0], box[1], image.width-box[2], image.height-box[3])
            if margin < 8:
                raise AssertionError(f"frame clips or lacks safety margin: {path} {box}")
            width,height = box[2]-box[0],box[3]-box[1]
            min_height = 4 if action == "death" else 15
            if width < 15 or height < min_height:
                raise AssertionError(f"partial/incorrect crop: {path} {box}")
            opaque = sum(1 for alpha in image.getchannel("A").getdata() if alpha)
            if opaque < 90:
                raise AssertionError(f"incomplete silhouette: {path} {opaque}")
            if action in ("idle", "walk"):
                feet.append(box[3])
            images.append(image)
            digest = hashlib.sha256(image.tobytes()).hexdigest()
            if digest in hashes:
                raise AssertionError(f"same frame reused in {action} and {hashes[digest]}: {path}")
            hashes[digest] = action
        if len({hashlib.sha256(i.tobytes()).hexdigest() for i in images}) < 3:
            raise AssertionError(f"insufficient motion: {folder} {action}")
    if max(feet)-min(feet) > 8:
        raise AssertionError(f"unstable idle/walk foot anchor: {folder} {feet}")
    return len(hashes)

def main():
    count = 0
    for folder in forms():
        count += audit(folder)
    print(f"Validated {count} structurally distinct frames across 62 active forms.")
    # Distinct pixels alone do not establish separately drawn art. The strict
    # provenance check is the release gate for the requested redraw.
    subprocess.run([sys.executable, str(ROOT / "scripts" / "Audit-Frame-Redraw.py")], check=True)

if __name__ == "__main__":
    try:
        main()
    except Exception as exc:
        print(f"Animation audit failed: {exc}", file=sys.stderr)
        raise
