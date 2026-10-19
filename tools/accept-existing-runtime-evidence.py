"""Bind the user's accepted test scope to actual, immutable Unity evidence."""
import csv
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
RUN = ROOT / 'output/runtime-validation/release-20261019'
RECORD = ROOT / 'resources/runtime-acceptance-0.6.0.json'
POLICY = ROOT / 'resources/0.6.0-release-policy.json'
SOURCES = {
    'native': RUN / 'native-2g-full-detail-observation.json',
    'nativeInput': RUN / 'native-input.json',
    'gpu': RUN / 'native-2g-full-detail-gpu-memory.csv',
    'sizes': RUN / 'size-normalized-observation.json',
    'comparison': RUN / 'size-normalized-observation-size-comparison.json',
    'reload': RUN / 'save-reload-age-fixed-observation.json',
    'gameplay': RUN / 'gameplay-stable-runner-observation.json',
    'baseline': RUN / 'baseline-full-detail-observation.json',
}


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write(path, data):
    path.write_text(json.dumps(data, ensure_ascii=True, indent=2) + '\n', encoding='utf-8')


def geometry_digest(manifest):
    groups = []
    for group in manifest['groups']:
        groups.append(dict(actorId=group['actorId'], resourcePath=group['resourcePath'],
            nativeResourcePath=group['nativeResourcePath'], scale=group['calibration']['renderScale'],
            actions={action: dict(loop=clip.get('loop'), visualFallback=clip.get('visualFallback'),
                frames=[{k: frame.get(k) for k in ('name', 'sha256', 'canvas', 'anchor', 'pixelsPerUnit', 'durationMs')}
                    for frame in clip.get('frames', [])]) for action, clip in group['actions'].items()}))
    icons = [{k: icon.get(k) for k in ('actorId', 'path', 'sha256')} for icon in manifest['icons']]
    return hashlib.sha256(json.dumps(dict(groups=groups, icons=icons), sort_keys=True).encode()).hexdigest()


def actual_checks(manifest):
    data = {key: read(path) for key, path in SOURCES.items() if key != 'gpu'}
    native, sizes, comparison, reload, gameplay = (data[key] for key in ('native', 'sizes', 'comparison', 'reload', 'gameplay'))
    code_hash = hashlib.sha256()
    for path in sorted((ROOT / 'code').rglob('*.cs')) + [ROOT / 'InterestingTrait.cs']:
        code_hash.update(path.relative_to(ROOT).as_posix().encode())
        code_hash.update(path.read_bytes())
    assert data['nativeInput']['productionCodeSha256'] == code_hash.hexdigest(), 'Production code differs from the tested native build'
    assert native['source'] == 'unity-runtime' and native['actualAIEnabled'] and not native['forcedVisibility']
    assert sizes['capturedUnits'] == sizes['expectedUnits'] == 69 and sizes['renderDataPipelineExecuted']
    assert comparison['sizeComparisonPassed'] and len(comparison['forms']) == 62 and all(f['passed'] for f in comparison['forms'])
    assert comparison['runtimeReportSha256'] == sha(SOURCES['sizes'])
    assert reload['status'] == 'reload-probe-completed' and reload['failed'] == 0
    assert len(reload['cases']) == 1000 and all(c['passed'] for c in reload['cases'])
    assert gameplay['status'] == 'controlled-integration-completed' and not gameplay['errors']
    assert len(gameplay['morphs']) == 21 and all(all(c[k] for k in ('animal', 'human', 'identity', 'reverted', 'healthRatioPreserved')) for c in gameplay['morphs'])
    assert len(gameplay['atlases']) == 48 and all(c['valid'] for c in gameplay['atlases'])
    assert len(gameplay['waves']) == 7 and all(c['valid'] for c in gameplay['waves'])
    assert len(gameplay['crafts']) == 3 and all(c['crafted'] for c in gameplay['crafts'])
    assert gameplay['diskSave']['saved']['actorRestored'] and gameplay['diskSave']['pendingWaveResumed']
    assert set(native['placementButtons']) == {i['actorId'] for i in manifest['icons']}
    assert all(b['fits'] and b['preserveAspect'] for b in native['placementButtons'].values())
    assert {s['units'] for s in native['scenarios']} == {100, 500, 1000}
    with SOURCES['gpu'].open(encoding='utf-8-sig', newline='') as file:
        gpu = list(csv.DictReader(file))
    scenarios = []
    for phase, units in enumerate((100, 500, 1000)):
        sample = next(s for s in native['scenarios'] if s['units'] == units)
        assert sample['durationSeconds'] >= 120 and len(set(sample['placedActorIds'])) == units
        assert sample['minimumAlive'] >= units and sample['minimumVisible'] >= units
        assert sample['renderGameplay'] and not sample['lowResolution'] and not sample['forcedRenderPipeline']
        assert sample['nativeCoverageMinimum'] == sample['nativeCoverageLast'] == 1
        assert sample['effectiveTargetFps'] == (30, 60, 144)[phase] and sample['cpuMemoryBytes'] > 0
        readings = [float(r['dedicatedGpuBytes']) for r in gpu if r.get('phase') == str(phase)
            and r.get('status') == 'observing-normal-gameplay' and r.get('dedicatedGpuBytes') and float(r['dedicatedGpuBytes']) > 0]
        assert len(readings) >= 10
        scenarios.append(dict(units=units, averageFps=sample['averageFps'], frameP95Ms=sample['frameP95Ms'],
            cpuMemoryBytes=sample['cpuMemoryBytes'], peakDedicatedGpuBytes=max(readings), gpuSamples=len(readings)))
    return dict(scenarios=scenarios, continuousRunSeconds=native['continuousRunSeconds'],
        twentyMinuteRunCompleted=native['continuousRunSeconds'] >= 1200 and native['status'] == 'completed',
        sizeForms=62, placementIcons=38, exactSaveReloadActors=1000, currentNativeCoverage=native['currentNativeCoverage'])


def accepted_existing_evidence():
    try:
        policy, record = read(POLICY), read(RECORD)
        if policy.get('runtimeAcceptance') != 'user-accepted-existing-evidence' or record.get('acceptance') != policy['runtimeAcceptance']:
            return False
        assert record['authorization'] == '2026-10-19 用户：现在的测试已经可以了'
        assert all(sha(ROOT / item['path']) == item['sha256'] for item in record['sources'].values())
        manifest = read(ROOT / 'resources/actor-frames.manifest.json')
        assert geometry_digest(manifest) == record['manifestGeometrySha256']
        assert actual_checks(manifest) == record['actualResults']
        return True
    except (OSError, ValueError, KeyError, AssertionError, TypeError):
        return False


def main():
    manifest_path = ROOT / 'resources/actor-frames.manifest.json'
    manifest = read(manifest_path)
    results = actual_checks(manifest)
    record = dict(acceptance='user-accepted-existing-evidence',
        authorization='2026-10-19 用户：现在的测试已经可以了', actualResults=results,
        manifestGeometrySha256=geometry_digest(manifest),
        sources={key: dict(path=path.relative_to(ROOT).as_posix(), sha256=sha(path)) for key,path in SOURCES.items()},
        limitations=['Further image review waived by the user.',
            'The last native run recorded approximately 19 minutes; automated 20-minute completion was not recorded.',
            'Final matched baseline was not rerun after the last fixes; earlier baseline is informational.',
            'Additional exhaustive recipe, spell and death sweeps were not rerun; existing scoped controlled checks and the 1000-actor save regression are retained.',
            'New saves only; tested on GTX 1660 Ti 6 GiB at 1920x1080.'])
    write(RECORD, record)
    policy = read(POLICY)
    policy.update(runtimeAcceptance=record['acceptance'], runtimeAcceptanceEvidence=RECORD.relative_to(ROOT).as_posix())
    write(POLICY, policy)
    for group in manifest['groups']:
        group['calibration'].update(verifiedInGame=True, measurementStatus='actual-placed-silhouette-comparison-passed',
            measurementEvidence=RECORD.relative_to(ROOT).as_posix())
    for icon in manifest['icons']:
        icon.update(verifiedInGame=True, runtimeEvidence=RECORD.relative_to(ROOT).as_posix())
    write(manifest_path, manifest)
    status_path = ROOT / 'resources/beast-overhaul-status.json'
    status = read(status_path)
    status.update(unityValidation='user-accepted-existing-evidence', performance1005001000='measured-native-three-scenarios',
        cpuGpuMemory='measured-in-actual-engine', releaseReady=False,
        formalResourceGate='user-accepted-runtime-scope-awaiting-packaging-checks')
    write(status_path, status)
    assert accepted_existing_evidence()
    print(json.dumps(dict(acceptedScope=True, **results)))


if __name__ == '__main__':
    main()
