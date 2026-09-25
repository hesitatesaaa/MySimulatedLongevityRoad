using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.UI;
using Newtonsoft.Json;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslBagSystem
{
    private sealed class CachedBag
    {
        internal string Json;
        internal MclslBagState Bag;
    }

    private static ConditionalWeakTable<Actor, CachedBag> _readCache = new();
    private static readonly MclslBagState Empty = new();

    // Read-only callers may share the parsed object. Mutations must still use Read + Write.
    internal static MclslBagState Peek(Actor actor)
    {
        if (actor?.data == null) return Empty;
        string json = MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBag);
        if (string.IsNullOrWhiteSpace(json)) return Empty;
        CachedBag entry = _readCache.GetOrCreateValue(actor);
        if (entry.Bag == null || !string.Equals(entry.Json, json, StringComparison.Ordinal))
        {
            entry.Bag = Parse(json);
            entry.Json = json;
        }
        return entry.Bag;
    }

    internal static void ClearRuntime() => _readCache = new ConditionalWeakTable<Actor, CachedBag>();

    internal static MclslBagState Read(Actor actor)
    {
        string json = MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBag);
        return string.IsNullOrWhiteSpace(json) ? new MclslBagState() : Parse(json);
    }

    private static MclslBagState Parse(string json)
    {
        try
        {
            MclslBagState bag = JsonConvert.DeserializeObject<MclslBagState>(json) ?? new();
            bag.Items ??= new();
            bag.Items.RemoveAll(x => x == null || MclslItemCatalog.Get(x.ItemId) == null || x.Count < 1);
            return bag;
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("qiankun-bag-load", "乾坤袋读取失败: " + ex.Message);
            return new();
        }
    }

    internal static void Write(Actor actor, MclslBagState bag)
    {
        if (actor?.data == null || bag == null) return;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.QiankunBag, JsonConvert.SerializeObject(bag));
        _readCache.Remove(actor);
    }

    internal static int Count(Actor actor, string itemId) => Count(Peek(actor), itemId);

    internal static int Count(MclslBagState bag, string itemId)
    {
        int total = 0;
        foreach (MclslOwnedItem item in bag.Items)
            if (item.ItemId == itemId) total += item.Count;
        return total;
    }

    internal static void Add(Actor actor, string itemId, int count = 1)
    {
        if (actor?.data == null || MclslItemCatalog.Get(itemId) == null || count <= 0) return;
        MclslBagState bag = Read(actor);
        Add(bag, itemId, count);
        Write(actor, bag);
    }

    internal static void Add(MclslBagState bag, string itemId, int count = 1, int durability = 100)
    {
        if (bag == null || count <= 0) return;
        MclslItemDefinition definition = MclslItemCatalog.Get(itemId);
        if (definition == null) return;
        if (definition.Category == "Artifact")
        {
            for (int i = 0; i < count; i++)
                bag.Items.Add(new MclslOwnedItem { ItemId = itemId, InstanceId = Guid.NewGuid().ToString("N"), Durability = Math.Clamp(durability, 1, 100) });
            return;
        }
        MclslOwnedItem stack = bag.Items.FirstOrDefault(x => x.ItemId == itemId);
        if (stack == null) bag.Items.Add(new MclslOwnedItem { ItemId = itemId, Count = count });
        else stack.Count = Math.Min(999999, stack.Count + count);
    }

    internal static bool Remove(MclslBagState bag, string itemId, int count = 1)
    {
        if (bag == null || count <= 0 || Count(bag, itemId) < count) return false;
        for (int i = bag.Items.Count - 1; i >= 0 && count > 0; i--)
        {
            MclslOwnedItem item = bag.Items[i];
            if (item.ItemId != itemId) continue;
            int taken = Math.Min(item.Count, count);
            item.Count -= taken;
            count -= taken;
            if (item.Count <= 0) bag.Items.RemoveAt(i);
        }
        return count == 0;
    }
}

internal static class MclslProfessionSystem
{
    internal const string Alchemist = "alchemist";
    internal const string Refiner = "refiner";
    internal const string TalismanMaker = "talisman";
    private static readonly string[] ProfessionTraitIds =
        { MclslTraitRegistration.AlchemistTraitId, MclslTraitRegistration.RefinerTraitId, MclslTraitRegistration.TalismanTraitId };
    private static readonly int[] PromotionThresholds = { 10, 30, 80, 160 };

    internal static bool IsProfessionTrait(string traitId) => traitId is
        MclslTraitRegistration.AlchemistTraitId or MclslTraitRegistration.RefinerTraitId or MclslTraitRegistration.TalismanTraitId;

    internal static void OnTraitGranted(Actor actor, string traitId)
    {
        string previous = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession);
        string profession = traitId == MclslTraitRegistration.AlchemistTraitId ? Alchemist
            : traitId == MclslTraitRegistration.RefinerTraitId ? Refiner : TalismanMaker;
        if (!string.IsNullOrEmpty(previous) && previous != profession)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionGrade, 0);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionExperience, 0);
        }
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Profession, profession);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionChecked, 1);
        SyncTrait(actor, profession);
        RefreshActorInfoIfProfessionChanged(actor, previous, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade));
    }

    internal static void OnTraitRemoved(Actor actor, string traitId)
    {
        string profession = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession);
        if ((profession == Alchemist && traitId == MclslTraitRegistration.AlchemistTraitId)
            || (profession == Refiner && traitId == MclslTraitRegistration.RefinerTraitId)
            || (profession == TalismanMaker && traitId == MclslTraitRegistration.TalismanTraitId))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.Profession, string.Empty);
            MclslActorInfoPanel.RefreshOpenForActor(actor);
        }
    }

    internal static void ProcessAnnual(Actor actor, int year)
    {
        if (actor?.data == null || !MclslActorAccessor.Alive(actor)) return;
        string previousProfession = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession);
        int previousGrade = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade);
        string profession = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession);
        if (string.IsNullOrEmpty(profession))
        {
            profession = FromTrait(actor);
            if (!string.IsNullOrEmpty(profession))
                MclslActorAccessor.Set(actor, MclslActorDataKeys.Profession, profession);
        }
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionChecked) == 0)
        {
            int age = Math.Max(0, (int)Math.Floor((double)actor.getAge()));
            if (age > 18) MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionChecked, 1);
            else if (age == 18)
            {
                MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionChecked, 1);
                if (string.IsNullOrEmpty(profession) && MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude) > 0)
                {
                    int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|career|18") % 10000;
                    int a = Math.Clamp(MclslRuntimeSettings.AlchemistChanceBasisPoints, 0, 10000);
                    int r = Math.Clamp(MclslRuntimeSettings.RefinerChanceBasisPoints, 0, 10000 - a);
                    int t = Math.Clamp(MclslRuntimeSettings.TalismanChanceBasisPoints, 0, 10000 - a - r);
                    profession = roll < a ? Alchemist : roll < a + r ? Refiner : roll < a + r + t ? TalismanMaker : string.Empty;
                    if (!string.IsNullOrEmpty(profession)) MclslActorAccessor.Set(actor, MclslActorDataKeys.Profession, profession);
                }
            }
        }
        if (string.IsNullOrEmpty(profession))
        {
            RefreshActorInfoIfProfessionChanged(actor, previousProfession, previousGrade);
            return;
        }
        SyncTrait(actor, profession);
        Promote(actor);
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionLastCraftYear, -1) == year)
        {
            RefreshActorInfoIfProfessionChanged(actor, previousProfession, previousGrade);
            return;
        }
        if (TryCraft(actor, profession)) MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionLastCraftYear, year);
        RefreshActorInfoIfProfessionChanged(actor, previousProfession, previousGrade);
    }

    internal static string FromTrait(Actor actor)
    {
        if (actor.hasTrait(MclslTraitRegistration.AlchemistTraitId)) return Alchemist;
        if (actor.hasTrait(MclslTraitRegistration.RefinerTraitId)) return Refiner;
        if (actor.hasTrait(MclslTraitRegistration.TalismanTraitId)) return TalismanMaker;
        return string.Empty;
    }

    private static void SyncTrait(Actor actor, string profession)
    {
        string selected = profession == Alchemist ? ProfessionTraitIds[0] : profession == Refiner ? ProfessionTraitIds[1] : ProfessionTraitIds[2];
        foreach (string id in ProfessionTraitIds)
        {
            if (id != selected && actor.hasTrait(id)) actor.removeTrait(id);
        }
        if (!actor.hasTrait(selected))
        {
            ActorTrait trait = AssetManager.traits.get(selected);
            if (trait != null) actor.addTrait(trait, true);
        }
    }

    private static void Promote(Actor actor)
    {
        int grade = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade);
        int experience = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionExperience);
        int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        int initialGrade = grade;
        while (grade < 4 && experience >= PromotionThresholds[grade] && realm >= grade)
            grade++;
        if (grade != initialGrade)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionGrade, grade);
    }

    private static void RefreshActorInfoIfProfessionChanged(Actor actor, string previousProfession, int previousGrade)
    {
        if (actor?.data == null) return;
        if (string.Equals(previousProfession, MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession), StringComparison.Ordinal)
            && previousGrade == MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade)) return;
        MclslActorInfoPanel.RefreshOpenForActor(actor);
    }

    private static bool TryCraft(Actor actor, string profession)
    {
        int grade = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade);
        if (profession != Refiner && string.IsNullOrEmpty(MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBag)))
            return false;
        MclslBagState bag = MclslBagSystem.Read(actor);
        if (grade == 0)
        {
            string practice = profession == Alchemist ? "A08" : profession == TalismanMaker ? "F01" : "R01";
            if (!ConsumeIngredient(actor, bag, practice, 1)) return false;
            MclslBagSystem.Write(actor, bag);
            GainExperience(actor);
            Promote(actor);
            return true;
        }
        string category = profession == Alchemist ? "Pill" : profession == TalismanMaker ? "Talisman" : "Artifact";
        foreach (MclslItemDefinition item in MclslItemCatalog.All)
        {
            if (item.Category != category || item.Grade != grade) continue;
            if (!HasPair(actor, bag, item.IngredientA, item.IngredientB)) continue;
            if (!ConsumeIngredient(actor, bag, item.IngredientA, 1)) continue;
            if (!ConsumeIngredient(actor, bag, item.IngredientB, 1)) continue;
            MclslBagSystem.Add(bag, item.Id);
            MclslBagSystem.Write(actor, bag);
            if (item.Category == "Artifact") MclslArtifactSystem.TryEquipBest(actor);
            GainExperience(actor);
            Promote(actor);
            return true;
        }
        return false;
    }

    private static void GainExperience(Actor actor) => MclslActorAccessor.Set(actor,
        MclslActorDataKeys.ProfessionExperience,
        MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionExperience) + 1);

    private static bool HasIngredient(Actor actor, MclslBagState bag, string id)
    {
        if (!id.StartsWith("R", StringComparison.Ordinal)) return MclslBagSystem.Count(bag, id) > 0;
        string native = id switch { "R01" => "wood", "R02" => "stone", "R03" => "common_metals", "R04" => "common_metals", "R05" => "mythril", "R06" => "adamantine", _ => string.Empty };
        return !string.IsNullOrEmpty(native) && actor.city?.getResourcesAmount(native) > 0;
    }

    private static bool HasPair(Actor actor, MclslBagState bag, string first, string second)
    {
        if (!HasIngredient(actor, bag, first) || !HasIngredient(actor, bag, second)) return false;
        if (first == second) return first.StartsWith("R", StringComparison.Ordinal)
            ? actor.city?.getResourcesAmount(NativeResource(first)) >= 2
            : MclslBagSystem.Count(bag, first) >= 2;
        if (first.StartsWith("R", StringComparison.Ordinal) && second.StartsWith("R", StringComparison.Ordinal)
            && NativeResource(first) == NativeResource(second))
            return actor.city?.getResourcesAmount(NativeResource(first)) >= 2;
        return true;
    }

    private static string NativeResource(string id) => id switch
    {
        "R01" => "wood", "R02" => "stone", "R03" => "common_metals", "R04" => "common_metals",
        "R05" => "mythril", "R06" => "adamantine", _ => string.Empty
    };

    private static bool ConsumeIngredient(Actor actor, MclslBagState bag, string id, int count)
    {
        if (!HasIngredient(actor, bag, id)) return false;
        if (!id.StartsWith("R", StringComparison.Ordinal)) return MclslBagSystem.Remove(bag, id, count);
        string native = id switch { "R01" => "wood", "R02" => "stone", "R03" => "common_metals", "R04" => "common_metals", "R05" => "mythril", "R06" => "adamantine", _ => string.Empty };
        actor.city?.takeResource(native, count);
        return true;
    }

    private static int PositiveHash(string value)
    {
        unchecked
        {
            int hash = 29;
            foreach (char c in value) hash = hash * 43 + c;
            return hash & int.MaxValue;
        }
    }
}
