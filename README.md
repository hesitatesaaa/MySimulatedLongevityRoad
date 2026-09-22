# 我的模拟长生路

> 当前模组版本为 **0.1.9.2**。本版本为遗府、持久秘境和灵机灾变增加可定位的地图观察标记，并完善读档、换图与时代切换时的贴图清理。

WorldBox 模组，作者：溪上翁、kinght。模组版本 `0.1.9.2`，`mod.json` 中的目标游戏构建为 `115`。
本项目由“0.5.1+我的模拟长生路0.1.4-新法时代优化-最终版.zip”整理而来；压缩包名称中的版本信息与模组元数据分别保留，不推定其兼容关系。

## 目录

- `InterestingTrait.sln` / `InterestingTrait.csproj`：原有解决方案与 C# 项目，目标框架为 `netstandard2.1`，可由 WorldBox 的 Unity/Mono 运行时加载。
- `InterestingTrait.cs`：NeoModLoader 模组入口。
- `code/MySimulatedLongevityRoad/`：核心逻辑、数据、系统、补丁、查询及 UI。
- `GameResources/`：游戏资源。
- `Locales/`：本地化资源。
- `mod.json`：模组元数据。
- `default_config.json`：默认配置。
- `icon.png`：模组图标。
- `docs/IMPORT.md`：导入来源与完整性记录。
- `references/WorldBoxGameDecompiled/`：WorldBox 原版反编译源码，仅用于接口和行为检索，不参与模组编译或发布。
- `CHANGELOG.md`：按版本记录功能更新和兼容性说明。
- `VERSIONING.md`：版本隔离、发布、回滚和分支规则。
- `AGENTS.md`：项目长期协作、可运行打包和 GitHub 推送边界规则。
- `scripts/Build-Mod.ps1`：WorldBox 依赖预检和 Release 构建入口。
- `scripts/Test-Project.ps1`：统一执行静态检查、构建或打包验证。
- `scripts/Package-Mod.ps1`：源码型运行包生成和包内容校验脚本。
- `docs/UI_DESIGN.md`：排行榜、玄黄仙录与中文命名的界面设计约束。

## 开发与构建

项目目标框架为 `netstandard2.1`，用于 WorldBox 的 Unity/Mono 运行环境。项目不会把 WorldBox、Unity、Harmony 或 NeoModLoader DLL 提交到仓库。

默认依赖目录是项目外的 `..\..\worldbox_Data`，也可以通过 `-WorldBoxDataRoot` 指定本机安装位置。构建脚本会先检查全部依赖，缺失时直接列出文件，不继续产生大量无效编译错误。

```powershell
pwsh -NoProfile -File .\scripts\Build-Mod.ps1 -Configuration Release -WorldBoxDataRoot D:\path\to\worldbox_Data
```

日常修改优先使用统一检查入口：

```powershell
# 文档、JSON、本地化或资源修改
pwsh -NoProfile -File .\scripts\Test-Project.ps1 -Mode Static

# C#、项目文件或构建脚本修改
pwsh -NoProfile -File .\scripts\Test-Project.ps1 -Mode Build -WorldBoxDataRoot D:\path\to\worldbox_Data

# 一组修改完成后的最终源码包
pwsh -NoProfile -File .\scripts\Test-Project.ps1 -Mode Package -ChangeTag 项目整理
```

首次构建成功后，依赖和源码未发生还原变化时，可以给 `Build` 模式增加 `-NoRestore`，跳过重复的 NuGet 还原。

如果只需要检查项目文件而没有 WorldBox 依赖，可使用：

```powershell
dotnet msbuild .\InterestingTrait.csproj -getItem:Compile -nologo
```

该检查应确认 `DeveloperTools/` 和 `references/` 不在编译项中。

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

常用检索示例：

```powershell
rg -n "class Actor|updateAge|killHimself" .\references\WorldBoxGameDecompiled
rg -n "class MapBox|updateSimulation" .\references\WorldBoxGameDecompiled
rg -n "class SaveManager" .\references\WorldBoxGameDecompiled
```

开发工具源码只存在于本机被忽略的 `DeveloperTools/`，不会提交 GitHub，也不会进入玩家发布包。当前本机开发版的打开方式是：先点击一个人物选中目标，再按 `F8`；也可以打开“我的模拟长生路”页签并点击“开发者工具”。没有选中人物时不会打开编辑器。

完成一组代码、资源、配置或本地化修改后，先按修改类型完成必要验证，再生成一次源码型运行包：

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\Package-Mod.ps1 -ChangeTag 空间继承
```

若系统只有 Windows PowerShell 5.1，建议安装 PowerShell 7 并使用 `pwsh` 执行脚本。

脚本会按源码型发布结构，将入口源码、解决方案/项目文件、`mod.json`、资源、`code`、`GameResources`、`Locales`、配置和说明文档放入 `发布包/`。包名格式为 `0.5.1+我的模拟长生路0.1.9.2-综合更新.zip`；同名时追加时间戳。`references/`、构建缓存、`DeveloperTools/` 和打包脚本不进入玩家发布包，也不存在开发者工具混入玩家包的选项。

日常可运行包保持 `mod.json` 当前版本不变。只有明确要求正式版本或推到 GitHub 时，才更新版本号、CHANGELOG、正式标签或远程仓库；未收到明确指令时不得执行 `git push`、GitHub Release 或其他远程写操作。

## 版本管理

源码、资源和配置纳入 Git；构建输出、IDE 缓存和本地临时文件忽略。

所有玩家可见的新入口、页面、按钮、特征、物品、状态与说明必须提供中文显示名和中文本地化键，不得把内部英文标识直接显示给玩家。内部 ID、类名和资源路径继续使用稳定的 ASCII 名称，避免破坏旧存档、代码引用和模组兼容性。发布前需检查 `Locales/ch.json` 与 `Locales/cz.json` 均包含新增显示键。

每一个对外版本都必须使用独立且不可移动的 `v主版本.次版本.修订版本` Git 标签固定；发布包必须从该标签对应的提交生成，并保持版本号、更新日志、标签和压缩包名称一致。开发修改继续提交到 `main` 或功能分支，不得覆盖、移动或复用既有版本标签。详细规则及回滚命令见 `VERSIONING.md`。

日常修改与正式发布分开处理：每次修改完成后只生成本地可运行包，不自动递增版本、不创建正式标签、不推送 GitHub。只有用户明确要求“推到 GitHub”或等价操作时，才在构建、包内容校验和 Git 差异检查通过后进行远程操作。

```powershell
git status
git add .
git commit -m "描述本次修改"
```

当前开发机已安装 .NET SDK；完整构建仍要求项目引用路径中的 WorldBox、Unity、Harmony、NeoModLoader 与 Publicized 游戏程序集实际存在。未提供这些 DLL 时，不能把 SDK 安装成功视为模组编译成功。
原包未附带许可证，作者信息保留在 `mod.json` 中；本仓库未额外授予使用或分发许可。

## 还真实现与存储约束

还真默认启用。自动寻主只会在新法纪元正式开始后安排降临时点；仙道纪元仍可通过特质编辑器手动给予还真，手动授予时会收回其他人物身上的还真特质并立即写入还真纪事。

还真使用至多 3 个滚动世界锚点。持有者死亡后只选择同一世界、同一身份且满足安全年差的锚点；短期连续死亡必须退到更早锚点，以避免死循环。回载最多尝试 3 次，失败后停止，不会无限重试。加载后人物索引尚未恢复时会短暂分帧重试，不会立刻误判宿主丢失。

功能页中的“进入还真空间”是独立入口，使用 `GameResources/ui/Icons/HuanZhenEntrance.png`，不会复用玄黄仙录内部页签；玄黄仙录的“还真轮回”页面展示当前持有者和时间锚点，独立“还真纪事”页面集中展示还真降临、手动授予、锚点建立、还真归来及死亡回溯结果。空间灵蕴不会自然增长，只由宿主晋升、杀人夺宝、洞天炼化、天地之变与势力机缘等事件增加，且 v0.1.9 不设 999 点上限；还真回载使用执行还真前一刻的实际灵蕴值。玩家可消耗 80 点灵蕴手动建立锚点，也可在设置中开启默认启用的自动锚定；每枚新锚点都会记录宿主当时的修为、功法、资源、突破造物与天地道果。

成功回到锚点时，死前快照不再整包覆盖锚点人物，而是作为“前世档案”收入还真空间。v0.1.9 只生成快照中真实存在的具体遗产选项，并以当前还真锚点数量作为保留上限；界面显示锚点数、可保留数和已选择数，已占用名额可释放后重新选择。境界特质、还真本身和天地之魄实体标记不会进入可选特征，缺失模组的特征也不会被强行写入。

锚点世界固定写入游戏数据目录的 `MySimulatedLongevityRoad/HuanzhenAnchors` 专用文件夹，创建、校验、回载和清理统一使用绝对路径，避免游戏工作目录变化导致写盘失败。外部状态采用版本化、紧凑 JSON，并通过临时文件覆盖；内容未变化时不写盘。年度维护会清除记录失效的锚点和孤立目录，因此存档文件数量保持有界。轮回历史最多保留 40 条，灵蕴流水只保留最新 2 条。

死前快照除境界、真元和突破材料外，还保存允许列表中的灵根、灵石、仙凡瘴、古法阶段、太上进度及寿元改变量。允许列表防止把任意人物数据注入旧世界；新增修炼字段时，应明确加入 `MclslHuanzhenSystem` 的补充字段列表。

源码目录按 `Core / Data / Modules / Patches / Queries / Systems / Traits / UI` 分层。运行逻辑优先放入现有领域文件，避免为单个常量、薄包装或一次性迁移继续新增碎片文件。GameResources 中相邻重复帧可能是动画停顿帧，不应仅因文件哈希相同而删除。

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
