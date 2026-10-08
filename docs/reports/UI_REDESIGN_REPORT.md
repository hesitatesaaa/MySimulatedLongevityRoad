# 玄黄 UI 重构报告

## 交付范围

已将新生成的东方修仙边框与标题接入交易所、模组介绍、修士榜、还真空间、修士档案、玄黄仙录及修士列传。主标题按整窗中心定位、按可见图像区域等比缩放；按钮、面板与滚动条使用共享的程序纹理和 GUIStyle。交易所内容改为分类栏、宽榜单和下方详情；排行榜扩大中栏并把标题移至整窗顶部；仙录各内容页采用统一卡片与动态栏目标题。

修仙家族页面取消独立艺术字和独立边框，统一继承玄黄仙录主框架。

## 修改与新增文件

- `code/MySimulatedLongevityRoad/UI/XianLuUI.cs`：资源缓存、图集 UV、标题可见区域、共享按钮五态、Nine-Slice 组件纹理、滚动条、居中标题和空状态。
- `code/MySimulatedLongevityRoad/UI/MclslFeatureWindow.cs`：交易所与介绍页新框/标题、排布和统一样式。
- `code/MySimulatedLongevityRoad/UI/MclslCodexWindow.cs`：仙录、还真、列传框图与标题切换；统一栏目卡片、滚动条；摘要低频刷新。
- `code/MySimulatedLongevityRoad/UI/MclslCodexWindow.FamilyArchive.cs`：动态家族页标题、筛选换行、名录与搜索缓存。
- `code/MySimulatedLongevityRoad/UI/MclslRankWindow.cs`：整窗居中标题、三栏比例、空状态、框图接入。
- `code/MySimulatedLongevityRoad/UI/MclslActorInfoPanel.cs`：修士档案标题与竖框接入，姓名居中；沿用原滚动位置管理。
- `code/MySimulatedLongevityRoad/UI/MclslUiTheme.cs`：统一深墨青、玉青与暗金色板。
- `GameResources/ui/XuanhuangSkin/manifest.json`、`docs/玄黄UI皮肤资源说明.md`：更新资源清单。
- 新增 `docs/reports/UI_REDESIGN_ANALYSIS.md` 与本报告。
- 删除旧图片生成脚本 `scripts/generate_xuanhuang_skin.py`、旧风格板脚本 `scripts/design_xuanhuang_v2_moodboard.py` 和旧风格板说明；避免重新生成已废弃的皮肤。删除家族独立标题与边框 PNG。

## 图片资源与接入

所有路径均相对于 Mod 根目录。标题资源仅用于对应主界面；家族页面使用动态文字。

| 界面 | 主边框最终路径 | 标题最终路径 |
| --- | --- | --- |
| 天玄镜·万界交易所 | `GameResources/ui/XuanhuangSkin/frame_tianxuanjing.png` | `GameResources/ui/XuanhuangSkin/title_tianxuanjing.png` |
| 模组介绍 | `GameResources/ui/XuanhuangSkin/frame_mod_intro.png` | `GameResources/ui/XuanhuangSkin/title_mod_intro.png` |
| 玄黄修士榜 | `GameResources/ui/XuanhuangSkin/frame_ranking.png` | `GameResources/ui/XuanhuangSkin/title_ranking.png` |
| 还真空间 | `GameResources/ui/XuanhuangSkin/frame_huanzhen_space.png` | `GameResources/ui/XuanhuangSkin/title_huanzhen_space.png` |
| 修士档案 | `GameResources/ui/XuanhuangSkin/frame_character_panel.png` | `GameResources/ui/XuanhuangSkin/title_character_panel.png` |
| 修士列传 | `GameResources/ui/XuanhuangSkin/frame_biography.png` | `GameResources/ui/XuanhuangSkin/title_biography.png` |
| 玄黄仙录 | `GameResources/ui/XuanhuangSkin/frame_xuanhuang_record.png` | `GameResources/ui/XuanhuangSkin/title_xuanhuang_record.png` |

## 共享框架与数据刷新

`XianLuUIResources` 按资源名缓存 Sprite，并按图集 `textureRect` 计算 UV；标题可见区域会在显示时裁切透明边缘。`XianLuUIStyles` 只在首次使用时生成程序纹理与样式，提供按钮 Normal、Hover、Active、Selected、Disabled 配色以及面板、卡片、滚动条。`XianLuUIRenderer` 使用同一套居中标题与透明框图绘制。Canvas 排行榜、人物栏使用相同资源缓存和色板。

交易所继续在打开时捕获快照、交易修订变化时刷新，并只绘制可见行。修士榜继续使用分步筛选和卡片池。仙录保留分步 Snapshot，界面摘要改为每 0.25 秒刷新；家族排序最多每 2 秒刷新一次，搜索词变化才重建匹配列表。人物栏保持原地更新 ScrollRect 内容；打开期间保留滚动位置，关闭后重开从顶部开始。

## 验证结果

- `scripts/Test-Project.ps1 -Mode Build`：通过，Release 编译 0 警告、0 错误。JSON 与本地化静态检查通过。
- 资源清单：14 张 RGBA PNG 与代码引用逐一对应，边框中心透明；代码不再引用独立家族标题或边框。
- 对 1280×720、1600×900、1920×1080、2560×1440 的窗口尺寸公式完成静态校验，交易所、仙录及固定大小的排行榜均不超出画面。
- 参考 Mod 的 `LotmBrowserWindow.Skin.cs` 已直接阅读；仅借鉴图片缓存、Sprite UV、居中标题和 Nine-Slice 做法。
- 发布包生成后检查其中的图片清单与必需文件。

## 未完成验收与已知问题

- 本环境已编译项目，但没有把新包热重载进运行中的 WorldBox，因此无法实证七个入口的实际点击、悬停、滚动、帧率及 GC 数据。需要在游戏中用新包进行最终目视验收。
- Canvas 人物栏、排行榜的 Tooltip 继续调用 WorldBox 原生 Tooltip；本次统一了触发与面板配色，未替换游戏原生 Tooltip 窗口皮肤。
- 仙录其他大量旧内容页仍保留原有数据分组和操作顺序，统一主框、标题、组件色板与栏目头已覆盖它们。

## 2026-10-03 边框与标题修正

根据实测截图，用内置 imagegen 对天玄镜、模组介绍、玄黄仙录、还真空间、修士榜与修士档案边框做定向编辑。各边框提示词共同要求保留金玉修仙纹饰、让装饰靠近外缘、中心透明，并减少白色雾气。另以准确文字“修士档案”替换人物栏艺术字，重制更明亮的“玄黄仙录”标题。最终图片路径与上表一致。

窗口代码将边框、标题与控件分层：IMGUI 标题最后绘制，修士榜边框处于控件下层；人物栏竖框裁切透明侧边并放在正文下层。调大内容边距，给修士榜侧栏与人物栏底部按钮留出边框安全区。静态检查与 Release 编译通过；尚未把新包载入正在运行的游戏进行目视验收。

人物栏标题随后按原图进行单字修正：仅把第三字调整为左“木”右“当”的“档”，保留原有金玉浮雕字形、透明背景与整体排布。使用内置 imagegen 的定向编辑模式；未采用另一次改变字体风格的生成稿。
