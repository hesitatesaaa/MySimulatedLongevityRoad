"""Archive rejected program art without touching reviewed AI art or game resources."""
import importlib.util
import json
import shutil
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / "output/actor-redraw"


def main():
    timestamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%S%f")
    destination = WORK / "rejected/simplified-quality" / timestamp
    manifest = json.loads((ROOT / "resources/actor-frames.manifest.json").read_text(encoding="utf-8-sig"))
    approved = {g["actorId"] for g in manifest["groups"] if g.get("approved")}
    proofs = list((WORK / "frames").glob("*/main/*.provenance.json")) + list((WORK / "icons").glob("*.provenance.json"))
    selected, actors = set(), set()
    for proof in proofs:
        record = json.loads(proof.read_text(encoding="utf-8-sig"))
        if record.get("method") != "program-independent-pixel-draw":
            continue
        actor = record["actorId"]
        if actor in approved:
            raise ValueError("Refuse to retire reviewed installed art: " + actor)
        actors.add(actor)
        selected.add(proof)
        for key in ("sourceFile", "outputFile", "recipeFile"):
            path = (ROOT / record[key]).resolve()
            if not path.is_relative_to(WORK.resolve()):
                raise ValueError("Unexpected source outside authoring workspace")
            selected.add(path)
    for actor in actors:
        design = ROOT / "resources/actor-pixel-designs" / (actor + ".json")
        if design.exists():
            target = destination / "designs" / design.name
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.move(str(design), str(target))
        preview = WORK / "previews" / actor
        if preview.exists():
            selected.update(f for f in preview.rglob("*") if f.is_file())
    for path in sorted(selected):
        if not path.exists():
            raise ValueError("Missing rejected file: " + str(path))
        target = destination / path.resolve().relative_to(WORK.resolve())
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.move(str(path), str(target))
    targets_path = ROOT / "resources/actor-redraw.targets.json"
    targets = json.loads(targets_path.read_text(encoding="utf-8-sig"))
    targets["productionMode"] = "built-in-imagegen"
    targets["qualityPolicy"] = {
        "standard": "match-reviewed-lifan-qingluan-baihu-qinglong-detail",
        "simplifiedProgramArtAccepted": False,
        "reason": "User requires the earlier detailed pixel art; simplified samples rejected.",
        "updatedAtUtc": datetime.now(timezone.utc).isoformat(),
        "referenceForms": sorted(approved),
    }
    targets_path.write_text(json.dumps(targets, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    report = {"actors": sorted(actors), "archivedFiles": len(selected), "archive": str(destination),
              "installedArtTouched": False, "qualityPolicy": targets["qualityPolicy"]}
    (WORK / "simplified-art-retirement.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False))


if __name__ == "__main__":
    main()
