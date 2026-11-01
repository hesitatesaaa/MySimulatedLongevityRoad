# 最终施工方案（2026-11-01，按你的四项决定）

> 已确认的决定：
> 1. **保留原版**巨龙/巨人的标记比例 —— **只修改模组生物**（判据用 `IsModCreature`）
> 3. `MarkerScaleCeiling` 不需要作为判据，降级为自检告警阈值
> 4. 本命神通与法术**完全一致**（缩略世界的最小因子保持 `1/3`，不改）

涉及文件一览：

| 文件 | 改动 |
|---|---|
| `Systems/Visual/MclslBeastTalentFrames.cs` | PPU 160 → 192 |
| `Systems/Cultivation/MclslSpellSystem.cs` | 去掉 BT_ 的 2 处特例 |
| `Patches/MclslPatches.cs` | 拆分/新增 transpiler 注册；新增 getScaleMod 全局 postfix |
| `Systems/Beasts/MclslActorPresentation.cs` | 新增宽松 transpiler 用的两个入口 + 自检告警 |
| `Systems/Beasts/MclslBeastWorld.cs` | `OnBeastDeath`、`StaleWaveYears`、`Remaining` 改只读、巡检自愈 |
| `Systems/Runtime/MclslActorRegistry.cs` | 新增 `TryPeekAlive` |
| `Systems/Beasts/MclslBeastPopulationIndex.cs` | 新增 `ValidateSome` |
| `Systems/Death/MclslDeathEventRouter.cs` | 第 11 步调用 `OnBeastDeath` |

---

# 一、问题 1：本命神通特效 = 与法术一致

## 1.1 改动 1 —— `Systems/Visual/MclslBeastTalentFrames.cs:23`

```csharp
// 之前
frames[i]=Sprite.Create(texture,new Rect(i%4*192,(5-i/4)*192,192,192),new Vector2(0.5f,0.5f),160);
// 之后（PPU = 帧宽 192，与普通法术 Sprite.Create(..., Size) 的规则一致 => 世界尺寸 1.0）
frames[i]=Sprite.Create(texture,new Rect(i%4*192,(5-i/4)*192,192,192),new Vector2(0.5f,0.5f),192);
```

理由：普通法术的 Sprite 是 `Sprite.Create(frameRect, pivot, Size)`（`MclslSpellSystem.cs:1033-1034`），PPU 等于帧尺寸，世界尺寸恒为 1.0。本命神通原来用 160，世界尺寸 1.2，与法术不一致。改成 192 后两者完全对齐。

## 1.2 改动 2 —— `Systems/Cultivation/MclslSpellSystem.cs:927`

```csharp
// 之前
float zoom = spellId.StartsWith("BT_", StringComparison.Ordinal) ? 1f : ZoomFactor(camera);
// 之后
float zoom = ZoomFactor(camera);
```

## 1.3 改动 3 —— `Systems/Cultivation/MclslSpellSystem.cs:828`

```csharp
// 之前
float scale=ScreenScale(_camera,_spellId)*(_spellId.StartsWith("BT_",StringComparison.Ordinal)?0.5f:1f);
// 之后
float scale=ScreenScale(_camera,_spellId);
```

## 1.4 结果

| | 详细视图 | 缩略世界（拉最远） |
|---|---|---|
| 普通法术 | `ScreenScale × 1.0` | × 1/3 |
| 本命神通（改后） | `ScreenScale × 1.0`（**与法术相同**） | × 1/3（**与法术相同**） |

> `ZoomOverviewFactor`（`MclslSpellSystem.cs:902`）保持 `1f/3f` 不动，因为它对全部法术生效，改它会牵连普通法术。

## 1.5 验证
- 编译 0 error。
- 游戏内：妖兽本命神通在详细视图下与职业技能特效大小相当；拉远镜头时同步缩小。
- 回归：普通法术 S001/S010 等大小不变（它们本来就走 `zoom=ZoomFactor`、乘 1.0，未受影响）。

---

# 二、问题 2：只把"模组生物"的过大标识/特效压回基准

## 2.1 前提：现有判据已正确，问题在覆盖不全

`MclslActorPresentation.cs:115-126` 三个函数对模组生物已返回 `0.1`：

```csharp
internal static float SelectionScale(BaseSimObject unit)
    => UsesVanillaMarker(unit) ? NativeAdultMarkerScale : unit?.current_scale.y ?? 1f;
internal static Vector3 IndicatorScale(BaseSimObject unit)
    => UsesVanillaMarker(unit) ? new Vector3(NativeAdultMarkerScale, NativeAdultMarkerScale, 1f)
        : unit?.current_scale ?? Vector3.one;
internal static float MarkerScaleMod(Actor actor)
    => UsesVanillaMarker(actor) ? 1f : actor == null ? 1f : actor.current_scale.y / NativeAdultMarkerScale;
```

**这三个函数不改。** 下面只补"没接上它们"的绘制入口。

## 2.2 改动 A —— `Patches/MclslPatches.cs:29-32` 拆分注册

现状：`drawUnitsAvatars / drawUnitsEffectDamage / drawUnitsEffectHighlight` 三个都走
`HumanoidBodyScaleTranspiler`（对非飞升形态原样返回 `current_scale` → 妖翌特效巨大）。

改成：头像保持体型缩放，受击/高亮特效改走标记归一化：

```csharp
// 头像 = 生物本体，保持体型缩放（不缩小）
count += MclslHarmonyPatchGuard.TryPatch(harmony,
    "QuantumSpriteLibrary.drawUnitsAvatars.humanoidSize",
    AccessTools.Method(typeof(QuantumSpriteLibrary), "drawUnitsAvatars", new[]{ typeof(QuantumSpriteAsset) }),
    transpiler: AccessTools.Method(typeof(MclslPatches), nameof(HumanoidBodyScaleTranspiler))) ? 1 : 0;

// 受击 / 高亮特效 = 标识，按标记缩放（白十字即出自这里）
foreach (string method in new[] { "drawUnitsEffectDamage", "drawUnitsEffectHighlight" })
    count += MclslHarmonyPatchGuard.TryPatch(harmony,
        "QuantumSpriteLibrary." + method + ".markerSize",
        AccessTools.Method(typeof(QuantumSpriteLibrary), method, new[]{ typeof(QuantumSpriteAsset) }),
        transpiler: AccessTools.Method(typeof(MclslPatches), nameof(SelectedUnits_Scale_Transpiler))) ? 1 : 0;
```

`SelectedUnits_Scale_Transpiler` 已处理 `ldflda current_scale + ldfld x|y` 成对替换，
以及单独的 `ldfld current_scale`；对这两个方法同样适用。

## 2.3 改动 B —— 新增"宽松版" transpiler，覆盖特效/粒子/施法

扫描确认这 4 处也直接用 `current_scale` 且属于"视觉尺寸"：

- `BaseEffect::updateAttached`（妖兽身上挂的特效）
- `Actor::spawnParticle`（粒子）
- `Actor::doCastAnimation`（施法动画）
- `ActionLibrary::flamingWeapon`

在 `MclslPatches.cs` 新增（与严格版同逻辑，但匹配不到时**只记日志、不抛异常**）：

```csharp
private static IEnumerable<CodeInstruction> EffectScaleTranspiler(
    IEnumerable<CodeInstruction> instructions)
{
    var codes = new List<CodeInstruction>(instructions);
    FieldInfo scale = AccessTools.Field(typeof(BaseSimObject), "current_scale");
    FieldInfo vx = AccessTools.Field(typeof(Vector3), "x");
    FieldInfo vy = AccessTools.Field(typeof(Vector3), "y");
    MethodInfo indicator = AccessTools.Method(typeof(MclslActorPresentation), nameof(MclslActorPresentation.IndicatorScale));
    MethodInfo selection = AccessTools.Method(typeof(MclslActorPresentation), nameof(MclslActorPresentation.SelectionScale));
    int replaced = 0;
    // ldflda current_scale; ldfld x|y  -> call IndicatorScale (Vector3)
    for (int i = 0; i + 1 < codes.Count; i++)
    {
        if (codes[i].opcode == OpCodes.Ldflda && Equals(codes[i].operand, scale)
            && (codes[i+1].LoadsField(vx) || codes[i+1].LoadsField(vy)))
        {
            codes[i].opcode = OpCodes.Call; codes[i].operand = indicator;
            codes[i+1].opcode = OpCodes.Nop; codes[i+1].operand = null;
            replaced++;
        }
    }
    // ldfld current_scale -> call SelectionScale (float)
    for (int i = 0; i < codes.Count; i++)
    {
        if (codes[i].opcode == OpCodes.Ldfld && Equals(codes[i].operand, scale))
        { codes[i].opcode = OpCodes.Call; codes[i].operand = selection; replaced++; }
    }
    if (replaced == 0)
        MclslDiagnostics.Once("effect-scale-nomatch",
            "特效缩放未匹配到 current_scale（游戏版本 IL 可能已变化）");
    return codes;
}
```

在 `ApplySafely` 里注册：

```csharp
foreach (var spec in new[] {
    ("BaseEffect",    "updateAttached"),
    ("Actor",         "spawnParticle"),
    ("Actor",         "doCastAnimation"),
    ("ActionLibrary", "flamingWeapon"),
})
{
    Type t = AccessTools.TypeByName(spec.Item1);
    MethodInfo m = t == null ? null : AccessTools.Method(t, spec.Item2);
    if (m == null) { MclslDiagnostics.Once("effect-scale-missing:" + spec.Item1 + "." + spec.Item2,
        "未找到 " + spec.Item1 + "." + spec.Item2); continue; }
    count += MclslHarmonyPatchGuard.TryPatch(harmony, spec.Item1 + "." + spec.Item2, m,
        transpiler: AccessTools.Method(typeof(MclslPatches), nameof(EffectScaleTranspiler))) ? 1 : 0;
}
```

**不改**这 3 处（它们是**位置/偏移**，应随体型走，缩小反而会让特效应错位到身体中央）：
`Actor::getHeadOffsetPositionForFunRendering`、`Actor::getThrowStartPosition`、
`Actor::getSlashPosition`。

## 2.4 改动 C —— 全局 postfix：`Actor.getScaleMod`

`getScaleMod()` 全程序集只有 4 个调用点：`CombatActionLibrary::attackRangeAction`（攻击范围标识）、
`QuantumSpriteLibrary::drawStatusEffectFor`（已归一化）、`Actor::getSlashPosition`、一个初始化 lambda。
**没有任何一处用它绘制生物本体**，所以对模组生物统一返回 1.0 是安全的：

```csharp
// Actor.getScaleMod() = current_scale / 0.1。仅 4 个调用点，且都不用于生物本体绘制。
// 对模组生物（含神兽/克隆种）统一压回标记基准，一次性收敛攻击范围、挥砍位置等偏移标识。
[HarmonyPatch(typeof(Actor), "getScaleMod")]
private static class Actor_GetScaleMod_Marker_Patch
{
    private static void Postfix(Actor __instance, ref float __result)
    {
        if (__instance != null && MclslActorPresentation.UsesVanillaMarker(__instance))
            __result = 1f;   // = MarkerScaleMod 对模组生物的返回值
    }
}
```

> `UsesVanillaMarker` 需保持 `internal`（现有已是）。

## 2.5 改动 D —— 自检告警（防"漏登记的模组物种"）

在 `MclslActorPresentation.Configure` 末尾或注册时加一条一次性告警；更简单的是在
`UsesVanillaMarker` 的调用点旁加判定。推荐加在 `MclslActorPresentation`：

```csharp
// 只改模组生物的前提是"模组生物都能被识别"。若某单位体型明显大于原生成年，
// 却没被 IsModCreature 认出来，极可能是漏登记的克隆物种——记录一条诊断便于发现。
internal static bool UsesVanillaMarker(BaseSimObject unit)
{
    if (unit is not Actor actor) return false;
    bool mod = IsModCreature(actor);
    if (!mod && actor.current_scale.y > OversizedWarnThreshold)
        MclslDiagnostics.Once("marker-miss:" + actor.asset?.id,
            "单位体型 " + actor.current_scale.y + " 超过原生成年，但未被识别为模组生物："
            + actor.asset?.id);
    return mod;
}
private const float OversizedWarnThreshold = 0.2f;   // 原生成年 0.1 的 2 倍
```

> 这条**只记录日志、不改变行为**。你把这行日志发我，就能判断是否需要扩充
> `IsModCreature`（现有名单在 `MclslActorPresentation.cs:91-103`）。

## 2.6 验证
- 编译 0 error；启动时 `MclslHarmonyPatchGuard.FailedRequiredCount` 为 0。
- 游戏内（观感由你判断，无需定位方法）：
  1. 大体型妖兽被攻击 → 受击/高亮特效不再铺满屏幕
  2. 妖兽身上持续特效/粒子/挥砍/施法动画正常
  3. 攻击范围标识（悬停/交战时）正常
  4. **原版巨龙、巨人、村民的标识保持原样**（这是你要求保留的）
- 若诊断里出现 `marker-miss:`，把该条目发我。

## 2.7 风险
- 中低。所有补丁都走 `MclslHarmonyPatchGuard.TryPatch`，单项失败只记录、不影响启动。
- `EffectScaleTranspiler` 匹配不到时只记日志，不会抛异常。
- `getScaleMod` 全局 postfix 只对模组生物生效，原版单位 `__result` 不变。

---

# 三、问题 3：兽潮计数卡死

## 3.1 根因（已确认 3 处 + 1 处待观察）

- **A** `MclslBeastWorld.Remaining`（`:291`）用 `ResolveKnownOrWorld` → 会 `Register`+`Publish(All)`，
  UI 每刷新都在改注册表（读路径有副作用）。
- **B** 死亡时 `MclslDeathEventRouter:35-39` 只 `ForgetRank`，没从 `wave.Actors` 摘除、
  没 `MclslBeastPopulationIndex.Forget` → `CountAutomatic()` 虚高 → `PlannedCount` 的
  `available` 归零 → "已达自动妖兽总上限，暂无生成名额"。
- **C** 波次无超时/自愈，`Remaining` 不归零就永久"活动中"，累计 2 场即触发
  `Trigger` 的 `>=2` 拦截 → 再也召不出兽潮。

## 3.2 改动 1 —— `Systems/Runtime/MclslActorRegistry.cs` 新增只读探测

在 `ResolveKnownOrWorld` 下方新增：

```csharp
/// <summary>只读存活探测：不登记、不发布，供 UI 与巡检等纯查询使用。</summary>
internal static bool TryPeekAlive(long actorId, out Actor actor)
{
    actor = null;
    if (actorId <= 0L) return false;
    if (ActorsById.TryGetValue(actorId, out Entry known) && known.Actor?.data != null)
        actor = known.Actor;
    else
        actor = World.world?.units?.get(actorId);
    return actor?.data != null && actor.isAlive();
}
```

## 3.3 改动 2 —— `MclslBeastWorld.Remaining` 改只读

```csharp
// 之前
internal static int Remaining(MclslBeastWaveState wave)
    => wave.Actors.Count(id => MclslActorRegistry.ResolveKnownOrWorld(id, out var actor)
                            && MclslActorAccessor.Alive(actor));
// 之后
internal static int Remaining(MclslBeastWaveState wave)
    => wave.Actors.Count(id => MclslActorRegistry.TryPeekAlive(id, out _));
```

## 3.4 改动 3 —— `MclslBeastWorld` 新增死亡剪枝

```csharp
/// <summary>妖兽死亡：立刻从所属兽潮摘除并同步人口索引，不等周期性注册表清理。</summary>
internal static void OnBeastDeath(Actor actor)
{
    try
    {
        long id = MclslActorAccessor.Id(actor);
        if (id <= 0L) return;
        MclslBeastPopulationIndex.Forget(id);
        string source = MclslActorAccessor.GetString(actor, SourceKey);
        if (!source.StartsWith("wave:", StringComparison.Ordinal)) return;
        string waveId = source.Substring(5);
        for (int i = 0; i < State.Waves.Count; i++)
            if (State.Waves[i].Id == waveId) { State.Waves[i].Actors.Remove(id); break; }
    }
    catch (Exception ex) { MclslDiagnostics.Error("beast-on-death", "妖兽死亡剪枝失败: " + ex.Message); }
}
```

`Systems/Death/MclslDeathEventRouter.cs` 第 11 步（`:35-39`）：

```csharp
case 11:
    MclslBeastWorld.ForgetRank(MclslActorAccessor.Id(actor));
    MclslBeastWorld.OnBeastDeath(actor);           // ← 新增，且内部自包 try/catch
    if(state.HealthDepletedAtDeath)
        MclslBeastLoot.Commit(actor,state.CultivatorDeath.PendingCauseCode,attackType);
    break;
```

> `OnBeastDeath` 内部已包 try/catch，不会影响其后的 `MclslBeastLoot.Commit`。

## 3.5 改动 4 —— `MclslBeastWorld.TickCore` 巡检自愈

新增常量：`private const int StaleWaveYears = 3;`

替换 `:276-289` 的巡检块：

```csharp
if(++_auditFrame%20==0)
{
    int year = MclslRuntime.CurrentYear();
    foreach(var wave in State.Waves)
    {
        if (wave.Status == "已结束") continue;
        bool spawnedOut = wave.Cursor >= wave.Count;
        if (!spawnedOut) continue;
        int remaining = Remaining(wave);
        bool stale = year > wave.Year + StaleWaveYears;   // 兜底：超时强制收场
        if (remaining != 0 && !stale) continue;
        wave.Status = "已结束";
        MclslWorldRunRepository.AddEvent(year, "beast_tide_end",
            wave.Level + "级" + wave.Theme + "兽潮结束",
            wave.Year + "年始于(" + wave.X + "," + wave.Y + ")，实际生成" + wave.Actors.Count + "只，"
            + (remaining == 0 ? "已无存活妖兽。" : "超时强制结束（残留" + remaining + "）。"),
            wave.X, wave.Y, "兽潮发生地", string.Empty, "beast_tide_end|" + wave.Id);
        wave.Actors.Clear(); Save();
    }
    if(State.Waves.Count>64)State.Waves.RemoveAll(x=>x.Status=="已结束" && x.Year<year-20);
}
```

要点：
- 覆盖 `Status=="生成中"` 的卡死波（原来只处理 `"活动中"`）。
- `stale` 兜底：即使 `Remaining` 因极端情况不归零，超过 3 年也强制结束，**永远不会再锁死后续兽潮**。

## 3.6 改动 5 —— `MclslBeastPopulationIndex` 惰性自校验

```csharp
private static readonly List<long> _validateScratch = new();
private static int _validateCursor;
/// <summary>每帧校验有限个条目，剔除已死亡/已消失的妖兽，保证 CountAutomatic 准确。</summary>
internal static void ValidateSome(int budget)
{
    if (budget <= 0 || Automatic.Count == 0) return;
    _validateScratch.Clear();
    _validateScratch.AddRange(Automatic.Keys);
    if (_validateCursor >= _validateScratch.Count) _validateCursor = 0;
    int checks = Math.Min(budget, _validateScratch.Count);
    for (int i = 0; i < checks; i++)
    {
        if (_validateCursor >= _validateScratch.Count) _validateCursor = 0;
        long id = _validateScratch[_validateCursor++];
        if (!MclslActorRegistry.TryPeekAlive(id, out _)) Forget(id);
    }
}
```

在 `MclslBeastWorld.TickCore` 开头（`_locked` 判定之后）加：

```csharp
MclslBeastPopulationIndex.ValidateSome(16);
```

## 3.7 验证
1. 编译 0 error。
2. 游戏内主用例：手动召兽潮 → 杀光 / 等它们死 → "剩余 N 只"数秒内归零、
   状态变"已结束" → **下一年能正常自动召下一波**。
3. 回归：同时 2 场兽潮（上限）行为不变；自然妖兽计数不受影响。
4. 压力：兽潮上限 64，连续 3 轮，`Automatic` 不单调增长。

## 3.8 风险
- 中。改动集中在兽潮子系统，但触及死亡回调顺序；`OnBeastDeath` 已自包 try/catch。
- `TryPeekAlive` 与 `Alive` 语义一致（都是 `data != null && isAlive()`）。

---

# 四、实施顺序与构建

```
① 问题 1（3 处，最小风险）
② 问题 2 改动 A + B + C + D（覆盖）
③ 问题 3 改动 1~5
④ 编译 → 打开发者包 → 你验收
```

每步编译命令（零 error 才继续）：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File `
  MySimulatedLongevityRoad\tools\compile-check.ps1 `
  -GameRoot 'D:\owl\gameversion\worldbox-ab712b72f394' -OutputDirectory '_cc-fix'
```

全部通过后打包：

```powershell
# 开发者包
python tools/package-release.py --kind developer --local-development --skip-validators --output build/pkg-dev-final
# 玩家包
python tools/package-release.py --kind player --local-development --compress-assets --skip-validators --output build/pkg-player-final
```

---

# 五、回滚

- 改动前先按上次做法备份到 `_pre-change-backup2/`。
- 回滚：从该目录恢复，或 `git checkout -- MySimulatedLongevityRoad/code`。

---

# 六、仍可选的一个点

`MarkerScaleCeiling` 已按你的选择降级为**自检阈值**
（`OversizedWarnThreshold = 0.2f`，只用于发现"漏登记的模组物种"），
不再参与缩放判定。若你希望连这个告警也去掉，告诉我即可。

---

# 七、性能核算与执行结果

## 7.1 结论：不会造成性能下降，关键路径反而更快

| 改动 | 运行期成本 | 判定 |
|---|---|---|
| 问题1 PPU 160→192 | 仅图集构建时一次性生效，运行期零成本 | ✅ 零 |
| 问题1 去掉 BT_ 两处特例 | BT_ 每帧多算一次 `ZoomFactor`（几次浮点+读相机尺寸）；普通法术本来每帧都算，BT_ 同屏数量极少 | ✅ 可忽略 |
| 问题2 transpiler | **启动时一次性改写 IL**，运行期零成本 | ✅ 零 |
| 问题2 注入的 `SelectionScale/IndicatorScale` | 每次标记绘制走一次 `IsModCreature`。**已加按 assetId 的记忆化缓存**（`ModCreatureCache`），每次只剩 1 次字典查询；而原来 11 个已归一化方法每帧要做 5-6 次字典/字符串比较 | ✅ **净提速** |
| 问题2 `getScaleMod` postfix | 全程序集仅 4 个调用点，均非逐帧逐单位 | ✅ 可忽略 |
| 问题3 `Remaining` 改 `TryPeekAlive` | 原来 `ResolveKnownOrWorld` 可能 `Register`+`Publish(All)`；只读版不登记不发布 | ✅ **明显更快** |
| 问题3 `OnBeastDeath` | 每次妖兽死亡扫 `State.Waves`（≤64） | ✅ 可忽略 |
| 问题3 `ValidateSome` | **已改为摊销式**：只在走完一轮时重建一次快照，其余每帧只 8 次字典查询 | ✅ 可忽略 |

### 两处必须先修正、并已修正的点

1. **`IsModCreature` 无缓存会让新入口放大开销** → 已加 `ModCreatureCache`（按 assetId）。
   这是本次唯一可能"变慢"的地方，缓存后不但抵消，还让原有 11 个方法变快。
2. **`ValidateSome` 朴素写法 `AddRange(Automatic.Keys)` 每帧全量拷贝** →
   已改为分摊式（游标 + 走完一轮才重建）。

> 另修正：`EffectScaleTranspiler` 的类型配对必须与严格版一致
> （`ldflda+ldfld 单轴` → `SelectionScale`(float)；`ldfld 整向量` → `IndicatorScale`(Vector3)），
> 否则会因栈类型不匹配导致 IL 校验失败。

## 7.2 执行结果（已完成）

编译：`sources=276 exit=0 error=0 warning=16`（16 条与基线相同，均为 CS0649 DTO 噪声）。

| 文件 | 已改内容 |
|---|---|
| `Systems/Visual/MclslBeastTalentFrames.cs` | PPU 160 → 192 |
| `Systems/Cultivation/MclslSpellSystem.cs` | 去掉 `zoom=1f` 特例、去掉 `×0.5` |
| `Systems/Beasts/MclslActorPresentation.cs` | `ModCreatureCache` 记忆化；`UsesVanillaMarker` 加 `marker-miss` 自检 |
| `Patches/MclslPatches.cs` | 拆分 avatars/effect 注册；新增 `EffectScaleTranspiler`；新增 4 个特效入口；`Actor.getScaleMod` postfix |
| `Systems/Runtime/MclslActorRegistry.cs` | 新增 `TryPeekAlive` |
| `Systems/Beasts/MclslBeastWorld.cs` | `Remaining` 改只读；`OnBeastDeath`；`StaleWaveYears=3` 自愈巡检；`ValidateSome(8)` |
| `Systems/Beasts/MclslBeastPopulationIndex.cs` | 新增摊销式 `ValidateSome` |
| `Systems/Death/MclslDeathEventRouter.cs` | 第 11 步调用 `OnBeastDeath` |

回滚备份：`_pre-change-backup2/`（8 个文件）。

