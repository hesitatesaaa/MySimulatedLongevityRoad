# 0.8.7 结构对齐与全部修改检测报告（2026-11-06）

## 一、结构对齐（项目 vs 0.8.7 开发包）

以 0.8.7 开发包 `MySimulatedLongevityRoad` 为基准逐文件比对：

| 目录/范围 | 结果 |
|---|---|
| 顶层根文件（mod.json / icon.png / default_config.json / MclslMod.cs / README.md / AGENTS.md / .gitignore / FILE-MANIFEST.json / README.txt） | **全部一致** |
| `GameResources` | 新增 0、缺失 0、内容不同 0 |
| `InterfaceResources` | 新增 0、缺失 0、内容不同 0 |
| `NativeActorResources` | 新增 0、缺失 0、内容不同 0 |
| `Locales` | 新增 0、缺失 0、内容不同 0 |
| `resources` | 新增 0、缺失 0、内容不同 0 |
| `tools` | 新增 2（仅 `__pycache__/*.pyc` 缓存），内容不同 0 |
| `code` | 包独有 0；项目独有 1（`UI/MclslActorWindowOpener.cs`）；内容不同 12（见下） |

结论：**打包内容与 0.8.7 包结构完全一致**。旧的 0.8.5 拆分文件（`MclslFeatureWindow.Bag.cs`、`MclslFeatureWindow.Market.cs`、`MclslRankComparators.cs`、`MclslRankEntryComparators.cs`、`MclslRankWindow.Sorting.cs`、`MclslUiPalette.cs`、`MclslReflectionCache.cs` 等）已随镜像删除。

项目根仍有开发专用目录（`.git`、`.tests`、`docs`、`build`、`.github`、`skills`、`validation`、`deliverables`、`_pre-change-backup` 等），它们不属于发布包，`tools/release_inputs.py` 的打包清单不包含它们。

## 二、此前的全部修改（12 个文件 + 1 个新文件）与检测

| 文件 | 增/删行 | 目的 |
|---|---|---|
| `UI/MclslActorWindowOpener.cs` | 新增 1 文件 | 契约/宠物跳转统一入口：先结束窗口动画再打开、重绑回调 |
| `UI/MclslContractInfoBar.cs` | +18/-1 | 定位按钮改走统一入口 + `RefreshFromOpen` 重绑 |
| `UI/MclslRankModels.cs` | +4/-0 | 增加战斗职业快照字段 |
| `UI/MclslRankSnapshotSource.cs` | +28/-0 | 读取战斗职业/阶位/熟练度 |
| `UI/MclslRankWindow.cs` | +64/-2 | 6 个战斗职业可选排序 + 分组比较 + 提示 |
| `Systems/WorldResources/MclslNativeHistoryBridge.cs` | +9/-0 | 原版历史写入 x/y、unit_id（原版定位） |
| `Systems/WorldResources/MclslWorldRunRepository.cs` | +48/-3 | 坐标补写后再镜像原版历史（待发队列/Flush） |
| `Systems/WorldResources/MclslWorldRunRepository.AnnualFailures.cs` | +38/-0 | 年度阻塞自愈（退避重试 + 读档清除） |
| `Systems/Runtime/MclslAnnualBackpressure.cs` | +1/-1 | `IsBlocked` 空安全 |
| `Systems/Runtime/MclslScheduler.cs` | +5/-2 | ProcessAll 先尝试解除年度阻塞 |
| `Core/MclslAnnualSchedulingPolicy.cs` | +1/-1 | 最高档 0x → 0.25x，避免世界冻结死锁 |
| `Core/MclslRuntime.cs` | +2/-0 | 进入世界时清除上次年度阻塞 |
| `Patches/MclslPatches.cs` | +34/-0 | `Actor.checkCalibrateTargetPosition` 空引用守卫（游戏卡住修复） |

自动检查：
- 重名/重复排序 ID：**无**（SortDefs 17 项，ID 唯一）。
- 冲突标记/占位残留（`<<<<<<<`、`TODO`、`FIXME`）：**无**。

## 三、编译与测试

- 编译：**300 个源文件，0 错误、0 警告**（本机 WorldBox 0.51.2 / build 115 依赖）。
- 离线回归：`.tests` **11 个 portable 项目全部通过**（EconomyPlanScope、LineageCleanup、SharedHelper、HeavenGrant、BeastPopulation、EventArchive、AnnualYearReconcile、ArchivePersistence、SettingsTransaction、RuntimeEntry、AnnualFailure）；2 个需游戏安装的项目按设计跳过。

## 四、仍需注意

游戏目录 `D:\owl\gameversion\worldbox-ab712b72f394\Mods\我的模拟长生路` 仍是 21:06 解压的 0.8.7 原包，**不含上述修复**。要让游戏生效，需要重新打包或部署覆盖该目录。
