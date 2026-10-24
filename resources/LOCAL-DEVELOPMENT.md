# 0.7.0 生灵与职业版本开发说明

0.7.0 新增生灵目录、玄黄族文明及六种战斗职业。当前资源对应表见 living-resources.json；实施及本轮验收状态见 living-beings-implementation.md。正式发布由 tools/validate-living-assets.py --release 检查本轮 Unity 结果与当前源码/资源指纹；交互式视觉审看和性能测试单独记录，不沿用旧版通过标记。下文 0.6.0 的生物优化细节作为保留资源的背景说明。

默认交付采用此前约50多MB版本的压缩玩家包：包含完整运行源码与资源，PNG最多256色，大图标最长边256；仅压缩包内副本，工作目录原素材保持不变。实际体积随资源变化，不强行压到固定大小。开发包仅在明确要求时制作，不提交或推送 GitHub。安装需要 WorldBox 0.51.2 和 NeoModLoader，将包中的 MySimulatedLongevityRoad 文件夹放入 mods，避免重复安装；建议先用新世界测试，已有存档应备份后验证迁移。

2026-10-20 增量修改、尺寸比例、兽潮对照及验收限制见 beast-optimization-report.md。此次接入凤凰和大风共28张静止/移动重绘帧，并按 actor-size-policy.json 的 renderAdjustments 调整显示比例；下述PNG尺寸描述为原精简版资源基础，不代表新的游戏内实测。

## 图片与体型

保留 62 套形态、1,374 帧最终动画、38 个专用图标，以及此前选用的全部修复帧。具名兽形青神龙、帝叁貘、帝一的 PNG 长边分别为 400、320、256 像素；其他兽形按最低兽潮等级分为 128、160、224、320、400 像素；所有人形长边 256 像素，显示高度对齐精灵旅者。精确参照、像素尺寸和显示高度记录在 actor-size-policy.json。

每帧从最终大图进行一次预乘透明度的 Lanczos 等比缩放，完整画布保留，无裁剪、无调色板量化。缩小会减少细微像素信息。动画顺序、时长及原有各帧相对尺寸保留，锚点和 PPU 同步换算。初始化和等待姿态也使用新版资源，缺失新版资源会报错，不再寻找旧图。

旧动画、重复原图、失败候选、回滚副本、旧包及失效生产脚本已清理。当前运行 PNG 每帧一份，来源提示词、原始哈希、修复记录与转换输入/输出哈希保存在 actor-provenance 中。项目外 generated_images 和 Git 历史未清理。仍被玩法使用的地形精怪、法术和物品等资源继续保留。

## 本地构建与打包

需要 .NET 10 SDK、带 Pillow 的 Python，以及本机 WorldBox/NML 依赖。按自己的环境调整 Python 和游戏路径：

```powershell
tools/build-release.ps1 -LocalDevelopment -PythonExecutable <带Pillow的Python路径> -GameRoot <游戏目录>
tools/package-release.ps1 -LocalDevelopment -PythonExecutable <带Pillow的Python路径> -GameRoot <游戏目录> -OutputDirectory <项目内新目录>
```

默认的压缩玩家包包含运行源码和资源，由 NeoModLoader 编译加载；编译验证输出不会作为多余 DLL 打进包。FILE-MANIFEST.json 记录所有包内源文件的 SHA256、基础提交及未提交状态；外部 SHA256SUMS.txt 用于检查 ZIP。需要开发包时显式使用 tools/package-release.py --kind developer --local-development --output build/<新目录>。

## 验证范围

资源校验、编译和离线加载/动画测试不等于 Unity 实测。本次新增凤凰/大风静止和移动帧的离线审查，游戏内尺寸、状态切换、存读档迁移和性能仍未验证。history/20261019 中的实际运行记录属于旧版本，本次没有沿用其“已通过”标记。正式发布校验继续阻止把本地开发包称作新实测通过版本。

可单独运行 `tools/validate-current-assets.py`、`tools/validate-beast-overhaul.py`、`tools/test-current-assets.py`，以及 `dotnet run --project .dev-tools/animation-tests`、`dotnet run --project .dev-tools/actor-loading-tests`。前两个 Python 工具使用 `--release` 时会拒绝此次未重新实测的版本。

一条命令跑完全部离线检查：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/verify-all.ps1
```

默认依次检查 NML 源码选择、本地化、人物资源、兽潮资源，编译一次并运行 actor-loading / animation / death-visual 三个测试；报告与隔离的中间产物写入 `build/verify`。没有本机游戏目录时加 `-SkipBuild`，CI 工作流 `.github/workflows/verify.yml` 就是这样跑的。测试替身不再依赖 `build/`：`MCLSL_ACTOR_FIXTURE`、`MCLSL_ANIMATION_FIXTURE`、`MCLSL_WORKSPACE` 可重定向临时目录。

性能面板（`MclslPerformanceProbe`）新增“贴图常驻 / 显卡报告显存 / 原生帧缓存与预算 / 待加载帧”一行，用于确认显存改动前后是否生效；这仍是采样读数，不能替代 Unity 实测结论。

## NML 加载修复

NML 会递归编译模组目录中的所有可见 `.cs` 文件，原开发包的测试替身因此与游戏类重复定义，造成加载失败。全部 C# 测试和元数据工具现位于 `.dev-tools`，该点号目录被当前 NML 明确忽略；Python/PowerShell 构建工具仍位于 tools。构建与资源验证都会检查 NML 的实际源码选择，只允许正式生产源码参与编译。

安装修复包前，先将旧模组文件夹移出 mods，再完整解压新包。不要直接覆盖旧目录，否则旧 tools 下的测试文件会残留；也不要把 .dev-tools 改成不带点号的目录。
