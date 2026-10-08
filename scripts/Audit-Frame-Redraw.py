"""Strict visual-resource audit for the 62 forms in 0.5.4.

Pixel differences cannot establish separate authorship. This audit therefore
checks a per-file provenance record as well as measurable image properties.
"""
import argparse
import hashlib
import json
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
ACTORS = ROOT / "GameResources" / "actors"
SOURCE = ROOT / "scripts" / "beast_sprite_sources" / "manifest.json"
PROVENANCE = ROOT / "scripts" / "actor_frame_provenance.json"
COUNTS = {"idle": 4, "attack": 6, "walk": 4, "hit": 4, "death": 4}


def forms():
    data = json.loads(SOURCE.read_text(encoding="utf-8"))
    assert len(data["beasts"]) == 21 and len(data["named"]) == 16
    for name in data["beasts"]:
        for form in ("Beast", "Human"):
            yield ACTORS / "Beasts" / name / form
    for name in data["named"]:
        if name == "BaiXianSheng":
            continue  # Existing character is Immortals/Bai.
        base = ACTORS / "Named" / name
        for form in (("Beast", "Human") if name in ("DiYi", "DiSanMo", "QingShenLong") else ("",)):
            yield base / form
    for name in ("Bai", "Chuanfa"):
        yield ACTORS / "Immortals" / name


def relative(path):
    return path.relative_to(ROOT).as_posix()


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def inspect(path, issues, edges, min_margin=8):
    if not path.is_file():
        issues.append(f"missing: {relative(path)}")
        return None
    with Image.open(path) as original:
        if original.format != "PNG" or original.mode != "RGBA":
            issues.append(f"not RGBA PNG: {relative(path)} ({original.format}/{original.mode})")
        image = original.convert("RGBA")
    box = image.getchannel("A").getbbox()
    if box is None:
        issues.append(f"empty image: {relative(path)}")
        return None
    margins = (box[0], box[1], image.width - box[2], image.height - box[3])
    if min(margins) == 0:
        edges.append({"path": relative(path), "bbox": box, "margins": margins})
    if min(margins) < min_margin:
        issues.append(f"margin < {min_margin} px: {relative(path)} {margins}")
    return image, box


def run(structural_only=False):
    inventory = json.loads(PROVENANCE.read_text(encoding="utf-8")) if PROVENANCE.exists() else {}
    frame_sources = inventory.get("frames", {})
    icon_sources = inventory.get("icons", {})
    issues, edges, metrics = [], [], []
    frames, icons, redrawn = 0, 0, 0
    source_ids = set()
    for folder in forms():
        sizes, pixel_hashes, feet = set(), set(), []
        bbox_by_action = {}
        for action, count in COUNTS.items():
            boxes, action_hashes = [], set()
            for index in range(count):
                path = folder / "main" / f"{action}_{index}.png"
                frames += 1
                found = inspect(path, issues, edges)
                if found is None:
                    continue
                image, box = found
                boxes.append(box)
                sizes.add(image.size)
                if image.width != image.height or not 64 <= image.width <= 512:
                    issues.append(f"invalid canvas: {relative(path)} {image.size}")
                if box[2] - box[0] < 15 or box[3] - box[1] < (4 if action == "death" else 15):
                    issues.append(f"partial silhouette: {relative(path)} {box}")
                if action in ("idle", "walk"):
                    feet.append(box[3])
                pixels = hashlib.sha256(image.tobytes()).hexdigest()
                if pixels in pixel_hashes:
                    issues.append(f"duplicate frame: {relative(path)}")
                pixel_hashes.add(pixels)
                action_hashes.add(pixels)
                source = frame_sources.get(relative(path), {})
                if source.get("sha256") != sha(path):
                    issues.append(f"provenance hash mismatch: {relative(path)}")
                source_id = source.get("source_id")
                if source.get("origin") == "independent_draw" and source_id and source_id not in source_ids:
                    redrawn += 1
                    source_ids.add(source_id)
                elif not structural_only:
                    issues.append(f"frame needs independent art/source: {relative(path)}")
            bbox_by_action[action] = boxes
            if len(action_hashes) < min(3, count):
                issues.append(f"insufficient distinct poses: {relative(folder)} {action}")
        if len(sizes) != 1:
            issues.append(f"inconsistent canvas: {relative(folder)} {sorted(sizes)}")
        if feet and max(feet) - min(feet) > 8:
            issues.append(f"unstable foot anchor: {relative(folder)} {feet}")
        icon = folder / "spawn_icon.png"
        found = inspect(icon, issues, edges, min_margin=1)
        if found:
            icons += 1
            image, box = found
            occupancy = max((box[2] - box[0]) / image.width, (box[3] - box[1]) / image.height)
            if image.width != image.height or not .70 <= occupancy <= .80:
                issues.append(f"icon occupancy outside 70–80%: {relative(icon)} {occupancy:.3f}")
            idle = folder / "main" / "idle_0.png"
            if idle.is_file():
                with Image.open(idle) as source_image:
                    source_image = source_image.convert("RGBA")
                idle_box = source_image.getchannel("A").getbbox()
                if idle_box and image.crop(box).tobytes() == source_image.crop(idle_box).tobytes():
                    issues.append(f"icon is an idle-frame crop: {relative(icon)}")
            source = icon_sources.get(relative(icon), {})
            if source.get("sha256") != sha(icon):
                issues.append(f"icon provenance hash mismatch: {relative(icon)}")
            if source.get("origin") != "independent_draw" and not structural_only:
                issues.append(f"icon needs independent art: {relative(icon)}")
        metrics.append({"form": relative(folder), "canvas": list(sizes)[0] if len(sizes) == 1 else None,
                        "idle_bbox": bbox_by_action["idle"][0] if bbox_by_action["idle"] else None})
    if len(metrics) != 62 or frames != 1364 or icons != 62:
        issues.append(f"count mismatch: {len(metrics)} forms, {frames} frames, {icons} icons")
    return {"forms": len(metrics), "frames": frames, "icons": icons,
            "independent_frames": redrawn, "edge_touching": edges,
            "metrics": metrics, "issues": issues}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--structural-only", action="store_true", help="inspect legacy art without approving it")
    parser.add_argument("--report", type=Path)
    args = parser.parse_args()
    report = run(args.structural_only)
    if args.report:
        args.report.parent.mkdir(parents=True, exist_ok=True)
        args.report.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"{report['forms']} forms, {report['frames']} frames, {report['icons']} icons; "
          f"{report['independent_frames']} independently sourced frames; "
          f"{len(report['edge_touching'])} edge-touching files; {len(report['issues'])} issues")
    for issue in report["issues"][:12]:
        print("  " + issue)
    if len(report["issues"]) > 12:
        print(f"  ... {len(report['issues']) - 12} more (see JSON report)")
    raise SystemExit(bool(report["issues"]))
