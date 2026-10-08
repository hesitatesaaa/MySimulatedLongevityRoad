using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal enum MclslConsumableUseTrigger : byte
{
    Manual = 0,
    Combat = 1,
    Emergency = 2,
    Cultivation = 3,
    Breakthrough = 4
}

/// <summary>
/// Shared rules for which carried consumables remain useful, when automatic use
/// is allowed, and which per-item reserves the Tianxuan market must preserve.
/// </summary>
internal static class MclslConsumableInventoryPolicy
{
    internal const int PerItemReserveTarget = 2;

    private static readonly string[] CombatTalismanPriority = { "F024", "F002", "F004", "F006", "F001", "F005", "F003", "F020", "F021", "F022", "F023", "F025", "F026", "F027" };
    private static readonly string[] DefensiveTalismanPriority = { "F025", "F023", "F022", "F001", "F005", "F003", "F027" };

    internal static string BreakthroughPillForRealm(string realm) => realm switch
    {
        MclslRealmIds.LianQi => "D001",
        MclslRealmIds.ZhuJi => "D002",
        MclslRealmIds.JinDan => "D003",
        MclslRealmIds.YuanYing => "D012",
        _ => string.Empty
    };

    internal static int CombatTalismanCount => CombatTalismanPriority.Length;
    internal static string CombatTalismanAt(int index) => index >= 0 && index < CombatTalismanPriority.Length
        ? CombatTalismanPriority[index]
        : string.Empty;
    internal static int DefensiveTalismanCount => DefensiveTalismanPriority.Length;
    internal static string DefensiveTalismanAt(int index) => index >= 0 && index < DefensiveTalismanPriority.Length
        ? DefensiveTalismanPriority[index]
        : string.Empty;

    internal static bool CanUse(Actor actor, MclslItemDefinition item, int acquiredYear, int year,
        MclslConsumableUseTrigger trigger, string nextRealm = null)
    {
        if (!MclslActorAccessor.Alive(actor) || item == null
            || (item.Category != "Pill" && item.Category != "Talisman")) return false;

        string breakthroughItem = BreakthroughPillForRealm(nextRealm ?? string.Empty);
        if (IsBreakthroughPill(item.Id))
            return trigger == MclslConsumableUseTrigger.Breakthrough
                && string.Equals(item.Id, breakthroughItem, StringComparison.Ordinal)
                && IsOlderThanCurrentYear(acquiredYear, year);
        if (trigger == MclslConsumableUseTrigger.Breakthrough) return false;

        if (trigger != MclslConsumableUseTrigger.Manual && !IsOlderThanCurrentYear(acquiredYear, year)) return false;
        if (item.Id == "D011" && year - MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.life_save_year", -1000) < 50) return false;
        if (item.Id == "D018" && year - MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.taixu_life_save_year", -1000) < 50) return false;

        if (trigger == MclslConsumableUseTrigger.Manual)
        {
            if (item.Category == "Talisman" && actor.hasStatus("mclsl_item_" + item.Id)) return false;
            if (item.Id == "D005") return CanUseCultivationSupplement(actor);
            if (item.Id == "F007") return HasCultivationActivity(actor);
            if (item.Id is "D013" or "D017" or "D020" or "F020") return MclslSpellSystem.CurrentMana(actor) < MclslSpellSystem.MaxMana(actor);
            if (item.Id == "F024") return actor.has_attack_target && actor.attack_target != null;
            return true;
        }
        if (trigger == MclslConsumableUseTrigger.Emergency) return item.Id is "D011" or "D018";
        if (trigger == MclslConsumableUseTrigger.Cultivation)
        {
            if (item.Id == "D004") return MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.taishang_taken") == 0;
            if (item.Id == "D005") return CanUseCultivationSupplement(actor);
            return false;
        }
        if (trigger != MclslConsumableUseTrigger.Combat) return false;

        if (item.Id is "D006" or "D007" or "D010" or "D015" or "D019") return IsCurrentGradeUseful(actor, item);
        if (item.Id is "D013" or "D017" or "D020" or "F020") return IsCurrentGradeUseful(actor, item)
            && MclslSpellSystem.CurrentMana(actor) < MclslSpellSystem.MaxMana(actor) / 2;
        if (item.Id == "D014") return HasControlStatus(actor);
        if (item.Id == "D016") return MclslSpellSystem.CurrentMana(actor) < MclslSpellSystem.MaxMana(actor) * 3 / 4
            && !actor.hasStatus("mclsl_item_D016");
        if (item.Id == "D008") return actor.hasStatus("voices_in_my_head");
        if (item.Id == "D009") return HasCureableNegativeStatus(actor);
        if (item.Id == "F024") return actor.has_attack_target && actor.attack_target != null
            && actor.attack_target.isAlive();
        if (item.Id is "F001" or "F002" or "F003" or "F004" or "F005" or "F006" or "F021" or "F022" or "F023" or "F025" or "F026" or "F027")
            return IsCurrentGradeUseful(actor, item) && !actor.hasStatus("mclsl_item_" + item.Id);
        return false;
    }

    internal static void BuildReservePlan(Actor actor, List<string> itemIds)
    {
        if (itemIds == null) return;
        itemIds.Clear();
        if (!MclslActorAccessor.Alive(actor)) return;

        string realm = MclslActorAccessor.Realm(actor);
        int gradeCap = CurrentGradeCap(actor);
        AppendIfUseful(actor, itemIds, BreakthroughPillForRealm(realm));
        AppendIfUseful(actor, itemIds, "D018");
        AppendIfUseful(actor, itemIds, "D011");
        AppendIfUseful(actor, itemIds, BestHealingPill(gradeCap));
        if (MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.taishang_taken") == 0)
            AppendIfUseful(actor, itemIds, "D004");

        for (int i = 0; i < CombatTalismanPriority.Length; i++)
        {
            int before = itemIds.Count;
            AppendIfUseful(actor, itemIds, CombatTalismanPriority[i]);
            if (itemIds.Count > before) break;
        }

        if (HasCureableNegativeStatus(actor)) AppendIfUseful(actor, itemIds, "D009");
        if (actor.hasStatus("voices_in_my_head")) AppendIfUseful(actor, itemIds, "D008");
        if (CanUseCultivationSupplement(actor)) AppendIfUseful(actor, itemIds, "D005");
        if (HasCultivationActivity(actor))
            AppendIfUseful(actor, itemIds, "F007");

    }

    internal static int ReserveCount(Actor actor, MclslItemDefinition item, List<string> plan,
        IReadOnlyDictionary<string, int> craftReserves)
    {
        if (item == null) return 0;
        if (craftReserves != null && craftReserves.TryGetValue(item.Id, out int required)) return required;
        if (IsBreakthroughPill(item.Id)) return plan != null && plan.Contains(item.Id) ? 1 : 0;
        if (item.Category is "Pill" or "Talisman" && plan != null && plan.Contains(item.Id)
            && IsCurrentGradeUseful(actor, item))
            return IsInCombat(actor) ? 4 : PerItemReserveTarget;
        if (item.Id == "D014" && HasControlStatus(actor)) return 1;
        if (item.Id == "D016" && MclslSpellSystem.CurrentMana(actor) < MclslSpellSystem.MaxMana(actor) * 3 / 4
            && !actor.hasStatus("mclsl_item_D016")) return 1;
        return plan != null && plan.Contains(item.Id) ? 1 : 0;
    }

    internal static bool IsCurrentGradeUseful(Actor actor, MclslItemDefinition item)
    {
        if (actor == null || item == null) return false;
        int realmIndex = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        if (realmIndex < 0) return item.Grade == 0;
        int gradeCap = Math.Clamp(realmIndex + 1, 1, 4);
        int minimumUsefulGrade = gradeCap == 1 ? 0 : Math.Max(1, gradeCap - 1);
        return item.Grade >= minimumUsefulGrade && item.Grade <= gradeCap;
    }

    private static bool IsBreakthroughPill(string itemId) => itemId is "D001" or "D002" or "D003" or "D012";

    private static bool IsInCombat(Actor actor)
    {
        try { return actor?.has_attack_target == true && actor.attack_target != null; }
        catch { return false; }
    }

    private static bool IsOlderThanCurrentYear(int acquiredYear, int year) => acquiredYear < 0 || year > acquiredYear;

    private static int CurrentGradeCap(Actor actor)
    {
        int realmIndex = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        return Math.Clamp(realmIndex + 1, 1, 4);
    }

    private static string BestHealingPill(int gradeCap) => gradeCap >= 3 ? "D007" : gradeCap >= 2 ? "D010" : "D006";

    private static bool CanUseCultivationSupplement(Actor actor)
    {
        int realmIndex = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        if (realmIndex < 0 || realmIndex >= MclslRealmIds.Index(MclslRealmIds.HuaShen)) return false;
        string realm = MclslActorAccessor.Realm(actor);
        bool ancient = MySimulatedLongevityRoad.Traits.MclslTraitRegistration.UsesAncientRealmTrait(actor);
        int current = MclslCultivationGrowthSystem.CurrentTrueEssence(actor);
        int cap = MclslRealmProgress.NextRealmMinimum(realm, ancient);
        if (cap <= 0 || current < cap) return true;
        return MclslActorAccessor.GetFloat(actor, MclslActorDataKeys.CultivationProgress) >= 99f
            && MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.buque_break_bonus") <= 0;
    }

    private static bool HasCultivationActivity(Actor actor)
    {
        int realmIndex = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        return realmIndex >= 0 && realmIndex < MclslRealmIds.Index(MclslRealmIds.ChangSheng);
    }

    private static void AppendIfUseful(Actor actor, List<string> itemIds, string itemId)
    {
        if (string.IsNullOrEmpty(itemId) || itemIds.Contains(itemId)) return;
        MclslItemDefinition item = MclslItemCatalog.Get(itemId);
        if (item == null) return;
        bool useful = item.Id switch
        {
            "D001" or "D002" or "D003" or "D012" => string.Equals(item.Id, BreakthroughPillForRealm(MclslActorAccessor.Realm(actor)), StringComparison.Ordinal),
            "D011" => true,
            "D004" => MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.taishang_taken") == 0,
            "D008" => actor.hasStatus("voices_in_my_head"),
            "D009" => HasCureableNegativeStatus(actor),
            "D005" => CanUseCultivationSupplement(actor),
            "F007" => HasCultivationActivity(actor),
            _ => IsCurrentGradeUseful(actor, item)
        };
        if (useful) itemIds.Add(itemId);
    }

    internal static bool HasCureableNegativeStatus(Actor actor)
    {
        if (actor == null) return false;
        foreach (string id in actor.getStatusesIds())
        {
            if (string.IsNullOrEmpty(id) || id.StartsWith("mclsl_item_", StringComparison.Ordinal)) continue;
            if (AssetManager.status.get(id)?.can_be_cured == true) return true;
        }
        return false;
    }

    internal static bool HasControlStatus(Actor actor)
    {
        if (actor == null) return false;
        foreach (string id in actor.getStatusesIds())
        {
            if (string.IsNullOrEmpty(id) || AssetManager.status.get(id)?.can_be_cured != true) continue;
            if (id.Contains("stun", StringComparison.OrdinalIgnoreCase)
                || id.Contains("slow", StringComparison.OrdinalIgnoreCase)
                || id.Contains("freeze", StringComparison.OrdinalIgnoreCase)
                || id.Contains("bind", StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }
}
