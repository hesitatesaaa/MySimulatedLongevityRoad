"""Create disjoint per-creature built-in-imagegen jobs. No image requests or global edits."""
import argparse
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'output/actor-redraw'
WALK_CONTACTS = (
    'Near foot clearly forward and planted; far foot trailing with heel raised.',
    'Near supporting foot passes under the hip; far knee and foot swing clearly forward.',
    'Far foot reaches forward for heel contact; near foot trails behind.',
    'Far foot clearly forward and planted; near foot trailing with heel raised.',
    'Far supporting foot passes under the hip; near knee and foot swing clearly forward.',
    'Near foot reaches forward for heel contact; far foot trails behind.',
)

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--creature', required=True)
    args = parser.parse_args()
    base = args.creature
    if not base.replace('_', '').isalnum():
        raise ValueError('Unsafe creature id')
    forms = {f['id']: f for f in json.loads((ROOT/'resources/actor-redraw.targets.json').read_text(encoding='utf-8-sig'))['forms']}
    if base not in forms:
        raise ValueError('Unknown creature')
    actors = [a for a in (base, base + '_yuan') if a in forms]
    directory = WORK/'child-tasks'/base
    directory.mkdir(parents=True, exist_ok=True)
    output = directory/'jobs.json'
    if output.exists():
        raise FileExistsError('Existing ownership queue must be resumed, not replaced')
    queue = json.loads((WORK/'balanced-resume-after-quota.jobs.json').read_text(encoding='utf-8-sig'))
    jobs = []
    for old in queue:
        if old['actor'] not in actors or old['action'] not in ('idle','walk','death','icon') or Path(old['output']).exists():
            continue
        job = dict(old)
        family = forms[job['actor']]['family']
        if family == 'human':
            anatomy = 'Exactly two arms, two hands, two legs, two feet, one head and one torso. Trace every limb continuously through coherent joints; cloth must not resemble extra limbs.'
        elif family == 'bird':
            anatomy = 'Exactly two anatomical legs and two claws total, one pair of wings, and the exact established head count from the standard reference. A raised claw replaces a grounded claw, never adds a third leg. Coherent wing joints.'
        elif family in ('quadruped','heavy'):
            anatomy = 'Exactly four anatomical legs, consistently occluded if hidden, no extra paws; coherent joints and the exact established head count from the standard reference.'
        else:
            anatomy = 'Exactly the approved reference anatomy and coherent limb joints; no extra appendages.'
        job['prompt'] += ' ANATOMY acceptance: ' + anatomy + ' Keep the entire silhouette and all extremities inside the canvas; aim for 32px transparent padding. A smaller intact transparent margin alone is not an anatomical defect or a reason to redraw. Exactly ONE newly drawn independent transparent PNG.'
        if family == 'human' and job['action'] == 'walk':
            count = forms[job['actor']]['actions']['walk']['count']
            phase = min(5, job['index'] * 6 // count)
            job['prompt'] += ' WALK CONTACT REQUIREMENT: ' + WALK_CONTACTS[phase] + ' Keep exactly two anatomical legs. Make the alternating near/far boot contacts readable beneath the robe; do not draw every phase with the same leading leg. Opposite arm swing follows the contact phase without adding hands.'
        prompt = directory/f"{job['actor']}.{job['action']}_{job['index']}.prompt.txt"
        prompt.write_text(job['prompt']+'\n',encoding='utf-8')
        job['promptFile'] = str(prompt)
        jobs.append(job)
    output.write_text(json.dumps(jobs,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(creature=base,actors=actors,jobs=len(jobs),queue=str(output),actions=sorted({j['action'] for j in jobs})),ensure_ascii=True))

if __name__ == '__main__':
    main()
