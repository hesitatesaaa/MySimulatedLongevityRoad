# 代码质量施工补充记录（2026-11-03）

本轮自行实施，没有委派 DSH。承接既有施工记录，补齐存档保护、世界初始化、默认设置事务、年度失败写入和业务回归 CI。保留施工前工作区已有改动；未改变存档版本、键名、资源图片或资源校验规则。

代码实现及本机离线回归已完成。发布验收尚未完成：资源门禁存在失败，Unity 集成和性能实测未执行。本文不替代原施工报告，也不将此前跳过的静态状态注册表化或可选数据外置标记为完成。

## 1. 存档与初始化

- [存档状态与加载](D:/模拟长胜路/code/MySimulatedLongevityRoad/Systems/WorldResources/MclslWorldArchiveStore.cs:9)：区分等待世界、确实无档、主档有效、备份恢复、故障锁定。世界尚未就绪时不创建新档；主档损坏、版本不兼容或读取失败时尝试备份；两份均不可用时禁止覆盖。
- [统一写保护](D:/模拟长胜路/code/MySimulatedLongevityRoad/Systems/WorldResources/MclslWorldArchiveStore.cs:83)：初始化异常也锁住存档层，覆盖还真、时代切换等直接保存入口。同一世界进入周期内不自动重试故障；明确重新载入/清理后才允许重新初始化。
- [备份轮换](D:/模拟长胜路/code/MySimulatedLongevityRoad/Systems/WorldResources/MclslWorldArchiveStore.cs:96)：只将已验证的主档原文放入备份。从备份恢复后的首次保存只修复主档，保留有效备份；首次建档先建立备份再写主档。写入失败不发布“保存成功”状态。
- [初始化状态机](D:/模拟长胜路/code/MySimulatedLongevityRoad/Core/MclslWorldInitialization.cs:3)和[真实入口](D:/模拟长胜路/code/MySimulatedLongevityRoad/Core/MclslRuntime.cs:104)：使用 Waiting → Initializing → Ready，异常进入 Faulted。等待世界可重试，部分执行后异常不可逐帧重放。核心模块及公共进入步骤全部完成后才发布 Ready。
- [保存入口](D:/模拟长胜路/code/MySimulatedLongevityRoad/Core/MclslRuntime.cs:248)：核心尚未初始化、初始化失败或档案不可写时，提前拒绝模组保存准备，避免把残缺状态写回。

保存键仍是 `mclsl.080.world-state.v1` 及其 `.backup`。两份 custom_data 是恢复世代，实际磁盘持久化仍由游戏原生保存负责，不能据此声称整个游戏保存具有原子事务。仓储导入、完整游戏对象及所有直接调用方的 Unity 行为仍需集成验收。

## 2. 默认设置恢复事务

[设置恢复入口](D:/模拟长胜路/code/MySimulatedLongevityRoad/UI/MclslSettingsReset.cs:31)采用以下顺序：校验默认配置的 ID、类型、覆盖范围和值 → 暂存配置值并回读核对 → 读取妖兽设置快照 → 写同目录临时文件 → Replace/Move 提交 → 刷新运行时设置、特效和 FPS 显示。

提交前失败会尝试恢复配置对象原值并清理临时文件，运行时刷新与特效回收尚未发生。回滚或清理自身异常有可见日志。提交后刷新失败时，保留已提交配置并明确提示“默认设置已保存，但运行时刷新失败，请重新载入游戏”，不声称磁盘或已发生的特效操作可以回滚。

[妖兽配置](D:/模拟长胜路/code/MySimulatedLongevityRoad/Systems/Beasts/MclslBeastConfig.cs:34)拆为启动迁移、纯读取 `Read` 与运行时 `Apply`。普通 `Load` 不再删除旧配置项或隐式保存；设置事务中的预读不触发运行时副作用。本轮没有将每个独立开关操作扩展为通用事务框架。

## 3. 年度结算失败入口

[仓储年度失败入口](D:/模拟长胜路/code/MySimulatedLongevityRoad/Systems/WorldResources/MclslWorldRunRepository.AnnualFailures.cs:10)统一维护阻断标记、失败记录与脏标记；世界阶段失败另统一维护阶段和次数。调度器、天玄镜清理、世界年度阶段的 8 个阻断调用点已接入。

失败原因使用不受诊断开关影响、按键节流的 Fatal 日志。记录仍保持最多 512 条、错误内容最多 300 字符；空 scope/target/step 先归一化再查重，避免同一空值失败重复新增。这是局部写入边界收敛，没有全面拆分大型仓储或迁移存档结构。

## 4. 回归与 CI

[正式测试入口](D:/模拟长胜路/.tests/run.ps1)保留默认 All 模式，并增加 Portable / Game 分类。13 个正式项目中，11 个无需游戏文件，2 个依赖游戏安装；跳过会打印名称和原因，不计入通过。[CI](D:/模拟长胜路/.github/workflows/code-quality.yml)配置 Windows / Ubuntu 执行 Portable。

新增测试直接编译生产源码，配置对象、游戏容器、依赖模块和特效池使用边界替身；JSON 使用真实 Newtonsoft.Json。设置测试实际创建文件并执行替换/移动，不复制一套业务判断来验证自身。

| 本轮新增项目 | 最终断言 | 覆盖重点 |
| --- | ---: | --- |
| ArchivePersistenceTests | 54 | 主备读取、空白/损坏和版本拒绝、恢复后首次保存、写入中断、世界代次、中央故障锁、单次序列化、初始化状态机 |
| SettingsTransactionTests | 75 | 配置校验、部分赋值回滚、保存与提交失败、提交先于特效变化、纯读取无隐式保存、提交后刷新异常 |
| RuntimeEntryTests | 90 | 实际 Runtime 入口；20 个依赖边界分别注入异常，禁止重放/保存，等待就绪、开关与重新载入 |
| AnnualFailureTests | 9 | 阻断、记录、脏标记、空值去重、次数、容量、日志异常时状态及可见原因 |

新增合计 228 项断言，全部 13 个正式项目通过。本机同命令通过不等同于远端 GitHub Actions 已运行；本轮未推送或执行远端 CI。

## 5. 验证证据

| 检查 | 本轮结果与证据 |
| --- | --- |
| 游戏程序集编译 | 295 个源码，exit=0，warnings=0，budget=0；[编译日志](D:/模拟长胜路/build/quality-followup-20261103/compile-final/compile.log) |
| 全部正式测试 | 13/13 通过；[最终日志](D:/模拟长胜路/build/quality-followup-20261103/tests-all-final.log) |
| 无游戏目录的 Portable 测试 | 11/11 通过，另 2 个明确跳过；[日志](D:/模拟长胜路/build/quality-followup-20261103/tests-portable.log)。该运行早于最后的空白存档、中央写保护及日志断言补充；最终版本随后由上述 All 回归覆盖 |
| NML 源选择 | sourceCount=295；[报告](D:/模拟长胜路/build/quality-followup-20261103/offline/nml-sources.json) |
| 本地化 | 1329 条、51 个配置项，errors=[]；[日志](D:/模拟长胜路/build/quality-followup-20261103/localization.log)。动态 UI 未验证 |
| 独立妖兽神通素材 | 21 种、504 帧、21 图标，离线通过；[日志](D:/模拟长胜路/build/quality-followup-20261103/beast-talents.log) |
| 全量资源门禁 | 失败，见下一节；不能宣称 verify-all 或发布门禁通过 |

复现代码编译和测试（PowerShell，从项目根目录运行）：

```powershell
& 'D:/模拟长胜路/tools/compile-check.ps1' -GameRoot 'D:/owl/gameversion/worldbox-ab712b72f394' -OutputDirectory 'D:/模拟长胜路/build/quality-followup-20261103/compile-recheck'
& 'D:/模拟长胜路/.tests/run.ps1' -Profile All -GameRoot 'D:/owl/gameversion/worldbox-ab712b72f394'
& 'D:/模拟长胜路/.tests/run.ps1' -Profile Portable
```

## 6. 资源发布阻断

`verify-all -SkipBuild -SkipTests` 在 Actor frames and icons 步骤失败。[原始错误](D:/模拟长胜路/build/quality-followup-20261103/actor-resources.log)指向[鲲鹏 idle_0.png](D:/模拟长胜路/NativeActorResources/actors/Beasts/kunpeng/Beast/main/idle_0.png)：

- 图片实际 SHA256 与资源清单一致：`23c5815208a0c2fca046c0193f95de44262e02fa6614e694e58576b39eeb94d9`。
- [来源记录](D:/模拟长胜路/resources/actor-provenance/mclsl_beast_kunpeng/native/idle_0.json)中的 outputSha256：`ef3365ab9dbbe9f03285405011d90f458a7e5597304869efaf5ff5e6f7ad1e2c`。
- 校验要求三者一致，因此该帧未通过。不能仅改来源哈希来冒充来源链已核验。

独立运行妖兽资源总检另报告 114 个 invalid、missing=[]、resourceValid=false、releaseReady=false，[完整报告](D:/模拟长胜路/build/quality-followup-20261103/beast-resources.json)列出受影响项。这个数字是该校验器的无效条目数，不是全部资源错误总数或损坏图片数量。

本轮未修改这些图片、来源记录、资源清单或校验器；未绕过门禁生成玩家包。后续需要核对实际素材与可信来源链，再恢复资源门禁。资源通过后按既有 player + compress-assets 规则生成压缩玩家包，保留工作目录原素材。

## 7. 性能边界与尚待验收

本轮新增的常态逐帧逻辑是枚举/布尔状态检查，没有新增逐帧反射、JSON 操作、全量实体遍历或集合分配。正常保存只序列化一次，不重新解析旧主档；保留的是已验证原文的引用，不额外复制同一 JSON 字符串。测试通过真实 JSON 回调断言正常保存的一次序列化和零次重新解析。

恢复默认设置时的快照、列表和文件操作只发生在用户触发的设置操作；年度失败日志和记录操作只发生在对应异常路径。这些代码约束不能证明 Unity 的 FPS、GC、CPU/GPU 内存或大存档耗时没有变化，本轮未作这些量化承诺。

待实际 Unity 验收：连续载入/保存/重载、核心开关切换、主档损坏后的备份恢复、初始化故障后无重复奖励/保存、默认设置与特效联动；100/500/1000 单位场景应有放置单位证据及实际运行采样。离线替身断言不能代替上述验收。

## 8. 变更范围与回退依据

施工前已有大量未提交修改，未用 HEAD 覆盖工作区。施工前快照为 [baseline.zip](D:/模拟长胜路/build/quality-followup-20261103/baseline.zip)，配套 [SHA256 清单](D:/模拟长胜路/build/quality-followup-20261103/baseline-sha256.json)。本轮独立差异见 [own-changes.diff](D:/模拟长胜路/build/quality-followup-20261103/own-changes.diff)，文件清单见 [own-changes.json](D:/模拟长胜路/build/quality-followup-20261103/own-changes.json)。

需要回退时，以本轮文件清单逐项比对快照，只撤销本轮差异，并保留后续人工修改。不要整库 checkout，也不要把快照覆盖到整个工作目录。测试和编译产物保留在 build 下，未加入玩家内容。
