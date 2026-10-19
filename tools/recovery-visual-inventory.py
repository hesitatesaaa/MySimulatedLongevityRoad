"""Rebuild a hash-bound recovery inventory without approving or changing artwork."""
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'output/actor-redraw'


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    targets = read(ROOT / 'resources/actor-redraw.targets.json')
    final = read(WORK / 'final-visual-review.json')
    historical = {f['actorId']: f for f in final['reviewedForms']}
    rows, anomalies, forms = [], [], []
    for form in targets['forms']:
        current = []
        start = len(rows)
        for action, spec in form['actions'].items():
            if spec.get('generationRequired') is False:
                continue
            indices = spec.get('frameIndices', spec.get('frameOrder', spec.get('indices', list(range(spec['count'])))))
            for index in indices:
                name = f'{action}_{index}'
                proof = WORK / 'frames' / form['id'] / 'main' / (name + '.provenance.json')
                row = dict(actorId=form['id'], frame=name, reviewed=False)
                try:
                    p = read(proof)
                    source = (ROOT / p['sourceFile']).resolve()
                    source.relative_to(ROOT.resolve())
                    digest = sha(source)
                    row.update(path=source.relative_to(ROOT).as_posix(), sha256=digest)
                    assert digest == p['sourceSha256'], 'source hash mismatch'
                    current.append((name, digest))
                except (OSError, ValueError, KeyError, AssertionError) as error:
                    anomalies.append(dict(actorId=form['id'], frame=name, error=str(error)))
                rows.append(row)
        inventory = '\n'.join(n + ':' + h for n, h in sorted(current))
        digest = hashlib.sha256(inventory.encode()).hexdigest()
        old = historical.get(form['id'])
        valid = bool(old and old['sourceInventorySha256'] == digest and old['frameCount'] == len(current))
        if old and not valid:
            anomalies.append(dict(actorId=form['id'], error='Historical review inventory changed; affected form needs review'))
        if valid:
            for row in rows[start:]:
                row.update(reviewed=True, evidence='final-visual-review.json', reviewer=old.get('reviewer'))
        forms.append(dict(actorId=form['id'], targetFrames=len(rows)-start, historicalReviewValid=valid))
    by_source = {(r.get('path'), r.get('sha256')): [] for r in rows if 'path' in r}
    for row in rows:
        if 'path' in row:
            by_source[(row['path'], row['sha256'])].append(row)
    reports = []
    for report_path in sorted(WORK.glob('recovery-visual-*.json')):
        if report_path.name == 'recovery-visual-inventory.json':
            continue
        report = read(report_path)
        reports.append(report_path.relative_to(ROOT).as_posix())
        # Worker reports may use frames/images/reviewedFrames; never infer visual review from a file list.
        viewed = report.get('viewed', report.get('reviewedFrames', report.get('frames', report.get('images', []))))
        if not isinstance(viewed, list):
            anomalies.append(dict(report=str(report_path), error='Unsupported worker review schema'))
            continue
        for item in viewed:
            if not isinstance(item, dict):
                continue
            if item.get('viewed') is False or item.get('reviewed') is False:
                continue
            path = item.get('sourceFile', item.get('sourcePath', item.get('path', ''))).replace('\\', '/')
            if Path(path).is_absolute():
                path = Path(path).resolve().relative_to(ROOT).as_posix()
            digest = item.get('sourceSha256', item.get('sha256'))
            for row in by_source.get((path, digest), []):
                row.update(reviewed=True, evidence=report_path.relative_to(ROOT).as_posix(), reviewer=report.get('reviewer', report_path.stem))
    reviewed = {r['sha256'] for r in rows if r.get('reviewed') and 'sha256' in r}
    unique = {r['sha256'] for r in rows if 'sha256' in r}
    output = dict(scope='0.6.0-only', updatedAtUtc=datetime.now(timezone.utc).isoformat(),
                  targetSlots=len(rows), uniqueSourceImages=len(unique), reviewedUniqueImages=len(reviewed),
                  unreviewedUniqueImages=len(unique-reviewed), missingSlots=sum('path' not in r for r in rows),
                  forms=forms, frames=rows, anomalies=anomalies, workerReports=reports,
                  historicalIconsRecorded=len(final['reviewedIcons']), iconsRevalidated=False,
                  passed=False, releaseReady=False, verifiedInGame=False)
    assert output['reviewedUniqueImages'] + output['unreviewedUniqueImages'] == output['uniqueSourceImages']
    path = WORK / 'recovery-visual-inventory.json'
    temporary = path.with_suffix('.tmp')
    temporary.write_text(json.dumps(output, ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
    temporary.replace(path)
    print(json.dumps({k: output[k] for k in ('targetSlots', 'uniqueSourceImages', 'reviewedUniqueImages', 'unreviewedUniqueImages', 'missingSlots')}))
    print('anomalies=' + str(len(anomalies)))


if __name__ == '__main__':
    main()
