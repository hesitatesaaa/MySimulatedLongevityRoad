"""Archive specifically rejected low-reference candidates and prepare native redraw jobs."""
import json
import shutil
import hashlib
from copy import deepcopy
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT/'output/actor-redraw'
old_batch = json.loads((WORK/'installed-frame-resume-20261013.jobs.json').read_text(encoding='utf-8-sig'))
archive = WORK/'rejected/low-reference-20261013'
if archive.exists():
    raise ValueError('Correction archive already exists; refuse repeated retirement')
quality_reference = WORK/'originals/mclsl_beast_zhuque_yuan/attack_2.png'
identity_reference = WORK/'originals/mclsl_beast_zhuque_yuan/idle_0.png'
for reference in (quality_reference, identity_reference):
    if not reference.is_file():
        raise ValueError('Missing approved high-resolution reference')
archive.mkdir(parents=True)
archived = []
jobs = []
for index, old in enumerate(old_batch):
    output = Path(old['output'])
    proof = output.with_suffix('.provenance.json')
    original = WORK/'originals'/old['actor']/(old['action']+'_'+str(old['index'])+'.png')
    for source in (output, proof, original):
        if not source.is_file():
            raise ValueError('Missing candidate to archive: '+str(source))
    for source in (output, proof, original):
        destination = archive/source.relative_to(WORK)
        destination.parent.mkdir(parents=True, exist_ok=True)
        sha = hashlib.sha256(source.read_bytes()).hexdigest()
        shutil.move(str(source), str(destination))
        archived.append(dict(previousPath=str(source), preservedPath=str(destination), sha256=sha))
    job = deepcopy(old)
    job['references'] = [str(identity_reference), str(quality_reference)]
    job['prompt'] = old['prompt'].split(' Final game asset uses')[0]
    start = job['prompt'].find('Square 1254x1254 transparent generation canvas;')
    end = job['prompt'].find('Exactly one character and one pose', start)
    if start != -1 and end != -1:
        job['prompt'] = job['prompt'][:start]+'Square 1254x1254 transparent native source canvas. '+job['prompt'][end:]
    job['prompt'] += (' Image 1 is the approved full-resolution idle identity and anatomical-size reference. '
        'Image 2 is the user-approved attack_2 CLARITY reference only, not the required pose. '
        'Match the fine crisp pixel density, clear face, fingers, hair edges and fabric folds of Image 2. '
        'Keep the figure anatomically as large as Image 1 on the source canvas; do not draw a small rough sprite centered in a large canvas. '
        'Do not reinterpret either image as 96/128-pixel sprite art, do not add coarse enlarged pixel blocks. '
        'Keep moderate ornament detail and two or three useful shade levels; no photo realism or heavy microtexture. '
        'The pose requirements in this prompt take priority over the attacking pose of Image 2. '
        'Standing return poses must return to idle stature. Death stages continuously lose support and lower, no upright return. '
        'Generate just this independent frame, save native resolution; no downsampling, no cropping, no collage. '
        'Keep limbs and robe inside transparent padding, all body parts anatomically attached.')
    prompt = WORK/'prompts/original-clarity-corrections-20261013'/old['actor']/(old['action']+'_'+str(old['index'])+'.txt')
    prompt.parent.mkdir(parents=True, exist_ok=True)
    prompt.write_text(job['prompt']+'\n', encoding='utf-8')
    job['promptFile'] = str(prompt)
    jobs.append(job)
archive_record = archive/'archive-record.json'
archive_record.write_text(json.dumps(dict(reason='User rejected coarse detail caused by installed-size generation references', files=archived), ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
batch_file = WORK/'original-clarity-corrections-20261013.jobs.json'
batch_file.write_text(json.dumps(jobs, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
queue_file = WORK/'balanced-resume-after-quota.jobs.json'
remaining = json.loads(queue_file.read_text(encoding='utf-8-sig'))
names = {j['output'] for j in jobs}
remaining = jobs + [j for j in remaining if j['output'] not in names and not Path(j['output']).exists()]
queue_file.write_text(json.dumps(remaining, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(json.dumps(dict(batch=str(batch_file), jobs=len(jobs), archivedFiles=len(archived), remainingJobs=len(remaining))))
