"""Prepare one isolated native/baseline gameplay observation without shipping probes."""
import argparse
import hashlib
import json
import shutil
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
RUN = ROOT / 'output/runtime-validation/release-20261019'
STAGE = RUN / 'game/mods/MySimulatedLongevityRoad'


def bounds(path):
    with Image.open(path) as image:
        alpha = image.convert('RGBA').getchannel('A').point(lambda a: 255 if a >= 128 else 0).getbbox()
        assert alpha
        return list(image.size), list(alpha)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--baseline', action='store_true')
    parser.add_argument('--probe', choices=('runtime', 'recipe', 'death', 'spell', 'reload'))
    args = parser.parse_args()
    assert not (args.baseline and args.probe)
    STAGE.resolve().relative_to(ROOT)
    shutil.copytree(ROOT / 'code', STAGE / 'code', dirs_exist_ok=True)
    runtime = STAGE / 'code/MySimulatedLongevityRoad/Core/MclslRuntime.cs'
    source = runtime.read_text(encoding='utf-8-sig')
    needle = '        MclslActorSizeProbe.Tick();'
    assert source.count(needle) == 1
    probe = 'MclslBeast' + dict(runtime='Runtime', recipe='Recipe', death='Death', spell='Spell', reload='Reload')[args.probe] + 'Probe' if args.probe else 'MclslReleasePopulationProbe'
    runtime.write_text(source.replace(needle, needle + '\n        ' + probe + '.Tick();'), encoding='utf-8')
    shutil.copyfile(Path(__file__).with_name('MclslReleasePopulationProbe.cs'),
                    STAGE / 'code/MySimulatedLongevityRoad/Systems/Beasts/MclslReleasePopulationProbe.cs')
    if args.probe:
        probe_folder = Path(__file__).parent if args.probe == 'reload' else ROOT / 'tools/beast-runtime-probe'
        shutil.copyfile(probe_folder / (probe + '.cs'),
            STAGE / 'code/MySimulatedLongevityRoad/Systems/Beasts' / (probe + '.cs'))
        if args.probe == 'spell':
            spells = STAGE / 'code/MySimulatedLongevityRoad/Systems/Cultivation/MclslSpellSystem.cs'
            code = spells.read_text(encoding='utf-8-sig')
            anchor = 'try {target.getHit(damage,true,type,caster,false);}finally {_applyingSpellDamage=previous;}'
            assert code.count(anchor) == 1
            replacement = 'var receipt=MclslBeastSpellProbe.BeginHit(caster,target,damage); try {target.getHit(damage,true,type,caster,false);}finally {MclslBeastSpellProbe.EndHit(receipt,target);_applyingSpellDamage=previous;}'
            spells.write_text(code.replace(anchor, replacement), encoding='utf-8')
    manifest = json.loads((ROOT / 'resources/actor-frames.manifest.json').read_text(encoding='utf-8-sig'))
    if args.baseline:
        for group in manifest['groups']:
            first = group['actions']['idle']['frames'][0]
            _, native_bounds = bounds(ROOT / 'NativeActorResources' / group['nativeResourcePath'] / 'main' / (first['name'] + '.png'))
            native_height = (native_bounds[3] - native_bounds[1]) / first['pixelsPerUnit'] * group['calibration']['renderScale']
            folder = STAGE / 'GameResources' / group['resourcePath'] / 'main'
            canvas, legacy_bounds = bounds(folder / 'idle_0.png')
            group['calibration']['renderScale'] = 1
            group.pop('nativeResourcePath', None)
            group.update(canvas=canvas, anchor=[canvas[0] / 2, legacy_bounds[3]], pixelsPerUnit=(legacy_bounds[3] - legacy_bounds[1]) / native_height)
            for action, clip in group['actions'].items():
                if clip.get('visualFallback'):
                    continue
                duration = sum(f['durationMs'] for f in clip['frames'])
                available = [f for f in clip['frames'] if (folder / (f['name'] + '.png')).exists()]
                if not available:
                    assert action in ('attack', 'hit'), ('Missing legacy baseline action', group['actorId'], action)
                    clip.update(loop=False, frames=[], visualFallback='idle', durationMs=duration)
                    continue
                for frame in available:
                    path = folder / (frame['name'] + '.png')
                    frame_canvas, frame_bounds = bounds(path)
                    frame.update(canvas=frame_canvas, anchor=[frame_canvas[0] / 2, frame_bounds[3]], pixelsPerUnit=group['pixelsPerUnit'],
                        geometryConfigured=True, durationMs=duration / len(available), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
                clip['frames'] = available
        manifest['status'] = 'diagnostic-legacy-pixel-baseline-only'
    output = STAGE / 'resources/actor-frames.manifest.json'
    output.write_text(json.dumps(manifest, ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
    snapshot = RUN / ((args.probe + '-input.json') if args.probe else 'baseline-input.json' if args.baseline else 'native-input.json')
    code_hash = hashlib.sha256()
    for path in sorted((ROOT / 'code').rglob('*.cs')) + [ROOT / 'InterestingTrait.cs']:
        code_hash.update(path.relative_to(ROOT).as_posix().encode())
        code_hash.update(path.read_bytes())
    snapshot.write_text(json.dumps(dict(mode=args.probe or ('baseline' if args.baseline else 'native'), manifestSha256=hashlib.sha256(output.read_bytes()).hexdigest(),
        productionCodeSha256=code_hash.hexdigest(),
        baselineDefinition='same production gameplay code; legacy GameResources PNGs with matching visible world height' if args.baseline else None), indent=2), encoding='utf-8')
    print(json.dumps(dict(stage=str(STAGE), baseline=args.baseline)))


if __name__ == '__main__':
    main()
