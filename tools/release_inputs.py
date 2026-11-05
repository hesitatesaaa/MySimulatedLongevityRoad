"""Canonical player inputs shared by packaging and acceptance fingerprints."""
import hashlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SKIP = {'.git', '.codex', '__pycache__', 'bin', 'obj', '.vs', 'fantasy-reference'}
BASE = ['code', 'GameResources', 'NativeActorResources', 'InterfaceResources', 'Locales', 'MclslMod.cs',
        'default_config.json', 'mod.json', 'icon.png']
PLAYER_INPUTS = BASE + ['resources/actor-frames.manifest.json', 'resources/world-soul-frames.manifest.json',
                       'resources/beast-talents.manifest.json', 'resources/living-resources.json',
                       'resources/0.8.0-release-policy.json', 'resources/0.8.0-architecture.md',
                       'resources/LOCAL-DEVELOPMENT.md', 'resources/refactor-080-progress.md', 'README.md',
                       'resources/contract-skill-icons.manifest.json', 'resources/career-effects.manifest.json',
                       'resources/contract-rework.md']
EVIDENCE_INPUTS = {'resources/refactor-080-progress.md'}
PLAYER_EXCLUDED = {'code/MySimulatedLongevityRoad/UI/MclslDeveloperActorEditor.cs'}


def files(paths, root=ROOT):
    found = {}
    for relative in paths:
        source = root / relative
        if not source.exists():
            raise FileNotFoundError(source)
        for path in source.rglob('*') if source.is_dir() else [source]:
            name = path.relative_to(root)
            if path.is_file() and not any(part in SKIP for part in name.parts) and path.suffix not in ('.pyc', '.pyo', '.dll', '.exe', '.pdb'):
                found[name.as_posix()] = path
    return sorted(found.items())


def player_files(root=ROOT):
    return [(name, path) for name, path in files(PLAYER_INPUTS, root) if name not in PLAYER_EXCLUDED]


def fingerprint(root=ROOT):
    digest = hashlib.sha256()
    for name, path in player_files(root):
        if name in EVIDENCE_INPUTS:
            continue
        digest.update(name.encode('utf-8') + b'\0')
        digest.update(hashlib.sha256(path.read_bytes()).digest())
    return digest.hexdigest()
