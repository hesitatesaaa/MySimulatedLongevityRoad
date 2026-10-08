using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ai;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Core;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslItemEffectDriver
{
    private static readonly MclslTimedPulseQueue Healing = new();
    private static long _deadline;
    private static readonly Func<bool> Expired = () => MclslFrameDeadline.Expired || System.Diagnostics.Stopwatch.GetTimestamp() >= _deadline;
    private static readonly Func<long, float, bool> ApplyHealing = Apply;
    internal static int PendingCount => Healing.Count;
    internal static bool CanSchedule(Actor actor) => Healing.CanSchedule(MclslActorAccessor.Id(actor));
    internal static void ClearRuntime() => Healing.Clear();
    internal static void Forget(long id) => Healing.Cancel(id);
    internal static void HealOverTime(Actor actor, float portion, int times, float interval)
    {
        if (!Healing.Schedule(MclslActorAccessor.Id(actor), Time.time, portion, times, interval))
            throw new InvalidOperationException("持续恢复队列已满，物品应在使用前检查容量");
    }
    internal static void Tick()
    {
        if (Healing.Count == 0 || MclslFrameDeadline.Expired) return;
        long sample = MclslPerformanceProbe.Begin();
        _deadline = System.Diagnostics.Stopwatch.GetTimestamp() + System.Diagnostics.Stopwatch.Frequency * 6 / 10000;
        try { Healing.Tick(Time.time, 128, ApplyHealing, Expired); }
        finally { MclslPerformanceProbe.End("持续恢复.延迟脉冲", sample); }
    }
    private static bool Apply(long id, float portion)
    {
        if (!MclslActorRegistry.ResolveKnownOrWorld(id, out Actor actor) || !MclslActorAccessor.Alive(actor)) return false;
        try { actor.restoreHealthPercent(portion); }
        catch (Exception ex) { MclslDiagnostics.Error("healing-pulse", "持续恢复失败: " + ex.Message); return false; }
        return true;
    }
}

internal static class MclslItemUseSystem
{
    private const string StatusPrefix = "mclsl_item_";

    internal static void RegisterStatuses()
    {
        Register("F026", 15f, ("multiplier_damage", 0.05f));
        Register("F027", 15f, ("multiplier_speed", 0.05f));
        Register("F001", 30f, ("armor", 15f));
        Register("F002", 30f, ("multiplier_damage", 0.15f));
        Register("F003", 30f, ("multiplier_speed", 0.15f));
        Register("F004", 30f, ("attack_speed", 0.12f));
        Register("F005", 45f, ("multiplier_health", 0.20f));
        Register("F006", 45f, ("critical_chance", 0.08f), ("accuracy", 10f));
        Register("F007", 60f);
        Register("D004", 999999999f, ("multiplier_health", 0.10f));
        Register("D010", 10f, ("armor", 20f));
        Register("D011", 5f, ("armor", 90f));
        Register("D009", 30f, ("armor", 5f));
        Register("D014", 20f);
        Register("D015", 20f);
        Register("D016", 60f);
        Register("D018", 5f, ("armor", 50f));
        Register("F021", 30f, ("multiplier_speed", 0.12f));
        Register("F022", 25f, ("armor", 15f));
        Register("F023", 20f);
        Register("F025", 15f);
    }

    private static void Register(string itemId, float duration, params (string Stat, float Value)[] stats)
    {
        string id = StatusPrefix + itemId;
        if (AssetManager.status.get(id) != null) return;
        MclslItemDefinition definition = MclslItemCatalog.Get(itemId);
        StatusAsset status = new() { id = id, duration = duration, base_stats = new BaseStats(), path_icon = definition?.IconPath ?? string.Empty,
            locale_id = itemId, locale_description = itemId + " Description" };
        foreach ((string stat, float value) in stats) TrySetStat(status.base_stats, stat, value);
        AssetManager.status.add(status);
    }

    internal static void TrySetStat(BaseStats stats, string id, float value)
    {
        if (stats == null || AssetManager.base_stats_library?.get(id) == null) return;
        stats.set(id, value);
    }

    internal static bool TryUse(Actor actor, string itemId)
        => TryUseWithTrigger(actor, itemId, MclslConsumableUseTrigger.Manual, MclslRuntime.CurrentYear());

    internal static string ManualUseFailureReason(Actor actor, string itemId)
    {
        MclslItemDefinition item = MclslItemCatalog.Get(itemId);
        if (!MclslActorAccessor.Alive(actor)) return "人物已不在世，无法使用物品。";
        if (MclslBagSystem.IsLocked(actor)) return "乾坤袋数据损坏，已锁定物品使用。";
        if (item == null || item.Category is not ("Pill" or "Talisman")) return "该物品不能直接使用。";
        MclslOwnedItem owned = MclslBagSystem.Peek(actor).Items.FirstOrDefault(x => x.ItemId == item.Id);
        if (owned == null) return "乾坤袋中没有这件物品。";
        if (item.Id is "D013" or "D020" or "F020" && ManaRestoreCooldownActive(actor))
            return "回灵物品仍在30秒冷却中。";
        if (item.Category == "Talisman" && actor.hasStatus(StatusPrefix + item.Id)) return "同类符箓效果仍在生效，不会消耗符箓。";
        if (item.Id is "D001" or "D002" or "D003" or "D012") return "该突破丹只会在对应境界突破时生效。";
        if (item.Id is "D006" or "D007" && !MclslItemEffectDriver.CanSchedule(actor)) return "持续恢复效果已达容量，请稍后使用；物品不会消耗。";
        if (item.Id == "D005") return "当前修为已无可用的补缺空间。";
        if (item.Id == "F007") return "当前没有可推进的修炼进度。";
        if (item.Id == "D011" && MclslRuntime.CurrentYear() - MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.life_save_year", -1000) < 50)
            return "造化紫金丹的保命效果尚在五十年冷却中。";
        return "当前状态不满足使用条件。";
    }

    private static bool TryUseAutomatic(Actor actor, string itemId, MclslConsumableUseTrigger trigger, int year)
        => TryUseWithTrigger(actor, itemId, trigger, year);

    private static bool TryUseWithTrigger(Actor actor, string itemId, MclslConsumableUseTrigger trigger, int year)
    {
        MclslItemDefinition item = MclslItemCatalog.Get(itemId);
        if (!MclslActorAccessor.Alive(actor) || MclslBagSystem.IsLocked(actor)
            || item == null || (item.Category != "Pill" && item.Category != "Talisman")) return false;
        MclslBagState bag = MclslBagSystem.Read(actor);
        MclslOwnedItem owned = MclslInventoryDataRules.FindOldest(bag.Items, itemId);
        if (owned == null || !MclslConsumableInventoryPolicy.CanUse(actor, item, owned.AcquiredYear, year, trigger)) return false;
        if (itemId is "D006" or "D007" && !MclslItemEffectDriver.CanSchedule(actor)) return false;
        bool manaRestoreItem = itemId is "D013" or "D020" or "F020";
        double worldTime = 0d;
        if (manaRestoreItem)
        {
            worldTime = World.world == null ? 0d : Convert.ToDouble(World.world.getCurWorldTime());
            if (ManaRestoreCooldownActive(actor, worldTime)) return false;
        }
        switch (itemId)
        {
            case "D019":
                actor.restoreHealthPercent(0.10f);
                break;
            case "D020":
                MclslSpellSystem.RestoreMana(actor, 0.08f);
                break;
            case "D004":
                if (MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.taishang_taken") == 0)
                {
                    MclslActorAccessor.Set(actor, "mclsl.architecture.v2.actor.taishang_taken", 1);
                    actor.addStatusEffect(StatusPrefix + itemId, 999999999f);
                    actor.finishStatusEffect("mclsl_item_foundation_damaged");
                    actor.updateStats();
                }
                else actor.restoreHealthPercent(0.20f);
                break;
            case "D005":
                int previousYear = MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.buque_year", -1000);
                int previousCount = MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.buque_count");
                int useCount = year - previousYear <= 30 ? previousCount + 1 : 1;
                float strength = useCount switch { 1 => 1f, 2 => 0.7f, 3 => 0.4f, _ => 0.1f };
                string realm = MclslActorAccessor.Realm(actor);
                bool ancient = MySimulatedLongevityRoad.Traits.MclslTraitRegistration.UsesAncientRealmTrait(actor);
                int span = MclslRealmProgress.RealmSpan(realm, ancient);
                int current = MclslCultivationGrowthSystem.CurrentTrueEssence(actor);
                int cap = MclslRealmProgress.NextRealmMinimum(realm, ancient);
                if (MclslActorAccessor.GetFloat(actor, MclslActorDataKeys.CultivationProgress) >= 99f)
                    MclslActorAccessor.Set(actor, "mclsl.architecture.v2.actor.buque_break_bonus", Math.Max(1, (int)MathF.Round(25f * strength)));
                int gain = Math.Max(1, (int)MathF.Round(span * 0.35f * strength));
                MclslCultivationGrowthSystem.SetTrueEssence(actor, realm,
                    cap > 0 ? Math.Min(cap, current + gain) : current + gain, ancient, enforceRealmMinimum: true);
                MclslActorAccessor.Set(actor, "mclsl.architecture.v2.actor.buque_year", year);
                MclslActorAccessor.Set(actor, "mclsl.architecture.v2.actor.buque_count", useCount);
                break;
            case "D006":
                actor.restoreHealthPercent(0.30f);
                MclslItemEffectDriver.HealOverTime(actor, 0.02f, 10, 2f);
                actor.finishStatusEffect("injured");
                break;
            case "D007":
                actor.restoreHealthPercent(0.85f);
                actor.finishStatusEffect("injured");
                actor.finishStatusEffect("crippled");
                MclslItemEffectDriver.HealOverTime(actor, 0.01f, 15, 1f);
                break;
            case "D008":
                actor.restoreHealthPercent(0.30f);
                actor.finishStatusEffect("voices_in_my_head");
                break;
            case "D009":
                actor.restoreHealthPercent(0.15f);
                CleanseCommon(actor);
                actor.addStatusEffect(StatusPrefix + itemId, 30f);
                break;
            case "D010":
                actor.restoreHealthPercent(0.55f);
                actor.finishStatusEffect("injured");
                actor.addStatusEffect(StatusPrefix + itemId, 10f);
                break;
            case "D011":
                actor.setHealth(Math.Max(1, (int)MathF.Round(actor.getMaxHealth() * 0.80f)), true);
                actor.finishStatusEffect("injured");
                actor.addStatusEffect(StatusPrefix + itemId, 5f);
                MclslActorAccessor.Set(actor, "mclsl.architecture.v2.actor.life_save_year", MclslRuntime.CurrentYear());
                break;
            case "D013":
                MclslSpellSystem.RestoreMana(actor, 0.25f);
                break;
            case "D014":
                CleanseCommon(actor);
                actor.addStatusEffect(StatusPrefix + itemId, 20f);
                break;
            case "D015":
                actor.restoreHealthPercent(0.35f);
                actor.addStatusEffect(StatusPrefix + itemId, 20f);
                break;
            case "D016":
                actor.addStatusEffect(StatusPrefix + itemId, 60f);
                break;
            case "D017":
                actor.finishStatusEffect("voices_in_my_head");
                MclslSpellSystem.RestoreMana(actor, 0.40f);
                break;
            case "D018":
                actor.restoreHealthPercent(0.50f);
                actor.addStatusEffect(StatusPrefix + itemId, 5f);
                MclslActorAccessor.Set(actor, "mclsl.architecture.v2.actor.taixu_life_save_year", year);
                break;
            case "F020":
                MclslSpellSystem.RestoreMana(actor, 0.15f);
                break;
            case "F024":
                if (actor.attack_target is Actor lightningTarget && MclslActorAccessor.Alive(lightningTarget)
                    && lightningTarget.current_tile != null)
                {
                    int hits = 0;
                    foreach (Actor target in Finder.getUnitsFromChunk(lightningTarget.current_tile, 1, 3f, false))
                    {
                        if (!MclslActorAccessor.Alive(target) || target == actor
                            || target != lightningTarget && (lightningTarget.kingdom == null
                                || target.kingdom != lightningTarget.kingdom)) continue;
                        target.getHit(Math.Max(1f, actor.stats.get("damage") * 0.8f), true, AttackType.Divine, actor);
                        if (++hits >= 3) break;
                    }
                }
                break;
            case "F025":
                actor.addStatusEffect(StatusPrefix + itemId, 15f);
                int allies = 0;
                if (actor.current_tile == null) break;
                foreach (Actor candidate in Finder.getUnitsFromChunk(actor.current_tile, 1, 3f, false))
                {
                    if (!MclslActorAccessor.Alive(candidate) || candidate == actor
                        || actor.kingdom == null || candidate.kingdom != actor.kingdom) continue;
                    candidate.addStatusEffect(StatusPrefix + itemId, 15f);
                    if (++allies >= 7) break;
                }
                break;
            default:
                if (item.Category == "Talisman")
                {
                    actor.addStatusEffect(StatusPrefix + itemId, itemId == "F007" ? 60f
                        : itemId is "F005" or "F006" ? 45f : itemId == "F025" ? 15f
                        : itemId == "F023" ? 20f : itemId == "F022" ? 25f
                        : itemId is "F026" or "F027" ? 15f : 30f);
                    if (itemId == "F005") actor.restoreHealthPercent(0.20f);
                }
                break;
        }
        // Apply the validated effect first; only then spend the stack item.
        if (!MclslBagSystem.Remove(bag, itemId)) return false;
        MclslBagSystem.Write(actor, bag);
        if (manaRestoreItem)
            MclslActorAccessor.Set(actor, "mclsl.mana_restore.last_time", worldTime.ToString("R", CultureInfo.InvariantCulture));
        return true;
    }

    private static bool ManaRestoreCooldownActive(Actor actor, double? atWorldTime = null)
    {
        double now = atWorldTime ?? (World.world == null ? 0d : Convert.ToDouble(World.world.getCurWorldTime()));
        string lastText = MclslActorAccessor.GetString(actor, "mclsl.mana_restore.last_time");
        return double.TryParse(lastText, NumberStyles.Float, CultureInfo.InvariantCulture, out double last)
            && now >= last && now - last < 30d;
    }

    internal static int ConsumeBreakthroughBonus(Actor actor, string nextRealm, int year = -1)
    {
        string itemId = MclslConsumableInventoryPolicy.BreakthroughPillForRealm(nextRealm);
        if (itemId.Length == 0) return 0;
        int useYear = year < 0 ? MclslRuntime.CurrentYear() : year;
        MclslItemDefinition item = MclslItemCatalog.Get(itemId);
        MclslBagState bag = MclslBagSystem.Read(actor);
        MclslOwnedItem owned = MclslInventoryDataRules.FindOldest(bag.Items, itemId);
        if (owned == null || !MclslConsumableInventoryPolicy.CanUse(actor, item, owned.AcquiredYear, useYear,
            MclslConsumableUseTrigger.Breakthrough, nextRealm)) return 0;
        if (!MclslBagSystem.Remove(bag, itemId)) return 0;
        MclslBagSystem.Write(actor, bag);
        MclslActorAccessor.Set(actor, "mclsl.architecture.v2.actor.break_pill", itemId);
        MclslActorAccessor.Set(actor, "mclsl.architecture.v2.actor.break_pill_year", useYear);
        return itemId switch { "D001" => 25, "D002" => 30, "D003" => 35, _ => 40 };
    }

    internal static float FailureSetbackFactor(Actor actor, string nextRealm, int year)
    {
        if (MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.break_pill_year", -1) != year) return 1f;
        string pill = MclslActorAccessor.GetString(actor, "mclsl.architecture.v2.actor.break_pill");
        return (nextRealm, pill) switch
        {
            (MclslRealmIds.ZhuJi, "D001") => 0.5f,
            (MclslRealmIds.JinDan, "D002") => 0.6f,
            (MclslRealmIds.YuanYing, "D003") => 0.5f,
            (MclslRealmIds.HuaShen, "D012") => 0.5f,
            _ => 1f
        };
    }

    internal static bool ConsumedBreakthroughPill(Actor actor, string itemId, int year) =>
        MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.break_pill_year", -1) == year
        && MclslActorAccessor.GetString(actor, "mclsl.architecture.v2.actor.break_pill") == itemId;

    internal static void SyncPersistent(Actor actor)
    {
        if (MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.taishang_taken") > 0
            && !actor.hasStatus(StatusPrefix + "D004"))
            actor.addStatusEffect(StatusPrefix + "D004", 999999999f);
    }

    private static void CleanseCommon(Actor actor)
    {
        List<string> removable = new();
        foreach (string id in actor.getStatusesIds())
        {
            if (id.StartsWith(StatusPrefix, StringComparison.Ordinal)) continue;
            if (AssetManager.status.get(id)?.can_be_cured == true) removable.Add(id);
        }
        foreach (string id in removable) actor.finishStatusEffect(id);
    }

    internal static void ShortenCommonNegativeStatus(Actor actor, StatusAsset status, ref float duration)
    {
        if (actor == null || status == null || !status.can_be_cured) return;
        float factor = actor.hasStatus(StatusPrefix + "D009") ? 0.70f : 1f;
        string id = status.id ?? string.Empty;
        bool control = id.Contains("stun", StringComparison.OrdinalIgnoreCase)
            || id.Contains("slow", StringComparison.OrdinalIgnoreCase)
            || id.Contains("freeze", StringComparison.OrdinalIgnoreCase)
            || id.Contains("bind", StringComparison.OrdinalIgnoreCase);
        if (control && actor.hasStatus(StatusPrefix + "D014")) factor *= 0.5f;
        if (factor < 1f) duration = (duration > 0f ? duration : status.duration) * factor;
    }

    internal static bool TryAutoLifeSave(Actor actor, float incomingDamage = 0f)
    {
        if (!MclslActorAccessor.Alive(actor) || actor.getHealth() - incomingDamage > actor.getMaxHealth() * 0.10f) return false;
        int year = MclslRuntime.CurrentYear();
        if (MclslBagSystem.Count(actor, "D018") > 0
            && year - MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.taixu_life_save_year", -1000) >= 50
            && TryUseAutomatic(actor, "D018", MclslConsumableUseTrigger.Emergency, year)) return true;
        if (year - MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.life_save_year", -1000) < 50) return false;
        if (MclslBagSystem.Count(actor, "D011") > 0)
            return TryUseAutomatic(actor, "D011", MclslConsumableUseTrigger.Emergency, year);
        return false;
    }

    internal static void TryAutoCombatConsumables(Actor actor, float incomingDamage, bool lifeSaveUsed = false)
    {
        if (!MclslActorAccessor.Alive(actor) || incomingDamage <= 0f) return;
        MclslBagState bag = MclslBagSystem.Peek(actor);
        if (bag.Items.Count == 0) return;
        bool consumedPill = lifeSaveUsed;
        if (!consumedPill && actor.getHealth() - incomingDamage < actor.getMaxHealth() * 0.60f)
        {
            int year = MclslRuntime.CurrentYear();
            consumedPill = TryUseAutomatic(actor, "D010", MclslConsumableUseTrigger.Combat, year)
                || TryUseAutomatic(actor, "D007", MclslConsumableUseTrigger.Combat, year)
                || TryUseAutomatic(actor, "D006", MclslConsumableUseTrigger.Combat, year)
                || TryUseAutomatic(actor, "D015", MclslConsumableUseTrigger.Combat, year)
                || TryUseAutomatic(actor, "D019", MclslConsumableUseTrigger.Combat, year);
        }
        int currentYear = MclslRuntime.CurrentYear();
        if (!consumedPill && actor.hasStatus("voices_in_my_head"))
            consumedPill = TryUseAutomatic(actor, "D008", MclslConsumableUseTrigger.Combat, currentYear);
        if (!consumedPill && MclslConsumableInventoryPolicy.HasControlStatus(actor))
            consumedPill = TryUseAutomatic(actor, "D014", MclslConsumableUseTrigger.Combat, currentYear);
        if (!consumedPill && MclslConsumableInventoryPolicy.HasCureableNegativeStatus(actor))
            consumedPill = TryUseAutomatic(actor, "D009", MclslConsumableUseTrigger.Combat, currentYear);
        for (int i = 0; i < MclslConsumableInventoryPolicy.DefensiveTalismanCount; i++)
        {
            string talismanId = MclslConsumableInventoryPolicy.DefensiveTalismanAt(i);
            if (MclslBagSystem.Count(bag, talismanId) > 0)
            {
                if (TryUseAutomatic(actor, talismanId, MclslConsumableUseTrigger.Combat, currentYear)) break;
            }
        }
    }

    internal static void TryAutoCombatTalisman(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        MclslBagState bag = MclslBagSystem.Peek(actor);
        if (bag?.Items == null || bag.Items.Count == 0) return;

        bool hasSpecialPill = false;
        int talismanMask = 0;
        foreach (MclslOwnedItem owned in bag.Items)
        {
            if (owned == null || owned.Count <= 0 || string.IsNullOrEmpty(owned.ItemId)) continue;
            if (string.Equals(owned.ItemId, "D016", StringComparison.Ordinal)) hasSpecialPill = true;
            for (int priority = 0; priority < MclslConsumableInventoryPolicy.CombatTalismanCount; priority++)
            {
                if (!string.Equals(owned.ItemId, MclslConsumableInventoryPolicy.CombatTalismanAt(priority), StringComparison.Ordinal)) continue;
                talismanMask |= 1 << priority;
                break;
            }
        }
        MclslPerformanceProbe.RecordCombatBagSingleScan();
        if (!hasSpecialPill && talismanMask == 0) return;

        int year = MclslRuntime.CurrentYear();
        if (hasSpecialPill)
            TryUseAutomatic(actor, "D016", MclslConsumableUseTrigger.Combat, year);
        for (int priority = 0; priority < MclslConsumableInventoryPolicy.CombatTalismanCount; priority++)
        {
            if ((talismanMask & (1 << priority)) == 0) continue;
            string itemId = MclslConsumableInventoryPolicy.CombatTalismanAt(priority);
            if (TryUseAutomatic(actor, itemId, MclslConsumableUseTrigger.Combat, year)) return;
        }
    }

    internal static void TryAutoCultivationConsumables(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        TryUseAutomatic(actor, "D004", MclslConsumableUseTrigger.Cultivation, year);
        TryUseAutomatic(actor, "D005", MclslConsumableUseTrigger.Cultivation, year);
    }
}
