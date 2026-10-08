using System.Collections.Generic;
using System;

namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslOwnedItem
{
    public string ItemId { get; set; } = string.Empty;
    public string InstanceId { get; set; } = string.Empty;
    public int Count { get; set; } = 1;
    public int Durability { get; set; }
    public int AcquiredYear { get; set; } = -1;
}

internal sealed class MclslBagState
{
    internal long MutationVersion;
    public int Version { get; set; } = MclslSaveVersions.Inventory;
    public List<MclslOwnedItem> Items { get; set; } = new();
    // Slot values reference InstanceId entries in Items; they never own/copy artifacts.
    public Dictionary<string, string> EquippedArtifactSlots { get; set; } = new();
    public List<MclslMentorshipBook> Books { get; set; } = new();
}

internal sealed class MclslMentorshipBook
{
    public string BookId { get; set; } = string.Empty;
    public long TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TechniqueId { get; set; } = string.Empty;
    public string TechniqueName { get; set; } = string.Empty;
    public long TargetId { get; set; }
    public int StartYear { get; set; }
    public int Progress { get; set; }
}

internal sealed class MclslMarketListing
{
    public string ListingId { get; set; } = string.Empty;
    public long SellerId { get; set; }
    public MclslOwnedItem Item { get; set; } = new();
    public int Price { get; set; }
    public int ContributionUnitPrice { get; set; }
    public int SpiritStoneUnitPrice { get; set; }
    public int Year { get; set; }
}

internal sealed class MclslMarketActivity
{
    // Listed records are created when a cultivator offers an item. Purchased
    // records capture both sides after an atomic market settlement.
    public string Action { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public long ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public long OtherActorId { get; set; }
    public string OtherActorName { get; set; } = string.Empty;
    public int Count { get; set; } = 1;
    public int Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int UnitPrice { get; set; }
    public int TotalPrice { get; set; }
    public int Year { get; set; }
}

internal sealed class MclslMarketPriceState
{
    public int StonePrice { get; set; }
    public int LastUpdatedYear { get; set; }
    public int FiveYearSupply { get; set; }
    public int FiveYearFundedDemand { get; set; }
    public Dictionary<int, int> SupplyByYear { get; set; } = new();
    public Dictionary<int, int> FundedDemandByYear { get; set; } = new();
}

internal static class MclslInventoryDataRules
{
    internal static bool IsValid(MclslBagState bag)
    {
        if (bag == null || bag.Version != MclslSaveVersions.Inventory || bag.Items == null
            || bag.EquippedArtifactSlots == null || bag.Books == null) return false;
        Dictionary<string, (MclslItemDefinition Definition, int Count)> artifacts = new(StringComparer.Ordinal);
        foreach (MclslOwnedItem owned in bag.Items)
        {
            MclslItemDefinition definition = MclslItemCatalog.Get(owned?.ItemId);
            if (owned == null || definition == null || owned.ItemId != definition.Id || owned.Count < 1) return false;
            if (definition.Category != "Artifact") continue;
            if (string.IsNullOrWhiteSpace(owned.InstanceId)
                || artifacts.ContainsKey(owned.InstanceId) || owned.Durability < 1 || owned.Durability > 100) return false;
            artifacts.Add(owned.InstanceId, (definition, owned.Count));
        }
        HashSet<string> assigned = new(StringComparer.Ordinal);
        foreach (var pair in bag.EquippedArtifactSlots)
        {
            if (!Enum.TryParse(pair.Key, false, out MclslArtifactEquipmentSlot slot)
                || !artifacts.TryGetValue(pair.Value, out var owned)
                || owned.Count != 1 || owned.Definition.EquipmentSlot != slot || !assigned.Add(pair.Value)) return false;
        }
        foreach (MclslMentorshipBook book in bag.Books)
            if (book == null || book.Progress < 0 || book.Progress > 100) return false;
        return true;
    }

    internal static bool IsEquipped(MclslBagState bag, MclslOwnedItem item)
    {
        foreach (string id in bag.EquippedArtifactSlots.Values) if (id == item.InstanceId) return true;
        return false;
    }
    internal static MclslOwnedItem SingleArtifact(MclslOwnedItem stack) => new()
    {
        ItemId = stack.ItemId, InstanceId = Guid.NewGuid().ToString("N"), Count = 1,
        Durability = stack.Durability, AcquiredYear = stack.AcquiredYear
    };
    internal static MclslOwnedItem MaterializeArtifact(MclslBagState bag, MclslOwnedItem stack)
    {
        if (stack.Count == 1) return stack;
        MclslOwnedItem instance = SingleArtifact(stack);
        stack.Count--; bag.Items.Add(instance); bag.MutationVersion++;
        return instance;
    }

    internal static bool IsPreferredInstance(MclslBagState bag, MclslOwnedItem next, MclslOwnedItem previous)
    {
        bool nextEquipped = false, previousEquipped = false;
        foreach (string id in bag.EquippedArtifactSlots.Values)
        { if (id == next.InstanceId) nextEquipped = true; if (id == previous.InstanceId) previousEquipped = true; }
        if (nextEquipped != previousEquipped) return nextEquipped;
        if (next.Durability != previous.Durability) return next.Durability > previous.Durability;
        return string.CompareOrdinal(next.InstanceId, previous.InstanceId) < 0;
    }

    internal static MclslOwnedItem FindOldest(IReadOnlyList<MclslOwnedItem> items, string itemId)
    {
        MclslOwnedItem oldest = null;
        for (int i = 0; i < items.Count; i++)
        {
            MclslOwnedItem item = items[i];
            if (item.ItemId == itemId && item.Count > 0 && (oldest == null || item.AcquiredYear < oldest.AcquiredYear)) oldest = item;
        }
        return oldest;
    }

    internal static bool CompactMatureStacks(List<MclslOwnedItem> items, int year,
        Dictionary<string, MclslOwnedItem> stacks, MclslBagState bag = null,
        Dictionary<(string Item, int Durability), MclslOwnedItem> artifactStacks = null)
    {
        stacks.Clear(); artifactStacks?.Clear();
        int write = 0;
        bool changed = false;
        for (int read = 0; read < items.Count; read++)
        {
            MclslOwnedItem owned = items[read];
            bool artifact = MclslItemCatalog.Get(owned.ItemId)?.Category == "Artifact";
            bool matureStack = owned.AcquiredYear < year && !artifact;
            if (artifact && owned.AcquiredYear < year && artifactStacks != null && bag != null && !IsEquipped(bag, owned))
            {
                var key = (owned.ItemId, owned.Durability);
                if (artifactStacks.TryGetValue(key, out MclslOwnedItem before))
                {
                    int moved = Math.Min(int.MaxValue - before.Count, owned.Count);
                    before.Count += moved; owned.Count -= moved;
                    before.AcquiredYear = Math.Min(before.AcquiredYear, owned.AcquiredYear);
                    changed |= moved > 0;
                    if (owned.Count == 0) continue;
                }
                artifactStacks[key] = owned;
            }
            if (matureStack && stacks.TryGetValue(owned.ItemId, out MclslOwnedItem existing))
            {
                int moved = Math.Min(int.MaxValue - existing.Count, owned.Count);
                existing.Count += moved;
                existing.AcquiredYear = Math.Min(existing.AcquiredYear, owned.AcquiredYear);
                owned.Count -= moved;
                changed |= moved > 0;
                if (owned.Count == 0) continue;
            }
            if (matureStack) stacks[owned.ItemId] = owned;
            items[write++] = owned;
        }
        if (write < items.Count) items.RemoveRange(write, items.Count - write);
        stacks.Clear(); artifactStacks?.Clear();
        if (changed && bag != null) bag.MutationVersion++;
        return changed;
    }
}
