# 战斗之神相关改动撤销报告

日期：2026-10-20  
基线：`85602685832011ea4998e48a0807ab98b291de9f`（此前开发包记录的提交）  
当前分支：`0.6.0`，本次仅在工作区做定点回退，没有提交或推送。

## 撤销范围

逐文件对照基线和当前提交 `b7b4ec73`。新增职业与斗法代码、相关图标、获取入口和施法改写均来自本次对话；因此删除这些新增内容，并将只包含本次斗法差异的既有文件恢复到基线。混有其他项目改动的文件只移除斗法对应行。

移除四职业、16门 `C001`—`C016` 技能、战斗领悟、职业切换和灵契投影；移除新施法阶段、情境选招、连招、控制限制、投影伤害归属及斗法地形破坏。恢复原有26门法术的注册、学习、熟练度、选招、施法、伤害、特效和地形处理逻辑。

以下既有文件的战斗相关差异已恢复：

- `Data/MclslItemCatalog.cs`、`Modules/MclslBeastModule.cs`、`Patches/MclslCombatPatches.cs`
- `Systems/Adventure/MclslAdventureSystem.cs`、`Systems/Adventure/MclslFactionMissionSystem.cs`
- `Systems/Cultivation/MclslSpellCasting.cs`、`MclslSpellProgression.cs`、`MclslSpellSystem.cs`
- `Systems/MclslTraitGrantRouter.cs`、`Traits/MclslTraitRegistration.cs`
- `UI/MclslBackpackTabSystem.cs`、`MclslFeatureWindow.cs`、`MclslSpellCodexWindow.cs`

在 `Systems/Beasts/MclslBeastConfig.cs` 仅移除了斗法地形破坏选项及读取逻辑；保留同一文件中与本次斗法无关的兽类概率配置整理。README 仅删除四职斗法预览段落，保留当前项目说明、动画资源说明和本地开发说明。

删除六个新增战斗源码文件、四张职业特质图标、图标生成与战斗验证脚本、战斗专用离线测试目录、斗法设计／验证／来源说明，以及本次对话产生的开发包、编译目录、预览图和调试日志。

## 保留内容与来源判断

`Systems/Combat/MclslRealmSuppressionSystem.cs` 是基线已存在的境界压制实现，完整保留。原有妖兽、法宝、生产职业、26门法术、48份法术阶段资源及本分支的其他代码、资源、开发工具和文档均保留。

未发现来源不明的斗法实现。法术系统、施法队列、UI、模块和配置文件都按基线差异逐项审阅；配置及README中其他不相关变更没有回退。代码和资源范围内的斗法唯一标识与引用已清除。本报告保留名称仅用于说明撤销内容。

## 验证与限制

- 使用项目实际游戏与 NeoModLoader 程序集完成本地 Release 编译，成功。
- 运行现有生产目录／配方回归程序，692项断言通过；它直接编译并检查生产目录与配方源码，没有替身代码。
- 本地资源校验通过：62项表单、1,374帧、38个放置图标、18份保留修复帧；111个物品图标、48组法术阶段图均无缺失或无效项。
- 未进行 Unity 启动、游戏内人物动作／攻击／特效观察，也未进行100／500／1000单位性能与内存实测。因此编译和离线回归通过不代表游戏内验收通过。

## 交付

恢复版 Mod 以本地玩家包交付：`build/Restored-CombatGods-Package-20261020/MySimulatedLongevityRoad-0.6.0-player-local-compressed.zip`。包内采用项目既有的 PNG 调色板压缩流程；压缩仅作用于包内副本，源文件未改写。SHA-256：`235e35acaaf76bb18db01b52c5d9e4d885a6911ea4d9c2c2092993a1067cb0d8`。压缩包未经 Unity 实测。
