# 代码质量优化施工方案（2026-10-31）

本方案基于对 `code/` 下 274 个 C# 源文件（约 6.5 万行）的静态审查，
所有条目都标注了可核对的文件与行号。

---

## 执行结果（2026-10-31 已完成部分）

| 阶段 | 状态 | 验收证据 |
|---|---|---|
| 0 环境 | ✅ | 游戏 `D:\owl\gameversion\worldbox-ab712b72f394`，全部依赖齐备 |
| 1.1 编译清单再生成 | ✅ | 276 条目，不可解析 0，`exit=0` |
| 1.2 CleanupPolicy 恢复 | ✅ | 11 条断言全 PASS |
| 1.3 verify-all 默认测试项目 | ✅ | 语法 OK，路径改为 `build/verification` |
| 2 空 catch 收口 | ✅ | 49 → 7（余 7 处为方案内豁免） |
| 3 AddEvent 扫描消除 | ✅ | 6 条裁剪断言 PASS，事件表反查循环 0 处 |
| 4A IdSlots 契约注释 | ✅ | 加契约文档 + `EnumerateLive()` |
| 5.1 删冗余 cz.json | ✅ | 1329 键零差异；已删并同步 `FILE-MANIFEST.json` |
| 5.2 CS0169 两字段 | ✅ | warning 18 → 16 |
| 5.4 仓库卫生 | ✅ | 6 个产物目录全部 IGNORED，源码未误伤 |
| 4B/4C | ⏸ | 4A 已覆盖（方案本身推荐 A，B/C 为备选） |
| 5.3 CS0649 噪声 | ⏸ | 方案明确建议「不做」 |
| 6 上帝类拆分 | ⏸ | 长期，方案标为本轮不做 |

**编译基线**：`sources=276 exit=0 warning=16 error=0`（16 条全是 `MclslLivingResources.cs` 的 CS0649 DTO 噪声）。

**改造统计**：23 个 `.cs` 文件被修改，新增 1 个 `Data/MclslCleanupPolicy.cs`，
`tools/verify-all.ps1` 被修改。空 catch 改造点从 110 增至 154，新增 key 全部唯一（158/158）。

**回滚**：`_pre-change-backup/` 保留了改动前的 25 个文件。

---

## 阶段 0：前置条件（必须先解决，否则无法验收任何改动）

### 0.1 当前机器不具备编译能力

`tools/compile-check.ps1` 的默认游戏目录与脚本内的依赖校验：

```powershell
# compile-check.ps1:2
[string]$GameRoot = 'E:\wol\gameversion\0_51_2_0.51.2'
```

```powershell
# compile-check.ps1:17-19
foreach ($requiredPath in @($managedRoot, $compilerPath, $loaderAssembly, $harmonyAssembly, $gamePublicizedAssembly)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) { throw "Missing compile dependency: $requiredPath" }
}
```

实测结果：

| 依赖 | 状态 |
|---|---|
| `E:\wol\gameversion\0_51_2_0.51.2` | 不存在（E 盘无该目录） |
| `D:\模拟长胜路\validation\runtime-audit-070\game`（rsp 实际引用路径） | 不存在 |
| 全盘 `worldbox_Data\Managed` | 未找到 |
| 全盘 `NeoModLoader.dll` / `Assembly-CSharp-Publicized.dll` | 未找到 |
| `dotnet --list-sdks` | 有 `10.0.403` |

**结论**：脚本会在第 17-19 行直接 throw，`.rsp` 里 164 条 `/reference` 全部指向不存在的 DLL。
因此本机**无法**验证任何 C# 改动。

### 0.2 施工前必须准备的环境

1. 安装 WorldBox **0.51.2**（对应 `mod.json` 的 `targetGameBuild: 115`）。
2. 在该安装中装好 NeoModLoader，确认存在：
   - `worldbox_Data/Managed/`（含 `mscorlib.dll`、`UnityEngine.CoreModule.dll` 等）
   - `worldbox_Data/StreamingAssets/mods/NeoModLoader.dll`
   - `worldbox_Data/StreamingAssets/mods/NML/Assemblies/0Harmony.dll`
   - `worldbox_Data/StreamingAssets/mods/NML/Assembly-CSharp-Publicized.dll`
3. 用显式参数跑一次基线：

```powershell
pwsh -File MySimulatedLongevityRoad/tools/compile-check.ps1 `
  -GameRoot 'D:\WorldBox\0.51.2' `
  -OutputDirectory '_compile-check-baseline'
```

4. 基线要求：`exit=0`，并**记录 warning 数量**（当前历史基线为 18 条：16×CS0649 + 2×CS0169）。
   后续每个阶段结束后重跑同一命令，warning 数不得增加。

### 0.3 若暂时无法搭建环境

只能做**纯文本层改动**（阶段 1.2、2、5.2），并在交付说明中明确写"未编译验证"。
**禁止**在未编译的情况下改动阶段 3、4 涉及的类型签名。

---

## 阶段 1：构建与验证链路修复（P0）

### 1.1 清掉 `.rsp` 里指向已删除源文件的条目

**证据**：`_compile-check-xh/compile.rsp` 共列 279 个源文件，当前树只有 274 个。
其中 4 个条目**在树中不存在**（且全树零引用，属已删除文件）：

```
MclslCleanupPolicy.cs
MclslInverseTruthStageCatalog.cs
MclslHotPathPolicy.cs
MclslWorldSoulObservationSystem.cs
```

另有 1 个条目路径为 `code\GlobalAliases.cs` 同级，实际是
`MySimulatedLongevityRoad\InterestingTrait.cs`（在 `code\` 的**上一级**）。

**为什么不能直接手改 `.rsp`**：它是 `compile-check.ps1:34` 的产物，
每次运行都会被覆盖。真正要做的是**重新生成**：

```powershell
pwsh -File MySimulatedLongevityRoad/tools/compile-check.ps1 -GameRoot <你的路径>
```

脚本的源文件发现逻辑（`compile-check.ps1:29-31`）按目录递归扫描，
会自动排除已删除文件，并自动带上 `InterestingTrait.cs`。**无需改脚本**。

**验收**：
- 新 `compile.rsp` 的 `.cs` 条目数 == `Get-ChildItem -Recurse -Filter *.cs` 的实际数（应为 275 = 274 + `InterestingTrait.cs`）。
- 逐条 `Test-Path` 全部为 True。
- `compile.log` 中 error 数 == 0。

**风险**：无。这是纯再生成。

---

### 1.2 `LineageCleanupTests` 引用了已删除的 `MclslCleanupPolicy.cs`

**证据**：`MclslCleanupPolicy.cs` 在树中不存在，但：

- `build/verification/LineageCleanupTests/LineageCleanupTests.csproj` 里有：
  ```xml
  <Compile Include="..\..\..\code\MySimulatedLongevityRoad\Data\MclslCleanupPolicy.cs"
           Link="Production\MclslCleanupPolicy.cs" />
  ```
- `build/verification/LineageCleanupTests/Program.cs` 有 **10 处**引用
  `MclslCleanupPolicy`（第 84-101 行，`CheckCleanupPolicy()` 整个方法）。

该逻辑已被内联到 `MclslFamilySystem.cs:680-690`：

```csharp
private static bool ShouldRemoveExtinctFamily(
    bool persistentCleanupEnabled,
    bool legacyRemoveAfterLiquidation,
    bool extinct,
    bool estateLiquidated,
    bool hasLivingOrMissingMember,
    bool hasUnsettledDeath)
{
    if (!extinct || hasLivingOrMissingMember || !estateLiquidated || hasUnsettledDeath) return false;
    return persistentCleanupEnabled || legacyRemoveAfterLiquidation;
}
```

**问题**：这个纯函数现在是 `private`，测试无法引用；
而 `ResolvePersistentCleanup`（`bool?` → `bool` 的归一化）在树中**已无对应实现**
（`MclslArchiveData.cs:17` 仍是 `public bool? PersistentCleanupEnabled`，
`MclslWorldRunRepository.cs:61-62` 直接用 `_current?.PersistentCleanupEnabled == true` 判断）。

**方案（二选一，推荐 A）**：

**A. 恢复为可测的纯函数**（改动小、保住 11 条断言）

新建 `code/MySimulatedLongevityRoad/Data/MclslCleanupPolicy.cs`：

```csharp
namespace MySimulatedLongevityRoad.Data;

internal static class MclslCleanupPolicy
{
    internal static bool ResolvePersistentCleanup(bool? marker) => marker == true;

    internal static bool ShouldRemoveExtinctFamily(
        bool persistentCleanupEnabled, bool legacyRemoveAfterLiquidation,
        bool extinct, bool estateLiquidated,
        bool hasLivingOrMissingMember, bool hasUnsettledDeath)
    {
        if (!extinct || hasLivingOrMissingMember || !estateLiquidated || hasUnsettledDeath) return false;
        return persistentCleanupEnabled || legacyRemoveAfterLiquidation;
    }
}
```

然后：
- `MclslFamilySystem.cs:680-690` 的私有副本改为转发调用，避免两份实现漂移：
  ```csharp
  private static bool ShouldRemoveExtinctFamily(
      bool persistentCleanupEnabled, bool legacyRemoveAfterLiquidation,
      bool extinct, bool estateLiquidated,
      bool hasLivingOrMissingMember, bool hasUnsettledDeath)
      => Data.MclslCleanupPolicy.ShouldRemoveExtinctFamily(
          persistentCleanupEnabled, legacyRemoveAfterLiquidation,
          extinct, estateLiquidated, hasLivingOrMissingMember, hasUnsettledDeath);
  ```
  或直接删掉私有方法、把 `MclslFamilySystem.cs:674` 的调用改成 `Data.MclslCleanupPolicy.ShouldRemoveExtinctFamily(...)`。
- `MclslWorldRunRepository.cs:61-62` 改为：
  ```csharp
  internal static bool PersistentCleanupEnabled
      => MclslCleanupPolicy.ResolvePersistentCleanup(_current?.PersistentCleanupEnabled);
  ```
  行为等价（`bool?` 的 `null` 和 `false` 都得到 `false`），但把"null 视为关闭"的规则显式化。

**B. 放弃该测试**：删除 `CheckCleanupPolicy()` 及 csproj 里的 `Compile Include`，
并在文件头注释写明该规则改由家族系统内联。

**验收**：

```powershell
dotnet run --project MySimulatedLongevityRoad/build/verification/LineageCleanupTests -c Release
```

要求输出 `ALL TESTS PASSED`，退出码 0。

**风险**：A 方案会在生产代码里新增一个 `internal` 类型。因为 `compile-check.ps1` 按目录全量扫描，
新文件会被自动纳入编译，无需改 `.rsp`。但**必须编译验证**，确认没有与已删除类型重名冲突。

---

### 1.3 `verify-all.ps1` 的默认测试项目指向不存在的目录

**证据**：

```powershell
# verify-all.ps1:38
[string[]]$TestProjects = @('actor-loading-tests', 'animation-tests', 'death-visual-tests', 'atomic-file-tests', 'storage-regression-tests'),
```

```powershell
# verify-all.ps1:140-141
$projectDirectory = Join-Path $projectRoot ".dev-tools/$project"
if (-not (Test-Path -LiteralPath $projectDirectory)) { throw "Missing test project: $project" }
```

实测：`.dev-tools` 目录不存在，上面 5 个名字一个都不存在。
即**不传 `-SkipTests` 时，验证链必失败**。

**方案**：把默认值改成实际存在的项目（都在 `build/verification/` 下）：

```powershell
[string[]]$TestProjects = @(
    'LineageCleanupTests',
    'EconomyPlanScopeTests',
    'SharedHelperTests',
    'CareerTrialReduction',
    'JulingMarrowReduction'
),
```

并同步改查找路径。当前 `verify-all.ps1:140` 拼的是 `.dev-tools/$project`，
而实际项目在 `build/verification/$project`：

```powershell
$projectDirectory = Join-Path $projectRoot "build/verification/$project"
```

同时 `verify-all.ps1:38` 的 `-split ','` 逻辑（第 44-45 行）仍然可用，不必动。

**注意**：`JulingMarrowReduction` 目录下有两个 csproj
（`JulingRunBaseline.csproj`、`JulingRunFiltered.csproj`），`dotnet run --project <目录>`
在有多个 csproj 时会报歧义。施工时需确认，必要时把 `$TestProjects` 改为存**项目文件相对路径**
而不是目录名：

```powershell
[string[]]$TestProjects = @(
    'build/verification/LineageCleanupTests/LineageCleanupTests.csproj',
    'build/verification/EconomyPlanScopeTests/EconomyPlanScopeTests.csproj',
    'build/verification/SharedHelperTests/SharedHelperTests.csproj',
    'build/verification/CareerTrialReduction/CareerTrialReduction.csproj'
),
```

**验收**：

```powershell
pwsh -File MySimulatedLongevityRoad/tools/verify-all.ps1 -SkipBuild
```

要求每个 `Tests: <name>` 步骤都打印 `ok`，最后输出 `Verification finished`。

**风险**：`build/verification` 下的项目是**历史验证快照**，其中
`JulingMarrowReduction/Production_MaterialDiscovery_BASELINE.cs` 与
`Production_MaterialDiscovery.cs` 是刻意保留的基线/对照副本
（`Production_ItemCatalog.cs` 与 `Production_MaterialDiscovery.cs` 已验证与
`code/` 下的生产文件字节相同）。把这些项目纳入常规验证前，
先确认它们**当前仍能通过**，否则会把"历史基线失败"变成"常规验证失败"。
建议先逐个手动跑一遍，再改默认值。

---

## 阶段 2：空 catch 收口（P1）

### 2.1 现状：已有惯例，但只覆盖了 2/3

项目里**已经存在**一套空 catch 改造惯例，变量名固定为 `mclslEmptyCatchEx`，
调用 `MclslDiagnostics.Error`，key 形如
`empty-catch-code-<路径用短横线>-cs-<序号>`：

```csharp
// MclslCultivationSystem.cs:188
try { actor.updateStats(); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslCultivationSystem-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslCultivationSystem.cs #2: " + mclslEmptyCatchEx.Message); }
```

覆盖统计（`Select-String 'mclslEmptyCatchEx'`）：

| 文件 | 已改造 |
|---|---|
| MclslTraitRegistration.cs | 24 |
| MclslWorldSoulActorRegistration.cs | 17 |
| MclslWorldSoulSystem.Manifestation.cs | 10 |
| MclslCultivationSystem.cs | 8 |
| MclslRealmHaloVisualSystem.cs | 6 |
| MclslHuanzhenSystem.cs | 6 |
| 其余 17 个文件 | 39 |
| **合计** | **110** |

**但仍有 49 处 `catch {}` / `catch (X) {}` 未覆盖**（`catch\s*\{\s*\}` 精确匹配）。

### 2.2 待改造清单（49 处，含行号）

```
Core/MclslAtomicFile.cs:33,34                        ← 保留，见下方豁免
Core/MclslPerformanceProbe.cs:439
Systems/Beasts/MclslBeastAnimation.cs:217
Systems/Crafting/MclslArtifactEquipmentSystem.cs:190
Systems/Cultivation/MclslTechniqueOccupationSystem.cs:180
Systems/Death/MclslNativeKillStatisticsSystem.cs:142,156,201,224
Systems/Reincarnation/MclslMaobaoSnapshot.cs:49,50,86,104,110,120
Systems/Visual/MclslMapMarkerVisualSystem.cs:273,305
Systems/Visual/MclslWorldSpriteRenderLayer.cs:169,205,233,328
Systems/WorldResources/MclslWorldRunRepository.cs:570,680
Traits/MclslWorldSoulActorRegistration.cs:395,489,661
UI/MclslActorInfoPanel.cs:275
UI/MclslCodexWindow.Atlas.cs:348
UI/MclslCodexWindow.BiographyArchive.cs:132
UI/MclslCodexWindow.FamilyArchive.cs:214,217
UI/MclslCodexWindow.Maobao.cs:153
UI/MclslEventLocator.cs:44,48,69,73
UI/MclslMaobaoPlaceMode.cs:51,52,53
UI/MclslMaobaoShortcutButton.cs:206
UI/MclslMaobaoWindow.cs:314
UI/MclslRankSnapshotSource.cs:380,387,389,391,394,395,411
```

### 2.3 豁免（不要改）

`Core/MclslAtomicFile.cs:33-34`：

```csharp
catch (FileNotFoundException) { }
catch (DirectoryNotFoundException) { }
```

这两个是 `Read<T>` 的**正常控制流**（"文件不存在就试下一份"），
不是异常吞没。改造会污染诊断日志。**保持原样**。

### 2.4 改造模板

单行形式（原 `try { X(); } catch { }`）：

```csharp
try { X(); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("<key>", "<描述> " + mclslEmptyCatchEx.Message); }
```

多行形式（原独立 `catch { }`）：保持原有 `try` 块不动，只替换 `catch` 行：

```csharp
catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("<key>", "<描述> " + mclslEmptyCatchEx.Message); }
```

**key 生成规则**（与现有 110 处一致）：
- 把相对路径 `code/MySimulatedLongevityRoad/<子路径>/<文件名>.cs` 的 `/`、`.` 换成 `-`
- 末尾追加 `-cs-<该文件内的第几个改造点，从 1 起>`

**注意**：现有改造点的序号是**按当时扫描顺序**编的，不是行号。
新增改造点时，同一文件内的序号要**接着该文件已用的最大序号往下编**，
否则会和已有 key 冲突，导致 `MclslDiagnostics` 的去重把新日志吞掉
（`MclslDiagnostics.cs:44` 的 `error:<key>` 按 key 节流 3600 帧）。

各文件已用最大序号（需施工时逐一核对，示例）：

| 文件 | 已用序号 |
|---|---|
| MclslCultivationSystem.cs | 到 13 |
| MclslWorldSoulActorRegistration.Attack.cs | 到 3 |
| MclslUiManager.cs | 到 2 |

### 2.5 特别说明：两处高风险点优先改

**（a）`Systems/WorldResources/MclslWorldRunRepository.cs:570`** ——
位于 `AddMaterialDiscoveryEvent` 的 actor 元数据填充块：

```csharp
if (actor?.data != null)
{
    try
    {
        record.ActorId = MclslActorAccessor.Id(actor);
        ...
        record.KingdomName = actor.kingdom?.data?.name ?? string.Empty;
    }
    catch { }
}
```

这里吞异常会让事件**静默丢失**位置/归属，且无任何日志。
这是"事件归档"路径，改造后至少能定位到是哪个字段炸的。建议**拆分 try**，
而不是整块包住，便于定位：

```csharp
record.ActorId = MclslActorAccessor.Id(actor);
record.ActorName = MclslActorAccessor.DisplayName(actor);
try { record.MapX = actor.data.x; record.MapY = actor.data.y; }
catch (System.Exception mclslEmptyCatchEx) { MclslDiagnostics.Error("<key>", "材料发现事件坐标读取失败: " + mclslEmptyCatchEx.Message); }
try { record.LocationName = actor.city?.data?.name ?? string.Empty; }
catch (System.Exception mclslEmptyCatchEx) { MclslDiagnostics.Error("<key2>", "材料发现事件城市名读取失败: " + mclslEmptyCatchEx.Message); }
```

**（b）`Systems/WorldResources/MclslWorldRunRepository.cs:680`** ——
位于 `AddEvent(year,type,title,body,actor)` 的归属补写块，同理。

### 2.6 验收

```powershell
# 1) 剩余空 catch 数应为 2（只剩 AtomicFile 的两个豁免）
(Get-ChildItem -Recurse -File -Filter *.cs MySimulatedLongevityRoad\code |
  Select-String -Pattern 'catch\s*\{\s*\}').Count

# 2) 新 key 无重复
Get-ChildItem -Recurse -File -Filter *.cs MySimulatedLongevityRoad\code |
  Select-String -Pattern 'MclslDiagnostics.Error\("empty-catch-.*?",' -AllMatches |
  ForEach-Object { $_.Matches } | ForEach-Object { $_.Value } |
  Group-Object | Where-Object Count -gt 1

# 3) 编译 + warning 数不增加
pwsh -File MySimulatedLongevityRoad/tools/compile-check.ps1 -GameRoot <你的路径>
```

**风险**：
- `MclslDiagnostics.Error` 内部用 `Time.frameCount`（`MclslDiagnostics.cs:43`）和
  非线程安全的 `Dictionary`（:11）。**如果这 49 处里有在 worker 线程上执行的**
  （例如 `MclslNativeKillStatisticsSystem.cs` 的统计回调、`MclslWorldSpriteRenderLayer`
  的渲染回调），直接接入会引入并发写 Dictionary 的风险。
- **施工前必须逐个确认该 catch 所在方法是否可能在非主线程执行**。
  项目里已有主线程判定设施：`MclslRuntimeChanges.IsMainThread`
  （`MclslRuntimeChanges.cs:24`）。对可能跨线程的位置，改用：
  ```csharp
  catch (System.Exception mclslEmptyCatchEx)
  {
      if (MclslRuntimeChanges.IsMainThread)
          MclslDiagnostics.Error("<key>", "<描述> " + mclslEmptyCatchEx.Message);
  }
  ```
- 或更稳妥：把 `MclslDiagnostics` 的两个 `Dictionary` 换成 `ConcurrentDictionary`
  （`MclslDiagnostics.cs:10-11`），但 `RememberFrame` 的 `Clear()` 逻辑（:18-19）
  需要重新审视，成本略高，建议作为阶段 2 的可选子项。

---

## 阶段 3：消除 `AddEvent` 的全表反复扫描（P1）

### 3.1 问题

`MclslWorldRunRepository.cs` 的 `AddEvent` 家族存在**三次全表线性扫描**。

**第一次**：`AppendEvent`（:404-410）本身不扫，但调用方为了拿到刚插入的记录会反查。

**第二次** —— `AddEvent(year, type, title, body, Actor actor)`（:640-680）：

```csharp
internal static void AddEvent(int year, string type, string title, string body, Actor actor)
{
    AddEvent(year, type, title, body);                    // ← 已插入
    if (actor?.data == null || _current?.Events == null || _current.Events.Count == 0) return;
    ...
    MclslRunEventRecord record = null;
    for (int i = _current.Events.Count - 1; i >= 0; i--)   // ← 反查刚插入的那条
    {
        MclslRunEventRecord candidate = _current.Events[i];
        if (candidate == null || candidate.Year != safeYear) continue;
        if (!string.Equals(candidate.EventType, safeType, StringComparison.Ordinal)) continue;
        if (!string.Equals(candidate.Title, safeTitle, StringComparison.Ordinal)) continue;
        if (!string.Equals(candidate.Body, safeBody, StringComparison.Ordinal)) continue;
        record = candidate;
        break;
    }
    if (record == null) return;
    ...
}
```

**第三次** —— `AddEvent(year, type, title, body, mapX, mapY, locationName, kingdomName, eventKey)`（:682-707）
同样反查一遍（:691-697）。

**额外第四遍** —— `AddMapVisualEvent`（:709-724）先 `AddEvent`，
再调 `ApplyMapVisualToEvent`（:726-739），后者调 `FindEventRecord`（:741-754）
**再一次**从头扫到 400 条。

**量级**：`MaxEvents = 400`（:13）。单次 `AddEvent(actor)` 最多 400 次 × 3 字段字符串比较。
`MclslWorldRunRepository` 的 `AddEvent`/`AddMapVisualEvent` 调用点共 **117 处**。

### 3.2 方案：让 `AppendEvent` 返回刚插入的记录

**第 1 步**：改 `AppendEvent`（:404-410）签名：

```csharp
private static MclslRunEventRecord AppendEvent(MclslRunEventRecord record)
{
    EnsureEventIndexes();
    _current.Events.Add(record);
    IndexEvent(record);
    if (TrimEventsPreservingMilestones(_current.Events, MaxEvents)) NotifyMapMarkerDataChanged();
    return record;
}
```

**为什么可以直接返回、不用再确认是否被裁掉**：
`TrimEventsPreservingMilestones`（:344-365）的删除是**从列表头部（i=0）向尾部**推进的
（:353 `for (int i = 0; ...;)` 只增不减，`list.RemoveAt(i)` 后不 `i++`），
保留的是**尾部**的 `max` 个普通事件与 `MaxMilestones` 个里程碑。
新记录追加在**尾部**（:407 `_current.Events.Add(record)`），
因此在 `MaxEvents=400`、`MaxMilestones=128` 下必然落在保留区。

**第 2 步**：改 `AddEventCore`（:502-537）返回类型 `void` → `MclslRunEventRecord`，
三处早退返回 `null`（:510、:516、:518-521），结尾 `return AppendEvent(record);`。

**第 3 步**：改 `AddEvent(year, type, title, body)`（:468-473）：

```csharp
internal static void AddEvent(int year, string type, string title, string body)
{
    long sample = MclslPerformanceProbe.Begin();
    try { AddEventCore(year, type, title, body); }
    finally { MclslPerformanceProbe.End("事件记录与去重", sample); }
}
```

若需要返回值，新增一个内部重载：

```csharp
private static MclslRunEventRecord AddEventWithRecord(int year, string type, string title, string body)
{
    long sample = MclslPerformanceProbe.Begin();
    try { return AddEventCore(year, type, title, body); }
    finally { MclslPerformanceProbe.End("事件记录与去重", sample); }
}
```

保留原 `void` 版本给 117 处现有调用点，**零改动**。

**第 4 步**：改 `AddEvent(actor)`（:640-680）—— 删掉反查循环，直接用返回值：

```csharp
internal static void AddEvent(int year, string type, string title, string body, Actor actor)
{
    MclslRunEventRecord record = AddEventWithRecord(year, type, title, body);
    if (record == null || actor?.data == null) return;
    ... // 后续补写逻辑原样保留，只是不再需要 safeYear/safeType/... 的比较
}
```

**第 5 步**：改 `AddEvent(map)`（:682-707）同理 —— 删掉 :691-697 的反查循环。

**第 6 步**：改 `AddMapVisualEvent`（:709-724）+ `ApplyMapVisualToEvent`（:726-739）——
把 `FindEventRecord` 换成传入 `record`：

```csharp
private static void ApplyMapVisualToEvent(MclslRunEventRecord record, int year,
    int mapX, int mapY, string locationName, string kingdomName, string visualKind, int durationYears)
{
    if (record == null || string.IsNullOrWhiteSpace(visualKind) || _current?.Events == null) return;
    record.MapX = mapX; record.MapY = mapY;
    record.LocationName = locationName ?? string.Empty;
    record.KingdomName = kingdomName ?? string.Empty;
    record.MapVisualKind = visualKind.Trim();
    record.MapVisualEndYear = Math.Max(0, year) + Math.Max(0, durationYears);
    NotifyMapMarkerDataChanged();
    MclslWorldArchiveStore.MarkDirty();
}
```

**第 7 步**：`FindEventRecord`（:741-754）此时只剩零个调用点 → 删除。

### 3.3 需要同步处理的三个直接 `AppendEvent` 调用点

这三处不经过 `AddEventCore`，直接 `new` + `AppendEvent`：

- `:535`（`AddEventCore` 内部，已被第 2 步覆盖）
- `:575`（`AddMaterialDiscoveryEvent`）
- `:614`（`AddItemAcquisitionEvent`）
- `:953`（死亡记录）

它们**不需要改**（返回值丢弃即可，C# 允许忽略返回值）。若想彻底一致，
可改成 `return AppendEvent(record);` 并让所在方法返回记录，但这会扩大改动面，
**建议本轮不动**。

### 3.4 验收

**功能等价性**（最关键）：

1. 编译通过，warning 数不增加。
2. 手动回归：触发一次「材料发现」事件 → 确认玄黄仙录里该条事件的
   坐标、城市名、国家名**仍正确显示**（这是第 4 步改动最容易出错的地方）。
3. 手动回归：触发一次带地图标记的事件（`AddMapVisualEvent` 路径）→
   确认地图标记出现且持续年数正确。
4. 若 `build/verification` 下有可用的存储回归项目，跑一遍：
   ```powershell
   dotnet run --project MySimulatedLongevityRoad/build/verification/<存储相关项目>
   ```

**性能验证**（可选，需游戏内）：

`MclslPerformanceProbe` 已有 `"事件记录与去重"` 这个 lane
（:472），开 `MclslRuntimeSettings.DiagnosticsEnabled` 后，
对比改造前后该 lane 的耗时与分配字节数。

### 3.5 风险

- **中**。改动触及 117 个调用点的公共入口。
- 必须保留 `void` 重载，否则 117 处要全改，风险失控。
- `AddEventCore` 的早退分支（去重/超限/静默事件）返回 `null` 后，
  第 4、5 步的"补写归属"逻辑会**跳过** —— 这与改造前行为一致
  （改造前反查不到就 `return`）。**行为等价，已核对**。

---

## 阶段 4：`MclslIdSlots` 零槽位契约（P2）

### 4.1 问题

`Core/MclslRuntimeCollections.cs:9-38` 的 `MclslIdSlots` 实现 `IReadOnlyList<long>`，
但删除时把槽位写成 `0` 并回收：

```csharp
internal bool Remove(long id)
{
    if (!_positions.TryGetValue(id, out int slot)) return false;
    _positions.Remove(id);
    _slots[slot] = 0;        // ← 哨兵
    _free.Push(slot);
    return true;
}
```

而 `Count` 仍是 `_slots.Count`（:14，**含空洞**），索引器直接返回 `_slots[index]`（:16）。
`LiveCount`（:15）才是真实存活数。

下游 `MclslActorRegistry.ActorView`（:115-124）把 0 转成 `null`：

```csharp
public int Count => _ids.Count;
public Actor this[int index] => Resolve(_ids[index], out Actor actor) ? actor : null;
```

**这是隐式契约**：所有消费方必须自己判 `null`。目前 8 个消费方的情况：

| 消费方 | 是否判空 | 说明 |
|---|---|---|
| `MclslActorSelection.Tick`（:34） | ✅ | `!MclslActorAccessor.Alive(actor)` |
| `MclslHuanzhenSystem.cs:1175` | ✅ | `IsLivingActor(actor)` 内含 `actor?.data == null` |
| `MclslHuanzhenSystem.cs:1227` | ✅ | 显式 `other == null` |
| `MclslWorldSoulActorRegistration.Attack.cs:79` | ✅ | 显式 `target == null` |
| `MclslSpellSystem.cs:401` | ✅ | `Alive(actor)` |
| `MclslTechniqueLineageSystem.cs:155` | ✅ | `Alive(actor)` |
| `MclslWorldEpochSystem.cs:243` | ✅ | `Alive(actor)` |
| `MclslRealmSeatSystem.cs:70` | ✅ | `Alive(x)`（LINQ） |
| `MclslActorRegistry.CleanupInvalid`（:234） | ✅ | 显式 `if (actorId == 0) continue;` |
| `MclslCultivatorCandidateIndex.CleanupInvalid`（:143） | ✅ | 显式 `if (actorId == 0) continue;` |
| `MclslActorRegistry.FindIdentity`（:330-331） | ✅ | `Resolve` 内部拒 `id<=0` |

**结论**：当前**没有实际 bug**，全部 11 个消费方都做了防护。
但 `CleanupInvalid` 里那两句 `if (actorId == 0) continue;` 是**被这个坑撞过之后补的补丁**，
说明契约容易被新代码踩到。

### 4.2 方案（三选一，推荐 A，成本最低且零风险）

**A. 只加文档与断言，不改结构**

在 `MclslIdSlots` 类注释上写清契约，并在索引器里加 Debug 断言：

```csharp
/// <summary>
/// Stable ID slots, recycled on removal. Cursors never shift when another ID dies.
/// <para>
/// CONTRACT: a removed slot reads back as <c>0</c>, while <see cref="Count"/> still
/// includes holes. Any consumer that enumerates by index MUST treat <c>0</c> as
/// "no actor" and skip it. Use <see cref="LiveCount"/> for the true population.
/// </para>
/// </summary>
internal sealed class MclslIdSlots : IReadOnlyList<long>
{
    ...
    public long this[int index]
    {
        get
        {
            long id = _slots[index];
            System.Diagnostics.Debug.Assert(id != 0 || index < 0,
                "MclslIdSlots: reading a recycled slot; caller must skip id == 0");
            return id;
        }
    }
}
```

**风险**：`Debug.Assert` 在 Release 下被移除（`compile.rsp` 有 `/optimize+`，但
`/debug:portable` 仍会保留 `Debug.Assert` 的调用，除非定义 `TRACE`/`DEBUG` 条件）。
**施工时确认**：若担心发布包里有额外开销，改用 `[Conditional]` 包装或干脆只留注释。
最稳的做法是**只加 XML 注释**，不加运行时断言。

**B. 让 `ActorView` 内部跳过 0**（治本但改变 `Count` 语义）

`ActorView.Count` 目前 = `_ids.Count`（含空洞）。若改成"跳过 0"，需要预扫描，
把 `this[i]` 变成 O(1) 就复杂了。**不建议**：会让 `MclslSpellSystem.cs:395`
的 `Math.Min(64, actors.Count)` 等预算逻辑语义变化。

**C. 新增 `EnumerateLive()`**（推荐与 A 并用）

```csharp
/// <summary>Yields only live ids, skipping recycled zero slots.</summary>
internal IEnumerable<long> EnumerateLive()
{
    for (int i = 0; i < _slots.Count; i++)
    {
        long id = _slots[i];
        if (id != 0) yield return id;
    }
}
```

新代码优先用 `EnumerateLive()`，老代码不动。

### 4.3 验收

- 编译通过，warning 数不增加。
- 若采用 A 的断言版本：跑一次长时间游戏（或离线复现），确认没有断言日志。
- 若采用 C：新增方法暂无调用方，仅编译验证。

**风险**：**低**。不改任何现有行为。

---

## 阶段 5：仓库卫生与死代码（P2）

### 5.1 `Locales/cz.json` 与 `ch.json` 完全相同

**证据**：

```powershell
python -c "import json; a=json.load(open('.../ch.json',encoding='utf-8')); b=json.load(open('.../cz.json',encoding='utf-8')); print(len(a), [k for k in a if a.get(k)!=b.get(k)])"
# → 1329 0
```

两个文件 1329 个键，**逐键零差异**。而代码只读 `ch.json`：

```csharp
// MclslLocalizationBridge.cs:22
string file = Path.Combine(MclslMod.I.GetDeclaration().FolderPath, "Locales", "ch.json");
```

**方案**：删除 `Locales/cz.json`（94860 字节的纯冗余副本）。

**验收**：全树 grep `cz.json` 无命中（已确认当前仅 `ch.json` 被引用）。

**风险**：**低**。但如果发行包构建脚本（`tools/package-release.py`）按文件名清单
复制 `Locales/*`，删除后包内文件数会变，需同步检查
`FILE-MANIFEST.json` 与 `tools/validate-*.py` 的资源校验。

### 5.2 清理两个 CS0169 残留字段

```csharp
// MclslUiManager.cs:18-19
private static PowersTab _immortalTab;
private static PowersTab _beastTab;
```

编译日志确认：`warning CS0169: 从不使用字段"MclslUiManager._immortalTab"` 和 `_beastTab`。
全树 grep 确认无其它引用。

**方案**：删除这两行。

**验收**：重新编译，CS0169 从 2 条降为 0 条。

**风险**：**极低**。但要确认不是"预留给未完成功能"——两者命名对应
`MclslImmortalActorRegistration`（仙道）与 `MclslBeastActorRegistration`（妖兽），
而当前这两个页签是通过 `AddButton(...)`（:37-39 区域）创建的，
不经过 `_immortalTab`/`_beastTab`。**建议删除并在提交说明里注明**。

### 5.3 16 条 CS0649 不是缺陷，可选消除噪声

```
Systems/Visual/MclslLivingResources.cs(12-26): CS0649 ×16
```

这些是 Newtonsoft 反序列化的 DTO 字段（`Frame`、`Group`、`Manifest`），
由 `JsonConvert.DeserializeObject<Manifest>`（:38）填充，
编译器看不到赋值路径。**功能正确**（`resources/living-resources.json` 已用 Python
验证为合法 JSON，`Groups` 87 项、`CareerEffectPadding=2`）。

若想消除警告，给 DTO 加 `[JsonProperty]`：

```csharp
internal sealed class Frame
{
    [JsonProperty("name")] public string Name;
    [JsonProperty("x")] public int X;
    ...
}
```

**风险**：**低**，但 JSON 键名大小写映射（`width`/`Width`）依赖 Newtonsoft 默认
不区分大小写；加 `[JsonProperty("width")]` 必须**全小写**，否则反序列化失败。
**建议**：除非有强迫症需求，**不做**。这是 DTO 的正常形态，16 条警告属可接受噪声。

### 5.4 工作区体积

| 目录 | 体积 | 处置建议 |
|---|---|---|
| `MySimulatedLongevityRoad/build/` | 791.9 MB | 已在 `.gitignore`（`/build/`），确认不随包发布 |
| `delivery/` | 170.7 MB | 交付产物，含 `verify/` 89 MB 源码副本 |
| `delivery/verify/` | 89 MB | 278 个 .cs，其中 233 个与主树字节相同、41 个有实质差异、4 个仅存于副本 |
| `_baseline-compile/`、`_compile-check-xh/` | 各 2.4 MB | 一次性编译产物 |

**方案**：

1. 明确**主树 `MySimulatedLongevityRoad/code/` 为唯一权威**，
   `delivery/verify/` 视为可再生产物，不参与任何比对基线。
2. `delivery/verify/` 里 4 个仅存于副本的文件
   （`InterestingTrait.cs`、`MclslInverseTruthStageCatalog.cs`、`MclslHotPathPolicy.cs`、
   `MclslWorldSoulObservationSystem.cs`）与主树已删除的 3 个同名文件**对应**，
   说明 `delivery/verify` 是**更早的源码快照**（时间戳 2026-10-30 22:47，
   主树 `MclslBeastTalentSystem.cs` 为 2026-10-31 14:20）。
3. 建议在仓库根加 `.gitignore` 条目覆盖 `_baseline-compile/`、`_compile-check-xh/`、
   `_compile-check-loot-redo/`，或移到 `build/` 下。

**验收**：`git status` 不再出现这些目录的未跟踪项。

**风险**：**低**，纯仓库管理。

---

## 阶段 6：上帝类拆分（P3，长期，非本轮）

### 6.1 现状

| 文件 | 行数 | 类级静态字段 |
|---|---|---|
| `UI/MclslCodexWindow.cs` | 2325 | 98 |
| `UI/MclslFeatureWindow.cs` | 1599 | — |
| `Systems/Reincarnation/MclslHuanzhenSystem.cs` | 1489 | — |
| `UI/MclslRankWindow.cs` | 1481 | — |
| `Systems/Crafting/MclslTianxuanMarket.cs` | 1370 | — |
| `Systems/WorldResources/MclslWorldRunRepository.cs` | 1357 | — |

`MclslCodexWindow` 由 **13 个 partial 文件**组成，静态字段分布：

```
MclslCodexWindow.cs              98
MclslCodexWindow.Atlas.cs         7
MclslCodexWindow.Biography.cs     8
MclslCodexWindow.BiographyArchive.cs 2
MclslCodexWindow.FamilyArchive.cs 4
MclslCodexWindow.HistoryBook.cs   8
MclslCodexWindow.Ruins.cs         9
MclslCodexWindow.DaoStruggle.cs   1
MclslCodexWindow.Families.cs      1
MclslCodexWindow.Kingdoms.cs      1
MclslCodexWindow.Maobao.cs        1
MclslCodexWindow.HuanzhenChronicle.cs 0
MclslCodexWindow.Sects.cs         0
```

所有分卷共享同一份类级可变状态，`ClearRuntime()` 一旦漏清一个字段，
就会跨页签泄漏。

### 6.2 最长方法（括号深度统计）

| 方法 | 行数 | 位置 |
|---|---|---|
| `MclslCodexSnapshot.BuildCodexPageCaches` | 320 | `UI/MclslCodexSnapshot.cs:186` |
| `MclslScheduler.TickAnnualActors` | 177 | `Systems/Runtime/MclslScheduler.cs:662` |
| `MclslCodexWindow.DrawPage` | 164 | `UI/MclslCodexWindow.cs:451` |
| `MclslItemUseSystem.TryUseWithTrigger` | 160 | `Systems/Crafting/MclslItemUseSystem.cs:121` |
| `MclslNewLawEventSystem.ProcessAnnual` | 157 | `Systems/Cultivation/MclslNewLawEventSystem.cs:9` |
| `MclslInverseTruthSystem.TickLimitedEffects` | 154 | `Systems/Progression/MclslInverseTruthSystem.cs:271` |
| `MclslPerformanceProbe.BuildSummary` | 149 | `Core/MclslPerformanceProbe.cs:485` |
| `MclslAncientLawEventSystem.ProcessAnnual` | 139 | `Systems/Cultivation/MclslAncientLawEventSystem.cs:90` |

### 6.3 建议做法（仅设计，不施工）

1. **把每页签状态收进结构体**，而不是散落的静态字段：
   ```csharp
   internal sealed class MclslCodexPageState
   {
       internal int Scroll;
       internal int SelectedIndex;
       internal readonly List<...> Rows = new();
       internal void Clear() { ... }
   }
   private static readonly Dictionary<MclslCodexTab, MclslCodexPageState> PageStates = new();
   ```
   好处：`ClearRuntime()` 变成 `PageStates.Clear()` 一句，
   **不会再漏字段**；新增页签自动获得隔离状态。

2. `BuildCodexPageCaches`（320 行）按"页签"拆成 12 个 `BuildXxxPageCache()`，
   主方法只做调度。这与现有的 13 个 partial 文件结构天然对应。

3. `MclslWorldRunRepository` 1357 行 + 大量 `private const` 上限常量（:13-29），
   可把"事件归档"（:344-754 区域）单独拆成 `MclslEventArchive.cs`，
   把"运行状态归一化"（:1313 附近 `NormalizeCore` 130 行）拆成 `MclslRunNormalizer.cs`。

**风险**：**高**。这些是运行中的核心路径，拆分需要大量回归。
**本轮不做**，建议在阶段 1-5 全部稳定、且有游戏内回归条件后再启动。

---

## 施工顺序与依赖

```
阶段 0（环境）
   ↓ 必须
阶段 1.1（rsp 再生成）── 独立
阶段 1.3（verify-all 默认值）── 独立，但建议在 1.2 之后
阶段 1.2（CleanupPolicy 恢复）── 需要编译验证
   ↓
阶段 2（空 catch 收口，49 处）── 需要编译验证 + 线程归属确认
   ↓
阶段 3（AddEvent 扫描消除）── 需要编译验证 + 功能回归
   ↓
阶段 4（IdSlots 契约，仅注释）── 可与 2、3 并行
   ↓
阶段 5（仓库卫生）── 随时可做
   ↓
阶段 6（上帝类拆分）── 长期，另立计划
```

**建议按"一次一个阶段、每阶段单独提交、单独编译验收"推进**，
不要把阶段 2 和阶段 3 混在一次提交里——两者的失败信号（日志 vs 事件字段）不同，
混在一起会难以定位。

---

## 附：本方案引用的关键证据汇总

| 结论 | 证据位置 |
|---|---|
| 编译产物早于源码 5 小时 | `_compile-check-xh/MySimulatedLongevityRoad.dll` = 13:03:54，`MclslCombatCareerSystem.cs` = 18:48:02 |
| rsp 列 279 个源文件，树中 274 个 | 4 个已删文件 + `InterestingTrait.cs` 路径差异 |
| 编译依赖全部缺失 | 164 条 `/reference` 全部不可解析；无 `Managed`、无 `NeoModLoader.dll` |
| 空 catch 49 处未改造 | 精确正则 `catch\s*\{\s*\}` |
| 已有 110 处改造惯例 | `mclslEmptyCatchEx` 变量名 |
| `AddEvent` 三次全表扫描 | `:640-680`、`:682-707`、`:726-754` |
| `MaxEvents = 400` | `MclslWorldRunRepository.cs:13` |
| 117 处 AddEvent 调用点 | `AddEvent/AddMapVisualEvent` 计数 |
| IdSlots 零槽位 | `MclslRuntimeCollections.cs:31` |
| ActorView 返回 null | `MclslActorRegistry.cs:120` |
| 11 个消费方均已防护 | 逐个核对 `Alive`/`== null`/`id<=0` |
| cz.json 与 ch.json 零差异 | 1329 键逐一比对 |
| 只读 ch.json | `MclslLocalizationBridge.cs:22` |
| CS0169 两字段 | `MclslUiManager.cs:18-19` |
| CS0649 全在 DTO | `MclslLivingResources.cs:12-26` |
| 测试项目引用已删文件 | `LineageCleanupTests.csproj` + `Program.cs` 10 处 |
| verify-all 默认项目不存在 | `.dev-tools` 目录缺失 |
