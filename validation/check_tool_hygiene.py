# -*- coding: utf-8 -*-
"""Report-only hygiene scan for tools/*.py.

Flags Python scripts without an `if __name__ == "__main__"` guard and lines that
contain hardcoded absolute Windows paths. This is informational (exit 0); wiring
it into CI as a hard gate would require fixing every listed script first.
"""
import json
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TOOLS = os.path.join(ROOT, 'tools')

report = {'missingMainGuard': [], 'hardcodedPaths': []}
for fn in sorted(os.listdir(TOOLS)):
    if not fn.endswith('.py'):
        continue
    p = os.path.join(TOOLS, fn)
    try:
        with open(p, 'r', encoding='utf-8', errors='replace') as f:
            text = f.read()
    except OSError:
        continue
    if '__name__' not in text:
        report['missingMainGuard'].append(fn)
    for i, line in enumerate(text.splitlines(), 1):
        if re.search(r'[A-Za-z]:\\', line) and ('"' in line or "'" in line):
            report['hardcodedPaths'].append({'file': fn, 'line': i, 'text': line.strip()[:120]})

print(json.dumps(report, ensure_ascii=False, indent=2))
