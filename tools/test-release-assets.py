"""Small real-file regression checks for the review-waived release boundary."""
import copy
import tempfile
from pathlib import Path
from PIL import Image
from release_assets import read, sha, validate, write


with tempfile.TemporaryDirectory(prefix='mclsl-release-') as temporary:
    root = Path(temporary)
    actions, specs = {}, {}
    group = dict(actorId='test', resourcePath='actors/test', nativeResourcePath='actors/test',
                 enabled=True, reviewStatus='waived-by-user', approved=False, anchorReviewed=False,
                 canvas=[4, 4], anchor=[2, 3], pixelsPerUnit=1, calibration=dict(renderScale=.3, verifiedInGame=False))
    for number, action in enumerate(('idle', 'walk', 'death', 'icon')):
        relative = ('GameResources/ui/test.png' if action == 'icon' else
                    f'NativeActorResources/actors/test/main/{action}_0.png')
        path = root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        image = Image.new('RGBA', (4, 4))
        image.putpixel((1, 1), (20 + number, 80, 90, 255))
        image.save(path)
        source = f'originals/{action}.png'
        (root / source).parent.mkdir(exist_ok=True)
        (root / source).write_bytes(path.read_bytes())
        provenance = f'resources/provenance-{action}.json'
        write(root / provenance, dict(actorId='test', action=action, index=0,
              method='built-in-imagegen-independent-frame', sourceFile=source,
              sourceSha256=sha(path), outputSha256=sha(path), reviewStatus='waived-by-user',
              normalization=dict(resampling='none', cropped=False, spritesheetSlicing=False)))
        entry = dict(name=f'{action}_0', durationMs=125, canvas=[4, 4], anchor=[2, 3], pixelsPerUnit=1,
                     sha256=sha(path), provenance=provenance, anchorReviewed=False)
        if action == 'icon':
            icon = dict(entry, actorId='test', resourcePath='ui/test', enabled=True, reviewStatus='waived-by-user')
        else:
            actions[action] = dict(loop=action in ('idle', 'walk'), frames=[entry])
            specs[action] = dict(count=1, durationMs=125)
    for action in ('attack', 'hit'):
        specs[action] = dict(count=0, generationRequired=False, fallbackDurationMs=125)
        actions[action] = dict(loop=False, frames=[], visualFallback='idle', durationMs=125)
    group['actions'] = actions
    target = dict(id='test', actions=specs)
    write(root / 'resources/actor-redraw.targets.json', dict(forms=[target], icons=[dict(actorId='test')]))
    manifest = dict(groups=[group], icons=[icon])
    manifest_path = root / 'resources/actor-frames.manifest.json'
    write(manifest_path, manifest)
    result = validate(root)
    assert result['checkedFrames'] == 3 and result['resourceIntegrityPassed'] is True
    assert result['releaseReady'] is False and result['verifiedInGame'] is False
    assert not read(manifest_path)['groups'][0]['anchorReviewed']
    checks = 3
    for mutate in (
        lambda d: d['groups'][0].update(enabled=False),
        lambda d: d['groups'][0]['actions']['idle']['frames'][0].update(sha256='0' * 64),
        lambda d: d['groups'][0]['actions']['idle']['frames'][0].update(pixelsPerUnit=0),
        lambda d: d['groups'][0]['actions']['attack'].update(visualFallback='walk'),
        lambda d: d['groups'][0]['actions']['idle']['frames'][0].update(provenance='../outside.json'),
    ):
        modified = copy.deepcopy(manifest)
        mutate(modified)
        write(manifest_path, modified)
        try:
            validate(root)
        except (ValueError, FileNotFoundError):
            checks += 1
        else:
            raise AssertionError('Invalid release boundary accepted')
    write(manifest_path, manifest)
    (root / 'NativeActorResources/actors/test/main/idle_0.png').unlink()
    try:
        validate(root)
    except FileNotFoundError:
        checks += 1
    else:
        raise AssertionError('Missing PNG accepted')
    print(f'PASS: {checks} release boundary assertions')
