"""Extract production repository entrypoints; no copied implementation in tests.

The full repository depends on game-native systems. These entrypoints are tested
with the real archive store, family system, material discovery and currency engine;
only unrelated game services and normalization dependencies are stubbed.
"""
from pathlib import Path

root = Path(__file__).resolve().parents[2]
repo = (root / 'code/MySimulatedLongevityRoad/Systems/WorldResources/MclslWorldRunRepository.cs').read_text(encoding='utf-8-sig')
epoch = (root / 'code/MySimulatedLongevityRoad/Systems/WorldResources/MclslWorldEpochSystem.cs').read_text(encoding='utf-8-sig')
market = (root / 'code/MySimulatedLongevityRoad/Systems/Crafting/MclslTianxuanMarket.cs').read_text(encoding='utf-8-sig')
backpack = (root / 'code/MySimulatedLongevityRoad/UI/MclslBackpackTabSystem.cs').read_text(encoding='utf-8-sig')
backpack = backpack[backpack.index('internal sealed class MclslBackpackTabElement'):]

def method(text, signature):
    start = text.index(signature)
    brace = text.index('{', start)
    depth = 1
    end = brace + 1
    while depth:
        depth += (text[end] == '{') - (text[end] == '}')
        end += 1
    return text[start:end]

generated = '''using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
namespace MySimulatedLongevityRoad.Systems;
internal static partial class MclslWorldRunRepository {
''' + method(repo, 'internal static bool ImportArchive(') + '\n' + method(repo, 'private static void EnsureCurrentRunCore(') + '''
internal static void EnsureCurrentRun(int year) => EnsureCurrentRunCore(year);
}
internal static partial class MclslWorldEpochSystem {
''' + method(epoch, 'private static void NormalizeEra(') + '''
internal static void NormalizeForTest(MclslWorldRunState run) => NormalizeEra(run);
}
'''
out = Path(__file__).parent / 'obj/ExtractedRepository.cs'
out.parent.mkdir(exist_ok=True)
out.write_text(generated, encoding='utf-8')
market_out = out.parent / 'ExtractedMarket.cs'
market_out.write_text('''using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
namespace MySimulatedLongevityRoad.Systems;
internal static partial class MclslTianxuanMarket {
''' + '\n'.join(method(market, signature) for signature in [
    'private static bool TryReturnExpiredListing(', 'internal static void BeginAnnualPrune(',
    'private static bool TryRecycleExpiredListing(', 'internal static bool TickAnnualPrune(']) + '\n}', encoding='utf-8')
print('Prepared tests from current production entrypoints.')
ui_out = out.parent / 'ExtractedBackpack.cs'
ui_out.write_text('''using UnityEngine;
using UnityEngine.UI;
namespace MySimulatedLongevityRoad.UI;
internal sealed partial class MclslBackpackTabElement : UnitElement {
''' + '\n'.join(method(backpack, signature) for signature in [
    'public override void clear()', 'public override void OnDisable()', 'internal void CompleteDeferredCleanup()']) + '\n}', encoding='utf-8')
