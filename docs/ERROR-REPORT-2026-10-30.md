# 2026-10-30 错误报告核验

核对用户提供的 15:33:24、15:38:14、16:09:14、16:12:57 四份日志与当前源码。

| 错误 | 次数 | 结论 |
| --- | ---: | --- |
| MclslBeastSurfaceIndex 空引用 / 越界 | 383 | 属实，已修复源码 |
| WordLibraryAsset 格言重复注册 | 20 | 日志属实；本项目无对应注册代码，来源无法确认 |
| Subspecies.recalcBaseStats 空引用 | 1 | 日志属实；发生在原生存档亚种加载，缺少存档及完整游戏日志，根因未确认 |

## 修复

- 清图前暂停并清空地形索引，加载完成后恢复，避免清图期间地块回调访问旧索引。
- 处理地形事件前校验坐标、地图尺寸和地块对象归属，过滤旧地图对象。
- 地图尺寸变化后重新构建索引。
- 构建遇到空地块或未初始化地形时保留游标，后续重试。

## 验证

- tools/compile-check.ps1：274 个 C# 源文件编译成功，exit=0；存在已有警告。
- tools/test-beast-surface-index.ps1：通过海陆切换、重复事件、尺寸变化、负坐标、旧地块、清图、空地块、未初始化地形重试及延迟事件测试。
- 编译产物：../_compile-check/MySimulatedLongevityRoad.dll。

未启动游戏进行实机回归；隔离测试使用简化游戏对象，仍需在游戏中验证清图、新建不同尺寸地图和读档。不应据此宣称格言或原生亚种加载错误已解决。

## 游戏内验证尝试

- 已确认当前未运行 worldbox.exe，只有 OWL 启动器进程。
- 安装目录原先未包含本次修复；现已将 Core/MclslRuntime.cs、Patches/MclslBeastRuntimePatches.cs、Systems/Beasts/MclslBeastSurfaceIndex.cs 部署至 E:/wol/gameversion/0_51_2_0.51.2/mods/我的模拟长生路/code/MySimulatedLongevityRoad，三个文件的 SHA256 均与工作区一致。
- 原安装文件备份在 E:/0.8.0/_compile-check/installed-backup-before-surface-fix，未修改存档。
- 主、备用界面控制入口均因 MXC CreateProcessSecurityEnvironment failed (HRESULT = 0x80070002) 无法启动；因此没有实际启动或操作游戏，清图、新建地图和读档仍未实测，不计为通过。
- 安装的“全文明种族版中文名（三词库独立版）”中找到报告所列格言名称引用；尚不足以确定重复注册的具体根因，未修改该第三方模组。
