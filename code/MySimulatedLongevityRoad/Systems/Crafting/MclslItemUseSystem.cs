using System;
using System.Collections;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Core;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal sealed class MclslItemEffectDriver : MonoBehaviour
{
    internal static MclslItemEffectDriver Instance;

    internal static void Ensure()
    {
        if (Instance != null) return;
        GameObject host = new("MclslItemEffectDriver");
        DontDestroyOnLoad(host);
        Instance = host.AddComponent<MclslItemEffectDriver>();
    }

    internal static void ClearRuntime() => Instance?.StopAllCoroutines();

    internal void HealOverTime(Actor actor, float portion, int times, float interval)
    {
        StartCoroutine(Heal(actor, portion, times, interval));
    }

    private static IEnumerator Heal(Actor actor, float portion, int times, float interval)
    {
        for (int i = 0; i < times; i++)
        {
            yield return new WaitForSeconds(interval);
            if (!MclslActorAccessor.Alive(actor)) yield break;
            actor.restoreHealthPercent(portion);
        }
    }
}

internal static class MclslItemUseSystem
{
    private const string StatusPrefix = "mclsl_item_";

    internal static void RegisterStatuses()
    {
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
    {
        MclslItemDefinition item = MclslItemCatalog.Get(itemId);
        if (!MclslActorAccessor.Alive(actor) || item == null || (item.Category != "Pill" && item.Category != "Talisman")) return false;
        if (itemId is "D001" or "D002" or "D003" or "D012") return false; // consumed at a matching breakthrough
        if (itemId == "D005" && MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) >= MclslRealmIds.Index(MclslRealmIds.HuaShen)) return false;
        if (itemId == "D011" && MclslRuntime.CurrentYear()
            - MclslActorAccessor.GetInt(actor, "mclsl.v020.life_save_year", -1000) < 50) return false;
        MclslBagState bag = MclslBagSystem.Read(actor);
        if (!MclslBagSystem.Remove(bag, itemId)) return false;
        MclslBagSystem.Write(actor, bag);
        switch (itemId)
        {
            case "D004":
                if (MclslActorAccessor.GetInt(actor, "mclsl.v020.taishang_taken") == 0)
                {
                    MclslActorAccessor.Set(actor, "mclsl.v020.taishang_taken", 1);
                    actor.addStatusEffect(StatusPrefix + itemId, 999999999f);
                    actor.finishStatusEffect("mclsl_item_foundation_damaged");
                    actor.updateStats();
                }
                else actor.restoreHealthPercent(0.20f);
                break;
            case "D005":
                int year = MclslRuntime.CurrentYear();
                int previousYear = MclslActorAccessor.GetInt(actor, "mclsl.v020.buque_year", -1000);
                int previousCount = MclslActorAccessor.GetInt(actor, "mclsl.v020.buque_count");
                int useCount = year - previousYear <= 30 ? previousCount + 1 : 1;
                float strength = useCount switch { 1 => 1f, 2 => 0.7f, 3 => 0.4f, _ => 0.1f };
                string realm = MclslActorAccessor.Realm(actor);
                bool ancient = MySimulatedLongevityRoad.Traits.MclslTraitRegistration.UsesAncientRealmTrait(actor);
                int span = MclslRealmProgress.RealmSpan(realm, ancient);
                int current = MclslCultivationGrowthSystem.CurrentTrueEssence(actor);
                int cap = MclslRealmProgress.NextRealmMinimum(realm, ancient);
                if (MclslActorAccessor.GetFloat(actor, MclslActorDataKeys.CultivationProgress) >= 99f)
                    MclslActorAccessor.Set(actor, "mclsl.v020.buque_break_bonus", Math.Max(1, (int)MathF.Round(25f * strength)));
                int gain = Math.Max(1, (int)MathF.Round(span * 0.35f * strength));
                MclslCultivationGrowthSystem.SetTrueEssence(actor, realm,
                    cap > 0 ? Math.Min(cap, current + gain) : current + gain, ancient, enforceRealmMinimum: true);
                MclslActorAccessor.Set(actor, "mclsl.v020.buque_year", year);
                MclslActorAccessor.Set(actor, "mclsl.v020.buque_count", useCount);
                break;
            case "D006":
                actor.restoreHealthPercent(0.30f);
                MclslItemEffectDriver.Ensure();
                MclslItemEffectDriver.Instance.HealOverTime(actor, 0.02f, 10, 2f);
                actor.finishStatusEffect("injured");
                break;
            case "D007":
                actor.restoreHealthPercent(0.85f);
                actor.finishStatusEffect("injured");
                actor.finishStatusEffect("crippled");
                MclslItemEffectDriver.Ensure();
                MclslItemEffectDriver.Instance.HealOverTime(actor, 0.01f, 15, 1f);
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
                MclslActorAccessor.Set(actor, "mclsl.v020.life_save_year", MclslRuntime.CurrentYear());
                break;
            default:
                if (item.Category == "Talisman")
                {
                    actor.addStatusEffect(StatusPrefix + itemId, itemId == "F007" ? 60f : itemId is "F005" or "F006" ? 45f : 30f);
                    if (itemId == "F005") actor.restoreHealthPercent(0.20f);
                }
                break;
        }
        return true;
    }

    internal static int ConsumeBreakthroughBonus(Actor actor, string nextRealm)
    {
        string itemId = nextRealm switch
        {
            MclslRealmIds.ZhuJi => "D001", MclslRealmIds.JinDan => "D002",
            MclslRealmIds.YuanYing => "D003", MclslRealmIds.HuaShen => "D012", _ => string.Empty
        };
        if (itemId.Length == 0) return 0;
        MclslBagState bag = MclslBagSystem.Read(actor);
        if (!MclslBagSystem.Remove(bag, itemId)) return 0;
        MclslBagSystem.Write(actor, bag);
        MclslActorAccessor.Set(actor, "mclsl.v020.break_pill", itemId);
        MclslActorAccessor.Set(actor, "mclsl.v020.break_pill_year", MclslRuntime.CurrentYear());
        return itemId switch { "D001" => 25, "D002" => 30, "D003" => 35, _ => 40 };
    }

    internal static float FailureSetbackFactor(Actor actor, string nextRealm, int year)
    {
        if (MclslActorAccessor.GetInt(actor, "mclsl.v020.break_pill_year", -1) != year) return 1f;
        string pill = MclslActorAccessor.GetString(actor, "mclsl.v020.break_pill");
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
        MclslActorAccessor.GetInt(actor, "mclsl.v020.break_pill_year", -1) == year
        && MclslActorAccessor.GetString(actor, "mclsl.v020.break_pill") == itemId;

    internal static void SyncPersistent(Actor actor)
    {
        if (MclslActorAccessor.GetInt(actor, "mclsl.v020.taishang_taken") > 0
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
        if (actor == null || status == null || !status.can_be_cured || !actor.hasStatus(StatusPrefix + "D009")) return;
        duration = (duration > 0f ? duration : status.duration) * 0.70f;
    }

    internal static void TryAutoLifeSave(Actor actor, float incomingDamage = 0f)
    {
        if (!MclslActorAccessor.Alive(actor) || actor.getHealth() - incomingDamage > actor.getMaxHealth() * 0.10f) return;
        int year = MclslRuntime.CurrentYear();
        if (year - MclslActorAccessor.GetInt(actor, "mclsl.v020.life_save_year", -1000) < 50) return;
        if (MclslBagSystem.Count(actor, "D011") > 0) TryUse(actor, "D011");
    }

    internal static void TryAutoCombatConsumables(Actor actor, float incomingDamage)
    {
        if (!MclslActorAccessor.Alive(actor) || incomingDamage <= 0f) return;
        MclslBagState bag = MclslBagSystem.Peek(actor);
        if (bag.Items.Count == 0) return;
        if (actor.getHealth() - incomingDamage < actor.getMaxHealth() * 0.60f)
        {
            if (MclslBagSystem.Count(bag, "D010") > 0) TryUse(actor, "D010");
            else if (MclslBagSystem.Count(bag, "D007") > 0) TryUse(actor, "D007");
            else if (MclslBagSystem.Count(bag, "D006") > 0) TryUse(actor, "D006");
        }
        if (!actor.hasStatus(StatusPrefix + "F001") && MclslBagSystem.Count(bag, "F001") > 0) TryUse(actor, "F001");
    }
}
