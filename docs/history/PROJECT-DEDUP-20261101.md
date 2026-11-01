# 全项目重复实现检查与整合

本轮扫描全部 281 个实施前生产 C# 输入（code 目录 280 个，加模组入口），并复核跨文件重复片段和方法体。未调用 DSH，未覆盖此前修改。codegraph 已安装版本不支持 duplicates 子命令，实际使用源码归一化片段及方法体比较；该检测提供候选，不证明所有语义重复均已识别。

## 已整合的九组实现

| 原重复范围 | 统一入口 | 保留的行为 |
| --- | --- | --- |
| 世界灵魂注册、境界光环贴图加载 | Core/MclslSpriteLoader.cs：TryLoad | 四级查找顺序及异常返回 null |
| 世界灵魂注册、逆理系统按 ID 查找灵魂 | Systems/WorldResources/MclslWorldRunRepository.cs：FindWorldSoulById | 首个匹配、空集合和空标识返回 null；不增加索引或扫描 |
| 兽族排行榜、角色卡片战力显示 | UI/MclslRankTextFormat.cs：FormatPower | 凡人、万、亿、万亿边界及当前文化格式 |
| 图鉴快照、历史书事件分类 | Data/MclslEventCatalog.cs：EventCategory | 显式分类优先、事件类型回退、空记录默认分类；同时更新 partial 文件调用 |
| 奇遇、地图位置哈希 | Core/MclslStableHash.cs：Positive43x53 | 初值 43、乘数 53、unchecked 溢出与正数掩码；其他种子/乘数算法不变 |
| 古法突破、古法状态路径名称 | Systems/WorldResources/MclslWorldEpochSystem.cs：SpiritualPathLabel | 仙道、旧法、古法阶段名称与时序 |
| 古法突破、古法事件、新法事件、逆理属性调整 | Systems/WorldResources/MclslResourceSystem.cs：AddClamped | 货币专用长整型路径及普通属性 Clamp 路径 |
| 修炼、新法突破资质解析 | Systems/Cultivation/MclslSpiritualRootSystem.cs：ResolveCultivationGift | 原默认资质、1～100 范围及灵根优先顺序 |
| 洞天、天地变化地点选择 | Systems/WorldResources/MclslMapLocationResolver.cs：PickLocation / LocationSeed | 城市去重、最多 64 个、确定性选择、坐标及荒域回退；两处原类型默认地点虽不同，但实际构造均显式写入 Location，合并不改变返回值 |

没有引入委托、接口调度、LINQ、缓存、额外全世界扫描或更新频率变化。原实现直接移动到内部共享静态入口，调用方不保留转发副本。方法移动不等于实际 Unity 性能验证；共享类的冷启动成本仍需实际运行测量。

## 经复核保留的相似代码

- 洞天 CaveClaim 与天地变化 ChangeClaim 的 CountValidClaims 方法外形相同，但调用各自 IsValidClaim，记录类型和有效性规则不同。没有通过逐项委托或公共接口强行合并。
- 两个排行榜存在相似布局、下拉框、刷新逻辑，但还涉及各自尺寸、筛选、排序和控件状态。本轮复用确定等价的显示格式，不整体重构窗口。
- 死亡瘴气与世界灵魂反射读取角色的异常日志标识不同；兽族与具名角色的贴图初始化还包含不同的周边注册流程。保留这些语义边界。
- 不同哈希初值、乘数、缺省值，以及各模块不同的存档校验规则不作等价合并。

不能将以上结果表述为“整个项目不存在重复”。本轮已消除确认等价且适合共享的九组实现，剩余相似片段有明确的类型或行为差异。

## 验证与交付

- 全生产编译通过：283 个输入，仍为 16 项既有 CS0649 警告。
- 四个正式回归项目通过：RuntimeQualityTests 93 项；SharedHelperTests 23 项，包含本轮新增的哈希固定样本、战力边界和预热后 100,000 次哈希读取零分配检查。
- 玩家包源码 282 个（沿用开发编辑器排除规则），字节与工作区一致，无测试源码/替身/项目文件及 DLL/PDB；解包源码再次编译通过。
- 本轮实施前源码快照和原始日志保留在 MySimulatedLongevityRoad/build/quality-20261101/project-dedup/，包含 before.zip、compile.log、tests.log、method-duplicates-after.json、package-check.json、player-compile.log。仅匹配单行声明的精确方法检测仍有一组候选，即上述两种 CountValidClaims；该结果不覆盖所有近似或语义重复。
- 既有资源门禁失败尚未解决；包使用已有 local-development/skip-validators 路径，仅供验收。仅支持当前构建的当前格式新存档。
- Unity 完整场景性能验收仍待完成，不能据编译、替身测试或静态分析宣称完全没有性能损耗。

本地压缩包：MySimulatedLongevityRoad/build/quality-20261101/player-project-dedup/MySimulatedLongevityRoad-0.8.3-player-local-compressed.zip。

大小 75,858,161 字节；SHA256：799e49da606eaa79f72520720513af55be077c834f470ba5ff8086874ae50c69。
