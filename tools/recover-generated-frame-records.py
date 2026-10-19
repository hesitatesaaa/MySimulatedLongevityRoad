"""Recover actor/frame identity from local generation calls, matching output bytes."""
import base64
import hashlib
import json
import re
from datetime import datetime
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'output/actor-redraw'
SESSIONS = Path('C:/Users/Arthur.lu/.codex/sessions')


def main():
    audit = json.loads((WORK / 'generated-images-audit.json').read_text())
    unresolved = {r['sha256']: r for r in audit['files'] if r['status'] == 'unmapped'}
    threads = {Path(r['file']).parent.name for r in unresolved.values()}
    targets = json.loads((ROOT / 'resources/actor-redraw.targets.json').read_text(encoding='utf-8-sig'))
    actor_ids = {f['id'] for f in targets['forms']}
    calls, recovered = {}, []
    for session in SESSIONS.rglob('*.jsonl'):
        if not any(t in session.name for t in threads):
            continue
        generation_calls = []
        for line in session.open(encoding='utf-8'):
            if 'image_gen__imagegen' not in line and 'custom_tool_call_output' not in line and 'function_call_output' not in line:
                continue
            item = json.loads(line)
            payload = item.get('payload', {})
            kind = payload.get('type')
            call_id = payload.get('call_id')
            if kind in ('custom_tool_call', 'function_call'):
                code = payload.get('input', payload.get('arguments', ''))
                if 'image_gen__imagegen' not in code:
                    continue
                prompts = re.findall(r'(?:\bprompt"?\s*:\s*|\b(?:const|let)\s+prompt\s*=\s*)("(?:\\.|[^"\\])*")', code)
                prompt = json.loads(prompts[0]) if len(prompts) == 1 else ''
                actors = set(re.findall(r'mclsl_[a-z0-9_]+', prompt)) & actor_ids
                if not actors:
                    actors = set(re.findall(r'mclsl_[a-z0-9_]+', code)) & actor_ids
                frames = list(dict.fromkeys(re.findall(r'\b(?:idle|walk|death|attack|hit)_\d+\b', prompt)))
                if not frames:
                    frames = [f'{action}_{index}' for action, index in re.findall(r'\b(idle|walk|death|attack|hit)\s+frame\s+(\d+)\b', prompt)]
                reference_frames = list(dict.fromkeys(re.findall(r'\b(?:idle|walk|death|attack|hit)_\d+\b', code)))
                calls[call_id] = dict(prompt=prompt, actors=sorted(actors), frames=frames,
                    referenceFrames=reference_frames, session=str(session), timestamp=item.get('timestamp'), code=code)
                generation_calls.append(calls[call_id])
            elif kind in ('custom_tool_call_output', 'function_call_output') and call_id in calls:
                output = payload.get('output')
                if not isinstance(output, list):
                    continue
                for block in output:
                    if not isinstance(block, dict) or block.get('type') != 'input_image':
                        continue
                    url = block.get('image_url', '')
                    if not isinstance(url, str) or not url.startswith('data:image/png;base64,'):
                        continue
                    digest = hashlib.sha256(base64.b64decode(url.split(',', 1)[1])).hexdigest()
                    if digest not in unresolved:
                        continue
                    if not calls[call_id]['actors']:
                        continue
                    record = dict(calls[call_id], file=unresolved[digest]['file'], sha256=digest)
                    recovered.append(record)
        # Some calls returned only metadata, without embedding their image bytes.
        # A single output in a sequential call's time interval identifies that output.
        already = {r['sha256'] for r in recovered}
        generation_calls.sort(key=lambda c: c['timestamp'])
        for index, call in enumerate(generation_calls):
            start = datetime.fromisoformat(call['timestamp'].replace('Z', '+00:00')).timestamp()
            end = datetime.fromisoformat(generation_calls[index + 1]['timestamp'].replace('Z', '+00:00')).timestamp() if index + 1 < len(generation_calls) else start + 600
            matches = [r for r in unresolved.values() if Path(r['file']).parent.name in session.name
                       and start <= Path(r['file']).stat().st_mtime < min(end, start + 600)]
            if len(matches) == 1 and call['actors'] and matches[0]['sha256'] not in already:
                recovered.append(dict(call, file=matches[0]['file'], sha256=matches[0]['sha256'],
                                      matchingMethod='single-output-in-sequential-generation-interval'))
    output = WORK / 'generated-frame-recovery.json'
    output.write_text(json.dumps(dict(recovered=recovered), ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(unmapped=len(unresolved), recovered=len(recovered), output=str(output))))


if __name__ == '__main__':
    main()
