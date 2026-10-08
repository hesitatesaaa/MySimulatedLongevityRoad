# 年度停滞与原生时间控制修复（2026-10-04）

> 历史诊断记录：本文的原生控速修复已由后续[无降速方案](annual-no-throttle-1500-validation.md)替代。跨年等待状态修复继续保留，当前版本不再挂载原生时间控制补丁。

## 原因与旧版对照

对照用户提供的 `0.5.1+我的模拟长生路0.4.0.zip`（内部源码目录为 `0.5.1+我的模拟长生路0.3.5-天资分类`）和当前 0.5.2，确认两处回归：

1. 当前 `MclslModuleHub` 在世界年度完成前将 `RuntimeCadence` 标为 `AwaitingWorld`。完成后递增 `NextYear` 时却没有清除此标记。高倍速下如果下一年的请求已经排队，下一年将直接等待世界完成，却永远不调用启动它的 `TickAnnual`。旧包没有该等待状态。
2. 旧包通过 Harmony 拦截 `MapBox.updateSimulation(float)`，实际缩放传入步长或跳过本次模拟。当前版本删除了该补丁，改写 `Time.timeScale`。目标游戏程序集显示，`MapBox.Update` 调用 `calculateCurElapsed`，后者返回 `Time.fixedDeltaTime × Config.time_scale_asset.multiplier`；`checkMainSimulationUpdate` 再按原生 ticks 调用 `updateSimulation`，其内部使用传入步长更新 `MapStats.updateWorldTime`。因此仅修改 Unity 时间倍率不能控制这条原生时钟链。

原报告中世界 389 年、已完成 114 年、activeYear=0、ready=0、waiting=703、年度处理速度为零，与第一处永久等待相符。它不能解释为“700 名修士每年都需要很久”；单纯放宽处理预算也无法唤醒没有启动的下一年。

## 修复

- 提交上一年、推进到下一年时清除 `AwaitingWorld`，保留年度完成屏障和逐年事件处理。
- 恢复原生模拟入口的步长控制，每次模拟调用前检查最新积压；0–2 年原速、3–7 年 75%、8–11 年 50%、12 年暂停并追至不超过 2 年。阻塞立即暂停。
- 移除无效的 Unity timeScale 控制器。原生速度档位、玩家／界面暂停状态由游戏保留；关闭世界或禁用核心后不再应用背压。
- 时间补丁挂载失败时显示明确提示。报告包含 `nativeTimePatch`、`nativeCalls`、`nativeSkipped`、`nativeStep=输入->输出`，区分策略计算和实际入口调用。
- 不提高每帧 CPU 预算、不跳年、不迁移或批量补算旧积压存档。

## 验证与边界

- 新增连续多年年度启动测试。修复前明确失败于 `RuntimeCadence:43` 未执行；修复后 42–45 年各启动一次。
- 回归项目直接引用生产 `MclslAnnualBackpressure`，验证实际传入步长、12 年暂停／2 年恢复、任务阻塞、核心停用、世界清理及补丁缺失提示。
- 闭环负载测试使用生产模块调度器和背压控制器：700 个模拟角色、每人每年 24 步、每帧只处理 32 步、原生 x20 步长运行 25000 帧。已完成年度从 99 推进到 146，最大落后 12 年，确实出现暂停及恢复。角色工作量为合成数据，不代表真实游戏 CPU 性能。
- `Build` 增加目标游戏程序集与编译产物的接线检查，核对世界时钟调用、补丁注册、ref 步长及阻止模拟的布尔返回值。此检查不执行 Harmony 或 Unity。
- 统一静态检查、原生接线检查和 Release 构建通过（0 警告、0 错误）。RegressionSafety、ProfessionGradePolicy 及 EconomySimulation（100 个种子 × 1000 年）通过。
- 当前环境未完成真实新世界 x20／约 700 修士长测，也未完成游戏界面暂停、读档切换的实机操作；这些仍需要在游戏中验收。自动测试不能替代这部分结论。

每次相关重构必须同时保留连续年度启动、生产背压控制器和原生接线检查，不能仅验证阈值计算。
