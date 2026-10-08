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
        internal long Revision;
        internal int CompactedYear = -1;
        internal readonly Dictionary<string, MclslOwnedItem> MatureStacks = new(StringComparer.Ordinal);
        internal readonly Dictionary<(string Item, int Durability), MclslOwnedItem> ArtifactStacks = new();
    }

    private sealed class BagIndex
    {
        internal long Revision = long.MinValue;
        internal readonly Dictionary<string, long> Counts = new(StringComparer.Ordinal);
        internal readonly Dictionary<string, int> Instances = new(StringComparer.Ordinal);
    }
    private static ConditionalWeakTable<MclslBagState, BagIndex> Indexes = new();
    private static BagIndex Indexed(MclslBagState bag)
    {
        BagIndex index = Indexes.GetOrCreateValue(bag);
        if (index.Revision == bag.MutationVersion) return index;
        index.Counts.Clear(); index.Instances.Clear();
        for (int i = 0; i < bag.Items.Count; i++)
        {
            MclslOwnedItem owned = bag.Items[i];
            index.Counts.TryGetValue(owned.ItemId, out long count); index.Counts[owned.ItemId] = count + owned.Count;
            if (!string.IsNullOrEmpty(owned.InstanceId)) index.Instances[owned.InstanceId] = i;
        }
        index.Revision = bag.MutationVersion;
        return index;
    }
    internal static MclslOwnedItem FindInstance(MclslBagState bag, string instanceId)
        => bag != null && instanceId != null && Indexed(bag).Instances.TryGetValue(instanceId, out int at) ? bag.Items[at] : null;
    private static readonly Dictionary<long, CachedBag> _readCache = new();
    private static readonly MclslChangeQueue PendingWrites = new();
    private static CachedBag Cache(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        if (!_readCache.TryGetValue(id, out CachedBag entry)) _readCache[id] = entry = new CachedBag();
        return entry;
    }
    private static string ReadStoredJson(Actor actor)
    {
        ((BaseSystemData)actor.data).get(MclslActorDataKeys.QiankunBag, out string value, string.Empty);
        return value ?? string.Empty;
    }
    internal static long Revision(Actor actor) => actor?.data == null ? 0 : Cache(actor).Revision;
    internal static int CacheCount => _readCache.Count;
    internal static int PendingWriteCount => PendingWrites.Count;
    private static readonly MclslBagState Empty = new();

    internal static MclslBagState Peek(Actor actor)
        => actor?.data == null ? Empty : GetCachedBag(actor, ReadStoredJson(actor), false);
    internal static bool IsLocked(Actor actor)
    {
        if (actor?.data == null) return false;
        Peek(actor);
        return Cache(actor).Invalid;
    }
    internal static MclslBagState Read(Actor actor)
        => actor?.data == null ? new MclslBagState() : GetCachedBag(actor, ReadStoredJson(actor), true);

    internal static MclslBagState Copy(MclslBagState source)
    {
        MclslBagState copy = new()
        {
            Version = source.Version,
            MutationVersion = source.MutationVersion,
            EquippedArtifactSlots = new Dictionary<string, string>(source.EquippedArtifactSlots),
            Books = new List<MclslMentorshipBook>(source.Books)
        };
        foreach (MclslOwnedItem item in source.Items)
            copy.Items.Add(new MclslOwnedItem { ItemId = item.ItemId, InstanceId = item.InstanceId,
                Count = item.Count, Durability = item.Durability, AcquiredYear = item.AcquiredYear });
        return copy;
    }
    internal static void ClearRuntime() { _readCache.Clear(); PendingWrites.Clear(); Indexes = new(); }
    internal static void Forget(long id) { _readCache.Remove(id); PendingWrites.Cancel(id); }
    internal static void BeginTransaction(Actor actor)
    {
        if (actor?.data == null) return;
        Read(actor); Cache(actor).TransactionDepth++;
    }
    internal static void EndTransaction(Actor actor)
    {
        if (actor?.data == null || !_readCache.TryGetValue(MclslActorAccessor.Id(actor), out CachedBag entry)) return;
        if (entry.TransactionDepth <= 0) throw new InvalidOperationException("Unbalanced inventory transaction");
        entry.TransactionDepth--;
        if (entry.TransactionDepth == 0 && entry.Dirty) Commit(actor, entry);
    }
    internal static void FlushPending(int budget = 8)
    {
        for (int i = 0; i < budget && !MclslFrameDeadline.Expired; i++)
        {
            if (!PendingWrites.TryTake(out long id, out _)) break;
            if (MclslActorRegistry.Resolve(id, out Actor actor)) FlushActor(actor);
            else Forget(id);
        }
    }
    internal static void FlushForSave()
    {
        foreach (KeyValuePair<long, CachedBag> pair in _readCache)
            if (pair.Value.Dirty && MclslActorRegistry.Resolve(pair.Key, out Actor actor)) FlushActor(actor);
        PendingWrites.Clear();
    }
    internal static string SnapshotJson(Actor actor)
    {
        if (actor?.data == null) return string.Empty;
        FlushActor(actor);
        return ReadStoredJson(actor);
    }
    private static void FlushActor(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        if (!_readCache.TryGetValue(id, out CachedBag entry) || !entry.Dirty) return;
        if (entry.Invalid) throw new InvalidOperationException("乾坤袋数据损坏，已锁定写入");
        string json = Serialize(entry.Bag);
        // Persistence writes do not publish another gameplay change.
        ((BaseSystemData)actor.data).set(MclslActorDataKeys.QiankunBagBackup, json);
        ((BaseSystemData)actor.data).set(MclslActorDataKeys.QiankunBag, json);
        entry.Json = json; entry.Dirty = false; PendingWrites.Cancel(id);
    }

    private static MclslBagState GetCachedBag(Actor actor, string json, bool failOnInvalid)
    {
        CachedBag entry = Cache(actor);
        if (entry.Bag != null && (entry.Dirty || string.Equals(entry.Json, json, StringComparison.Ordinal)))
        {
            if (entry.Invalid && failOnInvalid) throw new InvalidOperationException("乾坤袋数据损坏，已锁定写入");
            return entry.Bag;
        }

        if (string.IsNullOrWhiteSpace(json) && !HasBackup(actor))
        {
            entry.Bag = new MclslBagState(); entry.Json = string.Empty; entry.Invalid = false;
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
        entry.Revision++;
        entry.Bag = parsed!;
        entry.Invalid = false;
        entry.Json = json;
        entry.Dirty = false;
        return entry.Bag;
    }

    internal static void CompactMatureStacks(Actor actor, int year)
    {
        if (actor?.data == null) return;
        MclslBagState bag = Peek(actor);
        CachedBag cache = Cache(actor);
        if (cache.Invalid || cache.CompactedYear == year) return;
        cache.CompactedYear = year;
        bool changed = MclslInventoryDataRules.CompactMatureStacks(bag.Items, year, cache.MatureStacks, bag, cache.ArtifactStacks);
        if (changed) Write(actor, bag);
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
            if (!MclslInventoryDataRules.IsValid(bag))
                throw new JsonSerializationException("背包版本、物品实例或装备引用无效");
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
        string current = ReadStoredJson(actor);
        if (!string.IsNullOrWhiteSpace(current) || HasBackup(actor)) GetCachedBag(actor, current, true);
        CachedBag entry = Cache(actor);
        if (entry.Invalid) throw new InvalidOperationException("乾坤袋数据损坏，已锁定写入");
        bag.MutationVersion++;
        entry.Bag = bag;
        entry.Revision++;
        entry.Dirty = true;
        if (entry.TransactionDepth == 0) Commit(actor, entry);
    }

    private static void Commit(Actor actor, CachedBag entry)
    {
        if (entry.Invalid) throw new InvalidOperationException("乾坤袋数据损坏，已锁定写入");
        entry.Dirty = true;
        PendingWrites.Publish(MclslActorAccessor.Id(actor), 1);
        MclslRuntimeChanges.Publish(actor, MclslActorChange.Inventory);
    }

    internal static int Count(Actor actor, string itemId) => Count(Peek(actor), itemId);

    internal static int Count(MclslBagState bag, string itemId)
    {
        string canonicalId = MclslItemCatalog.NormalizeId(itemId);
        if (canonicalId.Length == 0) return 0;
        long total = Indexed(bag).Counts.TryGetValue(canonicalId, out long count) ? count : 0;
        return (int)Math.Min(int.MaxValue, total);
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
        bag.MutationVersion++;
        int acquiredAt = acquiredYear >= 0 ? acquiredYear : MclslRuntime.CurrentYear();
        if (definition.Category == "Artifact")
        {
            int health = Math.Clamp(durability, 1, 100);
            for (int i = 0; i < bag.Items.Count && count > 0; i++)
            {
                MclslOwnedItem held = bag.Items[i];
                if (held.ItemId != itemId || held.Durability != health || held.AcquiredYear != acquiredAt
                    || MclslInventoryDataRules.IsEquipped(bag, held)) continue;
                int added = Math.Min(int.MaxValue - held.Count, count);
                held.Count += added; count -= added;
            }
            if (count > 0) bag.Items.Add(new MclslOwnedItem { ItemId = itemId, InstanceId = Guid.NewGuid().ToString("N"),
                Count = count, Durability = health, AcquiredYear = acquiredAt });
            return;
        }
        MclslOwnedItem stack = null;
        for (int i = 0; i < bag.Items.Count; i++)
            if (bag.Items[i].ItemId == itemId && bag.Items[i].AcquiredYear == acquiredAt) { stack = bag.Items[i]; break; }
        if (stack == null) bag.Items.Add(new MclslOwnedItem { ItemId = itemId, Count = count, AcquiredYear = acquiredAt });
        else
        {
            int added = Math.Min(int.MaxValue - stack.Count, count);
            stack.Count += added;
            if (added < count) bag.Items.Add(new MclslOwnedItem { ItemId = itemId, Count = count - added, AcquiredYear = acquiredAt });
        }
    }

    internal static bool Remove(MclslBagState bag, string itemId, int count = 1)
    {
        itemId = MclslItemCatalog.NormalizeId(itemId);
        if (bag == null || count <= 0 || Count(bag, itemId) < count) return false;
        bag.MutationVersion++;
        while (count > 0)
        {
            MclslOwnedItem item = null;
            for (int i = 0; i < bag.Items.Count; i++)
            {
                MclslOwnedItem candidate = bag.Items[i];
                if (candidate.ItemId == itemId && (item == null || candidate.AcquiredYear < item.AcquiredYear))
                    item = candidate;
            }
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

    internal static bool IsProfessionTrait(string traitId) => traitId is
        MclslTraitRegistration.AlchemistTraitId or MclslTraitRegistration.RefinerTraitId or MclslTraitRegistration.TalismanTraitId;

    // Correct old saves on the authoritative read. Keep experience for future promotion.
    internal static int GetGrade(Actor actor)
    {
        if (actor?.data == null) return 0;
        return GetGrade(actor, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
    }

    private static int GetGrade(Actor actor, int realmIndex)
    {
        int stored = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade, 0);
        int grade = MclslProfessionCraftingPolicy.ClampGrade(stored, realmIndex);
        if (grade != stored) MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionGrade, grade);
        return grade;
    }

    internal static void ReconcileAndPromote(Actor actor)
    {
        if (actor?.data == null) return;
        int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        int grade = GetGrade(actor, realm);
        if (grade >= MclslProfessionCraftingPolicy.MaximumGrade(realm)) return;
        string profession = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession);
        if (string.IsNullOrEmpty(profession)) profession = FromTrait(actor);
        if (string.IsNullOrEmpty(profession)) return;
        int experience = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionExperience);
        int promoted = MclslProfessionCraftingPolicy.PromotedGrade(grade, experience, realm);
        if (promoted == grade) return;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionGrade, promoted);
        MclslRecipeKnowledge.SeedGrade(actor, profession, promoted);
        MclslActorInfoPanel.RefreshOpenForActor(actor);
    }

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
        RefreshActorInfoIfProfessionChanged(actor, previous, MclslProfessionSystem.GetGrade(actor));
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
        int previousGrade = MclslProfessionSystem.GetGrade(actor);
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
        long identitySample = MclslPerformanceProbe.Begin();
        try
        {
            SyncTrait(actor, profession);
        }
        finally { MclslPerformanceProbe.End("职业.身份与晋升", identitySample); }
        long knowledgeSample = MclslPerformanceProbe.Begin();
        try { MclslRecipeKnowledge.EnsureUsableRecipe(actor, profession,
            MclslProfessionSystem.GetGrade(actor)); }
        finally { MclslPerformanceProbe.End("职业.配方知识", knowledgeSample); }
        if (!MclslProfessionCraftingPolicy.CanCraftInYear(year,
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionLastCraftYear, -1)))
        {
            RefreshActorInfoIfProfessionChanged(actor, previousProfession, previousGrade);
            return;
        }
        long craftSample = MclslPerformanceProbe.Begin();
        try { TryCraft(actor, profession, year); }
        finally { MclslPerformanceProbe.End("职业.制作提交", craftSample); }
        RefreshActorInfoIfProfessionChanged(actor, previousProfession, previousGrade);
    }

    internal static string FromTrait(Actor actor)
    {
        if (actor.hasTrait(MclslTraitRegistration.AlchemistTraitId)) return Alchemist;
        if (actor.hasTrait(MclslTraitRegistration.RefinerTraitId)) return Refiner;
        if (actor.hasTrait(MclslTraitRegistration.TalismanTraitId)) return TalismanMaker;
        return string.Empty;
    }

    internal static IReadOnlyList<MclslItemDefinition> KnownCraftRecipes(Actor actor, MclslBagState bag)
    {
        string profession = FromTrait(actor);
        if (string.IsNullOrWhiteSpace(profession))
            profession = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession, string.Empty);
        string category = MclslRecipeKnowledge.CategoryForProfession(profession);
        int grade = MclslProfessionSystem.GetGrade(actor);
        return MclslRecipeKnowledge.KnownCraftRecipes(actor, category, grade);
    }

    internal static void BuildCraftMaterialReserves(Actor actor, MclslBagState bag,
        Dictionary<string, int> reserves, int year = 0)
    {
        reserves.Clear();
        string profession = FromTrait(actor);
        if (string.IsNullOrWhiteSpace(profession))
            profession = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession, string.Empty);
        if (profession is not (Alchemist or Refiner or TalismanMaker)) return;
        MclslItemDefinition recipe = PlannedRecipe(actor, bag, year <= 0 ? MclslRuntime.CurrentYear() : year);
        if (recipe == null) return;
        if (MclslItemCatalog.Get(recipe.IngredientA) is { } first)
            reserves[first.Id] = recipe.IngredientA == recipe.IngredientB ? 2 : 1;
        if (MclslItemCatalog.Get(recipe.IngredientB) is { } second)
            reserves[second.Id] = recipe.IngredientA == recipe.IngredientB ? 2 : 1;
    }

    private static MclslItemDefinition PlannedRecipe(Actor actor, MclslBagState bag, int year)
    {
        IReadOnlyList<MclslItemDefinition> recipes = KnownCraftRecipes(actor, bag);
        if (recipes.Count == 0) return null;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.InventoryPlanYear, -1) == year)
        {
            string selectedId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.InventoryPlanRecipeId, string.Empty);
            MclslItemDefinition selected = recipes.FirstOrDefault(x => x.Id == selectedId);
            return selected != null && RecipeStillNeeded(actor, bag, selected) ? selected : null;
        }
        List<string> needs = new();
        MclslConsumableInventoryPolicy.BuildReservePlan(actor, needs);
        int start = (int)(unchecked((ulong)MclslActorAccessor.Id(actor)) % (uint)recipes.Count);
        MclslItemDefinition planned = null;
        for (int offset = 0; offset < recipes.Count; offset++)
        {
            MclslItemDefinition recipe = recipes[(start + offset) % recipes.Count];
            int owned = MclslBagSystem.Count(bag, recipe.Id);
            int personalTarget = recipe.Category == "Artifact" ? 1
                : MclslConsumableInventoryPolicy.ReserveCount(actor, recipe, needs, null);
            if (MclslEconomicPolicy.ShouldProduce(owned, personalTarget,
                MclslTianxuanMarket.ActiveItemCount(recipe.Id))) { planned = recipe; break; }
        }
        MclslActorAccessor.Set(actor, MclslActorDataKeys.InventoryPlanRecipeId, planned?.Id ?? string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.InventoryPlanYear, year);
        return planned;
    }

    private static bool RecipeStillNeeded(Actor actor, MclslBagState bag, MclslItemDefinition recipe)
    {
        List<string> needs = new();
        MclslConsumableInventoryPolicy.BuildReservePlan(actor, needs);
        int owned = MclslBagSystem.Count(bag, recipe.Id);
        int personalTarget = recipe.Category == "Artifact" ? 1
            : MclslConsumableInventoryPolicy.ReserveCount(actor, recipe, needs, null);
        return MclslEconomicPolicy.ShouldProduce(owned, personalTarget,
            MclslTianxuanMarket.ActiveItemCount(recipe.Id));
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
        int grade = MclslProfessionSystem.GetGrade(actor);
        string category = profession == Alchemist ? "Pill"
            : profession == TalismanMaker ? "Talisman"
            : profession == Refiner ? "Artifact" : string.Empty;
        if (category.Length == 0) return (string.Empty, string.Empty);
        MclslItemDefinition recipe = PlannedRecipe(actor, bag,
            year < 0 ? MclslRuntime.CurrentYear() : year)
            ?? PreferredRecipe(actor, category, grade,
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

    private static void RefreshActorInfoIfProfessionChanged(Actor actor, string previousProfession, int previousGrade)
    {
        if (actor?.data == null) return;
        if (string.Equals(previousProfession, MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession), StringComparison.Ordinal)
            && previousGrade == MclslProfessionSystem.GetGrade(actor)) return;
        MclslActorInfoPanel.RefreshOpenForActor(actor);
    }

    private static bool TryCraft(Actor actor, string profession, int year)
    {
        if (MclslBagSystem.IsLocked(actor) || !MclslProfessionCraftingPolicy.CanCraftInYear(year,
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionLastCraftYear, -1))) return false;
        int grade = MclslProfessionSystem.GetGrade(actor);
        MclslBagState bag = MclslBagSystem.Read(actor);
        string category = profession == Alchemist ? "Pill" : profession == TalismanMaker ? "Talisman" : "Artifact";
        long recipeSample = MclslPerformanceProbe.Begin();
        MclslItemDefinition item;
        try { item = PlannedRecipe(actor, bag, year); }
        finally { MclslPerformanceProbe.End("职业.选方", recipeSample); }
        if (item != null && HasPair(actor, bag, item.IngredientA, item.IngredientB))
        {
            if (!ConsumeIngredient(actor, bag, item.IngredientA, 1)) return false;
            if (!ConsumeIngredient(actor, bag, item.IngredientB, 1)) return false;
            MclslBagSystem.Add(bag, item.Id, acquiredYear: year);
            MclslBagSystem.Write(actor, bag);
            // Mark delivery before optional history/equipment callbacks so an
            // exception there cannot let the same year's recipe run twice.
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionLastCraftYear, year);
            int experience = MclslProfessionCraftingPolicy.ExperienceGain(grade, item.Grade);
            if (experience > 0)
            {
                GainExperience(actor, experience);
            }
            if (item.Category == "Artifact") MclslArtifactSystem.OnArtifactAcquired(actor);
            MclslWorldRunRepository.AddItemAcquisitionEvent(year, actor, item.Id, 1, "职业制作");
            return true;
        }
        if (item == null) return TryPractice(actor, year, bag);
        return false;
    }

    private static bool TryPractice(Actor actor, int year, MclslBagState bag)
    {
        IReadOnlyList<MclslItemDefinition> recipes = KnownCraftRecipes(actor, bag);
        for (int i = 0; i < recipes.Count; i++)
        {
            MclslItemDefinition recipe = recipes[i];
            if (!HasPair(actor, bag, recipe.IngredientA, recipe.IngredientB)) continue;
            if (!ConsumeIngredient(actor, bag, recipe.IngredientA, 1)
                || !ConsumeIngredient(actor, bag, recipe.IngredientB, 1))
                throw new InvalidOperationException("职业练习材料扣除未完成");
            MclslBagSystem.Write(actor, bag);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ProfessionLastCraftYear, year);
            int experience = MclslProfessionCraftingPolicy.ExperienceGain(GetGrade(actor), recipe.Grade);
            GainExperience(actor, MclslEconomicPolicy.PracticeExperience(experience));
            return true;
        }
        return false;
    }

    private static MclslItemDefinition PreferredRecipe(Actor actor, string category, int grade, int year, MclslBagState bag)
    {
        if (MclslProfessionCraftingPolicy.MinimumRecipeGrade(grade) < 0) return null;
        long id = Math.Max(0L, MclslActorAccessor.Id(actor));
        MclslItemDefinition fallback = null;
        MclslItemDefinition[] recipes = MclslRecipeKnowledge.CraftCandidates(actor, category, grade);
        if (recipes.Length == 0) return null;
        int start = (int)((id + Math.Max(0, year)) % recipes.Length);
        for (int i = 0; i < recipes.Length; i++)
        {
            MclslItemDefinition recipe = recipes[(start + i) % recipes.Length];
            fallback ??= recipe;
            if (HasPair(actor, bag, recipe.IngredientA, recipe.IngredientB)) return recipe;
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
