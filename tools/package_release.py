"""Package the source-loader mod and verify both extracted deliverables."""
import argparse
import os
import shutil
import zipfile
from pathlib import Path
from release_assets import ROOT, MANIFEST, read, sha, validate, within, write

EXCLUDED = {'.git', '.codex', '.agents', '__pycache__', 'bin', 'obj', '.vs'}


def files(directory):
    return [p for p in directory.rglob('*') if p.is_file()
            and not any(part in EXCLUDED for part in p.relative_to(directory).parts)
            and p.suffix.lower() not in ('.dll', '.exe', '.pdb', '.pyc', '.pyo', '.user', '.suo')]


def copy(relative, destination):
    source, target = within(ROOT, relative), within(destination, relative)
    target.parent.mkdir(parents=True, exist_ok=True)
    if target.exists():
        if sha(source) == sha(target):
            return
        target.unlink()
    try:
        os.link(source, target)
    except OSError:
        shutil.copyfile(source, target)


def split(archive, limit=1900 * 1024 * 1024):
    if archive.stat().st_size <= limit:
        return [archive]
    parts = []
    with archive.open('rb') as stream:
        while stream.tell() < archive.stat().st_size:
            part = archive.with_name(archive.name + f'.{len(parts)+1:03d}')
            with part.open('xb') as output:
                remaining = limit
                while remaining:
                    data = stream.read(min(8 * 1024 * 1024, remaining))
                    if not data:
                        break
                    output.write(data)
                    remaining -= len(data)
            parts.append(part)
    return parts


def package(output, compiler):
    output = within(ROOT, output)
    if output.exists():
        raise ValueError('Choose a fresh package output directory')
    if read(ROOT / 'mod.json')['version'] != '0.6.0':
        raise ValueError('Expected declaration version 0.6.0')
    report = read(ROOT / 'output/actor-redraw/validation.json')
    if report.get('resourceIntegrityPassed') is not True:
        raise ValueError('Run the resource integrity check first')
    build = read(compiler / 'build-record.json')
    if (build['version'] != '0.6.0' or build['configuration'] != 'Release'
            or sha(compiler / 'MySimulatedLongevityRoad.dll') != build['assemblySha256'].lower()):
        raise ValueError('Missing or mismatched Release compiler output')
    output.mkdir(parents=True)
    attachments, archives = [], []
    summary = {k: build[k] for k in ('configuration', 'version', 'sources', 'builtAtUtc', 'assemblySha256')}
    for kind in ('player', 'developer'):
        stage = output / kind / 'MySimulatedLongevityRoad'
        stage.mkdir(parents=True)
        for directory in ('code', 'GameResources', 'NativeActorResources', 'Locales'):
            for source in files(ROOT / directory):
                copy(source.relative_to(ROOT).as_posix(), stage)
        required = {'InterestingTrait.cs', 'default_config.json', 'mod.json', 'icon.png', MANIFEST,
                    'resources/actor-redraw.targets.json', 'resources/RELEASE-0.6.0.md'}
        for relative in required | set(report['files']):
            copy(relative, stage)
        write(stage / 'build-summary.json', summary)
        write(stage / 'asset-validation.json', {k: v for k, v in report.items() if k not in ('files', 'sources')})
        if kind == 'developer':
            for directory in ('tools', 'resources'):
                for source in files(ROOT / directory):
                    relative = source.relative_to(ROOT).as_posix()
                    if source.suffix.lower() != '.png' or relative in report['sources']:
                        copy(relative, stage)
            for relative in report['sources']:
                copy(relative, stage)
            for source in (ROOT / 'output/actor-redraw/frames').glob('*/normalization-profile.json'):
                copy(source.relative_to(ROOT).as_posix(), stage)
            shutil.copyfile(ROOT / 'resources/DEVELOPMENT-0.6.0.md', stage / 'DEVELOPMENT.md')
        archive = output / f'MySimulatedLongevityRoad-0.6.0-{kind}.zip'
        with zipfile.ZipFile(archive, 'x', allowZip64=True, compression=zipfile.ZIP_DEFLATED, compresslevel=1) as z:
            for file in sorted(files(stage)):
                z.write(file, (Path(stage.name) / file.relative_to(stage)).as_posix(),
                        compress_type=zipfile.ZIP_STORED if file.suffix.lower() == '.png' else zipfile.ZIP_DEFLATED)
        extracted = output / 'verified' / kind
        with zipfile.ZipFile(archive) as z:
            z.extractall(extracted)
        checked = extracted / 'MySimulatedLongevityRoad'
        validation = validate(checked, include_sources=kind == 'developer')
        if read(checked / 'mod.json')['version'] != '0.6.0' or validation['checkedFrames'] != report['checkedFrames']:
            raise ValueError('Packaged version or coverage mismatch')
        for file in files(stage):
            if sha(file) != sha(checked / file.relative_to(stage)):
                raise ValueError('Extracted package hash mismatch: ' + str(file))
        forbidden = [p for p in checked.rglob('*') if p.is_file() and
                     (any(part in EXCLUDED for part in p.relative_to(checked).parts)
                      or p.suffix.lower() in ('.dll', '.exe', '.pdb', '.pyc'))]
        if forbidden:
            raise ValueError('Forbidden package file: ' + str(forbidden[0]))
        # Only discard this run's generated extraction, after checking its resolved scope.
        extracted.resolve().relative_to(output.resolve())
        shutil.rmtree(extracted)
        archives.append(dict(name=archive.name, sha256=sha(archive), bytes=archive.stat().st_size,
                             extractedVerificationPassed=True))
        attachments.extend(split(archive))
        print(archive.name + ': extracted verification passed', flush=True)
    notes = output / 'RELEASE-NOTES.md'
    shutil.copyfile(ROOT / 'resources/RELEASE-0.6.0.md', notes)
    if any(p.suffix[1:].isdigit() for p in attachments):
        with notes.open('a', encoding='utf-8') as stream:
            stream.write('\n\n## 分卷解压\n下载同名ZIP的全部编号分卷，按编号二进制拼接为原名.zip后解压。'
                         'Windows示例：`cmd /c copy /b 文件.zip.001+文件.zip.002 文件.zip`。'
                         '分卷及完整ZIP的SHA256均见校验文件。\n')
    checksum = output / 'SHA256SUMS.txt'
    hashes = sorted(set(attachments + [output / a['name'] for a in archives]), key=lambda p: p.name)
    checksum.write_text(''.join(f'{sha(p)}  {p.name}\n' for p in hashes), encoding='ascii')
    attachments.extend((checksum, notes))
    write(output / 'package-record.json', dict(archives=archives, attachments=[
        dict(name=p.name, bytes=p.stat().st_size, sha256=sha(p)) for p in attachments]))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', required=True)
    parser.add_argument('--compiler', required=True)
    args = parser.parse_args()
    package(args.output, Path(args.compiler))
