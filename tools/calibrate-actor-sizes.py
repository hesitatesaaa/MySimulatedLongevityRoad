"""Propose render scales from actual placed-reference measurements; never mark a pass."""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
REFERENCE = ROOT / 'output/runtime-validation/fantasy-reference-used/GameResources'


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def height(path):
    with Image.open(path) as image:
        bounds = image.convert('RGBA').getchannel('A').point(lambda a: 255 if a >= 128 else 0).getbbox()
        assert bounds
        return bounds[3] - bounds[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--runtime-report', required=True)
    parser.add_argument('--apply', action='store_true')
    parser.add_argument('--normalize-render-unit', action='store_true')
    args = parser.parse_args()
    runtime_file = Path(args.runtime_report).resolve()
    runtime_file.relative_to(ROOT)
    report = read(runtime_file)
    assert report['kind'] == 'worldbox-placed-unit-size-probe' and report['renderDataPipelineExecuted'] is True
    manifest_file = ROOT / 'resources/actor-frames.manifest.json'
    manifest = read(manifest_file)
    changes = []
    for group in manifest['groups']:
        calibration = group['calibration']
        reference = report['samples'][calibration['reference']]
        reference_file = (REFERENCE / reference['texturePath'] / 'idle_0.png').resolve()
        reference_file.relative_to(REFERENCE)
        reference_height = height(reference_file) / reference['ppu'] * abs(reference['actualRenderScale'][1])
        idle = group['actions']['idle']['frames'][0]
        own = ROOT / 'NativeActorResources' / group['nativeResourcePath'] / 'main' / (idle['name'] + '.png')
        own_height = height(own) / idle['pixelsPerUnit']
        scale = reference_height / own_height
        assert 0 < scale <= 2
        if args.normalize_render_unit:
            # Keep the same authored world size while avoiding sub-0.1 actor
            # scales where a vanilla +/-0.01 size gene dominates the artwork.
            for clip in group['actions'].values():
                for frame in clip.get('frames', []):
                    previous_ppu = frame.get('pixelsPerUnit', group['pixelsPerUnit'])
                    frame['pixelsPerUnit'] = previous_ppu / scale
                    assert abs(scale / previous_ppu - 1 / frame['pixelsPerUnit']) < 1e-10
            group['pixelsPerUnit'] /= scale
            calibration['renderUnitNormalization'] = dict(previousScale=scale,
                pixelsPerUnitMultiplier=1 / scale, sourcePixelsChanged=False)
            scale = 1
        changes.append(dict(actorId=group['actorId'], previousScale=calibration['renderScale'], proposedScale=scale,
            reference=calibration['reference'], referenceVisibleWorldHeight=reference_height,
            nativeIdleSha256=idle['sha256'], referenceSha256=hashlib.sha256(reference_file.read_bytes()).hexdigest()))
        calibration.update(renderScale=scale, verifiedInGame=False, previewScaleApproved=True,
            measurementStatus='runtime-reference-silhouette-match-awaiting-final-Unity-retest',
            measurementEvidence='resources/actor-runtime-reviews/calibration-20261019.json')
    if args.apply:
        evidence = dict(kind='scale-proposal-from-actual-placed-reference-units', sizeComparisonPassed=False,
            runtimeReport=runtime_file.relative_to(ROOT).as_posix(), runtimeReportSha256=hashlib.sha256(runtime_file.read_bytes()).hexdigest(),
            limitation='Matches visible alpha-silhouette height; no manual anatomical landmark or art review. Final placed-unit retest remains required.', forms=changes)
        (ROOT / 'resources/actor-runtime-reviews/calibration-20261019.json').write_text(json.dumps(evidence, ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
        manifest_file.write_text(json.dumps(manifest, ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(applied=args.apply, forms=len(changes), minimumScale=min(x['proposedScale'] for x in changes), maximumScale=max(x['proposedScale'] for x in changes))))


if __name__ == '__main__':
    main()
