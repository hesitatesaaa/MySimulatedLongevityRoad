"""Install explicitly reviewed standalone icons without resizing their PNGs."""
import argparse
import hashlib
import json
import shutil
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
def read(p): return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()

def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--actor', action='append', required=True)
    args=p.parse_args()
    manifestPath=ROOT/'resources/actor-frames.manifest.json'
    manifest=read(manifestPath)
    operations=[]
    for actor in args.actor:
        entry=next(x for x in manifest['icons'] if x['actorId']==actor)
        assert not entry['approved'], 'Refuse approved icon overwrite'
        source=ROOT/'output/actor-redraw/icons'/f'{actor}.png'
        proof=read(source.with_suffix('.provenance.json'))
        original=(ROOT/proof['sourceFile']).resolve()
        original.relative_to(ROOT.resolve())
        assert sha(source)==proof['outputSha256']==proof['sourceSha256']==sha(original)
        assert proof['actorId']==actor and proof['action']=='icon'
        review=proof.get('iconFinalReview', {})
        assert (review.get('sourceSha256')==proof['sourceSha256']
                and review.get('identityReferenceCompared') is True
                and review.get('anatomyAndSilhouetteReviewed') is True
                and review.get('hardClipping') is False
                and not review.get('confirmedDefects')), 'Source-bound final icon review required'
        with Image.open(source) as im:
            im.load()
            bounds=im.getchannel('A').point(lambda v:255 if v>=128 else 0).getbbox()
            assert bounds and bounds[0]>0 and bounds[1]>0 and bounds[2]<im.width and bounds[3]<im.height
            canvas=list(im.size)
        destination=ROOT/'GameResources'/(entry['resourcePath']+'.png')
        provenance=ROOT/'resources/actor-provenance'/actor/'icon.json'
        assert not destination.exists() and not provenance.exists(), 'Refuse overwrite'
        entry.update(approved=True,verifiedInGame=False,nativeResolution=True,canvas=canvas,
                     sha256=sha(source),provenance=provenance.relative_to(ROOT).as_posix())
        proof.update(outputFile=destination.relative_to(ROOT).as_posix(),
                     review=dict(identity=True,pixelEdges=True,canvas=True,independentIcon=True))
        operations.append((source,destination,provenance,proof))
    for source,destination,provenance,proof in operations:
        destination.parent.mkdir(parents=True,exist_ok=True)
        provenance.parent.mkdir(parents=True,exist_ok=True)
        shutil.copyfile(source,destination)
        provenance.write_text(json.dumps(proof,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    manifestPath.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(installedIcons=args.actor,gameSizeVerified=False)))

if __name__=='__main__': main()
