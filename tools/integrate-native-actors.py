"""Integrate current target originals without claiming visual or Unity acceptance."""
import argparse
import copy
import hashlib
import json
import math
import shutil
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'output/actor-redraw'


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def within(relative):
    path = (ROOT / relative).resolve()
    path.relative_to(ROOT)
    return path


def prepare(actor_ids=None):
    policy = read(ROOT / 'resources/0.6.0-release-policy.json')
    assert policy['visualReview'] == 'waived-by-user' and policy['requireUnityValidation'] is True
    targets = read(ROOT / 'resources/actor-redraw.targets.json')
    manifest = read(ROOT / 'resources/actor-frames.manifest.json')
    groups = {g['actorId']: g for g in manifest['groups']}
    assert set(groups) == {f['id'] for f in targets['forms']}
    assert not actor_ids or set(actor_ids) <= set(groups)
    operations, inventory, source_owners = [], [], set()
    for target in targets['forms']:
        actor = target['id']
        if actor_ids and actor not in actor_ids:
            continue
        group = groups[actor]
        review_file = ROOT / 'resources/actor-native-reviews' / (actor + '.json')
        review = read(review_file) if review_file.exists() else {}
        reviewed = {f['name']: f for a in review.get('actions', {}).values() for f in a.get('frames', [])}
        old_entries = {f['name']: f for a in group['actions'].values() for f in a.get('frames', [])}
        profile_file = WORK / 'frames' / actor / 'normalization-profile.json'
        profile = read(profile_file) if profile_file.exists() else None
        installed = copy.deepcopy(group)
        installed['nativeResourcePath'] = target['resourcePath']
        installed['integrationAccepted'] = True
        installed['geometryConfigured'] = True
        installed['visualReviewStatus'] = 'waived-by-user'
        installed['calibration']['verifiedInGame'] = False
        installed['calibration']['previewScaleApproved'] = True
        installed['calibration']['measurementStatus'] = 'awaiting-final-Unity-validation'
        assert math.isfinite(installed['calibration']['renderScale']) and installed['calibration']['renderScale'] > 0
        for action, spec in target['actions'].items():
            if spec.get('generationRequired') is False:
                installed['actions'][action] = dict(loop=False, frames=[], visualFallback='idle', durationMs=spec['fallbackDurationMs'])
                continue
            indices = spec.get('frameIndices', spec.get('frameOrder', spec.get('indices', list(range(spec['count'])))))
            entries = []
            for index in indices:
                name = f'{action}_{index}'
                staged = WORK / 'frames' / actor / 'main' / (name + '.png')
                proof = read(staged.with_suffix('.provenance.json'))
                source = within(proof['sourceFile'])
                digest = sha(source)
                assert digest == proof['sourceSha256'], (actor, name, 'source hash')
                assert sha(staged) == proof['outputSha256'], (actor, name, 'staged hash')
                assert proof['actorId'] == actor and proof['action'] == action and proof['index'] == index
                assert proof['method'] == 'built-in-imagegen-independent-frame'
                assert digest not in source_owners, (actor, name, 'reused original')
                source_owners.add(digest)
                with Image.open(source) as image:
                    assert image.format == 'PNG'
                    canvas = list(image.size)
                    bounds = image.convert('RGBA').getchannel('A').point(lambda a: 255 if a >= 128 else 0).getbbox()
                assert bounds, (actor, name, 'empty original')
                old = old_entries.get(name, {})
                checked = reviewed.get(name, {})
                if old.get('sha256') == digest and group.get('nativeResourcePath'):
                    geometry = {k: old.get(k, group[k]) for k in ('canvas', 'anchor', 'pixelsPerUnit')}
                elif checked.get('sourceSha256') == digest and checked.get('proposedGeometry'):
                    geometry = copy.deepcopy(checked['proposedGeometry'])
                elif proof.get('integrationGeometry'):
                    geometry = copy.deepcopy(proof['integrationGeometry'])
                    geometry['canvas'] = canvas
                    if target['family'] in ('human', 'quadruped', 'heavy'):
                        geometry['anchor'][1] = bounds[3]
                else:
                    normalization = proof['normalization']
                    scale = profile['scale'] if profile else normalization['scale']
                    anchor_x = profile['sourceAnchorX'] if profile else normalization.get('sourceAnchorX',
                        (target['anchor'][0] - normalization.get('translation', [0, 0])[0]) / scale)
                    anchor_y = bounds[3] if target['family'] in ('human', 'quadruped', 'heavy') else (
                        profile['sourceAnchorY'] if profile else target['anchor'][1] / scale)
                    geometry = dict(canvas=canvas, anchor=[anchor_x, anchor_y], pixelsPerUnit=target['pixelsPerUnit'] / scale)
                assert geometry['canvas'] == canvas
                assert all(math.isfinite(v) and 0 <= v <= canvas[i] for i, v in enumerate(geometry['anchor']))
                assert math.isfinite(geometry['pixelsPerUnit']) and geometry['pixelsPerUnit'] > 0
                destination = within('NativeActorResources/' + target['resourcePath'] + '/main/' + name + '.png')
                provenance = ROOT / 'resources/actor-provenance' / actor / 'native' / (name + '.json')
                native_proof = copy.deepcopy(proof)
                native_proof.update(outputFile=destination.relative_to(ROOT).as_posix(), outputSha256=digest,
                                    nativeResolution=True, pendingNativeRuntimeIntegration=False,
                                    integrationStatus='accepted-by-user-without-further-visual-review')
                native_proof['normalization'] = dict(resampling='none', sourceCanvas=canvas, canvas=canvas,
                    scale=1, cropped=False, spritesheetSlicing=False, byteIdenticalToGeneratedSource=True)
                entry = dict(name=name, durationMs=spec['durationMs'], sha256=digest, origin='independently-redrawn',
                    provenance=provenance.relative_to(ROOT).as_posix(), geometryConfigured=True,
                    anchorReviewed=old.get('anchorReviewed', False) if old.get('sha256') == digest else False,
                    **{k: geometry[k] for k in ('canvas', 'anchor', 'pixelsPerUnit')})
                entries.append(entry)
                operations.append((source, destination, provenance, native_proof))
                inventory.append(dict(actorId=actor, frame=name, sha256=digest, destination=destination.relative_to(ROOT).as_posix()))
            installed['actions'][action] = dict(loop=action in ('idle', 'walk'), frames=entries)
        first = installed['actions']['idle']['frames'][0]
        for key in ('canvas', 'anchor', 'pixelsPerUnit'):
            installed[key] = first[key]
        manifest['groups'][manifest['groups'].index(group)] = installed
    manifest['status'] = 'integrated-awaiting-Unity-validation'
    return manifest, operations, inventory


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--apply', action='store_true')
    parser.add_argument('--actor', action='append', help='Restrict a repair update to already integrated forms')
    args = parser.parse_args()
    manifest, operations, inventory = prepare(args.actor)
    changed = sum(not dst.exists() or sha(dst) != proof['outputSha256'] for _, dst, _, proof in operations)
    if args.apply:
        # Preflight above completes for every target before any installation write.
        for source, destination, provenance, proof in operations:
            if destination.exists() and sha(destination) != proof['outputSha256']:
                previous = sha(destination)
                backup = WORK / 'rejected/pre-integration-20261019' / destination.relative_to(ROOT)
                backup = backup.with_name(backup.stem + '-' + previous[:12] + '.png')
                backup.parent.mkdir(parents=True, exist_ok=True)
                if not backup.exists():
                    shutil.copyfile(destination, backup)
                assert sha(backup) == previous
            destination.parent.mkdir(parents=True, exist_ok=True)
            if not destination.exists() or sha(destination) != proof['outputSha256']:
                shutil.copyfile(source, destination)
            assert sha(destination) == proof['outputSha256']
            provenance.parent.mkdir(parents=True, exist_ok=True)
            provenance.write_text(json.dumps(proof, ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
        (ROOT / 'resources/actor-frames.manifest.json').write_text(json.dumps(manifest, ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
        record_file = WORK / 'integration-20261019.json'
        previous = read(record_file)['frames'] if record_file.exists() else []
        merged = {(f['actorId'], f['frame']): f for f in previous + inventory}
        record_file.write_text(json.dumps(dict(forms=len(manifest['groups']), frames=list(merged.values()),
            visualReview='waived-by-user', unityValidationPassed=False), ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(applied=args.apply, forms=len(manifest['groups']), frames=len(inventory), changedFrames=changed)))


if __name__ == '__main__':
    main()
