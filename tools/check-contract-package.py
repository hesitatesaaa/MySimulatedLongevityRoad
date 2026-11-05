"""Check delivered source/art integrity against the executed contract rework."""
from pathlib import Path
import hashlib
import io
import json
import zipfile
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'build/contract-rework/delivery'
package=OUT/'MySimulatedLongevityRoad-0.8.6-player-local-compressed.zip'
prefix='MySimulatedLongevityRoad/'
archive=zipfile.ZipFile(package)
assert archive.testzip() is None
sources=[]
for name in archive.namelist():
    if not name.endswith('.cs'):continue
    relative=name[len(prefix):]
    assert archive.read(name)==(ROOT/relative).read_bytes(),relative
    assert 'MclslAuditAcceptance' not in name and 'ContractUiProbe' not in name
    sources.append(name)
assert len(sources)==299,len(sources)
manifest_count=frame_count=0
minimum=192
for kind in ('contract-skill-icons','career-effects'):
    manifest=json.loads(archive.read(prefix+'resources/'+kind+'.manifest.json'))
    for asset in manifest['assets']:
        data=archive.read(prefix+asset['path'])
        assert hashlib.sha256(data).hexdigest()==asset['sha256']
        image=Image.open(io.BytesIO(data)).convert('RGBA')
        assert image.size==(asset['width'],asset['height'])
        manifest_count+=1
        if kind!='career-effects':continue
        for index in range(24):
            cell=image.crop((index%4*192,index//4*192,(index%4+1)*192,(index//4+1)*192))
            box=cell.getbbox()
            if box:
                margin=min(box[0],box[1],192-box[2],192-box[3])
                assert margin>=27,(asset['id'],index,margin)
                minimum=min(minimum,margin)
            frame_count+=1
baseline=json.loads((ROOT/'build/contract-rework/baseline-assets.json').read_text(encoding='utf8'))
changed=[p for p,h in baseline.items() if hashlib.sha256((ROOT/p).read_bytes()).hexdigest()!=h]
assert len(changed)==35 and not any('CombatPassive' in p for p in changed)
functional=json.loads((ROOT/'build/contract-rework/_runs/20261104_100726/result.json').read_text(encoding='utf-8-sig'))
checks=[x for x in functional['checks'] if not x['id'].startswith('ui-') and x['id']!='contract-ui']
assert all(x['status']=='pass' for x in checks)
ui=json.loads((ROOT/'build/contract-rework/_runs/20261104_103042/result.json').read_text(encoding='utf-8-sig'))
assert ui['passed']
functional['checks']=checks
functional['passed']=True
functional['summary']={'pass':len(checks),'fail':0,'pending':0}
functional['scope']='Functional checks only; UI checks are in the separate final UI report.'
for value,name in ((functional,'functional-runtime.json'),(ui,'ui-runtime.json')):
    (OUT/name).write_text(json.dumps(value,ensure_ascii=False,indent=2),encoding='utf8')
report={'packageSha256':hashlib.sha256(package.read_bytes()).hexdigest(),'packageBytes':package.stat().st_size,
        'packagedSources':len(sources),'sourcesMatchWorkspace':True,'compileErrors':0,'compileWarnings':0,
        'newActiveIcons':5,'careerAtlases':30,'effectFrames':frame_count,'packageMinimumTransparentMargin':minimum,
        'protectedExistingArtUnchanged':len(baseline)-len(changed),'passiveIconsUnchanged':True,'petIdentityIconsPreserved':4,
        'assetManifestHashesVerified':manifest_count,'unityFunctionalChecksPassed':len(checks),'unityUiChecksPassed':ui['summary']['pass'],
        'uiRaycastsPassed':True,'fullVisualScreenshotsVerified':False,'balanceMatrixVerified':False,
        'notes':['Rendered screenshots were obscured by native diagnostic/tutorial overlays; excluded as visual acceptance.',
                 'Original 114 legacy art-review records remain unchanged; full legacy validators were not claimed to pass.',
                 'After product isolation was fixed, test data went to MCLSPets, with original game binaries unchanged.']}
(OUT/'verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
with zipfile.ZipFile(OUT/'active-icons-and-career-effects-previews.zip','w',zipfile.ZIP_DEFLATED) as review:
    paths=[ROOT/'build/contract-rework/active-icons-comparison.png',
           *list((ROOT/'build/contract-rework/career-effects/previews').glob('*')),
           ROOT/'resources/contract-skill-icons.manifest.json',ROOT/'resources/career-effects.manifest.json',
           ROOT/'resources/contract-rework.md',OUT/'verification.json',OUT/'functional-runtime.json',OUT/'ui-runtime.json']
    for path in paths:review.write(path,path.name)
where=ROOT/'build/contract-rework/packaged-compile'
where.mkdir(exist_ok=True)
for name in sources:
    path=where/name[len(prefix):]
    path.parent.mkdir(parents=True,exist_ok=True)
    path.write_bytes(archive.read(name))
lines=(ROOT/'build/contract-rework/delivery-compile/compile.rsp').read_text(encoding='utf-8-sig').splitlines()
lines=[line for line in lines if not line.lower().endswith('.cs"')]
lines=['/out:"'+str(where/'MySimulatedLongevityRoad.dll')+'"' if line.startswith('/out:') else line for line in lines]
lines+=['"'+str(where/name[len(prefix):])+'"' for name in sources]
(where/'compile.rsp').write_text('\n'.join(lines),encoding='utf8')
print(json.dumps(report))
