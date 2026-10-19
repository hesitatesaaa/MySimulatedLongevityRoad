"""Native installation with an explicit review waiver, and mechanical release checks."""
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
MANIFEST = 'resources/actor-frames.manifest.json'


def read(path):
    return json.loads(Path(path).read_text(encoding='utf-8-sig'))


def write(path, value):
    Path(path).parent.mkdir(parents=True, exist_ok=True)
    Path(path).write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def within(root, relative):
    path = (root / relative).resolve()
    path.relative_to(root.resolve())
    return path


def indices(spec):
    return spec.get('frameIndices', spec.get('frameOrder', spec.get('indices', list(range(spec['count'])))))


def png(path):
    with Image.open(path) as image:
        image.load()
        if image.format != 'PNG' or 'A' not in image.getbands():
            raise ValueError('Expected PNG with alpha: ' + str(path))
        alpha = image.getchannel('A')
        bounds = alpha.point(lambda a: 255 if a >= 128 else 0).getbbox()
        if not bounds or alpha.getextrema()[0] != 0:
            raise ValueError('Empty or nontransparent PNG: ' + str(path))
        return list(image.size), list(bounds)


def positive(value):
    return not isinstance(value, bool) and isinstance(value, (int, float)) and math.isfinite(value) and value > 0


def geometry(frame):
    canvas, anchor, ppu = frame['canvas'], frame['anchor'], frame['pixelsPerUnit']
    if (len(canvas) != 2 or len(anchor) != 2 or not all(positive(n) for n in canvas)
            or not positive(ppu) or any(isinstance(n, bool) or not isinstance(n, (int, float))
                or not math.isfinite(n) or n < 0 or n > limit for n, limit in zip(anchor, canvas))):
        raise ValueError('Invalid frame geometry')


def source_frame(actor, action, index):
    staged = WORK / ('icons/' + actor + '.png' if action == 'icon'
                     else 'frames/' + actor + '/main/' + f'{action}_{index}.png')
    proof = read(staged.with_suffix('.provenance.json'))
    source = within(ROOT, proof['sourceFile'])
    if (proof['actorId'] != actor or proof['action'] != action or proof['index'] != index
            or proof.get('method') != 'built-in-imagegen-independent-frame'
            or sha(staged) != proof['outputSha256'] or sha(source) != proof['sourceSha256']
            or proof.get('normalization', {}).get('spritesheetSlicing') is not False
            or proof.get('normalization', {}).get('cropped') is not False):
        raise ValueError('Invalid source or candidate provenance: ' + str(staged))
    canvas, bounds = png(source)
    return source, proof, canvas, bounds


def install_copy(source, proof, destination, provenance, canvas):
    destination.parent.mkdir(parents=True, exist_ok=True)
    if not destination.exists() or sha(destination) != proof['sourceSha256']:
        shutil.copyfile(source, destination)
    if sha(destination) != proof['sourceSha256']:
        raise ValueError('Native installation hash mismatch')
    proof = copy.deepcopy(proof)
    proof.update(outputFile=destination.relative_to(ROOT).as_posix(),
                 outputSha256=proof['sourceSha256'], nativeResolution=True,
                 pendingNativeRuntimeIntegration=False, reviewStatus='waived-by-user')
    proof['normalization'] = dict(resampling='none', sourceCanvas=canvas, canvas=canvas,
                                 scale=1, cropped=False, spritesheetSlicing=False,
                                 byteIdenticalToGeneratedSource=True)
    write(provenance, proof)


def install_form(actor):
    targets = read(ROOT / 'resources/actor-redraw.targets.json')
    target = next(f for f in targets['forms'] if f['id'] == actor)
    manifest = read(ROOT / MANIFEST)
    group = next(g for g in manifest['groups'] if g['actorId'] == actor)
    # A complete already-native installation keeps its historical review records.
    expected = {a: [f'{a}_{i}' for i in indices(s)] for a, s in target['actions'].items()
                if s.get('generationRequired') is not False}
    if group.get('nativeResourcePath') and all(
            [f['name'] for f in group['actions'][a]['frames']] == names and all(
                within(ROOT, 'NativeActorResources/' + group['nativeResourcePath'] + '/main/' + f['name'] + '.png').exists()
                and sha(within(ROOT, 'NativeActorResources/' + group['nativeResourcePath'] + '/main/' + f['name'] + '.png'))
                    == source_frame(actor, a, int(f['name'].rsplit('_', 1)[1]))[1]['sourceSha256'] == f['sha256']
                for f in group['actions'][a]['frames']) for a, names in expected.items()):
        return
    profile_path = WORK / 'frames' / actor / 'normalization-profile.json'
    if profile_path.exists():
        profile = read(profile_path)
    else:
        _, idle_proof, idle_canvas, idle_bounds = source_frame(actor, 'idle', 0)
        norm = idle_proof['normalization']
        if not positive(norm.get('scale')):
            raise ValueError('Missing original normalization scale: ' + actor)
        profile = dict(sourceCanvas=idle_canvas, scale=norm['scale'],
                       sourceAnchorX=norm.get('sourceAnchorX',
                           (target['anchor'][0] - norm['translation'][0]) / norm['scale']
                           if norm.get('translation') else (idle_bounds[0] + idle_bounds[2]) / 2),
                       sourceAnchorY=idle_bounds[3])
    installed = copy.deepcopy(group)
    installed.update(enabled=True, reviewStatus='waived-by-user', approved=False,
                     anchorReviewed=False, nativeResourcePath=target['resourcePath'])
    installed['calibration']['verifiedInGame'] = False
    installed['actions'] = {}
    for action, spec in target['actions'].items():
        if spec.get('generationRequired') is False:
            installed['actions'][action] = dict(loop=False, frames=[], visualFallback='idle',
                                               durationMs=spec['fallbackDurationMs'])
            continue
        frames = []
        for index in indices(spec):
            source, proof, canvas, bounds = source_frame(actor, action, index)
            ratio = canvas[0] / profile['sourceCanvas'][0]
            ground = bounds[3] if target['family'] in ('human', 'quadruped', 'heavy') else min(
                canvas[1], profile['sourceAnchorY'] * canvas[1] / profile['sourceCanvas'][1])
            frame = dict(name=f'{action}_{index}', durationMs=spec['durationMs'], sha256=proof['sourceSha256'],
                         canvas=canvas, anchor=[profile['sourceAnchorX'] * ratio, ground],
                         pixelsPerUnit=target['pixelsPerUnit'] / profile['scale'] * ratio,
                         anchorReviewed=False, origin='independently-redrawn')
            geometry(frame)
            destination = within(ROOT / 'NativeActorResources', target['resourcePath'] + '/main/' + frame['name'] + '.png')
            provenance = ROOT / 'resources/actor-provenance' / actor / 'native' / (frame['name'] + '.json')
            install_copy(source, proof, destination, provenance, canvas)
            frame['provenance'] = provenance.relative_to(ROOT).as_posix()
            frames.append(frame)
        installed['actions'][action] = dict(loop=action in ('idle', 'walk'), frames=frames)
    idle = installed['actions']['idle']['frames'][0]
    installed.update({k: idle[k] for k in ('canvas', 'anchor', 'pixelsPerUnit')})
    manifest['groups'][manifest['groups'].index(group)] = installed
    write(ROOT / MANIFEST, manifest)


def install_icons(actors):
    manifest = read(ROOT / MANIFEST)
    for actor in actors:
        entry = next(i for i in manifest['icons'] if i['actorId'] == actor)
        source, proof, canvas, _ = source_frame(actor, 'icon', 0)
        destination = within(ROOT / 'GameResources', entry['resourcePath'] + '.png')
        if entry.get('nativeResolution') and destination.exists() and sha(destination) == proof['sourceSha256'] == entry.get('sha256'):
            continue
        provenance = ROOT / 'resources/actor-provenance' / actor / 'native/icon.json'
        install_copy(source, proof, destination, provenance, canvas)
        entry.update(enabled=True, approved=False, reviewStatus='waived-by-user', verifiedInGame=False,
                     nativeResolution=True, canvas=canvas, sha256=proof['sourceSha256'],
                     provenance=provenance.relative_to(ROOT).as_posix())
    write(ROOT / MANIFEST, manifest)


def validate(root=ROOT, include_sources=True):
    manifest = read(root / MANIFEST)
    targets = read(root / 'resources/actor-redraw.targets.json')
    groups = manifest['groups']
    if (len(groups) != len(targets['forms']) or {g['actorId'] for g in groups} != {f['id'] for f in targets['forms']}
            or len(manifest['icons']) != len(targets['icons'])
            or {i['actorId'] for i in manifest['icons']} != {i['actorId'] for i in targets['icons']}):
        raise ValueError('Manifest target coverage mismatch')
    files = set()
    sources = set()
    owners = set()
    frames = 0

    def check_file(relative, entry, actor, action, index):
        file = within(root, relative)
        canvas, _ = png(file)
        if canvas != entry['canvas'] or sha(file) != entry['sha256']:
            raise ValueError('Installed PNG canvas/hash mismatch: ' + relative)
        proof_path = within(root, entry['provenance'])
        proof = read(proof_path)
        norm = proof.get('normalization', {})
        if (proof.get('actorId') != actor or proof.get('action') != action or proof.get('index') != index
                or proof.get('method') != 'built-in-imagegen-independent-frame'
                or proof.get('outputSha256') != entry['sha256'] or proof.get('sourceSha256') != entry['sha256']
                or norm.get('resampling') != 'none' or norm.get('cropped') is not False
                or norm.get('spritesheetSlicing') is not False):
            raise ValueError('Invalid native provenance: ' + relative)
        if entry['sha256'] in owners:
            raise ValueError('Original reused across distinct required assets: ' + relative)
        owners.add(entry['sha256'])
        if include_sources:
            original = within(root, proof['sourceFile'])
            if sha(original) != entry['sha256']:
                raise ValueError('Original source mismatch: ' + relative)
        sources.add(proof['sourceFile'])
        files.update((relative, entry['provenance']))

    for target in targets['forms']:
        group = next(g for g in groups if g['actorId'] == target['id'])
        legacy_approved = group.get('approved') is True and group.get('anchorReviewed') is True
        waived = group.get('enabled') is True and group.get('reviewStatus') == 'waived-by-user'
        if group.get('enabled') is False or not (legacy_approved or waived) or not group.get('nativeResourcePath'):
            raise ValueError('Disabled or unauthorized native form: ' + target['id'])
        geometry(group)
        if not positive(group['calibration']['renderScale']):
            raise ValueError('Invalid render scale')
        for action, spec in target['actions'].items():
            clip = group['actions'][action]
            if spec.get('generationRequired') is False:
                if (action not in ('attack', 'hit') or clip.get('visualFallback') != 'idle' or clip.get('frames') != []
                        or clip.get('loop') is not False or not positive(clip.get('durationMs'))):
                    raise ValueError('Invalid waived combat fallback')
                continue
            if clip.get('loop') != (action in ('idle', 'walk')) or [f['name'] for f in clip['frames']] != [f'{action}_{i}' for i in indices(spec)]:
                raise ValueError('Action order/loop mismatch: ' + target['id'] + '/' + action)
            for frame in clip['frames']:
                geometry(frame)
                if not positive(frame['durationMs']):
                    raise ValueError('Invalid duration')
                relative = 'NativeActorResources/' + group['nativeResourcePath'] + '/main/' + frame['name'] + '.png'
                check_file(relative, frame, target['id'], action, int(frame['name'].rsplit('_', 1)[1]))
                frames += 1
    for entry in manifest['icons']:
        if entry.get('enabled') is False or not (entry.get('approved') or
                entry.get('enabled') is True and entry.get('reviewStatus') == 'waived-by-user'):
            raise ValueError('Disabled icon')
        check_file('GameResources/' + entry['resourcePath'] + '.png', entry, entry['actorId'], 'icon', 0)
    item_icons, spell_atlases = 0, 0
    beast_content = root / 'resources/beast-content.manifest.json'
    if beast_content.exists():
        content = read(beast_content)
        effects = read(root / 'resources/beast-spell-effects.manifest.json')
        if (len(content['icons']) != 111 or len(effects['assets']) != 48
                or len(content['recipes']) != 75 or len(content['species']) != 21):
            raise ValueError('Additional content coverage mismatch')
        for filename in ('beast-content.manifest.json', 'beast-spell-effects.manifest.json',
                         'beast-original-icons.manifest.json'):
            files.add('resources/' + filename)
        for entry in content['icons'] + effects['assets']:
            path = within(root, entry['path'])
            canvas, _ = png(path)
            expected = [entry['width'], entry['height']] if 'spell' in entry else [64, 64]
            if canvas != expected or sha(path) != entry['sha256'].lower():
                raise ValueError('Additional content PNG/hash mismatch: ' + entry['id'])
            proof = entry['provenance']
            if proof.get('tool') != 'built-in-imagegen':
                raise ValueError('Missing additional content generation provenance')
            for source in [proof] + proof.get('parts', []):
                original = within(root, source['source'])
                if include_sources and sha(original) != source['sourceSha256'].lower():
                    raise ValueError('Additional content original mismatch: ' + entry['id'])
                sources.add(source['source'])
            files.add(entry['path'])
        item_icons, spell_atlases = len(content['icons']), len(effects['assets'])
    return dict(forms=len(groups), checkedFrames=frames, icons=len(manifest['icons']),
                itemIcons=item_icons, spellStageAtlases=spell_atlases,
                resourceIntegrityPassed=True, visualReviewStatus='waived-by-user', verifiedInGame=False,
                releaseReady=False, saveTestScope='new-save-only', files=sorted(files), sources=sorted(sources))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('command', choices=('install-all', 'validate', 'stage-from-manifest'))
    args = parser.parse_args()
    if args.command == 'stage-from-manifest':
        manifest = read(ROOT / MANIFEST)
        entries = [(g['actorId'], a, f) for g in manifest['groups'] for a, c in g['actions'].items() for f in c['frames']]
        entries += [(i['actorId'], 'icon', i) for i in manifest['icons']]
        for actor, action, entry in entries:
            proof = read(within(ROOT, entry['provenance']))
            source = within(ROOT, proof['sourceFile'])
            if sha(source) != entry['sha256']:
                raise ValueError('Original hash mismatch')
            staged = WORK / ('icons/' + actor + '.png' if action == 'icon' else
                             'frames/' + actor + '/main/' + entry['name'] + '.png')
            if staged.exists() and sha(staged) != entry['sha256']:
                raise ValueError('Refuse to overwrite a different candidate: ' + str(staged))
            staged.parent.mkdir(parents=True, exist_ok=True)
            if not staged.exists():
                shutil.copyfile(source, staged)
            proof['outputFile'] = staged.relative_to(ROOT).as_posix()
            write(staged.with_suffix('.provenance.json'), proof)
        print(json.dumps(dict(staged=len(entries), reencoded=False)))
        return
    if args.command == 'install-all':
        targets = read(ROOT / 'resources/actor-redraw.targets.json')
        for target in targets['forms']:
            install_form(target['id'])
        install_icons([i['actorId'] for i in targets['icons']])
    report = validate()
    write(WORK / 'validation.json', report)
    write(WORK / 'progress.json', dict(generatedFrames=report['checkedFrames'], generatedIcons=report['icons'],
          targetFrames=report['checkedFrames'], targetIcons=report['icons'], offlineInstalledForms=report['forms'],
          releaseReady=False, resourceIntegrityPassed=True, visualReviewStatus='waived-by-user'))
    print(json.dumps({k: v for k, v in report.items() if k not in ('files', 'sources')}))


if __name__ == '__main__':
    main()
