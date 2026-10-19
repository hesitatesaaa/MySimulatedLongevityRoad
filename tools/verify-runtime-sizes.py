"""Compare actual placed-unit geometry with declared reference silhouettes."""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
REFERENCE = ROOT / 'output/runtime-validation/fantasy-reference-used/GameResources'


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def height(path):
    with Image.open(path) as image:
        box = image.convert('RGBA').getchannel('A').point(lambda a: 255 if a >= 128 else 0).getbbox()
        assert box, ('Empty silhouette', str(path))
        return box[3] - box[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--runtime-report', required=True)
    args = parser.parse_args()
    raw = Path(args.runtime_report).resolve()
    raw.relative_to(ROOT)
    data = read(raw)
    assert data['kind'] == 'worldbox-placed-unit-size-probe'
    assert data['renderDataPipelineExecuted'] is True
    assert data['capturedUnits'] == data['expectedUnits'] == 69, 'Incomplete actual placement'
    manifest_path = ROOT / 'resources/actor-frames.manifest.json'
    manifest = read(manifest_path)
    input_path = raw.parent / 'native-input.json'
    assert read(input_path)['manifestSha256'] == sha(manifest_path), 'Stale tested manifest'
    forms = []
    for group in manifest['groups']:
        own = data['samples'][group['actorId']]
        reference = data['samples'][group['calibration']['reference']]
        texture = 'mclsl_native_' + group['resourcePath'] + '/main/' + own['sprite']
        assert own['texture'] == texture and own['alive']
        frame = next(f for clip in group['actions'].values() for f in clip.get('frames', []) if f['name'] == own['sprite'])
        source = ROOT / 'NativeActorResources' / group['nativeResourcePath'] / 'main' / (own['sprite'] + '.png')
        assert sha(source) == frame['sha256']
        assert own['canvas'] == frame.get('canvas', group['canvas'])
        assert abs(own['ppu'] - frame['pixelsPerUnit']) < .0001
        assert own['filter'] == 'Point' and own['mipmaps'] == 1 and own['readable'] is False
        assert own['textureFormat'] in ('RGBA32', 'ARGB32', 'RGB24')
        reference_source = REFERENCE / reference['texturePath'] / (reference['sprite'].removeprefix('gen_') + '.png')
        measured = height(source) / own['ppu'] * abs(own['actualRenderScale'][1])
        expected = height(reference_source) / reference['ppu'] * abs(reference['actualRenderScale'][1])
        error = abs(measured / expected - 1)
        forms.append(dict(actorId=group['actorId'], reference=group['calibration']['reference'],
            measuredVisibleWorldHeight=measured, referenceVisibleWorldHeight=expected,
            relativeError=error, passed=error <= .05, nativeSha256=sha(source), referenceSha256=sha(reference_source)))
    record = dict(kind='actual-placed-unit-silhouette-comparison', source='unity-runtime',
        runtimeReport=raw.relative_to(ROOT).as_posix(), runtimeReportSha256=sha(raw),
        testedManifestSha256=sha(manifest_path), tolerance=.05, forms=forms,
        sizeComparisonPassed=all(f['passed'] for f in forms),
        limitation='Actual paused placement and native render-data geometry; alpha silhouette height only. No anatomical image review or population performance certification.')
    output = raw.with_name(raw.stem + '-size-comparison.json')
    output.write_text(json.dumps(record, ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(forms=len(forms), passed=record['sizeComparisonPassed'],
        failures=[f['actorId'] for f in forms if not f['passed']], maximumRelativeError=max(f['relativeError'] for f in forms))))
    assert record['sizeComparisonPassed'], 'Actual placed-unit scale comparison failed'


if __name__ == '__main__':
    main()
