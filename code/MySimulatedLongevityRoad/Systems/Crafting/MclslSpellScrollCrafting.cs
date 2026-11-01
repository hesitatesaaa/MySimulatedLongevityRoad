using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslSpellScrollCrafting
{
    internal static IReadOnlyDictionary<string, int> Recipe(MclslSpellDefinition spell)
    {
        if (spell == null) return new Dictionary<string, int>();
        return MclslEconomicPolicy.ScrollRecipe(spell.MinRealm, spell.Law);
    }

    internal static void ProgressAnnual(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        string intent = MclslActorAccessor.GetString(actor, MclslActorDataKeys.SpellScrollIntent, string.Empty);
        int intentYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpellScrollIntentYear, -1);
        if (!string.IsNullOrEmpty(intent) && intentYear < year && MclslSpellSystem.TryGet(intent, out MclslSpellDefinition pending))
        {
            if (TryCraft(actor, pending, year)) intent = string.Empty;
        }
        if (string.IsNullOrEmpty(intent) && StableRoll(actor, year) < 6)
        {
            List<MclslSpellDefinition> priorities = MclslSpellProgression.PracticePriorities(actor);
            if (priorities.Count == 0) priorities = MclslSpellSystem.Known(actor);
            if (priorities.Count > 0) intent = priorities[StableRoll(actor, year + 17) % priorities.Count].Id;
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellScrollIntentYear, year);
        }
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellScrollIntent, intent);
    }

    internal static bool TryCraft(Actor actor, MclslSpellDefinition spell, int year)
    {
        if (!MclslSpellSystem.Knows(actor, spell.Id)
            || MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpellScrollLastCraftYear, -1) == year) return false;
        MclslBagState bag = MclslBagSystem.Read(actor);
        IReadOnlyDictionary<string, int> recipe = Recipe(spell);
        foreach (KeyValuePair<string, int> entry in recipe)
            if (MclslBagSystem.Count(bag, entry.Key) < entry.Value) return false;
        foreach (KeyValuePair<string, int> entry in recipe) MclslBagSystem.Remove(bag, entry.Key, entry.Value);
        MclslBagSystem.Add(bag, spell.Id + "_SCROLL", acquiredYear: year);
        MclslBagSystem.Write(actor, bag);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellScrollLastCraftYear, year);
        return true;
    }

    internal static void AddMissingPurchaseNeeds(Actor actor, MclslBagState bag, List<MclslMarketPurchaseNeed> needs)
    {
        string intent = MclslActorAccessor.GetString(actor, MclslActorDataKeys.SpellScrollIntent, string.Empty);
        if (!MclslSpellSystem.TryGet(intent, out MclslSpellDefinition spell) || !MclslSpellSystem.Knows(actor, intent)) return;
        foreach (KeyValuePair<string, int> entry in Recipe(spell))
        {
            int missing = Math.Max(0, entry.Value - MclslBagSystem.Count(bag, entry.Key));
            if (missing <= 0) continue;
            MclslMarketPurchaseNeed existing = needs.Find(need => need.ItemId == entry.Key);
            if (existing == null) needs.Add(new MclslMarketPurchaseNeed { ItemId = entry.Key, Kind = "Craft", Remaining = missing });
            else existing.Remaining = Math.Max(existing.Remaining, missing);
        }
    }

    internal static void AddMaterialReserves(Actor actor, Dictionary<string, int> reserves)
    {
        string intent = MclslActorAccessor.GetString(actor, MclslActorDataKeys.SpellScrollIntent, string.Empty);
        if (!MclslSpellSystem.TryGet(intent, out MclslSpellDefinition spell) || !MclslSpellSystem.Knows(actor, intent)) return;
        foreach (KeyValuePair<string, int> entry in Recipe(spell))
            reserves[entry.Key] = Math.Max(reserves.TryGetValue(entry.Key, out int current) ? current : 0, entry.Value);
    }

    private static int StableRoll(Actor actor, int year)
    {
        unchecked
        {
            ulong value = (ulong)MclslActorAccessor.Id(actor) + (uint)year * 0x9e3779b97f4a7c15UL;
            value ^= value >> 33; value *= 0xff51afd7ed558ccdUL; value ^= value >> 33;
            return (int)(value % 100UL);
        }
    }
}
