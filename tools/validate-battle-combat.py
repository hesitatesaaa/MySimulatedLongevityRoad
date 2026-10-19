"""Validate four-profession resources/wiring; explicitly not Unity acceptance."""
import argparse
import hashlib
import json
import re
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument("--record",action="store_true")
args=parser.parse_args()

def read(path):
    text=(ROOT/path).read_text(encoding="utf-8-sig")
    assert "\ufffd" not in text and "\x00" not in text,path
    return text

spell_path="code/MySimulatedLongevityRoad/Systems/Cultivation/MclslSpellSystem.cs"
spell_source=read(spell_path)
definitions=re.findall(r'Spell\("([SC]\d{3})",\s*"([^"]+)"',spell_source)
assert len(definitions)==42 and len(dict(definitions))==42
assert {identifier for identifier,_ in definitions}=={
    *(f"S{i:03}" for i in range(1,27)),*(f"C{i:03}" for i in range(1,17))}
items=read("code/MySimulatedLongevityRoad/Data/MclslItemCatalog.cs")
scrolls=items.split("private static IEnumerable<MclslItemDefinition> BattleScrolls()",1)[1]
scroll_names=re.findall(r'"([^"]+)"',scrolls.split("string[] names=",1)[1].split(";",1)[0])
assert scroll_names==[dict(definitions)[f"C{i:03}"] for i in range(1,17)]
manifest=json.loads(read("resources/battle-icons.manifest.json"))
assert len(manifest["icons"])==4
for icon in manifest["icons"]:
    target=ROOT/icon["path"]
    assert hashlib.sha256(target.read_bytes()).hexdigest()==icon["sha256"]
    with Image.open(target) as image:
        assert image.size==(64,64) and image.mode=="RGBA" and image.getbbox()
        assert image.getchannel("A").getextrema()[0]==0

base="code/MySimulatedLongevityRoad/"
wiring={
    "Systems/Cultivation/MclslSpellProgression.cs":["CanStudy(actor,spellId)","CanStudy(actor,id)"],
    "Systems/Adventure/MclslAdventureSystem.cs":["TryTransmission(actor,year,\"ruin|"],
    "Systems/Adventure/MclslFactionMissionSystem.cs":["TryTransmission(actor,year,\"faction|"],
    "Systems/MclslTraitGrantRouter.cs":["SetMain(actor", "ClearMain(actor)"],
    "Modules/MclslBeastModule.cs":["RegisterStatuses()","MclslBattleCombat.Load()","MclslSpiritPact.Load()",
        "MclslSpellTerrain.Load()","MclslBattleCombat.Tick()","MclslSpiritPact.Save()","MclslBattleVisuals.Clear()"],
    "UI/MclslSpellCodexWindow.cs":["SetMain(selected,profession)","AcquisitionDescription(spell.Id)"],
}
for path,needles in wiring.items():
    source=read(base+path)
    for needle in needles:
        assert needle in source,(path,needle)
combat_files=sorted((ROOT/base/"Systems/Combat").glob("MclslBattle*.cs"))+[ROOT/base/"Systems/Combat/MclslSpiritPact.cs"]
for path in combat_files:
    source=path.read_text(encoding="utf-8")
    assert "Time.timeScale" not in source and "Process.Start" not in source
assert "_externalCount" in spell_source and "MaxVisibleEffects = 96" in spell_source
assert "MCLSL_config_battle_terrain" in read(base+"Systems/Beasts/MclslBeastConfig.cs")

test_log=ROOT/"build/battle-tests.log"
tests=json.loads(test_log.read_text(encoding="utf-8-sig").strip().splitlines()[-1])
assert tests["passed"] and not tests["actualUnityStartupTested"] and not tests["actualUnitStressTested"]
beast_log=(ROOT/"build/battle-beast-tests.log").read_text(encoding="utf-8-sig")
assert "PASS: 692 assertions" in beast_log
assembly=ROOT/"build/Battle-Compiler/MySimulatedLongevityRoad.dll"
assert assembly.is_file()
record={"schemaVersion":1,"status":"local-development-awaiting-Unity","clientDate":"2026-10-19",
        "existingSpells":26,"professionSkills":16,"professionScrolls":16,"newTraitIcons":4,
        "offlineBattleChecks":tests["checks"],"existingCatalogAssertions":692,
        "compiledAssemblySha256":hashlib.sha256(assembly.read_bytes()).hexdigest(),
        "sources":{str(p.relative_to(ROOT)).replace("\\","/"):hashlib.sha256(p.read_bytes()).hexdigest()
                   for p in [*combat_files,ROOT/spell_path,ROOT/base/"Systems/Cultivation/MclslSpellCasting.cs"]},
        "actualUnityStartupTested":False,"actualInGameSizeTested":False,
        "actualCpuMemoryMeasured":False,"actualGpuMemoryMeasured":False,
        "stressTests":[{"units":n,"status":"not-run","placedUnitsEvidence":None} for n in (100,500,1000)],
        "releaseReady":False}
if args.record:
    (ROOT/"resources/battle-combat-validation.json").write_text(json.dumps(record,ensure_ascii=True,indent=2)+"\n",encoding="utf-8")
print(json.dumps({key:record[key] for key in ("status","existingSpells","professionSkills","professionScrolls","newTraitIcons","offlineBattleChecks","existingCatalogAssertions","releaseReady")},ensure_ascii=True))
