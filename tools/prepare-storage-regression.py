"""Compile the actual storage/new-world method bodies with isolated Unity stubs."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
source = (ROOT / 'code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs').read_text(encoding='utf-8-sig')
target = ROOT / '.dev-tools/storage-regression-tests/obj/fixture'
target.mkdir(parents=True, exist_ok=True)
def method(name):
    match = re.search(r'^    (?:internal|private) static [^\n]+\b'+name+r'\([^\n]*\)\n    \{', source, re.M)
    assert match, name
    brace=source.index('{', match.start()); end=brace+1; depth=1
    while depth:
        depth += (source[end]=='{')-(source[end]=='}'); end+=1
    return source[match.start():end]
fields=source[source.index('    private const int MaxAnchors'):source.index('    internal static bool IsApplyingRestore')]
methods='\n'.join(method(name) for name in ['EnsureLoaded','Flush','PrepareForNewWorld','ClearRuntime',
    'Normalize','NormalizeSnapshot','SyncCurrentEssenceLedger','DeleteAnchorFolders','ResolveAnchorSavePath'])
hooks='''
    internal static void Reset(string root)
    {
        Application.persistentDataPath = root;
        _loaded = _writeLocked = _preserveBackup = _anyWorldLoadInProgress = false;
        _lastFlushedJson = _lastCommittedJson = _storageFailure = string.Empty;
        _state = new();
        ClearRuntime();
    }
    internal static MclslHuanzhenExternalState State => _state;
    internal static bool Locked => _writeLocked;
    internal static string Failure => _storageFailure;
    internal static bool Save() => Flush();
'''
headers='''using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
namespace MySimulatedLongevityRoad.Systems;
internal class Actor { }
internal class MclslActorSelection { }
internal static class SaveManager { internal static string folderPath(string value) => Path.GetFullPath(value); }
internal static class MclslHuanzhenSystem {
'''
(target/'HuanzhenFixture.cs').write_text(headers+fields+methods+hooks+'\n}',encoding='utf-8')
data=(ROOT/'code/MySimulatedLongevityRoad/Data/MclslHuanzhenData.cs').read_text(encoding='utf-8-sig')
(target/'HuanzhenData.cs').write_text(data[:data.index('internal readonly struct MclslHuanzhenDeathSnapshot')],encoding='utf-8')
print('Generated storage fixture from current production method bodies')
