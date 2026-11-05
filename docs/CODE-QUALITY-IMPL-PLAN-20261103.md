# 代码质量改进施工方案（除上帝类拆分）2026-11-03

> 依据：2026-11-03 对本项目 `code/MySimulatedLongevityRoad`（290 个 C# 文件、73,766 行）的只读静态评估 + 一次真实编译校验
> （`sources=292 exit=0 warnings=16/16`），并承接 `docs/CODE-QUALITY-IMPL-PLAN-20261102.md` 的既有结论。
> **本文件只描述"怎么改"，不含任何已落地的源码改动。**

---

## 0. 范围

### 0.1 本方案覆盖
| 编号 | 问题 | 本方案阶段 |
|---|---|---|
| P1-1 | 异常被系统性静默吞掉，故障不可见 | M2 |
| P1-3 | 静态全局状态 + 手工生命周期重置 | M6 |
| P1-4 | 全静态导致可测试性差 | M7 |
| P2-1 | 反射读无缓存 / Harmony 补丁失败不暴露 | M3（+ M2 步骤 4） |
| P2-2 | 重复样板（SafeName/StableHash/PositiveHash） | M4 |
| P2-3 | 可读性（超长行、单行 try/catch） | M5 |
| P2-4 | 缺静态分析器、警告预算被 DTO 噪声占满 | M1 |
| P3 | 工具链/仓库卫生（默认路径、入口文件命名等） | M1 |
| （可选） | 数据与代码混杂 | M8 |

### 0.2 明确不做
- **P1-2 上帝类拆分（Codex/Rank/Huanzhen/WorldRunRepository 等）——另行立项。** 理由：风险高、必须实机回归，且拆分本身不提升性能（编译器产物基本不变，引入接口反而可能略慢）。
- 不承诺任何"性能提升"；P2-1 的反射缓存是唯一与性能沾边的项，且必须 Unity 实测才可宣称。

---

## 1. 前置约束（AGENTS.md + 本机实测）

| 约束 | 说明 |
|---|---|
| 编译命令 | `powershell -NoProfile -ExecutionPolicy Bypass -File tools/compile-check.ps1 -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394'`。`tools/compile-check.ps1:2` 默认 `E:\...` **不存在**，必须显式传参（M1 会修）。 |
| 执行策略 | 本机 `LocalMachine` 为 `Restricted`，所有 `.ps1` 需 `-ExecutionPolicy Bypass`。 |
| 编译基线 | `sources=292 exit=0 warnings=16`，16 条全部为 `MclslLivingResources.cs` 的 `CS0649`。**警告数不得增加**；建议 M1 后把预算从 16 下调。 |
| 验收口径 | 离线测试/编译/内存估算**不能**标记为"游戏内 / 实机 / FPS / 内存"通过；需 Unity 运行记录 + 放置单位证据（AGENTS.md）。 |
| 交付 | 默认压缩玩家包（`tools/package-release.py` player + compress-assets），不降画质、不删有效资源。 |

---

## 2. 基线指标快照（施工前后对比用）

| 指标 | 基线（2026-11-03 实测） | 采集方式 |
|---|---|---|
| C# 文件 / 行 | 290 / 73,766（主目录）；编译含 root 2 个 = 292 | `tools/report-code-metrics.py` + 行数统计 |
| `catch (Exception)` | 305 | 正则计数 |
| 仅 `Probe` 吞掉的 catch | 157（74 个单行） | `empty-catch-code-` 计数 |
| `Error` / `Fatal` / `Alert` / `Notify` 调用 | 132 / 7 / 22 / 2 | 计数 |
| 长行 >200 / >400 / 最长 | 411 / 27 / 1010 | 逐行长度 |
| ≥500 / ≥800 / ≥1000 行文件 | 32 / 15 / 8 | 行数分布 |
| `static` 非只读可变字段 / `static` 类 | 695 / 259 | 正则计数 |
| `AccessTools.*` / 裸 `GetField|GetProperty` | 87 / 58 | 计数 |
| 重复 helper | `SafeName` 11、`StableHash` 12、`PositiveHash` 25 | 定义点计数 |
| 编译 | exit=0，warnings=16/16 | `compile-check.ps1` |

> 保存基线：`python tools/report-code-metrics.py` 的输出复制到 `build/quality-20261103/baseline/`，**不要覆盖** `docs/CODE-METRICS.json`。

---

## 3. 阶段总览

| 阶段 | 内容 | 优先级 | 风险 | 需编译 | 需实机 | 预估 |
|---|---|---|---|---|---|---|
| M0 | 基线固化 + 可回滚点 | P0 | 无 | 是 | 否 | 0.25 天 |
| M1 | 工具链 + 仓库卫生 + 静态分析预算（P3/P2-4） | P0 | 低 | 是 | 否 | 0.5–1 天 |
| M2 | 异常分级 + 补丁失败暴露（P1-1/P2-1） | P1 | 中 | 是 | 建议 | 2–3 天 |
| M3 | 反射读缓存（P2-1，性能相关） | P1 | 低-中 | 是 | 建议 | 1 天 |
| M5 | 可读性（P2-3） | P2 | 低 | 是 | 否 | 0.5 天 |
| M4 | 重复代码收敛（P2-2） | P2 | 中 | 是 | 否 | 1–1.5 天 |
| M6-A | 静态状态"漏清检测"（只诊断） | P1 | 低 | 是 | 建议 | 1 天 |
| M6-B | 静态状态注册表化（P1-3） | P1(长期) | 高 | 是 | **是** | 3–5 天 |
| M7 | 可测试性增强（P1-4） | P2 | 中 | 是 | 否 | 2–3 天 |
| M8 | 数据外置（可选） | P3 | 中 | 是 | 否 | 1–2 天 |

**推荐顺序**：M0 → M1 →（M2 / M3 / M5 可并行）→ M4 → M6-A → M6-B → M7 → M8。

---

## 4. 分阶段详细施工方案

### M0　基线固化与可回滚点（P0，最先做）
**目标**：让后续每个阶段都能单独回滚、单独验收。

**步骤**
1. 固化当前改动：当前工作区有 **19 个已跟踪文件被修改 + 5 个未跟踪项**（`git status`）。先提交或 `git stash` 快照；新建分支 `quality/20261103`，打 tag `baseline-20261103`。
2. 记录编译基线：运行 §1 的编译命令，保存 `compile.log` 与 `exit/warnings` 到 `build/quality-20261103/`。
3. 记录指标基线：运行 `python tools/report-code-metrics.py`，把结果存到 `build/quality-20261103/baseline/CODE-METRICS.json`（不覆盖 docs 版本）。
4. 生成源快照 zip（供 `tests/compare-offline.ps1`）：`build/quality-20261103/baseline/source-snapshot.zip`。
5. （若 M3 要做性能对比）在 Unity 中开 `MCLSL_config_performance_monitor`，采样年度结算帧耗时；无法实机则记录"未采集"。

**验收**：分支/tag 存在；baseline 文件齐全；编译记录为 exit=0/warn=16。
**风险/回滚**：无。删除分支即可。

---

### M1　工具链与仓库卫生 + 静态分析预算（P3 + P2-4）
**目标**：先让"验证入口本身可靠"，再谈代码改动。

**现状证据**
- `tools/compile-check.ps1:2`：默认 `-GameRoot 'E:\wol\gameversion\0_51_2_0.51.2'`，本机不存在（实测 `D:\owl\gameversion\worldbox-ab712b72f394` 存在）。
- `tools/build-release.ps1:2`：默认 `-GameRoot 'D:\owl\gameversion\0_51_2_imported'`，与 compile-check 不一致，需核对。
- `tools/verify-all.ps1:142`：调用 `tests/run.ps1`，但**实际目录是 `.tests/run.ps1`**（实测 `Test-Path tests/run.ps1=False`）。→ 全量验证会在测试步失败。
- `tools/deadcode-scan.ps1:66`：输出写死 `D:\模拟长胜路\_refcounts.csv`，与脚本第 2 行"避免硬编码本机绝对路径"的注释矛盾。
- `code/_probe_code.txt`：内容仅 `x`，属残留探针文件。
- 入口类 `class MclslMod` 在仓库根 `InterestingTrait.cs:14`（90 行），文件名与内容无关；且不在 `CODE-METRICS.json` 统计范围（scope=`code/MySimulatedLongevityRoad`）。
- `tools/package-release.py:22`：`expected_sources = {… startswith('code/')} | {'InterestingTrait.cs'}`，**改名前必须同步这里**，否则打包校验失败。
- `.editorconfig` 无任何 `dotnet_diagnostic.*` 严重级别；16 条警告预算被 `MclslLivingResources.cs:12-26` 的 `CS0649` 占满。

**步骤**
1. `compile-check.ps1:2`：把默认 `-GameRoot` 改为本机正确路径，或改为**必填**（为空即 `throw` 并提示）；同时可抽取为一个共享的默认常量供 `build-release.ps1` 复用。
2. `build-release.ps1:2`：核对并统一默认 `GameRoot`（与上一步一致）。
3. `verify-all.ps1:142`：`tests/run.ps1` → `.tests/run.ps1`。
4. `deadcode-scan.ps1:66`：输出改为相对路径（如 `Join-Path $base '..\build\refcounts.csv'`），去掉 `D:\模拟长胜路`。
5. 删除 `code/_probe_code.txt`。
6. 入口文件：**最小改动方案**——`InterestingTrait.cs` 原地更名为 `MclslMod.cs`，同步把 `package-release.py:22` 的 `{'InterestingTrait.cs'}` 改为 `{'MclslMod.cs'}`。**不要只改名不改打包脚本。**
   - 备选（风险略高）：迁入 `code/MySimulatedLongevityRoad/`，则 `package-release.py:22` 可只保留 `startswith('code/')`，但需确认 `build-release.ps1` 的源码收集仍包含它。
7. 静态分析（两级，任选/渐进）：
   - 轻量：在 `MclslLivingResources.cs` 顶部用 `#pragma warning disable CS0649` 包住 DTO 字段，把 16 条预算腾出来；再把 `compile-check.ps1:4` 的 `MaxWarnings` 从 16 下调。
   - 完整：`.editorconfig` 增加 `dotnet_diagnostic.CAxxxx.severity` 规则块，并在 `compile-check.ps1` 的 csc 参数加 `/analyzer:<NetAnalyzers.dll 路径>`（需先试验 csc.dll 直编能否加载该 analyzer）。
   - 建议先做"轻量"，"完整"另起小任务。

**验收**：`compile-check` exit=0 且 warnings ≤ 16（期望约 0）；`verify-all.ps1` 能顺利走到测试步；`report-code-metrics` 数字不回退。
**风险**：低（多为脚本/配置）。**回滚**：`git revert` 对应提交。

---

### M2　异常分级与可观测性（P1-1 + P2-1 补丁失败）
**目标**：让"该可见的失败可见"，同时不刷屏。

**现状证据**
- `MclslDiagnostics.cs:67-73` `Probe`：`if (!Enabled || …) return;` → 诊断关闭时**完全静默**。
- `MclslDiagnostics.cs:26-31` `Error`：`if (!Enabled) return;` → 同样受门控。
- 配置默认关闭：`default_config.json` 中 `MCLSL_config_enable_diagnostics` 的 `BoolVal=false`。
- 典型：`MclslCultivationSystem.cs:188` `try { actor.updateStats(); } catch(...) { Probe(...) }`。
- 全项目 `Error` 132 / `Fatal` 7 / `Alert` 22 → 默认配置下 132+157 个失败点无输出。
- `MclslHarmonyPatchGuard.cs:11-12` 定义 `FailedRequiredCount/Summary`，**全项目 0 处读取**；挂载失败仅 `Error`（受门控），生产环境静默。

**步骤**
1. 校准语义（不改调用点）：确定三档——`expected`（可选探测/UI 装饰，保留 `Probe`）、`degraded`（功能降级，用 `Alert`，常显一次）、`fatal`（存档/状态损坏，用 `Fatal`）。在 `MclslDiagnostics` 注释中写清三档适用场景。
2. 导出 157 处 probe 站点清单（脚本：grep `empty-catch-code-` 输出 `file:line: key: message`），逐条归类 A/B/C。**优先审**：
   - `Systems/WorldResources/MclslWorldRunRepository.cs`
   - `Systems/Reincarnation/MclslHuanzhenSystem.cs`（存档/还原）
   - `Systems/Runtime/MclslScheduler.cs`（持久化）
   - `Traits/MclslTraitRegistration.cs`
   - `Systems/Cultivation/MclslCultivationSystem.cs`（属性刷新）
3. 批量替换 B/C 站点：把自动生成的 `Probe("empty-catch-code-…", "空 catch 捕获: code/… #n: " + ex.Message)` 改为 `Alert(...)` / `Fatal(...)`，并把文案换成业务可读信息（如"存档还原失败""属性刷新失败"）。**保留 A 类为 `Probe`。**
4. 补丁失败暴露：把 `MclslHarmonyPatchGuard.FailedRequiredCount/Summary` 接入
   - `MclslMod.OnModLoad` 末尾：若 `>0` 用 `Fatal` 汇总；
   - 调试面板/命令输出（便于排查）。
   验收：模拟一个缺失补丁（或单测 mock `TryPatch(null,…)`）能看到 `Fatal`。
5. 性能向修正：热点处（每帧/每人路径）避免在异常分支外拼字符串——新增 `ProbeLazy(string key, Func<string> messageFactory)`，仅在 `Enabled` 时构造消息。热点 catch 改用 Lazy 版。
6. 单行 try/catch 拆多行（与 M5 合并做）。

**验收**：compile exit=0、warn 不增；诊断关闭时 B/C 仍输出、A 静默；日志量检查无异常刷屏（`Alert`/`Fatal` 有 60s 去重，见 `MclslDiagnostics.LogTick`）。
**风险**：中（日志级别改动可能刷屏）。**回滚**：按文件/系统分批提交。

---

### M3　反射读缓存（P2-1，性能相关，唯一可能提性能的项）
**目标**：消除每 actor/每年重复的反射解析；**不宣称 FPS，必须实测**。

**现状证据**
- 写路径已缓存：`Core/MclslReflectionNumbers.cs:33-47`（`(Type,string)` → `MemberAccessor` 的 `ConcurrentDictionary`；`FactoryCalls/CacheCount` :36-38）。
- 读路径未缓存：`Systems/Cultivation/MclslCultivationSystem.cs:191-214` `GetNumber` 每次 `GetField/GetProperty`；调用点 :182/:186/:187，位于年度修炼链 `ApplyNativeLevelFloor`（:167，被 :157 调用）。
- 其余裸 `GetField/GetProperty` 分布（58 处）：`MclslWorldSoulActorRegistration.cs`(13)、`MclslNativeKillStatisticsSystem.cs`(6)、`MclslTraitEditorEraFilter.cs`(4)、`MclslTraitRegistration.cs`(4)、`MclslMaobaoCommands.cs`(4)、`MclslBackpackTabSystem.cs`(4) 等。

**步骤**
1. 在 `MclslReflectionNumbers` 增加 `TryGetNumber(object target, string name, out int value, string site, …)`（以及 float/long/bool 变体），复用现有 `Cache` 与 `BindingFlags`（:23-47）。
2. 用缓存版替换 `MclslCultivationSystem.GetNumber`，删除私有反射实现（已核实仅本文件使用）。
3. 审计其余 58 处：
   - 数值读写 → 走 `MclslReflectionNumbers`；
   - 只读结构/委托（多在各 `Mclsl*Patches.cs`，已用 `AccessTools.Method` 一次性解析）→ 若在循环内重复 `AccessTools.*`，提升为 `static readonly FieldInfo/MethodInfo`。
4. 用 `FactoryCalls/CacheCount` 验证"每 (Type, member) 只解析一次"。

**验收**：compile exit=0；`.tests/SharedHelperTests` 新增"同一 (type,name) 多次读取 `FactoryCalls` 只 +1"用例并通过；性能仅在 Unity 实测后标注（否则"未实机"）；可跑 `.tests/RuntimeQualityBenchmarks` 做微基准参考（README 已说明其不满足正式 Unity 性能矩阵）。
**风险**：低-中（int/float 语义兼容）。**回滚**：`git revert`。

---

### M4　重复代码收敛（P2-2）
**目标**：消除复制粘贴工具函数。

**现状证据**：`SafeName` 11 处（抽查 `MclslFactionMissionSystem.cs:409` 与 `MclslTianxuanMarket.cs:501` 实现完全相同：`return MclslActorAccessor.DisplayName(actor)`）；`StableHash` 12 处；`PositiveHash` 25 处（`Data/MclslAptitudeGiftCatalog.cs:112` … `Systems/WorldResources/MclslWorldChangeSystem.cs:699`）。

**步骤**
1. `MclslActorAccessor` 增加 `internal static string SafeName(Actor)`（或直接别名 `DisplayName`），替换 11 处私有实现。
2. 在 `Core/MclslStableHash.cs` 统一 `StableHash` / `PositiveHash`；替换 12 + 25 处副本。
   - **前置硬门槛**：逐副本 diff 算法。只有"逐字等价"的副本才可合并替换；**若哈希算法不同，替换会改变姓名/生成物/事件 id 的确定性，可能破坏旧存档一致性**——这类副本本轮不改，另列清单单独评估。
3. 替换后 compile + 跑相关测试（`SharedHelperTests`、名称/生成物相关用例）。

**验收**：compile exit=0；`.tests` 相关通过；固定种子输出与基线一致（必要时 `compare-offline.ps1`）。
**风险**：中（hash 确定性）。**回滚**：按文件分批提交。

---

### M5　可读性（P2-3）
**现状证据**：411 行 >200 字符，27 行 >400，最长 1010（`MclslHuanzhenSystem.cs:1339-1346` 的长对象初始化）；74 处单行 `try{…}catch{…}`。

**步骤**
1. `MclslHuanzhenSystem.cs:1339-1346`：对象初始化逐属性换行。
2. 脚本化拆分 74 处单行 try/catch（与 M2 步骤 6 合并）。
3. `.editorconfig` 增 `max_line_length`（仅提示；csc 不强制），并新增 `tools/check-long-lines.ps1` 作为非阻断检查纳入 `verify-all` 可选步。
4. 纯格式化，单独 commit（diff 只含换行/缩进）。

**验收**：长行数下降；compile exit=0；无逻辑改动。
**风险**：低。**回滚**：单 commit revert。

---

### M6　静态状态生命周期治理（P1-3）—— 分两步

#### M6-A　漏清检测（低风险，只诊断，不改行为）
**现状证据**：695 个 `static` 非只读字段、259 个 `static` 类；`MclslRuntime.cs:210-253` `ClearWorldState()` 手写 40+ 个 `Xxx.Clear()`；全项目 87 个分散 `Clear/ClearRuntime/Reset`。

**步骤**
1. 新增 `Core/MclslWorldStateAudit`：诊断开启时，在世界卸载/重载前后，用反射枚举各 `Mclsl*` 类型的 `static` 字段，比较"可测量值"（`Dictionary/List.Count`、`bool`、引用是否为 null 等）是否归零；输出 `Alert` 汇总"未回收静态状态：Type.field=value"。
2. 接入 `MclslRuntime.ClearWorldState()` 末尾与 `OnWorldLoaded()` 开头。
3. 用一次"加载→卸载→再加载"（Unity 或模拟）收集漏清清单，作为 M6-B 输入；无法实机则标注"未实机"。

**验收**：compile exit=0；诊断关闭无输出；开启能列出未清字段。
**风险**：低（注意排除 `readonly` 常量与运行时单例）。**回滚**：独立文件，易删。

#### M6-B　注册表化（高风阸，需实机）
**步骤**
1. 定义 `internal interface IWorldScoped { void ClearWorldState(); }` 或静态注册 `MclslWorldScope.Register(Action clear)`，各系统在 `Ensure/静态构造` 时注册。
2. `MclslRuntime.ClearWorldState` 改为遍历注册表，替代手写 40 行；保留手写清单作为 feature flag 后备。
3. 分批迁移：先数据层（`MclslWorldRunRepository` 索引、`MclslTraitRegistration` 缓存、Beast 状态机，这些可离线测试）→ 再运行时/UI（需实机）。
4. 每批：compile + `.tests`（世界清理用例）+ 一次实机"加载/换档/读档 3 次"验证状态不残留、不重复计数。

**验收**：M6-A 漏清清单归零；实机换世界 3 次计数/内存不增长；附 Unity 运行记录。
**风险**：高——生命周期错位会导致状态提前清空或未清。**回滚**：分系统提交 + feature flag。

---

### M7　可测试性增强（P1-4，渐进）
**现状证据**：259/399 类为 `static`；`.tests` 9 个项目，替身式，链接生产 `.cs`（`.tests/README.md` 明确其不替代 Unity）。
**步骤**
1. 选取纯逻辑热点（价格/境界映射/名字生成/排程策略），抽为可实例化类或无状态纯函数，**保持外部调用签名不变**（内部委托）。
2. 为抽出逻辑补 `.tests` 用例（沿 `projects.json` 机制）。
3. 为 M6-A 的静态字段审计写离线替身测试。
**验收**：`.tests/run.ps1` 全绿；新增用例覆盖抽出逻辑。
**风险**：中（重构边界）。**需实机**：否。

---

### M8　（可选）数据外置（P3 数据/代码混杂）
**现状**：`MclslHonorificNameCatalog.cs` 948 行、`MclslItemCatalog` 大段硬编码数据表。
**步骤**：迁为 JSON 资源，加载解析并校验一致性。
**权衡**：可维护性↑，加载成本略增；优先级最低，可暂缓。

---

## 5. 通用验收命令
```powershell
# 编译（每阶段必跑）
powershell -NoProfile -ExecutionPolicy Bypass -File tools/compile-check.ps1 -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394'
# 指标（对比 baseline）
python tools/report-code-metrics.py
# 全量（M1 修复 verify-all 后可用）
powershell -NoProfile -ExecutionPolicy Bypass -File tools/verify-all.ps1 -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394'
# 离线回归
powershell -NoProfile -ExecutionPolicy Bypass -File .tests/run.ps1 -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394'
```
判据：exit=0；warnings ≤ 基线（16，M1 后更低）；指标不回退；无新增失败测试。
性能（仅 M3/M6）：Unity 内开 `MCLSL_config_performance_monitor` 采样；未实机则标注"未测"。

---

## 6. 需要拍板的决策点
1. **M6-B 是否本轮做？** 建议只做 M6-A（诊断），M6-B 另立高风险项需实机。
2. **入口文件**：原地更名 `InterestingTrait.cs → MclslMod.cs`（推荐，最小影响），还是迁入 `code/`？
3. **M4 StableHash**：仅合并"逐字等价"副本，还是允许统一算法（会影响确定性 id/旧存档）？建议前者。
4. **静态分析**：先做轻量（清 CS0649 + 下调预算），还是直接引入 NetAnalyzers（需试验 csc `/analyzer`）？
5. 是否要我按 M0 起逐阶段执行（每阶段单独编译+提交）？

---

## 7. 风险与纪律
- 一次只做一个阶段；每阶段**单独编译 + 单独提交**，可单独回滚。
- 动代码前留 `_pre-change-backup/` 快照。
- 高风险项（M4 hash、M6-B 状态）必须分批 + feature flag + 实机验证。
- 日志级别变更（M2）上线前检查日志量，避免刷屏。
- 所有数字由脚本采集，不手写；本文件不含已落地改动。

> 明确边界：M1–M5、M7 多数可在离线/编译层验收；**M6-B 及任何"内存/FPS"结论必须 Unity 实测**，不得以离线测试代替实机通过记录。
