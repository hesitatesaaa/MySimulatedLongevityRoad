using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslSectSystem
{
    private static readonly List<MclslSectRecord> Snapshot = new();

    internal static void OnWorldLoaded(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        run.Sects ??= new List<MclslSectRecord>();
        for (int i = 0; i < run.Sects.Count; i++) Normalize(run.Sects[i]);
    }

    internal static void TickAnnual(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.Sects == null) return;
        bool changed = false;
        for (int i = run.Sects.Count - 1; i >= 0; i--)
        {
            MclslSectRecord sect = run.Sects[i];
            if (sect == null) { run.Sects.RemoveAt(i); changed = true; continue; }
            Normalize(sect);
            if (sect.State == "destroyed") continue;
            Actor leader = MclslNativeWorldAdapter.ResolveActor(sect.LeaderActorId);
            if (MclslActorAccessor.Alive(leader))
            {
                sect.SeatMapX = leader.data.x;
                sect.SeatMapY = leader.data.y;
                sect.MemberCountSnapshot = Math.Max(sect.MemberCountSnapshot, sect.MemberActorIds.Count);
                continue;
            }

            Actor successor = null;
            for (int j = 0; j < sect.MemberActorIds.Count; j++)
            {
                Actor candidate = MclslNativeWorldAdapter.ResolveActor(sect.MemberActorIds[j]);
                if (MclslActorAccessor.Alive(candidate)) { successor = candidate; break; }
            }
            if (successor != null)
            {
                sect.LeaderActorId = MclslActorAccessor.Id(successor);
                sect.SeatMapX = successor.data.x;
                sect.SeatMapY = successor.data.y;
                sect.LastChangedYear = year;
                MclslWorldRunRepository.AddEvent(year, "sect_leader_changed", sect.Name + "宗主更替", sect.Name + "由" + MclslActorAccessor.DisplayName(successor) + "承继宗主之位。");
                changed = true;
            }
            else
            {
                sect.State = "destroyed";
                sect.DestroyedYear = year;
                sect.LastChangedYear = year;
                for (int j = 0; j < sect.ControlledNodeIds.Count; j++)
                {
                    MclslMapNodeRecord node = MclslMapNodeSystem.Find(sect.ControlledNodeIds[j]);
                    if (node == null) continue;
                    node.OwnerKind = "none";
                    node.OwnerId = string.Empty;
                    node.OwnerNameSnapshot = string.Empty;
                    node.LifecycleState = "active";
                    node.LastChangedYear = year;
                }
                MclslWorldRunRepository.AddEvent(year, "sect_destroyed", sect.Name + "覆灭", sect.Name + "失去全部传承者，所控节点重新陷入无主之局。");
                changed = true;
            }
        }
        if (changed) MclslWorldArchiveStore.MarkDirty();
    }

    internal static MclslSectRecord EnsureForActor(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return null;
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L) return null;
        string existingId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.SectId, string.Empty);
        MclslSectRecord existing = Find(existingId);
        if (existing != null && existing.State != "destroyed")
        {
            AddMember(existing, actorId);
            return existing;
        }

        if (MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) < MclslRealmIds.Index(MclslRealmIds.JinDan)) return null;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        run.Sects ??= new List<MclslSectRecord>();
        if (run.Sects.Count >= MclslWorldRunRepository.MaxSectRecords) return null;
        string city = MclslNativeWorldAdapter.CityName(actor);
        string name = string.IsNullOrWhiteSpace(city) ? MclslActorAccessor.DisplayName(actor) + "道场" : city + "仙门";
        MclslSectRecord sect = new()
        {
            Id = "sect_" + actorId + "_" + Math.Max(0, year),
            Name = name,
            SeatMapX = actor.data.x,
            SeatMapY = actor.data.y,
            LeaderActorId = actorId,
            MainTechniqueId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty),
            FoundedYear = year,
            LastChangedYear = year,
            State = "active"
        };
        AddMember(sect, actorId);
        run.Sects.Add(sect);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SectId, sect.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SectRole, "leader");
        MclslWorldRunRepository.AddEvent(year, "sect_founded", sect.Name + "立宗", sect.Name + "以" + MclslActorAccessor.DisplayName(actor) + "为宗主，驻于" + (string.IsNullOrWhiteSpace(city) ? "无主荒域" : city) + "。");
        MclslWorldArchiveStore.MarkDirty();
        return sect;
    }

    internal static void ClaimNode(Actor actor, MclslMapNodeRecord node, int year)
    {
        if (node == null || !MclslActorAccessor.Alive(actor)) return;
        MclslSectRecord sect = EnsureForActor(actor, year);
        string ownerId = sect == null ? "actor:" + MclslActorAccessor.Id(actor) : "sect:" + sect.Id;
        string ownerName = sect?.Name ?? MclslActorAccessor.DisplayName(actor);
        MclslMapNodeSystem.MarkControlled(node.Id, ownerId, ownerName, year);
        if (sect != null && !sect.ControlledNodeIds.Contains(node.Id) && sect.ControlledNodeIds.Count < 12)
        {
            sect.ControlledNodeIds.Add(node.Id);
            sect.LastChangedYear = year;
        }
        MclslWorldRunRepository.AddEvent(year, "node_owner_changed", node.Name + "易主", ownerName + "抵达并控制了" + node.Name + "。", node.MapX, node.MapY, node.LocationName, node.NativeKingdomNameSnapshot, node.Id);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static IReadOnlyList<MclslSectRecord> SnapshotSects()
    {
        Snapshot.Clear();
        List<MclslSectRecord> sects = MclslWorldRunRepository.Current?.Sects;
        if (sects == null) return Snapshot;
        for (int i = 0; i < sects.Count; i++) if (sects[i] != null) Snapshot.Add(sects[i]);
        return Snapshot;
    }

    internal static void Clear() => Snapshot.Clear();

    private static MclslSectRecord Find(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        List<MclslSectRecord> sects = MclslWorldRunRepository.Current?.Sects;
        if (sects == null) return null;
        for (int i = 0; i < sects.Count; i++)
            if (sects[i] != null && string.Equals(sects[i].Id, id, StringComparison.Ordinal)) return sects[i];
        return null;
    }

    private static void AddMember(MclslSectRecord sect, long actorId)
    {
        if (sect == null || actorId <= 0L) return;
        sect.MemberCountSnapshot = Math.Max(sect.MemberCountSnapshot, sect.MemberActorIds.Count + 1);
        if (!sect.MemberActorIds.Contains(actorId) && sect.MemberActorIds.Count < 128) sect.MemberActorIds.Add(actorId);
    }

    private static void Normalize(MclslSectRecord sect)
    {
        if (sect == null) return;
        sect.MemberActorIds ??= new List<long>();
        sect.ControlledNodeIds ??= new List<string>();
        sect.Inventory ??= new Dictionary<string, int>();
        if (string.IsNullOrWhiteSpace(sect.State)) sect.State = "active";
        sect.LineageCompleteness = Math.Clamp(sect.LineageCompleteness <= 0 ? 100 : sect.LineageCompleteness, 0, 100);
    }
}
