# 代码与性能优化施工方案（2026-11-01）

本文基于对 `code/` 下 288 个 C# 文件（约 6.5 万行）的静态审查，沿每帧 `MclslRuntime.Tick` 调用链、
渲染补丁、事件/存档入口逐条核对。**所有条目都标注可核对的文件与行号，并给出回滚面。**
本方案为“施工设计”，**不包含任何代码改动**；按阶段逐条实施、逐阶段独立编译与验收。

---

## 0. 总体结论与改动范围

- **不存在“每帧无界扫全人口/全事件”的严重缺陷**。热路径已经索引化、预算化、分帧化（见评估报告）。
- 现存开销集中在三类：**低频事件追加时的 O(N) 重扫**、**存档边界的多次整包 JSON 序列化**、
  **一次性/低频的整图重算与全人口扫描**。都属于“有上界、非持续拖帧”类型。
- 目标不是压低帧数峰值，而是**减少确定出现的 GC 分配与同步卡顿**、降低读档/存档峰值延迟。

**改动边界**：
- 只动本次列出的文件与 API；`MclslWorldRunRepository` 的公共调用面（117 处 `AddEvent` 相关调用）
  **保持不变**，只改其内部实现。
- 不引入新的后台线程、不加锁、不增加每帧更新频率、不做全世界扫描。
- 遵守 `AGENTS.md`：**离线编译与单测通过 ≠ Unity 内尺寸/CPU/GPU 实测**，性能收益须用本方案附录
  的 `runtime-benchmark.ps1` + `MclslPerformanceProbe` 在真实 Unity 记录。

---

## 1. 环境与验证门户（先决条件，所有阶段共用）

| 工具 | 路径 | 用途 |
|---|---|---|
| 编译门禁 | `tools/compile-check.ps1 -GameRoot <游戏根>` | 全量编译，要求 `exit=0`，warning 不增（基线 16 条 CS0649 DTO） |
| 正式离线回归 | `pwsh .tests/run.ps1 -GameRoot <游戏根>` | 跑 `projects.json` 列出的全部正式项目 |
| 全量验证 | `tools/verify-all.ps1 -GameRoot <游戏根>` | 编译 + 单测 + 本地化 + 资源/来源门禁 |
| Unity 性能对照 | `tools/runtime-benchmark.ps1 -RuntimeRoot <隔离运行期>` | vanilla/nml/baseline/optimized 四档对照 |
| 帧内分道采样 | `MclslPerformanceProbe`（探针名已存在） | 各阶段 lane 的耗时/GC 分配 |

> 当前机器默认游戏目录基线为 `D:\owl\gameversion\worldbox-ab712b72f394`（`.tests/run.ps1` 默认值），
> 与 `docs/CODE-QUALITY-PLAN-20261031.md` 记录的旧路径不同，实际以 `compile-check.ps1 -GameRoot` 显式
> 传入为准。**每个阶段完成后重跑 `compile-check.ps1` 与 `.tests/run.ps1`，warning 数不得增加。**

**上线原则**（沿用该仓库历史约定 `docs/CODE-QUALITY-PLAN-20261031.md:937-940`）：
一次一个阶段、每阶段单独提交、单独编译验收。阶段 P1 与其他阶段不混入同一提交。

---

## 2. 阶段 P1：事件裁剪改为均摊 O(1)（收益最确定，先做）

### 2.1 问题与源码依据

`AppendEvent` 每追加一条事件就调用 `TrimEventsPreservingMilestones`：

- `Systems/WorldResources/MclslWorldRunRepository.cs:412-419`（`AppendEvent` → `TrimEventsPreservingMilestones`）
- `:346-367`：该方法默认对整张事件表**遍历两次**：
  - 第一遍 `for (int i=0;i<list.Count;i++)` 统计普通/里程碑数（:350-351）
  - 第二遍从表头 `RemoveAt` 逐条删除（:355-365）
- `MaxEvents=400`、`MaxMilestones=128` 分别在 `:13-14`

只要事件数进入常态（游戏中很快超过 128），**每条 `AddEvent` 都摊上约 N=400 级别的两次迭代**。
`AppendEvent` 的调用路径来自 `AddEventCore`（:559/:604/:643）、`AddMaterialDiscoveryEvent`（:604）、
`AddItemAcquisitionEvent`（:643）、死亡记录（:951）——其中 `AddEventCore` 由 117 处调用点进入（阶段 6.1 记录）。
单条开销不大，但它是**全年每帧都在发生的高频、确定性 GC 无关但 CPU 累积**的点。

### 2.2 设计（增量化，不改公共 API）

**（a）新增两个运行时计数器字段**（`MclslWorldRunRepository` 内）：

```csharp
private static int _eventOrdinaryCount;
private static int _eventMilestoneCount;
```

这两个计数针对 `_current.Events` 的唯一意义是“是否超额、各可裁多少”。它们随事件索引
一起维护（见下），并在 `EnsureEventIndexes()`/`InvalidateEventIndexes()` 时重建。

**（b）维护点**：
- `IndexEvent(record)`（:437-449）按 `IsMilestoneEvent(record.EventType)` 递增对应计数。
- `UnindexEvent(record)`（:382-394）递减对应计数（该处已逐个 Decrement 各索引，顺带递减）。
- `EnsureEventIndexes()`（:421-435）重建时清零并累加；`InvalidateEventIndexes()`（:465-474）清零。

**（c）改写 `TrimEventsPreservingMilestones`**，让它只在“分摊阈值”之上被真正调用：

```csharp
private const float TrimThresholdMultiplier = 1.25f;   // 裁剪滞后量，避免每次追加都扫
private static bool TrimEventsPreservingMilestones(List<MclslRunEventRecord> list, int max)
{
    if (list == null) return false;
    // 均摊：普通事件未超 max*1.25 之前不扫表；里程碑同理由其计数门控。
    if (!int.TryParse("0", out _)) return false; // placeholder, remove
    if (_eventOrdinaryCount + _eventMilestoneCount <= max + Math.Max(MaxMilestones, max / 4)) return false;
    // 到达阈值：一次把普通事件批量裁到 ≤ max（保留尾部），里程碑批次裁到 ≤ MaxMilestones，
    // 同时用先统计的 removeOrdinary/removeMilestones 一次算好，删除时不再两遍扫描。
    ...
}
```

具体删除仍保持“从头向尾、保留尾部”的既有语义（`:353-365`），但：
1. 普通/里程碑计数改为**直接读两个字段**，不再第一遍遍历。
2. 增加“滞后裁剪”，即事件数涨到 `1.25×` 上限才真正触发一次裁剪到上限，让追加路径在大部分时间
   **完全不进入裁剪循环**（都摊后每条 `AddEvent` 约为 O(1)）。

**（d）正确性等价**：滞后只会让事件表在**短的过渡期内**略微超过 `MaxEvents`（最多到 `1.25×400=500`），
裁剪后回落到 `400`。这不会破坏历史语义（原来“恰在 400 就裁”没有外部依赖），但保存/展示前最好
仍把事件 **Normalize 时裁到硬上限**（见 (e)），保证存档内永不超。

**（e）硬上限保底**：`NormalizeCore()` 的 `:1344 TrimEventsPreservingMilestones(_current.Events, MaxEvents)`
保留不变作为“读档/保存前的硬裁剪”，并同样走增量计数（此时字段一定已同步）。这样即使滞后暂时超限，
存档边界也会回到硬上限，行为边界清晰。

### 2.3 不改
- 不新增 `List`、不改 `_current.Events` 的类型，不改变 `MclslRunEventRecord` 结构。
- `MaxEvents`/`MaxMilestones`/`MaxEventsPerYear`（:13/:14/:29）与 `EventTypeYearLimit`（:799-809）不变。

### 2.4 新增/复用测试
新增 `build`（或 `.tests`）下 `EventArchiveTests` 项目，链接 `MclslWorldRunRepository.cs` + 事件 DTO + Stubs，
断言（参考现有一致风格的检查条目）：
1. 追加到 `MaxEvents` 前，普通事件计数 == 表内普通数，里程碑计数 == 表内里程碑数。
2. 超过 `max*1.25` 后追加一次，普通事件回落到 ≤ `MaxEvents`，里程碑 ≤ `MaxMilestones`。
3. 保底：`AlwaysKeepEvent`/`IsMilestoneEvent` 类型在裁剪后不被误删（尾部保留语义）。
4. `UnindexEvent` 后计数与表内一致（不产生负数计数）。
5. 重入：`AddEvent` 在裁剪期间再次入队不破坏索引一致性（复用 `RuntimeQualityTests` 的队列入队测法）。

### 2.5 验收
- `compile-check.ps1`：确认新增调用面编译通过、退出 0、warning 不增。
- 新增 `EventArchiveTests` 通过；既有全部正式项目通过。
- 用 `runtime-benchmark.ps1` 的 baseline 与 optimized 对照，事件归档 lane（探针名 `事件记录与去重`）
  的**分配字节与均值耗时不增，且随事件数量增长不再线性放大**。

### 2.6 风险与回滚
- **中**：改动触碰 `Current.Events` 的核心裁剪逻辑。必须保留滞后语义 + 存档硬裁剪双保险。
- 若滞后引入“事件表在过渡期超 400”的展示异常，可回退 `2.1` 的滞后并发比到 `1.0`（退化为“每次追加即查”），
  仍保留计数增量化的收益；这是单点回退，不牵连其它阶段。
- 绝不能在未编译的环境改 P1（涉及事件索引一致性，`docs/CODE-QUALITY-PLAN-20261031.md:0.3` 禁止）。

---

## 3. 阶段 P2-A：存档序列化合并与脏标记（降低保存/读档峰值卡顿）

### 3.1 问题与源码依据

- `Systems/WorldResources/MclslWorldArchiveStore.cs:72-101`：`SaveNow` 在原生存档边界同步执行
  `ExportArchive()`（:84 + `MclslWorldRunRepository.cs:151`）+ `JsonConvert.SerializeObject(archive)`（:88）。
- `Core/MclslRuntime.cs:195-205`（`PrepareForSave`）在同一时点**又串行**调用
  `MclslCareerCombat.Save()`、`MclslBeastWorld.Save()`、`MclslBeastLoot.Save()`、
  `MclslBeastTalentSystem.Save()`、`MclslSpellCasting.Save()`，其中每个都各自
  `JsonConvert.SerializeObject` 写进 `map_stats.custom_data`（例：`Systems/Combat/MclslCareerCombat.cs:83-94` :91）。
- 结果：一次存档 = 1 份整包 JSON + ≥5 份模块 JSON，全部同步集中在一个调用点，自动存档频繁时会形成
  明显峰值卡顿与一轮 GC。

### 3.2 设计（分期，最低风险优先）

**Step A1（先做，纯减负）**：给每个独立持久化模块加“上次序列化哈希/版本”缓存，仅当 `MarkDirty()`
被调用过才重新 `SerializeObject`：
- `MclslCareerCombat`、`MclslBeastWorld`、`MclslBeastLoot`、`MclslBeastTalentSystem`、`MclslSpellCasting`
  各自已有 `_loadedRaw` 式缓存（`MclslCareerCombat.cs:93` 存 `_loadedRaw=raw`），可扩展为“本次世界代次内，
  若序列化串未变则跳过写 custom_data”。注意这些是**内存写**，SaveCustomData 的写入仍只发生在 `PrepareForSave`。
- 验收：存档 lane（探针 `存档.JSON序列化`、`存档.导出档案`）的总耗时与分配下降；功能不变。

**Step A2（再做，收敛边界）**：把“整包序列化”与“模块序列化”合并到**同一个** `PrepareForSave`
调用面，避免跨类重复 `EnsureData(true)` 与重复的 `custom_data` 写入锁/装箱。
- 可把模块序列化结果作为 `MclslWorldArchiveBundle` 的一个子字段一并打包，或保持各自键但统一在
  `PrepareForSave` 入口注入一次 `EnsureData`。
- **不在本轮引入异步写档**（自动存档线程模型与原生 `SaveCustomData` 绑定，改动大、风险高，列为“不做”）。

### 3.3 不改
- 不改存档文件格式与键名（`Key`/`BackupKey`，`MclslWorldArchiveStore.cs:12-13`），不迁移旧档。
- 不动 `runtime-benchmark.ps1` 的存档读写度量协议。

### 3.4 验收
- 单测：给相关模块加接口级“仅当脏才序列化”断言（可放在 `RuntimeQualityTests` 或新增 `SaveCoalesceTests`）。
- Unity：`runtime-benchmark.ps1` 的 “保存/加载 10 次”对照，期望保存总耗时、GC 分配下降。
- 必须满足 `AGENTS.md`：不做“内存/CPU 内存估算代替实测”。

### 3.5 风险
- **低~中**：合并可能改变写入时序。Step A1 先独立做并验收，再把 Step A2 单独提交；任一失败可单独回退。

---

## 4. 阶段 P2-B：天恩特质唯一性改为持有者集合（消除全人口扫描）

### 4.1 问题与源码依据

- `Systems/MclslTraitGrantRouter.cs:97-111`：`EnforceUniqueHeavenFavor` 每次授予都对
  `World.world?.units?.getSimpleList()` **全量遍历** + `hasTrait(HeavenFavorTraitId)`（:101-108）。
- 影响面：授予次数 × 当前人口数（读档后人口可上万），且 `hasTrait(id)` 本身是字符串/内存查找。

### 4.2 设计
- 新增 `private static long _heavenFavorHolderId;`（或集合，视存在多名的情况）。
- 授予时：先 `RemoveTrait` 旧持有者（O(1) 读 ID + 单对象 remove），再设新持有者。
- 在持有者死亡/删除路径（`MclslRuntimeChanges.Remove` / `MclslTraitGrantRouter.HandleRemovedTrait`）
  清理过期 ID，保证一致。
- 若旧逻辑语义允许“多名替身并存”，则用 `Dictionary<long,int>` 引用计数替代单 ID；实现前先核对
  现有调用谁设/谁清，保持去重规则不变。

### 4.3 不改
- 不改天恩特质 ID、授予规则、`removeTrait` 回调链。

### 4.4 验收
- 新增 `HeavenGrantTests` 断言“授予第二人后旧持有者被清除且人口遍历次数为 0（有计数桩）”；
  复用既有 `HeavenGrantTests` 项目（`projects.json` 已含）。
- Unity：授予/清除行为不回归。

### 4.5 风险：低。纯内部状态重构，不触碰公共 API。

---

## 5. 阶段 P2-C：玄黄族换色改调色板（降低一次性整图重算与显存）

### 5.1 问题与源码依据

- `Patches/MclslXuanhuangDirectionPatch.cs:223-276`：
  - `PrepareWorldColors()` 读档后遍历全人口收集国家色（:230-233）。
  - `PrepareColor()` :242 对每个**新颜色**遍历**全部图集**。
  - `PrepareAtlas()` :252-263 对每个 (图集, 颜色) 对 `Pixels.Clone()` + 逐像素逐点重算 + `new Texture2D`
    + `SetPixels32` + 为每个帧 `Sprite.Create`（:264-275）。
- 触发点：读档/新国家色出现。一次性 CPU 与显存/GC 都不小，但颜色集有限，**不是每帧问题**。

### 5.2 设计（优先做最省的）
- **最小改动、低风险**：把“克隆整图 + 逐像素”改为**只对 mask 命中像素**用预计算的 LUT（按 `mask.r/190f`
  的 shade 档位预置小数乘法），避免每像素 `Mathf.Clamp` 与新 `Color32` 装箱。
- **可选（中风险，需美术核对）**：若图集使用的是 uniform 国家色叠加，可改为**单色贴图 + Sprite 的
  `texture` 共享 + 运行时 tint**（`SpriteRenderer.color`），彻底免去每色建图。此方案改变渲染语义
  （不再是烘焙像素），**列为可选项，须 Unity 视觉验收通过才采用**。
- 优先做第一项，第二项另行单独阶段并配视觉对照。

### 5.3 验收
- 视觉：角色国家色仍正确，独特色/渐变不偏差（用 `runtime-living-acceptance.ps1` 现有图像摘要对照）。
- 性能：探针 lane（`主贴图.自定义`、玄黄色准备路径）一次性耗时/显存下降。

### 5.4 风险：LUT 方案低；tint 方案中（改可视化语义），默认只做 LUT。

---

## 6. 阶段 P3：探针上下文字符串索引化（仅诊断期优化）

### 6.1 问题与源码依据
- `Core/MclslPerformanceProbe.cs:440-472`：`SampleFrame` 每帧把 `Era`、`WorldStage`、`TopLane` 等字符串
  写进 `FrameContext`（:463-465），仅服务“最差帧报告”（`AppendWorstFrames` :650-689）。
- 仅当探针开启（`DiagnosticsEnabled`）才累积；默认关闭，所以非持续问题。

### 6.2 设计
- 把这些字符串改为**枚举/整数**存进 `FrameContext`；`AppendWorstFrames` 出报告时再解析为文本。
- 目标：采样期的 GC 分配与字符串构造降到 0。属低优先级、低风险。

### 6.3 验收/风险：低。不改变探针输出文本内容（保持报告可读性）。

---

## 7. 阶段 P3：UI 大列表虚拟化（仅窗口打开时优化）

### 7.1 问题与源码依据
- `UI/MclslCodexWindow.cs:143-166`（`OnGUI` 每次 Layout/Repaint 用 `GUILayout` 重建整棵），
  `Update:113-141` 用 `MclslCodexSnapshot.Advance()` 增量推进；打开时会 `ApplyCodexPause` 暂停模拟。
- 大列表/档位页在窗口打开时若条目多，视觉滚动会对每行做 IMGUI 绘制，造成打开态帧率下降。
- 影响面是“窗口打开时的 UI 帧率”，不影响游戏模拟。

### 7.2 设计
- 对条目最多的页签（如天下纪事/角色排行）实现虚拟滚动：只重建可视区行 + 视口外的占位，按需加载。
- 这是**独立、低风险但改动面较大**的 UI 重构，列入可选项，非本轮必做。

### 7.3 风险：中（UI 布局回归）。须配合截图对比与视觉验收，不允许离线图片审看代替。

---

## 8. 施工顺序与依赖

```
环境（GameRoot 编译门禁）
   ↓ 必须先通
阶段 P1（事件裁剪增量化）：需编译 + EventArchiveTests + Unity 事件回归
   ↓ 独立
阶段 P2-A（存档脏标记）→ 再单独提交 P2-A-2（合并序列化边界）
   ↓ 独立
阶段 P2-B（天恩持有者集合）、P2-C-LUT（玄黄换色 LUT）
   ↓ 独立，全部可并行
阶段 P3-A（探针索引）、P3-B（UI 虚拟化，可选项）
```

**提交纪律**：P1、P2-A、P2-B、P2-C、P3-A、P3-B 各一提交，各自 `compile-check` + `.tests/run` 通过。
**不做清单**：异步后台存档线程、改变存档格式/键名、tint 换色（除非视觉验收通过）、引入新依赖程序集。

---

## 9. 验收指标汇总（区分离线与 Unity）

| 指标 | 离线可判 | 须 Unity 实测（`AGENTS.md` 门禁） |
|---|---|---|
| 编译 exit=0、warning 不增 | ✅ `compile-check.ps1` | — |
| 正式回归全绿 | ✅ `.tests/run.ps1` | — |
| 事件追加不再随 N 线性放大 | ✅ 新 `EventArchiveTests`（构造长表微基准） | ⚠️ 只能证明算法，不证明帧耗时 |
| 存档/读档耗时与 GC 分配下降 | 部分（SaveCoalesceTests 纯逻辑） | ✅ `runtime-benchmark.ps1` 10 次存档/加载 |
| 天恩唯一、玄黄换色正确 | ✅ 逻辑单测 + 图像摘要 | ✅ 实际换色/授予不回归 |
| 100/500/1000 单位及五负载、30s 预热/120s 采样、P95/P99、模组 CPU、GC 次数、内存峰值 | ❌ | ✅ 必须由 `runtime-benchmark.ps1` + 探针记录 |

**凡未在真实 Unity 实际运行并获得记录/放置单位证据的项目，一律不得标记为“Unity 尺寸/CPU/GPU/100-1000 单位实测通过”**
（沿用 `AGENTS.md` 验收记录条款）。