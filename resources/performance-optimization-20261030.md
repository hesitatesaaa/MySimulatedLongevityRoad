# 0.8.1 性能优化（2026-10-30）

本文件记录本轮针对《我的模拟长生路》0.8.1 的源码级优化：报告证据、实际实现、限制、改动文件与验证。
**本轮没有在实际 Unity 中运行采样，也不声称任何 FPS / 帧时 / GC 实测提升。** 下面给出的是“消除可证明的重复工作”的代码事实，以及可在实机回归中验证的预期机制。

---

## 1. 报告证据（`MCLS_perf_20261030_131254_899.txt`）

| 指标 | 数值 |
| --- | --- |
| 采样帧 / 当前 FPS / 人口 / 修士 | 1940 / 21 / 3738 / 924 |
| 年度队列 / 状态 | 923 / 923 |
| 请求年 / 最旧未完成年 | 520 / 295 |
| annualSteps/s · annualYears/s | 1690.8 · 0.000 |
| 变化队列 / 背包待写入 | 3455 / 765 |
| 天玄镜挂单 / 未完成采购批次 | 1000 / 502 |
| 模组主循环 CPU 总 / 均 / 最大 | 6170.07 ms / 3.180 ms / 109.23 ms |
| 帧时 均 / P95 / P99 / 最大 | 56.66 / 85.57 / 132.64 / 3697.69 ms |

热点（调用/总 ms/单次最大 ms）：

| 模块 | 调用 | 总 ms | 占比（/6170） |
| --- | ---: | ---: | ---: |
| 年度角色（合计） | 1858 | 4611.29 | 74.7% |
| ├ 年度角色.准备 | 51895 | 1306.97 | 21.2% |
| │ └ 年度角色.职业判定制作 | 6915 | 728.11 | 11.8% |
| ├ 年度角色.修行推进 | 48343 | 1176.32 | 19.1% |
| ├ 年度角色.天玄镜交易 | 46167 | 1174.57 | 19.0% |
| │ ├ 天玄镜.需求购买 | 39371 | 1093.98 | 17.7% |
| │ │ └ 天玄镜.需求清单生成 | 5342 | 547.07 | 8.9% |
| │ └ 职业.制作提交 / 职业.选方 | 1542 | 278.25 / 233.25 | 4.5% / 3.8% |
| 数据变化分发 | 3880 | 1062.60 | 17.2% |
| 档案.EnsureCurrentRun | 7804 | 210.67 | 3.4% |

最差帧：帧时 3697.69 ms，同一帧记录的模组主循环 CPU 为 3.39 ms。**该帧耗时无法用模组主循环 CPU 解释；本报告不对其归因（原生/GC/加载等均需 Unity 采样才能确认）。**

`annualYears/s=0.000` 是该采样窗口内的统计值，说明窗口内没有观察到世界年完成；**不能据此推断年度世界通道从未完成过年或永远不会完成**。

### 由证据推出的可改点（仅限代码可证明的重复工作）
1. **同一“年度操作”里重复计算计划**：`BuildPurchaseBatch` 先直接 `BuildReservePlan`，又经 `BuildCraftMaterialReserves → PlannedRecipe → RecipeStillNeeded` 再算一次；`TryListSurplus` 同样两次。`PlannedRecipe` 还每次新建市场物品计数字典。
2. **职业选方/制作**：`HasRecipe` 与 `ConsumeRecipe` 对同一配方各 `ResolveRecipe` 一次；`ConsumeRecipe` 每次制作都对整个背包 `JsonConvert.SerializeObject(bag.Items)` 只为失败回滚，成功则丢弃。
3. **档案重扫**：`EnsureCurrentRunCore` 只缓存单一 `_ensuredYear`，主循环在“世界当前年”和“积压旧年”间切换就重跑 `EnsureAnchors` 与 `EnsureAncientTechniqueSeeds`；后者对每个功法定义做 `TechniqueLineages.FirstOrDefault(...)` 线性扫描并分配 LINQ 闭包。
4. 年度积压本身是**吞吐**问题（923 名修士落后约 225 年，每角色年约 22 个流水线步骤），本轮只降低单位步骤成本，不改追赶策略。

---

## 2. 实现（按上述可改点）

### 2.1 新增共享辅助 `MclslAnnualEconomyPlan`（去重重复的计划计算）
`code/.../Systems/Crafting/MclslAnnualEconomyPlan.cs`。只在一个**年度流水线操作**内缓存“消耗品储备计划”，把同一操作内的 2~4 次重复计算收敛为 1 次。
- **作用域硬约束**：只有 `Begin(actorId, year)` 之后才缓存；作用域外调用一律重新计算到独立临时表且**不保留**——外部/UI/战斗调用无法污染缓存。
- **失效**：`MclslAnnualActorPipeline.ProcessStage` 用**外层 `try/finally`** 包住 `Begin` 之后的全部路径（含 `NextPrepareStep`、`PeekExisting`、`BeginTransaction`），`finally` 里**先** `Invalidate()` **再** `EndTransaction()`（背包提交路径不读该计划，无依赖），因此任何提前抛出的异常都不会让作用域或计划残留。
- **事务标记**：`transactionStarted` 只在 `BeginTransaction` 正常返回后置位，避免 `BeginTransaction` 抛异常时 `finally` 里的 `EndTransaction` 误报“Unbalanced inventory transaction”。
- **为什么不跨操作缓存**：计划依赖实时的境界/状态/法力；修行阶段可能突破境界（改变突破丹），跨操作复用会改玩法精度，所以刻意不跨操作。
- **返回列表的生命周期**：memo 只持有一份列表，调用方必须在同一次同步操作内消费完再请求下一份计划；本轮的三个调用点都立即使用（`ReserveCount`/遍历），不会跨请求持有。
- 调用方（`BuildPurchaseBatch`、`TryListSurplus`、`PlannedRecipe`/`RecipeStillNeeded`）改为共享该计划；`ReserveCount` 改为只读 `IReadOnlyList<string>`，语义与 `List.Contains` 全等（序数比较）。

### 2.2 市场物品计数快照按修订号缓存
`ActiveItemCountsSnapshot()` 由“每次调用新建字典并全量遍历挂单”改为按 `_revision` 缓存，并**把返回类型改为 `IReadOnlyDictionary<string,int>`**，从类型上禁止调用方污染共享字典（三个调用点都只用 `GetValueOrDefault`）。`ActiveItemCount(id)` 改为查询该快照。

为保证缓存正确，审计了**全部** `PendingByItem` 写路径并补失效：
- `EnsureIndex` 重建：结尾本来就有 `_revision++`；
- `TryListArtifactSurplus` 的“市场已满 → 挂到待发队列”分支：新增 `_revision++`（这里 `_indexedPendingCount` 保持同步，`EnsureIndex` 不会重建，原来会漏失效）；
- `PublishPendingArtifactListings` 出队/丢弃/卖家暂不可用：`inspected > 0` 时新增 `_revision++`；
- `QueueEstateItems`、`TransferSellerEscrowToFamily`、`ReleaseUnclaimedSellerEscrow`、`ClearRuntime`：原有 `_indexedPendingCount = -1` / `_revision++` 已覆盖。

> 采购批次的 `Year/Cursor/Profession/Grade` 与 `PendingMarketPurchases` 存档语义**未改动**：命中游标继续沿用，仍逐步推进、仍写脏标记，只是构建批次时的重复计划被消除。

### 2.3 职业选方/制作：单次解析 + 结构化回滚
`MclslItemEconomy.cs`：`TryCraft` 现在只 `ResolveRecipe` 一次并直接消费；`ConsumeRecipe` 复用 `ConsumeResolvedRecipe`。回滚从“整包 JSON 序列化/反序列化”改为“对持有的少量物品做字段级克隆，失败时还原列表”，并在还原后 `bag.MutationVersion++`：`MclslBagSystem` 的计数索引与装备/器物缓存都以该版本号为键，不推进就可能读到已扣除的计数（该字段是运行时 `internal` 字段，不进背包 JSON）。异常传播与“扣除失败即抛错”语义不变。

### 2.4 档案：锚点按 run 缓存；功法种子改为一次索引（不跳过）
`MclslWorldRunRepository.cs`：
- `EnsureAnchors` 增加按 run 的完成标记（时间线锚点只增不删，逻辑上不可能漏做）。
- `EnsureAncientTechniqueSeeds` **不跳过任何一次调用**（不引入“列表长度相同就跳过”的漏洞，删除+新增抵消时仍会补齐），只把“逐定义 `FirstOrDefault` 线性扫描 + LINQ 闭包”改为复用同一个 `Dictionary<string, record>` 索引，复杂度由 O(定义数 × 传承数) 降到 O(定义数 + 传承数)；保留“首个匹配优先”、新建记录写回索引（同 id 归一化不重复新增）与空字段修补行为。
- `InvalidateCurrentRunCaches` 清空锚点标记与种子索引（读档/换 run 时重新做一遍）。

### 2.5 合并确认相同的实现（用户指出的重复）
- `TrySetNumber`：`MclslCultivationSystem` / `MclslDeathSystem` / `MclslAncientLawSystem` / `MclslWorldSoulSystem` 四份实现逻辑相同 → 统一委托 `Core/MclslReflectionNumbers.TrySetNumber`，各文件保留一行带站点标识的包装。共享 helper 支持可选的 `diagnosticKey` / `messagePrefix`，因此 `MclslWorldSoulSystem` 原有日志 key（`world-soul-set-number-member`）与文案（`设置天地之魄数值成员失败: `）逐字保留，其余三处保留原消息模板。**`MclslWorldSoulActorRegistration.SetNumberIfExists` 形态不同（多字段名、支持 double、静默 catch），未合并。**
- `GetAssetDisplayName` / `GetTraitDisplayName`：`MclslRankSnapshotSource` 与 `MclslRankWindow` 两份逐字相同（含特质名），合并为 `UI/MclslRankTextFormat`，本地化解析顺序（运行时键→本地化名→locale key→兜底“未知生物/未知特质/未知特征”）与吞异常诊断逐字保留，两个原文件改为一行委托。

### 2.6 明确**没有**做的事
- **没有提高任何预算/配额**：`MclslAnnualFrameBudget.OperationLimit=4096`、`ForLag`（2.5/1.5 ms）、`MclslBagSystem.FlushPending(budget = 8)`、`MclslRuntimeChanges.Drain` 的 0.5 ms 上限全部保持原值（有静态校验守护）。
- **没有**跳过年份、降低收益、减少实际交易或降低逻辑精度。
- **数据变化分发**：已核查——`MclslChangeQueue` 已按角色 id 合并（同角色多帧变更只保留一条），`Drain` 每次 0.5 ms 上限；在无法实机细分单次下游刷新成本前**未改动**，也未提高预算。
- 未改 PNG/画质、未改 ACL/环境、未动其他目录、未提交 git。

---

## 3. 改动文件

新增：
- `code/MySimulatedLongevityRoad/Core/MclslReflectionNumbers.cs`
- `code/MySimulatedLongevityRoad/Systems/Crafting/MclslAnnualEconomyPlan.cs`
- `code/MySimulatedLongevityRoad/UI/MclslRankTextFormat.cs`

修改：
- `code/MySimulatedLongevityRoad/Systems/WorldResources/MclslWorldRunRepository.cs`
- `code/MySimulatedLongevityRoad/Systems/Crafting/MclslConsumableInventoryPolicy.cs`
- `code/MySimulatedLongevityRoad/Systems/Crafting/MclslTianxuanMarket.cs`
- `code/MySimulatedLongevityRoad/Systems/Crafting/MclslItemEconomy.cs`
- `code/MySimulatedLongevityRoad/Systems/Runtime/MclslAnnualActorPipeline.cs`
- `code/MySimulatedLongevityRoad/Systems/Cultivation/MclslCultivationSystem.cs`
- `code/MySimulatedLongevityRoad/Systems/Cultivation/MclslAncientLawSystem.cs`
- `code/MySimulatedLongevityRoad/Systems/Death/MclslDeathSystem.cs`
- `code/MySimulatedLongevityRoad/Systems/WorldSoul/MclslWorldSoulSystem.cs`
- `code/MySimulatedLongevityRoad/UI/MclslRankSnapshotSource.cs`
- `code/MySimulatedLongevityRoad/UI/MclslRankWindow.cs`

用户既有成果（边框置顶、召唤兽 2/3、一级帐篷 1.2 倍、远镜头特效平滑缩到 1/3、建筑影子不变）均不在上述文件内，未触碰。

---

## 4. 验证

自写验证放在 `build/` 下。**注意：NML 会递归选择 C# 源文件（见 `tools/check-nml-sources.py`），`build/` 这个目录名本身不会让它忽略测试代码；这里安全的前提是发布打包不会把 `build/` 拷进安装目录。** 仓库模式下 `check-nml-sources.py --repository` 会排除 `build/`，`tools/compile-check.ps1` 也排除 `build/`。

- `build/verification/verify-refactor-invariants.py`：静态不变量——计划缓存有活动作用域且仅作用域内缓存、`Begin` 之后所有路径都在外层 try 内、`finally` 先 `Invalidate` 再 `EndTransaction`、锚点标记按 run 重置、种子补齐没有被长度缓存跳过、四份反射设置器均已委托、两个排行文本已委托且兜底字面量只在共享类、市场计数快照为只读且两条待发库存写路径都失效、制作回滚已推进 `MutationVersion`、无 `ReservePlanScratch` 残留、**预算常量未被提高**、`code/` 与 `build/` 外无杂散 `.cs`。
- `build/verification/EconomyPlanScopeTests/`：**以 `Compile Include` 链接真实 `MclslAnnualEconomyPlan.cs`**（配最小 stub），覆盖：作用域外不复用、作用域内同角色/同年复用同一实例、跨年/跨角色重建（同实例复用，故比较内容与构建次数）、`Invalidate` 后重建、构建抛异常不留半成品且下次重建、`Begin` 开启新操作、跨操作不泄漏到其他角色。
- `build/verification/run.ps1`：一次跑完上面两部分。

### 执行状态

独立复核已完成：278 个源码文件编译通过（18 条既有警告）；37/37 静态不变量检查通过；真实计划缓存源码的 14 项作用域行为检查通过；真实共享反射和名称解析源码的 12 项行为检查通过；NML 源码选择检查通过。
测试使用最小依赖替身，不代表 Unity 实机验收。测试及编译产物位于工作区 build/，不进入安装包。

### 建议的实机回归关注点
读档一次、世界切换一次、跨越一次境界突破、天玄镜挂单满仓与采购批次未完成时保存再读，确认：年度角色仍按年逐个推进（不跳年）、采购批次游标与存档续接、制造成功/失败回滚、排行界面文本与旧版一致。

---

## 5. 残留风险与限制

1. **不解决积压本身**：本轮只降低每个年度操作的单位成本，不改变“按年逐个补齐、不能跳年”的追赶策略；能否追上世界钟需实机采样。
2. **跨操作重复仍在**：`PlannedRecipe` 在准备阶段与市场阶段各算一次（中间可能突破境界），这是刻意的精度取舍。
3. **memo 生命周期**：返回的储备计划列表为共享实例，只能在同一同步操作内使用；当前三个调用点均满足，但新增调用方必须遵守（XML 注释已写明）。
4. **诊断 key 变化**：三处合并后的反射报错 key 由 `empty-catch-code-…` 变为 `reflection-set-number:<站点>`（消息正文模板不变）；`MclslWorldSoulSystem` 保留原 key 与文案。
5. **数据变化分发未优化**：缺少实机细分前不改。
6. **没有实测数据**：以上为代码与静态不变量层面的结论；不声称实际 Unity 性能提升已测。

---

## 6. 后续实机验证

用同一存档对比 annualSteps/s、年度角色各阶段均 ms、需求清单生成次数及 EnsureCurrentRun 均 ms，并检查制造回滚、满仓挂单和采购批次保存续接。本次未进行 Unity 实机性能测量。

---

## 7. 补充（同日第二轮）：玄黄族逐帧渲染热区

本轮经济/年度优化之外，另做了一轮**只涉及缓存/分配**的逐帧渲染优化，详细报告见
[`performance-optimization-20261030-render-xuanhuang.md`](performance-optimization-20261030-render-xuanhuang.md)。

改动文件：
- `code/MySimulatedLongevityRoad/Patches/MclslXuanhuangDirectionPatch.cs`
  - 逐帧绘制路径：按 `(方向3 × 外观14)` 预生成 `Variant[3,14]` 资源表；帧元数据改为数组索引；颜色/帧复合键改为 `int`/`long` 打包键；每 actor 打包 `long Signature`，外观/颜色/方向/动画帧全未变时直接复用上一张 Sprite；`(源图集,颜色)` 着色结果命中即复用；静态 Zombie 资源键在 `Init` 预生成。
- `code/MySimulatedLongevityRoad/Systems/Visual/MclslXuanhuangBuildingShadow.cs`
  - 建筑影子缓存按“建筑对象 + 源精灵 + Rect + PPU + fundament + scale_base”签名复用；仅输入变化才提取像素/重采样/重建，并释放旧 Sprite/Texture；`Sprite.Create` 失败时清理已分配纹理；`clear()` 生命周期统一释放。

验证（源码层，非 Unity 实测）：
- `tools/compile-check.ps1`：279 个源码文件，exit 0，仅 18 条既有警告，无新增警告。
- 静态校验：签名/帧键/图集键无碰撞；`AfterSprite` 热路径无字符串拼接与复合键查表；`Clear`/`Release` 同时销毁并反注册 Sprite、销毁 Texture；14 外观 × 3 方向 = 42 条资源路径全部存在。

**限制**：本轮没有 Unity 实机采样，不声称 FPS/帧时/GC 提升；本报告不能冒充原版与玄黄族的单人对照。后续需按渲染报告 §5–§6 的 Profiler 步骤与数据表模板实机验证。
