"""Prioritize inspected missing poses; never generate or approve images."""
import importlib.util
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('authoring', ROOT / 'tools/actor-redraw.py')
A = importlib.util.module_from_spec(spec)
spec.loader.exec_module(A)
jobs = A.load_json(A.WORK / 'balanced-resume-after-quota.jobs.json')
priority = ['mclsl_beast_qilin', 'mclsl_beast_zhuque', 'mclsl_beast_qilin_yuan',
            'mclsl_beast_moqilin_yuan', 'mclsl_beast_xuanwu_yuan']
jobs = [j for j in jobs if not A.within(A.WORK, j['output']).exists()]
jobs.sort(key=lambda j: priority.index(j['actor']) if j['actor'] in priority else len(priority))
for job in jobs:
    if job['actor'] == 'mclsl_beast_zhuque' and job['action'] == 'walk' and job['index'] == 0:
        job['prompt'] += (' Both raised wings must fit fully inside x70..1180 and y70..1180 of the source canvas. '
                          'Bend the shoulder, elbow and wrist naturally; use a compact half-spread wing pose. '
                          'Keep the reference body size; never extend a feather onto any canvas edge.')
        Path(job['promptFile']).write_text(job['prompt'] + '\n', encoding='utf-8')
A.save_json(A.WORK / 'balanced-resume-wave-004.jobs.json', jobs)
print(__import__('json').dumps({'jobs': len(jobs), 'first': jobs[0]}, ensure_ascii=True))
