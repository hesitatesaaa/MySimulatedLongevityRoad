"""Build a separate queue for visually rejected native frames; archive only on --activate."""
import argparse
import hashlib
import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'output' / 'actor-redraw'
QUEUE = WORK / 'native-pose-corrections.jobs.json'
ARCHIVE = WORK / 'rejected' / 'pose-continuity-20261013'
TARGETS = json.loads((ROOT / 'resources' / 'actor-redraw.targets.json').read_text(encoding='utf-8-sig'))
FORM_BY_ID = {form['id']: form for form in TARGETS['forms']}
CORRECTIONS = (
    ('mclsl_beast_dafeng', 'hit', 0,
     'Redraw the startled backward recoil with EXACTLY TWO anatomical legs and two claws total. '
     'One leg supports the weight and the other folds toward the chest. Remove all additional legs/claws. '
     'Retain the blue-white wings, gold crest and identity; transition toward hit_1.'),
    ('mclsl_beast_baihu_yuan', 'death', 2,
     'Draw a new middle collapse pose. Compared with death_1, the head, chest and hips must sink LOWER; '
     'front leg folds slack, with no rising torso or straightened supporting leg. '
     'It should transition naturally toward the lower death_3 pose.'),
    ('mclsl_beast_zhuque_yuan', 'attack', 5,
     'Draw the immediate recoil from the fully extended casting pose in attack_4. '
     'The casting arm remains halfway extended and is pulling inward; the long flame sleeve follows with inertia. '
     'Body weight and feet begin to settle, but this frame must NOT snap back to idle. '
     'The next attack_6 continues the recovery.'),
    ('mclsl_beast_moqilin', 'death', 5,
     'Draw the final settled death pose after death_4. Head and chest stay ON the ground or lower, '
     'eyes closed, legs fully slack, tail and mane resting. Do not raise the head or torso or regain support.'),
    ('mclsl_beast_fenghuang', 'attack', 3,
     'Redraw the opening claw strike with EXACTLY TWO legs: one rear supporting leg and one forward raised striking leg. '
     'Remove the extra third leg. Retain the raised wings and forward-leaning torso between attack_2 and attack_4.'),
    ('mclsl_beast_fenghuang', 'attack', 6,
     'Redraw an intermediate recovery after attack_5: striking claw halfway retracts toward the body, '
     'wings lower gradually, torso remains slightly forward. Do not snap directly to the fully upright idle pose; '
     'attack_7 is the later standing recovery.'),
)

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def paths(actor, action, index):
    name = f'{action}_{index}'
    staged = WORK / 'frames' / actor / 'main' / (name + '.png')
    source = WORK / 'originals' / actor / (name + '.png')
    return source, staged, staged.with_suffix('.provenance.json')

def build_jobs():
    jobs = []
    for actor, action, index, correction in CORRECTIONS:
        form = FORM_BY_ID[actor]
        source, output, proof = paths(actor, action, index)
        record = json.loads(proof.read_text(encoding='utf-8-sig'))
        if sha(source) != record['sourceSha256'] or sha(output) != record['outputSha256']:
            raise ValueError('Existing frame provenance differs: ' + actor + '/' + output.stem)
        review = json.loads((ROOT / 'resources' / 'actor-native-reviews' / (actor + '.json')).read_text(encoding='utf-8-sig'))
        if not any(f.get('severity') == 'needs-redraw' and f.get('action') == action
                   and f.get('frame') == index for f in review.get('visualReviewFindings', [])):
            raise ValueError('Frame has no visual rejection: ' + actor + '/' + output.stem)
        refs = []
        for adjacent in (0, index - 1, index + 1):
            candidate = WORK / 'originals' / actor / f'{action}_{adjacent}.png'
            if candidate.exists() and candidate != source and candidate not in refs:
                refs.append(candidate)
        if not refs:
            refs.append(WORK / 'originals' / actor / 'idle_0.png')
        old_prompt = record['prompt']
        quality = ('Balanced high-resolution pixel art at the approved Li Fan/Baihu detail level: '
                   'clear outline, broad clean pixel clusters, two or three readable shade levels per material; '
                   'retain this character’s unique anatomy, colors, ornaments and gender. '
                   'Exactly one independently redrawn character in one transparent 1254x1254 PNG. '
                   'One frame only; no sprite sheet, grid, panels, background, particles, shadows, text, clipping, '
                   'resampling, reuse, transformation or recoloring of another frame.')
        prompt = (f'Use case: stylized-concept. Independently redraw {form["name"]} '
                  f'({actor}) {action}_{index} as ONE animation frame. '
                  f'The attached images are identity and adjacent-pose references, not material to paste. '
                  f'Correction required: {correction} {quality} '
                  f'Preserve action sequence continuity; previous rejected frame must not be copied. '
                  f'Original context: {old_prompt[:420]}')
        prompt_file = WORK / 'prompts' / actor / f'{action}_{index}.pose-correction.txt'
        prompt_file.parent.mkdir(parents=True, exist_ok=True)
        prompt_file.write_text(prompt + '\n', encoding='utf-8')
        jobs.append(dict(actor=actor, name=form['name'], action=action, index=index,
                         artDirectionRevision=2, prompt=prompt, promptFile=str(prompt_file),
                         references=[str(path) for path in refs], output=str(output),
                         replacedSourceSha256=sha(source), rejectedProof=str(proof)))
    return jobs

def activate(jobs):
    control = json.loads((WORK / 'pipeline.control.json').read_text(encoding='utf-8-sig'))
    if control.get('poseCorrectionsActivatedAtUtc'):
        raise ValueError('This correction archive has already run; do not archive reviewed replacement frames again')
    # Call only after one true missing-frame quota probe succeeds. Individual files
    # are archived, never recursively moved or deleted, and every hash is checked.
    for job in jobs:
        source, staged, proof = paths(job['actor'], job['action'], job['index'])
        if sha(source) != job['replacedSourceSha256']:
            raise ValueError('Old candidate changed: ' + str(source))
        for current, subdir in ((source, 'originals'), (staged, 'frames'), (proof, 'provenance')):
            destination = ARCHIVE / job['actor'] / subdir / current.name
            destination.parent.mkdir(parents=True, exist_ok=True)
            if destination.exists():
                raise FileExistsError(destination)
            shutil.move(str(current), str(destination))
    print(f'Archived {len(jobs)} rejected frames; correction queue now has free output paths')

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--activate', action='store_true')
    args = parser.parse_args()
    jobs = build_jobs() if not args.activate else json.loads(QUEUE.read_text(encoding='utf-8'))
    if not args.activate:
        QUEUE.write_text(json.dumps(jobs, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
        print(f'Prepared {len(jobs)} independent correction jobs: {QUEUE}')
    else:
        activate(jobs)
