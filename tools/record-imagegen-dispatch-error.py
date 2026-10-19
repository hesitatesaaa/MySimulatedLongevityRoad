"""Persist a failed built-in request and stop dispatching after provider failure."""
import argparse
import importlib.util
import os
import re
import time
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('authoring', ROOT / 'tools/actor-redraw.py')
A = importlib.util.module_from_spec(spec)
spec.loader.exec_module(A)
parser = argparse.ArgumentParser()
parser.add_argument('--jobs', required=True)
parser.add_argument('--index', required=True, type=int)
parser.add_argument('--error', required=True)
args = parser.parse_args()
file = A.within(A.WORK, args.jobs)
job = A.load_json(file)[args.index]
quota = 'usage_limit_reached' in args.error
record = {'jobIndex': args.index, 'actor': job['actor'], 'action': job['action'],
          'index': job['index'], 'artDirectionRevision': job['artDirectionRevision'],
          'completedAtUtc': datetime.now(timezone.utc).isoformat(), 'exitCode': 1,
          'error': args.error, 'imageGenerated': False, 'continueDispatch': False}
match = re.search(r'resets_at\D{0,16}(\d+)', args.error)
if quota and match:
    record['retryAfterUtc'] = datetime.fromtimestamp(int(match[1]), timezone.utc).isoformat()
lock = A.within(A.WORK, 'pipeline.control.lock')
for attempt in range(100):
    try:
        descriptor = os.open(lock, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
        break
    except FileExistsError:
        time.sleep(0.1)
else:
    raise RuntimeError('Dispatch-control lock did not become available')
try:
    control = A.load_json(A.WORK / 'pipeline.control.json')
    control.update(stopDispatch=True, reason=('built-in-imagegen-usage-limit-reached' if quota else 'built-in-imagegen-request-failed'))
    if quota:
        control['quotaRecordedAtUtc'] = record['completedAtUtc']
    if record.get('retryAfterUtc'):
        control['retryAfterUtc'] = record['retryAfterUtc']
    A.save_json(A.WORK / 'pipeline.control.json', control)
finally:
    os.close(descriptor)
    lock.unlink()
A.save_json(A.WORK / 'batch-results' / file.stem / f'{args.index:05d}.json', record)
print(__import__('json').dumps(record, ensure_ascii=True))
