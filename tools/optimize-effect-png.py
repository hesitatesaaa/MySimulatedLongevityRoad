"""Lossless effect PNG recompression preserving the original PNG filters and metadata."""
from pathlib import Path
import argparse,io,json,statistics,struct,time,zlib
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
def chunks(data):
    i=8
    while i<len(data):
        n=int.from_bytes(data[i:i+4],'big');yield data[i+4:i+8],data[i+8:i+8+n];i+=n+12

def recompress(data):
    parts=list(chunks(data));scan=zlib.decompress(b''.join(d for t,d in parts if t==b'IDAT'))
    candidates=[data]
    def rebuild(encoded):
        result=data[:8];written=False
        for kind,payload in parts:
            if kind==b'IDAT':
                if written:continue
                payload=encoded;written=True
            result+=struct.pack('>I',len(payload))+kind+payload+struct.pack('>I',zlib.crc32(kind+payload)&0xffffffff)
        return result
    for level in (6,9):
        encoded=zlib.compress(scan,level)
        assert zlib.decompress(encoded)==scan
        candidates.append(rebuild(encoded))
    # Re-select PNG row filters while retaining all original metadata chunks.
    with Image.open(io.BytesIO(data)) as image:
        buffer=io.BytesIO();image.save(buffer,format='PNG',optimize=True,compress_level=9)
    filtered=list(chunks(buffer.getvalue()))
    if filtered[0]==parts[0]:
        candidates.append(rebuild(b''.join(d for t,d in filtered if t==b'IDAT')))
    # Avoid a smaller loose PNG producing a larger distributed player ZIP.
    zip_size=len(zlib.compress(data,9))
    candidates=[c for c in candidates if len(c)<=len(data) and len(zlib.compress(c,9))<=zip_size]
    return min(candidates,key=len)

def decode_ms(items):
    start=time.perf_counter()
    for data in items:
        with Image.open(io.BytesIO(data)) as image:image.load()
    return (time.perf_counter()-start)*1000

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--all-visuals',action='store_true');args=parser.parse_args()
    paths=sorted(p for group in ['Spells','Careers','BeastTalents'] for p in (ROOT/'GameResources/effects'/group).rglob('*.png'))
    if args.all_visuals:
        jobs=json.loads((ROOT/'art-source/trait-redraw-83/jobs.json').read_text(encoding='utf-8'))
        paths+= [ROOT/job['path'] for job in jobs]
        paths+=sorted((ROOT/'GameResources/actors/CareerSpirits').glob('*.png'))
    original=[p.read_bytes() for p in paths];candidate=[recompress(data) for data in original]
    for old,new in zip(original,candidate):
        with Image.open(io.BytesIO(old)) as a,Image.open(io.BytesIO(new)) as b:
            assert a.size==b.size and a.convert('RGBA').tobytes()==b.convert('RGBA').tobytes()
    before=[];after=[]
    for n in range(0 if args.all_visuals else 5):
        if n%2:after.append(decode_ms(candidate));before.append(decode_ms(original))
        else:before.append(decode_ms(original));after.append(decode_ms(candidate))
    old_time=statistics.median(before) if before else None;new_time=statistics.median(after) if after else None
    # Reject an encoding change that makes the local decode proxy materially slower.
    accepted=args.all_visuals or new_time<=old_time*1.05
    if accepted:
        for p,old,new in zip(paths,original,candidate):
            if old!=new:p.write_bytes(new)
    report=dict(files=len(paths),changed=sum(a!=b for a,b in zip(original,candidate)) if accepted else 0,beforeBytes=sum(map(len,original)),candidateBytes=sum(map(len,candidate)),beforeDecodeMs=old_time,candidateDecodeMs=new_time,accepted=accepted,losslessRGBA=True,benchmark='Pillow in-memory decode proxy, not Unity startup measurement')
    report['benchmark']='Not run; lossless pixel verification only' if args.all_visuals else report['benchmark']
    name='visual-compression-report.json' if args.all_visuals else 'effect-compression-report.json'
    (ROOT/'resources'/name).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(report))
