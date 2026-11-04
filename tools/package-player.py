"""Build and verify a local NML source-player ZIP, excluding authoring drafts."""
from pathlib import Path
import argparse,hashlib,json,zipfile
p=argparse.ArgumentParser();p.add_argument('--output',required=True);p.add_argument('--root',type=Path);args=p.parse_args()
r=args.root.resolve() if args.root else Path(__file__).resolve().parents[1]
files=sorted(f for f in r.rglob('*') if f.is_file() and not any(x in ('art-source','build','bin','obj','__pycache__','.git') for x in f.relative_to(r).parts) and f.name!='FILE-MANIFEST.json')
mp=r/'FILE-MANIFEST.json';m=json.loads(mp.read_text(encoding='utf-8-sig'));m['unityRetested']=False;m['releaseVersion']=json.loads((r/'mod.json').read_text(encoding='utf-8-sig'))['version'];m['files']={f.relative_to(r).as_posix():{'bytes':f.stat().st_size,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()} for f in files};mp.write_text(json.dumps(m,ensure_ascii=False,indent=1)+'\n',encoding='utf-8');files.append(mp)
out=r.parent/'build'/args.output;out.parent.mkdir(parents=True,exist_ok=True)
with zipfile.ZipFile(out,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
 for f in files:z.write(f,r.name+'/'+f.relative_to(r).as_posix())
with zipfile.ZipFile(out) as z:
 assert z.testzip() is None
 for rel,meta in m['files'].items():assert hashlib.sha256(z.read(r.name+'/'+rel)).hexdigest()==meta['sha256'],rel
sha=hashlib.sha256(out.read_bytes()).hexdigest();out.with_suffix('.sha256.txt').write_text(sha+'  '+out.name+'\n',encoding='utf-8')
print('CRC/hash PASS:',out,'bytes:',out.stat().st_size);print('SHA256:',sha)