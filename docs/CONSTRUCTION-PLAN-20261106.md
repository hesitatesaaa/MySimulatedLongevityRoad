# 施工方案（2026-11-06）— 基于代码质量评估

> 覆盖范围：**P0 版本控制基线、P1 异常吞噬治理、P2-重复代码、P3 反射/长参数/热路径 LINQ、P4 可读性与度量**。
> **仅不包含「上帝类 / 超长方法」改造**——按要求该两项不排期；其余（含重复代码）全部纳入。
> 原则：每阶段独立可交付、可回滚；每步都有可运行的验收命令与量化门槛。

---

## 0. 总体目标与交付物

| 项 | 目标 |
|---|---|
| 版本控制 | 建立 0.8.7 基线，之后每阶段独立提交/分支，可回滚 |
| 异常治理 | “只记 Probe 的吞异常”从 **153 处**降到白名单内的可控集合；真实缺陷可上报 |
| 反射治理 | Traits/Death 等运行期 `GetType().GetField/GetProperty` **全部改为缓存访问器**（0 处未缓存） |
| 热路径 | 5 个 Tick/Update 方法去除 LINQ 分配；≥6 参数声明 Top10 改参数对象 |
| 可读性 | 超长行（>200 字符）从 357 降到阈值内（分文件推进）；注释率提升 |
| 去重 | 跨文件重复代码块从 **25 组**降到 **≤10 组**；按钮/图片工厂样板收敛到单一实现 |
| 度量 | `docs/CODE-METRICS.json` 与源码同步，并加入 CI 防漂移 |

交付物：
1. `MclslGuard` 统一异常守卫（新 Core 文件）
2. `MclslReflectionMembers` 通用成员缓存（扩展现有 `MclslReflectionNumbers` 模式）
3. `UI/MclslUiFactory.cs` 按钮/图片工厂（去重 E 簇）
4. `tools/code-quality-ratchet.py` 质量棘轮脚本（阈值门禁，含 duplicateBlocks）
5. 更新后的 `docs/CODE-METRICS.json`（含 5 项新指标）
6. CI 增补（`.github/workflows/code-quality.yml`）
7. 各阶段提交记录

---

## M0 · 版本控制与 CI 基线（P0，约 0.5 人日）

### 现状依据
- `git status --porcelain` = **181 项**未提交（0.8.7 导入 + 修复全在工作区）。
- 评审期间观察到 `.tests` 被外部并发改动（RankSortTests/PolicyTests 出现又消失）→ 无基线时冲突不可恢复。
- 现有 CI：`.github/workflows/code-quality.yml` 仅跑 `.tests/run.ps1 -Profile Portable`。

### 施工步骤
1. **建基线**：`git switch -c release/0.8.7-baseline` → `git add -A` → 提交
   `chore: import 0.8.7 package + applied fixes (baseline)`。
2. 打标签 `v0.8.7-local.1`，后续每阶段单独分支 `quality/p1-exceptions` 等。
3. **CI 增补**（在 code-quality.yml 追加 job）：
   - `metrics-fresh`：运行 `python tools/report-code-metrics.py`，若 `docs/CODE-METRICS.json` 变动则失败（防文档漂移）。
   - `quality-ratchet`：运行 `python tools/code-quality-ratchet.py`（M1 阶段引入），阈值失败即红。
   - 保留 portable regressions（已有）。
4. 约定：禁止在未提交状态下做批量重构；每阶段结束 `git diff --stat` 归档到 `docs/`。

### 验收
```powershell
git status --porcelain            # 预期：空
./.tests/run.ps1 -Profile Portable # 预期：11 个 portable 全 PASS
```
### 风险/回滚
- 风险：一次提交 181 项过大，不便 code review。缓解：先在本地分 3 次提交（导入 / 修复 / 文档）。
- 回滚：`git reset --hard v0.8.7-local.1`。

---

## M1 · 异常吞噬治理（P1，约 3–5 人日）

### 现状依据
- `catch(Exception)` **309 处**；`empty-catch-code-*` 形式的“只 Probe” **153 处**。
- 热点文件（empty-catch-code 次数）：`Traits/MclslTraitRegistration.cs` 24、`Traits/MclslWorldSoulActorRegistration.cs` 19、`Systems/WorldSoul/MclslWorldSoulSystem.Manifestation.cs` 10、`Systems/Cultivation/MclslCultivationSystem.cs` 7、`Systems/Visual/MclslWorldSpriteRenderLayer.cs` 7、`UI/MclslRankSnapshotSource.cs` 7、`Systems/Reincarnation/MclslHuanzhenSystem.cs` 6、`Systems/Visual/MclslRealmHaloVisualSystem.cs` 6。
- 典型：`Core/MclslRuntimeSettings.cs:189-192`（递归 `Find` 把任何异常当“未找到”）；
  `Systems/WorldSoul/MclslWorldSoulSystem.Manifestation.cs:283`（单行 351 字符 catch 只 Probe）。

### 技术方案：新增 `Core/MclslGuard.cs`
分类 + 计数 + 白名单，避免 150 处散落：

```csharp
internal enum MclslGuardScope { Reflection, NativeUi, Persistence, OptionalMember, None }
internal static class MclslGuard
{
    // 仅用于确定可忽略的边界；命中即计数并 Probe；未被登记的可忽略类别走 Error。
    internal static bool Try(Action action, string key, MclslGuardScope scope = MclslGuardScope.NativeUi);
    internal static T Try<T>(Func<T> action, T fallback, string key, MclslGuardScope scope = MclslGuardScope.NativeUi);
    internal static int SwallowedCount { get; }   // 供指标与棘轮读取
    internal static IReadOnlyDictionary<string,int> SwallowedByKey { get; }
}
```
规则：
- `scope=Reflection/OptionalMember` → 可忽略，计 `SwallowedByKey`，仅当诊断开启时 Probe；
- `scope=Persistence` → **不得忽略**：记录 `Fatal` 并停写（沿用现有 `MclslWorldArchiveStore.LockWrites` 语义）；
- 其余未登记异常 → `Error` 且计数，供棘轮下降。

### 施工步骤（按文件批量替换）
1. 新增 `Core/MclslGuard.cs` + 单测（放 `.tests/SharedHelperTests` 或新 `GuardTests`）。
2. 第一批（高频、低风险只读）：`MclslTraitRegistration.cs`(24)、`MclslWorldSoulActorRegistration.cs`(19)、`MclslWorldSoulSystem.Manifestation.cs`(10)。
3. 第二批：`MclslCultivationSystem.cs`(7)、`MclslWorldSpriteRenderLayer.cs`(7)、`MclslRankSnapshotSource.cs`(7)。
4. 第三批：`MclslHuanzhenSystem.cs`(6)、`MclslRealmHaloVisualSystem.cs`(6)、`MclslNativeKillStatisticsSystem.cs`(4)、`MclslEventLocator.cs`(4)、`MclslRankTextFormat.cs`(4) 等。
5. 逐处把 `catch (System.Exception mclslEmptyCatchEx) { Probe("empty-catch-code-…") }`
   改为 `MclslGuard.Try(...)`，并**收窄异常类型**（NRE→`NullReferenceException`、越界→`IndexOutOfRangeException`、集合并发→`InvalidOperationException`，并分别给不同 key）。
6. 对 `MclslRuntimeSettings.cs:189`：把异常收窄为 `InvalidCastException/KeyNotFoundException`，真正的类型错误要抛出。

### 门槛（棘轮）
- `catch(Exception)`：309 → **≤200**（M1 结束），后续每阶段 −30。
- `empty-catch-code-*`：153 → **≤80**（M1 结束）。
- 由 `tools/code-quality-ratchet.py` 校验。

### 验收
```powershell
python tools/report-code-metrics.py ; python tools/code-quality-ratchet.py
./.tests/run.ps1 -Profile Portable
tools/compile-check.ps1 -GameRoot <本机游戏目录>   # 需 pwsh
```
### 风险/回滚
- 风险：收窄异常类型可能暴露此前被吞的潜在错误 → 先在 `MclslGuard` 里临时保留“记录但不抛”，观察一轮实机日志再收紧。
- 回滚：按文件 fan-out 单独提交，可逐文件 revert。

---

## M2 · 反射治理（P3，约 3–4 人日）

### 现状依据
- 反射调用：`AccessTools.` 95、`GetField(` 34、`GetProperty(` 23、`BindingFlags` 43、`MethodInfo` 48。
- 其中 Patches 内的 `AccessTools` 多为启动期一次解析（可接受）；**问题在运行期的未缓存 lookup**：
  - `Traits/MclslTraitRegistration.cs:1002,1009,1040,1046`
  - `Traits/MclslWorldSoulActorRegistration.cs:412,420,584,590,609,616,638,640,658,660,673,674,688`
  - `Traits/MclslImmortalActorRegistration.cs:138`
  - `Systems/Death/MclslDeathSystem.cs:702,705`、`Systems/Death/MclslMortalMiasmaSystem.cs:92`
  - `Systems/Beasts/MclslBeastConfig.cs:121-122`（每次读取 backing field）
- 已有缓存范例：`Core/MclslReflectionNumbers.cs`（`ConcurrentDictionary<(Type,string), MemberAccessor>` 缓存，含计数器 `FactoryCalls/CacheCount`）。

### 技术方案
将 `MclslReflectionNumbers` 的缓存内核扩展为通用成员访问：
```csharp
internal static class MclslReflectionMembers   // 新文件，复用同款 ConcurrentDictionary 缓存
{
    internal static bool TryGet(object target, string name, out object value, string site);
    internal static bool TrySet(object target, string name, object value, string site);
    internal static bool TryGet<T>(object target, string name, out T value, string site);
    internal static void Prime(Type type, params string[] names);  // 启动期预热，缺失即早报错
    internal static int UnCachedLookupCount => ...;                 // 供棘轮
}
```
- 现有 `MclslReflectionNumbers.TryGetNumber/TrySetNumber` 内部改走该缓存（保持签名不变，避免大面积改动）。
- 禁止在 Tick 热路径直接 `GetType().GetField/GetProperty`。

### 施工步骤
1. 新增 `Core/MclslReflectionMembers.cs`（缓存 + 计数 + 线程安全）。
2. 在 `MclslRuntime.LoadCoreModules` 末尾对关键类型调用 `Prime(...)` 预热（native `Actor`、`ActorAsset`、`Subspecies`、`ModConfigItem` 等），缺失成员在启动期 `Fatal`。
3. 逐文件替换上列未缓存 lookup（Traits 三件、Death 两件、BeastConfig）。
4. 删除/合并重复的私有反射帮助方法（仅去重实现，不改行为；**不属于 P2 的“重复业务代码”**）。
5. 指标：`UnCachedLookupCount` 接入 `MclslPerformanceProbe` 指标文本。

### 验收
- 静态：`Select-String -Pattern 'GetType\(\)\.Get(Field|Property)\('` 计数 **= 0**（改走缓存）。
- 运行：`MclslReflectionMembers.UnCachedLookupCount == 0`（启动 + 跑 1 年后）。
- 回归：`.tests/run.ps1 -Profile Portable` 全通过。
### 风险/回滚
- 风险：`Prime` 预热若写错成员名会在启动期 Fatal；先以 `Alert` 记录过渡，确认后再改 `Fatal`。
- 回滚：`MclslReflectionMembers` 可保留但改回直接反射（单文件 switch）。

---

## M3 · 长参数列表改造（P3，约 2–3 人日）

### 现状依据（Top 声明，file:line）
| 参数数 | 位置 |
|---:|---|
| 34 | `Data/Death/MclslDeathSnapshot.cs:41`（构造函数） |
| 23 | `Traits/MclslPhysiqueSystem.cs:18`（`Physique`） |
| 11 | `Systems/Adventure/MclslAdventureSystem.cs:478 BuildExplorationRecord` |
| 10 | `Systems/WorldResources/MclslWorldRunRepository.cs:838 AddMapVisualEvent` |
| 10 | `Systems/WorldResources/MclslGeneratedObjectFactory.cs:59 CreateWorldChange` |
| 10 | `Systems/Adventure/MclslFactionPressureSystem.cs:168 Register` |
| 10 | `Data/MclslBeastContent.cs:16 MclslBeastContentRow` |
| 9 | `UI/MclslRankCard.cs:105 CreateText`、`UI/MclslBeastControlWindow.cs:179 Number` |

共 **127 个** ≥6 参数的声明（含构造/方法）。

### 技术方案
- 一次性构造参数（Snapshot/Row/Modifiers）→ 改 `record` + 命名参数：
  `internal sealed record MclslDeathSnapshotArgs(long ActorId, string ActorName, …)`，保留旧构造函数作为 `[Obsolete]` 重载一个版本周期。
- 调用频繁的“多可选参数”方法（`AddMapVisualEvent`/`CreateText`/`Number`）→ 抽 `record struct` 选项对象或分层重载（必选 3–4 个 + 可选 options）。

### 步骤
1. 先改 34/23 两个极端（构造参数对象 + 全量调用点替换，编译器会强制找齐调用点，安全）。
2. 再改 10–11 参数的 5 处。
3. 9 参数 UI 辅助方法改 options，减少调用处默认值噪声。
4. 棘轮：声明 ≥6 参数数量 127 → **≤90**。

### 验收
```powershell
tools/compile-check.ps1 -GameRoot <本机游戏目录>
./.tests/run.ps1 -Profile Portable
python tools/code-quality-ratchet.py   # long-param 阈值
```
### 风险/回滚
- 风险：参数顺序错位。缓解：改名参数 + record 命名参数，编译期强制；每文件单独提交。
- 回滚：保留旧签名重载，移除新 record 即可恢复。

---

## M4 · 热路径 LINQ 与可读性/度量（P3+P4，约 3–4 人日）

### 4.1 热路径 LINQ（依据）
| LINQ 次数 | 位置 |
|---:|---|
| 6 | `Systems/Crafting/MclslTianxuanMarket.cs:1519 UpdateMarketPrices` |
| 6 | `Systems/Crafting/MclslTianxuanMarket.cs:398 RefreshActivitySnapshots` |
| 3 | `UI/MclslMaobaoWindow.cs:79 Refresh` |
| 3 | `UI/MclslCodexWindow.Biography.cs:48 RefreshBiographyIndex` |
| 2 | `Systems/Cultivation/MclslTechniqueLineageSystem.cs:36 TickResolveAnnual` |

步骤：
1. `UpdateMarketPrices/RefreshActivitySnapshots`：`Where/Select/OrderBy/ToList` 改 for 循环 + 复用 `List<T>` 字段（项目已有 `MclslBudgetedView` 复用思路）。
2. UI 两个 `Refresh`：仅可见行时物化，避免全量 `ToList`。
3. 用 `RuntimeQualityBenchmarks`（`.tests/RuntimeQualityBenchmarks`）补分配基线测试：目标这些方法每次调用 GC 分配 ≤ 现有基线（记录前后字节数）。

验收：`dotnet run --project .tests/RuntimeQualityBenchmarks`（本机/CI 一致）+ 人工确认无新增分配。

### 4.2 超长行与注释
- 现状：>200 字符 **357 行**、>300 字符 **76 行**。Top：`Traits/MclslWorldSoulActorRegistration.cs:286`(577)、`UI/MclslCodexSnapshot.cs:453`(522)、`UI/MclslUiManager.cs:136`(444)、`MclslCodexSnapshot.cs:169`(436)、`MclslHuanzhenSystem.cs:1021`(401)。
- 步骤：只对**本方案改动到的文件**顺手折行（避免全库格式化产生巨大 diff）；在 `.editorconfig` 加 `max_line_length = 200`（`dotnet_diagnostic` 级别 warn，不强制）。
- 注释率 3.3%：对新增/改动的公共方法补 XML 注释（`MclslGuard`、`MclslReflectionMembers`、参数 record）。

### 4.3 度量文档同步
- 现状：`docs/CODE-METRICS.json` 仍为 0.8.5 / 291 文件 / 74390 行；实际 299 文件、约 7.6 万行。
- 步骤：`python tools/report-code-metrics.py` 重新生成；CI 加“重新生成后无 diff”校验（M0 已列）。
- 扩展 `report-code-metrics.py`：增加 `emptyCatch`、`catchException`、`uncachedReflection`、`longParams(>=6)`、`longLines(>200)` 五项，作为棘轮数据源。

### 验收
```powershell
python tools/report-code-metrics.py ; git diff --exit-code docs/CODE-METRICS.json
python tools/code-quality-ratchet.py
./.tests/run.ps1 -Profile Portable
```

---

## M5 · 重复代码治理（P2-重复代码，约 2–3 人日）

### 现状依据（跨文件 8 行重复块 25 组，抽 5 类典型）
| 重复簇 | 位置 | 说明 |
|---|---|---|
| A. 地图事件点收集 | `Systems/Adventure/MclslAdventureSystem.cs:839-841 / 852-858` ↔ `Systems/WorldResources/MclslMapLocationResolver.cs:89-91 / 101-105` | `List<LocationSeed>` 构建 + 单次解析逻辑各写一份 |
| B. 反射取成员 `TryGetActorMember` | `Systems/Death/MclslMortalMiasmaSystem.cs:82-88` ↔ `Systems/WorldSoul/MclslWorldSoulSystem.cs:550-556` | 两处**同名同实现**的反射助手（与 M2 合并处理） |
| C. 家族成员重建 | `Systems/Family/MclslFamilySystem.cs:214-230` ↔ `Systems/Family/MclslFamilySystem.Paternal.cs:61-77` | `FamilyId/Member` 写入与 `ByActor[id]=family` 重复 |
| D. 榜单重排 | `UI/MclslBeastRankWindow.cs:393` ↔ `UI/MclslRankWindow.cs:727` | 同款 `int oldIndex = Entries.IndexOf(id);` 重排/定位 |
| E. UI 按钮/图片样板 | 全库：`button.onClick.RemoveAllListeners()` **8 次**、`.targetGraphic=` **23 次**、`Selectable.Transition.None` **12 次**、`CreateImage(` **13 次** | 按钮创建/绑定/无过渡样板重复 |

### 技术方案与步骤
1. **B 簇并入 M2**：删掉两份 `TryGetActorMember`，统一调用 `MclslReflectionMembers.TryGet(...)`（同时满足 M2 的“运行期反射归零”）。
2. **A 簇**：把 `LocationSeed` 的构建/解析收敛到 `MclslMapLocationResolver` 单一入口（`MclslAdventureSystem` 改为调用），删除 Adventure 内的重复实现。
3. **C 簇**：在 `MclslFamilySystem` 抽 `EnsureMembershipEntry(Actor actor, MclslFamilyRecord family, long parent1, long parent2)`，两个 partial 都调用；保证写入顺序/字段完全一致。
4. **D 簇**：在 `UI/MclslRankComponents` 增加 `ReorderEntry(MclslOrderedIdIndex<T> entries, long id)`（或 `MoveEntryToFront`），Rank/BeastRank 共用。
5. **E 簇**：新增 `UI/MclslUiFactory.cs`：`CreateImage(name,parent,sprite,color,size)`、`MakeButton(go, targetGraphic, click)`（统一 `onClick` 重建 + `transition=None`），替换 8/23/12/13 处样板中的高频调用点。
6. 每完成一簇单独提交，跑一遍 UI 相关离线检查（`tools/test-contract-row-click.ps1` 等）与 `.tests` portable。

### 验收
- 跨文件 8 行重复块：**25 组 → ≤10 组**（棘轮 `duplicateBlocks`）。
- 关键样板计数：`TryGetActorMember` 定义 2→1（或 0，走 M2）；`button.onClick.RemoveAllListeners()` 8→≤3；`CreateImage(` 13→≤5。
- `.tests/run.ps1 -Profile Portable` 全通过；UI 交互脚本不回归。

### 风险/回滚
- 风险：抽公共方法时若字段写入顺序或默认值不一致，会引入难查的行为差异（尤其 C 簇家族结算、E 簇按钮点击）。
- 缓解：**先抽象成“同实现 + 老方法转发”**，确认无回归后再删旧实现；每簇单独提交便于 revert。
- 回滚：保留原方法作为转发壳，删除公共抽象即恢复。

---

## 5. 质量棘轮脚本设计（`tools/code-quality-ratchet.py`）

```python
LIMITS = {
  "catchException": 200,          # M1 后 ≤200，之后每阶段 -30
  "emptyCatch": 80,               # 只 Probe 的吞异常
  "uncachedReflection": 0,        # GetType().GetField/GetProperty 运行期
  "longParams6": 90,              # 声明 ≥6 参数
  "longLines200": 200,            # >200 字符行（按阶段收紧）
  "duplicateBlocks": 10,          # 跨文件 8 行重复块组数（M5）
}
# 读取源码统计，超过 LIMITS 即 exit 1 并打印 file:line Top10
```
每次阶段结束下调阈值并提交，形成“只降不升”。

---

## 6. 排期（人日）与里程碑

| 阶段 | 内容 | 估算 |
|---|---|---:|
| M0 | 基线提交 + CI 增补 | 0.5 |
| M1 | `MclslGuard` + 153 处吞异常分批治理 | 3–5 |
| M2 | 反射缓存 + 预热 + 替换运行期 lookup | 3–4 |
| M3 | 长参数 Top 改造（34/23/10…） | 2–3 |
| M4 | 热路径 LINQ + 超长行/注释 + 度量同步 | 3–4 |
| M5 | 重复代码治理（A/B/C/D/E 五簇） | 2–3 |
| 合计 | | **13.5–19.5 人日** |

里程碑：M0 完成即可锁基线；M1 完成显著改善“问题被吞”；M2（含 B 簇去重）完成降低运行期反射开销与重复实现；M3/M4/M5 为质量收尾。

---

## 7. 统一验证矩阵（每阶段结束必跑）

| 检查 | 命令 | 通过标准 |
|---|---|---|
| 编译 | `tools/compile-check.ps1 -GameRoot <本机游戏目录>` | 0 error / 0 warning |
| 离线回归 | `./.tests/run.ps1 -Profile Portable` | 11 项全 PASS |
| 度量 | `python tools/report-code-metrics.py` | 生成成功且数值更新 |
| 棘轮 | `python tools/code-quality-ratchet.py` | 各项不超阈值 |
| 静态专项 | `Select-String GetType().GetField/GetProperty` | 计数 0 |
| 运行期证据 | `MclslPerformanceProbe` 指标 / `MclslGuard.SwallowedCount` | 记录前后对比 |

> 说明：本机当前只有 Windows PowerShell 5.1、无 `pwsh`，`tools/compile-check.ps1` 等需 PowerShell 7 的脚本需在装有 pwsh 的环境（CI 或本机装 pwsh）执行；本方案不在评估阶段改动源码。
