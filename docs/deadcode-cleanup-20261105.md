# 未使用代码清理记录（2026-11-05）

本轮确认并删除 22 个无生产调用的方法，共 309 行（含方法间空行）。

检查覆盖 code/、MclslMod.cs、tools/、resources/ 和配置；配置回调、Unity/Harmony 入口及无法排除反射调用的候选保留。以下是本轮确认项，不代表全项目所有不可达代码已被证明。

| 文件 | 删除方法 |
| --- | --- |
| `code/MySimulatedLongevityRoad/Data/MclslAptitudeGiftCatalog.cs` | `ForTrait` |
| `code/MySimulatedLongevityRoad/Data/MclslMinorRealmCatalog.cs` | `ChronicleStageName` |
| `code/MySimulatedLongevityRoad/Systems/Adventure/MclslFactionMissionSystem.cs` | `RealmBand` |
| `code/MySimulatedLongevityRoad/Systems/Crafting/MclslItemEconomy.cs` | `ConsumeIngredient`, `HasPair`, `HasIngredient` |
| `code/MySimulatedLongevityRoad/Systems/Cultivation/MclslCultivationLocalCore.cs` | `ResolveNextRealmId` |
| `code/MySimulatedLongevityRoad/Systems/Cultivation/MclslCultivationSystem.cs` | `RestoreFromHuanzhen`, `RestoreLegacyTreasures`, `RestoreLegacyResources`, `RestoreHuanzhenLegacyCategory`, `RestoreLegacyDao` |
| `code/MySimulatedLongevityRoad/Systems/Cultivation/MclslImmortalPathSystem.cs` | `IsBreakthroughProtected` |
| `code/MySimulatedLongevityRoad/Systems/Cultivation/MclslSpellSystem.cs` | `IsAreaAttack` |
| `code/MySimulatedLongevityRoad/Systems/Death/MclslDeathSystem.cs` | `LongevityTitle` |
| `code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs` | `RestoreSupplementalState` |
| `code/MySimulatedLongevityRoad/Systems/Runtime/MclslLongevityRules.cs` | `MaxTransferBonus` |
| `code/MySimulatedLongevityRoad/Systems/WorldResources/MclslGeneratedObjectFactory.cs` | `FoundationRankColor` |
| `code/MySimulatedLongevityRoad/Systems/WorldResources/MclslWorldCalamitySystem.cs` | `HasFired` |
| `code/MySimulatedLongevityRoad/Systems/WorldResources/MclslWorldEpochSystem.cs` | `YearsUntilNewLaw` |
| `code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs` | `RegisteredActorSprite` |
| `code/MySimulatedLongevityRoad/UI/MclslFeatureWindow.cs` | `PixelPanel` |


旧还真恢复链的入口 RestoreFromHuanzhen 和 RestoreHuanzhenLegacyCategory 无调用者；当前实际入口为 RestoreHuanzhenLegacyOption。RestoreSupplementalState 仅由旧链使用，因此连同专用于它的 tools/test-huanzhen-supplemental-state.ps1 删除。上一轮补回该方法解决了编译错误，本轮进一步确认整条链已经闲置。

删除前源文件及旧测试备份：build/deadcode-cleanup-20261105/before.zip。备份位于已有编译排除目录，不混入生产源代码。

验证：实际游戏依赖编译通过（300 个源码文件，0 警告）；还真自动继承回归通过；体质评分穷举回归通过；22 个删除符号在生产源码中均无残留。未进行游戏内运行验收。
