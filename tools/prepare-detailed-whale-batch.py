"""Write independent detailed pose prompts; does not draw or transform any image."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / "output/actor-redraw"
ACTOR = "mclsl_beast_liejiejing"
POSES = {
    "walk": [
        "Swimming cycle neutral crossing: tail stock nearly level, flukes flat, near fin swept gently backward and far fin forward; head level.",
        "Swimming rising stroke: tail stock arches upward, both split flukes tilt upward, near pectoral fin extends diagonally forward; head level.",
        "Swimming upper stroke apex: tail stock lifted and curved, split flukes raised at upper arc, near fin lifted outward slightly, far fin angled back; head level.",
        "Swimming downward power stroke begins: tail stock straightening downward, flukes cupped downward, near fin flattening rearward; head level.",
        "Swimming downward power stroke: tail stock bends downward distinctly, split flukes push down, near fin swept back close to belly; head level.",
        "Swimming lower stroke apex: tail stock lowest and smoothly bent, flukes spread downward but stay inside canvas, near fin relaxed below belly; head level.",
        "Swimming recovery stroke begins: tail stock rising from low bend, flukes turning flat, near fin reaching forward, far fin trailing; head level.",
        "Swimming recovery to neutral: tail stock nearly level with slight upward curve, flukes mildly cupped up, near fin halfway back to starting position; head level.",
    ],
    "attack": [
        "Attack anticipation begins: closed jaw and chin pulled back slightly, near fin held forward as a brace, tail flukes slightly raised.",
        "Attack anticipation coiling: chest and neck compress subtly, tail stock curves upward, near fin curled against belly, mouth still closed.",
        "Attack maximum preparation: head tilts upward modestly and jaw starts opening, near fin tucked tight, tail stock bent upward ready to push.",
        "Attack release begins: head thrusts forward, jaw opens halfway, near fin extends diagonally forward, tail stock starts strong downward push.",
        "Attack forward strike: jaw open wide showing one readable oral cavity and small teeth, near fin thrust forward, tail flukes push down, powerful solid whale silhouette.",
        "Attack peak strike: head pushed forward and jaw widest, neck stretched slightly, near fin extended down-forward, tail flukes at lowest stroke. No projectiles or effects.",
        "Attack recovery begins: head retracts and jaw half closes, near fin bends back toward belly, tail begins rising.",
        "Attack recovery middle: mouth mostly closed, head returning level, near fin folds rearward, tail rises to modest upper bend.",
        "Attack recovery late: jaw closed and neck relaxed, near fin hanging naturally, tail returning close to neutral.",
        "Attack final settle: closed jaw, head level, fin slightly forward and split tail flukes flattened; shoulders relax toward idle.",
    ],
    "hit": [
        "Impact reaction: eye narrowed, head recoils slightly backward and lower, near fin curls protectively toward belly, tail flukes tense upward. No damage particles.",
        "Recover from impact: head straightens toward neutral but remains slightly low, eye reopened halfway, near fin unfolds partway, tail flukes relax downward.",
    ],
    "death": [
        "Dying onset: eye shut with a single dark eyelid, head just beginning to droop, near fin loses tension and angles down, tail flukes still extended.",
        "Dying slump: head and chest sag visibly, neck curved down, near fin limp downward, dorsal wavefins softened, tail stock slightly raised by slumping body.",
        "Dying collapse continues: broad body bends gently downward at neck and belly, nose lower, both fins hang limp, flukes droop and curl inward.",
        "Collapse toward prone resting pose: lower flattened body silhouette, eye closed, jaw shut, belly compressed, near fin folding under chest and tail flukes curling inward.",
        "Prone corpse almost settled: body lies low and wide with softened flattened belly, near fin folded along underside, dorsal wavefins drooped, tail low with split flukes still distinct.",
        "Prone corpse settles: eye closed, body slightly more compressed, near fin tip folded flat, far fin resting under chin, tail flukes gently curled low.",
        "Final corpse settling: flattened body retains complete whale identity, near fin rests with tip very slightly outward, dorsal wavefins lie softer and tail tip rests lower.",
        "Final corpse resting pose: closed eye and jaw, broad low body, both fins folded naturally, tail flukes slack and dorsal fins collapsed. Tiny final fin-tip settling differs from prior pose; no standing or revival.",
    ],
    "icon": [
        "Dedicated placement-button portrait pose: full whale in a compact graceful curve, tail curling upward behind body to fit a square, near fin extended elegantly, head right-facing and horns clearly separated. This is a newly drawn icon, not a copy or crop of an animation frame.",
    ],
}


def main():
    targets = json.loads((ROOT / "resources/actor-redraw.targets.json").read_text(encoding="utf-8-sig"))
    form = next(f for f in targets["forms"] if f["id"] == ACTOR)
    jobs = []
    for action, poses in POSES.items():
        indices = [0] if action == "icon" else form["actions"][action].get("frameOrder", list(range(form["actions"][action]["count"])))
        if action != "icon" and (len(indices) != form["actions"][action]["count"] or any(i >= len(poses) for i in indices)):
            raise ValueError("Pose list does not cover actual manifest")
        for index in indices:
            pose = poses[index]
            output = WORK / (f"icons/{ACTOR}.png" if action == "icon" else f"frames/{ACTOR}/main/{action}_{index}.png")
            if output.exists():
                continue
            prompt = (
                f"Use case: stylized-concept. Asset: exactly ONE independently redrawn transparent pixel-art game sprite, Liejiejing {action}_{index}. "
                "Input reference: identity, proportions, palette and detailed pixel rendering standard ONLY, never an old bitmap to transform. "
                f"New pose: {pose} "
                "Preserve broad Chinese fantasy deep-blue whale, layered blue and cyan stepped highlight clusters, intricate cyan fissure markings, cream-gray grooved belly, "
                "ornate cream horns and gold facial ridge, curling dorsal wavefins, exactly ONE pair of pectoral fins, split tail flukes, no legs or extra limbs. "
                "Match the reference's fine contour work, fin edging, horn shading, belly grooves and rich readable pixel clusters. No geometric simplification, no plain flat silhouette. "
                "Right-facing side view and same anatomical scale on square 1254x1254 source canvas. Hold main torso and head position stable except the described pose changes. "
                "Keep entire silhouette with generous transparent margin; no part touches any canvas edge. If tail bends widely, curl its tips inward rather than enlarge the character or cut off fins. "
                "Output one newly drawn character and one pose in one transparent RGBA PNG only. No water, background, light halo, particles, shadow, floor, text, panels, sprite sheet or multiple poses. "
                "Never translate, rotate, scale, recolor or paste an existing sprite as this frame."
            )
            path = WORK / f"prompts/{ACTOR}/{action}_{index}.txt"
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(prompt + "\n", encoding="utf-8")
            jobs.append({"actor": ACTOR, "action": action, "index": index, "prompt": prompt,
                         "promptFile": str(path), "reference": str(WORK / f"originals/{ACTOR}/idle_0.png")})
    destination = WORK / "detailed-whale-batch.jobs.json"
    destination.write_text(json.dumps(jobs, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"jobs": len(jobs), "path": str(destination)}))


if __name__ == "__main__":
    main()
