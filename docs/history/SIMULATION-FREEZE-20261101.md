# 角色模拟卡死修复记录（0.8.4，2026-11-01）

## 结论与证据

本次日志确认了一个会持续中断角色模拟的模组错误：原版 `Actor.findCurrentTile` 在 `Parallel.ForEach` 工作线程中执行，模组后置回调直接修改兽族人口索引的普通 Dictionary，产生并发写入和内部状态损坏。

证据日志最初来自用户提供的 Player.log；调查时游戏重启，文件轮转为 Player-prev.log，已保存到 `MySimulatedLongevityRoad/build/freeze/Player-evidence.log`。该副本共173811行、30822873字节，SHA256：`9e8fa67310dcf234d20c8dfcb80e5f505c287ffd906bc411d9feb40a14bb5fa5`。

第1332行首次出现 `InvalidOperationException: Operations that change non-concurrent collections must have exclusive access`。同类异常共4196次，外层 AggregateException也出现4196次；这是同一组错误及其包装，不应相加为8392次独立故障。

调用链：`ActorManager.update → JobManagerBase/Parallel.ForEach → Actor.updateParallelChecks → findCurrentTile → MclslLifecyclePatches.TileChanged → MclslBeastPopulationIndex.Moved → Adjust → Dictionary.FindEntry`。

异常向上逃逸至 MapBox 的角色更新入口，导致角色更新反复中断。这能够解释时间仍推进但小人不动的现象；本日志没有证明存在另一种独立死锁。Curl证书提示不在该故障调用链上。

## 本次变更

- `Systems/Beasts/MclslBeastPopulationIndex.cs`：后台移动回调先检查线程，只对普通兽族资产前缀提交 Position 通知；不在后台读取或写入人口字典。主线程移动仍立即更新。所有原计数及自然/兽潮分类规则保持不变。
- `Core/MclslRuntimeChanges.cs`：内部标志位扩为 ushort，增加 Position；复用已有线程交接、按角色合并、世界代次校验、存活/对象一致性检查及删除取消机制。主线程处理 Position 后移除该位；若没有其他变化就返回，避免面板、排行榜和族群等无关更新。合并的其他标志仍正常分发。
- `Core/MclslRuntimeCollections.cs`：交接队列复用链表节点，空闲节点池最多8192个，取消及取出都回收，清理世界时释放。池的访问处于原有同一把锁内。
- `tests/BeastPopulationTests`：链接真实生产人口索引、RuntimeChanges 和 RuntimeCollections；替身仅提供游戏/周边模块接口，加入正式测试清单。

没有增加另一套调度器、逐帧全世界扫描、存档字段或玩家配置；版本仍为0.8.4。此前工作区修改予以保留，未使用DSH。

## 离线验证

- 新增19项检查通过：1000个角色各20次并行回调合并、自然/全部人口计数、最新位置、取消、死亡、世界代次、世界清理、主线程立即更新、非兽族过滤、混合标志、同ID对象替换、并行生产与消费等。
- 持续压力用例：1000个角色各移动99次，生产线程与主线程接收同时进行，最终区域人口1000、自然人口500，总数一致。
- CoreCLR微测：预热后10000次提交/取出新增托管分配0字节。这不等于Unity全场景性能验收。
- 正式7个测试项目通过；新用例最终输出见 `build/freeze/population-final.log`，全套见 `build/freeze/regressions-final.log`。
- 全量编译285个输入，退出码0；16条既有资源清单字段未赋值警告。
- 源码选择检查、本地化检查通过；17项既有生产程序集接线检查通过；diff空白检查通过。
- 压缩包284份模组C#源码逐字节匹配工作区，无测试源码、测试项目、DLL或PDB。

## 性能边界及运行复测

普通/年度背压共享预算仍为4ms/5ms，交接分发原有0.5ms局部截止时间不变。位置通知会消耗该既有预算，极端积压时人口区域统计可能延后更新；不会为追赶统计突破预算。首次容量增长仍可能分配内存，空闲节点池会保留有界内存。此次离线数据不能证明完整游戏帧耗时、锁竞争或内存峰值绝无变化。

需要完全退出并重启游戏加载修复版，再载入原问题存档，复测出现问题时的速度及兽族规模，确认小人持续行动，并检查新Player.log不再出现此调用栈。已损坏的运行中字典不能靠暂停/继续恢复；无需因此删除存档。本轮没有自动覆盖运行中游戏目录，也没有取得修复后的Unity运行记录。

交付状态：离线验证通过，游戏内恢复及性能验收待完成。既有素材校验问题未在本次处理，因此包为本地验证包，不宣称完整发布验收通过。

## 本地验证包

`MySimulatedLongevityRoad\build\freeze\player\MySimulatedLongevityRoad-0.8.4-player-local-compressed.zip`

大小：75864717字节；SHA256：`c8351a4448e6c8f628ecc111ac3a2d4ccde13fc69eb4078e78e3bd97e68e29ce`。
