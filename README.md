# 我的模拟长生路

> 当前模组版本为 **0.1.9**。本版本在 0.1.8 的还真空间、猫宝和玄黄修士榜基础上，补齐前世具体遗产选择、修士列传、人物侧栏入口和开发者工具。

WorldBox 模组，作者：溪上翁、kinght。模组版本 `0.1.9`，`mod.json` 中的目标游戏构建为 `115`。
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
- `references/WorldBoxGameDecompiled/`：WorldBox 原版反编译源码，仅用于接口和行为检索，不参与模组编译或发布。
- `CHANGELOG.md`：按版本记录功能更新和兼容性说明。
- `VERSIONING.md`：版本隔离、发布、回滚和分支规则。
- `AGENTS.md`：项目长期协作、可运行打包和 GitHub 推送边界规则。
- `scripts/Package-Mod.ps1`：Release 构建、运行包生成和包内容校验脚本。
- `docs/UI_DESIGN.md`：排行榜、玄黄仙录与中文命名的界面设计约束。

## 开发与构建

安装支持 `net6.0` 的 .NET SDK，并准备 WorldBox 与 NeoModLoader 的对应依赖。
原项目通过相对路径 `../../worldbox_Data/` 引用游戏和加载器程序集，包括 Unity、Harmony、NeoModLoader 与 `Assembly-CSharp-Publicized.dll`。
这些外部 DLL 未包含在压缩包中，也未加入仓库。构建前应让该相对路径指向匹配的游戏数据目录，或按本机安装位置调整项目中的 `HintPath`。

`references/WorldBoxGameDecompiled/` 是单独导入的 WorldBox 反编译参考树，供搜索 `Actor`、`MapBox`、`SaveManager` 等原版类型、字段和方法使用。它不是模组依赖，已从项目编译项中排除；发布模组时不要把该目录复制到玩家的 mods 目录。

常用检索示例：

```powershell
rg -n "class Actor|updateAge|killHimself" .\references\WorldBoxGameDecompiled
rg -n "class MapBox|updateSimulation" .\references\WorldBoxGameDecompiled
rg -n "class SaveManager" .\references\WorldBoxGameDecompiled
```

在项目目录执行：

```powershell
dotnet build .\InterestingTrait.sln
```

开发工具可直接打开 `InterestingTrait.sln`。完成任何代码、资源、配置或本地化修改后，必须尝试生成可运行包：

```powershell
.\scripts\Package-Mod.ps1 -ChangeTag 空间继承
```

脚本会按现有发布包的源码型结构，将 `InterestingTrait.cs`、解决方案/项目文件、`mod.json`、入口资源、`code`、`GameResources`、`Locales`、配置和说明文档放入 `发布包/`。包名格式为 `0.5.1+我的模拟长生路0.1.9-综合更新.zip`；同名时追加时间戳。该流程不要求本机存在 WorldBox 编译依赖；`references/`、构建缓存、本地 `DeveloperTools/` 和打包脚本不进入玩家发布包。

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

## v0.1.9 更新摘要

- 前世轮盘只生成前世快照中真实拥有的具体对象，保留上限由当前还真锚点数量决定。
- 空间灵蕴无 999 点上限；还真回载使用执行前一刻的外部灵蕴值。自动寻主只在新法纪元启动。
- 玄黄仙录新增独立“修士列传”，人物右侧栏提供列传、猫宝和开发者编辑入口。
- 开发者工具支持点击人物直接选中、修改常用人物数据，并提供“立即进入新法纪元”。
