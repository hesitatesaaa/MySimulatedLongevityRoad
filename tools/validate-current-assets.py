"""Validate compact resources without claiming new Unity acceptance (Pillow required)."""
import argparse
import hashlib
import json
import math
import re
import importlib.util
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def within(relative):
    path = (ROOT / relative).resolve()
    path.relative_to(ROOT)
    return path


def validate():
    spec = importlib.util.spec_from_file_location('nml_sources', ROOT / 'tools/check-nml-sources.py')
    scanner = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(scanner)
    scanner.validate(ROOT)
    manifest = read(ROOT / 'resources/actor-frames.manifest.json')
    policy = read(ROOT / 'resources/actor-size-policy.json')
    assert manifest['assetProfile'] == 'compact-current-art-20261019'
    assert len(manifest['groups']) == len(policy['forms']) == 62 and len(manifest['icons']) == 38
    assert policy['unityRetested'] is False and policy['noCropping'] and policy['noColorQuantization']
    specs = {entry['actorId']: entry for entry in policy['forms']}
    assert len(specs) == 62 and {g['actorId'] for g in manifest['groups']} == set(specs)
    expected, proofs, source_hashes = set(), set(), set()
    count = repairs = 0
    for group in manifest['groups']:
        spec = specs[group['actorId']]
        assert group['calibration']['verifiedInGame'] is False and group['calibration']['previewScaleApproved']
        assert group['nativeResourcePath'] == group['resourcePath']
        assert set(group['actions']) == {'idle', 'walk', 'attack', 'hit', 'death'}
        maximum = 0
        for action, clip in group['actions'].items():
            entries = clip.get('frames', [])
            assert [f['name'] for f in entries] == spec['frameOrder'][action]
            if 'visualFallback' in clip:
                assert action in ('attack', 'hit') and clip['visualFallback'] == 'idle' and not entries
                assert clip['loop'] is False and clip['durationMs'] > 0
                continue
            assert entries and clip['loop'] == (action in ('idle', 'walk'))
            assert len({f['name'] for f in entries}) == len(entries)
            for frame in entries:
                assert re.fullmatch(action + r'_\d+', frame['name'])
                file = within('NativeActorResources/' + group['nativeResourcePath'] + '/main/' + frame['name'] + '.png')
                proof_file = within(frame['provenance'])
                proof = read(proof_file)
                transform = proof['normalization']
                assert file not in expected and proof_file not in proofs
                expected.add(file)
                proofs.add(proof_file)
                assert sha(file) == frame['sha256'] == proof['outputSha256'], str(file)
                assert within(proof['outputFile']) == file
                assert proof['method'] == 'built-in-imagegen-independent-frame' and proof['prompt']
                assert proof['actorId'] == group['actorId'] and proof['action'] == action
                assert proof['index'] == int(frame['name'].rsplit('_', 1)[1])
                assert re.fullmatch('[a-f0-9]{64}', proof['sourceSha256']) and proof['sourceSha256'] not in source_hashes
                source_hashes.add(proof['sourceSha256'])
                assert 'sourceFile' not in proof and proof['sourceRetention']
                assert transform['resampling'] == 'lanczos-premultiplied-alpha' and transform['resizePasses'] == 1
                assert transform['cropped'] is False and transform['spritesheetSlicing'] is False
                assert transform['colorQuantized'] is False and re.fullmatch('[a-f0-9]{64}', transform['inputSha256'])
                assert transform['scale'] == spec['resizeScale'] < 1
                with Image.open(file) as image:
                    image.load()
                    assert image.format == 'PNG' and image.mode == 'RGBA'
                    assert list(image.size) == frame['canvas'] == transform['canvas']
                    alpha = image.getchannel('A')
                    bounds = alpha.point(lambda value: 255 if value >= policy['visibleBoundsAlpha'] else 0).getbbox()
                    assert alpha.getextrema()[0] == 0 and bounds
                    maximum = max(maximum, *image.size)
                old_canvas = transform['sourceCanvas']
                assert frame['canvas'] == [max(1, round(v * transform['scale'])) for v in old_canvas]
                ratios = [frame['canvas'][i] / old_canvas[i] for i in (0, 1)]
                for axis in (0, 1):
                    assert math.isclose(frame['anchor'][axis], transform['sourceAnchor'][axis] * ratios[axis], abs_tol=1e-7)
                    assert 0 <= frame['anchor'][axis] <= frame['canvas'][axis]
                assert math.isclose(frame['pixelsPerUnit'], transform['sourcePixelsPerUnit'] * ratios[1] * transform['ppuFactor'], rel_tol=1e-9)
                assert math.isfinite(frame['pixelsPerUnit']) and frame['pixelsPerUnit'] > 0
                assert math.isfinite(frame['durationMs']) and frame['durationMs'] > 0
                if frame is group['actions']['idle']['frames'][0]:
                    height = (bounds[3] - bounds[1]) / frame['pixelsPerUnit'] * group['calibration']['renderScale']
                    assert abs(height - spec['targetVisibleWorldHeight']) <= max(spec['targetVisibleWorldHeight'] * .06, 6 / frame['pixelsPerUnit']), group['actorId']
                count += 1
                repairs += bool(proof.get('repair'))
        assert maximum == spec['longEdge'], group['actorId']
    assert count == 1374 and repairs == 18
    assert {p.resolve() for p in (ROOT / 'NativeActorResources').rglob('*.png')} == expected
    for folder in ('Beasts', 'Named', 'Immortals'):
        assert not (ROOT / 'GameResources/actors' / folder).exists(), 'Legacy actor directory remains: ' + folder
    icons = set()
    for icon in manifest['icons']:
        file = within('GameResources/' + icon['resourcePath'] + '.png')
        assert file not in icons
        icons.add(file)
        proof_file = within(icon['provenance'])
        proof = read(proof_file)
        proofs.add(proof_file)
        assert sha(file) == icon['sha256'] == proof['outputSha256']
        assert proof['actorId'] == icon['actorId'] and proof['action'] == 'icon' and proof['prompt']
        assert 'sourceFile' not in proof
        with Image.open(file) as image:
            image.load()
            assert image.format == 'PNG' and 'A' in image.getbands() and list(image.size) == icon['canvas']
            assert image.getchannel('A').getbbox()
    assert {p.resolve() for p in (ROOT / 'GameResources/ui/ActorIcons').glob('*.png')} == icons
    assert {p.resolve() for p in (ROOT / 'resources/actor-provenance').rglob('*') if p.is_file()} == proofs
    return dict(forms=62, animationFrames=count, placementIcons=len(icons), retainedRepairFrames=repairs,
                currentArtworkOnly=True, unityRetested=False)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--release', action='store_true')
    args = parser.parse_args()
    print(json.dumps(validate()))
    if args.release:
        raise SystemExit('Formal release blocked: compact art/geometry has not been revalidated in Unity. Use the explicit local development build.')
