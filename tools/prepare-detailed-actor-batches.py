"""Plan detailed independent imagegen requests. Never draws or modifies image pixels."""
import argparse
import importlib.util
import json
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("authoring", ROOT / "tools/actor-redraw.py")
A = importlib.util.module_from_spec(spec)
spec.loader.exec_module(A)
WORK = A.WORK

BEAST_DESIGNS = {
    "kunpeng": "blue/cyan aquatic Kunpeng fish-bird hybrid: broad horizontal whale/fish torso, avian beaked head, paired large feathered pectoral swimming fins and a broad fish tail. Two small pelvic fins. NO walking legs, paws, lion body or griffin anatomy. Clean blue/cyan/white groups with restrained feather bands; no gold filigree. Preserve the original fish-bird identity and blue palette",
    "huanglong": "gold/amber Chinese SERPENTINE dragon: very long narrow snake-like S-coiled torso, neck and tapering tail; four SMALL clawed limbs attached to that long torso, antlers, TWO whiskers, cream belly and a restrained flowing back mane. Torso length is at least four times its thickness. Large sparsely indicated scale groups, no densely outlined individual scales. No wings; never a stout lion, qilin or tiger body",
    "zhuque": "vermilion sacred bird, red/orange layered feathers with gold edging, flame-shaped crest, hooked gold beak, two wings, two legs and sweeping red-gold tail plumage",
    "qilin": "jade-green qilin with gold antlers, dragon-like muzzle and mane, scaled deer-lion quadruped body, four sturdy legs and flowing tufted tail",
    "moqilin": "ink-purple qilin, gold horns, dark scaled lion-deer body, layered violet mane and cream claws, four legs and tufted tail",
    "xuanwu": "teal/dark green tortoise with ornate segmented heavy shell, four short weight-bearing legs, one tortoise head and a distinct coiled snake companion rising from its shell",
    "fenghuang": "orange/gold phoenix, elaborate fan crest, two feathered wings, two legs, long layered flame-like orange and golden tail with intricate edging",
    "dafeng": "large azure blue storm bird, gold crest and beak, white/cyan breast, broad powerful paired wings, two legs and blue trailing tail feathers",
    "manman": "paired red-brown and cream Manman birds joined in a recognizable paired silhouette, two distinct heads and balanced paired wings; retain the source's paired bird identity rather than make a single generic phoenix",
    "xuanniao": "black-violet sacred swallow-like bird, slender hooked beak, subtle purple feather highlights, pointed paired wings and forked sweeping tail, two legs",
    "changliao": "brown/gray shaggy long-tusked ferocious hound, broad canine muzzle, prominent ivory fangs, four jointed legs, coarse individual fur clusters and ragged tail",
    "mo": "purple mythical tapir-elephant beast, short trunk, ivory tusks, compact thick torso, four weight-bearing legs and layered violet hide ornament",
    "hongmao": "red-haired shaggy mythical quadruped with bulky muscular orange-red body, cream horns, layered bristling fur and flame-shaped mane, four legs and tufted tail",
    "feiqiu": "cream/pink round flying-ball beast, large purple edged ears, tiny horns, expressive pink muzzle, four small paws and slender curled tail; retain a richly shaded fleshy furry silhouette rather than a geometric circle",
    "yingying": "dark purple shadow beast in a crouched lupine silhouette, four jointed legs, purple-black layered fur and pale violet facial highlights, long sweeping tail; draw an opaque readable animal, no halo or particles",
    "goushou": "black/red dog-headed beast, sharp canine muzzle and ivory tusks, horn-like ears, dark red layered mane, powerful four-legged torso and long tail",
    "tianshou": "heavy ivory/cream horned beast, broad weight-bearing quadruped body, dark brown markings and crest, four thick legs, ornate horns, thick mane and short heavy tail",
    "diyi": "Diyi named red-orange beast, ornate gold-red bristling mane, wide powerful four-legged body and distinctive layered spiked crest; match the original named beast, not a generic dog",
    "disanmo": "Disanmo named purple tapir-elephant beast, ivory tusks and short trunk, ornate gold-violet hide pattern and thick mane, four sturdy legs; distinct from ordinary Mo",
    "qingshenlong": "named blue divine Chinese SERPENTINE dragon: very long slender sapphire/cyan S-coiled snake torso and tapering tail, cream belly, ivory/gold antlers, flowing white mane and TWO whiskers, exactly four SMALL clawed limbs attached to that long torso. Body length at least four times its thickness. Sparse broad scale groups, no wings and no stout lion/qilin/tiger anatomy",
}
PEOPLE = {
    "tianyi": "elder healer in ivory/olive layered Daoist robes with detailed embroidered trim, white hair bun and beard, narrow medicine staff, herb pouch",
    "moruibin": "scholarly cultivator in brown/taupe layered robes, dark scholar cap and long dark hair, elegant folding fan, composed narrow silhouette",
    "xuantianwang": "regal warrior in deep purple embroidered robes and articulated armor, dark hair and ornate crown, restrained gold details and a sword",
    "xuke": "amber/brown robed scholar, distinctive tall scholar hat, narrow staff, layered long sleeves with gold embroidery",
    "yefeipeng": "blue/white swordsman with a feather-edged flowing cloak, dark hair tied with a headband, white cuffs and a slender sword",
    "xiaoheng": "olive/cream traveling cultivator, dark hair bun, layered practical robe with sash and hanging gourd, modest rounded traveler hat silhouette",
    "xubai": "white/pale-blue elegant scholar, long white hair and restrained hair band, embroidered white robes with blue shadows and folding fan",
    "liping": "brown/bronze practical warrior, short layered robes and visible trousers, dark cap, boots and compact axe, sturdy broad shoulders",
    "shanfan": "gold/cream Daoist sage, dark long hair and crown, staff, long embroidered robe with wide draped sleeves",
    "zhangfan": "dark crimson/black armored cultivator, dark tied hair and headband, layered shoulder armor, trailing red sash and sword",
    "zhenshen": "gold/white celestial sage, long white hair and crown, ornate staff and richly embroidered layered robe with pale gold cloth shadows",
    "Bai": "Bai Xiansheng, serene white-robed immortal with long dark hair, restrained violet shadows, refined hair bun and elegant fan, separate dignified scholar silhouette",
    "Chuanfa": "Chuanfa Tianzun, black/gold immortal teacher with tall ceremonial Daoist hat, layered dark robes with fine gold edging, dark long hair and a scripture book",
}
STYLE_ACTORS = {"human": "mclsl_named_lifan", "bird": "mclsl_beast_baihu", "dragon": "mclsl_beast_baihu",
                "quadruped": "mclsl_beast_baihu", "heavy": "mclsl_beast_baihu", "aquatic": "mclsl_beast_baihu", "special": "mclsl_beast_baihu"}


def design(form):
    species = form["species"]
    if form.get("visualGender") == "female":
        return (form["humanAppearance"] + ". Preserve the original beast palette and identifying motifs, but draw a clearly female adult human. "
                "The old male-looking reference does not override this female design. Distinct feminine face, hair and outfit; not a recolored male sprite.")
    if form["family"] != "human":
        return BEAST_DESIGNS[species]
    if species in PEOPLE:
        return PEOPLE[species]
    beast = BEAST_DESIGNS.get(species, {
        "baihu": "white tiger with charcoal stripes, gold eyes and ivory claws",
        "qinglong": "azure/cyan Chinese dragon with gold antlers, mane and cream belly",
        "qingluan": "teal/cyan sacred bird with gold crest and layered white/teal plumage",
        "liejiejing": "blue whale with cream horns, gold ridge, cyan fissures and wave-shaped fins",
    }.get(species))
    return ("ONE humanoid Chinese cultivator in detailed layered robes, with exactly two arms and two legs and a fully human face. "
            f"This is the ascended human form of {beast}. Preserve its palette and subtle identity through crest-like hair ornaments, "
            "horn motifs where appropriate, embroidered scale/fur/feather patterns, and a recognizable tail or shoulder mantle where present in source. "
            "Keep the baseline human hairstyle, costume silhouette and visible weapon; do not draw the animal itself or copy Li Fan's robe colors.")


def pose(form, action, index, count):
    family = form["family"]
    t = index / max(1, count - 1)
    if action == "idle":
        if index == 0:
            return "Standard neutral idle pose, relaxed coherent anatomy, full clear silhouette, right-facing three-quarter side view and all identifying details visible."
        phase = index / count
        return (f"Idle breathing cycle phase {phase:.3f}: chest and shoulders gently {'rise' if phase < .5 else 'settle'}, "
                f"cloth, hair, mane, tail or feather tips lag at phase angle {round(phase*360)} degrees. "
                "Feet/body base remain fixed; changes are newly drawn joint/cloth poses, not whole-sprite movement.")
    if action == "walk":
        phase = index / count
        if family == "human":
            return (f"Walking cycle phase {phase:.3f} ({round(phase*360)} degrees): two-foot alternating gait, "
                    "visible hip, knee and ankle articulation, opposing arm swing, trailing cloth and hair; maintain body height and grounded foot line. "
                    f"Near leg is {'forward contact' if index==0 else 'passing and lifting' if phase<.25 else 'rear extension' if phase<.5 else 'recovering to front'}, far leg opposite. No extra hands or legs.")
        if family == "bird":
            wing = ["neutral wings spread halfway", "upstroke halfway raised", "upper wingbeat apex with wings raised",
                    "downstroke begins", "strong downstroke wings below torso", "lower wingbeat apex", "recovery wings lift", "neutral recovery wings spread"]
            return f"FLYING wingbeat cycle phase {phase:.3f}: {wing[min(7,int(phase*8))]}; articulate paired shoulder/elbow/wrist joints and layered feathers, tuck legs naturally, head stable, tail follows the beat."
        if family == "aquatic":
            stroke = ["neutral tail crossing", "rising tail stroke", "upper tail stroke apex", "downward power stroke begins",
                      "downward power stroke", "lower tail stroke apex", "tail rises in recovery", "returns toward neutral"]
            return f"SWIMMING cycle phase {phase:.3f}: {stroke[min(7,int(phase*8))]}, paired fins/feathered swimming wings oppose the tail rhythm, bend tail joints rather than move entire body, head level."
        if family == "dragon":
            return f"Dragon moving/hovering cycle phase {phase:.3f}: smooth traveling wave through neck, S-shaped torso and tail; exactly four limbs alternate naturally, whiskers and mane lag. Retain torso thickness and anatomical length."
        if family == "special" and form["species"] == "feiqiu":
            return f"Flying-ball moving cycle phase {phase:.3f}: body buoyantly flexes, ears and tail sway, four little paws alternate and curl; preserve round volume, not a translated duplicate."
        return (f"Four-legged gait phase {phase:.3f} ({round(phase*360)} degrees): near front and opposite rear legs advance, other diagonal pair follows; "
                "jointed paws/hooves make clear alternating contact, shoulders and hips transfer weight, tail follows. Heavy beasts carry weight; no extra limbs.")
    if action == "attack":
        stage = "preparation" if t < .25 else "release" if t < .48 else "peak strike" if t < .62 else "recovery" if t < .87 else "final settle"
        movement = {
            "human": "weapon/spell gesture with a firmly attached hand, proper two-arm anatomy, trailing robe folds, bent leading knee and rear bracing foot",
            "bird": "head/beak and talon strike with wings balancing, layered feathers articulate, exactly two wings and two legs",
            "dragon": "head/jaw lunge from coiled neck with four clawed limbs bracing, tail and mane follow; no elongated noodle forearms or fifth limb",
            "aquatic": "jaw/head thrust with paired fins bracing and tail powering the strike, mouth opens during release/peak and closes through recovery",
            "heavy": "heavy head/horn/body strike with weight-bearing limbs bracing and shell/mane responding",
            "quadruped": "pounce/bite/head strike with articulated forelegs and hindlegs, body weight shifts forward then returns",
            "special": "body/forepaw/head strike with clear preparation, extension and recovery, preserve recognizable volume and anatomy",
        }[family]
        if stage == "preparation":
            posture = ("Stay close to neutral body height. Draw the attacking hand/weapon BACK beside the waist/chest, elbows bent; no extended casting arm yet. Feet planted, knees slightly bent."
                       if family == "human" else "Keep head ABOVE the normal chest, limbs supporting the body. Gather weight over bent limbs; jaws closed and no forward lunge yet.")
        elif stage == "release":
            posture = ("Extend the attacking arm/weapon forward from the bent elbow, leading knee bends and rear foot braces; sleeve opens behind the forearm."
                       if family == "human" else "Begin the forward head/body thrust. Front limbs or wings extend partway, rear limbs/tail power the movement; head moves forward.")
        elif stage == "peak strike":
            posture = ("One attacking arm/weapon reaches its fullest forward extension, off hand stays close to torso; planted wide stance at the peak."
                       if family == "human" else "The head and front limbs/fins reach fullest extension in ONE peak strike, mouth opens if a biting species. Keep the same anatomical limb lengths.")
        elif stage == "recovery":
            posture = ("RETRACT the attacking hand/weapon back toward waist; elbow bends noticeably, leading knee straightens and stance narrows. Do not keep the arm fully extended."
                       if family == "human" else "RETRACT head and forelimbs toward the idle silhouette, close jaws, bring supporting limbs under the body; raise chest back toward normal height. No second lunge.")
        else:
            posture = ("FINISHED strike: stand upright at IDLE HEIGHT, both feet grounded, hands/weapon held relaxed near body. No extended attacking arm or bent attack stance."
                       if family == "human" else "FINISHED strike: return to neutral IDLE body height and silhouette, supporting limbs under torso, head raised, jaw closed. No crouch, lunge or extended striking limbs.")
        return f"Attack single-shot progress {t:.3f}, stage {stage}. Required pose: {posture} Species movement: {movement}. Follow ONLY this stage, not a generic peak attack pose. No detached projectiles, trails, particles or light effects."
    if action == "hit":
        return ("Impact recoil: head/shoulders bend back, face winces, limbs fold protectively, cloth/feathers/fins recoil, body remains recognizable. No damage effects."
                if index == 0 else "Recovery from impact: torso straightens partway, folded limbs unfold, face relaxes and appendages settle toward idle without a new attack.")
    if action == "death":
        stage = "onset" if t < .18 else "slump" if t < .42 else "collapse" if t < .68 else "prone settling" if t < .9 else "final resting corpse"
        posture = ("ONSET ONLY: body remains almost upright at normal idle height; slight forward head droop and small knee bend. Do not kneel or lie down yet."
                   if stage == "onset" else "The body begins to buckle; head below idle height, knees/limbs noticeably bend, chest lowers but remains above the fully collapsed pose."
                   if stage == "slump" else "Body falls onto folded limbs, torso angles downward, head drops close to the body; never lift the head from the previous stage."
                   if stage == "collapse" else "Rest FLAT and slack on the ground, head very low beside collapsed torso, limbs folded beneath or beside body. No upright kneeling torso; no support under shoulders.")
        return (f"Death single-shot progress {t:.3f}, stage {stage}. Required pose: {posture} "
                "Eye CLOSED as a dark eyelid, no bright pupil or iris. Body loses support and lowers progressively, limbs fold slack, head droops, cloth/fins/wings/mane sag. "
                "Early poses transition from idle, collapse bends the body anatomically, late poses remain low/prone and never stand, fly or swim again. "
                f"At this progress keep {'some weight on bent legs or drooping fin/wings' if t<.5 else 'all limbs or fins slack against a flattened resting body'}, "
                "retain full identity and all body parts. No gore, disappearing pixels, fragments or particles.")
    return ("Dedicated square placement-icon pose: newly draw an attractive compact readable character, full recognizable silhouette with limbs, weapons and head included; "
            "long creatures coil their body/tail and birds fold wings to fit. This is an independent portrait pose, not a copy or crop of a world frame.")


def prompt(form, action, index, count, standard=False):
    composition = ("Full character centered with comfortable transparent margins on all four sides; neutral body base/feet near source y1080, "
                   "silhouette within x100..1150,y100..1100; avoid a tiny character surrounded by empty padding. Reserve room for limb/weapon extensions."
                   if standard else "Keep the standard's exact anatomical pixel scale, source canvas, body anchor and right-facing view. "
                   "Entire pose must remain inside the image with transparent padding; curl long tails and bend limbs naturally to retain all parts.")
    roles = ("Image 1 is the old identity reference: preserve species, palette, hairstyle, costume and recognizable silhouette, but freshly redraw all pixels. "
             "Image 2 is balanced pixel rendering STYLE ONLY: match its clean clusters, contours and limited shading. Its anatomy, proportions, identity, costume and colors MUST NOT transfer to the new species or person."
             if standard else "Image 1 is this form's newly reviewed standard identity, detail, palette and anatomical scale. Freshly draw the specified new pose; never paste/transform the reference bitmap.")
    character = design(form).replace("ornate", "restrained").replace("intricate", "selective").replace("elaborate", "recognizable").replace("richly embroidered", "lightly trimmed")
    gender = (f"Visual gender: {form.get('visualGender', 'male')}, adult fully clothed human. " if form["family"] == "human" else "")
    return (f"Use case: stylized-concept. Asset: exactly ONE independent transparent pixel-art game frame for {form['name']} ({form['id']}), {action}_{index}. "
            f"{roles} Character: {character}. {gender}Pose: {pose(form, action,index,count)} "
            "Balanced pixel detail EXACTLY like the approved Li Fan or Baihu style reference: clear crisp outline, broad clean square pixel clusters, "
            "two or three readable shade levels per material, restrained trim and a few useful cloth/fur/feather/scale details. "
            "Avoid dense individual-scale microtexture, crowded gold filigree, excessive ornaments, tiny highlight speckles and painterly rendering. "
            "Retain proper anatomy and crafted silhouettes; never substitute primitive geometric shapes or a plain flat silhouette. "
            f"{composition} Square 1254x1254 source canvas, transparent RGBA. "
            "Exactly one character and one pose in one PNG; no multi-frame image, panels, grid, sprite sheet, background, ground shadow, text, watermark, glow or particles. "
            "Each frame is independently drawn, never achieved by translating, scaling, rotating or recoloring an old sprite. No clipping, malformed joints or duplicate limbs.")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("phase", choices=["standards", "animations"])
    parser.add_argument("--actor", action="append")
    parser.add_argument("--exclude", action="append", default=[], help="Leave active or separately revised forms untouched")
    args = parser.parse_args()
    targets = A.load_json(A.RESOURCES / "actor-redraw.targets.json")
    manifest = A.load_json(A.RESOURCES / "actor-frames.manifest.json")
    approved = {g["actorId"] for g in manifest["groups"] if g.get("approved")}
    icons = {i["actorId"] for i in targets["icons"]}
    jobs = []
    for form in targets["forms"]:
        actor = form["id"]
        if actor in approved or actor in args.exclude or args.actor and actor not in args.actor:
            continue
        card = A.load_json(ROOT / form["referenceCard"])
        if args.phase == "standards":
            if (WORK / f"frames/{actor}/main/idle_0.png").exists():
                continue
            requests = [("idle", 0, form["actions"]["idle"]["count"])]
            references = [str(ROOT / card["identityReference"]), str(WORK / f"originals/{STYLE_ACTORS[form['family']]}/idle_0.png")]
        else:
            review_path = A.RESOURCES / f"actor-standard-reviews/{actor}.json"
            if not review_path.exists():
                continue
            standard = WORK / f"frames/{actor}/main/idle_0.png"
            review = A.load_json(review_path)
            if review.get("artDirectionRevision") != targets.get("qualityPolicy", {}).get("artDirectionRevision", 0):
                continue
            if not review.get("offlinePassed") or review.get("sha256") != A.digest(standard):
                raise ValueError("Unreviewed or changed standard: " + actor)
            requests = [(action, i, clip["count"]) for action, clip in form["actions"].items() for i in A.frame_indices(clip)]
            if actor in icons:
                requests.append(("icon", 0, 1))
            references = [str(WORK / f"originals/{actor}/idle_0.png")]
        for action, index, count in requests:
            output = WORK / (f"icons/{actor}.png" if action == "icon" else f"frames/{actor}/main/{action}_{index}.png")
            if output.exists():
                continue
            text = prompt(form, action, index, count, args.phase == "standards")
            file = WORK / f"prompts/{actor}/{action}_{index}.txt"
            file.parent.mkdir(parents=True, exist_ok=True)
            file.write_text(text + "\n", encoding="utf-8")
            jobs.append({"actor": actor, "name": form["name"], "action": action, "index": index,
                         "artDirectionRevision": targets.get("qualityPolicy", {}).get("artDirectionRevision", 0),
                         "prompt": text, "promptFile": str(file), "references": references, "output": str(output)})
    destination = WORK / f"detailed-{args.phase}.jobs.json"
    A.save_json(destination, jobs)
    print(json.dumps({"phase": args.phase, "jobs": len(jobs), "path": str(destination)}, ensure_ascii=False))


if __name__ == "__main__":
    main()
