"""Record the reviewed extra collapse transition without changing existing frames."""
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
path = root / 'resources/actor-redraw.targets.json'
data = json.loads(path.read_text(encoding='utf-8-sig'))
form = next(f for f in data['forms'] if f['id'] == 'mclsl_beast_baihu')
form['actions']['death'].update(count=11, frameOrder=[10, *range(10)])
data['totalFrames'] = sum(c['count'] for f in data['forms'] for c in f['actions'].values())
path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(json.dumps({'baihuDeathFrames': 11, 'totalFrames': data['totalFrames']}))
