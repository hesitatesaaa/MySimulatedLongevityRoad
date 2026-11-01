#!/usr/bin/env python3
"""Minimal icon manifest sync / validator for MySimulatedLongevityRoad.

Covers exactly the 153 static pixel icons:
  GameResources/trait                    76
  GameResources/ui/CareerSkills          30
  GameResources/ui/Spells                26
  GameResources/ui/BeastTalents          21

Usage:
  python tools/mclsl_icon_manifest.py            # sync living-resources.json Outputs + FILE-MANIFEST.json hashes
  python tools/mclsl_icon_manifest.py --verify   # check coverage + hashes only (no writes)
  python tools/mclsl_icon_manifest.py --verify --style   # also lint 64x64 / <=24 colours / alpha in {0,255}
"""
import hashlib
import json
import os
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
RES = os.path.join(ROOT, "resources")
LIVING = os.path.join(RES, "living-resources.json")
FILE_MANIFEST = os.path.join(ROOT, "FILE-MANIFEST.json")
ROOTS = [
    ("GameResources/trait", "trait"),
    ("GameResources/ui/CareerSkills", "ui/CareerSkills"),
    ("GameResources/ui/Spells", "ui/Spells"),
    ("GameResources/ui/BeastTalents", "ui/BeastTalents"),
]


def sha256(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def png_size(path):
    with open(path, "rb") as f:
        head = f.read(24)
    return [int.from_bytes(head[16:20], "big"), int.from_bytes(head[20:24], "big")]


def icons():
    out = []
    for rel_dir, key_prefix in ROOTS:
        directory = os.path.join(ROOT, rel_dir)
        for name in sorted(os.listdir(directory)):
            if not name.lower().endswith(".png"):
                continue
            rel = rel_dir + "/" + name
            key = key_prefix + "/" + os.path.splitext(name)[0]
            out.append((key, rel, os.path.join(directory, name)))
    return out


def load(path):
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def save(path, data):
    with open(path, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=1)
        f.write("\n")


def sync():
    items = icons()
    living = load(LIVING)
    outputs = living.setdefault("Outputs", [])
    by_res = {e.get("Resource"): e for e in outputs}
    manifest = load(FILE_MANIFEST)
    files = manifest.setdefault("files", {})
    added = 0
    for key, rel, abs_path in items:
        digest = sha256(abs_path)
        size = png_size(abs_path)
        entry = by_res.get(key)
        if entry is None:
            entry = {"Resource": key, "SourceSHA256": "", "SHA256": digest,
                     "Size": size, "Frames": "0", "File": rel}
            outputs.append(entry)
            by_res[key] = entry
            added += 1
        else:
            entry["SHA256"] = digest
            entry["Size"] = size
            entry["File"] = rel
        files[rel] = {"bytes": os.path.getsize(abs_path), "sha256": digest}
    save(LIVING, living)
    save(FILE_MANIFEST, manifest)
    print("icons=%d added_outputs=%d total_outputs=%d" % (len(items), added, len(outputs)))


def verify(style=False):
    items = icons()
    living = load(LIVING)
    manifest = load(FILE_MANIFEST)
    by_res = {e.get("Resource"): e for e in living.get("Outputs", [])}
    files = manifest.get("files", {})
    missing = [key for key, _, _ in items if key not in by_res]
    hash_bad = []
    for key, rel, abs_path in items:
        digest = sha256(abs_path)
        entry = by_res.get(key)
        if entry is None or entry.get("SHA256") != digest:
            hash_bad.append(key)
        meta = files.get(rel)
        if meta is None or meta.get("sha256") != digest:
            hash_bad.append(rel)
    print("icons=%d covered=%d missing=%d hash_mismatch=%d" %
          (len(items), len(items) - len(missing), len(missing), len(hash_bad)))
    if missing:
        print("MISSING:", missing[:10])
    ok = not missing and not hash_bad
    if style:
        from PIL import Image
        bad = []
        for key, _, abs_path in items:
            im = Image.open(abs_path).convert("RGBA")
            if im.size != (64, 64):
                bad.append((key, "size", im.size))
                continue
            pixels = list(im.getdata())
            if any(a not in (0, 255) for _, _, _, a in pixels):
                bad.append((key, "alpha", ""))
                continue
            colors = {p[:3] for p in pixels if p[3] > 0}
            if len(colors) > 24:
                bad.append((key, "colors", len(colors)))
        print("style_fail=%d" % len(bad))
        for row in bad[:10]:
            print("  ", row)
        ok = ok and not bad
    print("RESULT:", "PASS" if ok else "FAIL")
    return 0 if ok else 1


if __name__ == "__main__":
    argv = sys.argv[1:]
    if "--verify" in argv:
        sys.exit(verify(style="--style" in argv))
    sync()
    sys.exit(0)
