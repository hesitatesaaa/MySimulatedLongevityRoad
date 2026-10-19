"""Compare measured Unity geometry with PNG alpha bounds; never resample assets."""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
p = argparse.ArgumentParser(description=__doc__)
p.add_argument('--probe', required=True)
p.add_argument('--reference-root', required=True)
p.add_argument('--output', required=True)
a = p.parse_args()
probe = json.loads(Path(a.probe).read_text(encoding='utf-8-sig'))
manifest = json.loads((ROOT/'resources/actor-frames.manifest.json').read_text(encoding='utf-8-sig'))
samples = probe['samples']

def measure(sample, root):
    name = sample['sprite'].removeprefix('gen_')
    path = root/sample['texturePath']/(name+'.png')
    with Image.open(path) as im:
        bounds = im.convert('RGBA').getchannel('A').point(lambda v: 255 if v >= 128 else 0).getbbox()
        if not bounds: raise ValueError('Empty subject: '+str(path))
        if list(im.size) != [int(sample['rect'][2]), int(sample['rect'][3])]:
            raise ValueError('PNG/sprite rect differs: '+str(path))
    return dict(file=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
        alphaBounds=list(bounds), visibleWorldWidth=(bounds[2]-bounds[0])/sample['ppu']*abs(sample['actualRenderScale'][0]),
        visibleWorldHeight=(bounds[3]-bounds[1])/sample['ppu']*abs(sample['actualRenderScale'][1]))

results=[]
for group in manifest['groups']:
    if not group.get('approved'): continue
    aid=group['actorId']; reference=group['calibration']['reference']
    sample=samples[aid]
    native=bool(group.get('nativeResourcePath'))
    own=measure(sample,ROOT/('NativeActorResources' if native else 'GameResources'))
    ref=measure(samples[reference],Path(a.reference_root)/'GameResources')
    ratio=own['visibleWorldHeight']/ref['visibleWorldHeight']
    human=aid.endswith('_yuan') or aid=='mclsl_named_lifan'
    results.append(dict(actorId=aid, reference=reference, own=own, referenceGeometry=ref,
        heightRatio=ratio, exceedsHeightTolerance=abs(ratio-1)>0.10,
        measurementKind='human-body-height-proxy' if human else 'silhouette-height-screening-only',
        proposedHeightScale=abs(sample['actualRenderScale'][1])/ratio,
        verifiedInGame=False,
        limitation='Human head ornaments and beast wings/tails require reviewed body landmarks; no formal size pass from alpha bounds alone.'))
report=dict(kind='actual-unity-size-comparison-screening', probeSha256=hashlib.sha256(Path(a.probe).read_bytes()).hexdigest(),
    actualPlacedActors=True, forcedVisibility=probe.get('visibilityForcedForDiagnostic'), pausedFromPlacement=probe.get('pausedFromPlacement'),
    comparedForms=len(results), sizeComparisonPassed=False, framesChanged=False, results=results)
dest=Path(a.output);dest.parent.mkdir(parents=True,exist_ok=True)
dest.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(dict(forms=len(results),heightRatios={x['actorId']:round(x['heightRatio'],3) for x in results}),ensure_ascii=True))
