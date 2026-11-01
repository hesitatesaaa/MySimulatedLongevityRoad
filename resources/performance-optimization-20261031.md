# 0.8.1 性能优化（2026-10-31）— 分阶段诊断与热点重复工作削减

本文件记录本轮针对《我的模拟长生路》0.8.1 的源码级优化：诊断设施、实际实现、行为变化、验证、限制、改动文件与风险。

> **证据声明（必读）**
> - 本轮**没有在实际 Unity 中运行任何采样**（未启用 Unity Profiler / 未放置 100/500/1000 单位做实测）。
> - 历史采样 `MCLS_perf_20261030_131254_899.txt` **仅用于确定观测重点**（年度角色合计 74.7%、数据变化分发 17.2%、修行推进 19.1% 等），**不能当作本版本实测**，也不覆盖本轮新增诊断。
> - 因此本文件**不声称**任何 FPS / 帧时 / GC / 内存提升；所有结论均为“源码事实 + 静态不变量 + 可编译性”。
> - `tools/runtime-benchmark.ps1` 只测启动/加载，**不能**作为玩法 CPU 基准；未使用它来宣称玩法性能。

---

## 1. 本轮实施方式

采用“共享诊断设施（主 agent 统一持有）+ 6 个并行分支严格不重叠文件”的方式实现：

| 分支 | 范围 | 独占文件（互不重叠） |
| --- | --- | --- |
| 主 | 共享诊断设施 + calculateMainSprite 集成 | `Core/MclslPerformanceProbe.cs`、`Patches/MclslPatches.cs` |
| 1 | 年度队列 / 需求清单 / 数据变更分发 | `Core/MclslRuntimeChanges.cs`、`Core/MclslRuntimeCollections.cs`、`Systems/Crafting/MclslTianxuanMarket.cs`、`Systems/Runtime/MclslCultivatorCandidateIndex.cs` |
| 2 | 战斗命中 / 兽才 / 职业 / 法术 / 目标扫描 | `Patches/MclslCombatPatches.cs`、`Systems/Combat/MclslCareerCombat.cs`、`Systems/Combat/MclslCombatCareerSystem.cs`、`Systems/Beasts/MclslBeastTalentSystem.cs`、`Systems/Cultivation/MclslSpellCasting.cs`、`Systems/Cultivation/MclslSpellSystem.cs` |
| 3 | 渲染 / 灵兽生成·掉落·地表索引 | `Patches/MclslXuanhuangDirectionPatch.cs`、`Systems/Visual/MclslRealmHaloVisualSystem.cs`、`Systems/Visual/MclslVisibleActorRenderLane.cs`、`Systems/Beasts/MclslBeastWorld.cs`、`Systems/Beasts/MclslBeastSpawnSystem.cs`、`Systems/Beasts/MclslBeastSurfaceIndex.cs`、`Systems/Beasts/MclslBeastLoot.cs` |
| 4 | 战斗附属：自动消耗品 / 妖兽物品效果 / 背包单扫 | `Systems/Crafting/MclslItemUseSystem.cs`、`Systems/Crafting/MclslBeastItemEffects.cs`、`Systems/Crafting/MclslConsumableInventoryPolicy.cs` |
| 5 | 年度修行推进 / 修炼成长 诊断 | `Systems/Cultivation/MclslAnnualCultivationExecutor.cs`、`MclslCultivationGrowthSystem.cs`、`MclslSpiritualRootSystem.cs`、`MclslCultivationAgeSanity.cs` |
| 6 | 静态不变量回归脚本 | `build/verification/round2_perf_invariants.py`（仅新增） |

共享诊断设施由主 agent 统一定义与集成，各分支只调用、**不修改** `MclslPerformanceProbe.cs`，避免多 worker 写同一文件。

---

## 2. 共享诊断设施（`Core/MclslPerformanceProbe.cs`）

复用既有 opt-in probe（`Enabled` 关闭时所有打点为一次布尔分支、零分配、不污染正常性能数据；仅当开启“性能监视”时累计）。新增：

- **计时**：复用既有 `Begin()/End(lane)` 逐阶段 CPU 计时；除 `战斗.命中前缀`、`年度角色.修行快照/修行增长/修行写入/灵根入门/年龄修复` 外，本轮复核补齐 `战斗.职业命中前缀/后缀`、`战斗.职业逻辑`、`战斗.职业目标扫描`、`战斗.兽才逻辑`、`战斗.兽才目标扫描`、`战斗.法术目标锁定`、`灵兽.年度生成准备`、`灵兽.兽潮刷新`、`灵兽.地表索引刷新`、`灵兽.掉落交付`。它们与既有 `年度角色.准备/修行推进/天玄镜交易`、`天玄镜.需求清单生成`、`数据变化分发` 并列。
- **调用 / 处理数量**：新增 `RecordCombatHit / RecordCombatShieldConsumed / RecordCombatTargetScan(int) / RecordAnnualDemandBuild(int) / RecordAnnualDemandReused / RecordSpriteResolve(bool) / RecordSpriteMainCall / RecordSpriteMainCustom / RecordBeastSpawnScan(int) / RecordBeastSpawnPrepared(int) / RecordBeastSurfaceClassified(int,bool) / RecordChangeQueueDrained(int) / RecordChangeQueueMerged`。
- **队列积压 / 年龄**：Summary 新增“分阶段诊断”段，输出战斗命中/护盾吸收/目标扫描候选、年度需求清单生成/需求项/复用、玄黄精灵解析、主贴图调用/自定义、灵兽生成候选/已生成、地表索引处理(增量)、变化出队/同键合并，以及**年度等待队列 就绪/等待/最旧年/最新年**、变化队列积压。既有段继续输出 GC0/GC1 增量与 GC Alloc 均值/峰值。
- 复位：`SetEnabled` 时全部清零，避免跨采样污染。

`Patches/MclslPatches.cs` 的 `Actor.calculateMainSprite` 前缀增加 `RecordSpriteMainCall()` / `RecordSpriteMainCustom()`，用于看主贴图调用量与自定义（世界之魄/兽形）命中量。

`Actor.getHit` 的 `战斗.命中前缀` 测量模组 Harmony 前缀耗时，不包含原版方法完整耗时。主贴图回调也可能经过并行渲染预计算；当前 probe 的 lane 累积结构不是并发计时器，因此这里仅记录调用量和自定义命中量。两处完整方法 CPU 时间应使用 Unity Profiler 采集。年度生成/兽潮刷新 lane 会包含其内部的地表索引刷新；读取诊断时不能把嵌套 lane 的耗时相加。

> 说明：诊断只观测，不改变任何玩法；未开启性能监视时计时入口立即返回，不分配采样对象。

---

## 3. 分模块改动（只列“消除可证明的重复工作/新增诊断”，无玩法数值改动）

### 3.1 年度队列 / 需求清单 / 数据变更分发（分支 1）
- `MclslTianxuanMarket`：新增按市场 `_revision` 版本缓存的只读集合 `ListedArtifactIdsSnapshot()`（已在此前加入）、`ListedBeastConsumableIdsSnapshot()`、`ListedSpellScrollCandidates()`，替换 `BuildFinishedGoodsNeeds`/`AddSpellScrollNeed` 中**每个角色年度需求**对 `ByItem.Keys` 与全体法术目录的重复全量扫描，并消除 `Id+"_SCROLL"` 的逐次字符串分配。`ClearRuntime` 重置对应 revision/scratch。缓存失效依据 `_revision`（`EnsureIndex` 重建、`AddListing`、`RemoveListing`、待发库存发布、部分成交等所有改变挂单键集的路径均 `_revision++`，已核验充分）。
- `MclslCultivatorCandidateIndex.CleanupInvalid`：内层清理循环加入 `!MclslAnnualFrameBudget.Expired` 合作式检查（UI 路径在年度预算未激活时等价于帧 deadline，行为不变；未提高预算）。
- `MclslRuntimeChanges` / `MclslRuntimeCollections`：既有“按 actor id 同键合并 flags、单节点去重、Deferred/Pending 交替 + 0.5ms/帧 deadline 防饥饿”保持不变；新增 `RecordChangeQueueDrained/Merged` 观测。

### 3.2 战斗命中 / 兽才 / 职业 / 法术 / 目标扫描（分支 2）
- `Actor.getHit` 前缀：try/finally 包裹 + `RecordCombatHit`；护盾被完全吸收时 `RecordCombatShieldConsumed` 并提前返回（保持原有提前返回语义）。
- `MclslCareerPatches.BeforeHit/AfterHit` 增加分别计时的前缀/后缀 lane；与 `Actor.getHit` 主命中 lane 有调用范围重叠，不能直接将耗时求和。
- 目标扫描 `Nearby`（职业/兽才）：`Where().Distinct().ToArray()`、`GroupBy/OrderBy/ThenBy/Take` 等改为**单趟 id 去重 + 复用缓冲 + 一次就地排序**（保留“距离升序、同距按 Id、上限 16/8”等排序与上限语义）；返回数组为副本，嵌套调用安全。新增 `RecordCombatTargetScan`。
- 职业/兽才目标扫描及法术锁定增加独立计时 lane；候选数量仍由 `RecordCombatTargetScan` 统计。
- 每击/每决策 LINQ 消除：`Any`→手写扫描、`Friends().Where().ToArray()`→内联 `Friendly`、`Select().Distinct().Count()`→位掩码计数、多目标 `Where(Enemy).OrderBy(a==target?0:1).Take(n)`→单趟（主目标优先、其余扫描序、上限）。
- `MclslCombatCareerSystem.Passive(actor,tier)`：一次 `Applicable+Read+Realm`（原经 `Career()+HasPassive()` 为 2 次）；`HasPassive` 语义保留。
- `MclslCareerCombat.Tick` / `MclslBeastTalentSystem.Tick`：加入合作式 `MclslFrameDeadline.Expired` 退出 + 每帧 `RotateLeft` 公平轮转（不提高总帧预算，剩余留待下帧，不丢事件）。
- 两个 `Tick` 整体新增 `战斗.职业逻辑` / `战斗.兽才逻辑` lane，以统计每帧战斗状态推进 CPU 和 GC 分配；与其内部目标扫描 lane 有范围重叠。
- `MclslSpellCasting`：`MclslCombatTargets.Lock` 单趟去重+缓存排序；`CleanseDamageOverTime` 去 `Where().ToArray()`；`Select(Id).ToList()`→`Ids()`；`Tick` 增合作式 deadline（剩余下帧，`SortedSet` 不丢条目）。
- `MclslSpellSystem.TryCastCombat/TryCastSelf`：把 `KnowsAllSpells`、realm、`MaxMana` 提出逐法术/逐优先级循环。

### 3.3 战斗附属：自动消耗品 / 妖兽物品效果 / 背包单扫（分支 4）
- `MclslItemUseSystem`：`TryAutoLifeSave` 由两次 `Count(actor,..)` 改为一次 `Peek` 快照 + `Count(bag,..)`；`TryAutoCombatConsumables` 新增单趟 `ScanCombatConsumables` 得到存在性位掩码，避免每击最多 8 次 `TryUseAutomatic`（每次全表扫描）+ 7 次 `Count`；`TryAutoCombatTalisman` 与妖兽 pass 共享一次 `Peek`。触发阈值/顺序/冷却/优先级/break 语义不变。新增 `RecordCombatBagSingleScan()`。
- `MclslBeastItemEffects.TryAuto`：`Where/OrderByDescending/ThenBy/ToArray` 每击分配 → 复用静态缓冲 + 缓存 `Comparison` 就地排序（Grade 降序、同 Grade `ItemId` 序数升序），带重入保护回退；新增 4 参重载共享背包。`DamageFactor` 最多 3 次 `ArtifactBonus`（每次含 Peek+6×取袋）→ 单趟 `GuardArtifactSums`，逐 effect 的 0.4 上限与浮点求和顺序保持一致（结果不变）。
- `MclslConsumableInventoryPolicy.ReserveCount`：`PlanContains` 由最多 2 次降为 1 次。

### 3.4 渲染 / 灵兽生成·掉落·地表索引（分支 3）
- `MclslVisibleActorRenderLane`：当 `MclslRealmHaloVisualSystem.QualificationCount==0` 时跳过逐可见 actor 的光环扫描并省去 render-positions 反射取值；活动项更新/清理与诊断计数照常（零资格=零光环，行为等价）。**未改 LOD/cadence/stride/密度任何参数**。
- `MclslBeastWorld`：`Pool(theme,realm)` 结果按 `(theme,realm)` 缓存（过滤/顺序不变），`FillWaveCandidates` 用复用缓冲替代每次 spawn 的 `Select().Where().ToArray()`；`Clear()` 清理缓存。
- `MclslBeastSpawnSystem.TickAnnual`、`MclslBeastWorld.Tick`、`MclslBeastSurfaceIndex.Refresh` 与 `MclslBeastLoot.Deliver` 分别新增耗时 lane。年度生成准备/兽潮刷新耗时可能包含地表索引刷新。
- `MclslBeastSpawnSystem`（主 agent）：自然生成 prepare 循环用复用 `PoolScratch` 替代每 region 的 `ToArray`；`RecordBeastSpawnScan/Prepared`。
- `MclslBeastSurfaceIndex`（主 agent）：`Refresh` 区分初次构建与增量处理并 `RecordBeastSurfaceClassified`；既有“按 dirty 输入增量更新、不弱化正确性”保持不变。
- `MclslXuanhuangDirectionPatch`：既有“预生成资源表/签名复用/打包键”保持；新增 `RecordSpriteResolve(计算/复用)`。
- **掉落回滚备份结构化**（续轮补做）：`MclslBeastLoot.DeliverCore` 的 `JsonConvert.SerializeObject(bag.Items)` 整包备份 + 异常时反序列化还原，改为逐元素克隆 `MclslOwnedItem` 全部 5 个字段的结构化深拷贝（含 null 元素守卫），catch 时整体替换 `bag.Items` 后重抛。`BeginTransaction/EndTransaction` 的事务语义与所有失败路径行为不变（依据见 §4 第 6 条）。

### 3.5 年度修行推进诊断（分支 5）
- `MclslAnnualCultivationExecutor.TryApplyOneAnnualStep`：为兽类/快照/灵根入门/修行增长/修行写入/修行收尾补 `Begin/End` 分阶段计时；移除一处“等价可证”的 `HasCultivationPotential` 守卫（见 §4）。
- `MclslCultivationGrowthSystem.GrantTrueEssence`：缓存写入值，去掉诊断里对 `TrueEssenceRemainder/TrueEssence` 的重复读取。
- `MclslSpiritualRootSystem`：`Split(new[]{...})` 改为复用静态分隔符数组。
- `MclslCultivationAgeSanity.RepairImpossibleYouthCultivation`：加 `年度角色.年龄修复` lane。

---

## 4. 行为变化清单（显式记录）

本轮**未改动**任何修炼增长/突破/伤害/治疗/奖励/掉落概率/交易收益的数值与随机；以下为“行为相邻”改动，均给出等价依据：

1. **移除 `HasCultivationPotential` 守卫（AnnualCultivationExecutor）**：`CheckAnnualStep` 通过 ⟹ `snapshot.Gift!=null`；`Gift` 或来自 `GiftForCultivation`（其非空 ⟹ `GiftFromCurrentTrait!=null` 或 `HasCultivationPotential==true`）或来自 `aptitude>0`（⟹ `HasCultivationRootArchive` 因 `Aptitude>0` 返回 true）。两种情况都使 `HasCultivationPotential(actor)==true`，且两行之间无状态变更，故直接 `Profile(actor)` 与原先“先判后调”结果一致。
2. **战斗/法术 Tick 合作式 deadline**：极端积压时单帧处理量可能低于以往（把工作留到下帧继续，不丢弃条目/事件）；这是“尊重 3ms 合作式预算、不盲目提高总帧预算”的预期结果，不是丢事件。
3. **渲染零资格跳过逐 actor 扫描**：`QualificationCount==0` 时不可能产生/刷新光环，跳过与执行结果相同；未改任何视觉参数。
4. **需求清单缓存**：按 `_revision` 缓存，重建只在市场版本变化时发生；返回集合内容/顺序与原先按 `ByItem.Keys`/`All` 过滤一致（`SelectMarketNeeds`/卷轴两趟取最低价语义不变）。
5. **灵兽生成 `Pool` 缓存**：过滤谓词与顺序不变，仅首次计算；`Clear()` 清理。
6. **掉落回滚备份结构化（MclslBeastLoot.DeliverCore，续轮）**：`MclslOwnedItem`（Data/MclslItemState.cs）恰有 5 个 public get/set 属性、无 JSON 钩子，字段级克隆 ≡ JSON 往返；`MclslBagSystem.Add` 会原地累加既有条目（`stack.Count +=`），故必须深拷元素而非仅浅拷列表；`Write` 的抛出点均不涉及 `bag.Items` 内容，`MutationVersion` 不回滚的行为新旧一致。失败路径逐条核对：收件人失效/`!Supports`/`IsLocked`/非法奖励校验均发生在备份与 `Add` 之前（包状态不动），唯一需回滚的 `Add`/`Write` 抛异常路径由结构化备份逐字段等值还原。掉落内容、存档 JSON、异常外抛语义、`Deliver/Tick/Commit` 对外行为均不变。已知耦合：未来给 `MclslOwnedItem` 增删可序列化属性时需同步备份字段（已在代码注释标明）。

---

## 5. 验证（命令与结果）

编译/构建以仓库现成工具为准。本机 `E:` 盘无游戏，改用同仓校验用游戏副本 `D:\模拟长胜路\validation\runtime-audit-070\game`（Managed 依赖齐全）。

| 验证 | 命令 | 结果 |
| --- | --- | --- |
| 全量编译（仓库工具，补齐计时 lane 后复跑） | `powershell -NoProfile -ExecutionPolicy Bypass -File MySimulatedLongevityRoad\tools\compile-check.ps1 -GameRoot D:\模拟长胜路\validation\runtime-audit-070\game -OutputDirectory MySimulatedLongevityRoad\_compile-check-round2-review` | `sources=275 exit=0`，0 error，18 warning（与基线编译警告数一致） |
| 并发快照编译（整合前） | 同上（多 worker 运行中） | exit=0，0 error，18 warning |
| NML 源码选择 | `python tools/check-nml-sources.py --repository` | `{"sourceCount": 275, "developerToolsExcluded": true}` |
| 本轮静态不变量回归 | `python build/verification/round2_perf_invariants.py` | `RESULT: PASS (46/46 checks passed)`，exit 0；包括新增的十一处计时 lane 检查 |
| 地表索引隔离回归 | 用 Roslyn 编译 `MclslBeastSurfaceIndex.cs` + 现有内联 stub 后运行 | `PASS: transitions, duplicate events, resize, stale tiles, clear, incomplete tiles, deferred events`，exit 0 |
| 续轮复跑：NML 源码选择 | `python tools/check-nml-sources.py --repository` | sourceCount=275，通过 |
| 续轮复跑：静态不变量 | `python build/verification/round2_perf_invariants.py` | `RESULT: PASS (46/46)`，exit 0 |
| 续轮复跑：全量编译（掉落备份结构化 + 守卫移除复核后） | 同 compile-check 命令（子代理会话与本会话各跑一次） | 两次均 `sources=275 exit=0`，0 error，18 warning（既有基线） |

- 静态不变量覆盖：13 个新诊断方法与 Summary 段存在；11 处计数调用点与十一处耗时 lane 存在；`CareerCombat.TickCore`/`BeastTalentSystem.TickCore` 含 `MclslFrameDeadline.Expired`、`CultivatorCandidateIndex.CleanupInvalid` 含 `MclslAnnualFrameBudget.Expired`；`ListedArtifactIdsSnapshot` 与 `ClearRuntime` 重置存在；预算常量未抬高（`OperationLimit = 4096`、`ForLag` 仍 `2.5d : 1.5d`）；`code/` 外除根 `InterestingTrait.cs` 无游离 `.cs`。
- 说明：`tools/test-beast-surface-index.ps1` 在本机 PowerShell 5.1 下其 `Add-Type` 旧 C# 编译器无法编译含 `new()`/值元组的源码（既有环境限制，非本轮引入），故用 Roslyn 等价编译运行验证；不修改该脚本以外内容。

---

## 6. 未完成的实测项与限制

1. **无 Unity 实机采样**：未能获得帧时 P95/P99、模组主循环均 ms、GC 分配、各 lane 真实占比；上面所有数字均为“编译/静态校验”，不是运行性能。
2. **无原版对照**：本报告不构成“原版 vs 本模组”性能对照。
3. **诊断需实机开启**：新 lane/计数只在“性能监视”开启时可见；建议用同一存档对比 `年度角色.*` 各阶段、`天玄镜.需求清单生成` 次数、`战斗.命中前缀` 均 ms、`数据变化分发` 出队/合并与积压、年度等待队列年龄，并配合 Unity Profiler 看 GC。
4. **`runtime-benchmark.ps1` 仅启动/加载**，不能代表玩法 CPU。
5. **delivery 副本**：`delivery/verify/.../tools/test-beast-surface-index.ps1` 属交付副本，本轮未同步其 stub（按约定不改 delivery）；如需在交付副本跑该 harness 需另行同步。
6. **本会话沙箱限制**：续轮验证时主会话子进程被拒绝在 workspace 内创建目录/写文件（DSH 文件工具不受影响），编译输出改用 `%TEMP%`（已清理）；`MySimulatedLongevityRoad\_compile-check-loot-redo\.placeholder` 为子代理预建的空占位残留（0 字节、无影响，可手动删除）。
6. **未打包/未提交/未推送**；未改 PNG/素材。

---

## 7. 残留风险与建议实机回归关注点

1. **合作式 deadline 让出**：年度积压很高时战斗/法术单帧处理量会降低；需实机确认战斗表现与目标切换无异常。
2. **静态复用缓冲**（`Nearby`/`TryAuto`/`BeforeDamageScratch` 等）依赖“主线程单执行 + 返回副本 + 重入回退”；若未来改背包缓存为读时复制、或把 `Nearby` 改为返回共享引用，需重新评估。
3. **需求清单缓存**依赖 `_revision` 为挂单键集的可靠版本、`MclslSpellSystem.All`/灵兽目录为静态；若将来运行期增删目录需追加目录版本失效键。
4. **年度剩余热点**：`HasCultivationPotential` 冗余守卫已移除（每年度步省一次 `GiftFromCurrentTrait` 全目录扫描，见 §4 第 1 条）；单次年度步内 `GiftFromCurrentTrait` 仍被 `GiftForCultivation` 链（快照构建 2 次）与 `Profile` 各扫一次（共约 3 次）——彻底消除需跨方法传 gift/count 或加 trait-gift 缓存（依赖 `Forget` 失效完整性），按“缺乏可靠失效依据”未做。
5. **验证回归关注**：读档一次、世界切换一次、跨越境界突破、天玄镜满仓+未完成采购批次保存续接、制造成功/失败回滚、排行榜文本、灵兽兽潮生成与掉落结算、清图/新建不同尺寸地图的地表索引。

---

## 8. 改动文件清单（27 个 .cs + 1 个验证脚本）

- 诊断设施/集成：`Core/MclslPerformanceProbe.cs`、`Patches/MclslPatches.cs`
- 年度/队列/需求/变化分发：`Core/MclslRuntimeChanges.cs`、`Core/MclslRuntimeCollections.cs`、`Systems/Crafting/MclslTianxuanMarket.cs`、`Systems/Runtime/MclslCultivatorCandidateIndex.cs`
- 战斗命中/兽才/职业/法术/目标扫描：`Patches/MclslCombatPatches.cs`、`Patches/MclslCareerPatches.cs`、`Systems/Combat/MclslCareerCombat.cs`、`Systems/Combat/MclslCombatCareerSystem.cs`、`Systems/Beasts/MclslBeastTalentSystem.cs`、`Systems/Cultivation/MclslSpellCasting.cs`、`Systems/Cultivation/MclslSpellSystem.cs`
- 战斗附属消耗品/物品效果：`Systems/Crafting/MclslItemUseSystem.cs`、`Systems/Crafting/MclslBeastItemEffects.cs`、`Systems/Crafting/MclslConsumableInventoryPolicy.cs`
- 渲染/灵兽生成·掉落·地表索引：`Patches/MclslXuanhuangDirectionPatch.cs`、`Systems/Visual/MclslRealmHaloVisualSystem.cs`、`Systems/Visual/MclslVisibleActorRenderLane.cs`、`Systems/Beasts/MclslBeastWorld.cs`、`Systems/Beasts/MclslBeastSpawnSystem.cs`、`Systems/Beasts/MclslBeastSurfaceIndex.cs`
- 年度修行推进：`Systems/Cultivation/MclslAnnualCultivationExecutor.cs`、`Systems/Cultivation/MclslCultivationGrowthSystem.cs`、`Systems/Cultivation/MclslSpiritualRootSystem.cs`、`Systems/Cultivation/MclslCultivationAgeSanity.cs`
- 验证脚本（新增）：`build/verification/round2_perf_invariants.py`

> `MclslBeastLoot.Deliver` 只增加计时 wrapper，交易与回滚路径保持原样。先前已优化项（储备计划去重、市场数量快照、配方单次解析/回滚、档案索引、精灵/阴影缓存）按“去重”要求未重复实现；素材/PNG/delivery 未改；未提交/推送。
