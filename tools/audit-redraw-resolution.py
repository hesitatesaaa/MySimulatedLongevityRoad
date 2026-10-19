"""Audit only independently redrawn 0.6.0 installed PNGs, without modifying images."""
import json
import hashlib
import struct
from collections import Counter
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def canvas(path):
    with path.open('rb') as stream:
        header = stream.read(24)
    if header[:8] != b'\x89PNG\r\n\x1a\n':
        raise ValueError('Not PNG: '+str(path))
    return list(struct.unpack('>II', header[16:24]))

manifest = json.loads((ROOT/'resources/actor-frames.manifest.json').read_text(encoding='utf-8-sig'))
forms = []
total = downsampled = 0
for group in manifest['groups']:
    records = []
    for action in group['actions'].values():
        for frame in action['frames']:
            if frame.get('origin') != 'independently-redrawn':
                continue
            proof = json.loads((ROOT/frame['provenance']).read_text(encoding='utf-8-sig'))
            installed = ROOT/'GameResources'/group['resourcePath']/'main'/(frame['name']+'.png')
            original = ROOT/proof['sourceFile']
            if hashlib.sha256(installed.read_bytes()).hexdigest() != frame['sha256']:
                raise ValueError('Installed hash mismatch: '+str(installed))
            if hashlib.sha256(original.read_bytes()).hexdigest() != proof['sourceSha256']:
                raise ValueError('Original hash mismatch: '+str(original))
            source_size, installed_size = canvas(original), canvas(installed)
            scale = proof.get('normalization', {}).get('scale')
            lost = scale is not None and scale < 1
            total += 1
            downsampled += int(lost)
            records.append(dict(frame=frame['name'], originalCanvas=source_size,
                installedCanvas=installed_size, scale=scale, downsampled=lost))
    if records:
        forms.append(dict(actorId=group['actorId'], frames=records))
report = dict(checkedAtUtc=datetime.now(timezone.utc).isoformat(),
    scope='0.6.0-origin-independently-redrawn-only', oldBaselineImagesChecked=False,
    checkedFrames=total, downsampledFrames=downsampled, checkedForms=len(forms),
    originalsHashVerified=True, installedHashVerified=True, forms=forms,
    interpretation='PNG encoding is lossless; recorded nearest-neighbor downsampling loses source detail. Runtime does not call Compress or enable mipmaps.',
    currentUserPolicy='Use installed frames, retain existing installed resolution, one conversion from independent generated source; originals archive only.')
destination = ROOT/'output/actor-redraw/clarity-audit-20261013.json'
destination.write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(json.dumps(dict(report=str(destination), frames=total, downsampled=downsampled, forms=len(forms))))
