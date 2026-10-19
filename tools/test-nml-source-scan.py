"""Reproduce developer-stub leakage and verify NML exclusion, including build caches."""
import importlib.util
import tempfile
from pathlib import Path

spec = importlib.util.spec_from_file_location('nml_scan', Path(__file__).with_name('check-nml-sources.py'))
scan = importlib.util.module_from_spec(spec)
spec.loader.exec_module(scan)
with tempfile.TemporaryDirectory() as temp:
    root = Path(temp)
    for relative in ('InterestingTrait.cs', 'code/Actor.cs', 'tools/tests/Stubs.cs'):
        path = root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text('// fixture', encoding='utf-8')
    rejected = False
    try:
        scan.validate(root)
    except ValueError:
        rejected = True
    assert rejected, 'Old developer layout must fail before delivery'
    (root / 'tools').rename(root / '.dev-tools')
    for relative in ('.dev-tools/tests/obj/Generated.cs', 'obj/AssemblyInfo.cs', 'bin/Helper.cs', 'code/.Hidden.cs'):
        path = root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text('// fixture', encoding='utf-8')
    # Hidden files are excluded by NML even when in code; do not make them production inputs.
    (root / 'code/.Hidden.cs').unlink()
    assert len(scan.validate(root)) == 2
    (root / 'code/.Hidden.cs').write_text('// fixture', encoding='utf-8')
    assert root.joinpath('code/.Hidden.cs').resolve() not in scan.selected_sources(root)
print('PASS: old tools/stubs rejected; .dev-tools and generated caches excluded by NML scan')
