"""Check shipped locales, config callbacks and literal localization display paths."""
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def main():
    errors = []
    catalogs = []
    for name in ('ch', 'cz'):
        data = json.loads((ROOT / 'Locales' / (name + '.json')).read_text(encoding='utf-8-sig'))
        catalogs.append(data)
        for key, text in data.items():
            if not isinstance(text, str) or not text.strip() or '\ufffd' in text or '\0' in text:
                errors.append(f'{name}: invalid translation: {key}')
            elif re.fullmatch(r'(?:mclsl_|trait_Mclsl)\w+', text, re.I):
                errors.append(f'{name}: exposed key: {key}')
    if catalogs[0] != catalogs[1]:
        errors.append('cz must use the same simplified Chinese catalog as ch')
    sources = {p: p.read_text(encoding='utf-8-sig') for p in (ROOT / 'code').rglob('*.cs')}
    known = set(catalogs[0])
    for source in sources.values():
        known.update(re.findall(r'RegisterKey\(\s*"([^"]+)"', source))
    config_source = next(s for p, s in sources.items() if p.name == 'MclslConfigLocalization.cs')
    for key in re.findall(r'Register\("([^"]+)"', config_source):
        known.update((key, key + '_Description'))
    beast_source = next(s for p, s in sources.items() if p.name == 'MclslBeastConfig.cs')
    generated_config = set(re.findall(r'Add(?:Switch|Slider)\(config,items,"([^"]+)"', beast_source))
    lookups = 0
    for path, source in sources.items():
        for key in re.findall(r'LocalizedTextManager\.(?:getText|stringExists)\(\s*"([^"]+)"', source):
            if key.lower().startswith(('mclsl', 'trait_mclsl')):
                lookups += 1
                if key not in known:
                    errors.append(f'{path.relative_to(ROOT)}: missing literal key: {key}')
    config = json.loads((ROOT / 'default_config.json').read_text(encoding='utf-8-sig'))
    for item in config['ConfigItems']:
        for field in ('Name', 'Description'):
            if field not in item and item['Id'] in generated_config:
                continue
            if not item[field].strip() or '\ufffd' in item[field] or '\0' in item[field]:
                errors.append(f'{item["Id"]}: invalid {field}')
        callback = item.get('Callback')
        if callback:
            owner, method = callback.split(':')
            candidates = [s for s in sources.values() if re.search(r'\bclass\s+' + re.escape(owner) + r'\b', s)]
            if not any(re.search(r'\b' + re.escape(method) + r'\s*\(', s) for s in candidates):
                errors.append(f'{item["Id"]}: missing callback: {callback}')
    print(json.dumps(dict(localeEntries=len(catalogs[0]), sourceFiles=len(sources),
                         literalDisplayLookups=lookups, configItems=len(config['ConfigItems']),
                         errors=errors, dynamicUiVerified=False), ensure_ascii=True))
    if errors:
        raise SystemExit(1)


if __name__ == '__main__':
    main()
