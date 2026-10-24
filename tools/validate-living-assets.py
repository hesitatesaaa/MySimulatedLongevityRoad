"""Validate 0.7.0 resources and require current Unity evidence for formal release."""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
CAREERS = ("sword", "martial", "elemental", "sigil", "array", "spirit")


def fingerprint(root=ROOT):
    paths = [root / "InterestingTrait.cs", root / "mod.json", root / "default_config.json",
             root / "resources/living-resources.json"]
    paths += sorted((root / "code").rglob("*.cs"))
    paths += sorted((root / "GameResources/actors/Xuanhuang").glob("*.png"))
    paths += sorted((root / "GameResources/actors/Boats/Xuanhuang").glob("*.png"))
    paths += sorted((root / "GameResources/actors/CareerSpirits").glob("*.png"))
    paths += sorted((root / "GameResources/buildings/Xuanhuang").glob("*.png"))
    paths += sorted((root / "GameResources/effects/Careers").glob("*.png"))
    paths += sorted((root / "GameResources/ui/CareerSkills").glob("*.png"))
    paths += sorted((root / "GameResources/trait").glob("MclslCombat*.png"))
    digest = hashlib.sha256()
    for path in sorted(set(paths)):
        digest.update(path.relative_to(root).as_posix().encode("utf-8") + b"\0")
        digest.update(hashlib.sha256(path.read_bytes()).digest())
    return digest.hexdigest()


def validate(root=ROOT, release=False):
    manifest = json.loads((root / "resources/living-resources.json").read_text(encoding="utf-8-sig"))
    assert not manifest["Missing"], "Missing living artwork"
    groups = {g["Path"]: g for g in manifest["Groups"]}
    assert len(groups) == len(manifest["Groups"]), "Duplicate resource group"
    outputs = {o["Resource"]: o for o in manifest["Outputs"]}
    for resource, output in outputs.items():
        path = root / "GameResources" / (resource + ".png")
        assert hashlib.sha256(path.read_bytes()).hexdigest() == output["SHA256"], str(path)
        with Image.open(path) as image:
            assert list(image.size) == output["Size"] and image.convert("RGBA").getextrema()[3][0] == 0, resource
    for group in groups.values():
        with Image.open(root / "GameResources" / (group["Atlas"] + ".png")) as image:
            assert group["Atlas"] in outputs, group["Path"]
            names = set()
            for f in group["Frames"]:
                assert f["Name"] not in names, group["Path"]
                names.add(f["Name"])
                assert 0 <= f["X"] < f["X"]+f["Width"] <= image.width, group["Path"]
                assert 0 <= f["Y"] < f["Y"]+f["Height"] <= image.height, group["Path"]
                assert f["PixelsPerUnit"] > 0
    native = manifest["NativeArchitecture"]
    shared = {"bonfire", "statue", "well", "stockpile", "mine", "training_dummy"}
    buildings = [b for b in native["buildings"] if b["id"] not in shared]
    assert len(buildings) == 19
    for b in buildings:
        group = groups["buildings/civ_main/mclsl_xuanhuang/" + b["id"]]
        original = next(r for r in native["resources"] if r["path"] == b["path"])
        assert {f["Name"] for f in group["Frames"]} == {f["name"] for f in original["frames"]}, b["id"]
    for boat in ("boat_fishing", "boat_trading_human", "boat_transport_human"):
        group = groups["actors/boats/mclsl_xuanhuang_" + boat]
        original = next(r for r in native["resources"] if r["path"] == "actors/boats/"+boat)
        assert {f["Name"] for f in group["Frames"]} == {f["name"] for f in original["frames"]}
    for sex in ("male", "female"):
        for role in ("", "_child", "_warrior", "_king", "_leader", "_old", "_zombie"):
            for direction in ("", "_front", "_back"):
                group = groups[f"actors/species/civs/mclsl_xuanhuang{direction}/{sex}_1{role}"]
                assert {f["Name"] for f in group["Frames"]} == {f"{action}_{i}" for action in ("walk", "swim") for i in range(4)}
                mask = group["Atlas"] + "_robe_mask"
                assert mask in outputs
    for kind in range(4):
        assert len(groups[f"actors/CareerSpirits/{kind}/main"]["Frames"]) == 8
    for career in CAREERS:
        for icon in [f"trait/MclslCombatCareer_{career}"] + [f"trait/MclslCombatPassive_{career}_{i}" for i in range(1,6)] + [f"ui/CareerSkills/CP_{career}_{i}A" for i in range(1,6)]:
            with Image.open(root / "GameResources" / (icon+".png")) as image:
                assert image.size == (64,64), icon
        for i in range(1,6):
            with Image.open(root / f"GameResources/effects/Careers/CP_{career}_{i}A.png") as image:
                assert image.width*6 == image.height*4 and image.width%4 == 0
    result = dict(version="0.7.0", resourceValid=True, buildings=19, boats=3, characterSets=42,
                  spiritSets=4, careerIcons=66, activeEffects=30, inputSha256=fingerprint(root), unityRetested=False)
    if release:
        policy = json.loads((root / "resources/0.7.0-release-policy.json").read_text(encoding="utf-8"))
        evidence = root / policy["runtimeEvidence"]
        assert evidence.is_file(), "Current Unity acceptance report is required"
        runtime = json.loads(evidence.read_text(encoding="utf-8-sig"))
        assert runtime["passed"] and not runtime["errors"], "Unity acceptance failed"
        assert runtime["actualUnity"] and runtime["version"] == "0.7.0" and runtime["units"], "Actual Unity unit placement evidence is required"
        assert runtime["inputSha256"] == result["inputSha256"], "Unity evidence is stale"
        assert set(policy["requiredChecks"]).issubset(runtime["checks"]), "Unity checks incomplete"
        result["unityRetested"] = True
        result["interactiveVisualReview"] = runtime["interactiveVisualReview"]
        result["performanceTested"] = runtime["performanceTested"]
    return result


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=ROOT)
    parser.add_argument("--release", action="store_true")
    parser.add_argument("--fingerprint", action="store_true")
    args = parser.parse_args()
    print(fingerprint(args.root) if args.fingerprint else json.dumps(validate(args.root,args.release)))
