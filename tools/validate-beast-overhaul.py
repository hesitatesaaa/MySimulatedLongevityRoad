"""Validate resource coverage. Never infer Unity or performance acceptance."""
import argparse
import json
import hashlib
import importlib.util
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('inspection', ROOT / 'tools/beast-resource-inspection.py')
inspection = importlib.util.module_from_spec(spec)
spec.loader.exec_module(inspection)
parser = argparse.ArgumentParser()
parser.add_argument('--release', action='store_true')
args = parser.parse_args()
manifest = json.loads((ROOT / 'resources/beast-content.manifest.json').read_text(encoding='utf-8-sig'))
effects = json.loads((ROOT / 'resources/beast-spell-effects.manifest.json').read_text(encoding='utf-8-sig'))
assert len(manifest['icons']) == 111
assert len(manifest['recipes']) == 75
assert len(manifest['species']) == 21
assert len(effects['assets']) == 48
assert len({x['spell'] for x in effects['assets']}) == 16
assert all({x['phase'] for x in effects['assets'] if x['spell'] == spell} == {'cast', 'travel', 'hit'}
           for spell in {x['spell'] for x in effects['assets']})
paths = [x['path'] for x in manifest['icons'] + effects['assets']]
assert len(paths) == len(set(paths))
missing, invalid, unreviewed = [], [], []
pixel_hashes = defaultdict(list)
invalid_reasons = {}
for item in manifest['icons'] + effects['assets']:
    if item.get('status') != 'approved':
        unreviewed.append(item['path'])
    path = ROOT / item['path']
    if not path.exists():
        missing.append(item['path'])
        continue
    raw = path.read_bytes()
    expected = (64, 64) if item in manifest['icons'] else (item['width'], item['height'])
    try:
        digest = inspection.inspect_png(path, expected, item.get('frames'),
                                        item.get('frameSize', 192), item.get('columns', 4))
        pixel_hashes[(expected, digest)].append(item['path'])
    except (ValueError, OSError, SyntaxError) as error:
        invalid.append(item['path'])
        invalid_reasons[item['path']] = str(error)
    if item.get('status') == 'approved' and item.get('sha256', '').lower() != hashlib.sha256(raw).hexdigest():
        invalid.append(item['path'])
        invalid_reasons[item['path']] = 'Approved file hash differs from its review record'
duplicates = [group for group in pixel_hashes.values() if len(group) > 1]
for group in duplicates:
    for path in group:
        invalid.append(path)
        invalid_reasons[path] = 'Independent assets have identical visible pixels'
invalid = sorted(set(invalid))
status = json.loads((ROOT / 'resources/beast-overhaul-status.json').read_text(encoding='utf-8-sig'))
original_review_path = ROOT / 'resources/beast-original-icon-review.json'
original_review_valid = False
if original_review_path.exists():
    original = json.loads(original_review_path.read_text(encoding='utf-8-sig'))
    expected_original = json.loads((ROOT / 'resources/beast-original-icons.manifest.json').read_text(encoding='utf-8-sig'))
    original_review_valid = original.get('kind') == 'offline-existing-item-icon-review' \
        and original.get('visualReview') == 'passed' \
        and {x['id'] for x in original['items']} == {x['id'] for x in expected_original}
    for item in original['items']:
        path = ROOT / item['path']
        original_review_valid = original_review_valid and path.exists() \
            and hashlib.sha256(path.read_bytes()).hexdigest() == item['sha256']
evidence_path = ROOT / 'output/beast-overhaul/unity-validation.json'
evidence_valid = False
if evidence_path.exists():
    evidence = json.loads(evidence_path.read_text(encoding='utf-8-sig'))
    scenarios = evidence.get('scenarios', [])
    expected_units = {100, 500, 1000}
    evidence_valid = evidence.get('source') == 'unity-runtime' and bool(evidence.get('unityVersion')) \
        and evidence.get('continuousRunSeconds', 0) >= 1200 \
        and {x.get('units') for x in scenarios} == expected_units \
        and len(evidence.get('placedOrdinarySpeciesIds', [])) == 21 \
        and evidence.get('gameplayCasesPassed') is True
    for sample in scenarios:
        evidence_valid = evidence_valid and sample.get('durationSeconds', 0) >= 120 \
            and len(set(sample.get('placedActorIds', []))) >= sample.get('units', 0) \
            and sample.get('cpuMemoryBytes', 0) > 0 and sample.get('gpuMemoryBytes', 0) > 0 \
            and sample.get('baselineAverageFps', 0) > 0 \
            and sample.get('averageFps', 0) >= sample.get('baselineAverageFps', 0) * .9 \
            and sample.get('baselineFrameP95Ms', 0) > 0 \
            and sample.get('frameP95Ms', float('inf')) <= sample.get('baselineFrameP95Ms', 0) * 1.2
report = {'kind': 'offline-beast-resource-validation', 'missing': missing, 'invalid': invalid, 'unreviewed': unreviewed,
          'invalidReasons': invalid_reasons, 'duplicatePixelGroups': duplicates,
          'unityValidation': status['unityValidation'], 'performance1005001000': status['performance1005001000'],
          'cpuGpuMemory': status['cpuGpuMemory'], 'actualUnityEvidenceValid': evidence_valid,
          'originalIconReviewValid': original_review_valid,
          'releaseReady': not missing and not invalid and not unreviewed and evidence_valid and original_review_valid and status.get('oldItemArtAudit') == 'passed'
          and all(status[k] == 'passed' for k in ('unityValidation', 'performance1005001000', 'cpuGpuMemory'))}
output = ROOT / 'output/beast-overhaul/resource-validation.json'
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps(report, indent=2), encoding='utf-8')
print(json.dumps({'icons':111,'attackSpellStages':len(effects['assets']),'missing':len(missing),
                  'invalid':len(invalid),'releaseReady':report['releaseReady']}))
if invalid or args.release and not report['releaseReady']:
    raise SystemExit(1)
