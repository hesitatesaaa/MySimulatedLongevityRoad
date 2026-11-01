# 三项问题修复计划方案（2026-11-01）

> 本文仅为计划，尚未改动代码。每项都标注了可核对的 `文件:行号`。

---

## 结论速览

| # | 问题 | 根因确定性 | 改动面 |
|---|---|---|---|
| 1 | 本命神通特效太小 | **已确定**（一处乘数 0.5f） | 1 行 + 可选常量 |
| 2 | 原版攻击标识太大 | **高**（缩放入口漏了 2 个绘制方法），需 1 步确认 | 新增 1 个 helper + 1 个 transpiler |
| 3 | 兽潮计数卡死、无法刷下一波 | **已确定 3 处结构缺陷** + 1 处待验证假设 | 4 处改动，跨 3 个文件 |

---

## 问题 1：本命神通（BT_）特效太小

### 根因

`MclslSpellSystem.cs:828`：

```csharp
float scale = ScreenScale(_camera, _spellId)
            * (_spellId.StartsWith("BT_", StringComparison.Ordinal) ? 0.5f : 1f);
```

**本命神通（BT_ 前缀）被硬编码乘了 0.5，普通法术是 1.0。**

叠加贴图本身的差异后，实际更小：

| 项 | 普通法术 | 本命神通 |
|---|---|---|
| 图集帧尺寸 | `Size` px（`MclslSpellSystem.cs:1028-1034`） | 192 px（`MclslBeastTalentFrames.cs:23`） |
| Sprite 的 PPU | `Size` | **160** |
| Sprite 世界尺寸 | 1.00 | 192 / 160 = **1.20** |
| 额外乘数（:828） | ×1.0 | **×0.5** |
| **最终相对大小** | **1.00** | **0.60** |

即本命神通只有普通法术的 **60%**，与"太小了"完全吻合。

### 修复方案

**主改（1 行）** —— `MclslSpellSystem.cs:828`：

```csharp
// 之前：BT_ 固定 0.5
float scale = ScreenScale(_camera, _spellId)
            * (_spellId.StartsWith("BT_", StringComparison.Ordinal) ? BtEffectScale : 1f);
```

在 `MclslSpellSystem` 内新增可调常量（与 `VisualScale` 放一起，:681 附近）：

```csharp
private const float VisualScale = 18f;
// 本命神通贴图 192px/PPU160 => 世界尺寸 1.2；普通法术为 1.0。
// 取 0.85 使两者视觉大致齐平；想让神通更醒目可调到 1.0。
private const float BtEffectScale = 0.85f;
```

- `0.85f` → 最终 1.02，与普通法术齐平（推荐起点）
- `1.0f` → 最终 1.20，比普通法术大 20%（更醒目）
- 若只想要"比现在大但不夸张"，`0.75f` 也可

**可选微调** —— `MclslBeastTalentFrames.cs:23` 的 `160`：
把 PPU 从 160 降到例如 128，会让所有本命神通整体再放大 1.25 倍，但会同时影响"命中/取消"各阶段的相对观感，**建议先用 `BtEffectScale` 调，不动 PPU**。

### 验证

- 编译通过（`tools/compile-check.ps1`）。
- 游戏内：让带本命神通的妖兽释放技能，对比普通职业技能的特效大小；
  打开 `MclslRuntimeSettings.DiagnosticsEnabled` 观察 `"法术特效"` 相关告警无异常。
- 因为 `ScreenScale` 对 BT_ 的 `zoom` 恒为 1（`MclslSpellSystem.cs:927`），
  改完在任何缩放下都同比放大，不会出现"远景变大"的副作用。

### 风险

低。只影响 BT_ 特效的显示缩放，不触碰伤害/命中/结算。

---

## 问题 2：生物的原版攻击标识太大

### 现状机制（已读通）

模组把"原版反馈标记"（选中框、任务图标、笑脸、旗帜、收藏星、状态图标等）
统一压到原生成年人基准 `NativeAdultMarkerScale = 0.1f`
（`MclslActorPresentation.cs:12`、`:115-126`），
手段是 transpiler 把原生方法里的 `current_scale` 换成模组感知的缩放：

- `SelectedUnits_Scale_Transpiler`（`MclslPatches.cs:574-619`）用于 **10 个**方法：
  `drawSelectedUnits`、`drawUnitsToBeSelectedBySquareTool`、`drawUnitTaskIcons`、
  `drawUnitHappinessIcons`、`drawUnitBanners`、`drawFavoritesGame`、`drawUnitMetas`、
  `drawMagnetUnits`、`drawSocialize`、`drawUnexploredAugmentationSprite`
  （注册见 `MclslPatches.cs:25-28`）
- `HumanoidBodyScaleTranspiler`（`:563-572`）用于 **3 个**方法：
  `drawUnitsAvatars`、`drawUnitsEffectDamage`、`drawUnitsEffectHighlight`
  （注册见 `MclslPatches.cs:29-32`）

### 根因

`HumanoidBodyScale`（`MclslActorPresentation.cs:107-113`）对**非飞升形态**直接返回
`unit.current_scale`，即**不做任何归一化**：

```csharp
internal static Vector3 HumanoidBodyScale(BaseSimObject unit)
{
    if (unit is not Actor actor || !MclslBeastActorRegistration.IsAscended(actor))
        return unit.current_scale;     // ← 大体型妖兽原样返回，标记被放大到地图级
    float scale = actor.asset.base_stats["scale"];
    return new Vector3(scale, scale, 1f);
}
```

而 `drawUnitsEffectDamage` / `drawUnitsEffectHighlight` 正是绘制
**受击/高亮特效**的入口——图 1 那个横贯屏幕的白色十字就属于这一类。
它们走的是 `HumanoidBodyScaleTranspiler`，因此对大体型妖兽 **沿用了巨大的
`current_scale`**，标记被放大到地图级。

**对比**：`drawSelectedUnits` 等 10 个方法走的是 `SelectedUnits_Scale_Transpiler`，
对模组生物已正确返回 0.1，所以选中框是正常的。**唯独 damage/highlight 这两个漏了。**

### 需要先做的 1 步确认（低成本）

白色十字到底由哪个方法绘制，用二选一即可判定：

- 在 `MclslPatches.cs:29-32` 的循环里**临时只保留** `drawUnitsEffectHighlight`，
  重进游戏看十字是否还在；再把 `drawUnitsEffectDamage` 单独保留再测一次。
- 或在 `HumanoidBodyScale` 里临时加一行
  `MclslDiagnostics.Once("hbscale-"+unit?.GetType().Name, ...)` 打印调用来源。

> 这一步只需一次游戏内观察，确认后再落地下面改动，避免改错方法。

### 修复方案

**新增一个"特效标记缩放"helper**（`MclslActorPresentation.cs`，紧跟 `IndicatorScale`）：

```csharp
// 受击/高亮这类原生特效也要按"标记"而非"体型"缩放：
// 模组生物压到 0.1，飞升形态仍沿用其归一化体型，原生单位保持不变。
internal static Vector3 EffectMarkerScale(BaseSimObject unit)
    => UsesVanillaMarker(unit) ? IndicatorScale(unit) : HumanoidBodyScale(unit);
```

**新增一个 transpiler**（`MclslPatches.cs`，仿照 `HumanoidBodyScaleTranspiler`）：

```csharp
private static IEnumerable<CodeInstruction> EffectMarkerScaleTranspiler(
    IEnumerable<CodeInstruction> instructions)
{
    FieldInfo scale = AccessTools.Field(typeof(BaseSimObject), "current_scale");
    MethodInfo replacement = AccessTools.Method(
        typeof(MclslActorPresentation), nameof(MclslActorPresentation.EffectMarkerScale));
    foreach (CodeInstruction code in instructions)
    {
        if (code.LoadsField(scale)) { code.opcode = OpCodes.Call; code.operand = replacement; }
        yield return code;
    }
}
```

**改注册**（`MclslPatches.cs:29-32`）——只把 damage/highlight 换成新 transpiler，
`drawUnitsAvatars` 保持 `HumanoidBodyScale`（头像应跟随体型，不能压小）：

```csharp
// 头像仍跟随体型
count += MclslHarmonyPatchGuard.TryPatch(harmony, "QuantumSpriteLibrary.drawUnitsAvatars.humanoidSize",
    AccessTools.Method(typeof(QuantumSpriteLibrary), "drawUnitsAvatars", new[]{ typeof(QuantumSpriteAsset) }),
    transpiler: AccessTools.Method(typeof(MclslPatches), nameof(HumanoidBodyScaleTranspiler))) ? 1 : 0;
// 受击/高亮特效改按标记缩放
foreach (string method in new[] { "drawUnitsEffectDamage", "drawUnitsEffectHighlight" })
    count += MclslHarmonyPatchGuard.TryPatch(harmony, "QuantumSpriteLibrary." + method + ".markerSize",
        AccessTools.Method(typeof(QuantumSpriteLibrary), method, new[]{ typeof(QuantumSpriteAsset) }),
        transpiler: AccessTools.Method(typeof(MclslPatches), nameof(EffectMarkerScaleTranspiler))) ? 1 : 0;
```

### 验证

- 编译通过。
- 游戏内：让大体型妖兽被攻击 / 被选中，确认白色十字回落到普通小体型单位的大小；
  同时确认飞升形态（人形）的受击特效**没有**变怪异。
- 回归：选中框、任务图标、笑脸、旗帜、收藏星尺寸不变。

### 风险

中低。`TryPatch` 失败会被 `MclslHarmonyPatchGuard` 记录，不会静默失效。
若 `current_scale` 的 IL 形态与预期不同，transpiler 里的 `LoadsField` 匹配可能为 0，
需要像 `SelectedUnits_Scale_Transpiler:617` 那样加一句"replaced<1 就抛异常"的断言，
让挂载失败立刻暴露而不是悄悄不生效。

---

## 问题 3：兽潮计数卡死，无法自动召唤下一波

### 现象对应代码

- UI 显示"剩余 N 只"读的是 `MclslBeastWorld.Remaining(wave)`
  （`MclslBeastControlWindow.cs:171`，`Signature()` 在 `:163` 也调它）
- 波次结束判定同样用 `Remaining(wave)==0`（`MclslBeastWorld.cs:279`）
- "无法召唤下一波"的拦截点在 `MclslBeastWorld.Trigger`：
  ```csharp
  // MclslBeastWorld.cs:144-146
  if(State.Waves.Any(x=>x.Status=="生成中")) ...
  if(State.Waves.Any(x=>x.Year==year&&x.Status!="已结束")) ...
  if(State.Waves.Count(x=>x.Status!="已结束")>=2) return CannotTrigger("同时活动兽潮已达两场上限。");
  ```
- 生成名额拦截：
  ```csharp
  // MclslBeastWorld.cs:160-162
  int count = MclslBeastRules.PlannedCount(level, CountMultiplier, AutomaticLimit,
      CountAutomatic(), State.Waves.Sum(x=>x.Count-x.Cursor)+State.NaturalPending.Count, ...);
  if(count==0) return CannotTrigger("已达自动妖兽总上限，暂无生成名额。");
  ```
  而 `PlannedCount`（`MclslBeastRules.cs:42`）`available = max(0, limit - alive - reserved)`，
  `alive` 只要虚高就会返回 0。

### 已确定的 3 处结构缺陷

**缺陷 A：`Remaining()` 是"有副作用的查询"**

```csharp
// MclslBeastWorld.cs:291
internal static int Remaining(MclslBeastWaveState wave)
    => wave.Actors.Count(id => MclslActorRegistry.ResolveKnownOrWorld(id, out var actor)
                            && MclslActorAccessor.Alive(actor));
```

`ResolveKnownOrWorld`（`MclslActorRegistry.cs:169-176`）在注册表里查不到时会
**从 `World.world.units` 取到并 `Register(actor)`**；`Register`（`:135-158`）会
`RefreshTraits / RefreshTargets / Publish(All)` 一整套。
于是**每刷新一次 UI、每 3 秒一次结束巡检，都可能往注册表里重新登记 Actor 并触发变化分发**。
读路径带写副作用，本身就该修。

**缺陷 B：没有死亡时刻的剪枝**

`MclslDeathEventRouter.CommitActorDeath` 第 11 步（`:35-39`）对妖兽只做了
`MclslBeastWorld.ForgetRank(id)`，**没有**：
- 把该 id 从所属 wave 的 `Actors` 里移除
- 调用 `MclslBeastPopulationIndex.Forget(id)`

`MclslBeastPopulationIndex.Automatic` 只在 `MclslActorRegistry.Unregister`
（`:190`）里被 `Forget`，而 `Unregister` 只由**周期性、限额、轮转**的
`CleanupInvalid`（`MclslActorRegistry.cs:224-247`，从 `MclslScheduler.cs:1065` 驱动）
触发。注册表大时，死亡妖兽会在 `Automatic` 里滞留很久 → `CountAutomatic()` 虚高
→ `PlannedCount` 的 `available` 归零 → **"已达自动妖兽总上限，暂无生成名额"**。

**缺陷 C：波次没有超时/自愈**

`TickCore` 的结束巡检（`:276-289`）只处理 `Status=="活动中" && Remaining(wave)==0`。
一旦 `Remaining` 因 A/B 或 id 复用而永远 >0，波次永久停在"活动中"。
累计 2 场即触发 `Trigger` 的 `>=2` 拦截，**从此再也召不出兽潮**——与用户描述完全一致。

### 待验证的假设（第 4 处）

`wave.Actors` 存的是裸 `long` id，没有世代（generation）校验。
若 WorldBox 回收并复用了单位 id，`ResolveKnownOrWorld(旧id)` 会命中**新单位**（活着的），
`Remaining` 就永远 >0。**这可能就是"妖兽已经没了却显示存活"的直接原因。**

验证方法（不改游戏逻辑）：
在 `TickCore` 的巡检里对每场活动波打印一次诊断
`wave.Id / Remaining / Actors.Count / 每个 id 的 (registry命中, units命中, isAlive)`，
跑一次兽潮到全灭，看残留的是哪一类。
或直接看存档里 `Waves[].Actors` 与当前 `World.world.units` 的 id 差集。

### 修复方案（4 处，互为兜底）

**改 1（根修）：死亡时立即剪枝** —— `MclslDeathEventRouter.cs` 第 11 步

```csharp
case 11:
    MclslBeastWorld.ForgetRank(MclslActorAccessor.Id(actor));
    MclslBeastWorld.OnBeastDeath(actor);          // ← 新增
    if(state.HealthDepletedAtDeath)
        MclslBeastLoot.Commit(actor, state.CultivatorDeath.PendingCauseCode, attackType);
    break;
```

`MclslBeastWorld` 新增：

```csharp
// 妖兽死亡：立刻从所属兽潮的 Actors 里摘除，并同步人口索引，
// 避免依赖周期性的注册表清理（那是有预算和轮转的，会滞后很久）。
internal static void OnBeastDeath(Actor actor)
{
    long id = MclslActorAccessor.Id(actor);
    if (id <= 0L) return;
    MclslBeastPopulationIndex.Forget(id);
    string source = MclslActorAccessor.GetString(actor, SourceKey);
    if (!source.StartsWith("wave:", StringComparison.Ordinal)) return;
    string waveId = source.Substring(5);
    foreach (var wave in State.Waves)
        if (wave.Id == waveId) { wave.Actors.Remove(id); break; }
}
```

**改 2：读路径不再产生副作用** —— `MclslActorRegistry` 新增只读探测

```csharp
// 只读：不登记、不发布，供 UI/巡检等纯查询使用。
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

`MclslBeastWorld.Remaining` 改为：

```csharp
internal static int Remaining(MclslBeastWaveState wave)
    => wave.Actors.Count(id => MclslActorRegistry.TryPeekAlive(id, out _));
```

**改 3：波次自愈** —— `MclslBeastWorld.TickCore` 的巡检补齐

```csharp
if(++_auditFrame%20==0)
{
    int year = MclslRuntime.CurrentYear();
    foreach(var wave in State.Waves)
    {
        if (wave.Status == "已结束") continue;
        bool spawnedOut = wave.Cursor >= wave.Count;
        // 正常结束：已生成完且无存活
        // 兜底结束：已生成完、且超过 N 年仍无净进展（防止 id 复用/异常把波次锁死）
        bool stale = spawnedOut && year > wave.Year + StaleWaveYears;
        if (spawnedOut && (Remaining(wave) == 0 || stale))
        {
            wave.Status = "已结束";
            MclslWorldRunRepository.AddEvent(year, "beast_tide_end",
                wave.Level + "级" + wave.Theme + "兽潮结束",
                wave.Year + "年始于(" + wave.X + "," + wave.Y + ")，实际生成" + wave.Actors.Count
                + "只，" + (Remaining(wave) == 0 ? "已无存活妖兽。" : "超时强制结束。"),
                wave.X, wave.Y, "兽潮发生地", string.Empty, "beast_tide_end|" + wave.Id);
            wave.Actors.Clear(); Save();
        }
    }
    ...
}
```

新增常量：`private const int StaleWaveYears = 3;`（可按手感调）

> 注意：现在的巡检只处理 `Status=="活动中"`，若某波永远停在"生成中"（例如
> `Config.paused` 长期为真导致 `TickCore` 在 `:243` 提前 return），也会被这条兜底救回。

**改 4：人口索引自校验** —— `MclslBeastPopulationIndex` 增加惰性校验

```csharp
// 每帧校验有限个条目，剔除已死亡/已消失的妖兽，保证 CountAutomatic 准确。
private static readonly List<long> _validateScratch = new();
private static int _validateCursor;
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

在 `MclslBeastWorld.TickCore` 里每次调用 `MclslBeastPopulationIndex.ValidateSome(16);`
（`MclslBeastPopulationIndex` 需要能访问 `MclslActorRegistry.TryPeekAlive`，同程序集即可）。

### 验证

1. **静态**：编译通过。
2. **游戏内主用例**：手动召一场兽潮 → 把妖兽全部杀死（或等它们死光）→
   观察"剩余 N 只"在数秒内归零、状态变"已结束"→ 下一年能正常自动召下一波。
3. **回归**：连续两场兽潮同时进行（上限 2 场）时行为不变；
   自然生成的妖兽计数（`CountAutomatic(naturalOnly:true)`）不受影响。
4. **压力**：`兽潮数量`调到上限 64，反复 3 轮，确认 `Automatic` 不单调增长。

### 风险

中。改动集中在一个子系统内，但触及死亡回调顺序。建议：
- 改 1 里 `OnBeastDeath` 必须**不抛异常**（外层 `MclslDeathEventRouter` 虽有 try/catch，
  但顺序在 `MclslBeastLoot.Commit` 之前，抛异常会跳过战利品结算）——所以
  `OnBeastDeath` 内部要自包 try/catch 或只做纯字典操作。
- `TryPeekAlive` 与 `Alive` 语义需一致（都用 `data != null && isAlive()`）。
- 改 3 的超时阈值先取 3 年，若玩家反馈"兽潮被提前结束"就调大。

---

## 改动清单（预计）

| 文件 | 改动 |
|---|---|
| `Systems/Cultivation/MclslSpellSystem.cs` | `BtEffectScale` 常量；`:828` 乘数 |
| `Systems/Beasts/MclslActorPresentation.cs` | 新增 `EffectMarkerScale` |
| `Patches/MclslPatches.cs` | 新增 `EffectMarkerScaleTranspiler`；`:29-32` 注册拆分 |
| `Systems/Beasts/MclslBeastWorld.cs` | 新增 `OnBeastDeath`、`StaleWaveYears`；`Remaining` 改用 `TryPeekAlive`；巡检补自愈；调用 `ValidateSome` |
| `Systems/Runtime/MclslActorRegistry.cs` | 新增 `TryPeekAlive` |
| `Systems/Beasts/MclslBeastPopulationIndex.cs` | 新增 `ValidateSome` |
| `Systems/Death/MclslDeathEventRouter.cs` | 第 11 步插入 `OnBeastDeath` |

## 需要你确认

1. **问题 2** 需要先做那 1 步游戏内确认（到底是 `drawUnitsEffectHighlight` 还是
   `drawUnitsEffectDamage`），确认前我不动这段。
2. **问题 3** 建议先跑一次「诊断打印」确认真实卡住原因（是 id 复用还是存活判定），
   再决定改 1/改 2 是否都需要；改 3 的兜底无论如何都建议加上。
3. 是否需要我在动代码前，先把问题 3 的诊断日志版本做出来给你跑一次？
