"""Keep identity references stable after production frames are replaced."""
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
baseline = root / 'output/baseline-0.5.4-20261009'
targets = json.loads((root / 'resources/actor-redraw.targets.json').read_text(encoding='utf-8-sig'))
for form in targets['forms']:
    card_path = root / form['referenceCard']
    card = json.loads(card_path.read_text(encoding='utf-8-sig'))
    original = baseline / 'GameResources' / form['resourcePath'] / 'main/idle_0.png'
    if not original.is_file():
        raise ValueError('Baseline identity source missing: ' + str(original))
    card['identityReference'] = original.relative_to(root).as_posix()
    card['plannedCanvas'] = form['canvas']
    card['plannedAnchor'] = form['anchor']
    card['plannedActions'] = form['actions']
    card_path.write_text(json.dumps(card, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print('Updated stable identity references and current action specifications for 62 cards')
