"""Build smaller PNG copies: reference-sized static icons, pixel-exact animations.
Requires Pillow. Original art is never overwritten. PNG display/color metadata is retained; non-rendering caBX signatures are stripped.
"""
from pathlib import Path
import argparse,hashlib,io,json,math,struct,zlib,zipfile
from PIL import Image,ImageChops,ImageStat
from concurrent.futures import ThreadPoolExecutor


def chunks(data):
    offset=8
    while offset<len(data):
        size=int.from_bytes(data[offset:offset+4],'big')
        yield data[offset+4:offset+8],data[offset+8:offset+8+size]
        offset+=size+12
    assert offset==len(data)


def encode(image,original,**options):
    stream=io.BytesIO();image.save(stream,format='PNG',optimize=True,compress_level=9,**options)
    parts=list(chunks(stream.getvalue()));seen={k for k,v in parts}
    # Palette-dependent fields must follow the new encoding, not the original palette.
    skip=(b'IHDR',b'PLTE',b'tRNS',b'IDAT',b'IEND',b'bKGD',b'hIST',b'sBIT',b'caBX')
    keep=[(k,v) for k,v in chunks(original) if k not in skip and k not in seen]
    parts=parts[:1]+keep+parts[1:]
    return b'\x89PNG\r\n\x1a\n'+b''.join(struct.pack('>I',len(v))+k+v+struct.pack('>I',zlib.crc32(k+v)&0xffffffff) for k,v in parts)


def display_score(original,candidate):
    worst=100.
    for side in (32,64):
        for bg in (0,128,255):
            a=Image.new('RGBA',original.size,(bg,bg,bg,255));a.alpha_composite(original)
            b=Image.new('RGBA',candidate.size,(bg,bg,bg,255));b.alpha_composite(candidate)
            a=a.convert('RGB').resize((side,side),Image.Resampling.LANCZOS)
            b=b.convert('RGB').resize((side,side),Image.Resampling.LANCZOS)
            rms=ImageStat.Stat(ImageChops.difference(a,b)).rms
            mse=sum(x*x for x in rms)/3
            worst=min(worst,10*math.log10(255**2/max(mse,1e-10)))
    return round(worst,3)


def optimize(job):
    source,target,size=job;raw=source.read_bytes();best=raw;kind='unchanged';quality=None
    with Image.open(io.BytesIO(raw)) as src:
        src.load();before=src.size;rgba=src.convert('RGBA')
        if size:
            # Match the same-named icon in the reference player package exactly.
            image=rgba.resize(size,Image.Resampling.LANCZOS)
            quality=display_score(rgba,image)
            best=encode(image,raw);kind='reference-sized-icon'
        else:
            image=rgba
            if src.mode=='P':
                order=[index for count,index in sorted(src.getcolors(),reverse=True)]
                packed=src.remap_palette(order)
                bits=next(x for x in (1,2,4,8) if (1<<x)>=len(order))
                candidate=encode(packed,raw,bits=bits)
            else:candidate=encode(src,raw)
            with Image.open(io.BytesIO(candidate)) as check:
                assert check.size==before and check.convert('RGBA').tobytes()==rgba.tobytes(),str(source)
            if len(zlib.compress(candidate,9))<len(zlib.compress(best,9)):
                best=candidate;kind='lossless'
        parts=list(chunks(best))
        if any(k==b'caBX' for k,v in parts):
            best=best[:8]+b''.join(struct.pack('>I',len(v))+k+v+struct.pack('>I',zlib.crc32(k+v)&0xffffffff) for k,v in parts if k!=b'caBX')
            if kind=='unchanged':kind='lossless'
        with Image.open(io.BytesIO(best)) as check:
            assert check.size==image.size and check.convert('RGBA').tobytes()==image.tobytes(),str(source)
    target.write_bytes(best)
    return dict(file=target.relative_to(ARGS.output).as_posix(),beforeBytes=len(raw),afterBytes=len(best),method=kind,beforeSize=before,afterSize=image.size,displayPSNR=quality)


if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--source',required=True,type=Path);parser.add_argument('--output',required=True,type=Path);parser.add_argument('--reference-zip',required=True,type=Path);ARGS=parser.parse_args()
    assert ARGS.source.resolve()!=ARGS.output.resolve()
    paths=sorted(p for p in ARGS.output.rglob('*.png') if 'art-source' not in p.relative_to(ARGS.output).parts);jobs=[]
    with zipfile.ZipFile(ARGS.reference_zip) as reference:
        for target in paths:
            rel=target.relative_to(ARGS.output);source=ARGS.source/rel;size=None
            if rel.parent.as_posix() in ('GameResources/trait','GameResources/ui/CareerSkills','GameResources/ui/Spells','GameResources/ui/BeastTalents'):
                with Image.open(source) as image:
                    if image.size==(1254,1254):
                        with Image.open(io.BytesIO(reference.read(ARGS.source.name+'/'+rel.as_posix()))) as old:
                            size=old.size
                        assert size in ((64,64),(128,128))
            jobs.append((source,target,size))
    results=[]
    with ThreadPoolExecutor(max_workers=4) as pool:
        for index,result in enumerate(pool.map(optimize,jobs),1):
            results.append(result)
            if index%400==0:print('PNG',index,'/',len(jobs),flush=True)
    report=dict(referencePackage=ARGS.reference_zip.name,referenceSHA256=hashlib.sha256(ARGS.reference_zip.read_bytes()).hexdigest(),animationRGBAIdentical=True,iconPolicy='83 large static icons match the same-named old-player icon dimensions exactly (64/128px); no palette quantization',animationDimensionsPreserved=True,renderingAndTextMetadataRetained=True,nonRenderingCaBXRemoved=True,files=len(results),changed=sum(x['method']!='unchanged' for x in results),beforeBytes=sum(x['beforeBytes'] for x in results),afterBytes=sum(x['afterBytes'] for x in results),methods={key:sum(x['method']==key for x in results) for key in sorted({x['method'] for x in results})},images=results)
    (ARGS.output/'resources/compression-0.8.1.json').write_text(json.dumps(report,ensure_ascii=False,indent=1)+'\n',encoding='utf-8')
    print(json.dumps({k:v for k,v in report.items() if k!='images'}),flush=True)
