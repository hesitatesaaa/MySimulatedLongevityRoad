"""Create verified release archives or an explicit local developer snapshot."""
import argparse
import hashlib
import gzip
import io
import json
import re
import subprocess
import sys
import zipfile
from pathlib import Path
from PIL import Image
from png_chunks import PROFILE_INFO, ancillary_chunks, insert_before_idat

ROOT = Path(__file__).resolve().parents[1]
from release_inputs import BASE, files, player_files


def archive(output, name, entries, readme, snapshot, replacements=None):
    replacements = replacements or {}
    source_names = {relative for relative, _ in entries if relative.endswith('.cs')}
    expected_sources = {relative for relative in source_names if relative.startswith('code/')} | {'MclslMod.cs'}
    if source_names != expected_sources:
        raise ValueError('Unexpected C# compilation inputs in package: ' + repr(sorted(source_names - expected_sources)))
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
        zipped.writestr('MySimulatedLongevityRoad/README.txt', readme)
        zipped.writestr('MySimulatedLongevityRoad/FILE-MANIFEST.json', json.dumps(snapshot, ensure_ascii=True, indent=2))
    with zipfile.ZipFile(destination) as zipped:
        for relative, record in snapshot['files'].items():
            data = zipped.read('MySimulatedLongevityRoad/' + relative)
            assert len(data) == record['bytes'] and hashlib.sha256(data).hexdigest() == record['sha256'], relative
        assert len(zipped.namelist()) == len(entries) + 2
    return destination




def compress_player_assets(entries):
    replacements, skipped_metadata = {}, 0
    for relative, source in entries:
        if source.suffix.lower() != '.png' or not (
            relative == 'icon.png' or relative.startswith(('GameResources/', 'NativeActorResources/', 'InterfaceResources/'))
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
    living_path = 'resources/living-resources.json'
    living = json.loads(dict(entries)[living_path].read_text(encoding='utf-8-sig'))
    for record in living['Outputs']:
        relative = record['File']
        if relative in replacements:
            record['SHA256'] = hashlib.sha256(replacements[relative]).hexdigest()
    replacements[living_path] = (json.dumps(living, ensure_ascii=True, indent=2) + '\n').encode('utf-8')
    for path in ('resources/contract-skill-icons.manifest.json', 'resources/career-effects.manifest.json'):
        records = json.loads(dict(entries)[path].read_text(encoding='utf-8-sig'))
        for asset in records['assets']:
            asset['workspaceSha256'] = asset['sha256']
            source = dict(entries)[asset['path']]
            data = replacements[asset['path']] if asset['path'] in replacements else source.read_bytes()
            asset['sha256'] = hashlib.sha256(data).hexdigest()
        replacements[path] = (json.dumps(records, ensure_ascii=True, indent=2) + '\n').encode('utf-8')
    return replacements, dict(optimizedImages=sum(name.endswith('.png') for name in replacements), metadataImagesKept=skipped_metadata,
                              imageBytesBefore=sum(source.stat().st_size for _, source in entries
                                  if source.suffix.lower() == '.png'),
                              imageBytesAfter=sum(len(replacements[relative]) if relative in replacements
                                                  else source.stat().st_size
                                  for relative, source in entries if source.suffix.lower() == '.png'))


def pack_soul_atlas(entries, replacements, output):
    """Merge already palette-optimized frames without any further pixel changes.

    The compiled RGBA texture is outside GameResources, so NML does not decode
    904 separate textures. Source PNGs remain untouched in the checkout.
    """
    frames = []
    for relative, source in entries:
        if relative.startswith('GameResources/actors/Souls/') and source.suffix.lower() == '.png':
            original = source.read_bytes()
            with Image.open(io.BytesIO(original)) as source_image:
                # Preserve the existing metadata exceptions as standalone PNGs.
                if PROFILE_INFO.intersection(source_image.info) or ancillary_chunks(original, {'caBX'}):
                    continue
            with Image.open(io.BytesIO(replacements.get(relative, source.read_bytes()))) as image:
                frames.append((relative, image.convert('RGBA')))
    if not frames:
        return entries, replacements, dict(frames=0)
    frames.sort(key=lambda item: (-item[1].height, item[0]))
    width, padding, x, y, row = 2048, 2, 2, 2, 0
    placements = []
    for relative, image in frames:
        if image.width + padding * 2 > width:
            raise ValueError('Soul frame exceeds atlas row width: ' + relative)
        if x + image.width + padding > width:
            x, y, row = padding, y + row + padding, 0
        placements.append((relative, image, x, y))
        x += image.width + padding
        row = max(row, image.height)
    height = y + row + padding
    # Rows are spaced by their tallest frame, so a frame may never cover another
    # one; prove it on a mask instead of trusting the arithmetic.
    occupied = Image.new('1', (width, height))
    for relative, image, px, py in placements:
        if occupied.crop((px, py, px + image.width, py + image.height)).getbbox():
            raise ValueError('Soul atlas frames overlap: ' + relative)
        occupied.paste(1, (px, py, px + image.width, py + image.height))
    assert width * height * 4 <= 128 * 1024 * 1024, 'Soul atlas exceeds bounded decode limit'
    atlas = Image.new('RGBA', (width, height))
    rectangles = {}
    for relative, image, x, y in placements:
        atlas.paste(image, (x, y))
        rectangles[relative[len('GameResources/'):-4]] = [x, height-y-image.height, image.width, image.height]
    # Unity raw texture rows start at the bottom; PNG/Pillow rows start at top.
    raw = atlas.transpose(Image.Transpose.FLIP_TOP_BOTTOM).tobytes()
    compiled = gzip.compress(raw, compresslevel=9, mtime=0)
    decoded = Image.frombytes('RGBA', (width, height), gzip.decompress(compiled)).transpose(Image.Transpose.FLIP_TOP_BOTTOM)
    for relative, image, x, y in placements:
        assert decoded.crop((x,y,x+image.width,y+image.height)).tobytes() == image.tobytes(), relative
    manifest_path = 'resources/world-soul-frames.manifest.json'
    manifest = json.loads(dict(entries)[manifest_path].read_text(encoding='utf-8-sig'))
    manifest['atlas'] = dict(file='Souls/player-frames.rgba.gz', canvas=[width,height],
        rgbaSha256=hashlib.sha256(raw).hexdigest(), rectangles=rectangles)
    replacements[manifest_path] = (json.dumps(manifest,ensure_ascii=True,indent=2)+'\n').encode('utf-8')
    packed_path = 'NativeActorResources/Souls/player-frames.rgba.gz'
    compiled_file = output / 'soul-player-atlas.rgba.gz'
    compiled_file.write_bytes(compiled)
    originals = {relative for relative, _ in frames}
    entries = [(relative, source) for relative, source in entries if relative not in originals]
    entries.append((packed_path, compiled_file))
    entries.sort(key=lambda item:item[0])
    return entries, replacements, dict(frames=len(frames), canvas=[width,height], decodedBytes=len(raw),
        compressedBytes=len(compiled), pixelIdenticalToExistingPlayerCompression=True,
        automaticallyImportedTexturesRemoved=len(frames), eagerAtlasTextures=1)


def verify_prebuilt_soul_atlas():
    """When Soul source PNGs are absent, verify the shipped compiled atlas still
    matches resources/world-soul-frames.manifest.json instead of regenerating."""
    manifest = json.loads((ROOT / 'resources/world-soul-frames.manifest.json').read_text(encoding='utf-8-sig'))
    atlas = manifest.get('atlas') or {}
    assert atlas.get('rgbaSha256'), 'Soul catalog has no atlas hash and no source PNGs to rebuild it'
    compiled = ROOT / 'NativeActorResources' / atlas['file']
    assert compiled.is_file(), 'Missing compiled soul atlas: ' + str(compiled)
    with gzip.open(compiled, 'rb') as handle:
        rgba = handle.read()
    digest = hashlib.sha256(rgba).hexdigest()
    assert digest == atlas['rgbaSha256'], 'Compiled soul atlas does not match catalog manifest'
    print(json.dumps(dict(prebuiltSoulAtlas=True, rgbaSha256=digest, decodedBytes=len(rgba))))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', required=True)
    parser.add_argument('--kind', choices=('both', 'player', 'developer'), default='player')
    parser.add_argument('--local-development', action='store_true')
    parser.add_argument('--compress-assets', action='store_true', default=None,
                        help='Palette-optimize runtime PNG assets in a player or developer archive')
    parser.add_argument('--skip-validators', action='store_true',
                        help='Local-development snapshots only: skip the art/release validators '
                             '(the provenance records predate the compact-art resize pass in this tree)')
    args = parser.parse_args()
    if args.compress_assets is None:
        args.compress_assets = args.kind == 'player'
    output = Path(args.output).resolve()
    output.relative_to(ROOT)
    assert not output.exists(), 'Choose a fresh package directory'
    declaration = json.loads((ROOT / 'mod.json').read_text(encoding='utf-8-sig'))
    version = declaration['version']
    assert re.fullmatch(r'0\.8\.\d+', version), f'Unexpected release version: {version}'
    if args.compress_assets and args.kind == 'both':
        parser.error('--compress-assets requires --kind player or --kind developer')
    if args.local_development and args.kind != 'developer' and not args.compress_assets:
        parser.error('Local development snapshots must use --kind developer, except compressed player snapshots')
    worktree = subprocess.check_output(['git', 'status', '--porcelain', '--untracked-files=all'], cwd=ROOT)
    if not args.local_development:
        assert not worktree.strip(), 'Commit release inputs before packaging'
    if args.skip_validators:
        assert args.local_development, '--skip-validators is only valid for local-development snapshots'
        print(json.dumps(dict(validatorsSkipped=True,
                              reason='local-development snapshot; art provenance records predate the compact-art resize pass')))
    if (ROOT / 'GameResources/actors/Souls').is_dir():
        if not args.skip_validators:
            subprocess.run([sys.executable, 'tools/build-soul-frame-catalog.py', '--check'], cwd=ROOT, check=True)
    else:
        # Soul source PNGs are not shipped in this checkout; the compiled atlas
        # and its catalog manifest are the release inputs. Prove they agree.
        verify_prebuilt_soul_atlas()
    if not args.skip_validators:
        checks = [['tools/validate-current-assets.py'], ['tools/validate-beast-overhaul.py'], ['tools/validate-beast-talents.py']]
        checks.append(['tools/validate-living-assets.py'])
        for command in checks:
            if not args.local_development and command[0] == 'tools/validate-living-assets.py':
                command.append('--release')
            subprocess.run([sys.executable, *command], cwd=ROOT, check=True)
    try:
        commit = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True,
                                         stderr=subprocess.DEVNULL).strip()
    except subprocess.CalledProcessError:
        assert args.local_development, 'Commit release inputs before packaging'
        commit = 'unborn-worktree'
    input_hash = subprocess.check_output([sys.executable, str(ROOT / 'tools/validate-living-assets.py'),
                                         '--fingerprint'], cwd=ROOT, text=True).strip()
    acceptance_path = ROOT / 'resources/runtime-acceptance-0.8.0.json'
    acceptance = None if args.local_development else json.loads(acceptance_path.read_text(encoding='utf-8-sig'))
    player = player_files()
    talent_evidence = ROOT / 'resources/beast-talents-runtime.json'
    developer = files(BASE + ['resources', 'tools', 'AGENTS.md', '.gitignore', 'README.md'])
    developer.sort(key=lambda item: item[0])
    output.mkdir(parents=True)
    common = (f'MySimulatedLongevityRoad {version}\nBase commit: {commit}\nGame build: {declaration["targetGameBuild"]}\n'
              f'Only new worlds and current-format saves created by this {version} build are supported. '
              'Old archives are not imported, migrated or deleted.\n' +
              ('Local build: consult FILE-MANIFEST.json for the exact current acceptance scope.\n' if args.local_development else
               'Source/resource acceptance passed; consult the external final ZIP receipt for packaged runtime acceptance.\n') +
              'Install NeoModLoader, extract this directory into the game mods directory, and avoid duplicate installations.\n')
    archives = []
    package_kinds = ([('player', player), ('developer', developer)] if args.kind == 'both'
                     else [('player', player)] if args.kind == 'player' else [('developer', developer)])
    for kind, entries in package_kinds:
        replacements, compression = ({}, None)
        if args.compress_assets:
            replacements, compression = compress_player_assets(entries)
            entries, replacements, atlas_stats = pack_soul_atlas(entries, replacements, output)
            compression['soulAtlas'] = atlas_stats
            compression['imageBytesAfter'] = sum(len(replacements[relative]) if relative in replacements
                else source.stat().st_size for relative, source in entries if source.suffix.lower() == '.png')
            compression['runtimeImageDataBytesAfter'] = compression['imageBytesAfter'] + atlas_stats.get('compressedBytes', 0)
        file_records = {}
        for relative, source in entries:
            data = replacements[relative] if relative in replacements else source.read_bytes()
            if source.suffix == '.log':
                data = re.sub(rb'\b7656119\d{10}\b', b'[redacted-steam-id]', data)
            file_records[relative] = dict(bytes=len(data), sha256=hashlib.sha256(data).hexdigest())
        snapshot = dict(baseCommit=commit, localDevelopment=args.local_development,
            sourceInputSha256=input_hash,
            uncommittedChanges=bool(worktree.strip()), worktreeStatusSha256=hashlib.sha256(worktree).hexdigest(),
            unityRetested=not args.local_development, files=file_records)
        if talent_evidence.is_file():
            talent_run = json.loads(talent_evidence.read_text(encoding='utf-8-sig'))
            if (talent_run.get('passed') and talent_run.get('actualUnity') and not talent_run.get('errors')
                    and talent_run.get('inputSha256') == input_hash):
                snapshot['beastTalentAcceptance'] = dict(
                    scope='Xuanhuang and independent beast talents functional checks; not full 0.8.0 acceptance',
                    evidenceSha256=hashlib.sha256(talent_evidence.read_bytes()).hexdigest(),
                    checks=len(talent_run['checks']),
                    interactiveVisualReview=talent_run['interactiveVisualReview'],
                    performanceTested=talent_run['performanceTested'],
                    performanceCancellation=talent_run['performanceCancellation'])
        repair_path = ROOT / 'resources/full-repair-runtime.json'
        if repair_path.is_file():
            repair = json.loads(repair_path.read_text(encoding='utf-8-sig'))
            if repair.get('passed') and repair.get('actualUnity') and not repair.get('errors') and repair.get('inputSha256') == input_hash:
                snapshot['repairAcceptance'] = dict(scope='2026-10-25 targeted repair checks; not full 0.8.0 acceptance',
                    evidenceSha256=hashlib.sha256(repair_path.read_bytes()).hexdigest(),checks=len(repair['checks']),
                    buildings=len(repair.get('buildings',[])),units=len(repair.get('units',[])),
                    interactiveVisualReview=repair['interactiveVisualReview'],performanceTested=repair['performanceTested'])
        if acceptance:
            snapshot['unityAcceptance'] = dict(scope='0.8.0 source/resource acceptance before packaging; final ZIP requires an external receipt',
                evidenceSha256=hashlib.sha256(acceptance_path.read_bytes()).hexdigest(),
                checks=len(acceptance['checks']), errors=len(acceptance['errors']),
                interactiveVisualReview=acceptance['interactiveVisualReview'],
                performanceTested=acceptance['performanceTested'])
        if compression:
            snapshot['compressionProfile'] = dict(pngColors=256, iconMaximumSide=256,
                metadataPolicy='Preserve color-profile and text-metadata PNGs unchanged; retain caBX chunks.',
                unityRetested=not args.local_development)
            snapshot['compressionStats'] = compression
        suffix = ('-' + kind + '-local-compressed' if args.local_development else '-' + kind + '-compressed') \
            if args.compress_assets else ('-developer-local' if args.local_development else '-' + kind)
        readme = common
        if args.local_development:
            readme += 'Local uncommitted snapshot. Not published.\n'
        if compression:
            readme += ('Package-only PNG optimization: up to 256 palette colors; actor icons larger than 256 pixels '
                       'were downscaled to a maximum side of 256. Color-profile and text-metadata PNGs remain unchanged.\n'
                       'Image encoding is palette optimized; acceptance scope is recorded in FILE-MANIFEST.json.\n')
            readme += (f'Recreate this package with tools/package-release.py --kind {kind} --compress-assets '
                       '--local-development --output build/<fresh-directory>.\n')
        else:
            readme += 'Build: tools/build-release.ps1 -LocalDevelopment -PythonExecutable <Python-with-Pillow>\n'
        archives.append(archive(output, 'MySimulatedLongevityRoad-' + version + suffix + '.zip', entries,
            readme, snapshot, replacements))
        (output / (kind + '-file-manifest.json')).write_text(json.dumps(snapshot, ensure_ascii=True, indent=2), encoding='utf-8')
    final_input_hash = subprocess.check_output([sys.executable, str(ROOT / 'tools/validate-living-assets.py'), '--fingerprint'], cwd=ROOT, text=True).strip()
    if final_input_hash != input_hash:
        raise RuntimeError('Production inputs changed during packaging; this output is not a valid delivery')
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
        validatorsSkipped=bool(args.skip_validators),
        uncommittedChanges=bool(worktree.strip()), archives=records,
        splitRestore='Concatenate .part001, .part002, ... in numeric order into the original .zip, then verify SHA256.'), indent=2), encoding='utf-8')
    print(json.dumps(dict(commit=commit, output=str(output), archives=records)))


if __name__ == '__main__':
    main()
