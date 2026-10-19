"""Upload verified deliverables using the existing Git credential, without printing it."""
import argparse
import hashlib
import http.client
import json
import os
import subprocess
import time
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path

REPOSITORY = 'hesitatesaaa/MySimulatedLongevityRoad'
API = 'https://api.github.com/repos/' + REPOSITORY


def credential():
    result = subprocess.run(['git', '-c', 'credential.interactive=never', 'credential', 'fill'],
        input='protocol=https\nhost=github.com\n\n', text=True, capture_output=True,
        env={**os.environ, 'GIT_TERMINAL_PROMPT': '0', 'GCM_INTERACTIVE': 'Never'})
    if result.returncode:
        raise RuntimeError('GitHub login is unavailable to the release process')
    values = dict(line.split('=', 1) for line in result.stdout.splitlines() if '=' in line)
    return values['password']


def request(token, endpoint, payload=None, method=None):
    data = json.dumps(payload, ensure_ascii=True).encode('ascii') if payload is not None else None
    req = urllib.request.Request(API + endpoint, data=data, method=method,
        headers={'Authorization': 'Bearer ' + token, 'User-Agent': 'MCLSL-0.6.0-release',
                 'Accept': 'application/vnd.github+json', 'Content-Type': 'application/json'})
    with urllib.request.urlopen(req, timeout=60) as response:
        return json.load(response)


def upload(token, release, file):
    url = urllib.parse.urlsplit(release['upload_url'].split('{')[0] + '?name=' + urllib.parse.quote(file.name))
    if url.scheme != 'https' or url.hostname != 'uploads.github.com':
        raise ValueError('Unexpected GitHub upload endpoint')
    connection = http.client.HTTPSConnection(url.hostname, timeout=120)
    connection.putrequest('POST', url.path + '?' + url.query)
    for key, value in {'Authorization': 'Bearer ' + token, 'User-Agent': 'MCLSL-0.6.0-release',
                       'Content-Type': 'application/octet-stream', 'Content-Length': str(file.stat().st_size)}.items():
        connection.putheader(key, value)
    connection.endheaders()
    sent, last = 0, time.monotonic()
    try:
        with file.open('rb') as stream:
            while block := stream.read(4 * 1024 * 1024):
                connection.send(block)
                sent += len(block)
                if time.monotonic() - last >= 25:
                    print(f'Uploading {file.name}: {sent * 100 // file.stat().st_size}%', flush=True)
                    last = time.monotonic()
        response = connection.getresponse()
        result = json.loads(response.read())
        if response.status != 201:
            raise RuntimeError(f'GitHub upload failed: HTTP {response.status}, {result.get("message", "unknown")}')
        return result
    finally:
        connection.close()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--package', required=True)
    parser.add_argument('--commit', required=True)
    parser.add_argument('--download-verify-only', action='store_true')
    args = parser.parse_args()
    package = Path(args.package).resolve()
    record = json.loads((package / 'package-record.json').read_text(encoding='utf-8'))
    token = credential()
    releases = request(token, '/releases?per_page=100')
    matches = [r for r in releases if r['tag_name'] == 'v0.6.0']
    if len(matches) > 1:
        raise RuntimeError('Ambiguous release tag')
    release = matches[0] if matches else None
    tag = request(token, '/git/ref/tags/v0.6.0')
    if tag['object']['type'] != 'commit' or tag['object']['sha'] != args.commit:
        raise RuntimeError('The existing tag does not point directly to the release commit')
    if not args.download_verify_only:
        if release and release['target_commitish'] != args.commit:
            raise RuntimeError('Refuse to overwrite a release with a different target')
        if release is None:
            release = request(token, '/releases', dict(tag_name='v0.6.0', target_commitish=args.commit,
                name='0.6.0 开发者包与玩家包（预发布）', draft=True, prerelease=True,
                body=(package / 'RELEASE-NOTES.md').read_text(encoding='utf-8')))
        for item in record['attachments']:
            path = package / item['name']
            if path.stat().st_size != item['bytes']:
                raise RuntimeError('Local attachment size changed')
            with path.open('rb') as stream:
                if hashlib.file_digest(stream, 'sha256').hexdigest() != item['sha256']:
                    raise RuntimeError('Local attachment hash changed')
            assets = request(token, f'/releases/{release["id"]}/assets?per_page=100')
            existing = next((a for a in assets if a['name'] == path.name), None)
            if existing is None:
                existing = upload(token, release, path)
            if existing.get('digest') != 'sha256:' + item['sha256'] or existing['size'] != item['bytes']:
                raise RuntimeError('Refuse to replace an existing attachment with a different hash: ' + path.name)
            print('Verified uploaded digest: ' + path.name, flush=True)
        release = request(token, f'/releases/{release["id"]}', dict(draft=False, prerelease=True), method='PATCH')
        print('Published: ' + release['html_url'], flush=True)
    if release is None or release['draft']:
        raise RuntimeError('Public release is not available for download verification')
    assets = request(token, f'/releases/{release["id"]}/assets?per_page=100')
    downloaded = []
    for item in record['attachments']:
        asset = next(a for a in assets if a['name'] == item['name'])
        # Public downloads need no credential; redirects never receive the API token.
        req = urllib.request.Request(asset['browser_download_url'], headers={'User-Agent': 'MCLSL-release-verifier'})
        digest, size, last = hashlib.sha256(), 0, time.monotonic()
        with urllib.request.urlopen(req, timeout=120) as response:
            while block := response.read(4 * 1024 * 1024):
                digest.update(block)
                size += len(block)
                if time.monotonic() - last >= 25:
                    print(f'Verifying download {item["name"]}: {size * 100 // item["bytes"]}%', flush=True)
                    last = time.monotonic()
        if digest.hexdigest() != item['sha256'] or size != item['bytes']:
            raise RuntimeError('Downloaded attachment does not match: ' + item['name'])
        downloaded.append(dict(name=item['name'], sha256=digest.hexdigest(), bytes=size))
        print('Downloaded SHA256 verified: ' + item['name'], flush=True)
    result = dict(commit=args.commit, branch='0.6.0', release=release['html_url'], downloaded=downloaded)
    (package / 'github-publication.json').write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')


if __name__ == '__main__':
    main()
