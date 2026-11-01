"""One-time, full-canvas resize of the accepted production frames (requires Pillow)."""
import argparse
import copy
import hashlib
import io
import json
import re
import statistics
import zipfile
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / 'resources/actor-frames.manifest.json'


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def write(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=True, indent=2) + '\n', encoding='utf-8')


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def resize(image, size):
    # Premultiplied alpha prevents transparent RGB from bleeding into edges.
    return image.convert('RGBA').convert('RGBa').resize(size, Image.Resampling.LANCZOS).convert('RGBA')


def references(path):
    with zipfile.ZipFile(path) as archive:
        code = archive.read('Thefantasyworld/code/FantasyCreatures/FantasyCreatures.cs').decode('utf-8-sig')
        classes = re.findall(r'\{\s*"([^"]+)"\s*,\s*\(CreatureTier\.(\w+),', code)
        factors = {name: float(value) for name, value in
                   re.findall(r'\{\s*"([^"]+)"\s*,\s*([.\d]+)f\s*\}', code)}
        paths = {item.filename.split('/')[-3]: item.filename for item in archive.infolist()
                 if '/FantasyCreatures/' in item.filename and item.filename.endswith('/Main/idle_0.png')}
        entries = {}
        for name in set(name for name, _ in classes) | {'SpiritTraveler'}:
            with Image.open(io.BytesIO(archive.read(paths[name]))) as source:
                alpha = source.convert('RGBA').getchannel('A')
                bounds = alpha.point(lambda value: 255 if value >= 16 else 0).getbbox()
                entries[name] = dict(name=name, canvas=list(source.size), bounds=list(bounds),
                    pixelsPerUnit=1, scale=0.1 * factors.get(name, 1),
                    visibleWorldHeight=(bounds[3] - bounds[1]) * 0.1 * factors.get(name, 1),
                    pngSha256=hashlib.sha256(archive.read(paths[name])).hexdigest())
        tiers = {}
        for index, tier in enumerate(('Tier1', 'Tier2', 'Tier3', 'Tier4', 'Legendary'), 1):
            members = [entries[name] for name, kind in classes if kind == tier]
            middle = statistics.median(entry['visibleWorldHeight'] for entry in members)
            tiers[str(index)] = dict(visibleWorldHeight=middle, sampleCount=len(members),
                members=[entry['name'] for entry in members],
                nearestReference=min(members, key=lambda entry: (abs(entry['visibleWorldHeight'] - middle), entry['name'])))
        return dict(archiveSha256=sha(path), tiers=tiers, human=entries['SpiritTraveler'],
                    named=entries['PrimordialFrostDragon'],
                    scaleBasis='Reference $mob$ scale 0.1, sprite PPU 1; corroborated by historical placed reference units.')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--reference-zip', type=Path, required=True)
    args = parser.parse_args()
    manifest = read(MANIFEST)
    assert not manifest.get('assetProfile'), 'Already resized; never resample installed frames a second time'
    assert len(manifest['groups']) == 62 and len(manifest['icons']) == 38
    reference = references(args.reference_zip)
    levels = {name: int(level) + 1 for name, level in re.findall(r'new\("([a-z]+)",(\d),',
        (ROOT / 'code/MySimulatedLongevityRoad/Data/MclslBeastContent.cs').read_text(encoding='utf-8-sig'))}
    assert len(levels) == 21
    stage = ROOT / 'build/current-asset-resize-stage'
    assert not stage.exists(), 'Remove an incomplete stage only after inspecting it'
    staged = []
    groups = []
    count, before, after = 0, 0, 0
    for group in manifest['groups']:
        resource = group['resourcePath']
        named = {'qingshenlong': 400, 'disanmo': 320, 'diyi': 256}
        species = group['actorId'].removeprefix('mclsl_beast_').removeprefix('mclsl_named_')
        beast = resource.endswith('/Beast')
        if not beast:
            edge, target, label = 256, reference['human']['visibleWorldHeight'], 'SpiritTraveler'
        elif species in named:
            edge = named[species]
            target, label = reference['named']['visibleWorldHeight'] * edge / 400, 'PrimordialFrostDragon'
        else:
            level = levels[species]
            edge = (128, 160, 224, 320, 400)[level - 1]
            target, label = reference['tiers'][str(level)]['visibleWorldHeight'], 'western-tier-' + str(level) + '-median'
        frames = [(action, frame) for action, clip in group['actions'].items() for frame in clip.get('frames', [])]
        source_edge = max(max(frame.get('canvas', group['canvas'])) for _, frame in frames)
        ratio = edge / source_edge
        assert 0 < ratio < 1
        idle = group['actions']['idle']['frames'][0]
        idle_file = ROOT / 'NativeActorResources' / group['nativeResourcePath'] / 'main' / (idle['name'] + '.png')
        idle_size = [max(1, round(value * ratio)) for value in idle['canvas']]
        with Image.open(idle_file) as image:
            bounds = resize(image, tuple(idle_size)).getchannel('A').point(lambda value: 255 if value >= 16 else 0).getbbox()
        # Measure the result: tiny alpha values can round to zero during resampling.
        ppu_factor = ((bounds[3] - bounds[1]) * group['calibration']['renderScale'] / target
                      / (idle['pixelsPerUnit'] * idle_size[1] / idle['canvas'][1]))
        for action, frame in frames:
            file = ROOT / 'NativeActorResources' / group['nativeResourcePath'] / 'main' / (frame['name'] + '.png')
            assert sha(file) == frame['sha256'], 'Production frame changed before resize: ' + str(file)
            original_hash = frame['sha256']
            old_canvas, old_anchor, old_ppu = frame['canvas'][:], frame['anchor'][:], frame['pixelsPerUnit']
            size = [max(1, round(dimension * ratio)) for dimension in old_canvas]
            destination = stage / file.relative_to(ROOT)
            destination.parent.mkdir(parents=True, exist_ok=True)
            with Image.open(file) as image:
                assert list(image.size) == old_canvas
                result = resize(image, tuple(size))
                result.save(destination, format='PNG', compress_level=9)
            x_ratio, y_ratio = size[0] / old_canvas[0], size[1] / old_canvas[1]
            frame.update(canvas=size, anchor=[old_anchor[0] * x_ratio, old_anchor[1] * y_ratio],
                pixelsPerUnit=old_ppu * y_ratio * ppu_factor, sha256=sha(destination))
            proof_file = ROOT / frame['provenance']
            proof = read(proof_file)
            # Preserve the latest repair's actual prompt before deleting candidate directories.
            candidate = proof.get('repair', {}).get('candidateProvenance')
            if candidate and (ROOT / candidate).is_file():
                details = read(ROOT / candidate)
                proof['repair']['generationRecord'] = {key: details[key] for key in
                    ('prompt', 'sourceSha256', 'createdAtUtc', 'method') if key in details}
            for key in ('sourceFile', 'pendingNativeRuntimeIntegration', 'staticReview', 'anatomyReview', 'integrationGeometry'):
                proof.pop(key, None)
            proof['sourceRetention'] = 'hash-and-prompt-only; original/rollback copies removed by user request'
            proof['historicalReview'] = proof.pop('review', {})
            proof['normalization'] = dict(resampling='lanczos-premultiplied-alpha', sourceCanvas=old_canvas,
                canvas=size, scale=ratio, inputSha256=original_hash, sourceAnchor=old_anchor,
                sourcePixelsPerUnit=old_ppu, ppuFactor=ppu_factor, sourceRenderScale=group['calibration']['renderScale'],
                cropped=False, spritesheetSlicing=False, colorQuantized=False, resizePasses=1, visibleBoundsAlpha=16)
            proof.update(outputSha256=frame['sha256'], nativeResolution=False,
                integrationStatus='local-resized-build-awaiting-gameplay-observation')
            proof_stage = stage / proof_file.relative_to(ROOT)
            write(proof_stage, proof)
            staged.extend(((destination, file), (proof_stage, proof_file)))
            count += 1
            before += file.stat().st_size
            after += destination.stat().st_size
        group.update(canvas=idle['canvas'][:], anchor=idle['anchor'][:], pixelsPerUnit=idle['pixelsPerUnit'])
        group['calibration'] = dict(renderScale=group['calibration']['renderScale'], reference=label,
            targetVisibleWorldHeight=target, verifiedInGame=False, previewScaleApproved=True,
            measurementStatus='calculated-from-reference; not-retested-in-game',
            measurementEvidence='resources/actor-size-policy.json')
        groups.append(dict(actorId=group['actorId'], longEdge=edge, reference=label,
            targetVisibleWorldHeight=target, inputLongEdge=source_edge, resizeScale=ratio,
            frameOrder={action: [f['name'] for f in clip.get('frames', [])] for action, clip in group['actions'].items()}))
    assert count == 1374
    # The entire replacement is decoded and staged before the first production file is replaced.
    for source, destination in staged:
        source.replace(destination)
    for icon in manifest['icons']:
        proof_file = ROOT / icon['provenance']
        proof = read(proof_file)
        proof.pop('sourceFile', None)
        proof['sourceRetention'] = 'hash-and-prompt-only; installed icon unchanged'
        write(proof_file, proof)
    manifest.update(assetProfile='compact-current-art-20261019', status='local-development-not-Unity-retested')
    write(MANIFEST, manifest)
    write(ROOT / 'resources/actor-size-policy.json', dict(kind='user-approved-fixed-species-size-policy',
        noCropping=True, noColorQuantization=True, resampling='lanczos-premultiplied-alpha', visibleBoundsAlpha=16,
        unityRetested=False, references=reference, forms=groups))
    print(json.dumps(dict(frames=count, originalBytes=before, resizedBytes=after, forms=len(groups))))


if __name__ == '__main__':
    main()
