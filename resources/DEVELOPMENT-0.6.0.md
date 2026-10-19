# 0.6.0 开发说明

使用.NET SDK、Python 3（安装Pillow）和对应游戏的NeoModLoader依赖。游戏路径通过 `-GameRoot` 传入，Python通过 `-PythonExecutable` 指定。游戏、Unity和加载器DLL不随包分发。

构建：`./tools/build-release.ps1 -GameRoot <游戏目录> -AllowUnreviewedAssets`。

打包：`./tools/package-release.ps1 -GameRoot <游戏目录> -AllowUnreviewedAssets`。

当前采用的独立原图保持原相对路径，提示词和来源见清单引用的provenance。用 `python tools/release_assets.py stage-from-manifest` 可从原图重建制作候选，PNG不重编码；现有安装清单不会因此改变。

旧审看记录仅作历史；本版新增的用户豁免不声明图片审看或完整实机验收通过。资源完整性可用 `python tools/release_assets.py validate` 检查，动画逻辑可用 `dotnet run --project tools/animation-tests/AnimationTests.csproj -c Release` 检查。
