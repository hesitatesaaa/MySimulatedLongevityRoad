# 施工方案：本命神通特效 / 原版过大标识 / 兽潮计数（2026-11-01）

> 本文为**施工方案**，代码尚未改动。
> 依据：源码静态审查 + 对 `Assembly-CSharp-Publicized.dll` 的 IL 元数据扫描。

---

## 0. 扫描结论（问题 2 的事实基础）

用 `System.Reflection.Metadata` 扫描游戏程序集，枚举出**所有**读写
`BaseSimObject.current_scale` 字段或调用 `Actor.getScaleMod()` 的方法，共 46 处。
与"单位身上的标识/特效"相关的如下：

| 方法 | 使用的成员 | 当前是否已归一化 |
|---|---|---|
| `QuantumSpriteLibrary::drawSelectedUnits` | current_scale | ✅ 已归一化 |
| `QuantumSpriteLibrary::drawUnitsToBeSelectedBySquareTool` | current_scale | ✅ |
| `QuantumSpriteLibrary::drawUnitTaskIcons` | current_scale | ✅ |
| `QuantumSpriteLibrary::drawUnitHappinessIcons` | current_scale | ✅ |
| `QuantumSpriteLibrary::drawUnitBanners` | current_scale | ✅ |
| `QuantumSpriteLibrary::drawFavoritesGame` | current_scale | ✅ |
| `QuantumSpriteLibrary::drawUnitMetas` | current_scale | ✅ |
| `QuantumSpriteLibrary::drawMagnetUnits` | current_scale | ✅ |
| `QuantumSpriteLibrary::drawSocialize` | current_scale | ✅ |
| `QuantumSpriteLibrary::drawUnexploredAugmentationSprite` | current_scale | ✅ |
| `QuantumSpriteLibrary::drawStatusEffectFor` | current_scale + getScaleMod | ✅ |
| `UnitSelectionEffect::setVisible` | current_scale | ✅（postfix 单独处理） |
| **`QuantumSpriteLibrary::drawUnitsEffectDamage`** | current_scale | ❌ **走体型缩放** |
| **`QuantumSpriteLibrary::drawUnitsEffectHighlight`** | current_scale | ❌ **走体型缩放** |
| **`BaseEffect::updateAttached`** | current_scale | ❌ **未处理** |
| **`Actor::spawnParticle`** | current_scale | ❌ **未处理** |
| **`Actor::doCastAnimation`** | current_scale | ❌ **未处理** |
| **`ActionLibrary::flamingWeapon`** | current_scale | ❌ **未处理** |
| **`CombatActionLibrary::attackRangeAction`** | getScaleMod | ❌ **未处理**（攻击范围标识） |
| `Actor::getSlashPosition` | getScaleMod | ❌ 未处理（位置偏移） |
| `Actor::getHeadOffsetPositionForFunRendering` | current_scale | ❌ 位置偏移 |
| `Actor::getThrowStartPosition` | current_scale | ❌ 位置偏移 |
| `QuantumSpriteLibrary::drawUnitsAvatars` | current_scale | ⚠️ 走体型缩放（**这是生物本体，不应缩小**） |

> 与单位无关的建筑/图表类（`Building*`、`GraphTimeScaleContainer`、`ButtonGraphScale`、
> `TrailerMonolith`、`Actor::setActorScale`）**一律不动**。

**关键判断**：`getScaleMod()` 全程序集只有 4 个调用点
（`attackRangeAction`、`drawStatusEffectFor`、`getSlashPosition`、一个初始化 lambda），
**没有任何绘制生物本体的地方用它**。所以它可以做全局 postfix，一次性收敛所有
"偏移类"标识，风险低、收益大。

---

## 1. 问题 1：本命神通特效 —— 改成与法术完全一致

### 目标
本命神通（`BT_` 前缀）的尺寸规则与普通法术**共用同一段实现**：
详细视图 = 正常大小；缩略世界 = 随缩放缩小（与法术相同的 `ZoomFactor`）。

### 现状差异（3 处特例）

| 位置 | 普通法术 | 本命神通 |
|---|---|---|
| `MclslBeastTalentFrames.cs:23` | Sprite PPU = 帧尺寸（→ 世界尺寸 1.0） | PPU=160，192px → 世界尺寸 **1.2** |
| `MclslSpellSystem.cs:927` | `zoom = ZoomFactor(camera)` | **`zoom = 1f`（不随缩放）** |
| `MclslSpellSystem.cs:828` | ×1.0 | **×0.5** |

净效果：本命神通约等于法术的 60%，且缩略世界时不缩小。

### 施工步骤（3 处改动）

**改动 1** —— `Systems/Visual/MclslBeastTalentFrames.cs:23`

```csharp
// 之前：PPU 160
frames[i]=Sprite.Create(texture,new Rect(i%4*192,(5-i/4)*192,192,192),new Vector2(0.5f,0.5f),160);
// 之后：PPU 用帧尺寸 192，与普通法术 Sprite.Create(..., Size) 的规则一致 => 世界尺寸 1.0
frames[i]=Sprite.Create(texture,new Rect(i%4*192,(5-i/4)*192,192,192),new Vector2(0.5f,0.5f),192);
```

**改动 2** —— `Systems/Cultivation/MclslSpellSystem.cs:927`

```csharp
// 之前：BT_ 强制不缩放
float zoom = spellId.StartsWith("BT_", StringComparison.Ordinal) ? 1f : ZoomFactor(camera);
// 之后：与所有法术一致
float zoom = ZoomFactor(camera);
```

**改动 3** —— `Systems/Cultivation/MclslSpellSystem.cs:828`

```csharp
// 之前：BT_ 再乘 0.5
float scale=ScreenScale(_camera,_spellId)*(_spellId.StartsWith("BT_",StringComparison.Ordinal)?0.5f:1f);
// 之后：与所有法术一致
float scale=ScreenScale(_camera,_spellId);
```

### 预期结果

| | 详细视图 | 缩略世界 |
|---|---|---|
| 普通法术 | `ScreenScale × 1.0` | × `ZoomFactor`（最小 1/3） |
| 本命神通（改后） | `ScreenScale × 1.0`（与法术相同） | 同法术 |

> 注：代码里 `ZoomOverviewFactor = 1f / 3f`（`MclslSpellSystem.cs:902`），
> 完全拉到最远时约为 1/3。你描述"缩略世界显示 0.5"应是中间缩放档位的观感；
> 若确实希望最远处停在 0.5，需要改 `ZoomOverviewFactor` —— 但那是**全局**参数，
> 会同时影响所有法术，**默认不改**。

### 验证
- 编译通过。
- 游戏内：同一只妖兽释放本命神通，在"详细视图"下与普通职业技能特效大小相当；
  逐步拉远镜头，特效随缩放同步变小（与法术一致）。
- `MclslBeastTalentFrames` 的图集尺寸校验（`texture.width!=768||height!=1152` 抛异常）不受影响。

### 风险
低。仅影响 `BT_` 的显示缩放与贴图 PPU；伤害/命中/结算完全不动。

---

## 2. 问题 2：全部过大的原版标识/特效 —— 全量归一化

### 核心思路（不依赖你手工确认）

不再逐个猜"白色十字是哪个方法画的"，而是：

1. **把缩放判据从"物种白名单"改为"体型阈值"** —— 任何单位只要体型明显超过
   原生成年基准，标记/特效就压回基准。这样**未登记的克隆物种、神兽、以及任何
   体型异常的个体**都会被覆盖，不再漏。
2. **把归一化覆盖到扫描出的全部绘制/特效入口**（上表 ❌ 项）。
3. **对 `getScaleMod` 做全局 postfix** —— 一处改动收敛所有"偏移类"标识
   （攻击范围、挥砍位置）。
4. **每一处补丁都做成"尽力而为+日志"**，失败不会让模组启动崩溃，并汇总到诊断日志，
   你只需把日志发我即可定位残余问题。

### 2.1 缩放判据改为体型阈值

`Systems/Beasts/MclslActorPresentation.cs`，替换现有 `SelectionScale` / `IndicatorScale` /
`MarkerScaleMod` 的判定（保留原方法名，避免改调用点）：

```csharp
internal const float NativeAdultMarkerScale = 0.1f;
// 标记/特效缩放上限：原生成年（0.1）的 1.5 倍。
// 任何超过该体型的单位（模组妖兽、克隆神兽、原版巨龙等）统一压回此上限，
// 保证"过大的标识"全部收敛；正常体型单位保持原值，不受影响。
private const float MarkerScaleCeiling = 0.15f;

// 体型判据：不再依赖物种白名单。IsModCreature 仅用于日志/兼容，不再作为缩放前提。
private static bool IsOversized(BaseSimObject unit)
    => unit != null && unit.current_scale.y > MarkerScaleCeiling;

internal static float SelectionScale(BaseSimObject unit)
    => IsOversized(unit) ? MarkerScaleCeiling : unit?.current_scale.y ?? 1f;

internal static Vector3 IndicatorScale(BaseSimObject unit)
    => IsOversized(unit) ? new Vector3(MarkerScaleCeiling, MarkerScaleCeiling, 1f)
                         : unit?.current_scale ?? Vector3.one;

internal static float MarkerScaleMod(Actor actor)
    => actor == null ? 1f
       : IsOversized(actor) ? MarkerScaleCeiling / NativeAdultMarkerScale
                            : actor.current_scale.y / NativeAdultMarkerScale;

// 供 BaseEffect / 粒子等只有 BaseSimObject 引用的地方使用
internal static float MarkerVisualScale(BaseSimObject unit)
    => IsOversized(unit) ? MarkerScaleCeiling : unit?.current_scale.y ?? 1f;
```

> 若你希望**保留**原版巨龙/巨人的标记比例，可把 `IsOversized` 改成
> `IsModCreature(actor) && current_scale.y > MarkerScaleCeiling`。
> 但按"全部过大的都要缩小"的要求，默认用纯体型判据。

### 2.2 扩展 QuantumSpriteLibrary 的两个漏网方法

`Patches/MclslPatches.cs:29-32`，把 `drawUnitsEffectDamage` / `drawUnitsEffectHighlight`
从 `HumanoidBodyScaleTranspiler` 拆出来，改用 `SelectedUnits_Scale_Transpiler`：

```csharp
// 头像仍跟随体型（这是生物本体，不能压小）
count += MclslHarmonyPatchGuard.TryPatch(harmony, "QuantumSpriteLibrary.drawUnitsAvatars.humanoidSize",
    AccessTools.Method(typeof(QuantumSpriteLibrary), "drawUnitsAvatars", new[]{ typeof(QuantumSpriteAsset) }),
    transpiler: AccessTools.Method(typeof(MclslPatches), nameof(HumanoidBodyScaleTranspiler))) ? 1 : 0;

// 受击/高亮特效按"标记"归一化（白十字即出自这里）
foreach (string method in new[] { "drawUnitsEffectDamage", "drawUnitsEffectHighlight" })
    count += MclslHarmonyPatchGuard.TryPatch(harmony, "QuantumSpriteLibrary." + method + ".markerSize",
        AccessTools.Method(typeof(QuantumSpriteLibrary), method, new[]{ typeof(QuantumSpriteAsset) }),
        transpiler: AccessTools.Method(typeof(MclslPatches), nameof(SelectedUnits_Scale_Transpiler))) ? 1 : 0;
```

### 2.3 覆盖特效/粒子/施法动画（新增 4 个入口）

`BaseEffect::updateAttached`、`Actor::spawnParticle`、`Actor::doCastAnimation`、
`ActionLibrary::flamingWeapon` 都直接乘 `current_scale`，需要同样的 transpiler。
但它们不是 `QuantumSpriteAsset` 参数族，需单独 `TryPatch`，并且**必须用不抛异常的版本**：

先在 `MclslPatches.cs` 新增宽松版（与 `SelectedUnits_Scale_Transpiler` 同逻辑，
但 `replaced==0` 时只记日志、不抛异常）：

```csharp
private static IEnumerable<CodeInstruction> EffectScaleTranspiler(
    IEnumerable<CodeInstruction> instructions)
{
    var codes = new List<CodeInstruction>(instructions);
    const BindingFlags F = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    FieldInfo scale = AccessTools.Field(typeof(BaseSimObject), "current_scale");
    MethodInfo visual = AccessTools.Method(typeof(MclslActorPresentation), nameof(MclslActorPresentation.MarkerVisualScale));
    MethodInfo vec = AccessTools.Method(typeof(MclslActorPresentation), nameof(MclslActorPresentation.IndicatorScale));
    int replaced = 0;
    // ldflda current_scale; ldfld x|y  ->  call IndicatorScale (返回 Vector3)
    for (int i = 0; i + 1 < codes.Count; i++)
    {
        if (codes[i].opcode == OpCodes.Ldflda && Equals(codes[i].operand, scale)
            && (codes[i+1].LoadsField(AccessTools.Field(typeof(Vector3), "x"))
             || codes[i+1].LoadsField(AccessTools.Field(typeof(Vector3), "y"))))
        {
            codes[i].opcode = OpCodes.Call; codes[i].operand = vec;
            codes[i+1].opcode = OpCodes.Nop; codes[i+1].operand = null;
            replaced++;
        }
    }
    // ldfld current_scale -> call MarkerVisualScale (返回 float)
    for (int i = 0; i < codes.Count; i++)
    {
        if (codes[i].opcode == OpCodes.Ldfld && Equals(codes[i].operand, scale))
        { codes[i].opcode = OpCodes.Call; codes[i].operand = visual; replaced++; }
    }
    if (replaced == 0)
        MclslDiagnostics.Once("effect-scale-nomatch",
            "特效缩放未匹配到 current_scale（可能是新版本 IL 结构变化）");
    return codes;
}
```

注册（`ApplySafely` 内）：

```csharp
foreach (var (typeName, methodName, attrDesc) in new (string,string,string)[] {
    ("BaseEffect",  "updateAttached", "BaseEffect.updateAttached"),
    ("Actor",       "spawnParticle",  "Actor.spawnParticle"),
    ("Actor",       "doCastAnimation","Actor.doCastAnimation"),
    ("ActionLibrary","flamingWeapon",  "ActionLibrary.flamingWeapon"),
})
{
    var m = AccessTools.Method(AccessTools.TypeByName(typeName), methodName);
    if (m == null) { MclslDiagnostics.Once("effect-scale-missing:"+attrDesc, "未找到 "+attrDesc); continue; }
    count += MclslHarmonyPatchGuard.TryPatch(harmony, attrDesc, m,
        transpiler: AccessTools.Method(typeof(MclslPatches), nameof(EffectScaleTranspiler))) ? 1 : 0;
}
```

### 2.4 全局 postfix：`Actor.getScaleMod`

覆盖 `CombatActionLibrary::attackRangeAction`（攻击范围标识）、
`Actor::getSlashPosition`、以及内部 lambda，无需逐个 transpiler：

```csharp
// Actor.getScaleMod() = current_scale / 0.1。全程序集仅 4 处调用，且都不用于绘制生物本体，
// 因此对超体型单位统一压回标记基准，一次性解决"攻击范围/挥砍位置"这类偏移类标识。
[HarmonyPatch(typeof(Actor), "getScaleMod")]
private static class Actor_GetScaleMod_Marker_Patch
{
    private static void Postfix(Actor __instance, ref float __result)
    {
        if (__instance != null && __instance.current_scale.y > MclslActorPresentation.MarkerScaleCeilingPublic)
            __result = MclslActorPresentation.MarkerScaleMod(__instance);
    }
}
```

> 需要在 `MclslActorPresentation` 暴露一个只读属性
> `internal static float MarkerScaleCeilingPublic => MarkerScaleCeiling;`，
> 或在 patch 里直接调 `MarkerScaleMod` 并判断返回值。

### 2.5 自我诊断（让你无需人工二分）

在 `MclslBeastControlWindow` 或诊断页加一处"标识缩放自检"输出：

```csharp
// 打开性能/诊断后，打印当前显示单位的体型与最终标记缩放，便于你反馈残余问题
MclslDiagnostics.Once("marker-check:" + actor.asset.id,
    "asset=" + actor.asset.id + " current_scale=" + actor.current_scale.y
    + " => markerScale=" + MclslActorPresentation.SelectionScale(actor));
```

这样若还有东西过大，把日志里对应条目的 `current_scale` 发我即可，不必进游戏反复试。

### 2.6 验证
- 编译通过；启动日志里 `MclslHarmonyPatchGuard` 的失败项为 0（或只有明确列出的缺失项）。
- 游戏内逐项检查（**由你验证观感，不需要判断是哪个方法**）：
  1. 大体型妖兽被攻击 → 受击/高亮特效不再铺满屏幕
  2. 选中妖兽 → 选中框正常
  3. 妖兽身上的持续状态/粒子/挥砍特效正常
  4. 原版村民、原版巨龙（若希望保留其比例，见 2.1 的开关）无异常
  5. 建筑、图表、UI 不受影响

### 2.7 风险与兜底
- 中。`current_scale` 是字段，只能逐方法 transpile；IL 结构若与预期不符，
  宽松版只记日志不抛异常，不会导致启动失败。
- 全局 `getScaleMod` postfix 只影响 4 个已知调用点，且都不参与生物本体绘制。
- **建议分两步上线**：先做 2.1 + 2.2 + 2.4（低风险、覆盖面最大），
  观察后若仍有残留，再加 2.3 的四个入口。

---

## 3. 问题 3：兽潮计数卡死（沿用已确认的 4 项修复）

> 详细代码见前一版计划，这里保留结论与改动点清单。

### 已确认的 3 处结构缺陷
- **A** `MclslBeastWorld.Remaining`（`:291`）用了有副作用的 `ResolveKnownOrWorld`
  （会 `Register` + `Publish(All)`），UI 每次刷新都在改注册表。
- **B** 死亡时（`MclslDeathEventRouter.cs:35-39`）只 `ForgetRank`，**没有**把 id 从
  `wave.Actors` 移除、没调 `MclslBeastPopulationIndex.Forget` → `CountAutomatic()` 虚高
  → `PlannedCount` 的 `available` 归零 → "已达自动妖兽总上限，暂无生成名额"。
- **C** 波次无超时/自愈，`Remaining` 一旦不归零就永久"活动中"，累计 2 场即
  触发 `Trigger` 的 `>=2` 拦截 → 再也召不出兽潮。

### 4 项改动
1. `MclslDeathEventRouter` 第 11 步插入 `MclslBeastWorld.OnBeastDeath(actor)`：
   解析 `SourceKey` 的 `wave:<id>`，从该波 `Actors` 移除并 `PopulationIndex.Forget`。
2. `MclslActorRegistry` 新增只读 `TryPeekAlive(id, out actor)`（不登记、不发布）；
   `Remaining` 改用它。
3. `MclslBeastWorld.TickCore` 巡检加超时兜底（`StaleWaveYears = 3`），
   并把 `Status=="生成中"` 的卡死波也纳入。
4. `MclslBeastPopulationIndex` 新增 `ValidateSome(budget)` 惰性校验，
   在 `TickCore` 里每帧校验 16 个条目，剔除已死亡/消失的 id。

---

## 4. 实施顺序（建议）

```
① 问题 1（3 行改动）—— 最快、最安全，先做
② 问题 2 的 2.1 + 2.2 + 2.4（判据 + 两个绘制方法 + getScaleMod 全局）
③ 问题 3 的改 1/2/3/4
④ 重新编译 → 打开发者包 → 你进游戏验收
⑤ 若问题 2 仍有残留 → 再上 2.3 的四个特效入口
```

每步都：`tools/compile-check.ps1` 零 error → 继续下一步。

## 5. 构建与验证命令

```powershell
# 编译
powershell -NoProfile -ExecutionPolicy Bypass -File `
  MySimulatedLongevityRoad\tools\compile-check.ps1 `
  -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394' -OutputDirectory '_cc-fix'

# 验收后打包（开发者包 + 玩家包，沿用上次参数）
python tools/package-release.py --kind developer --local-development --skip-validators --output build/pkg-developer2
python tools/package-release.py --kind player --local-development --compress-assets --skip-validators --output build/pkg-player2
```

## 6. 风险与回滚

- 所有改动点都在 `code/` 下，改动前可再用 `_pre-change-backup` 模式备份。
- 回滚：`git checkout -- MySimulatedLongevityRoad/code` 或从备份恢复。
- 问题 2 的 transpiler 全部走 `MclslHarmonyPatchGuard.TryPatch`，
  单项失败不影响其它补丁与模组启动。

## 7. 需要你决定

1. **2.1 的判据**：默认"纯体型阈值"（任何 oversized 都压，含原版巨龙）。
   要不要保留原版巨龙/巨人的标记比例？（保留则改成"仅模组生物"判据）
2. **`drawUnitsAvatars`**：我按"生物本体"处理、**不缩小**。
   如果你认为缩略世界里的单位头像也过大，告诉我，我改用标记缩放。
3. **`MarkerScaleCeiling = 0.15`** 这个上限是否合适（原生成年 0.1 的 1.5 倍；
   想更小可调到 0.12，想更宽松可到 0.2）。
4. 问题 1 缩略世界你想要"最远 1/3"还是"最远 1/2"？后者需改全局 `ZoomOverviewFactor`，
   会影响所有法术，建议保持一致（1/3）。
