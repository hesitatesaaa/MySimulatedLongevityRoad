# 新增生物亚种、蓝色定位标记与头像修复

日期：2026-10-20。基于当前 0.6.0，核对本机 WorldBox 0.51.2 的真实游戏程序集及 NML 引用。没有使用子代理，没有推送代码，没有覆盖已安装的 Mod 或存档。

## 原因与修改

### 蓝色定位标记

原版有不同的绘制入口：

- 鼠标悬停：`UnitSelectionEffect.setVisible(bool)`。
- 已选单位：`QuantumSpriteLibrary.drawSelectedUnits(QuantumSpriteAsset)`。
- 框选候选：`QuantumSpriteLibrary.drawUnitsToBeSelectedBySquareTool(QuantumSpriteAsset)`。

之前只修改了第一处。后两处直接读取 `actor.current_scale.y`，继续把整身大图使用的角色缩放带入地面标记；主选中还保留原版的 1.1 倍强调。因此只修悬停入口无法解决截图中的大范围蓝色图形。

本次在后两处真实 IL 中各替换一次缩放读取：新增生物使用其原版同类模板的基础尺寸，原版生物继续返回自身 `current_scale.y`。原版标记贴图、颜色、动画、坐标和主选中强调不变；没有暂时修改角色数据或全局缩放，生物本体既定尺寸不变。

### 头像

世界画面使用自定义整身图，而原版 UI 仍有独立路径。`UnitAvatarLoader.showStatic` 会调用世界的覆盖贴图，并按贴图原始像素尺寸配置 UI；世界贴图的画布和脚底支点不适合作为头像。亚种旗标另外调用 `DynamicActorSpriteCreatorUI`，按原版身体／头部／肤色资源拼装，未自动使用世界的覆盖贴图。

本次每个资产复用本形态固定首帧的纹理，仅建立一个居中 UI Sprite 和一个缓存帧数组，不复制纹理、不逐帧生成贴图。接入原版 `get_override_avatar_frames`，单独设置界面缩放及偏移；化形使用化形图，原形使用原形图。面板、提示框、亚种旗标、资产图标及通用头像生成入口统一取该 UI Sprite。头像使用整身插画，不再叠加原版拆分式武器图层。调用 `Image.SetNativeSize` 的这些专用头像按 24 像素最大边布局，原版图标不受影响。

### 亚种

原版 `Subspecies.newSpecies` 将 `ActorAsset.id` 写入真实 `SubspeciesData.species_id`，`ActorManager.checkNewSpecies` 按这个 ID 查找同类。此前的归一和读档修复只覆盖妖兽，未覆盖具名人形及其他自定义入口；克隆资产还保留了原版分类命名元数据。亚种页的旗标取图也仍进入不适用的原版拼装流程。截图中 `NAME` 的具体运行时异常原因没有对应日志，不能仅凭截图确认。

本次统一补齐：

- 妖兽原形／化形使用对应妖兽的规范物种 ID；不同妖兽保持不同 ID。具名人物、仙道人物及各天地之魂保留各自资产 ID。
- 新建单位、从数据创建单位及现有的分批读档恢复入口校正真实 `actor.subspecies`，通过原版 `setSubspecies` 更新登记。
- 为新增资产配置自身中文分类名称，精确挂接原版 `generateName(ActorAsset, WorldTile)` 重载；旧自动命名及 `NAME` 占位名可校正，正常自定义名称保留。
- 正确已有亚种不合并。旧化形亚种迁移为规范物种下的独立亚种，复制亚种特质、染色体和出生特质，并保留自定义名称；记录来源并复用迁移结果，避免每个成员重复创建亚种。
- 没有调用原版 `mutateFrom` 完成迁移，因为检查确认该函数会主动增加随机基因／特质突变。使用原版复制接口后刷新基因和属性。

本次不删除旧的空亚种记录，以免破坏存档中的历史引用。

## 验证结果

- 当前真实 NML Roslyn 4.9 编译器、真实游戏引用：249 个生产源文件，0 个错误。
- 对编译后的生产补丁执行真实游戏 IL 检查：已选中入口 80 条指令、框选入口 69 条指令，分别替换 1 处，分支标签保留；不匹配的入口明确失败。
- 执行生产代码的规范物种映射：62 个形态全部符合预期；额外检查原版人／兽人／羊、具名人形和天地之魂的独立 ID。共 85 项断言通过，无 Mock／Stub。
- 资源打包校验：62 组原形／人形资源、1,374 张动画源图、38 个放置图标，以及现有法术／图标验证通过。
- 玩家包沿用既定 PNG 压缩方案，约 56.7 MiB；角色画布、锚点、PPU 和工作原图未改变。压缩包内文件有逐项哈希校验。

复现入口：`.dev-tools/ui-lineage-check`、`.dev-tools/nml-compiler-check`、`tools/package-release.py`。真实 IL 测试在 .NET 中读取原版指令并执行生产转换函数，不等于在 Unity 中实际挂载 Harmony 补丁。

## 交付与未验证项

压缩包：`build/Ui-Lineage-Fix-Compressed-Player-20261020/MySimulatedLongevityRoad-0.6.0-player-local-compressed.zip`。

证据：同目录 `diagnostics/`；SHA256 及逐文件记录见 `SHA256SUMS.txt`、`package-record.json`、`player-file-manifest.json`。

安装时替换原 Mod 的完整目录并重新启动加载，避免混用旧文件。本次没有新的游戏内操作及录屏证据；新世界／旧存档亚种统计、各个 UI 头像位置、形态切换、蓝色标记实际尺寸和与其他模组的 Harmony 共存情况均须游戏内复核。没有将编译、IL 或资源检查报告为游戏内测试通过。
