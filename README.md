# 我的模拟长生路

WorldBox 模组，作者：溪上翁。模组版本 `0.1.4`，`mod.json` 中的目标游戏构建为 `115`。
本项目由“0.5.1+我的模拟长生路0.1.4-新法时代优化-最终版.zip”整理而来；压缩包名称中的版本信息与模组元数据分别保留，不推定其兼容关系。

## 目录

- `InterestingTrait.sln` / `InterestingTrait.csproj`：原有解决方案与 C# 项目，目标框架为 .NET 6。
- `InterestingTrait.cs`：NeoModLoader 模组入口。
- `code/MySimulatedLongevityRoad/`：核心逻辑、数据、系统、补丁、查询及 UI。
- `GameResources/`：游戏资源。
- `Locales/`：本地化资源。
- `mod.json`：模组元数据。
- `default_config.json`：默认配置。
- `icon.png`：模组图标。
- `docs/IMPORT.md`：导入来源与完整性记录。

## 开发与构建

安装支持 `net6.0` 的 .NET SDK，并准备 WorldBox 与 NeoModLoader 的对应依赖。
原项目通过相对路径 `../../worldbox_Data/` 引用游戏和加载器程序集，包括 Unity、Harmony、NeoModLoader 与 `Assembly-CSharp-Publicized.dll`。
这些外部 DLL 未包含在压缩包中，也未加入仓库。构建前应让该相对路径指向匹配的游戏数据目录，或按本机安装位置调整项目中的 `HintPath`。

在项目目录执行：

```powershell
dotnet build .\InterestingTrait.sln
```

开发工具可直接打开 `InterestingTrait.sln`。安装到游戏时，保留入口、code、GameResources、Locales 和配置文件之间的相对位置，并遵循本机 NeoModLoader 的模组目录要求。

## 版本管理

源码、资源和配置纳入 Git；构建输出、IDE 缓存和本地临时文件忽略。

```powershell
git status
git add .
git commit -m "描述本次修改"
```

本次整理保留压缩包中文件内容和原有目录关系，未更改游戏逻辑。当前环境缺少 .NET SDK，尚未验证编译及游戏内运行。
原包未附带许可证，作者信息保留在 `mod.json` 中；本仓库未额外授予使用或分发许可。