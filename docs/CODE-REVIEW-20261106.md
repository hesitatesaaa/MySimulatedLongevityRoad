# 代码质量评估报告（2026-11-06，仅评估未改动）

范围：`code/MySimulatedLongevityRoad`，299 个 C# 文件、约 7.6 万行（0.8.7 基线 + 本次修复）。
方法：静态扫描（规模/方法长度/异常/重复/反射/LINQ）+ 项目自带度量脚本 + 实际编译与离线回归。**未修改任何源码。**

## 结论速览

- 编译：300 源文件 **0 错误 0 警告**；`.tests` **11 个 portable 项目全部通过**。
- 工程底子不错（诊断分级、帧预算、清单化打包、离线回归齐全），主要问题集中在：**版本控制缺口、异常吞噬、上帝类/超长方法、重复代码、反射与热路径分配、文档度量漂移**。

## 主要发现（按优先级）

### P0 版本控制基线缺失
- `git status` 显示 **181 项未提交改动**（整个 0.8.7 导入 + 修复都还在工作区）。
- 影响：任何一次误操作/并发修改都难以回滚比对（本轮就出现过外部同时改动 `.tests` 的情况）。
- 建议：尽快 `commit` 一个 0.8.7 基线（或建分支），之后每次改动单独提交。

### P1 异常处理过宽、吞掉真实错误
- `catch (…Exception …)` 共 **309 处**；`catch` 总数 705。
- 其中 **153 处**是形如 `catch (System.Exception mclslEmptyCatchEx) { MclslDiagnostics.Probe("empty-catch-code-…") }` 的“只记诊断”式吞异常。
  - 例：`Core/MclslRuntimeSettings.cs:189-192`（`Find` 的递归兜底 catch，把所有异常当“找不到”）。
  - 例：`Systems/WorldSoul/MclslWorldSoulSystem.Manifestation.cs:283`（单行 351 字符的 catch 只 Probe）。
- 风险：逻辑错误被静默为“无结果”；游戏“卡住/数值不对”这类问题极难定位（本轮卡住问题就是原生异常每帧抛出被淹没在日志里）。
- 建议：
  1. 按边界收窄异常类型（`NullReferenceException`/`ArgumentException`/`InvalidOperationException` 分别处理），避免一律 `Exception`；
  2. 引入统一的 `MclslGuard.Try(action, key, fallback)`，把“可忽略”的白名单集中声明并计数上报，而不是散落 150 处；
  3. 对确实不可恢复的路径记 `Fatal` 并停模块，而不是 Probe 后继续。

### P2 上帝类与超长方法
- 类族总行数（含 partial 分卷）：`MclslCodexWindow` **4886**、`MclslFamilySystem` 2558、`MclslRankWindow` 2192、`MclslFeatureWindow` 1807、`MclslWorldRunRepository` 1676、`MclslHuanzhenSystem` 1648、`MclslTianxuanMarket` 1609。
- 单文件 >800 行：14 个；最大 `UI/MclslCodexWindow.cs` 2296、`UI/MclslRankWindow.cs` 2048、`UI/MclslFeatureWindow.cs` 1589。
- 超长方法：
  - `UI/MclslCodexSnapshot.cs:182 BuildCodexPageCaches` **302 行**
  - `Systems/Runtime/MclslScheduler.cs:701 TickAnnualActors` **176 行**
  - `Systems/Crafting/MclslItemUseSystem.cs:115 TryUseAutomatic` **166 行**
  - `UI/MclslCodexWindow.cs:462 DrawPage` 165 行
- 建议：按“数据快照 / 视图绘制 / 交互命令”三层继续拆分；对 >120 行方法设阈值并补单测；目标单文件 ≤800 行。

### P2 重复代码
- 检测到 **69 个 6 行级重复块**。
- 例：`UI/MclslBeastRankWindow.cs:393` 与 `UI/MclslRankWindow.cs:727` 同为 `int oldIndex = Entries.IndexOf(id); …` 的榜单重排逻辑；多处 `button.onClick.RemoveAllListeners()/targetGraphic/transition` 也重复。
- 建议：把“榜单列表重排/按钮绑定/卡片绘制”抽到既有 `MclslRankComponents`、`MclslRankCard` 或新帮助类，减少三套榜单（Rank/BeastRank/Codex）各写一份。

### P3 反射面偏大
- `AccessTools.` 95、`GetField(` 34、`GetProperty(` 23、`GetMethod(` 9、`BindingFlags` 43、`MethodInfo` 48。
- 风险：启动变慢、版本升级易断、错误只能运行期暴露。
- 建议：统一走已存在的缓存访问器（`MclslReflectionNumbers`/`MclslReflectionCache`），禁止在 Tick 热路径做反射；对每个反射目标加启动期自检。

### P3 长参数列表
- **127 个**声明 ≥6 个参数。示例：`Data/Death/MclslDeathSnapshot.cs:41` 构造函数 **34 个参数**、`Traits/MclslPhysiqueSystem.cs:18` **23 个**、`Systems/WorldResources/MclslWorldRunRepository.cs:838 AddMapVisualEvent` 10 个、`MclslGeneratedObjectFactory.cs:59 CreateWorldChange` 10 个。
- 建议：用参数对象/`record`/构建器替代超长参数表，降低调用错位风险。

### P3 热路径 LINQ 分配
- 5 个 Tick/Update 方法含 ≥2 次 LINQ：`Systems/Crafting/MclslTianxuanMarket.cs:1519 UpdateMarketPrices`（6 次）、同文件 `:398 RefreshActivitySnapshots`（6 次）、`UI/MclslMaobaoWindow.cs:79 Refresh`（3 次）、`UI/MclslCodexWindow.Biography.cs:48 RefreshBiographyIndex`（3 次）、`Systems/Cultivation/MclslTechniqueLineageSystem.cs:36 TickResolveAnnual`（2 次）。
- 建议：热路径改用 for 循环 + 复用 List/数组，避免每帧 `Where/Select/ToList` 分配。

### P4 可读性与文档
- 超长行：**357 行 >200 字符**、76 行 >300 字符。例：`Traits/MclslWorldSoulActorRegistration.cs:286`（577 字符）、`UI/MclslCodexSnapshot.cs:453`（522 字符）、`UI/MclslUiManager.cs:136`（444 字符）。
- 注释率约 **3.3%**（含行尾注释）；关键算法/边界条件建议补 XML 注释。
- 文档漂移：`docs/CODE-METRICS.json` 仍是 0.8.5 / 291 文件 / 74390 行；实际 299 文件、约 7.6 万行。建议重跑 `tools/report-code-metrics.py` 并纳入 CI。
- `#pragma warning disable CS0649` 2 处（`Systems/Visual/MclslLivingResources.cs:10`、`UI/MclslCodexWindow.Maobao.cs:19`）——用途合理（反射赋值字段），建议加注释说明或改为显式初始化。

## 做得好的地方（保持）

- 编译零警告；诊断体系分为 Fatal/Error/Probe 且支持开关（`MclslDiagnostics`）。
- 有帧预算与背压框架（`MclslFrameDeadline`/`MclslAnnualFrameBudget`/`MclslAnnualBackpressure`），并有离线回归覆盖。
- 打包清单化（`tools/release_inputs.py` 只收运行时目录），资源与工具可复现。
- 命名与目录职责清晰（Core/Data/Systems/UI/Patches/Traits 分层）。

## 建议的推进顺序

1. **P0**：提交 0.8.7 基线；CI 加“编译 + 11 项 portable 回归 + 度量生成”。
2. **P1**：把 153 处“只 Probe 的 catch”收敛为统一 Guard 并分类计数；至少把 NRE/越界/集合修改类异常改为显式上报。
3. **P2**：拆分 Codex/Rank/Feature 三个超大 UI 类族；抽榜单公共组件；>120 行方法逐个拆。
4. **P3/P4**：反射统一缓存并加启动自检；热路径去 LINQ；长参数改参数对象；重刷指标文档。

## 评估局限

本报告基于静态扫描、编译与离线回归，**未做 Unity 实机帧率/内存、视觉效果与完整存读档验收**；性能类结论（LINQ/反射）为代码层风险提示，需实机采样确认。
