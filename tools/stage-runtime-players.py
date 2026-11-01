"""Prepare exact-commit and changed players with the same PNG compression policy."""
import argparse
import hashlib
import importlib.util
import json
import zipfile
from pathlib import Path

parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--runtime-root',type=Path,required=True)
parser.add_argument('--baseline-archive',type=Path,required=True)
parser.add_argument('--optimized-package',type=Path,required=True)
args=parser.parse_args()
root=args.runtime_root.resolve()
baseline=root/'baseline-source'
baseline.mkdir(exist_ok=False)
with zipfile.ZipFile(args.baseline_archive) as archive:
    names=archive.namelist();prefix=names[0].split('/')[0]+'/'
    for name in names:
        if name.endswith('/'): continue
        rel=Path(name[len(prefix):]);target=(baseline/rel).resolve()
        target.relative_to(baseline)
        target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(archive.read(name))
spec=importlib.util.spec_from_file_location('baseline_package',baseline/'tools/package-release.py')
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
entries=[(relative,source) for relative,source in module.files(module.BASE+['resources/actor-frames.manifest.json'])
         if relative!='code/MySimulatedLongevityRoad/UI/MclslDeveloperActorEditor.cs']
replacements,stats=module.compress_player_assets(entries)
records={relative:dict(bytes=len(data),sha256=hashlib.sha256(data).hexdigest())
         for relative,source in entries for data in [replacements.get(relative,source.read_bytes())]}
snapshot=dict(baseCommit='f9e1e7d37898990b58c3ea7fcc755d974a3a7e5e',localDevelopment=True,
              uncommittedChanges=False,unityRetested=False,files=records,compressionStats=stats)
baseline_zip=module.archive(root,'baseline-player.zip',entries,'Fixed upstream baseline; isolated benchmark only.',snapshot,replacements)
for kind,path in [('baseline',baseline_zip),('optimized',args.optimized_package)]:
    target=root/(kind+'-player');target.mkdir(exist_ok=False)
    with zipfile.ZipFile(path) as archive:
        for entry in archive.infolist():
            (target/entry.filename).resolve().relative_to(target)
        archive.extractall(target)
print(json.dumps(dict(baselineFrames=stats,baselineZipBytes=baseline_zip.stat().st_size,
    optimizedZipBytes=args.optimized_package.stat().st_size,baselineCommit=snapshot['baseCommit'])))
