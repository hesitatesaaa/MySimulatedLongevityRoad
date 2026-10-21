"""Check the committed catalog and exercise sparse/missing legacy sequences."""
import json
import tempfile
from pathlib import Path
import importlib.util

spec=importlib.util.spec_from_file_location('catalog',Path(__file__).with_name('build-soul-frame-catalog.py'))
catalog=importlib.util.module_from_spec(spec); spec.loader.exec_module(catalog)
expected,stats=catalog.build()
assert expected==json.loads((catalog.ROOT/'resources/world-soul-frames.manifest.json').read_text())
assert stats['catalogLookups']==904 and stats['previousCandidateLookups']>stats['catalogLookups']
with tempfile.TemporaryDirectory() as temporary:
    root=Path(temporary)
    base=root/'GameResources/actors/Souls/Test'
    base.mkdir(parents=True)
    for name in ('Test_idle_000.png','Test_idle_001.png','Test_idle_003.png','Hit/Hit_01.png','Hit/Hit_03.png'):
        p=base/name;p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(b'fixture')
    groups=catalog.build(root)[0]['groups']['Test']
    assert groups['idle']==['actors/Souls/Test/Test_idle_000','actors/Souls/Test/Test_idle_001']
    assert groups['hit']==['actors/Souls/Test/Hit/Hit_01','actors/Souls/Test/Hit/Hit_03']
    assert groups['attack']==[] and groups['run']==[]
print(json.dumps(dict(actualCatalogFrames=904, sparseAndMissingSequences=True, **stats)))
