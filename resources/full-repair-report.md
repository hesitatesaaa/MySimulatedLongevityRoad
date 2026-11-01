# 全面修复交付报告（2026-10-25）

本轮在当前工作区的0.7.0成果上增量修复；没有降回0.6.0，没有提交或推送GitHub。篝火、仓储广场按后续明确要求保留原版。

## 功能状态与证据

|需求章节|实施结果|验证范围|
|---|---|---|
|一 执行原则|保留既有代码成果和原始资源；新增资产有原件备份|工作区差异、压缩包清单|
|二 图片跨格|建筑隔离矩形、固定Pivot；30张职业图集的720帧加入透明边距，原有效区域逐像素一致|3085张PNG解码；694映射帧+720效果帧无几何错误/贴边|
|三 建筑重构|原19项重绘，新增4项独立公共建筑；2项按要求共用原版|Unity实际放置25项，独立路径、升级链、存读档通过|
|四 建筑动画|风车主体固定，4帧扇叶围绕同轴每帧22.5度前进；原生播放速度4；港口主体稳定|帧几何审计、Unity原生城市归属下两级风车均播放全部4帧，并采集原生相机画面；连续观感可继续人工审看|
|五 内存释放|每30秒最多清理16张闲置超过60秒的流式帧；按钮最多64张；活跃和初始帧保留，不强制GC|冷/热缓存生命周期断言通过；Unity按钮执行和建筑资源保留检查通过；未实测CPU/GPU释放量|
|六 脑后光圈|在境界光圈资格入口排除新增生物，保留选择、交互、战斗反馈|源码路径确认；交互式全种类审看待完成|
|七 两位人物全法术|按身份资产/特性动态读取完整普通法术注册表，突破普通学习/施法境界限制|两位人物通过玩家放置、全部26种法术掌握、灵力初始化和实际施法检查|
|八 玩家时代按钮|指定按钮移至普通栏目；其他开发工具仍受原权限门控；复用原时代入口，重复跳转不重置起点|Unity时代重复调用幂等检查通过；最终压缩玩家包实际点击与权限隔离通过|
|九 中文UI|底部按钮容器由30增至36，正文底界同步提高；避免按钮文字被换行裁切；保留既有UI修复；修复本地化同键更新缓存和栏目自身导航按钮引用|1326项本地化、264份源码检查通过；完整分辨率/长文本交互式验收待完成|
|十 兽潮双历史/公告|修复事件重要度两层判断；采用兽潮ID去重，记录坐标、等级、候选妖兽、生成计划和结束结果；6级及以上公告|Unity 5/6/7级公告边界、原生历史镜像及重复触发检查通过；完整战局结束过程待交互验证|
|十一 地图跟随|取当前人物窗口实体；先同步完成窗口关闭，再调用原生followUnit，避免延迟窗口清理取消跟随|Unity准确绑定及跨帧持续跟随通过；保留原版单位原路径|
|十二 生成标识/卡顿|世界反馈图标使用独立PPU且共享原贴图；避免复用PPU=1的大尺寸UI头像；相同目标境界不重复完整晋级初始化|Unity图标边界检查；24次实际createNewUnit调用计时见下文|
|十三 动作标识|原版任务、心情、状态、旗帜及选中标记的缩放与模组本体缩放解耦；非模组对象返回原值|真实Unity Harmony补丁加载通过；所有动作/体型视觉比例待交互审看|
|十四 妖兽普通法术限制|学习/玩家授予/研究/已知列表/年度学习/自用与战斗施放/排队入口均拦截；旧数据迁移清除普通学习记录|24种普通/具名妖兽的26法术授予拒绝及普通施法队列拒绝通过|
|十五 本命神通|独立单值ID和等级保存；晋级钩子同步；专用QueueTalent通道并保留持续伤害身份，不进入普通已学列表|自然/兽潮入口、晋级到元婴、保存重载26个神通状态通过；各神通完整战斗动画待交互验证|
|十六 自然概率|统一自然入口倍率由0.4改为0.004，即原来的1/100；兽潮、开发者和玩家主动放置不套用此倍率|7项生产规则断言通过；未声称等待数千游戏年实测概率|
|十七 Bug检查|修复实测发现的跟随关闭时序、身份识别，以及双历史重要度问题；保留失败验收记录|最后专项Unity440项检查、玩家包17项检查，均0错误|
|十八 性能原则|共享贴图/Sprite配置，复用原生动画，无新增逐帧资源扫描或强制GC；降低重复晋级初始化|有实际生成调用计时，未声明FPS提升或消除所有场景卡顿|
|十九 必要验证|生产编译、本地化/资源、动画缓存、加载、死亡姿态、概率及专项Unity|详细边界见下文；静态与替身测试不记为实机通过|
|二十 构建压缩|按player + compress-assets流程；同步压缩开发者包，排除.dev-tools测试和缓存|最终路径/大小见发布清单|
|二十一 交付报告|本报告、建筑JSON清单、资源审计JSON、Unity原始结果、发布校验清单|随最终交付保留|
|二十二 执行记录|resources/full-repair-progress.md记录实际阶段与原件位置|可按文件状态接续|

## 建筑清单

原可建造/升级定义25项；原独立19项；原共享6项。本轮重绘19项，补齐4项，解除共享4项；剩余2项为用户明确许可。住宅6级、主厅3级、风车2级均有对应资源；全部继承原版运行机制。原版人类建筑定义没有被改写。

|原定义|本轮外观映射|处理|
|---|---|---|
|tent_human|`buildings/civ_main/mclsl_xuanhuang/tent_human`|重绘|
|house_human_0|`buildings/civ_main/mclsl_xuanhuang/house_human_0`|重绘|
|house_human_1|`buildings/civ_main/mclsl_xuanhuang/house_human_1`|重绘|
|house_human_2|`buildings/civ_main/mclsl_xuanhuang/house_human_2`|重绘|
|house_human_3|`buildings/civ_main/mclsl_xuanhuang/house_human_3`|重绘|
|house_human_4|`buildings/civ_main/mclsl_xuanhuang/house_human_4`|重绘|
|house_human_5|`buildings/civ_main/mclsl_xuanhuang/house_human_5`|重绘|
|hall_human_0|`buildings/civ_main/mclsl_xuanhuang/hall_human_0`|重绘|
|hall_human_1|`buildings/civ_main/mclsl_xuanhuang/hall_human_1`|重绘|
|hall_human_2|`buildings/civ_main/mclsl_xuanhuang/hall_human_2`|重绘|
|library_human|`buildings/civ_main/mclsl_xuanhuang/library_human`|重绘|
|market_human|`buildings/civ_main/mclsl_xuanhuang/market_human`|重绘|
|barracks_human|`buildings/civ_main/mclsl_xuanhuang/barracks_human`|重绘|
|windmill_human_0|`buildings/civ_main/mclsl_xuanhuang/windmill_human_0`|重绘|
|windmill_human_1|`buildings/civ_main/mclsl_xuanhuang/windmill_human_1`|重绘|
|docks_human|`buildings/civ_main/mclsl_xuanhuang/docks_human`|重绘|
|fishing_docks_human|`buildings/civ_main/mclsl_xuanhuang/fishing_docks_human`|重绘|
|watch_tower_human|`buildings/civ_main/mclsl_xuanhuang/watch_tower_human`|重绘|
|temple_human|`buildings/civ_main/mclsl_xuanhuang/temple_human`|重绘|
|bonfire|`buildings/civ_general/bonfire`|按后续要求使用原版|
|statue|`buildings/civ_main/mclsl_xuanhuang/statue`|新增专属，解除共享|
|well|`buildings/civ_main/mclsl_xuanhuang/well`|新增专属，解除共享|
|stockpile|`buildings/civ_general/stockpile`|按后续要求使用原版|
|mine|`buildings/civ_main/mclsl_xuanhuang/mine`|新增专属，解除共享|
|training_dummy|`buildings/civ_main/mclsl_xuanhuang/training_dummy`|新增专属，解除共享|

升级与前置关系由原生人类完整构建订单、共享订单及升级图闭包建立，目标映射只写入克隆资产。Unity实际放置25项，均得到有效Sprite，25项重载后定义ID与建筑ID保持一致。施工、完整、缩略、废墟帧采用原生帧名。

## 图片、动画与原件

- 建筑原图备份：`.dev-tools/combat-art/xuanhuang-before-front/`。
- 新原画：`.dev-tools/combat-art/xuanhuang-front-20261025.png`、`xuanhuang-construction-20261025.png`、`xuanhuang-ruins-20261025.png`，使用内置imagegen生成。提示词要求为5×5玄黄文明正向建筑、青玉瓦/米白石/赭木/金纹、可见屋顶和侧墙阴影、透明背景；另分别生成施工和废墟状态。未旋转旧斜向建筑冒充重绘。
- 生成图集实际间隔不是规则网格，导入采用逐建筑审看的矩形；修复了候选导入中的邻列立柱及瞭望塔屋顶切入问题。候选不是发布资源。
- 建筑基线19个缩略帧贴边；职业效果基线719/720帧贴边。贴边是风险标记，不等价于全部存在可见串格。最终所有受查帧均有透明隔离，矩形无越界或重叠。
- 职业效果原图在`.dev-tools/combat-art/career-effects-before-padding/`；720帧的有效区域逐像素验证一致，未裁掉有效像素。PPU补偿保持原可见尺寸。
- 未重新绘制职业特效帧内部已有图案；新增留白解决帧边界隔离，无法凭离线几何检查证明原画内部没有其他视觉瑕疵。
- 帧检查数据：`resources/full-repair-frame-audit.json`；效果像素比对：`resources/career-effect-padding-report.json`。

## 性能与内存的真实结果

既有共享动画/首帧加载机制继续保留；新反馈图标共享已加载Texture，只建立独立Sprite/PPU，未增加重复图片解码。自然与兽潮生成若目标境界已一致，不再再次调用完整SetRealm。普通妖兽不再解析普通已学法术列表。风车沿用原生建筑播放器，没有新增逐建筑Update。

流式原生帧缓存清理按最后请求时间和既有生命周期规则运行，保留初始等待姿态、活跃帧与长期有效元数据。Unity的Destroy延迟执行，按钮只报告缓存账面数量和RGBA字节变化，不冒充CPU/GPU实际释放读数。无Resources.UnloadUnusedAssets同步卸载，也无强制完整GC。

专项Unity运行中24次原生单位创建调用（含生产创建后置初始化）的范围为0.5757–15.2292 ms，中位数0.8585 ms。该样本不是FPS、整帧耗时或性能提升对照；没有做100/500/1000单位负载测试，也不能据此保证所有场景肉眼无卡顿。

## 验证记录

- 最终压缩玩家包：`build/runtime-070/20261025_064447/result.json`，副本`resources/full-repair-player-runtime.json`；17项检查，0错误。实际点击跳转和清理按钮，确认其他开发按钮及编辑器不存在；两级风车均由原生播放器播放4帧。
- Unity相机采集：同目录的`player-character-ui.png`及两级风车各2张PNG。已审看该1280×720样本，底部两个按钮完整可见，风车主体稳定；不是全分辨率、全生物视觉验收。
- 最后专项Unity：`build/runtime-070/20261025_064601/result.json`；副本`resources/full-repair-runtime.json`。实际放置25项建筑和28个单位，440项检查，错误0；包含25建筑存读档和26个妖兽神通存读档。
- 自然/兽潮生成入口采用真实Unity单位创建，公告边界调用真实Trigger；测试为隔离验收世界，包含准备状态的测试操作，不是完整长期自然演进记录。
- 本轮专项输入SHA256：`5c2e511b3b0d75d32996b608810229ef0e4145dede6a5107f60b899882e1a832`。
- 动画与缓存：599项离线断言；离线加载17项、死亡视觉804项。这些测试包含Unity替身，均不记为游戏内视觉/内存验收。
- 自然概率及兽潮计数策略：7项纯生产规则断言。
- 最终生产编译和压缩包验证结果见`resources/full-repair-release.json`。
- 旧beast-tests控制台工程依赖已过时，单独运行无法编译；未将它记为通过，本轮使用专门的生产概率规则工程与上述实际Unity测试。

## 仍需人工实机确认的范围

交互式正向建筑风格、风车连续观感、全体生物脑后光圈/标识在不同缩放下的比例、全部中文长文本与分辨率、每种本命神通的完整战斗表现、完整兽潮结束结果、长期内存与大规模CPU/GPU/FPS表现，均不能由此次离线检查或短期Unity脚本替代。现有0.7.0全量发布门槛（包括三独立世界自然建国等）没有冒充重测通过，压缩包保留local-development验收标记。

## 最终发布

最终压缩玩家包、压缩开发者包、大小和SHA256以交付目录外部的`package-record.json`、`SHA256SUMS.txt`为准，工作区另存`resources/full-repair-release.json`。发布体积与ZIP哈希在打包后写入外部清单，避免把压缩包自身哈希循环写入包内。默认安装包只包含运行源码和必要资源；移出旧模组目录后，将压缩包中的`MySimulatedLongevityRoad`目录放入游戏`mods`。避免覆盖残留的旧测试源码。

### 本次发布文件（包外交付清单）

- 压缩玩家包：`D:\模拟长胜路\upstream-0.6.0\build\full-repair-release\MySimulatedLongevityRoad-0.7.0-player-local-compressed.zip`；71918679字节（68.59 MiB），SHA256 `d27616e8e2ed76c13072f9c1eb4335babd3285a2239841655449d76fe01e4244`。
- 压缩开发者包：`D:\模拟长胜路\upstream-0.6.0\build\full-repair-release\MySimulatedLongevityRoad-0.7.0-developer-local-compressed.zip`；78247243字节（74.62 MiB），SHA256 `7cfa5774ecd6d3b0eb13feec2ecbbe9d048c4a81ed2ffbcb0a454ae283c3157e`。

最终编译成功，265份生产源码；玩家包包含264份源码并排除开发者编辑器。两个ZIP均核对根目录、关键源码、23项独立建筑、全部映射矩形、开发者编辑器边界和测试缓存排除。最终输入指纹与两次实际Unity验收一致。

本轮没有剩余已确认的编译或专项脚本失败；“仍需人工实机确认”列出的范围保持未验收状态，不能据此宣称所有场景无Bug或长期性能达标。
