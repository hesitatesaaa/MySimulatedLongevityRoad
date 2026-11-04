# -*- coding: utf-8 -*-
"""Generate docs/CODE-METRICS.json from the current source tree.

Usage: python tools/report-code-metrics.py [--compile-log PATH]
Counts are the single source of truth for file/line/warning figures quoted in docs.
"""
import os, io, re, json, sys, datetime

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CODE = os.path.join(ROOT, 'code', 'MySimulatedLongevityRoad')
OUT = os.path.join(ROOT, 'docs', 'CODE-METRICS.json')

def read(p):
    with io.open(p, 'r', encoding='utf-8', errors='replace') as f:
        return f.read()

def read_any_encoding(p):
    with open(p, 'rb') as f:
        b = f.read()
    if b[:2] == b'\xff\xfe':
        return b.decode('utf-16-le', errors='replace')
    if b[:2] == b'\xfe\xff':
        return b.decode('utf-16-be', errors='replace')
    if b[:3] == b'\xef\xbb\xbf':
        return b[3:].decode('utf-8', errors='replace')
    return b.decode('utf-8', errors='replace')

cs_files = []
for dp, _, fs in os.walk(CODE):
    for fn in fs:
        if fn.endswith('.cs'):
            cs_files.append(os.path.join(dp, fn))

total_lines = 0
per_dir = {}
largest = []
catch_total = 0
catch_exc = 0
empty_catch = 0
error_calls = 0
probe_calls = 0
raw_log = 0
access_tools = 0
get_field = 0
get_property = 0
def count(pat, text):
    return len(re.findall(pat, text))

for p in cs_files:
    t = read(p)
    n = t.count('\n') + (1 if t and not t.endswith('\n') else 0)
    if t == '':
        n = 0
    total_lines += n
    d = os.path.relpath(os.path.dirname(p), CODE)
    d = '.' if d == '.' else d.split(os.sep)[0]
    per_dir[d] = per_dir.get(d, 0) + n
    largest.append((os.path.relpath(p, CODE).replace(os.sep, '/'), n))
    catch_total += count(r'catch\s*(\(|\{)', t)
    catch_exc += count(r'catch\s*\(\s*(System\.)?Exception', t)
    empty_catch += t.count('empty-catch-code-')
    error_calls += count(r'MclslDiagnostics\.Error\(', t)
    probe_calls += count(r'MclslDiagnostics\.Probe\(', t)
    if os.path.basename(p) != 'MclslDiagnostics.cs':
        raw_log += count(r'Debug\.Log(Error|Warning)?\s*\(', t)
    access_tools += count(r'AccessTools\.', t)
    get_field += count(r'\.GetField\(', t)
    get_property += count(r'\.GetProperty\(', t)

largest.sort(key=lambda x: x[1], reverse=True)

# version
version = None
mod = os.path.join(ROOT, 'mod.json')
if os.path.exists(mod):
    try:
        version = json.loads(read(mod)).get('version')
    except Exception:
        pass

compile_error = compile_warning = None
if '--compile-log' in sys.argv:
    i = sys.argv.index('--compile-log')
    logp = sys.argv[i + 1] if i + 1 < len(sys.argv) else os.path.join(ROOT, 'build', '_compile-check', 'compile.log')
    if os.path.exists(logp):
        lt = read_any_encoding(logp)
        compile_error = count(r'(?im):\s*error ', lt)
        compile_warning = count(r'(?im):\s*warning ', lt)

data = {
    'generatedAt': datetime.datetime.now().astimezone().isoformat(timespec='seconds'),
    'version': version,
    'scope': 'code/MySimulatedLongevityRoad',
    'sourceFiles': len(cs_files),
    'totalLines': total_lines,
    'linesPerTopDir': dict(sorted(per_dir.items(), key=lambda kv: kv[1], reverse=True)),
    'filesOver500Lines': sum(1 for _, n in largest if n >= 500),
    'filesOver800Lines': sum(1 for _, n in largest if n >= 800),
    'largestFiles': [{'file': f, 'lines': n} for f, n in largest[:10]],
    'catchTotal': catch_total,
    'catchException': catch_exc,
    'emptyCatchCode': empty_catch,
    'diagnosticsErrorCalls': error_calls,
    'diagnosticsProbeCalls': probe_calls,
    'rawDebugLogOutsideDiagnostics': raw_log,
    'reflection': {
        'accessTools': access_tools,
        'getField': get_field,
        'getProperty': get_property,
    },
    'compileError': compile_error,
    'compileWarning': compile_warning,
}

os.makedirs(os.path.dirname(OUT), exist_ok=True)
with io.open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    json.dump(data, f, ensure_ascii=False, indent=2)
    f.write('\n')
print('wrote', OUT)
print(json.dumps({k: data[k] for k in ('sourceFiles','totalLines','filesOver500Lines','filesOver800Lines','catchTotal','catchException','emptyCatchCode','diagnosticsErrorCalls','diagnosticsProbeCalls','rawDebugLogOutsideDiagnostics','compileError','compileWarning')}, ensure_ascii=False))
