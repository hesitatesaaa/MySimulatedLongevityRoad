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
long_params6 = 0
long_lines200 = 0
long_lines300 = 0
uncached_reflection = 0
guard_swallowed = 0
def count(pat, text):
    return len(re.findall(pat, text))


def count_long_params(text):
    """Count method/ctor declarations with >= 6 parameters (regex heuristic)."""
    total = 0
    for m in re.finditer(r'(?m)^\s*(?:internal|private|public|protected|static|sealed|override|virtual|abstract)[^\n;=]*?\b[A-Za-z_][A-Za-z0-9_]*\s*\(([^)]*)\)\s*$', text):
        params = [x for x in m.group(1).split(',') if x.strip()]
        if len(params) >= 6:
            total += 1
    return total


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
    empty_catch += count(r'MclslDiagnostics\.(?:Probe|Alert|Error)\(\s*"empty-catch-code-', t)
    guard_swallowed += count(r'MclslGuard\.Swallowed\(', t)
    error_calls += count(r'MclslDiagnostics\.Error\(', t)
    probe_calls += count(r'MclslDiagnostics\.Probe\(', t)
    if os.path.basename(p) != 'MclslDiagnostics.cs':
        raw_log += count(r'Debug\.Log(Error|Warning)?\s*\(', t)
    access_tools += count(r'AccessTools\.', t)
    get_field += count(r'\.GetField\(', t)
    get_property += count(r'\.GetProperty\(', t)
    long_params6 += count_long_params(t)
    uncached_reflection += count(r'GetType\(\)\.Get(Field|Property)\(', t)
    for line in t.split('\n'):
        L = len(line)
        if L > 200:
            long_lines200 += 1
        if L > 300:
            long_lines300 += 1

# duplicate 8-line blocks across >=2 files (cross-file duplication signal)
import hashlib
shingle = {}
for p in cs_files:
    ls = [re.sub(r'\s+', ' ', x).strip() for x in read(p).split('\n')]
    idx = [i for i, x in enumerate(ls) if x and not x.startswith('using ') and x not in ('{', '}', '};')]
    for a in range(len(idx) - 7):
        pos = idx[a:a + 7 + 1]
        if pos[-1] - pos[0] > 11:
            continue
        block = '\n'.join(ls[i] for i in pos)
        if sum(1 for i in pos if ';' in ls[i] or '(' in ls[i]) < 5:
            continue
        if len(block) < 140:
            continue
        h = hashlib.md5(block.encode('utf-8')).hexdigest()
        shingle.setdefault(h, set()).add(p)
duplicate_blocks = sum(1 for v in shingle.values() if len(v) >= 2)


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
    'catchExceptionUnrouted': max(0, catch_exc - guard_swallowed),
    'guardSwallowedCalls': guard_swallowed,
    'emptyCatchCode': empty_catch,
    'diagnosticsErrorCalls': error_calls,
    'diagnosticsProbeCalls': probe_calls,
    'rawDebugLogOutsideDiagnostics': raw_log,
    'reflection': {
        'accessTools': access_tools,
        'getField': get_field,
        'getProperty': get_property,
        'uncachedRuntimeLookups': uncached_reflection,
    },
    'longParams6': long_params6,
    'longLinesOver200': long_lines200,
    'longLinesOver300': long_lines300,
    'duplicateBlocks': duplicate_blocks,
    'compileError': compile_error,
    'compileWarning': compile_warning,
}

os.makedirs(os.path.dirname(OUT), exist_ok=True)

# --check: verify the committed metrics match the current tree, ignoring the
# volatile timestamp, so CI can detect drift without failing on every run.
if '--check' in sys.argv:
    if not os.path.exists(OUT):
        print('missing', OUT)
        sys.exit(2)
    with io.open(OUT, 'r', encoding='utf-8') as f:
        existing = json.load(f)
    keys = [k for k in data if k != 'generatedAt']
    drift = [k for k in keys if existing.get(k) != data[k]]
    if drift:
        print('metrics drift:', ', '.join(drift))
        for k in drift:
            print('  %s: committed=%r current=%r' % (k, existing.get(k), data[k]))
        sys.exit(1)
    print('metrics up to date')
    sys.exit(0)

with io.open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    json.dump(data, f, ensure_ascii=False, indent=2)
    f.write('\n')
print('wrote', OUT)
print(json.dumps({k: data[k] for k in ('sourceFiles','totalLines','filesOver500Lines','filesOver800Lines','catchTotal','catchException','emptyCatchCode','diagnosticsErrorCalls','diagnosticsProbeCalls','rawDebugLogOutsideDiagnostics','longParams6','longLinesOver200','longLinesOver300','duplicateBlocks','compileError','compileWarning')}, ensure_ascii=False))
