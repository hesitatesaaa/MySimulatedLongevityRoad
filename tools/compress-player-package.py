"""Palette-optimize PNG assets in a player ZIP (requires Pillow)."""
import argparse
import hashlib
import io
import json
import struct
import zipfile
from pathlib import Path

from PIL import Image

MANIFEST = 'resources/actor-frames.manifest.json'
PROFILE_INFO = {'dpi', 'srgb', 'gamma', 'icc_profile', 'chromaticity', 'exif', 'xmp', 'XML:com.adobe.xmp'}


def ancillary_chunks(data, selected):
    chunks, offset = [], 8
    while data[:8] == b'\x89PNG\r\n\x1a\n' and offset + 12 <= len(data):
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


def optimize_png(relative, original):
    with Image.open(io.BytesIO(original)) as image:
        image.load()
        if PROFILE_INFO.intersection(image.info):
            return original, 'metadata'
        width, height = image.size
        artwork = image.convert('RGBA')
        if relative.startswith('GameResources/ui/ActorIcons/') and max(width, height) > 256:
            scale = 256 / max(width, height)
            artwork = artwork.resize((max(1, round(width * scale)), max(1, round(height * scale))),
                                     Image.Resampling.LANCZOS)
        indexed = artwork.quantize(colors=256, method=Image.Quantize.FASTOCTREE,
                                   dither=Image.Dither.NONE)
        buffer = io.BytesIO()
        indexed.save(buffer, format='PNG', optimize=True, compress_level=9)
    candidate = insert_before_idat(buffer.getvalue(), ancillary_chunks(original, {'caBX'}))
    with Image.open(io.BytesIO(candidate)) as image:
        image.load()
        colors = image.getcolors(257)
        assert image.mode == 'P' and colors is not None and len(colors) <= 256
    return (candidate, 'optimized') if len(candidate) < len(original) else (original, 'unchanged')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--input', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args()
    source_path, output_path = args.input.resolve(), args.output.resolve()
    if source_path == output_path:
        parser.error('--output must be a different file from --input')

    replacements, png_stats = {}, dict(optimized=0, metadata=0, unchanged=0)
    with zipfile.ZipFile(source_path) as source:
        manifest_names = [name for name in source.namelist() if name.endswith(MANIFEST)]
        if len(manifest_names) != 1:
            raise ValueError(f'Expected one {MANIFEST} in the player archive')
        manifest_name = manifest_names[0]
        prefix = manifest_name[:-len(MANIFEST)]
        for info in source.infolist():
            if not info.filename.startswith(prefix):
                continue
            relative = info.filename[len(prefix):]
            if (relative == 'icon.png' or relative.startswith(('GameResources/', 'NativeActorResources/'))):
                if relative.lower().endswith('.png'):
                    data, result = optimize_png(relative, source.read(info.filename))
                    png_stats[result] += 1
                    if result == 'optimized':
                        replacements[info.filename] = data

        manifest = json.loads(source.read(manifest_name).decode('utf-8-sig'))
        for group in manifest['groups']:
            for clip in group['actions'].values():
                for frame in clip.get('frames', []):
                    relative = f"NativeActorResources/{group['nativeResourcePath']}/main/{frame['name']}.png"
                    name = prefix + relative
                    if name in replacements:
                        data = replacements[name]
                        with Image.open(io.BytesIO(data)) as image:
                            assert list(image.size) == frame['canvas'], name
                        frame['sha256'] = hashlib.sha256(data).hexdigest()
        for icon in manifest['icons']:
            name = prefix + 'GameResources/' + icon['resourcePath'] + '.png'
            if name in replacements:
                data = replacements[name]
                with Image.open(io.BytesIO(data)) as image:
                    icon['canvas'] = list(image.size)
                icon['sha256'] = hashlib.sha256(data).hexdigest()
                icon['verifiedInGame'] = False
        replacements[manifest_name] = (json.dumps(manifest, ensure_ascii=True, indent=2) + '\n').encode('utf-8')

        file_manifest_name = prefix + 'FILE-MANIFEST.json'
        if file_manifest_name in source.namelist():
            snapshot = json.loads(source.read(file_manifest_name).decode('utf-8-sig'))
            for relative, record in snapshot.get('files', {}).items():
                name = prefix + relative
                if name in replacements:
                    data = replacements[name]
                    record.update(bytes=len(data), sha256=hashlib.sha256(data).hexdigest())
            replacements[file_manifest_name] = (json.dumps(snapshot, ensure_ascii=True, indent=2) + '\n').encode('utf-8')

        readme_name = prefix + 'README-0.6.0.txt'
        if readme_name in source.namelist():
            readme = source.read(readme_name).decode('utf-8-sig')
            note = ('PNG assets were palette-optimized and large actor icons limited to 256 px. '
                    'The compressed assets have not been retested in Unity; earlier runtime measurements '
                    'apply to the original assets.')
            if note not in readme:
                replacements[readme_name] = (readme.rstrip() + '\n\n' + note + '\n').encode('utf-8')

        with zipfile.ZipFile(output_path, 'x', compression=zipfile.ZIP_DEFLATED,
                             compresslevel=6, allowZip64=True) as output:
            output.comment = source.comment
            for info in source.infolist():
                data = replacements.get(info.filename)
                if data is None:
                    data = source.read(info.filename)
                output.writestr(info, data, compress_type=zipfile.ZIP_DEFLATED, compresslevel=6)

    with zipfile.ZipFile(output_path) as output:
        bad = output.testzip()
        assert bad is None, ('Archive CRC failed', bad)
        assert len(output.namelist()) == len(source.namelist())
    print(json.dumps(dict(inputBytes=source_path.stat().st_size, outputBytes=output_path.stat().st_size,
                          png=png_stats, replacements=len(replacements)), ensure_ascii=True))


if __name__ == '__main__':
    main()
