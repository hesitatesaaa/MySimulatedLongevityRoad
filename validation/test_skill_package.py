# -*- coding: utf-8 -*-
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import skill_package_audit as audit  # noqa: E402


def _skill_dir():
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    return os.path.join(root, 'skills', 'token-efficient-workflow')


def test_skill_files_present():
    d = _skill_dir()
    assert os.path.isdir(d), d
    for rel in ('SKILL.md', 'agents/openai.yaml', 'references/specialized.md'):
        assert os.path.isfile(os.path.join(d, rel)), rel


def test_audit_within_budget():
    r = audit.audit_skill(_skill_dir(), 800, 1200)
    assert r['ok'], r['problems']


def test_all_files_utf8():
    r = audit.audit_skill(_skill_dir(), 10 ** 9, 10 ** 9)
    assert r['ok'], r['problems']
