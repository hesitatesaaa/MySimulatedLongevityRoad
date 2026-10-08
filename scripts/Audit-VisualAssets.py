"""Audit every PNG and every animated sequence shipped with the mod.

Usage: python scripts/Audit-VisualAssets.py [GameResources] [report.json]
"""

from __future__ import annotations

import json
import hashlib
import re
import sys
from collections import Counter
from pathlib import Path

from PIL import Image


ROOT = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).resolve().parents[1] / "GameResources"
REPORT = Path(sys.argv[2]) if len(sys.argv) > 2 else None
SOULS = (
    "Earth_Soul", "Fire_Soul", "Jin_Soul", "Kong_Soul", "Thunder_Soul",
    "Water_Soul", "Wind_Soul", "Wood_Soul", "Yang_Soul", "Yin_Soul",
)
SOUL_ACTIONS = ("idle", "breathing", "run", "attack", "hit", "death")
HALOS = ("ChangSheng", "HeDao", "HuaShen", "TaiShang")
MARKERS = ("ruin", "secret_realm", "spiritual_convergence", "earthfire", "meteor_stone")
errors: list[str] = []
notes: list[str] = []
decoded: dict[Path, tuple[int, int]] = {}
sequences: dict[str, int] = {}
distinct_frames: dict[str, int] = {}


def fail(message: str) -> None:
    errors.append(message)


def visible(path: Path) -> bool:
    with Image.open(path) as image:
        image.load()
        if "A" not in image.getbands() and "transparency" not in image.info:
            return True
        return image.convert("RGBA").getchannel("A").getbbox() is not None


def frame_digest(image: Image.Image) -> bytes:
    return hashlib.sha256(image.convert("RGBA").tobytes()).digest()


def numbered_sequence(folder: Path, pattern: str, first: int, label: str) -> None:
    files = sorted(folder.glob("*.png"))
    matched: dict[int, Path] = {}
    dimensions: set[tuple[int, int]] = set()
    transparent: list[int] = []
    expression = re.compile(pattern)
    for path in files:
        match = expression.fullmatch(path.name)
        if match is None:
            continue
        index = int(match.group(1))
        if index in matched:
            fail(f"{label}: duplicate frame {index}")
        matched[index] = path
        dimensions.add(decoded[path])
        if not visible(path):
            transparent.append(index)
    if not matched:
        fail(f"{label}: no frames")
        return
    expected = set(range(first, first + len(matched)))
    if set(matched) != expected:
        fail(f"{label}: frame numbers are not contiguous from {first}")
    if len(dimensions) != 1:
        fail(f"{label}: mixed frame sizes {sorted(dimensions)}")
    if transparent:
        if label.endswith("/death") and transparent == [max(matched)]:
            notes.append(f"{label}: final frame is transparent for death fade-out")
        else:
            fail(f"{label}: unexpected fully transparent frames {transparent}")
    sequences[label] = len(matched)
    hashes = []
    for index in sorted(matched):
        with Image.open(matched[index]) as image:
            hashes.append(frame_digest(image))
    distinct_frames[label] = len(set(hashes))
    if len(hashes) > 1 and distinct_frames[label] < 2:
        fail(f"{label}: all animation frames are identical")


def spell_count(number: int) -> int:
    if 1 <= number <= 7 or 13 <= number <= 16:
        return 16
    if 8 <= number <= 10 or 17 <= number <= 20:
        return 20
    return 24


def main() -> int:
    if not ROOT.is_dir():
        fail(f"missing GameResources: {ROOT}")
    else:
        for path in sorted(ROOT.rglob("*.png")):
            try:
                with Image.open(path) as image:
                    if image.format != "PNG":
                        fail(f"not PNG: {path}")
                    image.load()
                    if image.width <= 0 or image.height <= 0:
                        fail(f"empty dimensions: {path}")
                    decoded[path] = image.size
            except Exception as exc:
                fail(f"cannot decode {path}: {exc}")

        souls_root = ROOT / "actors" / "Souls"
        if {p.name for p in souls_root.iterdir() if p.is_dir()} != set(SOULS):
            fail("soul folder list differs from the ten registered souls")
        for soul in SOULS:
            folder = souls_root / soul
            recognized: set[Path] = set()
            for action in SOUL_ACTIONS:
                pattern = rf"{re.escape(soul)}_{action}_(\d{{3}})\.png"
                numbered_sequence(folder, pattern, 0, f"soul/{soul}/{action}")
                recognized.update(p for p in folder.glob("*.png") if re.fullmatch(pattern, p.name))
            for path in folder.glob("*.png"):
                if path not in recognized:
                    # Known hit/death variants are covered above; any extra image needs an explicit loader.
                    fail(f"unmapped soul animation image: {path}")

        immortal_root = ROOT / "actors" / "Immortals"
        for actor in ("Bai", "Chuanfa"):
            for action in ("Idle", "Walk"):
                folder = immortal_root / actor / action
                numbered_sequence(folder, r"(\d+)\.png", 1, f"immortal/{actor}/{action}")
                if sequences.get(f"immortal/{actor}/{action}") != 4:
                    fail(f"immortal/{actor}/{action}: expected four frames")

        halo_root = ROOT / "effects" / "halo"
        for name in HALOS:
            folder = halo_root / name
            numbered_sequence(folder, r"(\d+)\.png", 1, f"halo/{name}")
            if sequences.get(f"halo/{name}") != 18:
                fail(f"halo/{name}: expected 18 frames")

        spells_root = ROOT / "effects" / "Spells"
        spell_files = {p.name for p in spells_root.glob("*.png")}
        expected_spell_files = {f"S{i:03d}.png" for i in range(1, 27)}
        if spell_files != expected_spell_files:
            fail(f"spell atlas list mismatch: missing={sorted(expected_spell_files-spell_files)}, extra={sorted(spell_files-expected_spell_files)}")
        for number in range(1, 27):
            path = spells_root / f"S{number:03d}.png"
            if path not in decoded:
                continue
            count = spell_count(number)
            expected_size = (768, count // 4 * 192)
            if decoded[path] != expected_size:
                fail(f"spell/{number:03d}: atlas size {decoded[path]} != {expected_size}")
                continue
            with Image.open(path) as image:
                alpha = image.convert("RGBA").getchannel("A")
                hashes = []
                for frame in range(count):
                    x = (frame % 4) * 192
                    y = (frame // 4) * 192
                    if alpha.crop((x, y, x + 192, y + 192)).getbbox() is None:
                        fail(f"spell/{number:03d}: transparent frame {frame}")
                    hashes.append(frame_digest(image.crop((x, y, x + 192, y + 192))))
            sequences[f"spell/S{number:03d}"] = count
            distinct_frames[f"spell/S{number:03d}"] = len(set(hashes))
            if len(set(hashes)) < 2:
                fail(f"spell/{number:03d}: all animation frames are identical")

        marker_root = ROOT / "map" / "markers"
        if {p.stem for p in marker_root.glob("*.png")} != set(MARKERS):
            fail("map marker list differs from the five runtime marker kinds")
        for name in MARKERS:
            path = marker_root / f"{name}.png"
            if path not in decoded or not visible(path):
                fail(f"map marker missing or transparent: {name}")

    groups = Counter(path.relative_to(ROOT).parts[0] for path in decoded)
    result = {
        "root": str(ROOT.resolve()),
        "decoded_pngs": len(decoded),
        "groups": dict(sorted(groups.items())),
        "animation_sequences": len(sequences),
        "sequences_with_motion": sum(count > 1 for count in distinct_frames.values()),
        "minimum_distinct_frames": min(distinct_frames.values(), default=0),
        "soul_frames": sum(v for k, v in sequences.items() if k.startswith("soul/")),
        "immortal_frames": sum(v for k, v in sequences.items() if k.startswith("immortal/")),
        "halo_frames": sum(v for k, v in sequences.items() if k.startswith("halo/")),
        "spell_frames": sum(v for k, v in sequences.items() if k.startswith("spell/")),
        "notes": notes,
        "errors": errors,
    }
    if REPORT:
        REPORT.parent.mkdir(parents=True, exist_ok=True)
        REPORT.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
