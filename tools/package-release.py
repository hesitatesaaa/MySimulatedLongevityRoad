"""Create verified player/developer ZIP64 archives from one clean release commit."""
import argparse
import hashlib
import json
import re
import subprocess
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SKIP = {'.git', '.codex', '__pycache__', 'bin', 'obj', '.vs'}
BASE = ['code', 'GameResources', 'NativeActorResources', 'Locales', 'InterestingTrait.cs',
        'default_config.json', 'mod.json', 'icon.png']


def files(paths):
    found = {}
    for relative in paths:
        source = ROOT / relative
        if not source.exists():
            raise FileNotFoundError(source)
        for path in source.rglob('*') if source.is_dir() else [source]:
            name = path.relative_to(ROOT)
            if path.is_file() and not any(p in SKIP for p in name.parts) and path.suffix not in ('.pyc', '.pyo', '.dll', '.exe', '.pdb'):
                found[name.as_posix()] = path
    return sorted(found.items())


def archive(output, name, entries, readme):
    destination = output / name
    with zipfile.ZipFile(destination, 'x', compression=zipfile.ZIP_DEFLATED, compresslevel=1, allowZip64=True) as zipped:
        for relative, source in entries:
            name = 'MySimulatedLongevityRoad/' + relative
            if source.suffix == '.log':
                # Keep local evidence intact while removing the Steam account
                # identifier from the public developer archive's engine logs.
                zipped.writestr(name, re.sub(rb'\b7656119\d{10}\b', b'[redacted-steam-id]', source.read_bytes()))
            else:
                zipped.write(source, name)
        zipped.writestr('MySimulatedLongevityRoad/README-0.6.0.txt', readme)
    with zipfile.ZipFile(destination) as zipped:
        bad = zipped.testzip()
        assert bad is None, ('Archive CRC failed', bad)
        assert len(zipped.namelist()) == len(entries) + 1
    return destination


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', required=True)
    args = parser.parse_args()
    output = Path(args.output).resolve()
    output.relative_to(ROOT)
    assert not output.exists(), 'Choose a fresh package directory'
    declaration = json.loads((ROOT / 'mod.json').read_text(encoding='utf-8-sig'))
    assert declaration['version'] == '0.6.0'
    assert not subprocess.check_output(['git', 'status', '--porcelain', '--untracked-files=no'], cwd=ROOT).strip(), 'Commit release inputs before packaging'
    for command in (['tools/actor-redraw.py', 'validate', '--release'], ['tools/validate-beast-overhaul.py', '--release']):
        subprocess.run([sys.executable, *command], cwd=ROOT, check=True)
    commit = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip()
    player = files(BASE + ['resources/actor-frames.manifest.json'])
    developer = files(BASE + ['resources', 'tools', 'AGENTS.md', '.gitignore',
        'output/actor-redraw', 'output/beast-overhaul'])
    # Include raw acceptance reports, without copying game binaries or saved test worlds.
    reports = [p.relative_to(ROOT).as_posix() for directory in ('output/runtime-validation/release-20261019',
        'output/beast-runtime-20261014-a') for p in (ROOT / directory).glob('*') if p.is_file() and p.suffix in ('.json', '.log', '.csv', '.txt')]
    developer = sorted(dict(developer + files(reports)).items())
    output.mkdir(parents=True)
    common = (f'MySimulatedLongevityRoad 0.6.0\nCommit: {commit}\nGame build: {declaration["targetGameBuild"]}\n'
              'New saves only. Further image review was waived by the user.\n'
              'Tested with WorldBox 0.51.2, GTX 1660 Ti 6 GiB, 1920x1080. Native texture cache limit: 2 GiB.\n'
              'Released under the user-accepted existing test scope: native 100/500/1000-unit measurements and approximately 19 minutes; final matched baseline was not rerun.\n'
              'Install NeoModLoader, extract this directory into the game mods directory, and avoid duplicate installations.\n')
    archives = [archive(output, 'MySimulatedLongevityRoad-0.6.0-player.zip', player, common),
                archive(output, 'MySimulatedLongevityRoad-0.6.0-developer.zip', developer,
                    common + 'Developer files include tools, originals, provenance and retained revisions.\n'
                    'Public engine logs redact Steam account identifiers; local raw evidence is unchanged.\n'
                    'Build with tools/build-release.ps1 using your own WorldBox/NML dependencies. Python requires Pillow.\n')]
    records = []
    for path in archives:
        with path.open('rb') as stream:
            digest = hashlib.file_digest(stream, 'sha256').hexdigest()
        records.append(dict(name=path.name, bytes=path.stat().st_size, sha256=digest))
        if path.stat().st_size >= 2_000_000_000:
            with path.open('rb') as stream:
                index = 1
                while chunk := stream.read(512 * 1024 * 1024):
                    part = path.with_name(path.name + f'.part{index:03}')
                    part.write_bytes(chunk)
                    records.append(dict(name=part.name, bytes=len(chunk), sha256=hashlib.sha256(chunk).hexdigest()))
                    index += 1
    (output / 'SHA256SUMS.txt').write_text(''.join(f'{r["sha256"]}  {r["name"]}\n' for r in records), encoding='utf-8')
    (output / 'package-record.json').write_text(json.dumps(dict(commit=commit, archives=records,
        splitRestore='Concatenate .part001, .part002, ... in numeric order into the original .zip, then verify SHA256.'), indent=2), encoding='utf-8')
    print(json.dumps(dict(commit=commit, output=str(output), archives=records)))


if __name__ == '__main__':
    main()
