"""Record the user's balanced-detail style and mixed-gender human-form direction."""
import importlib.util
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("authoring", ROOT / "tools/actor-redraw.py")
A = importlib.util.module_from_spec(spec)
spec.loader.exec_module(A)
FEMALE_DESIGNS = {
    "kunpeng": "adult woman, dark side-braided hair, blue/white layered robe and feather-like fin mantle, slim gold hair pin, calm human face",
    "qinglong": "adult woman, dark half-up hair with jade horn-shaped pins, teal/cream robe, restrained scale motif on shoulder cape, regal human face",
    "huanglong": "adult woman, auburn braided hair and small gold crown, amber/cream robe, restrained dragon-tail sash, confident human face",
    "zhuque": "adult woman, red high ponytail and feather pin, vermilion/cream split robe and golden sash, sharp human face",
    "baihu": "adult woman warrior, white high ponytail, ivory/charcoal short layered robe over visible trousers, striped shoulder mantle, sword and strong human face",
    "qilin": "adult woman, dark looped braids with small antler ornament, jade/cream robe, simple fur-trimmed shoulder cape, warm human face",
    "fenghuang": "adult woman, copper-red loose high bun, orange/cream feather mantle and restrained gold sash, flowing robe with a few broad folds",
    "dafeng": "adult woman, navy braided hair, blue feather shoulder cape over white/blue practical robe and trousers, restrained gold hair band",
    "manman": "adult woman, two side braids, red/cream robe with paired-bird motifs, ONE human head and face, exactly two arms and two legs",
    "xuanniao": "adult woman, short dark bob hair and small violet feather pin, streamlined black/violet split robe, narrow swallow-tail mantle",
    "qingluan": "adult woman, dark crescent-shaped hair bun, turquoise/white robe, blue-green feather mantle and simple gold sash",
    "mo": "adult woman, violet swept-up curly hair, purple/cream robe, restrained ivory tusk-shaped clasps and staff, fully human face without trunk",
    "feiqiu": "adult woman, peach-pink short hair in two small buns, cream/pink layered dress and violet short mantle, round adult human face, subtle ear-shaped hair ornaments",
    "yingying": "adult woman, short dark hair partly under hood, midnight-purple split robe and practical trousers, subtle shadow-beast shoulder mantle, fully human face",
    "liejiejing": "adult woman, deep-blue side braid and silver wave comb, blue/cream-gray robe, fin-shaped shoulder mantle and simple wave cuffs, human face",
    "tianshou": "adult woman warrior, ivory hair in low braids, broad cream/brown layered robe and visible boots, simple heavy horn-shaped shoulder clasps",
    "qingshenlong": "adult woman, pale blue-white hair in high bun, ivory horn-shaped pins, sapphire/white robe and restrained scale shoulder cape",
    "disanmo": "adult woman, purple braided half-crown hairstyle, violet/cream robe, tusk-shaped ivory clasps and short staff, fully human face without trunk",
}

targets_path = A.RESOURCES / "actor-redraw.targets.json"
targets = A.load_json(targets_path)
female_ids = []
for form in targets["forms"]:
    if form["family"] != "human":
        continue
    female = form["id"].endswith("_yuan") and form["species"] in FEMALE_DESIGNS
    form["visualGender"] = "female" if female else "male"
    if female:
        form["humanAppearance"] = FEMALE_DESIGNS[form["species"]]
        female_ids.append(form["id"])
policy = {
    "revision": 2, "updatedAtUtc": datetime.now(timezone.utc).isoformat(),
    "standard": "balanced-pixel-detail-matching-lifan-and-baihu",
    "styleReferenceForms": ["mclsl_named_lifan", "mclsl_beast_baihu"],
    "femaleHumanForms": female_ids, "femaleCount": len(female_ids),
    "maleCount": sum(f["family"] == "human" and f["visualGender"] == "male" for f in targets["forms"]),
    "constraints": ["clean-large-pixel-clusters", "two-or-three-shade-levels-per-material",
                    "readable-silhouette", "restrained-trim", "no-dense-filigree-or-microtexture",
                    "no-geometric-placeholder-art", "independent-one-PNG-per-frame"],
    "saveAndGameplayGenderFieldsChanged": False,
}
if policy["femaleCount"] != 18 or policy["maleCount"] != 20:
    raise ValueError("Unexpected human-form gender coverage")
targets["qualityPolicy"] = {**targets.get("qualityPolicy", {}), "standard": policy["standard"],
                            "artDirectionRevision": 2, "referenceForms": policy["styleReferenceForms"],
                            "simplifiedProgramArtAccepted": False}
A.save_json(targets_path, targets)
A.save_json(A.RESOURCES / "actor-art-direction.json", policy)
A.save_json(A.WORK / "pipeline.control.json", {"artDirectionRevision": 2, "stopDispatch": False})
print("Recorded balanced pixel detail and 18 female / 20 male human forms; gameplay/save fields untouched.")
