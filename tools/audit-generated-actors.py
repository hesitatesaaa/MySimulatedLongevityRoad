"""Match generated images to installed hashes and retained provenance; no visual review."""
import hashlib
import json
from collections import Counter, defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
GENERATED = Path('C:/Users/Arthur.lu/.codex/generated_images')
WORK = ROOT / 'output/actor-redraw'


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def main():
    manifest = read(ROOT / 'resources/actor-frames.manifest.json')
    installed = defaultdict(list)
    for group in manifest['groups']:
        for action in group['actions'].values():
            for frame in action['frames']:
                installed[frame['sha256'].lower()].append(dict(actorId=group['actorId'], frame=frame['name']))
    for icon in manifest['icons']:
        installed[icon['sha256'].lower()].append(dict(actorId=icon['actorId'], frame='icon'))
    retained = defaultdict(list)

    def visit(value, metadata, path):
        if isinstance(value, dict):
            metadata = dict(metadata)
            for key in ('actorId', 'formId', 'frame', 'action', 'index', 'status', 'sourceFile', 'generatedSource', 'candidateFile'):
                if key in value and not isinstance(value[key], (dict, list)):
                    metadata[key] = value[key]
            for key, child in value.items():
                if isinstance(child, str) and 'sha256' in key.lower() and len(child) == 64:
                    retained[child.lower()].append(dict(metadata, record=path.relative_to(ROOT).as_posix(), hashField=key))
                elif isinstance(child, (dict, list)):
                    visit(child, metadata, path)
        elif isinstance(value, list):
            for child in value:
                visit(child, metadata, path)

    for directory in (WORK, ROOT / 'resources/actor-provenance', ROOT / 'resources/actor-native-reviews',
                      ROOT / 'output/beast-overhaul'):
        for path in directory.rglob('*.json'):
            if path.name == 'generated-images-audit.json':
                continue
            try:
                visit(read(path), {}, path)
            except (ValueError, OSError):
                continue
    records = []
    for path in GENERATED.rglob('*.png'):
        with path.open('rb') as stream:
            digest = hashlib.file_digest(stream, 'sha256').hexdigest()
        status = 'integrated' if digest in installed else ('retained-not-integrated' if digest in retained else 'unmapped')
        records.append(dict(file=str(path), sha256=digest, status=status, installed=installed[digest],
                            references=retained[digest]))
    summary = dict(Counter(x['status'] for x in records))
    output = WORK / 'generated-images-audit.json'
    output.write_text(json.dumps(dict(summary=summary, files=records), ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(summary=summary, output=str(output))))


if __name__ == '__main__':
    main()
