"""Verify matched real-engine population measurements and external GPU counters."""
import argparse
import csv
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
RUN = ROOT / 'output/runtime-validation/release-20261019'


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--native', required=True)
    parser.add_argument('--baseline', required=True)
    args = parser.parse_args()
    assert all(re.fullmatch('[a-z0-9-]+', name) for name in (args.native, args.baseline))
    native_file = RUN / (args.native + '-observation.json')
    baseline_file = RUN / (args.baseline + '-observation.json')
    native, baseline = read(native_file), read(baseline_file)
    inputs = [read(RUN / (mode + '-input.json')) for mode in ('native', 'baseline')]
    checks = dict(completed=all(r['status'] == 'completed' for r in (native, baseline)),
        sameCode=inputs[0]['productionCodeSha256'] == inputs[1]['productionCodeSha256'],
        sameMaps=native['fixtureSaveHashes'] == baseline['fixtureSaveHashes'] and len(native['fixtureSaveHashes']) == 3,
        sameResolution=native['renderResolution'] == baseline['renderResolution'],
        sameDevice=native['graphicsDevice'] == baseline['graphicsDevice'],
        duration=native['continuousRunSeconds'] >= 1200,
        newSave=native['newSaveCompatibilityPassed'] and baseline['newSaveCompatibilityPassed'],
        nativeManifest=native['manifestSha256'] == sha(ROOT / 'resources/actor-frames.manifest.json'),
        species=len(set(native['placedOrdinarySpeciesIds'])) == 21,
        scopes=all(r['actualAIEnabled'] and not r['forcedVisibility'] and r['saveTestScope'] == 'new-save-only' for r in (native, baseline)))
    hardware = (RUN / 'gpu-hardware.txt').read_text(encoding='utf-8-sig')
    capacity = int(re.search(r'(\d+) MiB', hardware).group(1)) * 1024 * 1024
    gpu_rows = {}
    source_paths = [native_file, baseline_file, RUN / 'native-input.json', RUN / 'baseline-input.json', RUN / 'gpu-hardware.txt']
    for label, name in (('native', args.native), ('baseline', args.baseline)):
        path = RUN / (name + '-gpu-memory.csv')
        source_paths.append(path)
        with path.open(encoding='utf-8-sig', newline='') as file:
            gpu_rows[label] = list(csv.DictReader(file))
    scenarios = []
    for phase, units in enumerate((100, 500, 1000)):
        n = next(s for s in native['scenarios'] if s['units'] == units)
        b = next(s for s in baseline['scenarios'] if s['units'] == units)
        row = dict(n, baselineAverageFps=b['averageFps'], baselineFrameP95Ms=b['frameP95Ms'])
        local = dict(duration=n['durationSeconds'] >= 120 and b['durationSeconds'] >= 120,
            placement=all(len(set(s['placedActorIds'])) >= units and s['minimumAlive'] >= units and s['minimumVisible'] >= units for s in (n, b)),
            normalRendering=all(s['renderGameplay'] and not s['lowResolution'] and not s['forcedVisibility'] and not s['forcedRenderPipeline'] and not s['gamePaused'] for s in (n, b)),
            targetFps=n['effectiveTargetFps'] == b['effectiveTargetFps'] == (30, 60, 144)[phase],
            warmup=n['warmupSeconds'] == b['warmupSeconds'],
            nativeCoverage=n['nativeCoverageMinimum'] >= .99 and n['nativeCoverageLast'] == 1,
            fps=n['averageFps'] >= b['averageFps'] * .9,
            frameP95=n['frameP95Ms'] <= b['frameP95Ms'] * 1.2,
            cpuMemory=n['cpuMemoryBytes'] > 0 and b['cpuMemoryBytes'] > 0)
        for label in ('native', 'baseline'):
            samples = [r for r in gpu_rows[label] if r.get('phase') == str(phase)
                and r.get('status') == 'observing-normal-gameplay' and r.get('dedicatedGpuBytes') and float(r['dedicatedGpuBytes']) > 0]
            local[label + 'GpuSamples'] = len(samples) >= 10
            peak = max((float(r['dedicatedGpuBytes']) for r in samples), default=0)
            row[label + 'GpuMemoryBytes'] = peak
            row[label + 'ProcessWorkingSetBytes'] = max((int(r['processWorkingSetBytes']) for r in samples), default=0)
            row[label + 'GpuSamples'] = len(samples)
            local[label + 'GpuCapacity'] = 0 < peak < capacity
        row['gpuMemoryBytes'] = row['nativeGpuMemoryBytes']
        row.update(checks=local, passed=all(local.values()))
        scenarios.append(row)
    checks['scenarios'] = all(s['passed'] for s in scenarios)
    result = dict(source='summary-of-unity-runtime-and-process-counters', checks=checks,
        passed=all(checks.values()), scenarios=scenarios, graphicsMemoryCapacityBytes=capacity,
        continuousRunSeconds=native['continuousRunSeconds'],
        sources=[dict(path=p.relative_to(ROOT).as_posix(), sha256=sha(p)) for p in source_paths],
        limitation='Full unit renderer at overview zoom with automatic minimap disabled; 60-second warmup excluded from each 120-second FPS measurement. Continuous duration counts unpaused warmup and gameplay, excluding loading and paused preparation.')
    (RUN / 'performance-comparison.json').write_text(json.dumps(result, ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(passed=result['passed'], failedChecks=[k for k,v in checks.items() if not v],
        failedScenarios=[dict(units=s['units'], checks=[k for k,v in s['checks'].items() if not v]) for s in scenarios if not s['passed']])))
    assert result['passed'], 'Actual performance acceptance failed'


if __name__ == '__main__':
    main()
