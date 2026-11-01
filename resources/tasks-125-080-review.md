# 0.8.0 任务 1、2、5 处理记录

本轮只处理用户指定的任务 1、2、5，不提交工作区，不启动性能测试，不修改发布策略。真实 Unity 功能结果与最终包指纹见 `0.8.0-delivery.md`。

## 任务 1：部分实现

`MclslCareerSupply.CraftAnnual` 仅缓存失败判定；依赖 Actor 引用、背包对象及 MutationVersion、职业与品阶。六种耗材配方只依赖袋内材料和成品数量，不依赖城市资源、市场、年龄或年份；任一背包写入、增减物品、替换背包或职业/品阶变化都会重新判定。成功不缓存，保持逐年制作到原库存阈值和每批 6 份的行为。空袋不持久化，坏袋仍在存在可用配方时由原 Read 抛错。弱引用缓存由职业 Clear 随世界重置。

主职业 `ProcessAnnual/TryCraft/TryPractice/PlannedRecipe` 未加休眠。原因：grade 0 可使用原版城市资源；练习遍历全部已知配方；需求取决于实时市场库存、生命/状态、境界、真元与技能消耗等。现有事件尚不足以构成完整唤醒契约，单独按袋版本会漏做原本可做的制作。按任务文档的等价要求保留原实现；18 岁判定、选方年份、经验、概率和消耗规则均不改。

## 任务 2：所有权与释放点

| 范围 | 持有者 / 资源 | 释放或保留依据 |
|---|---|---|
| 模组/进程 | LivingResources 的组元数据、图集切片；SpriteTextureLoader 的共享图集 | 游戏资产和 loader 全局目录引用；换世界保留。Init 成功后重复调用直接返回，避免替换并遗失旧切片 |
| 模组/进程 | XuanhuangDirectionPatch.Sources / AtlasSources / FrameData：原图和遮罩 Color32 数组、帧元数据 | 只从固定清单创建；Init 的 `_ready` 防止同程序集重复填充。Clear 不新增源数组；不会按国家/世界增加 Sources |
| 模组/进程 | NativeActorFrames 的 Files/Profiles/Groups、Initials、初始图的元数据别名 | 有限资产注册集合，初始图由游戏资产引用；保留 InitialBytes，不能把总 ResidentBytes=0 当作换世界目标 |
| 模组/进程 | ActorPixelResources 图标、透明占位图，SoulFrameCatalog、WorldSoulActorRegistration 的固定切片 | 固定资产缓存，游戏资产/loader 持有。Native 动画解码不进入图标 Loaded 缓存 |
| 世界 | Xuanhuang 彩色图集及 Sprite、Directions、Preparing/ColorQueue/PendingColors | Runtime.ClearWorldState → XuanhuangDirectionPatch.Clear：先 Unregister 再 Destroy，清空所有颜色工作队列；源像素保留以便重建 |
| 世界 | NativeActorFrames 的流式 Sprite 与各自独享 Texture | Runtime.ClearWorldState → BeastAnimation.Clear → NativeActorFrames.Clear → FrameCache.Clear(Release)：清空驻留、排队、失败状态和字节计数；逐个注销 Sprite 并销毁 Sprite/Texture。预算仍为原 96–384 MB，含初始图预留 64 MB |
| 世界 | BeastTalentFrames 的神通切片、CareerEffectFrames 的职业效果/被动切片 | BeastTalentSystem.Clear / SpellSystem.ClearRuntime 分别释放；前者注销注册表。两者借用 loader 图集，仅销毁自己的 Sprite，不销毁共享 Texture |
| 临时 | BeastDeathVisualSystem 的死亡 GameObject/Renderer | 动画到期、owner 恢复存活、Forget 或世界 Clear 销毁；素材借用动画缓存，无独立 Texture |
| 临时 | RealmHaloVisualSystem 的 Actor 条目、原版效果对象 | 死亡/不可见等进入清理队列，过期或 Clear 调用 kill 归还原版池；仅自行创建的 GameObject 由本系统 Destroy，避免破坏原版池 |
| 临时 | SpellCasting 的待施法/持续伤害任务 | 到期取出；施法者失效后跳过；Clear 提升代次并清空 Pending，世界切换不继续旧任务 |

`MclslSpriteOwnership` 是强引用注册表，不能全局清空（会丢失仍有效的进程级资产注册）。NativeActorFrames.Release 现先按托管引用注销，再检查 Unity 的伪 null，修复已被销毁 Sprite 的注册残留。其他世界级动态 Sprite 的现有 Clear 都逐个注销。

同一已加载程序集的重复初始化已覆盖。不同程序集版本的热卸载/重载没有通用卸载入口，本轮未验证，也不宣称旧程序集资源可自动释放；需要退出游戏后更新模组。

本轮验证对象释放、注册表与软件缓存字节计数；没有测量 Texture.currentTextureMemory、CPU/GPU 实际内存、100/500/1000 单位内存，也没有进行人工交互视觉验收。

## 任务 5：预算内推进修复，压力验收未完成

现有 `20261026_162310/result.performance.txt` 中：职业选方约 890 ms；法脉阶段约 161 ms / 3050 次，平均单次约 0.053 ms。调度器旧逻辑只要阶段名称没变就退出，导致每个已提交目标多等一帧，与工作本身成本不相称。

新增 `AnnualWorldRuntimeLane.TickWithinBudget`，在阶段变化或 ProgressVersion 增长时继续；无进展、WorldBlocked、原帧截止或 4096 操作上限任一成立即退出。Scheduler 仍先完成当年人物队列，仍停驻未来年份。目标由原持久化游标提交，不跳年、不改变目标顺序、次数或收益。没有改 3 ms 共享截止、1.5/2.5 ms 年度预算、原版 AI/战斗/寻路频率或软降速策略。

真实 Unity 中的操作上限/存档游标功能检查使用冻结预算时钟来确定性覆盖边界；它不是吞吐测试。高人口、20 倍速下积压能否持续收敛仍未验证，不能标记任务 5 的整体目标已达成。
