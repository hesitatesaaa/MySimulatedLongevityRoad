using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslFamilyRecord
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public long FounderActorId { get; set; }
    public long NativeFamilyId { get; set; }
    public long HeadActorId { get; set; }
    public long PreferredHeadActorId { get; set; }
    public long FocusActorId { get; set; }
    public int LastSettledYear { get; set; }
    public int LastAidYear { get; set; }
    public long SettlementDuesStones { get; set; }
    public bool Extinct { get; set; }
    public bool EstateLiquidated { get; set; }
    public List<MclslFamilyMemberRecord> Members { get; set; } = new();
    public List<MclslOwnedItem> Items { get; set; } = new();
    public List<string> TechniqueIds { get; set; } = new();
    public Dictionary<long, int> CityInfluence { get; set; } = new();
    public List<string> History { get; set; } = new();
}

internal sealed class MclslFamilyMemberRecord
{
    public long ActorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public long ParentId1 { get; set; }
    public long ParentId2 { get; set; }
    public int Sex { get; set; }
    public int Generation { get; set; } = 1;
    public int RealmIndex { get; set; }
    public int Aptitude { get; set; }
    public long CityId { get; set; }
    public bool Alive { get; set; } = true;
    public bool DeathSettled { get; set; }
    public int LastDuesYear { get; set; }
}

internal static class MclslFamilyTransferPolicy
{
    internal static MclslOwnedItem CopyItem(MclslOwnedItem item) => new()
    {
        ItemId = item.ItemId, InstanceId = item.InstanceId, Count = item.Count,
        Durability = item.Durability, AcquiredYear = item.AcquiredYear
    };

    internal static MclslOwnedItem TakeOne(List<MclslOwnedItem> source, MclslOwnedItem held)
    {
        if (held == null || held.Count <= 0 || !source.Contains(held)) return null;
        MclslOwnedItem moved = CopyItem(held);
        moved.Count = 1;
        if (held.Count > 1)
        {
            held.Count--;
            if (!string.IsNullOrWhiteSpace(moved.InstanceId)) moved.InstanceId = Guid.NewGuid().ToString("N");
        }
        else source.Remove(held);
        return moved;
    }
}
