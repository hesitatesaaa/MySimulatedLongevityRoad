# 0.6.0 本地精简开发版

本次只制作本地开发者包，不提交或推送 GitHub。安装需要 WorldBox 0.51.2 和 NeoModLoader，将包中的 MySimulatedLongevityRoad 文件夹放入 mods，避免重复安装；仅使用新存档。

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

开发者包包含源码和资源，由 NeoModLoader 编译加载；编译验证输出不会作为多余 DLL 打进包。FILE-MANIFEST.json 记录所有包内源文件的 SHA256、基础提交及未提交状态；外部 SHA256SUMS.txt 用于检查 ZIP。

## 验证范围

资源校验、编译和离线加载/动画测试不等于 Unity 实测。此次不重新审图，不追加游戏内尺寸或性能测试。history/20261019 中的实际运行记录属于缩放前版本，本次没有沿用其“已通过”标记。正式发布校验继续阻止把本地开发包称作新实测通过版本。

可单独运行 `tools/validate-current-assets.py`、`tools/validate-beast-overhaul.py`、`tools/test-current-assets.py`，以及 `dotnet run --project .dev-tools/animation-tests`、`dotnet run --project .dev-tools/actor-loading-tests`。前两个 Python 工具使用 `--release` 时会拒绝此次未重新实测的版本。

## NML 加载修复

NML 会递归编译模组目录中的所有可见 `.cs` 文件，原开发包的测试替身因此与游戏类重复定义，造成加载失败。全部 C# 测试和元数据工具现位于 `.dev-tools`，该点号目录被当前 NML 明确忽略；Python/PowerShell 构建工具仍位于 tools。构建与资源验证都会检查 NML 的实际源码选择，只允许正式生产源码参与编译。

安装修复包前，先将旧模组文件夹移出 mods，再完整解压新包。不要直接覆盖旧目录，否则旧 tools 下的测试文件会残留；也不要把 .dev-tools 改成不带点号的目录。
