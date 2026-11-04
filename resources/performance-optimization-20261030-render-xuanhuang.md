# 0.8.1 性能优化（2026-10-30）— 玄黄族逐帧渲染热区与候选热点清单

本文件是本轮「玄黄族逐帧渲染热区」的专项报告，与
[`performance-optimization-20261030.md`](performance-optimization-20261030.md)（经济/年度模块）并列。

> **证据声明（必读）**
> - 本轮**没有在实际 Unity 中运行任何采样**。本文件所有“成本 / 调用频率”都是**静态候选（通读代码与调用点推断）**，不是计时结果。
> - 唯一带数字的「实测」来自**既有历史采样** `MCLS_perf_20261030_131254_899.txt`，它只覆盖**经济/年度角色**模块，**不覆盖逐帧渲染**，也**不是原版 vs 玄黄族的单人对照**。
> - 因此本文件**不声称**任何 FPS / 帧时 / GC / 内存提升，也**不声称**原版与玄黄族“性能一致”。把这些静态结论当作实机回归前的“可验证假设”，而不是实测结论。

---

## 1. 本轮改动（仅缓存/分配，无行为变化）

### 1.1 `Patches/MclslXuanhuangDirectionPatch.cs`

优化目标：减少**逐次绘制**（每个玄黄族单位每一帧、每次取帧）里的路径/复合键字符串与重复查表。

| 项目 | 优化前（逐帧 / 逐次） | 优化后 |
| --- | --- | --- |
| 资源路径 | 每帧拼接 `skin` / `role` / `direction` / `path` 字符串 | `Init` 时按 `(方向 3 × 外观 14)` 预生成 `Variant[3,14]` 表，绘制路径只做数组索引 |
| 帧元数据 | 每帧 `FrameData[path+"/"+frame]` 复合键查字典 | `Variant.Frames[slot]` 直接数组索引（`slot = walk/swim + 0..3`） |
| 颜色键 | 每帧 `ColorKey(color)` 生成 `r-g-b` 字符串 | 打包 `int`：`(r<<16)|(g<<8)|b`，无字符串 |
| 着色帧查找 | 每帧 `ColoredFrames[path+"/"+color+"/"+frame]` 复合键查字典 | 打包 `long` 键 `FrameKey(variantIndex, colorCode, slot)`，一次 `ConcurrentDictionary` 查找 |
| 稳态复用 | 每次都要拼键/查表 | 每 actor 一个打包 `long Signature`；外观/颜色/方向/动画帧全未变时**只比较一次**并复用上一张 Sprite |
| 图集着色 | 每次着色按 `Path+"/"+color` 字符串键 | `AtlasColorKey(atlasIndex, colorCode)`；`ColoredAtlases.ContainsKey(...)` 命中即**复用**，(源图集, 颜色) 变化才重建 |
| 静态 Zombie 键 | 运行期按需拼键 | `Init` 一次性预生成全部 14 外观 × 3 方向（含静态 Zombie 皮肤）的资源键 |

缓存键的失效维度（全部覆盖）：
- **外观**：`SkinIndex = RoleIndex*2 + sex`，角色 `zombie / child / king / leader / warrior / old / 无` 与性别共 14 种；
- **阵营颜色**：打包 RGB `colorCode`；
- **方向**：`facing.Value ∈ {0=侧,1=前,2=后}`；
- **动画**：`walk ↔ swim` 通过 slot 高 3 位（`+4`）区分；
- **帧**：`0..3` 帧号进入 slot 低 3 位。

`VariantByDirSkin` 已逐一核对：资源清单中 14 外观 × 3 方向 = **42 条路径全部存在**（见 §4 校验），因此有效玄黄族角色不会出现 `variant == null`。

### 1.2 `Systems/Visual/MclslXuanhuangBuildingShadow.cs`

修复缓存复用：`Held` 现在记录**全部生成输入**（建筑对象、源精灵、源 Rect、`pixelsPerUnit`、`fundament.width/height/left/bottom`、`scale_base`）。
- 前缀命中且 `previous.Matches(__0,__1)` 为真时：**直接 `addSprite(hash, previous.Sprite)` 返回，不做像素提取/重采样/纹理分配**；
- 输入变化时才 `BuildFootprintShadow` 重建，并 `Release(previous)` 释放旧资源；
- `Release` 同时 `Destroy(Sprite)` + `Destroy(Texture)` + `MclslSpriteOwnership.Unregister`；
- `Sprite.Create` 抛异常时在 `catch` 中销毁已分配的 `Texture2D`，避免半成品纹理泄漏；
- 生命周期锚定原生 `DynamicSpritesAsset.clear()`（`BuildingShadowsCleared` → `Clear()` 释放全部 `Held`）。

> `DynamicSpritesAsset.addSprite` 是 `_dictionary_sprites[hash] = sprite`（幂等覆盖），复用分支重复 `addSprite` 不会抛错也不会重复分配。

---

## 2. 逐帧渲染热区（按调用频率 × 玩法影响 × 证据）

| 热点 | 位置 | 触发频率（静态推断，未计时） | 玩法影响 | 单次成本（静态） | 证据 |
| --- | --- | --- | --- | --- | --- |
| 玄黄族主精灵方向/着色选择 | `Actor.calculateMainSprite` postfix → `AfterSprite` | 每个玄黄族单位**每帧**（动画帧变化时更明显） | 直接影响画面与单位可见性 | 优化前：每帧 4~5 次字符串拼接 + 2 次复合键字典查找；优化后：数组索引 + 1 次 long 比较（未变直接返回） | 静态候选 |
| 取动画帧数据 | `Actor.getAnimationFrameData` postfix → `AfterFrameData` | 每个玄黄族单位取帧时 | 决定动画帧 | 只读 `facing.Frame` | 静态候选 |
| 世界颜色预热 | `MapBox.preloadRenderedSprites` prefix → `PrepareWorldColors` | 加载/进入存档时一次性（每色一次 `PrepareAtlas`） | 影响首次显示延迟 | 每色对每个源图集做一次逐像素着色 | 静态候选 |
| 颜色异步准备 | `TickColors` / `PrepareColor` / `PrepareAtlas` | 主线程每帧预算内，按颜色队列推进 | 决定新颜色多久可用 | 一次克隆 `Color32[]` + 逐像素 + 建纹理 + 建 Sprite；同 (图集,颜色) 只做一次 | 静态候选 |
| 建筑影子生成 | `DynamicSpriteCreator.createBuildingShadow` prefix → `CreateBuildingShadowPrefix` | 玄黄族建筑影子首次生成 / 缓存清空后 | 影响建筑投影外观 | 优化前：每次调用都 `GetPixels32` + 全框扫描 + 重采样 + 新纹理/精灵；优化后：输入不变直接复用 | 静态候选 |

---

## 3. 其他候选热点（本轮**只记录，不修改**）

> 证据分级：**实测** = 有真实采样文件；**静态候选** = 仅通读代码/调用点推断，未计时。
> 下表除 §3.0 的历史采样外，全部为**静态候选**。

### 3.0 历史实测（仅经济/年度，非渲染、非原版对照）

来自既有 `MCLS_perf_20261030_131254_899.txt`（1940 帧 / 人口 3738 / 修士 924，模组主循环 CPU 总 6170.07 ms）：年度角色合计 4611.29 ms（74.7%），数据变化分发 1062.60 ms（17.2%）。详见主报告。**该数据与本轮渲染改动无关，也不能作为原版 vs 玄黄族对照。**

### 3.1 热点清单

| 热点 | 位置 | 调用/触发频率（静态推断） | 玩法影响 | 证据 | 备注（不在本轮修改范围） |
| --- | --- | --- | --- | --- | --- |
| 年度资格判定 | `MclslEligibility.CanCultivate`（L33–41） | 每次 Actor `Observe`/`Resolve`、年度候选扫描每 actor | 高：决定谁能修炼/进候选 | 静态候选 | 已有 `CivilizedAssetCache`/`AssetBrainCache`/`SubspeciesBrainCache` |
| 锁定资产判定 | `MclslEligibility.IsLockedAsset`（L137–145） | 随 `CanCultivate` 每条路径 | 中：过滤船/建筑/亡灵等 | 静态候选 | 每资产 20 个 token 的 `IndexOf` 扫描，按资产不变，可考虑按 `ActorAsset` 缓存 bool（本轮不改） |
| 年度候选评分扫描 | `MclslScheduler.TickAnnualCandidateScan`（L612–634） | 每帧在 `MclslFrameDeadline` 预算内逐 actor 推进 | 高：年度吞吐 | 静态候选 | 已受帧预算限制，不改追赶策略 |
| 化身/猎人候选评分 | `MclslWorldSoulSystem.Manifestation.PickManifestTarget`（L164–177，最多 `ManifestCandidates(120)` 线性评分）；`HunterScore`（L108+） | 化身选择时 | 中：世界之魄行为 | 静态候选 | 线性 bestScore，候选上限已限制 |
| 事件/灾变候选评分 | `MclslWorldSoulActorRegistration`（L997–1005 `MatchScore` 循环）；`MclslWorldCalamitySystem`（L182–194 bestScore） | 事件/灾变触发时 | 中 | 静态候选 | — |
| 逐更新状态/UI（排行榜） | `MclslRankSnapshotSource.TickUpdates`（L74–110）→ `BuildEntry`（L238–317，分配 `RankValues`、多个 string、`MclslRankEntry`）；`UpdateFilters`/`RemoveFilters`（L111–158，`RemoveAll` 线性） | 有 pending 时每 1s 处理一批（`_nextUpdate`），逐 actor | 高：打开榜单/大量变更时 | 静态候选 | 已按 revision/pending 合并，但每 entry 分配仍可观 |
| 逐更新状态/UI（单位信息） | `MclslActorInfoPanel.Refresh`（L39+）经 `MclslPatches`（L405–446）在 stat 变化时触发 | 每次属性变化 | 高：单位面板 | 静态候选 | 已有内容/滚动节流，但刷新频繁 |
| 周期 UI 汇总 | `MclslCodexWindow` 汇总 0.25s（L124–134）；`MclslBeastControlWindow` 0.75s（L98–99）；`MclslCareerSkillView` 0.5s（L81–82）；`MclslCodexWindow.FamilyArchive` 2s（L36–48） | 打开对应窗口时周期性 | 低–中 | 静态候选 | 周期刷新本身是节流手段 |
| 市场列表刷新 | `MclslFeatureWindow`（L264 `_marketRevision != MclslTianxuanMarket.Revision` → `CaptureMarketSnapshot` L1054+）：每次构建 `Snapshot()`/`ListingActivitySnapshot()`/`PurchaseActivitySnapshot()`、按物品分桶、行排序 | 市场页每次 revision 变化（市场操作会多次 `_revision++`，如 `MclslTianxuanMarket` L259/278/294/415/560/617/737/766/1233） | 高：市场挂单/采购界面 | 静态候选 | `ActiveItemCountsSnapshot` 已按 `_revision` 缓存（L76–90）；列表行/分组仍每次重建 |

---

## 4. 静态校验结果（非 Unity 实测）

在本机以脚本对**源码事实**做了可重复的静态校验（不涉及游戏运行）：

1. **键打包无碰撞**：`Signature` 在 外观14 × 方向3 × 帧slot8 × 颜色9 种代表性色值 上**一一对应**；任一维度单独变化都会改变签名。
2. **复合帧键无碰撞**：`FrameKey` 在 variantIndex 0..119 × 颜色 × slot 上唯一；`AtlasColorKey` 在 atlas 0..119 × 颜色上唯一。
3. **热路径无字符串**：`AfterSprite` 方法体内**没有** `+"/"+` 拼接、没有 `ColorKey` 字符串键。
4. **缓存复用存在**：`ColoredAtlases.ContainsKey(atlasKey) → return`；`previous.Matches(...)` 命中即复用且不重建。
5. **销毁/失效路径**：`Clear` 同时 `Unregister+Destroy` 所有着色 Sprite、`Destroy` 所有着色纹理、重置 `Directions`（清 per-actor memo）；`Release` 同时销毁 Sprite 与 Texture 并置空。
6. **资源覆盖**：资源清单中 14 外观 × 3 方向 = 42 条路径全部存在。
7. **编译**：`tools/compile-check.ps1` 通过，279 个源码文件，exit 0，仅 18 条既有警告，无新增警告。

> 这些是“源码/键设计”层面的校验，**不是** Unity 实机尺寸、CPU/GPU 内存或 100/500/1000 单位实测。

---

## 5. Unity Profiler 可复现「原版 / 玄黄族」对照步骤

> 必须使用真实 Unity 运行（Development Build + Profiler）。离线图片审看、替身测试、内存估算**不能**代替此步骤（项目 AGENTS.md 亦如此要求）。以下为操作规范，本轮**未执行**，故不提供实测数字。

1. 取同一存档制作两份副本 A/B，确保人口、单位、建筑、年份一致。
2. A = 基线：关闭玄黄族逐帧渲染补丁（或卸载本模组相关渲染补丁），保持原生渲染；B = 玄黄族：启用 `MclslXuanhuangDirectionPatch` 与 `MclslXuanhuangBuildingShadow`。
3. 两份使用**相同画质、相同分辨率、相同镜头位置与缩放**；分别放置相同数量的玄黄族单位（建议 100 / 500 / 1000 三档）与相同建筑数量。
4. 用 Unity Profiler 采样 ≥60s，重点观察：
   - `PlayerLoop` 下 `Actor.calculateMainSprite`、`Actor.getAnimationFrameData`；
   - `DynamicSpriteCreator.createBuildingShadow`；
   - GC Alloc 列在 `AfterSprite` / `BuildFootprintSprite` 调用栈；
   - 模组自带的 `MclslUnityProfiler` marker（`MCLS/...`，如 `MCLS/Load/WorldEntry`）。
5. 记录 A/B 两组的 §6 指标，填入模板；对同一档位重复 3 次取中位数。
6. 若要验证“缓存复用”，在同一场景内**不改变外观/方向/颜色**地静置 10s，观察 `AfterSprite` GC Alloc 应接近 0、`createBuildingShadow` 在缓存命中后不再重建纹理。

---

## 6. 数据表模板

### 6.1 逐场景汇总（CSV 表头）

```csv
scenario,unit_count,building_count,variant,total_frame_ms_avg,total_frame_ms_p95,total_frame_ms_p99,afterSprite_ms,afterSprite_gc_bytes_per_frame,getAnimationFrameData_ms,createBuildingShadow_ms,prepareColors_ms,gc_kb_per_frame,evidence
A_vanilla_100,100,20,baseline,,,,,,,,,measured
B_xuanhuang_100,100,20,patched,,,,,,,,,measured
A_vanilla_500,500,80,baseline,,,,,,,,,measured
B_xuanhuang_500,500,80,patched,,,,,,,,,measured
A_vanilla_1000,1000,160,baseline,,,,,,,,,measured
B_xuanhuang_1000,1000,160,patched,,,,,,,,,measured
```

### 6.2 单热点对照（模板，待填实测）

| 指标 | 原版/基线 (A) | 玄黄族补丁 (B) | 差值 B−A | 单位 | 证据 |
| --- | ---: | ---: | ---: | --- | --- |
| `Actor.calculateMainSprite` 均 ms |  |  |  | ms | 待实测 |
| `AfterSprite` 每帧 GC Alloc |  |  |  | B | 待实测 |
| `Actor.getAnimationFrameData` 均 ms |  |  |  | ms | 待实测 |
| `createBuildingShadow` 均 ms（首次/命中） |  |  |  | ms | 待实测 |
| `PrepareColors` 总 ms（进入存档） |  |  |  | ms | 待实测 |
| 帧时 P95 / P99 |  |  |  | ms | 待实测 |

> 未填写的格子在报告对外发布时必须保留“待实测 / 未测”，**不得用估算值填充**。

---

## 7. 限制与风险

1. **完全未实测**：本轮未在 Unity 中采样；所有“成本/频率”均为静态推断，不代表实际帧时或内存。
2. **不是原版对照**：本报告不构成原版与玄黄族单人对照；不得据此宣称“性能一致”。
3. **渲染线程可见性**：`ColoredFrames` 仍是 `ConcurrentDictionary`（着色在主线程写入、渲染线程读取）；`Variant` 表在 `Init` 后只读，与既有 `FrameData` 的可见性模型一致。
4. **未改玩法**：未改模拟规则、动画选择、外观、颜色、帧预算或任何配额；仅减少重复字符串/查表与重复资源生成。
5. **建筑影子输入签名**：以“建筑对象 + 源精灵 + Rect + PPU + fundament + scale_base”判定复用；若未来运行期原地修改这些字段而不换对象，需要重新评估（当前代码路径不这样做）。
