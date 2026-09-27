using System.Collections.Generic;

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
    public int Version { get; set; } = 2;
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
