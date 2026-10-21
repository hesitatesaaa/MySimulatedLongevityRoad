"""Verify row orientation, alpha/RGB preservation and metadata exceptions."""
import gzip
import hashlib
import importlib.util
import io
import json
import tempfile
from pathlib import Path
from PIL import Image, PngImagePlugin

spec=importlib.util.spec_from_file_location('package',Path(__file__).with_name('package-release.py'))
package=importlib.util.module_from_spec(spec);spec.loader.exec_module(package)
with tempfile.TemporaryDirectory() as temporary:
    root=Path(temporary)
    entries=[]
    originals={}
    for index in range(3):
        relative=f'GameResources/actors/Souls/Test/frame_{index}.png'
        path=root/relative;path.parent.mkdir(parents=True,exist_ok=True)
        image=Image.new('RGBA',(3+index,5+index))
        pixels=[((x*31+index)%256,(y*41)%256,255 if y==0 else 10,0 if x==0 else 255)
                for y in range(image.height) for x in range(image.width)]
        image.putdata(pixels)
        image.save(path);entries.append((relative,path));originals[relative]=image
    metadata=root/'GameResources/actors/Souls/Test/profile.png'
    info=PngImagePlugin.PngInfo();info.add_text('XML:com.adobe.xmp','preserved metadata')
    Image.new('RGBA',(2,2),(1,2,3,255)).save(metadata,pnginfo=info)
    entries.append(('GameResources/actors/Souls/Test/profile.png',metadata))
    manifest=root/'resources/world-soul-frames.manifest.json';manifest.parent.mkdir(parents=True)
    manifest.write_text(json.dumps(dict(schemaVersion=1,groups={})))
    entries.append(('resources/world-soul-frames.manifest.json',manifest))
    hashes={name:hashlib.sha256(path.read_bytes()).hexdigest() for name,path in entries}
    packed,replacements,stats=package.pack_soul_atlas(entries,{},root)
    assert stats['frames']==3 and stats['pixelIdenticalToExistingPlayerCompression']
    descriptor=json.loads(replacements['resources/world-soul-frames.manifest.json'])['atlas']
    payload=dict(packed)['NativeActorResources/Souls/player-frames.rgba.gz'].read_bytes()
    raw=gzip.decompress(payload)
    assert hashlib.sha256(raw).hexdigest()==descriptor['rgbaSha256']
    width,height=descriptor['canvas']
    atlas=Image.frombytes('RGBA',(width,height),raw).transpose(Image.Transpose.FLIP_TOP_BOTTOM)
    for name,image in originals.items():
        x,y,w,h=descriptor['rectangles'][name[len('GameResources/'):-4]]
        assert atlas.crop((x,height-y-h,x+w,height-y)).tobytes()==image.tobytes()
        assert name not in dict(packed)
    assert 'GameResources/actors/Souls/Test/profile.png' in dict(packed)
    assert all(hashlib.sha256(path.read_bytes()).hexdigest()==hashes[name] for name,path in entries)
print(json.dumps(dict(orientation=True,rgbaIncludingTransparentRgb=True,metadataPreserved=True,sourceFilesUntouched=True)))
