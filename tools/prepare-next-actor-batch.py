"""Prepare a bounded built-in image queue from actual missing frames; never submit images."""
import argparse,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
WORK=ROOT/'output/actor-redraw'
WALK_CONTACTS=(
    'Near foot forward and planted; far foot trailing with heel raised.',
    'LOW BROAD STRIDE: near leg remains extended forward taking weight, far leg extends backward in toe push-off. Keep the feet widely separated along the ground line, knees relaxed. No raised knee or tucked boot beneath the pelvis.',
    'Far foot reaches forward for heel contact; near foot trails behind.',
    'Far foot forward and planted; near foot trailing with heel raised.',
    'OPPOSITE LOW BROAD STRIDE: far leg remains extended forward taking weight, near leg extends backward in toe push-off. Keep the feet widely separated along the ground line, knees relaxed. No raised knee or tucked boot beneath the pelvis.',
    'Near foot reaches forward for heel contact; far foot trails behind.',
)
p=argparse.ArgumentParser();p.add_argument('--count',type=int,default=8);p.add_argument('--name');p.add_argument('--in-place',action='store_true');a=p.parse_args()
if not 1<=a.count<=32 or (not a.in_place and (not a.name or not a.name.replace('-','').replace('_','').isalnum())): raise ValueError('Invalid bounded batch')
q=json.loads((WORK/'balanced-resume-after-quota.jobs.json').read_text(encoding='utf-8-sig'))
forms={f['id']:f for f in json.loads((ROOT/'resources/actor-redraw.targets.json').read_text(encoding='utf-8-sig'))['forms']}
jobs=[];indices=[]
for queue_index,original in enumerate(q):
    if original['action'] not in ('idle','walk','death','icon') or Path(original['output']).exists():continue
    if a.in_place and original.get('inlinePromptPrepared'):
        jobs.append(dict(original));indices.append(queue_index)
        if len(jobs)==a.count:break
        continue
    j=dict(original);family=forms[j['actor']]['family']
    if family=='human': anatomy='Exactly two arms, two hands, two legs, two feet, one head and one torso. Trace each leg continuously from one pelvis through its knee and ankle to its boot. Cloth hems must not resemble extra feet or boots. Hands join exactly two wrists. No duplicated joints or extra limb-shaped fabric.'
    elif family=='bird': anatomy='Exactly two anatomical legs and two claws total, one pair of wings; a raised claw replaces a grounded claw, never adds a third leg. Every leg connects coherently to the body.'
    elif family in ('quadruped','heavy'):anatomy='Exactly four anatomical legs, with any hidden legs consistently occluded by the body; no extra paws. Trace all visible joints coherently.'
    else:anatomy='Retain exactly the reference creature anatomy, correct limb counts and coherent joint connections; do not invent extra appendages.'
    j['prompt']+=' ANATOMY acceptance: '+anatomy+' Draw the entire pose inside the canvas; aim for 32px transparent padding. A smaller intact transparent margin alone is not a defect or a reason to redraw. Do not crop. This is ONE newly drawn standalone PNG.'
    if family=='human' and j['action']=='walk':
        phase=min(5,j['index']*6//forms[j['actor']]['actions']['walk']['count'])
        j['prompt']=re.sub(r'Near leg is [^.]*\.\s*','',j['prompt'])
        j['prompt']='PRIMARY ACTION POSE: '+WALK_CONTACTS[phase]+' USER GAIT DIRECTION: broad alternating near/far strides, boots close to the ground, at most slight natural sole clearance. Never march with a high raised knee or bring the boots together beneath the torso. The reference gives identity, not the standing pose; newly draw the specified leg contact.\n'+j['prompt']
        j['prompt']+=' WALK CONTACT REQUIREMENT: '+WALK_CONTACTS[phase]+' Make the alternating near/far boot contacts readable beneath the robe; do not draw every phase with the same leading leg. Opposite arm swing must not add hands.'
    if a.in_place:
        j.pop('promptFile',None)
        j['inlinePromptPrepared']=True
        q[queue_index]=j
    else:
        j['promptFile']=str(WORK/'prompts'/j['actor']/(f"{j['action']}_{j['index']}.anatomy-guard.txt"))
        file=Path(j['promptFile']);file.parent.mkdir(parents=True,exist_ok=True);file.write_text(j['prompt']+'\n',encoding='utf-8')
    jobs.append(j)
    indices.append(queue_index)
    if len(jobs)==a.count:break
if a.in_place:
    output=WORK/'balanced-resume-after-quota.jobs.json'
    output.write_text(json.dumps(q,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(queue=output.name,jobIndices=indices,count=len(jobs)),ensure_ascii=True))
    raise SystemExit(0)
output=WORK/(a.name+'.jobs.json')
if output.exists():raise FileExistsError(output)
output.write_text(json.dumps(jobs,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(dict(queue=output.name,jobs=jobs),ensure_ascii=True))
