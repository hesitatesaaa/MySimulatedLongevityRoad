"""Remove superseded 64px action folders after the new main atlas passes audit."""
import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ACTORS = (ROOT/"GameResources"/"actors").resolve()
manifest = json.loads((ROOT/"scripts"/"beast_sprite_sources"/"manifest.json").read_text(encoding="utf8"))
actions = ("Idle","Attack","Walk","Hit","Death")
targets = []
for name in manifest["beasts"]:
    for form in ("Beast","Human"):
        targets.extend(ACTORS/"Beasts"/name/form/action for action in actions)
for name in manifest["named"]:
    if name == "BaiXianSheng":
        targets.append(ACTORS/"Named"/name)
    elif name in ("DiYi","DiSanMo","QingShenLong"):
        targets.extend(ACTORS/"Named"/name/action for action in actions)
    else:
        targets.extend(ACTORS/"Named"/name/action for action in actions)

# Resolve every recursive-delete target first, then delete. Immortals/Bai and
# Immortals/Chuanfa retain their original source frames for reproducibility.
resolved = [target.resolve() for target in targets]
for target in resolved:
    if target == ACTORS or not target.is_relative_to(ACTORS):
        raise RuntimeError(f"unsafe deletion target: {target}")
for target in resolved:
    if target.is_dir():
        shutil.rmtree(target)
print(f"Pruned {len([p for p in resolved if not p.exists()])} legacy actor folders.")
