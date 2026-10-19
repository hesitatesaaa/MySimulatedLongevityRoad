"""Publish validated 0.6.0 packages to the explicitly authorized GitHub repository."""
import argparse
import hashlib
import json
import os
import subprocess
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
REPOSITORY = 'hesitatesaaa/MySimulatedLongevityRoad'
API = 'https://api.github.com/repos/' + REPOSITORY


def credential():
    token = os.environ.get('GH_TOKEN') or os.environ.get('GITHUB_TOKEN')
    if token:
        return token
    environment = dict(os.environ, GIT_TERMINAL_PROMPT='0', GCM_INTERACTIVE='Never')
    response = subprocess.run(['git', 'credential', 'fill'], input='protocol=https\nhost=github.com\n\n',
        text=True, capture_output=True, env=environment, timeout=30)
    values = dict(line.split('=', 1) for line in response.stdout.splitlines() if '=' in line)
    if not values.get('password'):
        raise RuntimeError('GitHub command-line authentication unavailable; sign in through Git Credential Manager or gh.')
    return values['password']


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--package-directory')
    parser.add_argument('--check-auth', action='store_true')
    args = parser.parse_args()
    token = credential()

    def request(url, method='GET', data=None, headers=None):
        parsed = urllib.parse.urlparse(url)
        assert parsed.scheme == 'https' and parsed.hostname in ('api.github.com', 'uploads.github.com')
        assert parsed.path.startswith('/repos/' + REPOSITORY)
        prepared = {'Authorization': 'Bearer ' + token, 'Accept': 'application/vnd.github+json',
                    'User-Agent': 'MySimulatedLongevityRoad-release', 'X-GitHub-Api-Version': '2022-11-28'}
        if headers:
            prepared.update(headers)
        if isinstance(data, dict):
            data = json.dumps(data, ensure_ascii=True).encode()
            prepared['Content-Type'] = 'application/json'
        with urllib.request.urlopen(urllib.request.Request(url, data=data, headers=prepared, method=method), timeout=1800) as response:
            return json.load(response)

    repository = request(API)
    if not repository.get('permissions', {}).get('push'):
        raise RuntimeError('Authenticated credential lacks repository push access')
    if args.check_auth:
        print(json.dumps(dict(repository=REPOSITORY, push=True, visibility=repository['visibility'])))
        return
    if not args.package_directory:
        parser.error('--package-directory is required for publication')
    directory = Path(args.package_directory).resolve()
    directory.relative_to(ROOT)
    record = json.loads((directory / 'package-record.json').read_text())
    branch = request(API + '/branches/0.6.0')
    assert branch['commit']['sha'] == record['commit'], 'Remote branch must match the packaged release commit'
    uploads = []
    for asset in record['archives']:
        path = directory / asset['name']
        assert path.resolve().parent == directory
        assert path.stat().st_size == asset['bytes']
        with path.open('rb') as stream:
            assert hashlib.file_digest(stream, 'sha256').hexdigest() == asset['sha256']
        if asset['bytes'] < 2_000_000_000:
            uploads.append(path)
    uploads += [directory / 'SHA256SUMS.txt', directory / 'package-record.json']
    restore = directory / 'README-download.txt'
    restore.write_text('玩家包选择 player，开发者包选择 developer。\n若有 .part001 等分卷，按编号顺序以二进制拼接成原 ZIP，然后核对 SHA256SUMS.txt 再解压。\n仅适用于新存档。\n', encoding='utf-8')
    uploads.append(restore)
    release = None
    try:
        release = request(API + '/releases/tags/v0.6.0')
    except urllib.error.HTTPError as error:
        if error.code != 404:
            raise
        drafts = [r for r in request(API + '/releases?per_page=100') if r['tag_name'] == 'v0.6.0']
        assert len(drafts) <= 1
        release = drafts[0] if drafts else None
    if release and (not release['draft'] or release['target_commitish'] != record['commit']):
        raise RuntimeError('v0.6.0 already exists; refusing to overwrite an existing release')
    if release is None:
        release = request(API + '/releases', method='POST', data=dict(tag_name='v0.6.0', target_commitish=record['commit'],
            name='0.6.0', draft=True, prerelease=False,
            body='接入 62 套形态、1374 帧动画、38 个专用图标及已采纳的最新修复。\n\n提供玩家包、完整开发者包及 SHA256 校验，仅适用于新存档。\n\n按用户认可的现有实测范围发布：62 形态尺寸、38 个按钮、100/500/1000 单位原生渲染、约 19 分钟运行，以及 1000 角色新档重载回归。最终匹配基线未重跑，完整记录与范围说明见开发者包。图片审看按用户要求取消。'))
    existing = {asset['name']: asset for asset in release.get('assets', [])}
    uploaded = []
    for path in uploads:
        url = release['upload_url'].split('{', 1)[0] + '?name=' + urllib.parse.quote(path.name)
        with path.open('rb') as stream:
            digest = hashlib.file_digest(stream, 'sha256').hexdigest()
        if path.name in existing:
            asset = existing[path.name]
            assert asset['size'] == path.stat().st_size and asset.get('digest') == 'sha256:' + digest, 'Draft asset differs from the local package; refusing to replace it'
        else:
            with path.open('rb') as stream:
                asset = request(url, method='POST', data=stream, headers={'Content-Type': 'application/octet-stream', 'Content-Length': str(path.stat().st_size)})
        assert asset['size'] == path.stat().st_size
        if asset.get('digest'):
            assert asset['digest'] == 'sha256:' + digest, 'GitHub asset digest mismatch'
        else:
            # The repository is public; download without forwarding the API credential.
            with urllib.request.urlopen(asset['browser_download_url'], timeout=1800) as stream:
                assert hashlib.file_digest(stream, 'sha256').hexdigest() == digest
        uploaded.append(dict(name=path.name, url=asset['browser_download_url'], sha256=digest))
        print(json.dumps(dict(uploaded=path.name, bytes=asset['size'])), flush=True)
    release = request(API + '/releases/' + str(release['id']), method='PATCH', data={'draft': False})
    result = dict(branch='https://github.com/' + REPOSITORY + '/tree/0.6.0', release=release['html_url'], assets=uploaded)
    (directory / 'github-publication.json').write_text(json.dumps(result, ensure_ascii=True, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(result))


if __name__ == '__main__':
    main()
