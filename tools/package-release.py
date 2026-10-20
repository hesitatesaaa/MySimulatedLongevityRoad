"""Create verified release archives or an explicit local developer snapshot."""
import argparse
import hashlib
import io
import json
import re
import struct
import subprocess
import sys
import zipfile
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
SKIP = {'.git', '.codex', '__pycache__', 'bin', 'obj', '.vs', 'fantasy-reference'}
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


def archive(output, name, entries, readme, snapshot, replacements=None):
    replacements = replacements or {}
    destination = output / name
    with zipfile.ZipFile(destination, 'x', compression=zipfile.ZIP_DEFLATED, compresslevel=6, allowZip64=True) as zipped:
        for relative, source in entries:
            name = 'MySimulatedLongevityRoad/' + relative
            if relative in replacements:
                zipped.writestr(name, replacements[relative])
            elif source.suffix == '.log':
                # Keep local evidence intact while removing the Steam account
                # identifier from the public developer archive's engine logs.
                zipped.writestr(name, re.sub(rb'\b7656119\d{10}\b', b'[redacted-steam-id]', source.read_bytes()))
            else:
                zipped.write(source, name)
        zipped.writestr('MySimulatedLongevityRoad/README-0.6.0.txt', readme)
        zipped.writestr('MySimulatedLongevityRoad/FILE-MANIFEST.json', json.dumps(snapshot, ensure_ascii=True, indent=2))
    with zipfile.ZipFile(destination) as zipped:
        for relative, record in snapshot['files'].items():
            data = zipped.read('MySimulatedLongevityRoad/' + relative)
            assert len(data) == record['bytes'] and hashlib.sha256(data).hexdigest() == record['sha256'], relative
        assert len(zipped.namelist()) == len(entries) + 2
    return destination


PROFILE_INFO = {'dpi', 'srgb', 'gamma', 'icc_profile', 'chromaticity', 'exif', 'xmp', 'XML:com.adobe.xmp'}


def ancillary_chunks(data, selected):
    if data[:8] != b'\x89PNG\r\n\x1a\n':
        return []
    chunks, offset = [], 8
    while offset + 12 <= len(data):
        size = struct.unpack_from('>I', data, offset)[0]
        end = offset + size + 12
        kind = data[offset + 4:offset + 8].decode('ascii', 'replace')
        if end > len(data):
            raise ValueError('Invalid PNG chunk length')
        if kind in selected:
            chunks.append(data[offset:end])
        offset = end
        if kind == 'IEND':
            break
    return chunks


def insert_before_idat(data, chunks):
    if not chunks:
        return data
    result, offset, inserted = bytearray(data[:8]), 8, False
    while offset + 12 <= len(data):
        size = struct.unpack_from('>I', data, offset)[0]
        end = offset + size + 12
        kind = data[offset + 4:offset + 8]
        if kind == b'IDAT' and not inserted:
            result.extend(b''.join(chunks))
            inserted = True
        result.extend(data[offset:end])
        offset = end
        if kind == b'IEND':
            break
    if not inserted:
        raise ValueError('PNG output has no IDAT chunk')
    return bytes(result)


def compress_player_assets(entries):
    replacements, skipped_metadata = {}, 0
    for relative, source in entries:
        if source.suffix.lower() != '.png' or not (
            relative == 'icon.png' or relative.startswith(('GameResources/', 'NativeActorResources/'))
        ):
            continue
        original = source.read_bytes()
        with Image.open(io.BytesIO(original)) as image:
            image.load()
            if PROFILE_INFO.intersection(image.info):
                skipped_metadata += 1
                continue
            width, height = image.size
            artwork = image.convert('RGBA')
            if relative.startswith('GameResources/ui/ActorIcons/') and max(width, height) > 256:
                scale = 256 / max(width, height)
                dimensions = (max(1, round(width * scale)), max(1, round(height * scale)))
                artwork = artwork.resize(dimensions, Image.Resampling.LANCZOS)
            indexed = artwork.quantize(colors=256, method=Image.Quantize.FASTOCTREE,
                                       dither=Image.Dither.NONE)
            buffer = io.BytesIO()
            indexed.save(buffer, format='PNG', optimize=True, compress_level=9)
        candidate = insert_before_idat(buffer.getvalue(), ancillary_chunks(original, {'caBX'}))
        with Image.open(io.BytesIO(candidate)) as optimized:
            optimized.load()
            colors = optimized.getcolors(257)
            assert optimized.mode == 'P' and colors is not None and len(colors) <= 256, relative
        if len(candidate) < len(original):
            replacements[relative] = candidate

    manifest_path = 'resources/actor-frames.manifest.json'
    manifest = json.loads((dict(entries)[manifest_path]).read_text(encoding='utf-8-sig'))
    for group in manifest['groups']:
        for clip in group['actions'].values():
            for frame in clip.get('frames', []):
                relative = (Path('NativeActorResources') / group['nativeResourcePath'] / 'main'
                            / (frame['name'] + '.png')).as_posix()
                if relative not in replacements:
                    continue
                data = replacements[relative]
                with Image.open(io.BytesIO(data)) as image:
                    assert list(image.size) == frame['canvas'], relative
                frame['sha256'] = hashlib.sha256(data).hexdigest()
    for icon in manifest['icons']:
        relative = 'GameResources/' + icon['resourcePath'] + '.png'
        if relative not in replacements:
            continue
        data = replacements[relative]
        with Image.open(io.BytesIO(data)) as image:
            icon['canvas'] = list(image.size)
        icon['sha256'] = hashlib.sha256(data).hexdigest()
        icon['verifiedInGame'] = False
    replacements[manifest_path] = (json.dumps(manifest, ensure_ascii=True, indent=2) + '\n').encode('utf-8')
    return replacements, dict(optimizedImages=len(replacements) - 1, metadataImagesKept=skipped_metadata,
                              imageBytesBefore=sum(source.stat().st_size for _, source in entries
                                  if source.suffix.lower() == '.png'),
                              imageBytesAfter=sum(len(replacements[relative]) if relative in replacements
                                                  else source.stat().st_size
                                  for relative, source in entries if source.suffix.lower() == '.png'))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', required=True)
    parser.add_argument('--kind', choices=('both', 'player', 'developer'), default='player')
    parser.add_argument('--local-development', action='store_true')
    parser.add_argument('--compress-assets', action='store_true', default=None,
                        help='Create a compact player archive with palette-optimized PNG assets')
    args = parser.parse_args()
    if args.compress_assets is None:
        args.compress_assets = args.kind == 'player'
    output = Path(args.output).resolve()
    output.relative_to(ROOT)
    assert not output.exists(), 'Choose a fresh package directory'
    declaration = json.loads((ROOT / 'mod.json').read_text(encoding='utf-8-sig'))
    assert declaration['version'] == '0.6.0'
    if args.compress_assets and args.kind != 'player':
        parser.error('--compress-assets must be used with --kind player')
    if args.local_development and args.kind != 'developer' and not args.compress_assets:
        parser.error('Local development snapshots must use --kind developer, except compressed player snapshots')
    worktree = subprocess.check_output(['git', 'status', '--porcelain', '--untracked-files=all'], cwd=ROOT)
    if not args.local_development:
        assert not worktree.strip(), 'Commit release inputs before packaging'
    checks = [['tools/validate-current-assets.py'], ['tools/validate-beast-overhaul.py']]
    for command in checks:
        if not args.local_development:
            command.append('--release')
        subprocess.run([sys.executable, *command], cwd=ROOT, check=True)
    commit = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip()
    player = [(relative, source) for relative, source in files(BASE + ['resources/actor-frames.manifest.json'])
              if relative != 'code/MySimulatedLongevityRoad/UI/MclslDeveloperActorEditor.cs']
    developer = files(BASE + ['resources', 'tools', '.dev-tools', 'AGENTS.md', '.gitignore', 'README.md'])
    output.mkdir(parents=True)
    common = (f'MySimulatedLongevityRoad 0.6.0\nBase commit: {commit}\nGame build: {declaration["targetGameBuild"]}\n'
              'Fresh test worlds are recommended. Back up existing saves before testing migration.\n'
              'Current art and code have not been retested in Unity; offline reviews do not certify gameplay or performance.\n'
              'Install NeoModLoader, extract this directory into the game mods directory, and avoid duplicate installations.\n')
    archives = []
    package_kinds = ([('player', player), ('developer', developer)] if args.kind == 'both'
                     else [('player', player)] if args.kind == 'player' else [('developer', developer)])
    for kind, entries in package_kinds:
        replacements, compression = ({}, None)
        if args.compress_assets:
            replacements, compression = compress_player_assets(entries)
        file_records = {}
        for relative, source in entries:
            data = replacements[relative] if relative in replacements else source.read_bytes()
            if source.suffix == '.log':
                data = re.sub(rb'\b7656119\d{10}\b', b'[redacted-steam-id]', data)
            file_records[relative] = dict(bytes=len(data), sha256=hashlib.sha256(data).hexdigest())
        snapshot = dict(baseCommit=commit, localDevelopment=args.local_development,
            uncommittedChanges=bool(worktree.strip()), worktreeStatusSha256=hashlib.sha256(worktree).hexdigest(),
            unityRetested=False, files=file_records)
        if compression:
            snapshot['compressionProfile'] = dict(pngColors=256, iconMaximumSide=256,
                metadataPolicy='Preserve color-profile and text-metadata PNGs unchanged; retain caBX chunks.',
                unityRetested=False)
            snapshot['compressionStats'] = compression
        suffix = ('-player-local-compressed' if args.local_development else '-player-compressed') \
            if args.compress_assets else ('-developer-local' if args.local_development else '-' + kind)
        readme = common
        if args.local_development:
            readme += 'Local uncommitted snapshot. Not published.\n'
        if compression:
            readme += ('Package-only PNG optimization: up to 256 palette colors; actor icons larger than 256 pixels '
                       'were downscaled to a maximum side of 256. Color-profile and text-metadata PNGs remain unchanged.\n'
                       'Image encoding changed slightly; the compressed package has not been retested in Unity.\n')
            readme += ('Recreate this package with tools/package-release.py --kind player --compress-assets '
                       '--local-development --output build/<fresh-directory>.\n')
        else:
            readme += 'Build: tools/build-release.ps1 -LocalDevelopment -PythonExecutable <Python-with-Pillow>\n'
        archives.append(archive(output, 'MySimulatedLongevityRoad-0.6.0' + suffix + '.zip', entries,
            readme, snapshot, replacements))
        (output / (kind + '-file-manifest.json')).write_text(json.dumps(snapshot, ensure_ascii=True, indent=2), encoding='utf-8')
    (output / 'worktree-status.txt').write_bytes(worktree)
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
    (output / 'package-record.json').write_text(json.dumps(dict(baseCommit=commit, localDevelopment=args.local_development,
        uncommittedChanges=bool(worktree.strip()), archives=records,
        splitRestore='Concatenate .part001, .part002, ... in numeric order into the original .zip, then verify SHA256.'), indent=2), encoding='utf-8')
    print(json.dumps(dict(commit=commit, output=str(output), archives=records)))


if __name__ == '__main__':
    main()
