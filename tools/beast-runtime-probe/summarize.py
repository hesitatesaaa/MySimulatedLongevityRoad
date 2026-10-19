"""Summarize scoped real-engine observations; never certify a release."""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
RUN = ROOT / "output/beast-runtime-20261014-a"
sources = {}

def read(name):
    path = RUN / name
    raw = path.read_bytes()
    sources[name] = {"path": str(path), "sha256": hashlib.sha256(raw).hexdigest()}
    data = json.loads(raw.decode("utf-8-sig"))
    assert data["source"] == "unity-runtime"
    return data

c = read("controlled-unity-integration-c.json")
g = read("controlled-unity-integration-g.json")
k = read("controlled-spells-k.json")
n = read("controlled-recipes-n.json")
o = read("controlled-spells-o.json")
s = read("controlled-deaths-s.json")
assert len(c["morphs"]) == 21
assert all(all(x[key] for key in ("animal", "human", "identity", "reverted", "healthRatioPreserved")) for x in c["morphs"])
assert len(c["atlases"]) == 48 and all(x["valid"] for x in c["atlases"])
assert [x["spawned"] for x in c["waves"]] == [24, 36, 48, 64, 80, 96, 112]
assert all(x["valid"] for x in c["waves"])
assert g["status"] == "controlled-integration-completed" and not g["errors"]
assert len(g["crafts"]) == 3 and all(x["crafted"] for x in g["crafts"])
assert all(x["used"] and x["after"] == x["before"] - 1 for x in g["uses"][:2])
assert g["uses"][2]["equipped"]
assert len(g["trades"]) == 4 and all(x["listed"] and x["bought"] and x["paid"] - x["expectedFee"] == x["received"] for x in g["trades"])
assert g["deaths"] and all(not x["aliveAfter"] and x["deathRouteCommitted"] for x in g["deaths"])
assert all(g["diskSave"]["saved"][key] for key in ("mapOnDisk", "actorRestored", "bagEqual", "recipesEqual", "pendingWaveEqual"))
assert g["diskSave"]["pendingWaveResumed"] and len(set(g["diskSave"]["finalActorIds"])) == 24
assert k["status"] == "spell-probe-completed" and not k["errors"]
spells = [x for x in k["cases"] if "id" in x]
assert len(spells) == 16 and all(all(x[key] for key in ("manaCorrect", "mainFirst", "friendExcluded", "singleHit", "mainHit")) for x in spells)
assert all(len(x["lockedTargets"]) <= x["targetLimit"] for x in spells)
assert all(len(x["lockedTargets"]) == 8 for x in spells if x["id"] in ("S010", "S012"))
assert spells[0]["teleported"]
specials = {x["scenario"]: x for x in k["cases"] if "scenario" in x}
assert specials["pause"]["stable"]
assert specials["pool-cap-precondition"]["filled"] and specials["pool-cap-precondition"]["active"] == 96
assert all(specials[key]["passed"] for key in ("offscreen", "pool-cap", "caster-death"))
assert specials["pending-disk-save-restored"]["equal"] and specials["pending-disk-save-restored"]["manaUnchanged"]
assert specials["pending-disk-save-resumed"]["once"]
phases = {(x["id"], x["phase"]) for x in k["visualSamples"] if x["scenario"] == x["id"]}
assert len(phases) == 48
assert n["status"] == "recipe-probe-completed" and not n["errors"]
assert len(n["icons"]) == 111 and all(x["valid"] for x in n["icons"])
assert len(n["recipes"]) == 75 and all(all(x[key] for key in ("insufficientAtomic", "crafted", "exactConsumed", "equipped")) for x in n["recipes"])
consumables = [x for x in n["uses"] if "Id" in x]
assert len(consumables) == 50 and all(x["used"] and x["consumed"] for x in consumables)
inventory = next(x for x in n["uses"] if x.get("scenario") == "automatic-inventory-policy")
assert inventory["worldYear"] >= 1 and all(inventory[key] for key in ("freshBlocked", "gradeBlocked", "matureUsed"))
assert next(x for x in n["uses"] if x.get("scenario") == "artifact-native-stat-merge")["passed"]
assert o["status"] == "spell-probe-completed" and not o["errors"]
edge_cases = {x["scenario"]: x for x in o["cases"]}
assert all(edge_cases[key]["passed"] for key in ("effects-disabled", "became-friendly", "accelerated"))
assert not edge_cases["effects-disabled"]["visualsEnabled"] and edge_cases["accelerated"]["speedMultiplier"] == 2
assert s["status"] == "death-probe-completed" and not s["errors"]
assert len(s["cases"]) == 9 and all(x["passed"] for x in s["cases"])

summary = {
    "source": "scoped-summary-of-unity-runtime-reports",
    "unityVersion": g["unityVersion"], "graphicsDevice": g["graphicsDevice"],
    "ordinaryMorphChecks": 21, "loadedSpellAtlases": 48,
    "sevenWaveSpawnCounts": [24, 36, 48, 64, 80, 96, 112],
    "realLootCommittedDeaths": len(g["deaths"]),
    "flyballCraftUseEquipment": "passed-controlled-runtime",
    "fourCategoryMarketplaceTrade": "passed-controlled-runtime-with-seeded-market-fixture-stock",
    "inventoryRecipeAndPendingWaveDiskReload": "passed-controlled-runtime",
    "spellBehavior": "passed-selected-controlled-runtime-cases",
    "spellStageObjectSamples": 48, "spellQueuedDiskReload": "passed-controlled-runtime",
    "loadedRegisteredItemIcons": 111, "allRecipeCraftAndAtomicConsumptionChecks": 75,
    "allArtifactEquipmentChecks": 25, "manualConsumableUseChecks": 50,
    "automaticInventoryAgeAndGradePolicy": "passed-controlled-runtime",
    "effectsDisabledFriendChangeAndTwoTimesSpeed": "passed-controlled-runtime",
    "nativeDeathAttributionAndImmediateSpellImpactChecks": 9,
    "normalBattleBenchmark": "pending",
    "releaseReady": False, "gameplayCasesPassed": False,
    "sourceReports": sources,
    "limitations": [
        "Report C failed later because its combat fixture caster died; only completed morph, atlas and wave observations are selected.",
        "Profession switching and auxiliary items are configured by the diagnostic; beast materials in the crafting loop came from real committed deaths.",
        "Marketplace stock is deliberately seeded independently; this is a real transaction test, not evidence of naturally produced supply.",
        "Spell actors' native attacks are suppressed in the fixture; production TryCastCombat, native getHit and world simulation time are exercised.",
        "The exhaustive recipe/consumable sweep uses seeded inventory and known recipes, correct native age/realm inputs and explicit annual arguments. It verifies consumption/use/equipment, not natural acquisition.",
        "No full visual review, all-special-case completion, population benchmark or twenty-minute performance certification."
    ]
}
out = ROOT / "output/beast-overhaul/controlled-unity-summary.json"
out.write_text(json.dumps(summary, ensure_ascii=True, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"scopedChecks": "passed", "releaseReady": False, "report": str(out)}, ensure_ascii=True))
