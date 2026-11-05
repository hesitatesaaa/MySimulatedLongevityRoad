# -*- coding: utf-8 -*-
"""Quality ratchet: fail when code-quality metrics regress past the agreed limits.

Usage:
    python tools/code-quality-ratchet.py            # enforce limits
    python tools/code-quality-ratchet.py --report    # print metrics, never fail
Reads docs/CODE-METRICS.json (run tools/report-code-metrics.py first).
"""
import io
import json
import os
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
METRICS = os.path.join(ROOT, 'docs', 'CODE-METRICS.json')

# 阶段目标（只降不升）：达到后继续下调并提交。
LIMITS = {
    'catchExceptionUnrouted': 200,
    'emptyCatchCode': 0,
    'longParams6': 90,
    'longLinesOver200': 200,
    'duplicateBlocks': 10,
}


def load():
    if not os.path.exists(METRICS):
        print('missing', METRICS, '- run tools/report-code-metrics.py first')
        sys.exit(2)
    with io.open(METRICS, 'r', encoding='utf-8') as f:
        return json.load(f)


def value_of(m, key):
    if key == 'catchExceptionUnrouted':
        return m.get('catchExceptionUnrouted')
    if key == 'emptyCatchCode':
        return m.get('emptyCatchCode')
    if key == 'longParams6':
        return m.get('longParams6')
    if key == 'longLinesOver200':
        return m.get('longLinesOver200')
    if key == 'duplicateBlocks':
        return m.get('duplicateBlocks')
    return m.get(key)


def main():
    report_only = '--report' in sys.argv
    m = load()
    rows = []
    failed = []
    for key, limit in LIMITS.items():
        cur = value_of(m, key)
        if cur is None:
            continue
        ok = cur <= limit
        rows.append((key, cur, limit, ok))
        if not ok:
            failed.append((key, cur, limit))
    width = max(len(r[0]) for r in rows) if rows else 10
    print('%-*s  current  limit  status' % (width, 'metric'))
    for key, cur, limit, ok in rows:
        print('%-*s  %7s  %5s  %s' % (width, key, cur, limit, 'OK' if ok else 'FAIL'))
    if report_only:
        return 0
    if failed:
        print('\nRATCHET FAILED:')
        for key, cur, limit in failed:
            print('  %s = %s (> %s)' % (key, cur, limit))
        return 1
    print('\nRATCHET OK')
    return 0


if __name__ == '__main__':
    sys.exit(main())
