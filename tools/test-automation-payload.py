"""Regression test for Chinese automation text through ASCII-only transport."""
import json
import subprocess
import sys
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
prompt = (ROOT / 'resources/automation-0-6-0.prompt.md').read_text(encoding='utf-8').strip()
with tempfile.TemporaryDirectory() as tmp:
    path = Path(tmp) / 'automation.toml'
    def save(text, identity='0-6-0'):
        fields = dict(id=identity, kind='heartbeat', name='中文任务：独立帧', prompt=text,
                      status='ACTIVE', rrule='FREQ=DAILY;BYHOUR=11', target_thread_id='test-thread')
        path.write_text('\n'.join(k+' = '+json.dumps(v,ensure_ascii=False) for k,v in fields.items()),encoding='utf-8')
    def run(*args):
        return subprocess.run([sys.executable,str(ROOT/'tools/automation-payload.py'),'--toml',str(path),*args],capture_output=True)
    save(prompt)
    r = run()
    assert r.returncode == 0, r.stderr
    assert all(b < 128 for b in r.stdout), 'Transport must contain ASCII bytes only'
    assert json.loads(r.stdout)['prompt'] == prompt, 'Chinese round trip lost data'
    assert run('--verify').returncode == 0
    save(prompt+'\ufffd')
    assert run('--verify').returncode != 0, 'Damaged saved prompt accepted'
    save(prompt+'changed')
    assert run('--verify').returncode != 0, 'Unexpected prompt change accepted'
    beast_prompt=(ROOT/'resources/automation-beast-overhaul.prompt.md').read_text(encoding='utf-8').strip()
    save(beast_prompt, 'automation')
    assert json.loads(run().stdout)['prompt']==beast_prompt
    assert run('--verify').returncode==0
    save(prompt, 'automation')
    assert run('--verify').returncode!=0, 'Other task canonical prompt accepted'
    save(beast_prompt, '0-6-0')
    assert run('--verify').returncode!=0, '0-6-0 canonical safeguard weakened'
    save(prompt, 'unknown-task')
    assert run().returncode!=0, 'Unknown task ID accepted'
print('PASS: ASCII transport, Chinese round trip, exact saved text, damaged/mismatched text rejected')
