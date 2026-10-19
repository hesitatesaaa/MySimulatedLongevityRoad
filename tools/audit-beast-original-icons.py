"""Read-only pixel audit of existing item resources; does not edit images."""
import hashlib
import json
from collections import defaultdict
from pathlib import Path
from PIL import Image

root = Path(__file__).resolve().parents[1]
items = json.loads((root / 'resources/beast-original-icons.manifest.json').read_text(encoding='utf-8-sig'))
missing, invalid, placeholders, samples = [], [], [], []
hashes = defaultdict(list)
for item in items:
    path = root / item['path']
    if not path.exists():
        missing.append(item)
        continue
    with Image.open(path) as image:
        alpha = image.convert('RGBA').getchannel('A')
        bounds = alpha.getbbox()
        occupied = sum(1 for value in alpha.getdata() if value)
        if image.size != (64, 64) or 'A' not in image.getbands():
            invalid.append(item)
        if bounds is None or occupied < 32 or bounds[2] - bounds[0] < 8 or bounds[3] - bounds[1] < 8:
            placeholders.append(item)
        digest = hashlib.sha256(image.convert('RGBA').tobytes()).hexdigest()
        hashes[digest].append(item['id'])
        samples.append(dict(item, occupiedPixels=occupied, bounds=bounds, rgbaPixelSha256=digest))
duplicates = [ids for ids in hashes.values() if len(ids) > 1]
report = dict(kind='offline-original-item-icon-pixel-audit', checked=len(items), missing=missing,
              invalid=invalid, possiblePlaceholders=placeholders, duplicatePixelGroups=duplicates,
              samples=samples, visualStyleReview='pending', unitySizeReview='pending')
output = root / 'output/beast-overhaul/original-icon-audit.json'
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps(report, indent=2), encoding='utf-8')
print(json.dumps({key: len(report[key]) for key in ('missing', 'invalid', 'possiblePlaceholders', 'duplicatePixelGroups')}
                 | dict(checked=len(items))))
