# 代码质量与性能优化实施记录 · 2026-11-01

代码实施、生产编译和四个正式回归项目已完成。**既有资源门禁未通过，实际 Unity 功能及完整场景性能验收待完成，因此尚未达到全部交付门槛。**本地压缩玩家包用于后续验收，不能作为已验收发布包。

仅支持当前构建生成的当前格式新存档，不提供旧格式迁移、兼容读取或降级转换；后续版本沿用这一原则。项目目录名保留不变，现有 `mod.json` 的版本为 0.8.3。本轮没有调用 DSH、修改玩家配置、增加共享预算或处理额度。

## 基线与变更边界

实施前当前工作区快照位于 `MySimulatedLongevityRoad/build/quality-20261101/baseline/`：

- `source-snapshot.zip`、330 项输入哈希 `manifest.json`、原有修改 `preexisting.diff`、原始 276 源文件编译结果均已保留。
- ZIP SHA256：`EB53DD9F216E1026082FDC6ABB02EB86E45E8CD12C63C44C17FDE5076F0CE036`。
- 未以目录外的 baseline 项目作对照。原有七个不与本轮修复重叠的修改文件，经与实施前 ZIP 比较，字节完全相同；兽潮文件中的原有修改亦保留。
- 未创建提交或修改 Git 暂存区。实现分为下表所列模块边界，可分别审阅；回退应按实施前快照恢复对应文件，不能对工作区执行整体重置。

## 已实现

| 范围 | 实现及源码 |
| --- | --- |
| 调度 | [MclslOwnerRound.cs](MySimulatedLongevityRoad/code/MySimulatedLongevityRoad/Core/MclslOwnerRound.cs) 复用快照、保存游标；职业战斗与兽族天赋使用同一实现。预算不足前不消费下一角色；移除角色跳过、新增角色下一轮进入；完成轮次后本次调用结束。天赋完成后等待 0.1 秒，未完成轮次下一帧可续跑。 |
| 存档生命周期 | [MclslModuleLoadState.cs](MySimulatedLongevityRoad/code/MySimulatedLongevityRoad/Core/MclslModuleLoadState.cs) 为兽潮、掉落、天赋、法术与职业战斗提供 `Unloaded/Ready/Faulted` 和世界代次绑定；仅 Ready 且代次匹配可保存。[MclslRuntime.cs](MySimulatedLongevityRoad/code/MySimulatedLongevityRoad/Core/MclslRuntime.cs) 在加载边界清除旧模块，并在核心重新开启后只初始化一次。 |
| 职业解析 | [MclslCareerStateValidation.cs](MySimulatedLongevityRoad/code/MySimulatedLongevityRoad/Systems/Combat/MclslCareerStateValidation.cs) 单次解析并校验、构建索引；检查版本、角色重复、职业/等级、五元素数组和有限时间/效果参数。过期有限时间有效；保留 LastAction=0 的空闲语义。[MclslCombatCareerPersistence.cs](MySimulatedLongevityRoad/code/MySimulatedLongevityRoad/Systems/Combat/MclslCombatCareerPersistence.cs) 缓存成功与失败，异常角色拥有独立禁用状态，禁止覆盖原文。战斗异常被模块入口隔离。 |
| 火符 | [MclslSpellCasting.cs](MySimulatedLongevityRoad/code/MySimulatedLongevityRoad/Systems/Cultivation/MclslSpellCasting.cs) 仅使用 `pending.v2`。初次命中直接复用当前记录和目标列表，随后四次、每秒一次、每次原伤害的 0.25/4；积压按到期时间执行，每次脉冲计入原有 128 额度。`RemainingPulses` 使用 byte，避免增加记录对象大小。修改排序时间前已出队；净化和死亡终止剩余次数，回调内净化亦有效。 |
| 容量与提交 | 普通法术、兽族天赋、符箓统一限制 16,384 条；未加载/失败状态拒绝入队。处理中的记录暂占容量名额，防止伤害回调重入突破上限。现有普通法术/天赋提交与符箓使用路径均在队列成功后提交；本轮保留其执行顺序。 |
| 测试入口 | 有效测试迁至根目录 [tests](tests/README.md)，正式项目由 `tests/projects.json` 明确列出；默认全跑、缺少项目失败。源码通过项目链接引用，输出进入忽略目录。全量脚本调用同一正式入口。 |
| 技能分配 | [MclslCombatCareerCatalog.cs](MySimulatedLongevityRoad/code/MySimulatedLongevityRoad/Data/MclslCombatCareerCatalog.cs) 三个属性改为 switch，五阶常量保持一致，非法阶位显式失败。 |

共享运行预算仍为普通 4ms、年度背压暂停 5ms；没有增加更新频率、排序、锁、后台定时器、全世界角色扫描或逐角色委托。性能监控默认关闭的配置保持原样。

## 验证结果

| 检查 | 结果 |
| --- | --- |
| 生产编译 | 通过，281 个 C# 输入；16 项既有资源反序列化 DTO 的 CS0649 警告，无编译错误。 |
| 正式回归 | EconomyPlanScopeTests、LineageCleanupTests、SharedHelperTests、RuntimeQualityTests 全部通过；新增 RuntimeQualityTests 93 个检查通过。 |
| NML 源码选择 | 通过，动态计数 281，测试没有进入模组目录。 |
| 本地化 | 通过，1,329 个词条，50 个配置项，0 错误；不代表动态 UI 游戏内验收。 |
| 玩家包 | 280 个源文件，保留既有开发编辑器排除规则；全部源码字节与当前工作区对应文件相同；无测试源码、替身、项目文件或 DLL/PDB。解包后的全部玩家源码再次编译通过。 |
| 预建灵体图集 | 包装校验通过；像素测试通过。 |
| 资源/来源门禁 | 未通过，详见下段；全量 verify-all 仍然失败，不将失败改为跳过。 |
| 实际 Unity | 尚无本轮真实运行记录，功能集成及性能矩阵待验收。 |

资源未在本轮修改。`validate-current-assets.py` 首先报告鲲鹏 `idle_0.png` 哈希与清单/来源记录不符；`validate-beast-talents.py` 的图像摘要检查亦失败；`validate-beast-overhaul.py` 报告 114 项既有法术阶段图像摘要不符。另外，工作区缺少 `GameResources/actors/Souls` 原始目录，目录重建型灵体测试失败，现有预建图集验证通过。没有修改图像或伪造来源记录来消除这些失败。

原始日志位于 `MySimulatedLongevityRoad/build/quality-20261101/`：`compile-final.log`、`regressions.log`、`verify.log`、`beast-resources.json`、`package-check.json`、`player-compile-final.log`。`git diff --check` 使用 `core.whitespace=cr-at-eol` 检查通过，保留工作区原有 CRLF/LF 风格。

## 前后离线数据

对照直接编译实施前 ZIP 和当前生产源码，采用同一游戏边界替身、.NET 10 CoreCLR、Release 优化；执行 5 对交替样本。下表为中位数。每组 128 次火符施放均实际完成 **640 次伤害、10,240 总伤害**，没有用漏执行减少工作量。JSON 比较的是相同的剩余持续伤害工作量。

| 指标 | 实施前 | 实施后 |
| --- | ---: | ---: |
| 100,000 次读取三项技能属性：分配 | 36,000,000 B | 0 B |
| 128 次火符入队：分配 | 48,128 B | 48,128 B |
| 初次命中后记录数 | 512 | 128 |
| 初次命中处理：分配 | 188,416 B | 6,144 B |
| 全部队列伤害处理：分配 | 188,704 B | 24,576 B |
| 全部队列伤害处理：耗时 | 0.5447 ms | 0.3016 ms |
| 保存 10 次：分配 | 8,813,120 B | 2,360,640 B |
| 保存 10 次：总耗时 | 25.5028 ms | 7.1605 ms |
| 加载 10 次：分配 | 3,715,680 B | 1,147,040 B |
| 加载 10 次：总耗时 | 52.6656 ms | 15.7103 ms |
| 相同剩余伤害的 JSON 字符数 | 133,121 | 35,969 |

另有生产轮次维护测试：1,000 个活跃标识、容量预热后连续 1,000 轮，轮次维护与技能读取测得新增托管分配 0 B。队列节点仍按现有 SortedSet 分配，没有宣称整个队列零分配。

完整逐样本数据、输入哈希及验收状态保存在 [QUALITY-OPTIMIZATION-20261101-results.json](QUALITY-OPTIMIZATION-20261101-results.json)，重跑方式见 [tests/README.md](tests/README.md)。这些是短时离线微基准，未测量 Unity 运行帧、GC 次数或内存峰值，不能据此宣称“完整场景性能不退化”。

## 本地玩家包与尚未完成的验收

按现有约定生成 player + compress-assets 包，源码素材未修改。最终文件位于 `MySimulatedLongevityRoad/build/quality-20261101/player-local-final-v2/MySimulatedLongevityRoad-0.8.3-player-local-compressed.zip`，75,857,762 字节，SHA256：

`a9e459046aa44c7d3095424df535ede48f68502591e4bbc0a6730e3e8513c14c`

由于上述既有资源门禁失败，本地快照使用工具已有的 `--local-development --skip-validators` 路径，并在包记录中明确标记 `validatorsSkipped=true`、`unityRetested=false`。这不代表资源验收通过。包内 README 已明确仅支持当前构建的当前格式新存档。

实际 Unity 中仍须完成原计划的 100/500/1000 单位及五种负载、30 秒预热/120 秒采样、至少 5 对前后交替、关闭诊断对照、各 10 次新存档读写。必须同时记录帧中位数/P95/P99、模组 CPU、GC 分配/次数、内存峰值、最长角色等待时间和实际效果完成数量；结果波动时扩展至 10 对。核心关闭加载/重新启用、五模块存档保护联动，以及实际背包/法力/冷却提交还需游戏集成验证。离线单元测试和调用顺序复核不代替这些记录。

当前结论：**代码离线回归及编译通过；资源门禁未通过，完整场景性能验收待完成。**
