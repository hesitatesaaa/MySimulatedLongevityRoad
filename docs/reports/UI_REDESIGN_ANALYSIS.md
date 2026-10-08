# UI 重构前分析

## 现状与入口

| 界面 | 绘制方式 | 主要入口 | 数据与生命周期 |
| --- | --- | --- | --- |
| 天玄镜、模组介绍 | IMGUI `OnGUI` / `GUI.Window` / `GUILayout` | `MclslFeatureWindow` | 打开时捕获交易快照，交易修订变化在 `Update` 刷新；滚动位置按视图保存 |
| 玄黄仙录、还真空间、列传 | IMGUI `OnGUI` / `GUI.Window` / `GUILayout` | `MclslCodexWindow` 及 partial 文件 | `MclslCodexSnapshot` 分步构建；栏目和滚动位置由窗口实例管理 |
| 玄黄修士榜 | Unity Canvas / `ScrollRect` | `MclslRankWindow` | `MclslRankSnapshotSource` 与增量筛选；可见卡片按滚动范围创建 |
| 右侧人物栏 | Unity Canvas / `ScrollRect` | `MclslActorInfoPanel` | 文本由 `MclslActorInfoFormatter` 准备；打开期间记录滚动位置，关闭时清理状态 |

`XianLuUI.cs` 已包含共享资源缓存、颜色、样式和边框/标题绘制器；`MclslUiTheme.cs` 提供色板。现有资源路径为 `GameResources/ui/XuanhuangSkin/`，发布脚本包含 `GameResources`。上一轮已放入 16 张新边框/标题 PNG，但旧代码仍引用已删除的旧资源名。`XianLuUIStyles.Ensure()` 也会尝试加载已删除的按钮和 Tooltip 图片。

## 主要问题

1. 交易所、仙录和排行榜的标题并非以整窗为基准居中；旧框名导致外框缺失。人物栏同样引用旧标题与旧边框。
2. 交易所顶部统计、分类、榜单；仙录目录与正文；榜单筛选、列表、排序的功能结构保留，但尺寸、层级和间距沿用旧版，视觉上像表格后台。
3. IMGUI 多处滚动条使用 Unity 默认样式。旧面板 PNG 已删，若继续按名加载会记录缺图错误。
4. `MclslCodexWindow.Update` 每帧更新若干计数；`MclslFeatureWindow.DrawMarketPage` 使用已缓存行和交易快照，行宽变化时才重排。需避免在美术接入中引入逐帧世界扫描或图片加载。
5. Canvas 排行榜与人物栏的 ScrollRect 节点应原位更新，不能因刷新反复重建，否则会丢失滚动位置。人物栏已隐藏滚动条，仍保留滚轮行为。

## 参考 Mod 代码核查

已直接读取参考包 `0.51+诡秘之主-宿命之环3.9.zip` 内 `LotmBrowserWindow.Skin.cs`：它把纹理和 GUIStyle 缓存在静态字段中，使用 `RectOffset` 与程序生成的小纹理做按钮/面板；标题以窗口中心计算 Rect，并用 `GUI.DrawTextureWithTexCoords` 绘制；外框在内容绘制后叠加，同时处理 Sprite 在图集里的 UV。借鉴缓存、UV 和居中计算方式，不复制其题材、美术或业务代码。

## 改造边界

- 新主图仅用于七个主界面；“山河万象 · 修仙家族”保留动态文字，继承玄黄仙录主框。
- 共用颜色、组件纹理、按钮五态和滚动条样式放在 `XianLuUI`，各窗口只选择布局和资源名。
- 保留交易、排行榜、还真、列传和家族的查询与操作逻辑；调整显示层、排布和资源接入。
- 用构建与资源路径检查验证代码；本环境无法声称完成 WorldBox 运行时的手工点击、帧率和多分辨率截图验收。
