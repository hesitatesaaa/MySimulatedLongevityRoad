# 代码质量改造执行报告 2026-11-03

> 对应方案：`docs/CODE-QUALITY-IMPL-PLAN-20261103.md`
> 执行方式：一次性执行全部阶段（按用户决定：**M6-B 不做**、**M4 只合并逐字等价**）。
> 范围：除 P1-2 上帝类拆分外的全部评估项。

---

## 1. 结论

已完成 **M0、M1、M2（基础设施+关键点+清单）、M3、M4（等价子集）、M5、M6-A、M7**。
**M6-B（静态状态注册表化）按决定未做；M8（数据外置，可选）暂缓。**
全部改动后：**编译 exit=0、warnings 0/0**；**全量离线测试 ALL TEST PROJECTS PASSED**。

> 边界：以上均为**编译/离线层**结论。M6-A、性能类结论**未做 Unity 实机验证**，不能等同"游戏内通过"（AGENTS.md）。

---

## 2. 关键指标（施工前 → 施工后）

| 指标 | 施工前 | 施工后 | 说明 |
|---|---|---|---|
| 源文件（`code/`） | 290 | 291 | 新增 `MclslWorldStateAudit.cs`；编译含根 `MclslMod.cs` 共 293 |
| 总行数 | 73,766 | 74,390 | 格式化增加行 |
| 编译警告 | 16/16 | **0/0** | 抑制 CS0649 噪声并把预算降为 0 |
| >200 字符行 | 411 | 393 | |
| >400 字符行 | 27 | **5** | |
| 最长行 | 1010 | **577** | |
| 单行 `try/catch` | 74 | **2** | 剩 2 为内嵌 lambda，保留 |
| `catch (Exception)` | 305 | 305 | 结构未动，仅升级可见性 |
| `Probe` 站点 | 157 | 151 | 5 处升 Alert、1 处随 GetNumber 重构消除 |
| `Alert` 站点 | 22 | 28 | 新增可见降级告警 |
| `static` 可变字段 | 695 | 695 | M6-B 未做，维持现状 |

---

## 3. 分阶段执行情况

| 阶段 | 状态 | 落地内容 |
|---|---|---|
| M0 | 完成（改用文件快照） | git 写被沙箱 ACL 阻止，无法建分支/提交；改用 `_pre-change-backup/20261103/code-tools-docs-20261103.zip` 作回滚点 |
| M1 | 完成 | 修 `compile-check.ps1`/`build-release.ps1` 默认 `GameRoot`、`verify-all.ps1` 的 `tests/`→`.tests/`、`deadcode-scan.ps1` 硬编码路径；删除 `_probe_code.txt`；入口 `InterestingTrait.cs`→`MclslMod.cs` 并同步 6 处引用（package-release/release_inputs/check-nml-sources/runtime-acceptance/runtime-living-acceptance/test-nml-source-scan）；抑制 CS0649、预算 16→0 |
| M2 | 部分（基础+关键点） | 新增 `MclslDiagnostics.ProbeLazy`（禁用路径不再构造诊断串）；持久化关键点 `MclslWorldRunRepository` cs-1/2/3、`MclslHuanzhenSystem` cs-3/cs-7 由 `Probe` 升 `Alert`；生成 `docs/CODE-QUALITY-M2-PROBE-INVENTORY.md`（151 处待分级清单） |
| M3 | 完成（热点） | `MclslReflectionNumbers.TryGetNumber` 缓存读；替换 `MclslCultivationSystem.GetNumber` 私有反射实现（年度链每修士热路径）；新增缓存命中回归断言 |
| M4 | 完成（等价子集） | 新增 `MclslStableHash.Positive31x37 / Stable17x31`、`MclslActorAccessor.SafeName`；合并 6 处 SafeName、2 处 StableHash、2 处 PositiveHash（共 10 处）为转发；**其余因种子不同未合并** |
| M5 | 完成 | 拆分 `MclslHuanzhenSystem` 63 个属性到独立行；72 处单行 `try/catch` 拆多行 |
| M6-A | 完成 | 新增 `MclslWorldStateAudit.ReportResidual`（仅诊断开启时），挂到 `MclslRuntime.ClearWorldState` 末尾 |
| M6-B | **未做** | 按用户决定跳过（高风险，需实机） |
| M7 | 完成（增量） | `.tests/SharedHelperTests` 增加哈希等价向量与 `TryGetNumber` 读缓存断言（49 项全 PASS） |
| M8 | 暂缓 | 可选，未做 |

---

## 4. 验证证据

| 验证 | 结果 |
|---|---|
| 编译 `tools/compile-check.ps1` | `sources=293 exit=0 warnings=0 budget=0` |
| 全量离线测试 `.tests/run.ps1` | **ALL TEST PROJECTS PASSED**：SharedHelper 49、RuntimeQuality 93、NativeFeedback 83、HeavenGrant 350、BeastPopulation 32、EventArchive 21、AnnualYearReconcile 22 |
| NML 源扫描 `check-nml-sources.py` | exit=0，`sourceCount=293`（含 `MclslMod.cs`） |
| `SharedHelperTests` 直跑 | `PASS ... 49 checks` |

---

## 5. 偏差与未完成（如实说明）

1. **M2 未对全部 157 处逐一人工分级**。分级需按业务语义判断 expected/degraded/fatal，属人工评审工作量（原计划 2–3 天）。本轮交付：分级语义、`ProbeLazy`、5 处关键点升级、**全部 151 处清单** `docs/CODE-QUALITY-M2-PROBE-INVENTORY.md`。剩余 146 处默认按 expected 暂留，待逐条评审。
2. **M3 只替换了热点 `GetNumber`**；其余约 58 处裸 `GetField/GetProperty` 位于各原生对象的一次性探测，语义各异，未做批量替换（避免改变空值/类型语义）。已在方案中列为后续项。
3. **M4 只合并 10 处**：其余 `StableHash/PositiveHash` 副本使用**不同种子/乘子**（17×31、31×41、61×43、79×73、FNV 等），并非逐字等价；合并会改变姓名/生成物/事件 id 的确定性、破坏旧存档一致性。依据"只合并逐字等价"约束**保持不动**。
4. **M1 采用轻量静态分析**（抑制 CS0649、预算降 0），未引入 NetAnalyzers `/analyzer`（需试验 csc 直编加载，另立小任务）。
5. **更正一处早前误判**：P2-1 曾称 `FailedRequiredCount/Summary`"从未使用"。实际入口 `MclslMod.cs:65-74` **已使用**（未挂载 `LogWarning`、必需补丁失败 `throw`）。原因是旧入口文件在仓库根、不在 `code/` 统计范围，脚本漏扫。此项无需改动。
6. **`tools/test-nml-source-scan.py` 无法在本沙箱运行**（尝试写系统临时目录被 ACL 拒绝），非逻辑问题；真实仓库 `check-nml-sources.py` 已通过。
7. **git 提交/分支被沙箱 ACL 阻止**，本轮未形成 git 提交；回滚依赖备份压缩包。

---

## 6. 回滚方式

- 施工前完整快照：`_pre-change-backup/20261103/code-tools-docs-20261103.zip`（含 code/tools/docs/.editorconfig/入口文件等）。
- 如需回滚：解压覆盖对应目录即可；`docs/CODE-METRICS.json` 已按新状态重新生成。

---

## 7. 建议的下一步

1. 人工评审 `docs/CODE-QUALITY-M2-PROBE-INVENTORY.md`，把 degraded/fatal 站点升级（M2 收尾）。
2. 逐文件评估其余 58 处裸反射，能缓存则缓存（M3 收尾）。
3. 若要做 M6-B（静态状态注册表化），必须配 Unity 实机"加载/换档/读档 ×3"验收。
4. Unity 内开 `MCLSL_config_performance_monitor`，用 M6-A 的漏清报告与性能探针做一次实机核对。
