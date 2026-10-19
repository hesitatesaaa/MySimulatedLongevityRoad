"""Prepare missing independent frames using full-resolution identity and clarity references."""
import json
import argparse
from copy import deepcopy
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT/'output/actor-redraw'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--count', type=int, default=8)
args = parser.parse_args()
if not 1 <= args.count <= 1238:
    raise ValueError('Batch count outside supported missing-queue range')
control = json.loads((WORK/'pipeline.control.json').read_text(encoding='utf-8-sig'))
if control.get('stopDispatch'):
    raise ValueError('Dispatch stopped; successful built-in quota probe required before preparing a batch')
queue_path = WORK/'balanced-resume-after-quota.jobs.json'
jobs = json.loads(queue_path.read_text(encoding='utf-8-sig'))
remaining = [j for j in jobs if not Path(j['output']).exists()]
batch_name = 'original-frame-resume-'+datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%S%fZ')
quality = WORK/'originals/mclsl_beast_zhuque_yuan/attack_2.png'
batch = []
for item in remaining[:args.count]:
    job = deepcopy(item)
    identity = WORK/'originals'/job['actor']/'idle_0.png'
    if not identity.is_file() or not quality.is_file():
        raise ValueError('Missing full-resolution reference')
    job['references'] = [str(identity), str(quality)]
    job['prompt'] += (' Image 1 is this form full-resolution identity and anatomical scale reference. '
        'Image 2 is the user-approved CLARITY and fine pixel density reference only: do not copy its human anatomy, clothes or palette into another character. '
        'Match clear facial/anatomical edges, hair/fur/feather/fins and material detail at native source resolution. '
        'No low-resolution installed-size reference or artificial coarse enlarged pixel blocks. '
        'Retain the same anatomical pixel scale as Image 1; posed limbs may fold but never shrink the skeleton. '
        'Moderate ornament detail and restrained shades; do not add excessive gold microtexture or photo realism. '
        'Exactly one independently drawn PNG with transparent full canvas; no grid, cutting, cropping, downsampling or interpolation. '
        'Native PNG will be retained without resampling and reviewed before runtime integration.')
    prompt = WORK/'prompts'/batch_name/job['actor']/(job['action']+'_'+str(job['index'])+'.txt')
    prompt.parent.mkdir(parents=True, exist_ok=True)
    prompt.write_text(job['prompt']+'\n', encoding='utf-8')
    job['promptFile'] = str(prompt)
    batch.append(job)
batch_path = WORK/(batch_name+'.jobs.json')
batch_path.write_text(json.dumps(batch, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(json.dumps(dict(batchFile=str(batch_path), jobs=batch, remainingJobs=len(remaining)), ensure_ascii=False))
