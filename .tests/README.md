# 正式离线回归

需要 .NET 10 SDK，以及与模组编译一致的游戏安装目录。测试直接链接生产 `.cs`，不从 `build` 复制业务判断。游戏状态、伤害入口、时钟、存档容器等边界使用替身；测试不能证明 Unity 的帧耗时、视觉效果或实际存档性能。

从项目根目录运行全部正式项目：

```powershell
pwsh -NoProfile -File .tests/run.ps1 -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394'
```

`projects.json` 是明确的正式项目清单。清单为空或任一项目缺失都会失败；默认执行全部项目。`RuntimeQualityBenchmarks` 是单独的性能对照工具，不混入功能回归清单。

| 项目 | 生产实现 |
| --- | --- |
| EconomyPlanScopeTests | 年度经济操作作用域 |
| LineageCleanupTests | 谱系清理策略、事件目录 |
| SharedHelperTests | 数值反射与排行文本格式 |
| RuntimeQualityTests | 轮次快照与天赋间隔、加载状态、职业解析及缓存、完整待命中队列、技能常量及预热后分配 |

`RuntimeQualityTests` 的 93 个检查包括：跨帧续跑、集合增减、世界清理、0.1 秒间隔、异常原文保护、重复角色、数组长度、NaN/Infinity、过期时间、四次火符伤害、读档剩余次数、积压、死亡、净化与伤害回调内净化、16,384 条容量、128 次处理额度、回调重入容量保护和三种入队路径。

全量验证入口：

```powershell
pwsh -NoProfile -File tools/verify-all.ps1 -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394'
```

该入口保留资源、来源记录等门禁，不跳过失败检查。独立测试入口可在资源门禁失败时继续诊断代码回归。中间产物默认放在被忽略的模组 `build/tests`，不进入玩家包。`UseAppHost=false` 使测试通过 `dotnet` 启动，避免本机原生 apphost 初始化失败。

## 实施前后离线对照

先保留实施前当前工作区的源码 ZIP，不能用其他项目目录替代。示例使用本轮已经固定的输入：

```powershell
pwsh -NoProfile -File tests/compare-offline.ps1 `
  -BaselineArchive "$PWD/build/quality-20261101/baseline/source-snapshot.zip" `
  -WorkDirectory "$PWD/build/a-fresh-comparison"
```

输出目录必须尚不存在。工具分别编译 ZIP 内原始生产实现与当前生产实现，运行至少 5 对交替样本，记录源码/基线哈希，并要求技能读取分配为零、入队分配及完整队列处理分配不增加。每组火符均完成 640 次命中、10,240 总伤害；保存和加载各测 10 次。该工具采用短时间 CoreCLR 微基准预热，**不满足**正式 Unity 性能矩阵的 30 秒预热、120 秒采样要求。

完整场景性能、游戏内核心开关/重载联动，以及调用方在拒绝施法时的实际背包、法力、冷却提交行为，仍需 Unity 集成验收。详细结果见项目根目录 `QUALITY-OPTIMIZATION-20261101.md`。
