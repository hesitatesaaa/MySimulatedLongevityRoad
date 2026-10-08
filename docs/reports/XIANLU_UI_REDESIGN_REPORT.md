# 玄黄 UI 重构报告

时间：2026-10-03（Asia/Shanghai）  
目标项目：`D:\模拟长胜路\模拟长生路0.3.5`，`mod.json` 版本保持 0.5.0。

## 范围与依据

- 已阅读目标项目的 `AGENTS.md`、UI 入口与实现、市场/还真/排行的只读数据接口、资源目录和构建脚本。目标项目没有 `PROJECT_STATUS.md`。
- CodeGraph CLI 在当前环境不可用；依据项目规则使用限定在目标项目内的源码检索。
- 保留原有入口、业务算法、存档结构、排行筛选和滚动状态。项目实际仙录按时代提供 14/16 个篇目，因此沿用真实篇目，不伪造六卷内容。

## 修改文件

- `code/MySimulatedLongevityRoad/UI/MclslFeatureWindow.cs`：交易所 PNG 标题与边框、动态今日统计、三栏详情、卖家入口、按修订号刷新与卖家可见行绘制。
- `code/MySimulatedLongevityRoad/UI/MclslCodexWindow.cs`：仙录/还真标题和边框、仙录捷径与篇目导航、还真三栏响应式宽度。
- `code/MySimulatedLongevityRoad/UI/MclslRankWindow.cs`：榜单 PNG 标题与边框、筛选/榜序面板图、空状态与导航文案；手动刷新保留位置。
- `code/MySimulatedLongevityRoad/UI/MclslActorInfoPanel.cs`：暗色底板上叠独立 frame 和 Logo，动态姓名/ID、列传入口提示；沿用原 ScrollRect 状态恢复。
- `code/MySimulatedLongevityRoad/UI/MclslActorInfoFormatter.cs`：修行、技艺、所属分组；无所属数据时隐藏对应空行及整组。
- `Locales/ch.json`、`Locales/cz.json`：新增 UI 文案键，双方键一致。

## 新增文件与架构

- `code/MySimulatedLongevityRoad/UI/XianLuUI.cs`：`XianLuUIResources` 缓存 Sprite；`XianLuUIColors` 集中本次色板；`XianLuUIStyles` 一次初始化窗口、面板、按钮、Tab、滚动条、Tooltip 与文字层级；`XianLuUIRenderer` 绘制独立 Frame 和保持比例的标题 Logo。
- 现有 `MclslUiTheme`、市场快照、仙录快照、排行成员快照与人物栏状态缓存继续承载业务数据，不复制一套业务模型。

## 图片资源与加载

- 使用现有 `GameResources/ui/XuanhuangSkin` 中五张标题 Logo、五张主 Frame、交易/仙录/排行/还真面板图及按钮、Tooltip 图。标题文字和 Frame 分离，动态人名、物品与数值仍由 UI 绘制。
- `SpriteTextureLoader` 加载后按资产名缓存。缺图记录限频 Warning；Frame 保留暗色底板，Logo 显示普通文字标题。人物栏与榜单侧栏使用底板和透明装饰双层，避免透明区域透出地图。
- 检查了 19 张本次直接接入的 PNG：全部存在、可解码，尺寸与清单一致。发布包也包含五张 Logo、五张 Frame、框架源码和双语言文件。

## 布局、滚动与数据

- 市场依据窗口宽度切换分类栏与横向分类 Tab；宽窗口显示右侧珍物详情。当前挂单、上架记录、成交记录均取市场快照，今日统计以记录年份筛选。系统没有市场活跃指标和玩家手动购入接口，因此不显示虚构活跃值或购入按钮。
- 市场主列表沿用二分定位与可见行绘制，详情卖家列表也只绘制可见行。市场修订号改变时更新快照，保留主滚动和分类滚动；卖家按钮只在真实在世 Actor 可解析时显示。
- 仙录正文及还真空间沿用滚动容器。还真三栏按宽度计算，较窄窗口改为纵向排列。排行沿用成员快照、虚拟卡片池、真实职业品阶排序与 ScrollRect；人物栏沿用原位置恢复逻辑。
- 代码计算得到的主窗尺寸：1280×720 时市场 1256×696、仙录 1248×688；1600×900 时 1576×876、1568×868；1920×1080 时 1600×900、1600×1048；2560×1440 时 1600×900、1600×1230。这是布局公式检查，不代替游戏内视觉检查。

## 构建、打包与验收

- `scripts/Test-Project.ps1 -Mode Static` 通过：JSON、ch/cz 键、编译边界、空白、基础资源检查全部通过。
- `scripts/Test-Project.ps1 -Mode Build -NoRestore` Release 构建通过：0 警告、0 错误。默认恢复尝试因沙箱无权读取用户 NuGet.Config 失败；使用项目已有 `obj/project.assets.json` 完成实际编译。
- `scripts/Test-Project.ps1 -Mode Package -ChangeTag 界面重构` 通过。最终源码型包：`D:\模拟长胜路\模拟长生路0.3.5\发布包\我的模拟长生路0.5.0-界面重构.zip`。ZIP 共 1582 项，检查的 13 项 UI 关键文件全部存在。
- 本环境没有正在运行的 WorldBox 游戏会话，无法实际点击五个窗口、查看运行日志、测量游戏内帧率或故障注入缺失 PNG。故“Mod 实际加载、按钮/筛选/滚动在游戏中的交互、四种分辨率的画面、持续 Error 与 FPS”仍需游戏内验收，不能以编译结果替代。

## 已知限制

- 排行沿用游戏原生 600×372 逻辑画布；四种目标分辨率下不越出屏幕，但高 DPI 的视觉大小需游戏内确认。
- 仙录沿用真实 14/16 篇体系，并未把现有业务页面压缩为六个示例卷；右侧捷径只指向项目真实存在的页面。
- 人物栏保留原生 172×360 侧栏位置和完整动态正文；新增分组只隐藏已经核实不存在的所属字段，其他字段仍依现有查询展示。
- 本报告按项目要求留在 `docs/reports`；当前发布脚本只收录 `docs/architecture`，报告不在源码型 Mod ZIP 中。
