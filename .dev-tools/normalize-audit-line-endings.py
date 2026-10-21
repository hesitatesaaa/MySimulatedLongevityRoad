"""Keep existing tracked files' line ending convention after audit edits."""
import subprocess
import difflib
from pathlib import Path
root=Path(__file__).resolve().parents[1]
git=['git','-c','safe.directory='+root.as_posix()]
names=subprocess.check_output(git+['diff','--name-only','-z'],cwd=root).decode().split('\0')
for name in filter(None,names):
    path=root/name
    if not path.is_file():continue
    baseline=subprocess.check_output(git+['show','HEAD:'+name],cwd=root)
    old=baseline.decode('utf-8-sig').splitlines(keepends=True)
    old_values=[line.rstrip('\r\n') for line in old]
    new=path.read_text(encoding='utf-8-sig').splitlines()
    result=[]
    for tag,a,b,c,d in difflib.SequenceMatcher(a=old_values,b=new,autojunk=False).get_opcodes():
        if tag=='equal':result.extend(old[a:b])
        else:
            neighbor=old[min(a,len(old)-1)] if old else '\n'
            ending='\r\n' if neighbor.endswith('\r\n') else '\n'
            result.extend(line+ending for line in new[c:d])
    data=''.join(result).encode('utf-8')
    if baseline.startswith(b'\xef\xbb\xbf'):data=b'\xef\xbb\xbf'+data
    path.write_bytes(data)
print('Preserved existing line endings')
