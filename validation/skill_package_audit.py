# -*- coding: utf-8 -*-
"""Validate a skill package: structure, UTF-8 encoding, and token budgets.

Usage:
  python validation/skill_package_audit.py --skill-dir skills/token-efficient-workflow \
      --max-skill-tokens 800 --max-runtime-tokens 1200 --json
"""
import argparse
import io
import json
import os
import sys


def _read(p):
    with io.open(p, 'r', encoding='utf-8') as f:
        return f.read()


def _tokens(text):
    try:
        import tiktoken
        enc = tiktoken.get_encoding('cl100k_base')
        return len(enc.encode(text))
    except Exception:
        # rough fallback when tiktoken is unavailable: ~4 chars per token
        return (len(text) + 3) // 4


def audit_skill(skill_dir, max_skill_tokens=800, max_runtime_tokens=1200):
    problems = []
    files = {}
    if not os.path.isdir(skill_dir):
        return {'ok': False, 'problems': ['skill dir not found: ' + skill_dir], 'files': {}}
    for dp, _, fs in os.walk(skill_dir):
        for fn in fs:
            p = os.path.join(dp, fn)
            rel = os.path.relpath(p, skill_dir).replace(os.sep, '/')
            try:
                raw = open(p, 'rb').read()
                text = raw.decode('utf-8')
            except UnicodeDecodeError as e:
                problems.append('not utf-8: %s (%s)' % (rel, e))
                continue
            if '\ufffd' in text:
                problems.append('replacement char in ' + rel)
            if '\x00' in text:
                problems.append('null char in ' + rel)
            files[rel] = {'bytes': len(raw), 'chars': len(text), 'tokens': _tokens(text)}
    if 'SKILL.md' not in files:
        problems.append('missing SKILL.md')
    skill_tokens = files.get('SKILL.md', {}).get('tokens', 0)
    runtime_tokens = sum(v['tokens'] for v in files.values())
    if skill_tokens > max_skill_tokens:
        problems.append('skill tokens %d > %d' % (skill_tokens, max_skill_tokens))
    if runtime_tokens > max_runtime_tokens:
        problems.append('runtime tokens %d > %d' % (runtime_tokens, max_runtime_tokens))
    return {
        'ok': not problems,
        'problems': problems,
        'files': files,
        'skillTokens': skill_tokens,
        'runtimeTokens': runtime_tokens,
        'maxSkillTokens': max_skill_tokens,
        'maxRuntimeTokens': max_runtime_tokens,
    }


def main(argv=None):
    ap = argparse.ArgumentParser()
    ap.add_argument('--skill-dir', required=True)
    ap.add_argument('--max-skill-tokens', type=int, default=800)
    ap.add_argument('--max-runtime-tokens', type=int, default=1200)
    ap.add_argument('--json', action='store_true')
    a = ap.parse_args(argv)
    r = audit_skill(a.skill_dir, a.max_skill_tokens, a.max_runtime_tokens)
    if a.json:
        print(json.dumps(r, ensure_ascii=False, indent=2))
    else:
        print('OK' if r['ok'] else 'FAIL')
        for p in r['problems']:
            print('-', p)
    return 0 if r['ok'] else 1


if __name__ == '__main__':
    sys.exit(main())
