"""Check complete original-source actions and propose per-frame geometry; never install or approve."""
import argparse
import hashlib
import json
from pathlib import Path
from datetime import datetime, timezone
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT/'output/actor-redraw'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--actor', required=True)
args = parser.parse_args()
targets = json.loads((ROOT/'resources/actor-redraw.targets.json').read_text(encoding='utf-8-sig'))
form = next(f for f in targets['forms'] if f['id'] == args.actor)
profile_path = WORK/'frames'/args.actor/'normalization-profile.json'
if profile_path.exists():
    profile = json.loads(profile_path.read_text(encoding='utf-8-sig'))
else:
    proof = json.loads((WORK/'frames'/args.actor/'main/idle_0.provenance.json').read_text(encoding='utf-8-sig'))
    normalization = proof['normalization']
    scale = normalization['scale']
    translation = normalization['translation']
    profile = dict(scale=scale, sourceAnchorX=normalization['sourceAnchorX'],
                   sourceAnchorY=(form['anchor'][1]-translation[1])/scale)
ppu = form['pixelsPerUnit']/profile['scale']
actions = {}
missing = []
source_hashes = set()
duplicates = []
decoded = 0
for action, specification in form['actions'].items():
    if specification.get('generationRequired') is False:
        actions[action] = dict(loop=False, frames=[], visualFallback='idle', durationMs=specification['fallbackDurationMs'])
        continue
    frames = []
    indices = specification.get('frameOrder', specification.get('indices', list(range(specification['count']))))
    # Explicit reviewed order may be recorded as frameIndices by older authoring tools.
    indices = specification.get('frameIndices', indices)
    for index in indices:
        name = f'{action}_{index}'
        staged = WORK/'frames'/args.actor/'main'/(name+'.png')
        proof_path = staged.with_suffix('.provenance.json')
        if not staged.exists() or not proof_path.exists():
            missing.append(name)
            continue
        proof = json.loads(proof_path.read_text(encoding='utf-8-sig'))
        source = (ROOT/proof['sourceFile']).resolve()
        source.relative_to(ROOT.resolve())
        digest = hashlib.sha256(source.read_bytes()).hexdigest()
        if digest != proof['sourceSha256'] or hashlib.sha256(staged.read_bytes()).hexdigest() != proof['outputSha256']:
            raise ValueError('Source/frame hash mismatch: '+name)
        if digest in source_hashes:
            duplicates.append(name)
        source_hashes.add(digest)
        with Image.open(source) as im:
            canvas = list(im.size)
            bounds = im.convert('RGBA').getchannel('A').point(lambda a: 255 if a >= 128 else 0).getbbox()
        if not bounds:
            raise ValueError('Empty original: '+name)
        ground = bounds[3] if form['family'] in ('human','quadruped','heavy') else profile.get('sourceAnchorY',bounds[3])
        decoded += canvas[0]*canvas[1]*4
        frames.append(dict(name=name, sourceFile=str(source.relative_to(ROOT)), sourceSha256=digest,
            canvas=canvas, visibleBoundsAtAlpha128=list(bounds),
            sourceTouchesEdge=bounds[0] <= 0 or bounds[1] <= 0 or bounds[2] >= canvas[0] or bounds[3] >= canvas[1],
            anatomyReview=proof.get('anatomyReview', {}),
            proposedGeometry=dict(canvas=canvas, anchor=[profile['sourceAnchorX'],ground],
                pixelsPerUnit=ppu, anchorReviewed=False), nativeCandidate=proof.get('nativeResolution',False)))
    actions[action] = dict(loop=action in ('idle','walk'),frames=frames)
death_heights = [f['visibleBoundsAtAlpha128'][3]-f['visibleBoundsAtAlpha128'][1] for f in actions['death']['frames']]
report = dict(actorId=args.actor, checkedAtUtc=datetime.now(timezone.utc).isoformat(),actions=actions,
    missingFrames=missing, duplicateSources=duplicates, rgba32SourceBytes=decoded,
    deathSilhouetteHeights=death_heights,
    deathHeightRiseCandidates=[i for i in range(1,len(death_heights)) if death_heights[i] > death_heights[i-1]],
    sourceFilesComplete=not missing, geometryApproved=False, actionContinuityApproved=False,
    installed=False, note='Geometry is a proposal using existing normalization scale. Silhouette changes are review hints, not automatic rejection. Original PNGs remain untouched.')
destination = ROOT/'resources/actor-native-reviews'/(args.actor+'.json')
destination.parent.mkdir(parents=True, exist_ok=True)
if destination.exists():
    previous = json.loads(destination.read_text(encoding='utf-8-sig'))
    old_frames = {f['name']: f for c in previous.get('actions', {}).values() for f in c.get('frames', [])}
    for clip in report['actions'].values():
        for frame in clip['frames']:
            old = old_frames.get(frame['name'], {})
            if old.get('sourceSha256') == frame['sourceSha256'] and old.get('anatomyReview'):
                frame['anatomyReview'] = old['anatomyReview']
    for key in ('visualReviewFindings', 'reviewerNotes'):
        if key in previous:
            report[key] = previous[key]
destination.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(dict(report=str(destination),missing=missing,decodedMiB=round(decoded/1048576,2),
    deathHeights=death_heights,installed=False)))
