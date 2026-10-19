"""Export an existing heartbeat safely through Windows console transports.

Only produces arguments for the app automation tool; never edits automation.toml.
"""
import argparse
import hashlib
import json
import tomllib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CANONICAL = ROOT / 'resources' / 'automation-0-6-0.prompt.md'

def validate_text(text):
    if '\ufffd' in text or '\x00' in text:
        raise ValueError('Refuse damaged Unicode automation text')
    if not text.strip():
        raise ValueError('Refuse empty automation text')
    return text

def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--toml', type=Path, required=True)
    p.add_argument('--rrule')
    p.add_argument('--verify', action='store_true')
    args = p.parse_args()
    current = tomllib.loads(args.toml.read_text(encoding='utf-8-sig', errors='strict'))
    expected = validate_text(CANONICAL.read_text(encoding='utf-8', errors='strict').strip())
    if args.verify:
        actual = validate_text(current['prompt'])
        if actual != expected:
            raise ValueError('Saved prompt differs from canonical UTF-8 prompt')
        print(json.dumps(dict(verified=True, promptSha256=hashlib.sha256(actual.encode('utf-8')).hexdigest(), status=current['status'], rrule=current['rrule']), ensure_ascii=True))
        return
    payload = dict(mode='update', id=current['id'], kind='heartbeat', destination='thread',
                   targetThreadId=current['target_thread_id'], name=validate_text(current['name']),
                   prompt=expected, status=current['status'], rrule=args.rrule or current['rrule'])
    if 'notification_policy' in current:
        payload['notificationPolicy'] = current['notification_policy']
    # ASCII-only JSON survives legacy console code pages; JSON.parse restores Unicode.
    print(json.dumps(payload, ensure_ascii=True))

if __name__ == '__main__':
    main()
