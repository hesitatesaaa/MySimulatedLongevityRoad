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
        internal int TransactionDepth;
        internal bool Dirty;
        internal bool Invalid;
    }

    private static ConditionalWeakTable<Actor, CachedBag> _readCache = new();
    private static readonly MclslBagState Empty = new();

    // Read-only callers share the parsed snapshot. Mutations persist through Write.
    internal static MclslBagState Peek(Actor actor)
    {
        if (actor?.data == null) return Empty;
        string json = MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBag);
        if (string.IsNullOrWhiteSpace(json))
        {
            if (_readCache.TryGetValue(actor, out CachedBag emptyEntry) && emptyEntry.TransactionDepth > 0)
                return emptyEntry.Bag;
            return HasBackup(actor) ? GetCachedBag(actor, json, false) : Empty;
        }
        return GetCachedBag(actor, json, false);
    }

    internal static void ClearRuntime() => _readCache = new ConditionalWeakTable<Actor, CachedBag>();

    internal static bool IsLocked(Actor actor)
    {
        if (actor?.data == null) return false;
        string json = MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBag);
        if (string.IsNullOrWhiteSpace(json) && !HasBackup(actor)) return false;
        GetCachedBag(actor, json, false);
        return _readCache.TryGetValue(actor, out CachedBag entry) && entry.Invalid;
    }

    internal static MclslBagState Read(Actor actor)
    {
        if (actor?.data == null) return new MclslBagState();
        string json = MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBag);
        if (string.IsNullOrWhiteSpace(json))
        {
            if (_readCache.TryGetValue(actor, out CachedBag emptyEntry) && emptyEntry.TransactionDepth > 0)
                return emptyEntry.Bag;
            if (HasBackup(actor)) return GetCachedBag(actor, json, true);
            return new MclslBagState();
        }
        return GetCachedBag(actor, json, true);
    }

    internal static void BeginTransaction(Actor actor)
    {
        if (actor?.data == null) return;
        string json = MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBag);
        MclslBagState bag = string.IsNullOrWhiteSpace(json) && !HasBackup(actor)
            ? new MclslBagState() : GetCachedBag(actor, json, true);
        CachedBag entry = _readCache.GetOrCreateValue(actor);
        if (entry.TransactionDepth == 0) entry.Bag = bag;
        entry.TransactionDepth++;
    }

    internal static void EndTransaction(Actor actor)
    {
        if (actor?.data == null || !_readCache.TryGetValue(actor, out CachedBag entry)) return;
        entry.TransactionDepth--;
        if (entry.TransactionDepth > 0) return;
        if (entry.Dirty) Commit(actor, entry);
        else if (string.IsNullOrWhiteSpace(entry.Json)) _readCache.Remove(actor);
    }

    private static MclslBagState GetCachedBag(Actor actor, string json, bool failOnInvalid)
    {
        CachedBag entry = _readCache.GetOrCreateValue(actor);
        if (entry.Bag != null && (entry.Dirty || string.Equals(entry.Json, json, StringComparison.Ordinal)))
        {
            if (entry.Invalid && failOnInvalid) throw new InvalidOperationException("乾坤袋数据损坏，已锁定写入");
            return entry.Bag;
        }

        string backup = MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBagBackup);
        MclslRecoverySource source = MclslPrimaryBackupRecovery.Select(json, backup,
            ParseOrNull, out MclslBagState? parsed);
        if (source == MclslRecoverySource.Backup)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.QiankunBagCorrupt, json);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.QiankunBag, backup);
            json = backup;
            MclslDiagnostics.Error("qiankun-bag-recovered", "乾坤袋主数据损坏，已从有效备份恢复");
        }
        else if (source == MclslRecoverySource.Locked)
        {
            entry.Json = json;
            entry.Bag = new MclslBagState();
            entry.Invalid = true;
            entry.Dirty = false;
            MclslDiagnostics.Error("qiankun-bag-locked", "乾坤袋主数据和备份均不可读，原始数据已保留，背包写入已锁定");
            if (failOnInvalid) throw new InvalidOperationException("乾坤袋数据损坏，已锁定写入");
            return entry.Bag;
        }
        entry.Bag = parsed!;
        entry.Invalid = false;
        string normalized = Serialize(entry.Bag);
        if (!string.Equals(normalized, json, StringComparison.Ordinal))
            MclslActorAccessor.Set(actor, MclslActorDataKeys.QiankunBag, normalized);
        if (!string.Equals(MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBagBackup), normalized, StringComparison.Ordinal))
            MclslActorAccessor.Set(actor, MclslActorDataKeys.QiankunBagBackup, normalized);
        entry.Json = normalized;
        entry.Dirty = false;
        return entry.Bag;
    }

    private static bool HasBackup(Actor actor) => !string.IsNullOrWhiteSpace(
        MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBagBackup));

    private static MclslBagState? ParseOrNull(string json)
        => TryParse(json, out MclslBagState parsed) ? parsed : null;

    private static bool TryParse(string json, out MclslBagState parsed)
    {
        parsed = null;
        long sample = MclslPerformanceProbe.Begin();
        try
        {
            MclslBagState bag = JsonConvert.DeserializeObject<MclslBagState>(json);
            if (bag == null) throw new JsonSerializationException("背包根对象为空");
            bag.Items ??= new();
            bag.EquippedArtifactSlots ??= new Dictionary<string, string>();
            bag.Books ??= new List<MclslMentorshipBook>();
            bag.Version = 2;
            List<MclslOwnedItem> normalized = new(bag.Items.Count);
            Dictionary<string, MclslOwnedItem> stacks = new(StringComparer.Ordinal);
            HashSet<string> artifactInstanceIds = new(StringComparer.Ordinal);
            foreach (MclslOwnedItem owned in bag.Items)
            {
                MclslItemDefinition definition = MclslItemCatalog.Get(owned?.ItemId);
                if (owned == null || definition == null || owned.Count < 1)
                    throw new JsonSerializationException("背包包含无法识别或数量无效的物品，已保留原文");
                int count = Math.Clamp(owned.Count, 1, 999999);
                if (definition.Category == "Artifact")
                {
                    for (int i = 0; i < count; i++)
                    {
                        string instanceId = i == 0 ? owned.InstanceId : string.Empty;
                        if (string.IsNullOrWhiteSpace(instanceId) || !artifactInstanceIds.Add(instanceId))
                        {
                            do { instanceId = Guid.NewGuid().ToString("N"); }
                            while (!artifactInstanceIds.Add(instanceId));
                        }
                        normalized.Add(new MclslOwnedItem
                        {
                            ItemId = definition.Id,
                            InstanceId = instanceId,
                            Count = 1,
                            Durability = Math.Clamp(owned.Durability <= 0 ? 1 : owned.Durability, 1, 100),
                            AcquiredYear = owned.AcquiredYear
                        });
                    }
                    continue;
                }

                string stackKey = definition.Id + "|" + owned.AcquiredYear;
                if (!stacks.TryGetValue(stackKey, out MclslOwnedItem stack))
                {
                    stack = new MclslOwnedItem { ItemId = definition.Id, Count = count, AcquiredYear = owned.AcquiredYear };
                    stacks[stackKey] = stack;
                    normalized.Add(stack);
                }
                else
                {
                    stack.Count = Math.Min(999999, stack.Count + count);
                }
            }
            bag.Items = normalized;
            parsed = bag;
            return true;
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("qiankun-bag-load", "乾坤袋读取失败: " + ex.Message);
            return false;
        }
        finally { MclslPerformanceProbe.End("乾坤袋JSON反序列化", sample); }
    }

    private static string Serialize(MclslBagState bag)
    {
        long sample = MclslPerformanceProbe.Begin();
        try { return JsonConvert.SerializeObject(bag); }
        finally { MclslPerformanceProbe.End("乾坤袋JSON序列化", sample); }
    }

    internal static void Write(Actor actor, MclslBagState bag)
    {
        if (actor?.data == null || bag == null) return;
        string current = MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBag);
        if (!string.IsNullOrWhiteSpace(current) || HasBackup(actor)) GetCachedBag(actor, current, true);
        CachedBag entry = _readCache.GetOrCreateValue(actor);
        if (entry.Invalid) throw new InvalidOperationException("乾坤袋数据损坏，已锁定写入");
        entry.Bag = bag;
        entry.Dirty = true;
        if (entry.TransactionDepth == 0) Commit(actor, entry);
    }

    private static void Commit(Actor actor, CachedBag entry)
    {
        if (entry.Invalid) throw new InvalidOperationException("乾坤袋数据损坏，已锁定写入");
        string json = Serialize(entry.Bag);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.QiankunBagBackup, json);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.QiankunBag, json);
        entry.Json = json;
        entry.Dirty = false;
    }

    internal static int Count(Actor actor, string itemId) => Count(Peek(actor), itemId);

    internal static int Count(MclslBagState bag, string itemId)
    {
        string canonicalId = MclslItemCatalog.NormalizeId(itemId);
        if (canonicalId.Length == 0) return 0;
        int total = 0;
        foreach (MclslOwnedItem item in bag.Items)
            if (item.ItemId == canonicalId) total += item.Count;
        return total;
    }

    internal static void Add(Actor actor, string itemId, int count = 1, int acquiredYear = -1)
    {
        if (actor?.data == null || MclslItemCatalog.Get(itemId) == null || count <= 0) return;
        MclslBagState bag = Read(actor);
        Add(bag, itemId, count, acquiredYear: acquiredYear);
        Write(actor, bag);
        if (MclslItemCatalog.Get(itemId)?.Category == "Artifact") MclslArtifactSystem.OnArtifactAcquired(actor);
    }

    internal static void Add(MclslBagState bag, string itemId, int count = 1, int durability = 100, int acquiredYear = -1)
    {
        if (bag == null || count <= 0) return;
        MclslItemDefinition definition = MclslItemCatalog.Get(itemId);
        if (definition == null) return;
        itemId = definition.Id;
        int acquiredAt = acquiredYear >= 0 ? acquiredYear : MclslRuntime.CurrentYear();
        if (definition.Category == "Artifact")
        {
            for (int i = 0; i < count; i++)
                bag.Items.Add(new MclslOwnedItem { ItemId = itemId, InstanceId = Guid.NewGuid().ToString("N"), Durability = Math.Clamp(durability, 1, 100), AcquiredYear = acquiredAt });
            return;
        }
        MclslOwnedItem stack = bag.Items.FirstOrDefault(x => x.ItemId == itemId && x.AcquiredYear == acquiredAt);
        if (stack == null) bag.Items.Add(new MclslOwnedItem { ItemId = itemId, Count = count, AcquiredYear = acquiredAt });
        else
        {
            stack.Count = (int)Math.Min(999999L, (long)stack.Count + count);
        }
    }

    internal static bool Remove(MclslBagState bag, string itemId, int count = 1)
    {
        itemId = MclslItemCatalog.NormalizeId(itemId);
        if (bag == null || count <= 0 || Count(bag, itemId) < count) return false;
        while (count > 0)
        {
            MclslOwnedItem item = bag.Items.Where(x => x.ItemId == itemId)
                .OrderBy(x => x.AcquiredYear).FirstOrDefault();
            if (item == null) return false;
            int taken = Math.Min(item.Count, count);
            item.Count -= taken;
            count -= taken;
            if (item.Count <= 0) bag.Items.Remove(item);
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
            int elapsedYears = Math.Max(0, MclslRuntime.CurrentYear() - year);
            int ageAtYear = Math.Max(0, (int)Math.Floor((double)actor.getAge()) - elapsedYears);
            if (ageAtYear >= 18)
            {
                MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionChecked, 1);
                if (string.IsNullOrEmpty(profession) && MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude) > 0)
                {
                    int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|career|18") % 10000;
                    int a = Math.Clamp(MclslRuntimeSettings.AlchemistChanceBasisPoints, 0, 10000);
                    int r = Math.Clamp(MclslRuntimeSettings.RefinerChanceBasisPoints, 0, 10000 - a);
                    int t = Math.Clamp(MclslRuntimeSettings.TalismanChanceBasisPoints, 0, 10000 - a - r);
                    profession = roll < a ? Alchemist : roll < a + r ? Refiner : roll < a + r + t ? TalismanMaker : string.Empty;
                    if (!string.IsNullOrEmpty(profession))
                    {
                        MclslActorAccessor.Set(actor, MclslActorDataKeys.Profession, profession);
                        MclslRecipeKnowledge.SeedNewProfession(actor, profession);
                    }
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
        MclslRecipeKnowledge.EnsureUsableRecipe(actor, profession,
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade));
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionLastCraftYear, -1) == year)
        {
            RefreshActorInfoIfProfessionChanged(actor, previousProfession, previousGrade);
            return;
        }
        if (TryCraft(actor, profession, year)) MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionLastCraftYear, year);
        RefreshActorInfoIfProfessionChanged(actor, previousProfession, previousGrade);
    }

    internal static string FromTrait(Actor actor)
    {
        if (actor.hasTrait(MclslTraitRegistration.AlchemistTraitId)) return Alchemist;
        if (actor.hasTrait(MclslTraitRegistration.RefinerTraitId)) return Refiner;
        if (actor.hasTrait(MclslTraitRegistration.TalismanTraitId)) return TalismanMaker;
        return string.Empty;
    }

    internal static int RequiredCraftMaterialCount(Actor actor, string itemId, int year = -1)
    {
        (string first, string second) = RequiredCraftMaterialPair(actor, year, MclslBagSystem.Peek(actor));
        int required = 0;
        if (string.Equals(first, itemId, StringComparison.Ordinal)) required++;
        if (string.Equals(second, itemId, StringComparison.Ordinal)) required++;
        return required;
    }

    internal static List<MclslItemDefinition> KnownCraftRecipes(Actor actor, MclslBagState bag)
    {
        string profession = FromTrait(actor);
        if (string.IsNullOrWhiteSpace(profession))
            profession = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession, string.Empty);
        string category = MclslRecipeKnowledge.CategoryForProfession(profession);
        int grade = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade);
        List<MclslItemDefinition> recipes = new();
        if (category.Length == 0 || grade < 0) return recipes;
        foreach (MclslItemDefinition item in MclslItemCatalog.All)
        {
            if (item.Category != category || !MclslProfessionCraftingPolicy.CanAttemptRecipe(grade, item.Grade)
                || !RecipeFitsGrade(item, item.Grade)
                || string.IsNullOrWhiteSpace(item.IngredientA) || string.IsNullOrWhiteSpace(item.IngredientB)
                || !MclslRecipeKnowledge.Knows(actor, item.Id)) continue;
            recipes.Add(item);
        }
        recipes.Sort((left, right) =>
        {
            int byGrade = right.Grade.CompareTo(left.Grade);
            return byGrade != 0 ? byGrade : string.Compare(left.Id, right.Id, StringComparison.Ordinal);
        });
        return recipes;
    }

    internal static void BuildCraftMaterialReserves(Actor actor, MclslBagState bag, Dictionary<string, int> reserves)
    {
        reserves.Clear();
        string profession = FromTrait(actor);
        if (string.IsNullOrWhiteSpace(profession))
            profession = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession, string.Empty);
        if (profession is not (Alchemist or Refiner or TalismanMaker)) return;
        foreach (MclslItemDefinition recipe in KnownCraftRecipes(actor, bag))
        {
            if (MclslItemCatalog.Get(recipe.IngredientA) is { } first)
                reserves[first.Id] = Math.Max(reserves.TryGetValue(first.Id, out int a) ? a : 0,
                    recipe.IngredientA == recipe.IngredientB ? 2 : 1);
            if (MclslItemCatalog.Get(recipe.IngredientB) is { } second)
                reserves[second.Id] = Math.Max(reserves.TryGetValue(second.Id, out int b) ? b : 0,
                    recipe.IngredientA == recipe.IngredientB ? 2 : 1);
        }
        int grade = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade);
        if (grade > 0)
        {
            foreach (MclslItemDefinition material in MclslItemCatalog.All)
            {
                if (IsProfessionMaterial(profession, material, grade))
                    reserves[material.Id] = Math.Max(reserves.TryGetValue(material.Id, out int count) ? count : 0, 1);
            }
            return;
        }
        (string practice, _) = RequiredCraftMaterialPair(actor, MclslRuntime.CurrentYear(), bag);
        if (MclslItemCatalog.Get(practice) != null) reserves[practice] = 1;
    }

    internal static bool IsProfessionMaterial(string profession, MclslItemDefinition material, int grade)
    {
        if (material == null || grade <= 0 || material.MaterialTier == MclslMaterialTier.None
            || (int)material.MaterialTier > grade) return false;
        return profession == Alchemist && material.Category is "Plant" or "SpiritObject"
            || profession == TalismanMaker && material.Category == "TalismanMaterial"
            || profession == Refiner && material.Category == "Material";
    }

    internal static bool RecipeFitsGrade(MclslItemDefinition recipe, int grade)
    {
        if (recipe == null || grade < 0) return false;
        // Grade zero is a separate apprentice economy: only explicit WorldBox
        // town resources are valid, never Huang/Xuan/Di/Tian mod materials or
        // an unknown id that happens to be absent from the item catalog.
        if (grade == 0)
            return IsNativeIngredient(recipe.IngredientA) && IsNativeIngredient(recipe.IngredientB);
        return IngredientFitsGrade(recipe.IngredientA, grade)
            && IngredientFitsGrade(recipe.IngredientB, grade);
    }

    private static bool IngredientFitsGrade(string itemId, int grade)
    {
        MclslItemDefinition material = MclslItemCatalog.Get(itemId);
        return material != null && material.MaterialTier != MclslMaterialTier.None
            && (int)material.MaterialTier <= grade;
    }

    internal static bool IsNativeIngredient(string itemId) => !string.IsNullOrEmpty(NativeResource(itemId));

    internal static (string First, string Second) RequiredCraftMaterialPair(Actor actor, int year, MclslBagState bag)
    {
        string profession = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession);
        if (string.IsNullOrWhiteSpace(profession)) profession = FromTrait(actor);
        int grade = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade);
        string category = profession == Alchemist ? "Pill"
            : profession == TalismanMaker ? "Talisman"
            : profession == Refiner ? "Artifact" : string.Empty;
        if (category.Length == 0) return (string.Empty, string.Empty);
        MclslItemDefinition recipe = PreferredRecipe(actor, category, grade,
            year < 0 ? MclslRuntime.CurrentYear() : year, bag);
        return recipe == null ? (string.Empty, string.Empty) : (recipe.IngredientA, recipe.IngredientB);
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
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionGrade, grade);
            MclslRecipeKnowledge.SeedGrade(actor, MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession), grade);
        }
    }

    private static void RefreshActorInfoIfProfessionChanged(Actor actor, string previousProfession, int previousGrade)
    {
        if (actor?.data == null) return;
        if (string.Equals(previousProfession, MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession), StringComparison.Ordinal)
            && previousGrade == MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade)) return;
        MclslActorInfoPanel.RefreshOpenForActor(actor);
    }

    private static bool TryCraft(Actor actor, string profession, int year)
    {
        if (MclslBagSystem.IsLocked(actor)) return false;
        int grade = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade);
        MclslBagState bag = MclslBagSystem.Read(actor);
        string category = profession == Alchemist ? "Pill" : profession == TalismanMaker ? "Talisman" : "Artifact";
        MclslItemDefinition item = PreferredRecipe(actor, category, grade, year, bag);
        if (item != null && HasPair(actor, bag, item.IngredientA, item.IngredientB))
        {
            if (!ConsumeIngredient(actor, bag, item.IngredientA, 1)) return false;
            if (!ConsumeIngredient(actor, bag, item.IngredientB, 1)) return false;
            MclslBagSystem.Add(bag, item.Id, acquiredYear: year);
            MclslBagSystem.Write(actor, bag);
            if (item.Category == "Artifact") MclslArtifactSystem.OnArtifactAcquired(actor);
            MclslWorldRunRepository.AddItemAcquisitionEvent(year, actor, item.Id, 1, "职业制作");
            int experience = MclslProfessionCraftingPolicy.ExperienceGain(grade, item.Grade);
            if (experience > 0)
            {
                GainExperience(actor, experience);
                Promote(actor);
            }
            return true;
        }
        return false;
    }

    private static MclslItemDefinition PreferredRecipe(Actor actor, string category, int grade, int year, MclslBagState bag)
    {
        int minimumGrade = MclslProfessionCraftingPolicy.MinimumRecipeGrade(grade);
        if (minimumGrade < 0) return null;
        long id = Math.Max(0L, MclslActorAccessor.Id(actor));
        MclslItemDefinition fallback = null;
        for (int recipeGrade = grade; recipeGrade >= minimumGrade; recipeGrade--)
        {
            MclslItemDefinition[] recipes = MclslItemCatalog.All.Where(x => x.Category == category
                && x.Grade == recipeGrade && MclslRecipeKnowledge.Knows(actor, x.Id)
                && RecipeFitsGrade(x, recipeGrade)
                && !string.IsNullOrWhiteSpace(x.IngredientA) && !string.IsNullOrWhiteSpace(x.IngredientB))
                .OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            if (recipes.Length == 0) continue;
            int start = (int)((id + Math.Max(0, year)) % recipes.Length);
            for (int i = 0; i < recipes.Length; i++)
            {
                MclslItemDefinition recipe = recipes[(start + i) % recipes.Length];
                fallback ??= recipe;
                if (HasPair(actor, bag, recipe.IngredientA, recipe.IngredientB)) return recipe;
            }
        }
        return fallback;
    }

    private static void GainExperience(Actor actor, int amount) => MclslActorAccessor.Set(actor,
        MclslActorDataKeys.ProfessionExperience,
        MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionExperience) + Math.Max(0, amount));

    private static bool HasIngredient(Actor actor, MclslBagState bag, string id)
    {
        if (!id.StartsWith("R", StringComparison.Ordinal)) return MclslBagSystem.Count(bag, id) > 0;
        string native = NativeResource(id);
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
        "R05" => "mythril", "R06" => "adamantine", "R07" => "herbs", "R08" => "berries",
        "R09" => "wheat", _ => string.Empty
    };

    private static bool ConsumeIngredient(Actor actor, MclslBagState bag, string id, int count)
    {
        if (!HasIngredient(actor, bag, id)) return false;
        if (!id.StartsWith("R", StringComparison.Ordinal)) return MclslBagSystem.Remove(bag, id, count);
        string native = NativeResource(id);
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
