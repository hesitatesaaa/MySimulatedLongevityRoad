# 代码质量改进施工方案（2026-11-02）

> 依据：2026-11-02 对本项目 `code/MySimulatedLongevityRoad`（288 个 C# 文件、67,919 行）的只读静态评估，
> 以及 `docs/CODE-QUALITY-PLAN-20261031.md`（963 行）的既往结论。
> 本文件只描述"怎么改"，不含任何已落地的源码改动。

---

## 0. 前置条件与全局约束

### 0.1 编译能力（已具备，但默认参数过期）

- 本机存在游戏目录：`D:\owl\gameversion\worldbox-ab712b72f394`（`Test-Path = True`）。
- `tools/compile-check.ps1` 的默认 `-GameRoot` 仍指向**不存在**的 `E:\wol\gameversion\0_51_2_0.51.2`（见脚本第 2 行），
  因此**每次调用必须显式传参**：
  ```powershell
  pwsh -NoProfile -File tools/compile-check.ps1 -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394'
  ```
- 基线要求：先跑一次记录 `error=0`、`warning=N`（历史基线 16 条，全部为 `MclslLivingResources.cs` 的 CS0649 DTO 噪声）。
  之后每个阶段结束后重跑同一命令，**warning 不得增加**。
- **执行策略约束（已发现）**：本机 `LocalMachine` 执行策略为 `Restricted`，运行 `tools/*.ps1`、`.tests/run.ps1` 需加 `-ExecutionPolicy Bypass`，否则报"禁止运行脚本"。上述命令实际应写成：
  ```powershell
  pwsh -NoProfile -ExecutionPolicy Bypass -File tools/compile-check.ps1 -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394'
  ```

### 0.2 验收口径（遵循 AGENTS.md）

- 离线测试、替身测试、内存估算、图片审看**不能**标记为"游戏内尺寸 / 实机 / CPU-GPU 内存"通过。
- 需要实机的内容（初始化去重、上帝类拆分、空 catch 影响面）必须给出 **Unity 运行记录 + 放置单位/世界加载证据**，否则只写"未实机验证"。
- 默认交付按 `tools/package-release.py` 的 player + compress-assets 路径，不降低画质、不删有效资源。

### 0.3 施工纪律

1. 一次只做一个阶段；每阶段 **单独编译验收 + 单独提交**。
2. 改动前用 `.archive` 或 `_pre-change-backup/` 留快照；每阶段能单独回滚。
3. 优先做"零逻辑风险"的改动（诊断开关、仓库整理），再做需要实机的核心路径。
4. 所有数字（文件数/行数/警告数）由脚本采集，不手写进文档。

### 0.4 当前基线指标（供每阶段对比）

| 指标 | 当前值 | 采集方式 |
|---|---|---|
| C# 文件 / 行数 | 288 / 67,919 | `Get-ChildItem -Recurse -Filter *.cs` |
| `catch` 总数 | 456 | `Select-String '\bcatch\b'` |
| `catch (Exception)` | 305 | `catch\s*\(\s*(System\.)?Exception` |
| `empty-catch-code-*` 调用点 | 157（41 个文件） | `Select-String 'empty-catch-code-'` |
| `MclslDiagnostics.Error` 调用 | 288 | 计数 |
| 裸 `Debug.Log*`（诊断类外） | 34（12 个文件） | 计数 |
| 反射面 | AccessTools 87 / GetField 35 / GetProperty 24 / BindingFlags 43 / MemberInfo-Info 105 | 计数 |
| 500+ / 800+ 行文件 | 32 / 15 | 行数分布 |

---

## 1. 阶段总览

| 阶段 | 内容 | 优先级 | 风险 | 需编译 | 需实机 | 预估 |
|---|---|---|---|---|---|---|
| M0-B | 仓库版本控制整理 + 基线记录 | P0 | 低 | 否 | 否 | 0.5 天 |
| M1-A | 诊断开关语义 + 配置项 | P0 | 低 | 是 | 建议 | 0.5 天 |
| M2-C | 反射访问缓存 | P1 | 低-中 | 是 | 否 | 1 天 |
| M2-F | 日志统一到 MclslDiagnostics | P2 | 低 | 是 | 否 | 0.5 天 |
| M2-G | MclslRuntime 初始化去重 | P2 | 中 | 是 | 是 | 0.5 天 |
| M3-D | 空 catch key 规范化（批量） | P1 | 中 | 是 | 建议 | 1.5 天 |
| M3-H | 指标脚本 + 文档同步 | P2 | 低 | 否 | 否 | 0.5 天 |
| M3-I | .editorconfig / 分析器 / CI 修复 | P2 | 低 | 是 | 否 | 0.5 天 |
| M3-J | 测试桩收敛 + 回归入清单 | P2 | 中 | 是 | 否 | 1 天 |
| M4-E | 上帝类拆分 | P1(长期) | 高 | 是 | 是 | 另立计划 |
| M5-K | 工具脚本卫生 / Conditional 澄清 | P3 | 低 | 否 | 否 | 0.5 天 |

> 推荐先做 **M0-B**，让后续每个阶段的改动都有可回滚的版本历史，再做代码修改。

---

## 2. 分阶段详细施工方案

### 阶段 M0-B：仓库分仓整理（P0，最先做）— 已定"分仓"

**目标**：源码与美术二进制分仓管理，各自恢复可评审/可回滚能力；工作区文件不移动，保证 mod 加载与工具路径不变。

**现状证据**
- `git ls-files` 共 4,111 条，**全部落在已从工作区删除的 `optimization-0.8.1-20261030/`**；`git status` 大量 `D`。
- 当前 `code/ tools/ docs/ resources/ GameResources/ ...` 全部**未跟踪**（18 个顶层未跟踪项）。
- `.gitignore` 的 `optimization-*/` 不能取消已跟踪文件（Git 只忽略未跟踪文件）。
- 无 remote（本地仓库）、无 `.gitattributes`；`deliverables/` 被忽略（`git ls-files deliverables` = 0）。

**实测体积（决定分仓边界）**

| 目录 | 文件数 | 体积 | 归属 |
|---|---|---|---|
| code | 290 | 3.62 MB | 源码仓 |
| tools | 55 | 0.30 MB | 源码仓 |
| docs | 24 | 0.22 MB | 源码仓 |
| resources（多为 json 清单） | 1,484 | 12.47 MB | 源码仓 |
| Locales（文本） | 2 | 0.18 MB | 源码仓 |
| .tests（不含 bin/obj） / .dev-tools / .github / 根文本 | — | <0.5 MB | 源码仓 |
| GameResources（png） | 833 | 43.95 MB | 资源仓 |
| NativeActorResources | 1,375 | 22.30 MB | 资源仓 |
| InterfaceResources | 23 | 5.62 MB | 资源仓 |
| `.archive` / `build` / `deliverables` / `.codegraph` | — | 3,894 / 395 / 81 / 58.7 MB | 不入库（保持忽略/本地快照） |
| 现有 `.git`（旧树历史） | — | **1,224 MB** | 备份后重建（旧历史无用） |

**分仓设计（两个独立仓库共享同一工作区）**

- 源码仓：`D:\模拟长胜路\.git`，跟踪 `code/ tools/ docs/ resources/ .tests/ .dev-tools/ .github/`、根文件、`Locales/`。
- 资源仓：独立 git 目录 `D:\模拟长胜路\.git-assets`（bare + 显式 `GIT_DIR`/`GIT_WORK_TREE`），工作区同为 `D:\模拟长胜路`，**只**跟踪 `GameResources/ InterfaceResources/ NativeActorResources/`。
- 两仓各自排除对方路径（源码仓 `.gitignore` 忽略三个资产目录；资源仓用 `.git-assets/info/exclude` 忽略其余全部），避免互相显示为未跟踪。
- 因工作区不移动，`compile-check.ps1`、`.tests`、打包脚本的相对路径全部不变。

**执行步骤（先在临时副本上彩排，再落地）**
1. 备份现有 `.git`（1.2 GB，历史仅 2 个旧 import 提交）：
   ```powershell
   git bundle create build/repo-backup-20261102.bundle --all
   Compress-Archive .git build/git-backup-20261102.zip
   ```
2. 重建源码仓（丢弃 1.2 GB 旧历史：旧树是已删除的 optimization 目录）：
   ```powershell
   Remove-Item .git -Recurse -Force         # 已在上一步备份
   git init
   # 写 .gitignore：忽略 GameResources/ InterfaceResources/ NativeActorResources/ + build/ deliverables/ .archive/ .codegraph/ bin/ obj/ *.dll *.pdb *.exe
   git add -A
   git status --short                        # 人工核对：无资产目录、无 bin/obj/dll
   git commit -m "chore: import source/tools/docs (0.8.4)"
   ```
3. 建资源仓（共享工作区）：
   ```powershell
   git init --bare .git-assets
   git --git-dir=.git-assets config core.bare false
   git --git-dir=.git-assets --work-tree=. add GameResources InterfaceResources NativeActorResources
   git --git-dir=.git-assets --work-tree=. commit -m "chore: import binary art assets (0.8.4)"
   ```
4. 两仓各写 `.gitattributes`（`*.png binary`、`*.cs text eol=lf`、`*.ps1 text eol=crlf`）。
5. 验收：
   - 源码仓 `git ls-files GameResources InterfaceResources NativeActorResources` 为空；`git ls-files` 无 `*.dll/*.pdb/*.exe`。
   - 资源仓 `git --git-dir=.git-assets ls-files` 计数 ≈ 2,231（833+1,375+23）。
   - 两仓 `status` 均干净；`compile-check.ps1` 与 `.tests/run.ps1` 仍可运行。

**风险与回滚**
- 风险：**高**（重建 `.git` 会丢弃旧历史——旧历史为无用旧 import；务必先完成第 1 步备份）。工作区文件不移动，故源码零风险。
- 回滚：用 `build/git-backup-20261102.zip` 解压覆盖 `.git` 即可回到原状态。
- 沙箱提示：`.git`、`.git-assets` 均在 `D:\模拟长胜路` 内，符合 workspace-write 边界。

---

### 阶段 M1-A：诊断开关语义与配置项（P0）

**目标**：让"诊断"确实受配置控制；把 157 处自动生成的空 catch 探针从 `Error` 降级，避免世界加载刷屏。

**现状证据**
- `MclslDiagnostics`（`Core/MclslDiagnostics.cs`）：
  - L9 `Enabled => MclslRuntimeSettings.DiagnosticsEnabled`；
  - `Once`(L46) / `Throttle`(L54) / `Cultivation`(L75) 都有 `if (!Enabled ...) return;`；
  - **`Error`(L63-70) 没有 Enabled 判定** → 288 处 Error 恒定输出。
- `MclslRuntimeSettings.cs`：`_diagnosticsEnabled` 默认 `false`（L49），L147 从 `MCLSL_config_enable_diagnostics` 读取；
  但 `default_config.json`（50 个 `MCLSL_config_*` 键）**没有该配置项**，`MclslConfig` 也没有对应回调。
  即：诊断永远为 false，但 Error 永远在打日志。
- `Error` 的节流单位问题：`Clock` 在非主线程返回 `Environment.TickCount`（毫秒），却与 `3600`（帧）比较，跨线程窗口变成 3.6s。

**施工步骤**

A1. **增加配置项**（暴露开关，默认关闭）
- `default_config.json` 在 `MCLSL_config_performance_monitor` 附近增加（沿用现有 schema）：
  ```json
  {
    "Id": "MCLSL_config_enable_diagnostics",
    "Name": "诊断日志",
    "Description": "输出模组内部诊断日志（含反射探测、防御分支）；默认关闭，排查问题时再开。",
    "Type": "SWITCH",
    "BoolVal": false,
    "Callback": "MclslConfig:DiagnosticsCallBack"
  }
  ```
- `Core/MclslConfig.cs` 增加回调（与该文件其它回调同构）：
  ```csharp
  public static void DiagnosticsCallBack(bool value)
      => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
  ```
- `MclslRuntimeSettings.LoadFromModConfig` 已读取该键（L147），无需改。

A2. **让 Error 受开关控制，并拆分"始终可见"的错误**
- 在 `MclslDiagnostics` 增加两档语义：
  ```csharp
  // 诊断级：受 DiagnosticsEnabled 控制
  internal static void Error(string key, string message)
  {
      if (!Enabled) return;                      // 新增
      LogOncePerWindow("error:" + key, message, 3600, Debug.LogWarning);
  }

  // 关键级：始终输出（存档损坏、外部状态写入失败等）
  internal static void Fatal(string key, string message)
      => LogOncePerWindow("fatal:" + key, message, 3600, Debug.LogError);
  ```
- 修正节流单位：`Clock` 主线程返回 `Time.frameCount`、非主线程返回 `TickCount`（ms），二者不可混用同一个间隔常量。
  建议 `LogOncePerWindow` 显式接收"秒"并用 `Environment.TickCount`（ms）统一计时；或对 worker 线程单独用 ms 常量（≥60000）。
- **迁移清单（必须人工甄别）**：把少数"必须始终可见"的调用从 `Error` 改为 `Fatal`，例如：
  - `Systems/Reincarnation/MclslHuanzhenSystem.cs:88`（外部状态读取失败，已禁止覆盖）
  - `Systems/WorldResources/MclslWorldArchiveStore.cs:58`（主档及备份均不可读）
  - 其余存档/写入锁相关日志。
  用 grep 核对：`Select-String 'MclslDiagnostics\.Error\('`，按 message 前缀分类。

A3. **探针降级**：把 157 处 `empty-catch-code-*` 的 `Error` 改成 `Once`（或新增 `Probe`），
使其在诊断关闭时完全静默、开启时每 key 只打一次：
```csharp
// 由脚本批量把
MclslDiagnostics.Error("empty-catch-code-...", "空 catch 捕获: ...");
// 改为
MclslDiagnostics.Probe("empty-catch-code-...", "空 catch 捕获: ...");
// 其中 Probe 语义 = Enabled ? LogOnce : nothing
```

A4. **（已决定：不做）** `MCLSL_CULTIVATION_DIAGNOSTICS` 全项目未定义，50 处 `MclslDiagnostics.Cultivation(...)` 实际被编译器连同参数一起移除。
用户已确认"不需要修炼链诊断"，故**不定义该符号、不改这 50 处调用**；在文档中注明"修炼链日志为编译期剥离能力，需专用构建"即可。

**验收**
- 配置关闭时：世界加载/切换后**无** `[模拟长生路][诊断]` 输出；开启后仅按 key 去重输出。
- `git grep 'MclslDiagnostics.Error('` 数量应从 288 降到"真错误 + 待迁移"范围；`empty-catch-code` 前缀不再出现在 `Error`。
- 编译 `error=0`、warning 不增。
- 实机：新建世界、读档、切换世界各一次，观察日志与功能正常（建议，非必须）。

**风险与回滚**：低。仅日志级别与配置项，不动业务逻辑。回滚：还原 `MclslDiagnostics.cs`、`MclslConfig.cs`、`default_config.json`。

---

### 阶段 M2-C：反射访问缓存（P1）

**目标**：消除热路径重复反射，降低 CPU 与类型裁剪/改名风险。

**现状证据**
- `Core/MclslReflectionNumbers.cs` L25/L32：每次调用都 `GetType().GetField(name, Flags)` 再 `GetProperty(...)`。
- 全项目 `AccessTools` 87、`GetField(` 35、`GetProperty(` 24、`BindingFlags` 43，缓存型辅助仅 1 处。
- `MclslReflectionNumbers.TrySetNumber` 仅 4 个调用点，其余反射分散在约 35+ 处（含 `AccessTools` 补丁绑定）。

**施工步骤**
1. 先给 `MclslReflectionNumbers` 加缓存（收益最集中、改动最小）：
   ```csharp
   private readonly struct MemberAccessor
   {
       internal readonly FieldInfo Field;
       internal readonly PropertyInfo Property;
       internal MemberAccessor(FieldInfo f, PropertyInfo p) { Field = f; Property = p; }
   }
   private static readonly ConcurrentDictionary<(Type, string), MemberAccessor> Cache = new();

   private static MemberAccessor Resolve(Type type, string name)
       => Cache.GetOrAdd((type, name), static k =>
       {
           FieldInfo f = k.Item1.GetField(k.Item2, Flags);
           PropertyInfo p = f == null ? k.Item1.GetProperty(k.Item2, Flags) : null;
           return new MemberAccessor(f, p);
       });
   ```
   - 注意线程安全：`MclslDiagnostics` 注释表明存在 worker 线程回调，必须用 `ConcurrentDictionary`。
   - 缓存随世界清理**不应清空**（类型元数据不随世界变化）；如担心内存，用上限或按 Type 弱引用。
2. 分批把其它高频反射（尤其年度/逐角色循环内的 `GetField/GetProperty/AccessTools.Method`）收敛到共享工具。
  建议先做静态扫描定位"循环体内反射"：grep `for (`/`foreach (` 附近 3 行内的 `GetField|GetProperty|AccessTools`。
3. 预热点（可选）：世界加载时对已知类型预热 `Resolve`，把首次反射成本移出帧循环。

**验收**
- 单测：在 `SharedHelperTests` 增加"连续两次 `TrySetNumber` 只解析一次"的断言（给 `Cache` 暴露只读计数器或注入解析计数）。
- 性能：用 `RuntimeQualityBenchmarks` 或 `MclslPerformanceProbe` 对比改动前后反射耗时（记录数字，不以离线估算冒充实机）。
- 编译 `error=0`、warning 不增；关键功能回归通过。

**风险与回滚**：低-中。缓存 key 需含 `IgnoreCase` 语义（保持一致）；mod 无热重载则无需失效。回滚：单文件改动，直接还原。

---

### 阶段 M2-F：日志统一（P2）

**目标**：消除"两套日志"，让所有输出受同一开关/节流约束。

**现状证据**：12 个文件、34 处裸 `Debug.Log/LogWarning/LogError`：
`MclslDeveloperBridge.cs`、`MclslStartupTiming.cs`、`MclslPatches.cs`、`MclslHuanzhenSystem.cs`、
`MclslAnnouncementSystem.cs`、`MclslMapMarkerVisualSystem.cs`、`MclslNativeHistoryBridge.cs`、
`MclslWorldArchiveStore.cs`、`MclslImmortalActorRegistration.cs`、`MclslTraitRegistration.cs`、
`MclslWorldSoulActorRegistration.cs`、`MclslUiManager.cs`。

**施工步骤**
1. 在 `MclslDiagnostics` 提供分级封装：`Log`（调试/受开关）、`Warn`（受开关）、`Error`（受开关）、`Fatal`（始终）。
2. 逐文件替换裸 `Debug.Log*`：
   - 启动耗时 `MclslStartupTiming` 可保留独立"启动计时"开关，但仍走封装。
   - `MclslAnnouncementSystem`、`MclslWorldArchiveStore` 的玩家可见/存档类日志归入 `Fatal`/`Warn`。
3. 验收：`Select-String 'Debug\.Log'` 在 `MclslDiagnostics.cs` 之外为 0（封装内部除外）。

**风险**：低。回滚：逐文件还原。

---

### 阶段 M2-G：MclslRuntime 初始化去重（P2）

**目标**：消除 `OnWorldLoaded` 与 `Tick` 中重复的"核心模块加载"逻辑。

**现状证据**：`Core/MclslRuntime.cs`
- `OnWorldLoaded` L37-75 与 `Tick` L108-119 对 `EnsureCurrentRun / EnsureRegistered / KillStatistics.OnWorldLoaded / LoadCoreModules` 几乎逐行重复，`_coreWorldInitialized` 两处都置位。

**施工步骤**
1. 抽出幂等方法：
   ```csharp
   private static bool TryLoadCoreWorld(int year)
   {
       if (_coreWorldInitialized) return true;
       if (!MclslRuntimeSettings.CoreEnabled || World.world?.map_stats == null) return false;
       _coreWorldInitialized = true;                 // 失败也只尝试一次（保留原语义）
       MclslWorldStateModifierSystem.ClearCache();
       MclslWorldArchiveStore.Load();
       if (MclslWorldArchiveStore.WriteLocked) return false;
       MclslWorldRunRepository.EnsureCurrentRun(year);
       MclslNativeHistoryBridge.EnsureRegistered();
       Systems.Death.MclslNativeKillStatisticsSystem.OnWorldLoaded(year);
       LoadCoreModules(year);
       return true;
   }
   ```
2. `OnWorldLoaded` 与 `Tick` 分支改为调用它；`LoadCoreModules` 内部不再重复置 `_coreWorldInitialized`。
3. 特别保留：`WriteLocked` 早退后**不得**把 `_coreWorldInitialized` 设为已完成（否则永不重试），需按原行为确认（原 `Tick` 分支在 WriteLocked 时直接 return，未完成加载）。

**验收**：编译通过；实机验证"新世界 / 读档 / 切换世界"三条路径均能正确加载核心模块；`WriteLocked` 场景不丢加载。

**风险**：中（核心加载路径）。必须实机验证；回滚：还原 `MclslRuntime.cs`。

---

### 阶段 M3-D：空 catch key 规范化（P1，批量机械改动）

**目标**：把 `empty-catch-code-*` 的 key 与物理路径/序号解耦，并按"预期失败 vs 真异常"分级。

**现状证据**：157 处 / 41 文件，key 形如 `empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-1`，
消息内嵌 `code/MySimulatedLongevityRoad/...cs #1`。示例：`Traits/MclslTraitRegistration.cs:179`。

**施工步骤**
1. 用脚本导出全部 157 处（文件、行、当前 key、message 前缀）到 CSV，作为分类底稿：
   `Select-String 'empty-catch-code-' | Export-Csv build/empty-catch-inventory.csv`
2. 人工分类为三类：
   - (a) **反射/可选成员探测**（预期失败）→ `Probe`（诊断关闭即静默）。
   - (b) **UI/渲染防御**（可恢复）→ `Once` / `Probe`。
   - (c) **存档/外部状态/写入锁**（严重）→ `Fatal`（始终记录）。
3. 用脚本按分类批量替换 `Error(...)` 为对应级别；key 统一改为"模块 + 用途"的稳定语义名，例如：
   `"probe:ui.info-panel.hide-actions"`、`"fatal:storage.archive-write"`。
   不再写物理路径和 `#N`（需要定位时用调用栈或 `nameof`）。
4. 人工抽检 20%，确保没有把"真错误"降级为静默。

**验收**
- `Select-String 'empty-catch-code-'` 归零，或仅剩约定的少量豁免。
- `catch` 总数不变（不改控制流；只是日志级别/key）。
- 编译 `error=0`；`.tests` 回归 + 关键路径实机抽查。

**风险**：中（碰 41 文件，但只改日志参数，不改业务逻辑）。分批提交：先 (c) 严重类，再 (a)(b) 大批量。

---

### 阶段 M3-H：指标脚本 + 文档同步（P2）

**目标**：消除文档数字漂移。

**现状证据**：`README-DEVELOPER.md:11` 写 278、`docs/PROJECT-STRUCTURE.md:14` 写 284、旧方案写 274/276，实际 288；目录名 0.8.1 但 `mod.json` 是 0.8.4。

**施工步骤**
1. 新增 `tools/report-code-metrics.py`：输出 `docs/CODE-METRICS.json`，含文件数/行数/目录分布/catch 统计/empty-catch 统计/反射统计。
2. `compile-check.ps1` 执行后追加写入 warning 数到同一 JSON。
3. 文档改为"引用脚本输出"（如"当前 288 个文件，见 CODE-METRICS.json"），并同步版本号 0.8.4。
4. 验收：脚本可重复执行、结果稳定；文档数字 == 脚本输出。

**风险**：低。

---

### 阶段 M3-I：.editorconfig / 分析器 / CI 修复（P2）

**目标**：把质量门禁前移；修复实际会失败的 CI。

**现状证据**
- 无 `.editorconfig`、无 `Directory.Build.props`、无分析器。
- `.github/workflows/skill-quality.yml` 只校验 `deliverables/token-efficient-workflow`（**该目录被 .gitignore，`git ls-files` = 0，CI checkout 拿不到**），且引用不存在的 `validation/` 目录（`Test-Path = False`）→ 该 workflow 必失败。

**施工步骤**
1. 新增 `.editorconfig`（示例）：
   ```ini
   root = true
   [*]
   charset = utf-8
   end_of_line = lf
   indent_style = space
   indent_size = 4
   insert_final_newline = true
   trim_trailing_whitespace = true
   [*.{json,yml,yaml}]
   indent_size = 2
   ```
2. 分析器：在 `compile-check.ps1` 的 csc 参数中固定 `warning` 基线；CI 若 warning 数 > 基线则失败。
   （可选）引入 `Microsoft.CodeAnalysis.NetAnalyzers` 并只对新增 warning 报错。
3. 修 CI（已定：迁移技能包 + 补 validation/，不删除 workflow）：
   - 新建受跟踪目录 `skills/token-efficient-workflow/`，把 `deliverables/token-efficient-workflow` 的 `SKILL.md`、`agents/openai.yaml`、`references/specialized.md` 迁入；`deliverables/` 继续忽略。
   - 新建 `validation/skill_package_audit.py` 与 `validation/test_skill_package.py`（当前不存在，需补齐；`--skill-dir` 指向新路径、token 预算参数沿用 workflow 中的 800/1200）。
   - 更新 `.github/workflows/skill-quality.yml` 的 `--skill-dir skills/token-efficient-workflow`。
4. 若要让 CI 编译模组：使用带 WorldBox 依赖的自托管 runner 跑 `compile-check.ps1 -GameRoot`。

**验收**：`act`/本地模拟或真实 push 后 CI 至少不因"路径缺失"失败；`python validation/skill_package_audit.py --skill-dir skills/token-efficient-workflow ...` 本地可跑通；编译 warning ≤ 基线。

**风险**：低。

---

### 阶段 M3-J：测试体系收敛（P2）

**目标**：降低桩与生产签名的"静默不同步"风险，让纯逻辑回归可离线跑。

**现状证据**：`.tests` 直接 link 生产 `.cs`（优点），但各项目自带 `Stubs.cs` 复制生产类型签名
（如 `SharedHelperTests/Program.cs:6-38` 自写 `ActorAsset`/`MclslDiagnostics`）；依赖本机游戏路径，普通 CI 跑不了；
旧方案 §1.2 记录 `LineageCleanupTests` 曾引用已删的 `MclslCleanupPolicy.cs`。

**施工步骤**
1. 建 `.tests/Shared/GameStubs.cs`，所有测试 csproj 用 `<Compile Include="../Shared/GameStubs.cs" Link="Shared/GameStubs.cs"/>` 共用一份，删除各项目内重复桩。
2. 核对 `LineageCleanupTests` 引用是否仍指向已删除文件（`MclslCleanupPolicy.cs`），若已修复则记录；未修复则按旧方案内联到 `MclslFamilySystem` 的现状修正。
3. 把纯逻辑项目（`SharedHelperTests`、`LineageCleanupTests`、`EconomyPlanScopeTests` 等不依赖 Unity 运行时的）整理为可 `dotnet test`/直接 `dotnet run` 的离线套件。
4. 验收：`.tests/run.ps1 -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394'` 全绿；`projects.json` 无失效条目。

**风险**：中（测试结构调整）。回滚：还原 `.tests`。

---

### 阶段 M4-E：上帝类拆分（P1 长期，需实机）

**目标与设计**：见旧方案 §6。核心两条：
1. 把每页签状态收进对象，`ClearRuntime()` 变一句 `PageStates.Clear()`，杜绝漏清导致跨页签泄漏：
   ```csharp
   internal sealed class MclslCodexPageState
   {
       internal int Scroll; internal int SelectedIndex;
       internal readonly List<Row> Rows = new();
       internal void Clear() { Scroll = 0; SelectedIndex = -1; Rows.Clear(); }
   }
   private static readonly Dictionary<MclslCodexTab, MclslCodexPageState> PageStates = new();
   ```
2. `MclslCodexSnapshot.BuildCodexPageCaches`（320 行）按页签拆 12 个 `BuildXxxPageCache()`；`MclslWorldRunRepository` 把"事件归档"拆 `MclslEventArchive.cs`、`NormalizeCore` 拆 `MclslRunNormalizer.cs`。

**前置门槛**：必须在有 Unity 实机回归条件后启动；否则只做第 1 条低风险的"状态对象化"。

**风险**：高。必须完整实机回归；分批、可回滚。

---

### 阶段 M5-K：工具脚本卫生 / Conditional 澄清（P3）

- 13/36 个 Python 缺 `if __name__ == "__main__":` 保护（`package-player.py`、`validate-beast-overhaul.py` 等），被 import 会执行模块级逻辑 → 逐个加保护。
- 根目录一次性脚本（`_deadcode_scan.ps1` 第 2 行硬编码 `cd "D:\模拟长胜路\code"`）、`清理报告-20261031.*` → 移入 `tools/` 或 `.archive/`，路径参数化。
- 在文档标注 `MCLSL_CULTIVATION_DIAGNOSTICS` 需诊断构建定义（已决定不改这 50 处调用，仅文档说明）。

**风险**：低。

---

## 3. 推荐里程碑与顺序

```
M0-B 仓库整理 + 编译基线记录
  └─ M1-A 诊断开关（低风险，先见效）
       └─ M2-C 反射缓存 + M2-F 日志统一 + M2-G 初始化去重（需编译）
            └─ M3-D 空catch规范化 + M3-H 指标/文档 + M3-I CI/分析器 + M3-J 测试收敛
                 └─ M4-E 上帝类拆分（长期，需实机）
                      └─ M5-K 工具卫生
```

每个里程碑：**改前备份 → 改 → 编译(error=0, warning≤基线) → 离线回归 → 单 commit → 记录指标**。

---

## 4. 全局验收门禁（每个阶段都跑）

| 门禁 | 命令 | 通过标准 |
|---|---|---|
| 编译 | `pwsh -NoProfile -File tools/compile-check.ps1 -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394'` | error=0，warning ≤ 基线 |
| 离线回归 | `pwsh -NoProfile -File .tests/run.ps1 -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394'` | 全部 PASS |
| 全量 | `pwsh -NoProfile -File tools/verify-all.ps1 -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394'` | 不跳过资源/来源门禁 |
| 实机 | Unity 运行 + 放置单位/世界加载截图或日志 | 才算"实机通过"（离线/桩/估算不计） |

---

## 5. 风险登记

| 风险 | 触发阶段 | 影响 | 缓解 |
|---|---|---|---|
| 分仓重建 .git 会丢弃旧历史 | M0-B | 高 | 先 `bundle`+`zip` 备份 `.git`；工作区文件不移动 |
| 两仓共享工作区时互相显示未跟踪文件 | M0-B | 中 | 源码仓 `.gitignore` 忽略资产三目录；资源仓 `info/exclude` 忽略其余 |
| 误把"真错误"降级为静默 | M1-A/M3-D | 中 | 人工分类 + 抽检 + 保留 Fatal 清单 |
| 反射缓存与无视热重载冲突 | M2-C | 低 | mod 不热重载；缓存只增不清 |
| 初始化去重改变 WriteLocked 语义 | M2-G | 中 | 严格对齐原分支；实机验证三条路径 |
| 上帝类拆分引入回归 | M4-E | 高 | 长期、分批、需实机回归 |
| CI 仍不可用 | M3-I | 低 | 技能包迁受跟踪路径 + 新建 validation/ 脚本 |

---

## 6. 已确认决策（2026-11-02）

| # | 决策项 | 结论 | 对方案的影响 |
|---|---|---|---|
| 1 | 仓库体积 | **分仓**（源码仓 + 美术资源仓，共享工作区） | M0-B 改为分仓重建设计（见上）；不走单仓/LFS |
| 2 | 诊断开关 | **是**：新增"诊断日志"开关，默认关闭并在 Mod 设置面板可见 | 按 M1-A 的 A1 执行（配置项 + 回调） |
| 3 | 修炼链日志 | **不用** | 取消 A4（不定义 `MCLSL_CULTIVATION_DIAGNOSTICS`）；50 处 `Cultivation` 调用保留为"编译期剥离"现状，不再追加工期 |
| 4 | CI 去留 | **迁移技能包到受跟踪路径 + 补 validation/** | M3-I 改为：`deliverables/token-efficient-workflow` → 受跟踪目录（如 `skills/token-efficient-workflow/`），并新建 `validation/` 脚本；`deliverables/` 仍忽略 |
| 5 | 实机条件 | **具备** | M2-G、M4-E 可按计划推进实机回归；仍需留存 Unity 运行证据 |

> 注：第 4 项需注意当前 `validation/` 目录**全仓不存在**，`skill_package_audit.py` / `test_skill_package.py` 需新建或从既有来源补齐，属实际工作量。

---

## 7. 执行记录（2026-11-02）

### M0-B 仓库分仓：已完成

- 旧 .git（1.2 GB，历史 2 个旧 import 提交）已完整移入 .archive/git-legacy-20261102（HEAD e8082fb，4111 文件）；另有 build/_git-backup-20261102/repo-backup-20261102.bundle（76.9 MB，git bundle verify 通过）。
- 源码仓 .git：重建为全新历史，跟踪 1,893 个文件，.git 体积 7.7 MB；不含三个美术目录、无 dll/pdb/exe。首个提交信息：chore: import source/tools/docs/resources under version control (0.8.4; assets in separate repo)。
- 资源仓 .git-assets：独立 git 目录、共享工作区，跟踪 2,232 个文件（GameResources 833 / NativeActorResources 1,375 / InterfaceResources 23 / .gitattributes 1），.git 体积 71.8 MB。
- 忽略分工：共享 .gitignore 只放构建产物/临时物；源码仓 .git/info/exclude 排除三个美术目录与 .git-assets/；资源仓 .git-assets/info/exclude 反向只保留三个美术目录。
- 操作封装：tools/git-assets.ps1；分仓说明：docs/REPO-LAYOUT.md。
- 两仓 git status 均干净；git log 各 1 个初始提交。
- 待办（可选）：确认 bundle 可用后，删除 .archive/git-legacy-20261102 回收约 1.2 GB。

> 备注：重建过程中发现旧 .git 带有 Windows 拒绝权限项，直接删除/改名被拒；最终通过把内容移入 .archive 得到完整备份，并在空 .git 上 git init 完成分仓（详见 docs/REPO-LAYOUT.md）。
### M1-A 诊断开关：已完成

- 变更：
  - MclslDiagnostics.cs：Error 改为受 DiagnosticsEnabled 控制；新增 Fatal（始终记录，用于存档/外部状态等关键错误）与 Probe（一次性探针，诊断关闭即静默）；节流改用 Environment.TickCount 毫秒，修正主/工作线程单位不一致。
  - MclslConfig.cs：新增 DiagnosticsCallBack。
  - default_config.json：新增 MCLSL_config_enable_diagnostics（SWITCH，默认 false，设置面板可见）。
- 决策3：不定义 MCLSL_CULTIVATION_DIAGNOSTICS，50 处 Cultivation 调用保持编译期剥离。
- 顺带修复构建链路：tools/compile-check.ps1 的源码发现排除 .archive/.tests/.dev-tools/.codegraph。原因：.archive 内有一份旧源码副本，被一起编译导致 sources=619 与大量 CS0111 重复类型；修复后 sources=290。
- 验证：compile error=0、warning=16（等于基线，全部 CS0649 DTO）；8 个离线测试项目全部 exit=0（EconomyPlanScope/LineageCleanup/SharedHelper/RuntimeQuality/NativeFeedback/HeavenGrant/BeastPopulation/EventArchive）。
- 已知问题（转 M3-J 修）：.tests/run.ps1 因 BaseIntermediateOutputPath/APPDATA 重定向导致 dotnet run 失败；本次改用逐项目 dotnet run 取得回归结果。
- 下一步：M2-C 反射缓存。

### M2-C 反射缓存：已完成

- 变更：MclslReflectionNumbers.cs 增加按 (Type, 成员名) 的 ConcurrentDictionary 缓存与 Resolve()；TrySetNumber 改用缓存访问器；新增 FactoryCalls/CacheCount/ResetCacheCounters 供观测。
- 测试：SharedHelperTests 增加缓存命中断言（同 type+name 只解析一次；missing 成员也缓存），检查数 38→41。
- 验证：compile error=0、warning=16（基线）；8 个离线测试项目全部 exit=0。
- 说明：全项目 AccessTools 87 / GetField 35 / GetProperty 24 的其余反射点分批收敛留待后续；本次先改唯一共享实现，收益最集中。
- 下一步：M2-F 日志统一。
