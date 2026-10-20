"""Apply the requested render ratios once; never resample animation pixels."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def apply():
    manifest_path = ROOT / 'resources/actor-frames.manifest.json'
    policy_path = ROOT / 'resources/actor-size-policy.json'
    manifest = json.loads(manifest_path.read_text(encoding='utf-8'))
    policy = json.loads(policy_path.read_text(encoding='utf-8'))
    if policy.get('optimizationRevision') == '20261020':
        return policy['renderAdjustments']
    specs = {item['actorId']: item for item in policy['forms']}
    ratios = {'huanglong': 2, 'qinglong': 2, 'qilin': .875, 'moqilin': .875,
              'feiqiu': .5, 'tianshou': 2 / 3, 'diyi': 2 / 3, 'disanmo': 2 / 3}
    changes = []
    for group in manifest['groups']:
        resource = group['nativeResourcePath']
        humanoid = not resource.endswith('/Beast')
        species = group['actorId'].removeprefix('mclsl_beast_').removeprefix('mclsl_named_')
        factor = 1.075 if humanoid else ratios.get(species, 1)
        if factor == 1:
            continue
        calibration = group['calibration']
        before = calibration['renderScale']
        calibration['renderScale'] *= factor
        calibration['targetVisibleWorldHeight'] *= factor
        calibration['verifiedInGame'] = False
        spec = specs[group['actorId']]
        spec['targetVisibleWorldHeight'] *= factor
        changes.append(dict(actorId=group['actorId'], ratio=factor,
                            renderScaleBefore=before, renderScaleAfter=calibration['renderScale'],
                            targetVisibleWorldHeight=spec['targetVisibleWorldHeight'], unityVerified=False))
    policy.update(optimizationRevision='20261020', renderAdjustments=changes, unityRetested=False)
    for file, value in ((manifest_path, manifest), (policy_path, policy)):
        newline = '\r\n' if b'\r\n' in file.read_bytes() else '\n'
        text = json.dumps(value, ensure_ascii=True, indent=2) + '\n'
        file.write_bytes(text.replace('\n', newline).encode('utf-8'))
    return changes


if __name__ == '__main__':
    changes = apply()
    print(json.dumps(dict(adjustedForms=len(changes), mode='render-parameters-only', unityVerified=False)))
