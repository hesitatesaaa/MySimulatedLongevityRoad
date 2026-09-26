# 我的模拟长生路

> 当前模组版本为 **0.2.0**。新增职业、物品制作、乾坤袋、天玄镜及入道指南。

WorldBox 模组，作者：溪上翁、kinght。模组版本 `0.2.0`，`mod.json` 中的目标游戏构建为 `115`。本版本以提供的 `0.5.1+我的模拟长生路0.1.9.2.zip` 为基线。

## 目录

- `InterestingTrait.sln` / `InterestingTrait.csproj`：原有解决方案与 C# 项目，目标框架为 `netstandard2.1`，可由 WorldBox 的 Unity/Mono 运行时加载。
- `InterestingTrait.cs`：NeoModLoader 模组入口。
- `code/MySimulatedLongevityRoad/`：核心逻辑、数据、系统、补丁、查询及 UI。
- `code/MySimulatedLongevityRoad/DeveloperTools/`：开发者版角色编辑器与调试 API；只包含在开发者包中。
- `GameResources/`：游戏资源。
- `Locales/`：本地化资源。
- `mod.json`：模组元数据。
- `default_config.json`：默认配置。
- `icon.png`：模组图标。
- `CHANGELOG.md`：按版本记录功能更新和兼容性说明。
- `scripts/Generate-ItemCatalog.py`：依据参考 DOCX 重新生成 ID 驱动的物品目录。

## 开发与构建

项目目标框架为 `netstandard2.1`，用于 WorldBox 的 Unity/Mono 运行环境。项目不会把 WorldBox、Unity、Harmony 或 NeoModLoader DLL 提交到仓库。

默认依赖目录是项目外的 `..\..\worldbox_Data`，也可以通过 `WorldBoxDataRoot` MSBuild 属性指定本机安装位置。

```powershell
dotnet build .\InterestingTrait.csproj -c Release -p:WorldBoxDataRoot=D:\path\to\worldbox_Data
```

构建结果位于 `bin/Release/netstandard2.1/MySimulatedLongevityRoad.dll`。发布的源码包保留 `InterestingTrait.cs`、`code/` 和 `GameResources/`，供 NeoModLoader 加载。

开发者包附带 `DeveloperTools/`。需要启用人物编辑器、F8 快捷键和开发者专用入口时，使用以下属性构建：

```powershell
dotnet build .\InterestingTrait.csproj -c Release -p:WorldBoxDataRoot=D:\path\to\worldbox_Data -p:IncludeDeveloperTools=true
```

如果只需要检查项目文件而没有 WorldBox 依赖，可使用：

```powershell
dotnet msbuild .\InterestingTrait.csproj -getItem:Compile -nologo
```

不传 `IncludeDeveloperTools=true` 时，项目会把开发者工具源码排除在玩家构建之外；传入该属性后，开发者编辑器与调试 API 会一同编译。`references/` 始终不加入编译项。

## 原版 WorldBox 导入

本项目发布包沿用现有的源码型 Mod 结构。请先安装与游戏版本匹配的 NeoModLoader，然后把压缩包内的模组目录解压到以下任一 NML 模组目录（只选一个）：

```text
<WorldBox>\mods\
<WorldBox>\worldbox_Data\StreamingAssets\mods\
```

解压后的目录可以保留外层文件夹，但 NML 搜索到的模组目录必须直接包含以下文件和目录：

```text
mod.json
icon.png
InterestingTrait.cs
code/
GameResources/
Locales/
```

不要把 ZIP 文件本身放进 `mods`，也不要多套一层目录。若之前安装过旧版，请先删除旧的 `我的模拟长生路` 文件夹，再解压本包；两个文件夹同时存在会触发相同 GUID 冲突，NML 只会加载其中一个。该发布格式不要求 ZIP 内携带 DLL；直接运行原版 WorldBox 时请确认游戏的实验性模组开关已开启。

安装支持 `netstandard2.1` 的 .NET SDK，并准备与游戏版本匹配的 WorldBox 与 NeoModLoader 依赖。外部 DLL 未包含在压缩包中，也未加入仓库。

`references/WorldBoxGameDecompiled/` 是单独导入的 WorldBox 反编译参考树，供搜索 `Actor`、`MapBox`、`SaveManager` 等原版类型、字段和方法使用。它不是模组依赖，已从项目编译项中排除；发布模组时不要把该目录复制到玩家的 mods 目录。

0.2.0 源码型发布包包含入口源码、`code/`、`GameResources/`、`Locales/`、配置、模组元数据和说明文件。`bin/` 与 `obj/` 是本地构建缓存，不属于玩家包。依赖 NeoModLoader 与游戏自带程序集由游戏提供。

## v0.2.0 更新摘要

- 幼年灵根概率与职业概率可设置；职业在十八岁时判定，之后按熟练度和境界晋级。
- 新增三种职业、六类乾坤袋、材料探索、制作、使用与贡献度交易；物品目录收录 38 个 ID。
- 新增独立入道指南、乾坤袋及天玄镜窗口和贴图；修士榜按境界序号排序，玄黄仙录正文显示滚动条。
- 还真唯一宿主的全量核查按年节流；旧版世界档案新增挂单字段并兼容空值。
## v0.1.9.1 更新摘要

- 两个纪元共用深青黑半透明人物信息栏，标题、分隔线和“修士列传”按钮固定，正文按现有数据分组展示。
- 正文使用独立滚动容器并隐藏可见滚动条；鼠标滚轮、触控板和惯性滚动仍然有效，数据刷新不重建节点、不改变滚动位置，关闭后重新打开才回到顶部。

## v0.1.9.2 更新摘要

- 遗府和持久秘境保存有效地块坐标，玄黄仙录可直接定位到对应地点；旧存档没有坐标时只保留文字记录，不猜测位置。
- 新增独立的遗府、秘境、灵气汇聚、地火和星石地图贴图，按持久地点状态与临时事件生命周期显示。
- 读档、换图和时代切换会清理旧地图标记；地图贴图使用独立资源目录，不复用 `ui/Icons`，并采用像素清晰的等比缩放与地图渲染层级。
- 临时灵机灾变到期后自动消失，遗府搜尽、封绝、崩毁或沉寂后同步隐藏标记。

## v0.1.9 更新摘要

- 前世轮盘只生成前世快照中真实拥有的具体对象，保留上限由当前还真锚点数量决定。
- 空间灵蕴无 999 点上限；还真回载使用执行前一刻的外部灵蕴值。自动寻主只在新法纪元启动。
- 玄黄仙录新增独立“修士列传”，人物右侧栏提供列传、猫宝和开发者编辑入口。
- 开发者工具支持点击人物直接选中、修改常用人物数据，并提供“立即进入新法纪元”。
