"""Retired low-resolution-reference helper; use the native clarity correction workflow."""
raise SystemExit('Retired by user instruction: use high-resolution original references, not installed-size references.')
import json
from copy import deepcopy
from pathlib import Path
from datetime import datetime, timezone
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'output/actor-redraw'
queue_file = WORK / 'balanced-resume-after-quota.jobs.json'
jobs = json.loads(queue_file.read_text(encoding='utf-8-sig'))
remaining = [j for j in jobs if not Path(j['output']).exists()]
queue_file.write_text(json.dumps(remaining, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
batch_name = 'installed-frame-resume-' + datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%S%fZ')
batch = []
for job in remaining[:8]:
    j = deepcopy(job)
    reference = WORK / 'frames' / j['actor'] / 'main/idle_0.png'
    if not reference.is_file():
        raise ValueError('Missing reviewed installed-size reference: '+str(reference))
    j['references'] = [str(reference)]
    profile = json.loads((reference.parents[1]/'normalization-profile.json').read_text(encoding='utf-8-sig'))
    with Image.open(reference) as ref:
        bounds = ref.convert('RGBA').getchannel('A').point(lambda a: 255 if a >= 128 else 0).getbbox()
    expected_source_height = round((bounds[3]-bounds[1])/profile['scale'])
    expected_source_width = round((bounds[2]-bounds[0])/profile['scale'])
    j['prompt'] = j['prompt'].replace('Square 1254x1254 source canvas, transparent RGBA.',
        f'Square 1254x1254 transparent generation canvas. Image 1 is an installed-size identity reference. Its standing/resting silhouette corresponds to approximately {expected_source_height} pixels high and {expected_source_width} pixels wide on this generation canvas before the fixed installation conversion. Keep anatomical limb lengths and head size consistent with that source scale; bent/folded poses naturally lower total height. Installation padding differs from source padding: do not match the fraction of reference canvas. Independently redraw clean detailed pixel clusters at source resolution; never enlarge or paste the reference bitmap.')
    j['prompt'] += ' Final game asset uses the existing installed canvas and stable world size, with only one nearest-neighbor conversion from this independently generated frame. Keep limbs within safe padding.'
    prompt_file = WORK / 'prompts' / batch_name / j['actor'] / (j['action']+'_'+str(j['index'])+'.txt')
    prompt_file.parent.mkdir(parents=True, exist_ok=True)
    prompt_file.write_text(j['prompt']+'\n', encoding='utf-8')
    j['promptFile'] = str(prompt_file)
    batch.append(j)
batch_path = WORK / (batch_name + '.jobs.json')
batch_path.write_text(json.dumps(batch, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
control_file = WORK / 'pipeline.control.json'
control = json.loads(control_file.read_text(encoding='utf-8-sig'))
control.update(stopDispatch=False, reason='built-in-probe-succeeded', resumedAtUtc=datetime.now(timezone.utc).isoformat())
control.pop('retryAfterUtc', None)
control_file.write_text(json.dumps(control, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(json.dumps(dict(batchFile=str(batch_path), jobs=batch, remainingJobs=len(remaining)), ensure_ascii=False))
