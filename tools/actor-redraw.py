"""Independent-frame authoring, provenance and release checks. Never slices sprite sheets."""
from __future__ import annotations

import argparse
from collections import Counter
from datetime import datetime, timezone
import hashlib
import importlib.util
import json
import math
from pathlib import Path
import re
import shutil
import statistics
import zipfile

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
RESOURCES = ROOT / "resources"
WORK = ROOT / "output" / "actor-redraw"
ACTIONS = ("idle", "walk", "attack", "hit", "death")
PRESETS = {
    "human": (128, (8, 8, 12, 4, 10), "TenguSamurai", "Earth_Soul"),
    "bird": (128, (10, 12, 14, 4, 12), "Phoenix", "Wind_Soul"),
    "dragon": (256, (10, 12, 16, 5, 14), "PrimordialFrostDragon", "Water_Soul"),
    "quadruped": (96, (8, 8, 12, 4, 10), "NightWolf", "Jin_Soul"),
    "aquatic": (256, (10, 12, 14, 4, 12), "Behemoth", "Water_Soul"),
    "heavy": (128, (8, 8, 14, 4, 12), "StoneGolem", "Earth_Soul"),
    "special": (96, (8, 10, 12, 4, 10), "SpiritTraveler", "Yin_Soul"),
}
BIRDS = {"zhuque", "fenghuang", "dafeng", "manman", "xuanniao", "qingluan"}
DRAGONS = {"qinglong", "huanglong", "qingshenlong"}
AQUATIC = {"kunpeng", "liejiejing"}
HEAVY = {"xuanwu", "tianshou"}
SPECIAL = {"feiqiu", "yingying"}
BEAST_SOURCE = ROOT / "code/MySimulatedLongevityRoad/Data/MclslBeastCatalog.cs"
NAMED_SOURCE = ROOT / "code/MySimulatedLongevityRoad/Traits/MclslNamedCharacterRegistration.cs"


def digest(file: Path) -> str:
    return hashlib.sha256(file.read_bytes()).hexdigest()


def save_json(file: Path, data) -> None:
    file.parent.mkdir(parents=True, exist_ok=True)
    temp = file.with_suffix(file.suffix + ".tmp")
    temp.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    temp.replace(file)


def load_json(file: Path):
    return json.loads(file.read_text(encoding="utf-8-sig"))


def frame_indices(clip):
    order = clip.get("frameOrder", list(range(clip["count"])))
    if (len(order) != clip["count"] or any(type(i) is not int or i < 0 for i in order)
        or len(set(order)) != clip["count"]):
        raise ValueError("Frame order must include each authored action frame exactly once")
    return order


def within(root: Path, relative: str) -> Path:
    target = (root / relative).resolve()
    if not target.is_relative_to(root.resolve()):
        raise ValueError(f"Path escapes {root}: {relative}")
    return target


def image_info(file: Path):
    with Image.open(file) as source:
        source.load()
        if source.format != "PNG":
            raise ValueError(f"Not PNG: {file}")
        image = source.convert("RGBA")
        bounds = image.getchannel("A").point(lambda a: 255 if a >= 128 else 0).getbbox()
        return {"canvas": list(image.size), "bodyBounds": list(bounds) if bounds else None,
                "rgbaSha256": hashlib.sha256(image.tobytes()).hexdigest(),
                "sha256": digest(file)}


def check_source_edges(file: Path):
    """Padding a normalized frame must never hide clipping in its original."""
    with Image.open(file) as source:
        alpha = source.convert("RGBA").getchannel("A")
        bounds = alpha.point(lambda a: 255 if a >= 128 else 0).getbbox()
        if not bounds or bounds[0] <= 0 or bounds[1] <= 0 or bounds[2] >= source.width or bounds[3] >= source.height:
            raise ValueError(f"Empty or potentially clipped generated original: {file}, {bounds}, {source.size}")


def definitions():
    named = {i: (n, f, bool(b)) for i, n, f, b in re.findall(
        r'new\("([^"\n]+)", "([^"\n]+)", "([^"\n]+)"(, true)?\)',
        NAMED_SOURCE.read_text(encoding="utf-8-sig"))}
    beasts = re.findall(r'new\("([^"\n]+)", "([^"\n]+)", "([^"\n]+)", MclslBeastHabitat\.',
                        BEAST_SOURCE.read_text(encoding="utf-8-sig"))
    forms, icons = [], []
    for identifier, name, _ in beasts:
        folder = "actors/Named/" + named[identifier][1] if identifier in named else "actors/Beasts/" + identifier
        actor = ("mclsl_named_" if identifier in named else "mclsl_beast_") + identifier
        kind = ("bird" if identifier in BIRDS else "dragon" if identifier in DRAGONS else
                "aquatic" if identifier in AQUATIC else "heavy" if identifier in HEAVY else
                "special" if identifier in SPECIAL else "quadruped")
        for suffix, shape, family in (("", "Beast", kind), ("_yuan", "Human", "human")):
            forms.append({"id": actor + suffix, "name": name + ("·化形" if suffix else "·兽形"),
                          "species": identifier, "resourcePath": folder + "/" + shape, "family": family})
        icons.append({"actorId": actor, "name": name, "formPath": folder + "/Beast"})
    for identifier, (name, folder, beast) in named.items():
        if beast:
            continue
        actor = "mclsl_named_" + identifier
        forms.append({"id": actor, "name": name, "species": identifier,
                      "resourcePath": "actors/Named/" + folder, "family": "human"})
        icons.append({"actorId": actor, "name": name, "formPath": "actors/Named/" + folder})
    for actor, name, folder in (("mclsl_bai_xiansheng", "白先生", "Bai"),
                                ("mclsl_chuanfa_tianzun", "传法天尊", "Chuanfa")):
        forms.append({"id": actor, "name": name, "species": folder,
                      "resourcePath": "actors/Immortals/" + folder, "family": "human"})
        icons.append({"actorId": actor, "name": name, "formPath": "actors/Immortals/" + folder})
    if len(forms) != 62 or len(icons) != 38:
        raise ValueError(f"Unexpected source catalog: {len(forms)} forms, {len(icons)} icons")
    return forms, icons


def prepare(args):
    manifest_file = RESOURCES / "actor-frames.manifest.json"
    targets_file = RESOURCES / "actor-redraw.targets.json"
    if manifest_file.exists() or targets_file.exists():
        raise ValueError("Already initialized; refuse to overwrite manifest or production targets")
    forms, icons = definitions()
    refs = set(p[2] for p in PRESETS.values()) | {"Behemoth"}
    reference_root = ROOT / "reference/western-fantasy"
    with zipfile.ZipFile(args.zip) as archive:
        for entry in archive.infolist():
            match = re.search(r"/actors/FantasyCreatures/[^/]+/([^/]+)/Main/[^/]+\.png$", entry.filename)
            is_icon = any(entry.filename.endswith("/ui/Icons/FantasyCreaturesSet/" + r + ".png") for r in refs)
            if not ((match and match[1] in refs) or is_icon):
                continue
            relative = entry.filename.split("/", 1)[1]
            target = within(reference_root, relative)
            target.parent.mkdir(parents=True, exist_ok=True)
            content = archive.read(entry)
            if target.exists() and target.read_bytes() != content:
                raise ValueError(f"Existing reference differs from supplied ZIP: {target}")
            target.write_bytes(content)
    active, reference_data = [], {}
    reference_source = (reference_root / "code/FantasyCreatures/FantasyCreatures.cs").read_text(encoding="utf-8-sig")
    scale_block = reference_source.split("creatureScaleFactors =", 1)[1].split("};", 1)[0]
    factors = dict((key, float(value)) for key, value in re.findall(r'\{ "([^"]+)", ([\d.]+)f \}', scale_block))
    for ref in sorted(refs):
        frame = next(reference_root.glob("GameResources/actors/FantasyCreatures/*/" + ref + "/Main/idle_0.png"))
        info = image_info(frame)
        reference_data[ref] = {"frame": frame.relative_to(ROOT).as_posix(), **info,
                               "scaleFactor": factors[ref], "baseTemplate": "$mob$",
                               "referencePPU": None, "referenceBaseScale": None, "verifiedInGame": False}
    for form in forms:
        family = form["family"]
        canvas, counts, ref, soul = PRESETS[family]
        if form["species"] == "tianshou" and family == "heavy":
            ref = "Behemoth"
        group_id = form["id"]
        current = ROOT / "GameResources" / form["resourcePath"] / "main"
        first = image_info(current / "idle_0.png")
        form.update({"canvas": [canvas, canvas], "anchor": [canvas // 2, canvas - 16],
                     "bodyHeight": 88 if family == "human" else round(canvas * (0.48 if family == "bird" else 0.58 if family == "quadruped" else 0.66)),
                     "bodyLongestEdge": round(canvas * 0.78) if family == "aquatic" else None,
                     "pixelsPerUnit": 1, "reference": ref, "soulReference": soul,
                     "referenceCard": f"resources/actor-reference-cards/{group_id}.json",
                     "actions": {a: {"count": n, "durationMs": (140 if a == "idle" else 90 if a == "walk" else
                                                                    60 if a == "attack" else 80 if a == "hit" else 90),
                                      "loop": a in ("idle", "walk")} for a, n in zip(ACTIONS, counts)}})
        save_json(ROOT / form["referenceCard"], {
            "actorId": group_id, "name": form["name"], "family": family,
            "identityReference": "GameResources/" + form["resourcePath"] + "/main/idle_0.png",
            "worldReference": reference_data[ref],
            "soulReferenceFolder": "GameResources/actors/Souls/" + soul,
            "constraints": ["independent-generation-per-frame", "no-spritesheet-slicing", "preserve-identity-and-palette",
                            "consistent-canvas-and-anchor", "pixel-art", "five-actions"],
            "calibration": {"metric": "bodyHeight" if family == "human" else "bodyLongestEdge",
                            "allowedRelativeError": 0.1, "verifiedInGame": False}})
        actions = {}
        for action in ACTIONS:
            files = sorted(current.glob(action + "_*.png"), key=lambda p: int(p.stem.rsplit("_", 1)[1]))
            if not files:
                raise ValueError(f"Missing legacy action: {current}/{action}")
            old_duration = {"idle": 14 / 60, "walk": 10 / 60, "attack": 6 / 60, "hit": 5 / 60, "death": 7 / 60}[action]
            actions[action] = {"loop": action in ("idle", "walk"), "frames": [
                {"name": p.stem, "durationMs": old_duration * 1000, "sha256": digest(p), "origin": "baseline-0.5.4"}
                for p in files]}
        active.append({"actorId": group_id, "resourcePath": form["resourcePath"], "canvas": first["canvas"],
                       "anchor": [first["canvas"][0] / 2, first["canvas"][1] / 2], "pixelsPerUnit": 1,
                       "approved": False, "anchorReviewed": False, "actions": actions,
                       "calibration": {"renderScale": 0.2 if family == "human" else 0.18,
                                       "reference": ref, "verifiedInGame": False}})
    for icon in icons:
        icon.update({"canvas": [128, 128], "bodyLongestEdge": 94,
                     "resourcePath": "ui/ActorIcons/" + icon["actorId"], "approved": False})
    target_frames = sum(sum(a["count"] for a in f["actions"].values()) for f in forms)
    if target_frames != 2741:
        raise ValueError(f"Unexpected target frame count: {target_frames}")
    save_json(targets_file, {"schemaVersion": 1, "targetVersion": "0.6.0", "productionMode": "built-in-imagegen",
                             "singleFrameNormalizationAuthorized": True, "referenceZipSha256": digest(Path(args.zip)),
                             "forms": forms, "icons": icons, "totalFrames": target_frames,
                             "references": reference_data})
    save_json(manifest_file, {"schemaVersion": 2, "status": "development-legacy-resources",
                              "groups": active, "icons": icons})
    save_json(WORK / "progress.json", {"generatedFrames": 0, "generatedIcons": 0,
                                      "installedForms": 0, "verifiedInGame": False, "releaseReady": False})
    print(json.dumps({"forms": len(forms), "icons": len(icons), "targetFrames": target_frames,
                      "legacyFrames": sum(len(a["frames"]) for g in active for a in g["actions"].values())}))


def normalize(args):
    targets = load_json(RESOURCES / "actor-redraw.targets.json")
    form = next(f for f in targets["forms"] if f["id"] == args.actor)
    icon = args.action == "icon"
    if not icon and (args.action not in form["actions"] or args.index not in frame_indices(form["actions"][args.action])):
        raise ValueError("Frame is outside target action")
    canvas = (128, 128) if icon else tuple(form["canvas"])
    anchor = (64, 112) if icon else tuple(form["anchor"])
    output = within(WORK, f"icons/{args.actor}.png" if icon else f"frames/{args.actor}/main/{args.action}_{args.index}.png")
    if output.exists() and not args.realign:
        raise ValueError(f"Frame already exists; refuse silent overwrite: {output}")
    if args.realign and not output.with_suffix(".provenance.json").exists():
        raise ValueError("Realignment requires existing independent-frame provenance")
    source = Path(args.source).resolve()
    existing_record = load_json(output.with_suffix(".provenance.json")) if args.realign else None
    if existing_record and digest(source) != existing_record["sourceSha256"]:
        raise ValueError("Realignment must use the preserved, independently generated original")
    with Image.open(source) as raw:
        frame = raw.convert("RGBA")
    cleanup_authorized = targets.get("singleFrameAlphaCleanupAuthorized") is True
    alpha_histogram = frame.getchannel("A").histogram()
    cleared_pixels = sum(alpha_histogram[1:16]) if cleanup_authorized else 0
    if cleanup_authorized:
        frame.putalpha(frame.getchannel("A").point(lambda a: 0 if a < 16 else a))
    bounds = frame.getchannel("A").point(lambda a: 255 if a >= 128 else 0).getbbox()
    if not bounds:
        raise ValueError("Empty generated frame")
    if bounds[0] <= 0 or bounds[1] <= 0 or bounds[2] >= frame.width or bounds[3] >= frame.height:
        raise ValueError(f"Generated subject touches source canvas edge; redraw rather than concealing possible clipping: {bounds}, {frame.size}")
    width, height = bounds[2] - bounds[0], bounds[3] - bounds[1]
    pose_profile_file = within(WORK, f"frames/{args.actor}/normalization-profile.json")
    if icon:
        scale = 94 / max(width, height)
        pose_profile = None
    elif pose_profile_file.exists():
        pose_profile = load_json(pose_profile_file)
        if pose_profile["sourceCanvas"] != list(frame.size):
            raise ValueError("Generated frame canvas changed; redraw with the same source canvas")
        scale = pose_profile["scale"]
    else:
        if args.action != "idle" or args.index != 0:
            raise ValueError("Author idle_0 first to establish one fixed pixel scale for the entire form")
        scale = form["bodyLongestEdge"] / max(width, height) if form.get("bodyLongestEdge") else form["bodyHeight"] / height
        pose_profile = {"sourceCanvas": list(frame.size), "scale": scale,
                        "sourceAnchorX": (bounds[0] + bounds[2]) / 2, "sourceAnchorY": bounds[3]}
    if pose_profile is not None and "sourceAnchorY" not in pose_profile:
        with Image.open(WORK / f"originals/{args.actor}/idle_0.png") as standard:
            standard_bounds = standard.convert("RGBA").getchannel("A").point(lambda a: 255 if a >= 128 else 0).getbbox()
        pose_profile["sourceAnchorY"] = standard_bounds[3]
        save_json(pose_profile_file, pose_profile)
    # Resize the complete single-frame image, without cropping or quantizing alpha/RGB.
    resized_size = tuple(max(1, round(s * scale)) for s in frame.size)
    resized = frame.resize(resized_size, Image.Resampling.NEAREST)
    resized_bounds = resized.getchannel("A").getbbox()
    if not resized_bounds:
        raise ValueError("Normalization lost the generated subject")
    source_anchor_x = pose_profile["sourceAnchorX"] if not icon else None
    if not icon and form["species"] == "qingluan" and form["family"] == "bird" and args.action in ("idle", "attack", "hit"):
        foot_pixels = []
        for y in range(max(bounds[1], bounds[3] - 40), bounds[3]):
            for x in range(bounds[0], bounds[2]):
                r, g, b, a = frame.getpixel((x, y))
                if a >= 128 and r > 130 and g > 85 and b < 110:
                    foot_pixels.append(x)
        if foot_pixels:
            source_anchor_x = statistics.median(foot_pixels)
    pose_offset = [0, 0] if icon else pose_profile.get("actionOffsets", {}).get(args.action, [0, 0])
    if len(pose_offset) != 2 or not all(isinstance(v, (int, float)) and math.isfinite(v) for v in pose_offset):
        raise ValueError("Invalid reviewed pose alignment offset")
    frame_offset = [0, 0] if icon else pose_profile.get("frameOffsets", {}).get(f"{args.action}_{args.index}", [0, 0])
    if len(frame_offset) != 2 or not all(isinstance(v, (int, float)) and math.isfinite(v) for v in frame_offset):
        raise ValueError("Invalid per-frame pose alignment")
    pose_offset = [pose_offset[i] + frame_offset[i] for i in range(2)]
    dx = round(anchor[0] - (resized_bounds[0] + resized_bounds[2]) / 2) if icon else round(anchor[0] - source_anchor_x * scale + pose_offset[0])
    requested_dx = dx
    minimum_dx, maximum_dx = 2 - resized_bounds[0], canvas[0] - 2 - resized_bounds[2]
    if minimum_dx <= maximum_dx:
        dx = min(max(dx, minimum_dx), maximum_dx)
    solid_bounds = resized.getchannel("A").point(lambda a: 255 if a >= 128 else 0).getbbox()
    grounded = form["family"] in ("human", "quadruped", "heavy") or (form["family"] == "bird" and args.action != "walk")
    ground_bottom = (resized_bounds[3] if icon else solid_bounds[3]
                     if grounded
                     else pose_profile["sourceAnchorY"] * scale)
    foot_bottom = None
    if not icon and form["species"] == "qingluan" and args.action in ("idle", "attack", "hit"):
        # The low tail feathers must not lift the grounded golden toes.
        foot_center = round(source_anchor_x * scale)
        toes = []
        for y in range(max(0, solid_bounds[3] - 20), solid_bounds[3]):
            for x in range(max(0, foot_center - 12), min(resized.width, foot_center + 13)):
                r, g, b, a = resized.getpixel((x, y))
                if a >= 128 and r > 130 and g > 85 and b < 110:
                    toes.append(y)
        if not toes:
            raise ValueError("Cannot locate Qingluan grounded golden toes")
        foot_bottom = max(toes) + 2  # Includes the one-pixel dark toe outline.
        ground_bottom = foot_bottom
    dy = round(anchor[1] - ground_bottom + pose_offset[1])
    requested_dy = dy
    # Retain faint alpha below a grounded pose rather than cutting it off.
    # A minimal canvas translation is authorized; review records its anchor delta.
    minimum_dy, maximum_dy = 2 - resized_bounds[1], canvas[1] - 2 - resized_bounds[3]
    if minimum_dy <= maximum_dy:
        dy = min(max(dy, minimum_dy), maximum_dy)
    visible = (dx + resized_bounds[0], dy + resized_bounds[1], dx + resized_bounds[2], dy + resized_bounds[3])
    if visible[0] < 2 or visible[1] < 2 or visible[2] > canvas[0] - 2 or visible[3] > canvas[1] - 2:
        raise ValueError(f"Generated pose does not fit target canvas without clipping: {visible}, {canvas}")
    normalized = Image.new("RGBA", canvas)
    normalized.alpha_composite(resized, (dx, dy))
    output.parent.mkdir(parents=True, exist_ok=True)
    originals = within(WORK, f"originals/{args.actor}/{args.action}_{args.index}.png")
    originals.parent.mkdir(parents=True, exist_ok=True)
    if source != originals:
        shutil.copyfile(source, originals)
    image_temp = output.with_suffix(".tmp.png")
    normalized.save(image_temp)
    image_temp.replace(output)
    if not icon and not pose_profile_file.exists():
        save_json(pose_profile_file, pose_profile)
    prompt_file = Path(args.prompt_file)
    record = {"actorId": args.actor, "action": args.action, "index": args.index,
              "artDirectionRevision": existing_record.get("artDirectionRevision", 0) if existing_record else targets.get("qualityPolicy", {}).get("artDirectionRevision", 0),
              "method": "built-in-imagegen-independent-frame", "sourceFile": originals.relative_to(ROOT).as_posix(),
              "sourceSha256": digest(originals), "outputFile": output.relative_to(ROOT).as_posix(),
              "outputSha256": digest(output), "createdAtUtc": datetime.now(timezone.utc).isoformat(),
              "prompt": existing_record["prompt"] if existing_record else prompt_file.read_text(encoding="utf-8-sig"),
              "references": existing_record["references"] if existing_record else args.reference,
              "normalization": {"authorized": True, "resampling": "nearest-neighbor", "sourceCanvas": list(frame.size),
                                "scale": scale, "translation": [dx, dy], "canvas": list(canvas),
                                "groundAlignmentDelta": dy - requested_dy,
                                "horizontalAlignmentDelta": dx - requested_dx,
                                "sourceAnchorX": source_anchor_x,
                                "groundedFootBottom": foot_bottom,
                                "poseAlignmentOffset": pose_offset,
                                "alphaPreserved": cleared_pixels == 0,
                                "alphaCleanup": {"authorized": cleanup_authorized, "cutoffExclusive": 16,
                                                 "clearedSourcePixelCount": cleared_pixels, "preservedAlphaAtOrAboveCutoff": True},
                                "cropped": False, "spritesheetSlicing": False},
              "review": {"identity": False, "pixelEdges": False, "anchor": False, "continuity": False}}
    save_json(output.with_suffix(".provenance.json"), record)
    print(json.dumps({"output": str(output), **image_info(output)}, ensure_ascii=False))


def jobs(args):
    targets = load_json(RESOURCES / "actor-redraw.targets.json")
    selected = [f for f in targets["forms"] if f["id"] == args.actor]
    if not selected:
        raise ValueError("Unknown actor")
    form = selected[0]
    first_original = WORK / f"originals/{args.actor}/idle_0.png"
    if not first_original.exists():
        raise ValueError("Generate and inspect idle_0 before requesting animation jobs")
    specs = []
    for action in ACTIONS:
        if form["actions"][action].get("generationRequired") is False:
            continue
        count = form["actions"][action]["count"]
        for index in range(count):
            output = WORK / f"frames/{args.actor}/main/{action}_{index}.png"
            if output.exists():
                continue
            specs.append({"actorId": args.actor, "name": form["name"], "family": form["family"],
                          "action": action, "index": index, "count": count,
                          "sourceCanvas": load_json(WORK / f"frames/{args.actor}/normalization-profile.json")["sourceCanvas"],
                          "targetCanvas": form["canvas"], "reference": str(first_original),
                          "output": str(output), "soulReference": form["soulReference"],
                          "promptFile": str(WORK / f"prompts/{args.actor}/{action}_{index}.txt")})
    print(json.dumps(specs, ensure_ascii=False))


def preview(args):
    targets = load_json(RESOURCES / "actor-redraw.targets.json")
    form = next(f for f in targets["forms"] if f["id"] == args.actor)
    clip = form["actions"][args.action]
    frames, hashes, source_indices = [], [], []
    for index in frame_indices(clip):
        file = WORK / f"frames/{args.actor}/main/{args.action}_{index}.png"
        if args.partial and not file.exists():
            continue
        record = load_json(file.with_suffix(".provenance.json"))
        if record["outputSha256"] != digest(file):
            raise ValueError("Staged frame provenance mismatch: " + str(file))
        if digest(within(ROOT, record["sourceFile"])) != record["sourceSha256"]:
            raise ValueError("Generated original provenance mismatch: " + str(file))
        with Image.open(file) as source:
            image = source.convert("RGBA")
        if list(image.size) != form["canvas"]:
            raise ValueError("Staged canvas mismatch")
        hashes.append(hashlib.sha256(image.tobytes()).hexdigest())
        source_indices.append(index)
        checker = Image.new("RGB", image.size, (58, 74, 66))
        checker.paste(image, (0, 0), image)
        frames.append(checker.resize((image.width * 3, image.height * 3), Image.Resampling.NEAREST))
    if not frames:
        raise ValueError("No authored frames to preview")
    out = WORK / f"previews/{args.actor}/{args.action}{'-partial' if args.partial else ''}.gif"
    out.parent.mkdir(parents=True, exist_ok=True)
    playback = {"loop": 0} if clip["loop"] else {}
    frames[0].save(out, save_all=True, append_images=frames[1:], duration=clip["durationMs"],
                   disposal=2, **playback)
    columns = 4
    cell_width, cell_height = frames[0].width, frames[0].height + 24
    contact = Image.new("RGB", (columns * cell_width, math.ceil(len(frames) / columns) * cell_height), (35, 42, 39))
    labels = ImageDraw.Draw(contact)
    for index, image in enumerate(frames):
        x, y = index % columns * cell_width, index // columns * cell_height
        contact.paste(image, (x, y + 24))
        labels.text((x + 8, y + 6), f"{args.action}_{source_indices[index]} | {clip['durationMs']} ms", fill="white")
    contact.save(out.with_suffix(".png"))
    save_json(out.with_suffix(".json"), {"actorId": args.actor, "action": args.action,
                                        "frames": len(frames), "uniqueRgbaFrames": len(set(hashes)),
                                        "complete": len(frames) == clip["count"],
                                        "frameOrder": source_indices,
                                        "previewOnly": True, "source": "independent-PNG-frames"})
    print(json.dumps({"preview": str(out), "frames": len(frames), "uniqueFrames": len(set(hashes))}))


def install(args):
    """Install one fully reviewed form; preserve the previous files before replacement."""
    targets = load_json(RESOURCES / "actor-redraw.targets.json")
    form = next(f for f in targets["forms"] if f["id"] == args.actor)
    manifest_file = RESOURCES / "actor-frames.manifest.json"
    manifest = load_json(manifest_file)
    group = next(g for g in manifest["groups"] if g["actorId"] == args.actor)
    review = load_json(Path(args.review_file))
    if review.get("actorId") != args.actor or not review.get("offlinePassed"):
        raise ValueError("A complete, explicit offline review is required before installing")
    verified, sources = [], set()
    for action, clip in form["actions"].items():
        for index in frame_indices(clip):
            name = f"{action}_{index}"
            file = within(WORK, f"frames/{args.actor}/main/{name}.png")
            provenance = load_json(file.with_suffix(".provenance.json"))
            info = image_info(file)
            checks = review.get("frames", {}).get(name, {})
            if (checks.get("sha256") != info["sha256"] or
                not all(checks.get(k) is True for k in ("identity", "pixelEdges", "anchor", "continuity"))):
                raise ValueError("Missing review or changed frame: " + name)
            original = within(ROOT, provenance["sourceFile"])
            check_source_edges(original)
            if (provenance.get("method") != "built-in-imagegen-independent-frame" or
                provenance.get("actorId") != args.actor or provenance.get("action") != action or provenance.get("index") != index or
                provenance.get("outputSha256") != info["sha256"] or digest(original) != provenance.get("sourceSha256") or
                provenance["sourceSha256"] in sources or info["canvas"] != form["canvas"] or not info["bodyBounds"] or
                provenance.get("normalization", {}).get("cropped") is not False or
                provenance.get("normalization", {}).get("spritesheetSlicing") is not False):
                raise ValueError("Invalid independent source: " + name)
            sources.add(provenance["sourceSha256"])
            verified.append((name, action, file, original, provenance, checks))
    icon_target = next((i for i in targets["icons"] if i["actorId"] == args.actor), None)
    icon_file = within(WORK, f"icons/{args.actor}.png")
    icon_record = None
    if icon_file.exists() and icon_target:
        icon_record = load_json(icon_file.with_suffix(".provenance.json"))
        check_source_edges(within(ROOT, icon_record["sourceFile"]))
        info = image_info(icon_file)
        if (review.get("icon", {}).get("sha256") != info["sha256"] or not review["icon"].get("offlinePassed") or
            info["canvas"] != [128, 128] or not info["bodyBounds"] or
            icon_record.get("method") != "built-in-imagegen-independent-frame" or
            icon_record.get("outputSha256") != info["sha256"] or
            digest(within(ROOT, icon_record["sourceFile"])) != icon_record.get("sourceSha256") or
            icon_record["sourceSha256"] in sources):
            raise ValueError("Invalid or unreviewed independent icon")
    # All checks finish before touching production files.
    snapshot = within(WORK, f"install-backups/{args.actor}/{datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%S%f')}")
    destination = within(ROOT / "GameResources", form["resourcePath"] + "/main")
    snapshot.mkdir(parents=True)
    shutil.copytree(destination, snapshot / "main")
    shutil.copyfile(manifest_file, snapshot / "manifest.json")
    proof_root = within(RESOURCES, "actor-provenance/" + args.actor)
    actions = {a: {"loop": c["loop"], "frames": []} for a, c in form["actions"].items()}
    for name, action, staged, original, provenance, checks in verified:
        target = destination / (name + ".png")
        temporary = target.with_suffix(".installing")
        shutil.copyfile(staged, temporary)
        temporary.replace(target)
        saved_original = proof_root / "originals" / (name + ".png")
        saved_original.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(original, saved_original)
        provenance.update({"sourceFile": saved_original.relative_to(ROOT).as_posix(),
                           "outputFile": target.relative_to(ROOT).as_posix(),
                           "review": {k: checks[k] for k in ("identity", "pixelEdges", "anchor", "continuity")}})
        proof_file = proof_root / (name + ".json")
        save_json(proof_file, provenance)
        actions[action]["frames"].append({"name": name, "durationMs": form["actions"][action]["durationMs"],
                                         "sha256": digest(target), "origin": "independently-redrawn",
                                         "provenance": proof_file.relative_to(ROOT).as_posix()})
    group.update({"canvas": form["canvas"], "anchor": form["anchor"], "pixelsPerUnit": form["pixelsPerUnit"],
                  "approved": True, "anchorReviewed": True, "actions": actions})
    # World calibration stays unverified until it has actual game evidence.
    group["calibration"]["verifiedInGame"] = False
    if icon_record:
        entry = next(i for i in manifest["icons"] if i["actorId"] == args.actor)
        icon_destination = within(ROOT / "GameResources", icon_target["resourcePath"] + ".png")
        icon_destination.parent.mkdir(parents=True, exist_ok=True)
        if icon_destination.exists():
            shutil.copyfile(icon_destination, snapshot / "icon.png")
        shutil.copyfile(icon_file, icon_destination)
        original = proof_root / "originals" / "icon.png"
        shutil.copyfile(within(ROOT, icon_record["sourceFile"]), original)
        icon_record.update({"sourceFile": original.relative_to(ROOT).as_posix(),
                            "outputFile": icon_destination.relative_to(ROOT).as_posix(),
                            "review": review["icon"]})
        proof_file = proof_root / "icon.json"
        save_json(proof_file, icon_record)
        entry.update({"sha256": digest(icon_destination), "approved": True, "verifiedInGame": False,
                      "provenance": proof_file.relative_to(ROOT).as_posix()})
    manifest["status"] = "development-redraw-in-progress"
    save_json(manifest_file, manifest)
    names = {name + ".png" for name, *_ in verified}
    removed = []
    for old_file in destination.glob("*.png"):
        if re.fullmatch(r"(?:idle|walk|attack|hit|death)_\d+\.png", old_file.name) and old_file.name not in names:
            # Exact files are already backed up. Never recursively delete a computed directory.
            old_file.unlink()
            removed.append(old_file.name)
    save_json(snapshot / "installation.json", {"actorId": args.actor, "frames": len(verified),
                                               "icon": bool(icon_record), "removedOldFiles": removed,
                                               "worldCalibrationPassed": False})
    print(json.dumps({"installed": args.actor, "frames": len(verified), "icon": bool(icon_record),
                      "removedOldFiles": removed, "backup": str(snapshot)}))


def validate(args):
    policy_file = RESOURCES / "0.6.0-release-policy.json"
    policy = load_json(policy_file) if policy_file.exists() else {}
    visual_waived = (policy.get("targetVersion") == "0.6.0"
                     and policy.get("visualReview") == "waived-by-user"
                     and policy.get("requireUnityValidation") is True)
    manifest = load_json(RESOURCES / "actor-frames.manifest.json")
    targets = load_json(RESOURCES / "actor-redraw.targets.json")
    errors, pending, duplicates = [], [], []
    expected = {f["resourcePath"]: f for f in targets["forms"]}
    groups = manifest.get("groups", [])
    paths = [g.get("resourcePath") for g in groups]
    if len(paths) != len(set(paths)) or set(paths) != set(expected):
        errors.append("Manifest does not cover exactly the 62 registered forms")
    frames_checked = 0
    source_owners = {}
    for group in groups:
        resource = group["resourcePath"]
        target = expected.get(resource)
        seen_pixels = {}
        integrated = visual_waived and group.get("integrationAccepted") and group.get("geometryConfigured")
        if (not (group.get("approved") or integrated) or not (group.get("anchorReviewed") or integrated)
                or not group.get("calibration", {}).get("verifiedInGame")):
            pending.append("Unapproved or uncalibrated form: " + resource)
        for action in ACTIONS:
            clip = group.get("actions", {}).get(action, {})
            frames = clip.get("frames", [])
            if clip.get("visualFallback") is not None:
                spec = target.get("actions", {}).get(action, {}) if target else {}
                if (action not in ("attack", "hit") or clip["visualFallback"] != "idle"
                        or spec.get("generationRequired", True) or frames or clip.get("loop") is not False
                        or not isinstance(clip.get("durationMs"), (int, float))
                        or not math.isfinite(clip["durationMs"]) or clip["durationMs"] <= 0):
                    errors.append("Invalid waived combat fallback: " + resource + "/" + action)
                continue
            if not frames or clip.get("loop") != (action in ("idle", "walk")):
                errors.append("Invalid action: " + resource + "/" + action)
            names = [f.get("name") for f in frames]
            if target and (group.get("approved") or integrated) and names != [f"{action}_{i}" for i in frame_indices(target["actions"][action])]:
                errors.append("Installed action differs from reviewed target order: " + resource + "/" + action)
            if len(names) != len(set(names)):
                errors.append("Duplicate frame names: " + resource + "/" + action)
            for frame in frames:
                try:
                    name = frame["name"]
                    if not re.fullmatch(re.escape(action) + r"_\d+", name):
                        raise ValueError("Invalid frame name: " + name)
                    duration = frame["durationMs"]
                    if isinstance(duration, bool) or not isinstance(duration, (int, float)) or not math.isfinite(duration) or duration <= 0:
                        raise ValueError("Invalid duration: " + name)
                    native = group.get("nativeResourcePath")
                    file = within(ROOT / ("NativeActorResources" if native else "GameResources"),
                                  (native or resource) + "/main/" + name + ".png")
                    info = image_info(file)
                    expected_canvas = frame.get("canvas", group["canvas"])
                    if info["canvas"] != expected_canvas or not info["bodyBounds"]:
                        raise ValueError("Canvas mismatch or empty frame: " + str(file))
                    if native and (not (frame.get("anchorReviewed") or integrated and frame.get("geometryConfigured")) or not frame.get("anchor")
                                   or not frame.get("pixelsPerUnit")):
                        raise ValueError("Unreviewed native geometry: " + name)
                    if frame.get("sha256") != info["sha256"]:
                        raise ValueError("Hash mismatch: " + str(file))
                    frames_checked += 1
                    if info["rgbaSha256"] in seen_pixels:
                        duplicates.append(resource + "/" + name + " == " + seen_pixels[info["rgbaSha256"]])
                    seen_pixels[info["rgbaSha256"]] = name
                    if frame.get("origin") != "independently-redrawn":
                        pending.append("Legacy frame: " + resource + "/" + name)
                    else:
                        provenance = load_json(within(ROOT, frame["provenance"]))
                        if provenance.get("outputSha256") != info["sha256"] or provenance.get("method") != "built-in-imagegen-independent-frame":
                            raise ValueError("Invalid independent-frame provenance: " + name)
                        original = within(ROOT, provenance["sourceFile"])
                        if not visual_waived:
                            check_source_edges(original)
                        if (digest(original) != provenance.get("sourceSha256") or provenance.get("actorId") != group["actorId"] or
                            provenance.get("action") != action or provenance.get("index") != int(name.rsplit("_", 1)[1]) or
                            provenance.get("normalization", {}).get("cropped") is not False or
                            provenance.get("normalization", {}).get("spritesheetSlicing") is not False):
                            raise ValueError("Invalid independent source or normalization: " + resource + "/" + name)
                        source_hash = provenance["sourceSha256"]
                        if source_hash in source_owners:
                            raise ValueError("Generated original reused: " + source_owners[source_hash] + " and " + resource + "/" + name)
                        source_owners[source_hash] = resource + "/" + name
                        if not visual_waived and not all(provenance.get("review", {}).get(k) for k in ("identity", "pixelEdges", "anchor", "continuity")):
                            pending.append("Unreviewed frame: " + resource + "/" + name)
                except (ValueError, KeyError, OSError) as ex:
                    errors.append(str(ex))
        if target and group.get("approved") and not group.get("nativeResourcePath") and group.get("canvas") != target["canvas"]:
            errors.append("Approved form differs from target canvas: " + resource)
    for icon in targets["icons"]:
        file = within(ROOT / "GameResources", icon["resourcePath"] + ".png")
        if not file.exists():
            pending.append("Missing icon: " + icon["actorId"])
            continue
        entry = next((i for i in manifest.get("icons", []) if i["actorId"] == icon["actorId"]), None)
        try:
            info = image_info(file)
            expected_canvas = entry.get("canvas") if entry and entry.get("nativeResolution") else [128, 128]
            if info["canvas"] != expected_canvas or not info["bodyBounds"]:
                raise ValueError("Invalid icon PNG: " + icon["actorId"])
            if not entry or entry.get("sha256") != info["sha256"] or not entry.get("approved"):
                pending.append("Icon is unapproved or has an invalid hash: " + icon["actorId"])
            if not entry or not entry.get("verifiedInGame"):
                pending.append("Icon has no real-game size calibration: " + icon["actorId"])
            if entry and entry.get("provenance"):
                record = load_json(within(ROOT, entry["provenance"]))
                if record.get("outputSha256") != info["sha256"] or record.get("method") != "built-in-imagegen-independent-frame":
                    raise ValueError("Invalid independent icon provenance")
                original = within(ROOT, record["sourceFile"])
                if not visual_waived:
                    check_source_edges(original)
                if digest(original) != record.get("sourceSha256") or record.get("actorId") != icon["actorId"] or record.get("action") != "icon":
                    raise ValueError("Invalid independent icon source")
                if record["sourceSha256"] in source_owners:
                    raise ValueError("World animation source reused for placement icon")
                source_owners[record["sourceSha256"]] = icon["actorId"] + "/icon"
            else:
                pending.append("Missing independent icon provenance: " + icon["actorId"])
        except (ValueError, KeyError, OSError) as ex:
            errors.append(str(ex))
    final_review = WORK / "final-visual-review.json"
    if visual_waived:
        pass  # Explicit user policy waives visual review only; runtime checks below remain mandatory.
    elif not final_review.exists():
        pending.append("Missing full 0.6.0 animation and icon review evidence")
    else:
        try:
            review = load_json(final_review)
            reviewed_forms = review.get("reviewedForms", [])
            reviewed_icons = review.get("reviewedIcons", [])
            if (review.get("passed") is not True or review.get("scope") != "0.6.0-only"
                or review.get("manifestSha256") != digest(RESOURCES / "actor-frames.manifest.json")
                or not isinstance(reviewed_forms, list) or not isinstance(reviewed_icons, list)
                or len(reviewed_forms) != len(targets["forms"])
                or len(reviewed_icons) != len(targets["icons"])
                or {f["actorId"] for f in reviewed_forms} != {f["id"] for f in targets["forms"]}
                or {i["actorId"] for i in reviewed_icons} != {i["actorId"] for i in targets["icons"]}
                or review.get("unresolvedDefects") != []):
                pending.append("Incomplete or stale full visual review evidence")
        except (ValueError, KeyError, OSError, TypeError):
            pending.append("Invalid full visual review evidence")
    acceptance_spec = importlib.util.spec_from_file_location('runtime_acceptance', ROOT / 'tools/accept-existing-runtime-evidence.py')
    acceptance = importlib.util.module_from_spec(acceptance_spec)
    acceptance_spec.loader.exec_module(acceptance)
    user_runtime_accepted = acceptance.accepted_existing_evidence()
    evidence = WORK / "in-game-validation.json"
    if not evidence.exists() and not user_runtime_accepted:
        pending.append("Missing real-game size, compatibility and performance evidence")
    elif not user_runtime_accepted:
        report = load_json(evidence)
        if (not report.get("passed") or set(report.get("forms", [])) != {f["id"] for f in targets["forms"]}
            or set(report.get("icons", [])) != {i["actorId"] for i in targets["icons"]}
            or set(report.get("testedFps", [])) != {30, 60, 144}
            or set(report.get("visiblePopulationScenarios", [])) != {100, 500, 1000}
            or report.get("saveTestScope") != "new-save-only"
            or not report.get("newSaveCompatibilityPassed")):
            pending.append("Incomplete real-game validation evidence")
    if duplicates and not visual_waived:
        pending.append(f"{len(duplicates)} duplicate RGBA frames require explicit review")
    record = {"checkedFrames": frames_checked, "forms": len(groups), "errors": errors, "pending": pending,
              "duplicateFrames": duplicates, "releaseReady": not errors and not pending,
              "userAcceptedRuntimeScope": user_runtime_accepted,
              "generatedFrames": len(list((WORK / "frames").glob("*/main/*.png"))),
              "generatedIcons": len(list((WORK / "icons").glob("*.png"))),
              "targetFrames": targets["totalFrames"], "targetIcons": len(targets["icons"]),
              "offlineInstalledForms": sum(1 for g in groups if g.get("approved") and
                                            all(f.get("origin") == "independently-redrawn" for c in g["actions"].values() for f in c["frames"]))}
    save_json(WORK / "validation.json", record)
    save_json(WORK / "progress.json", {k: record[k] for k in ("generatedFrames", "generatedIcons", "releaseReady",
                                                             "targetFrames", "targetIcons", "offlineInstalledForms")})
    print(json.dumps({"checkedFrames": frames_checked, "errors": len(errors), "pending": len(pending),
                      "releaseReady": record["releaseReady"], "report": str(WORK / "validation.json")}))
    if errors or args.release and pending:
        raise SystemExit(1)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    init = commands.add_parser("prepare")
    init.add_argument("--zip", required=True)
    norm = commands.add_parser("normalize")
    norm.add_argument("--actor", required=True)
    norm.add_argument("--action", choices=(*ACTIONS, "icon"), required=True)
    norm.add_argument("--index", type=int, required=True)
    norm.add_argument("--source", required=True)
    norm.add_argument("--prompt-file", required=True)
    norm.add_argument("--reference", action="append", default=[])
    norm.add_argument("--realign", action="store_true", help="Reprocess an existing generated frame from its preserved original")
    check = commands.add_parser("validate")
    check.add_argument("--release", action="store_true")
    queue = commands.add_parser("jobs")
    queue.add_argument("--actor", required=True)
    animation = commands.add_parser("preview")
    animation.add_argument("--actor", required=True)
    animation.add_argument("--action", choices=ACTIONS, required=True)
    animation.add_argument("--partial", action="store_true")
    installer = commands.add_parser("install")
    installer.add_argument("--actor", required=True)
    installer.add_argument("--review-file", required=True)
    args = parser.parse_args()
    {"prepare": prepare, "normalize": normalize, "validate": validate, "jobs": jobs, "preview": preview,
     "install": install}[args.command](args)


if __name__ == "__main__":
    main()
