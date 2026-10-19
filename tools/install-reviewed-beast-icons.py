"""Install explicitly reviewed 64px candidates without altering original art."""
import argparse
import hashlib
import importlib.util
import json
import shutil
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('inspection', ROOT/'tools/beast-resource-inspection.py')
inspection = importlib.util.module_from_spec(spec); spec.loader.exec_module(inspection)
parser = argparse.ArgumentParser()
parser.add_argument('--ids', nargs='+', required=True)
parser.add_argument('--revision', type=int, default=2)
parser.add_argument('--original-revision', type=int, default=0)
parser.add_argument('--prompts', required=True)
args = parser.parse_args()
manifest_path = ROOT/'resources/beast-content.manifest.json'
manifest = json.loads(manifest_path.read_text(encoding='utf-8-sig'))
prompts = {x['id']: x['prompt'] for x in json.loads((ROOT/args.prompts).read_text(encoding='utf-8-sig'))}
ready = []
pixel_hashes = set()
for item in manifest['icons']:
    target = ROOT/item['path']
    if target.exists():
        pixel_hashes.add(inspection.inspect_png(target, (64,64)))
for identity in args.ids:
    item = next(x for x in manifest['icons'] if x['id']==identity)
    if identity not in prompts: raise ValueError('Actual prompt missing: '+identity)
    source = ROOT/f'output/beast-overhaul/icon-candidates/{identity}-v{args.revision}.png'
    suffix = f'-v{args.original_revision}' if args.original_revision else ''
    original = ROOT/f'output/beast-overhaul/generated-originals/{identity}{suffix}.png'
    target = ROOT/item['path']
    if target.exists(): raise FileExistsError(target)
    digest = inspection.inspect_png(source,(64,64))
    if digest in pixel_hashes: raise ValueError('Duplicate visible image: '+identity)
    pixel_hashes.add(digest)
    if not original.exists(): raise FileNotFoundError(original)
    ready.append((item,source,original,target))
now = datetime.now(timezone.utc).isoformat()
for item, source, original, target in ready:
    target.parent.mkdir(parents=True,exist_ok=True)
    shutil.copy2(source,target)
    item['status']='approved'
    item['sha256']=hashlib.sha256(target.read_bytes()).hexdigest()
    item['review']={'kind':'offline-item-art-review','result':'passed','reviewedAtUtc':now,
                   'checked':['subject','category','style','64px-readability','alpha','unique-silhouette'],
                   'unitySizeReview':'pending'}
    item['provenance']={'tool':'built-in-imagegen','source':str(original.relative_to(ROOT)).replace('\\','/'),
                        'sourceSha256':hashlib.sha256(original.read_bytes()).hexdigest(),'prompt':prompts[item['id']],
                        'conversion':{'tool':'System.Drawing','size':[64,64],'visibleBoundsAlpha':16,'contentMaxPixels':56},
                        'installedAtUtc':now}
manifest_path.write_text(json.dumps(manifest,ensure_ascii=True,indent=2),encoding='utf-8')
status_path=ROOT/'resources/beast-overhaul-status.json'
status=json.loads(status_path.read_text(encoding='utf-8-sig'))
status['generatedItemImages']=sum((ROOT/f"output/beast-overhaul/generated-originals/{x['id']}.png").exists() or any((ROOT/'output/beast-overhaul/generated-originals').glob(x['id']+'-v*.png')) for x in manifest['icons'])
status['generatedItemRevisions']=len(list((ROOT/'output/beast-overhaul/generated-originals').glob('*-v*.png')))
status['generatedSpellStageCandidates']=len(list((ROOT/'output/beast-overhaul/generated-spell-originals').glob('*.png')))
status['generatedImages']=status['generatedItemImages']+status['generatedSpellStageCandidates']
status['installedItemIcons']=sum(x.get('status')=='approved' and (ROOT/x['path']).exists() for x in manifest['icons'])
status['missingItemIcons']=sum(not (ROOT/x['path']).exists() for x in manifest['icons'])
status['candidateItemIcons']=status['generatedItemImages']-status['installedItemIcons']
status['imageGeneration']='resumed-built-in';status['releaseReady']=False
status_path.write_text(json.dumps(status,ensure_ascii=True,indent=2),encoding='utf-8')
print(json.dumps({'installed':[x[0]['id'] for x in ready],'remainingItemIcons':status['missingItemIcons'],'unityValidation':'pending'}))
