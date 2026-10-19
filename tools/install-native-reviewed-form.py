"""Install one fully reviewed form as native PNG frames for development previews."""
import argparse
import copy
import hashlib
import json
import math
import shutil
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'output' / 'actor-redraw'
NATIVE = ROOT / 'NativeActorResources'
MANIFEST = ROOT / 'resources' / 'actor-frames.manifest.json'

def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--actor', required=True)
    parser.add_argument('--skip-review', action='store_true', help='Install with the explicit user review waiver; never mark review passed')
    args = parser.parse_args()
    if args.skip_review:
        from release_assets import install_form
        install_form(args.actor)
        return
    targets = read(ROOT / 'resources' / 'actor-redraw.targets.json')
    target = next(f for f in targets['forms'] if f['id'] == args.actor)
    review_path = ROOT / 'resources' / 'actor-native-reviews' / (args.actor + '.json')
    review = read(review_path)
    if not (review.get('sourceFilesComplete') and review.get('geometryApproved')
            and review.get('actionContinuityApproved')):
        raise ValueError('Full geometry and action review required before native installation')
    if any(f.get('severity') == 'needs-redraw' for f in review.get('visualReviewFindings', [])):
        raise ValueError('Form still has rejected frames')
    manifest = read(MANIFEST)
    group = next(g for g in manifest['groups'] if g['actorId'] == args.actor)
    if group.get('nativeResourcePath') or group.get('approved'):
        raise ValueError('Refuse to overwrite an already installed form')
    installed = copy.deepcopy(group)
    installed['nativeResourcePath'] = target['resourcePath']
    idle = review['actions']['idle']['frames'][0]['proposedGeometry']
    installed['canvas'] = idle['canvas']
    installed['anchor'] = idle['anchor']
    installed['pixelsPerUnit'] = idle['pixelsPerUnit']
    installed['approved'] = True
    installed['anchorReviewed'] = True
    installed['calibration']['verifiedInGame'] = False
    # Offline previews can inherit a measured reference size without claiming
    # that this newly installed form has already passed the Unity acceptance.
    preview = review.get('previewCalibration')
    if preview is not None:
        scale = preview.get('renderScale')
        if (not isinstance(scale, (int, float)) or not math.isfinite(scale) or scale <= 0
                or preview.get('verifiedInGame') is not False
                or preview.get('previewScaleApproved') is not True
                or preview.get('reference') != installed['calibration'].get('reference')):
            raise ValueError('Invalid reviewed preview size calibration')
        installed['calibration'].update(copy.deepcopy(preview))
    operations = []
    for action, spec in target['actions'].items():
        if spec.get('generationRequired') is False:
            installed['actions'][action] = dict(loop=False, frames=[], visualFallback='idle', durationMs=spec['fallbackDurationMs'])
            continue
        frames = review['actions'][action]['frames']
        indices = spec.get('frameOrder', spec.get('indices', list(range(spec['count']))))
        if [f['name'] for f in frames] != [f'{action}_{i}' for i in indices]:
            raise ValueError('Review differs from action target: ' + action)
        duration = spec['durationMs']
        native_entries = []
        for frame in frames:
            name = frame['name']
            source = (ROOT / frame['sourceFile']).resolve()
            source.relative_to(ROOT.resolve())
            staged = WORK / 'frames' / args.actor / 'main' / (name + '.png')
            proof = read(staged.with_suffix('.provenance.json'))
            if sha(source) != frame['sourceSha256'] or sha(source) != proof['sourceSha256']:
                raise ValueError('Original source hash mismatch: ' + name)
            with Image.open(source) as image:
                image.verify()
            anatomy = frame.get('anatomyReview', {})
            if (anatomy.get('sourceSha256') != frame['sourceSha256']
                    or anatomy.get('limbConnectionsReviewed') is not True
                    or anatomy.get('extraLimbs') is not False):
                raise ValueError('Manual source-bound anatomy review required: ' + name)
            geometry = frame['proposedGeometry']
            if not geometry.get('anchorReviewed') or frame['sourceTouchesEdge']:
                raise ValueError('Unreviewed/clipped frame: ' + name)
            destination = NATIVE / target['resourcePath'] / 'main' / (name + '.png')
            provenance = ROOT / 'resources' / 'actor-provenance' / args.actor / (name + '.json')
            if destination.exists() or provenance.exists():
                raise FileExistsError('Refuse to overwrite native frame or provenance: ' + name)
            native_proof = copy.deepcopy(proof)
            native_proof['outputFile'] = destination.relative_to(ROOT).as_posix()
            native_proof['outputSha256'] = frame['sourceSha256']
            native_proof['nativeResolution'] = True
            native_proof['pendingNativeRuntimeIntegration'] = False
            native_proof['normalization'] = dict(resampling='none', sourceCanvas=frame['canvas'],
                                                  canvas=frame['canvas'], scale=1, cropped=False,
                                                  spritesheetSlicing=False,
                                                  byteIdenticalToGeneratedSource=True)
            native_proof['review'] = dict(identity=True, pixelEdges=True, anchor=True, continuity=True)
            operations.append((source, destination, provenance, native_proof))
            native_entries.append(dict(name=name, durationMs=duration, sha256=frame['sourceSha256'],
                                       origin='independently-redrawn',
                                       provenance=provenance.relative_to(ROOT).as_posix(),
                                       canvas=geometry['canvas'], anchor=geometry['anchor'],
                                       pixelsPerUnit=geometry['pixelsPerUnit'], anchorReviewed=True))
        installed['actions'][action] = dict(loop=action in ('idle', 'walk'), frames=native_entries)
    for source, destination, provenance, proof in operations:
        destination.parent.mkdir(parents=True, exist_ok=True)
        provenance.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, destination)
        if sha(destination) != proof['sourceSha256']:
            raise ValueError('Native copy hash mismatch: ' + str(destination))
        provenance.write_text(json.dumps(proof, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    manifest['groups'][manifest['groups'].index(group)] = installed
    MANIFEST.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    review['installed'] = True
    review['installation'] = dict(nativeResourcePath=target['resourcePath'],
                                  frameCount=len(operations), gameSizeVerified=False,
                                  gameMemoryVerified=False)
    review_path.write_text(json.dumps(review, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'actor': args.actor, 'nativeFrames': len(operations),
                      'gameSizeVerified': False, 'gameMemoryVerified': False}))

if __name__ == '__main__':
    main()
