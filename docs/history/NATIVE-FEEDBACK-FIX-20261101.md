# 模组生物原版反馈动画修复

## 改动范围

新增内部 `Patches/MclslNativeFeedbackPatches.cs`，由现有 `MclslPatches.ApplySafely` 注册六个精确签名补丁。复用 `MclslActorPresentation.UsesVanillaMarker` 的生物身份识别和 0.1 原版成年反馈基准。

| 原版方法 | 局部替换 | 匹配数量 |
| --- | --- | --- |
| Actor.spawnSlash | 特效尺寸 actor_scale | 1 |
| Actor.spawnSlashKick | 起始偏移中的 actor_scale | 1 |
| Actor.getSlashPosition | 特效位置中的 getScaleMod | 1 |
| Actor.doCastAnimation | 两个特效尺寸、顶部偏移尺寸 | 2 + 1 |
| EffectsLibrary.showMetaEventEffect(string, Actor) | 心情变化、升级等事件尺寸 | 1 |
| EffectsLibrary.showMetaEventEffectConversion(string, Actor) | 转化事件尺寸 | 1 |

模组生物的反馈 scale 为 0.1，scaleMod 为 1；原版角色分别读取原有 actor_scale、getScaleMod、stats["scale"]、current_scale.y，保留四种输入间可能存在的差异。原版巨人等角色不会被统一缩小。

保留原方法的可见性与设置判断、效果 ID、随机偏移、方向、getHeight、目标高度、特效调用次数，以及分支和异常元数据。没有全局拦截 effect spawner 或尺度 getter，没有写入角色尺寸、属性、伤害、范围、冷却或存档。已有选择框、头像、状态图标补丁保持独立，没有重叠挂载上述六个方法。

每个补丁在安装时检查精确替换数量，不符合则抛出异常，由现有补丁保护器记录为关键补丁失败。热路径只有身份判断与数值读取，不增加逐帧扫描、日志、反射、列表或委托分配。反射和 IL 列表仅在补丁安装时使用。

## 离线验证

- 对当前游戏 Assembly-CSharp.dll 的真实 IL 调用生产 transpiler：83 项检查通过，包括缺失、重复、二次规范化拒绝、原版精确透传、模组归一化、角色状态不变、原始特效调用及分支/异常元数据保留。
- 游戏程序集 SHA256：`51D275F0168BE2F6CA26341AB292406714E694E0270EAFCB25B999D5DF6DD69F`；MVID：`3aa7d3da-1573-446f-8db1-3cd80acc5c24`。
- 100,000 次四个数值辅助方法组合读取新增分配为 0 字节（CoreCLR，身份识别与游戏接口使用替身；不代表完整 Unity 路径测量）。
- 全量编译：284 个源码文件，成功；16 条既有 CS0649 警告。
- 全部五个正式测试项目通过；新增测试位于模组外 tests/NativeFeedbackTests，并纳入 tests/projects.json。
- 源码选择检查通过；本地化检查 1,329 条目、无错误；Git 空白检查通过。
- 玩家包 283 个源码文件逐字节匹配工作区（省略既有开发编辑器）；未混入 tests、csproj、dll、pdb。解包后源码选择检查及重新编译通过。

## 玩家包与验收边界

包：`MySimulatedLongevityRoad/build/native-feedback/player-final/MySimulatedLongevityRoad-0.8.3-player-local-compressed.zip`

大小：75,859,986 字节；SHA256：`cfaafda57b3f0df378e91734a8f29d31a2445325ebdaac949a3e2871ea75738b`。

使用既有 player + compress-assets 的本地开发打包路径。既有素材来源/哈希校验问题未在本轮处理，打包明确使用 --local-development --skip-validators；不视为完整发布验收通过。

没有替换运行中游戏的模组目录，没有尝试热重载。安装新包并重启游戏后，仍须检查实际 Harmony 挂载、白虎及大型生物挥击锚点、踢击、心情正负、升级、地面和顶部施法效果，以及原版人类/巨人对照。保留实际高度不代表大型生物画面锚点已确认合适，需要游戏画面验证后决定是否微调视觉位置。

未取得新代码的 Unity 截图或帧耗时、GC、内存对照数据，因此当前结论为：**离线验证通过，游戏内视觉与性能验收待完成**。仅支持当前格式新存档的既有交付原则保持不变。
