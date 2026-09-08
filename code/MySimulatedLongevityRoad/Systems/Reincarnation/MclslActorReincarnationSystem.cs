using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslActorReincarnationSystem
{
    private const int MaxApplicationsPerYear = 6;
    private const int MaxPendingRecords = 80;

    internal static void RecordFromDeath(Actor actor, MclslDeathRecord death)
    {
        if (actor?.data == null || death == null) return;
        if (!MclslInverseTruthSystem.IsTruthReversed("truth_player_reincarnation_unbroken")) return;
        int realmIndex = MclslRealmIds.Index(death.RealmId);
        if (realmIndex < MclslRealmIds.Index(MclslRealmIds.JinDan)) return;
        int chance = ReincarnationChance(death.RealmId);
        int roll = PositiveHash(death.ActorId + "|reincarnation|" + death.Year) % 100;
        if (roll >= chance) return;

        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null) return;
        run.ReincarnationRecords ??= new List<MclslActorReincarnationRecord>();
        PrunePending(run.ReincarnationRecords);

        string id = "reincarnation_" + death.ActorId + "_" + death.Year;
        for (int i = 0; i < run.ReincarnationRecords.Count; i++)
            if (string.Equals(run.ReincarnationRecords[i]?.Id, id, StringComparison.Ordinal)) return;

        run.ReincarnationRecords.Add(new MclslActorReincarnationRecord
        {
            Id = id,
            SourceActorId = death.ActorId,
            SourceActorName = death.ActorName,
            SourceRealmId = death.RealmId,
            SourceRealmName = death.RealmName,
            SourceTechniqueName = death.TechniqueName,
            RaceKey = actor.asset?.id ?? string.Empty,
            DeathYear = death.Year,
            RootPurityFloor = RootPurityFloor(death.RealmId),
            Status = "待转"
        });
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static bool TryApplyAnnual(int year, IReadOnlyList<Actor> actors)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.ReincarnationRecords == null || run.ReincarnationRecords.Count == 0 || actors == null || actors.Count == 0) return false;

        bool changed = false;
        int applied = 0;
        for (int i = 0; i < run.ReincarnationRecords.Count && applied < MaxApplicationsPerYear; i++)
        {
            MclslActorReincarnationRecord record = run.ReincarnationRecords[i];
            if (record == null || !string.Equals(record.Status, "待转", StringComparison.Ordinal)) continue;
            Actor target = PickTarget(record, actors, year);
            if (target == null) continue;
            Apply(record, target, year);
            applied++;
            changed = true;
        }

        if (changed) MclslWorldArchiveStore.MarkDirty();
        return changed;
    }

    private static Actor PickTarget(MclslActorReincarnationRecord record, IReadOnlyList<Actor> actors, int year)
    {
        if (actors.Count == 0) return null;
        int start = PositiveHash(record.Id + "|target|" + year) % actors.Count;
        for (int i = 0; i < actors.Count; i++)
        {
            Actor actor = actors[(start + i) % actors.Count];
            if (!CanReceive(actor, record)) continue;
            return actor;
        }
        return null;
    }

    private static bool CanReceive(Actor actor, MclslActorReincarnationRecord record)
    {
        if (!MclslActorAccessor.Alive(actor) || !MclslEligibility.CanCultivate(actor)) return false;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ReincarnationApplied, 0) == 1) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor))) return false;
        int age = SafeAge(actor);
        if (age > 5) return false;
        string race = actor.asset?.id ?? string.Empty;
        return string.IsNullOrWhiteSpace(record.RaceKey) || string.IsNullOrWhiteSpace(race) || string.Equals(record.RaceKey, race, StringComparison.Ordinal);
    }

    private static void Apply(MclslActorReincarnationRecord record, Actor actor, int year)
    {
        long targetId = MclslActorAccessor.Id(actor);
        int currentAptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0), 0, 100);
        int seedBonus = PositiveHash(record.Id + "|purity|" + targetId) % 7;
        int aptitude = Math.Clamp(Math.Max(currentAptitude, record.RootPurityFloor + seedBonus), 1, 100);

        MclslActorAccessor.Set(actor, MclslActorDataKeys.ReincarnationApplied, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ReincarnationSourceActorId, record.SourceActorId);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ReincarnationSourceName, record.SourceActorName);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ReincarnationSourceRealm, record.SourceRealmId);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ReincarnationDeathYear, record.DeathYear);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ReincarnationTechniqueName, record.SourceTechniqueName);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, aptitude);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ImmortalFate, Math.Clamp(aptitude + 8, 20, 100));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.MindState, Math.Clamp(60 + MclslRealmIds.Index(record.SourceRealmId) * 4, 50, 95));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "前世余烬入梦，灵根早显");
        MclslTraitRegistration.SyncGiftTrait(actor, aptitude);

        record.TargetActorId = targetId;
        record.TargetActorName = MclslActorAccessor.DisplayName(actor);
        record.AppliedYear = year;
        record.Status = "已转";

        if (!MclslSpiritualRootEntrySystem.TryEnterFromGiftTrait(actor, year))
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ChildhoodRootChecked, 1);
        MclslCultivationWake.EnsureAwake(
            actor,
            ensureEntryFromGift: true,
            enqueueAnnual: true,
            refreshUi: false);

        string sourceRealm = string.IsNullOrWhiteSpace(record.SourceRealmName) ? MclslRealmIds.Display(record.SourceRealmId) : record.SourceRealmName;
        MclslWorldRunRepository.AddEvent(year, "reincarnation_unbroken", MclslActorAccessor.DisplayName(actor) + "承前世而生",
            "前世：" + record.SourceActorName + "，旧境：" + sourceRealm + "。");
    }

    private static void PrunePending(List<MclslActorReincarnationRecord> records)
    {
        if (records == null || records.Count <= MaxPendingRecords) return;
        int removeCount = records.Count - MaxPendingRecords;
        for (int i = records.Count - 1; i >= 0 && removeCount > 0; i--)
        {
            if (records[i] == null || string.Equals(records[i].Status, "待转", StringComparison.Ordinal))
            {
                records.RemoveAt(i);
                removeCount--;
            }
        }
    }

    private static int ReincarnationChance(string realmId)
    {
        return realmId switch
        {
            MclslRealmIds.JinDan => 18,
            MclslRealmIds.YuanYing => 28,
            MclslRealmIds.HuaShen => 38,
            MclslRealmIds.HeDao => 55,
            MclslRealmIds.ChangSheng => 70,
            _ => 0
        };
    }

    private static int RootPurityFloor(string realmId)
    {
        return realmId switch
        {
            MclslRealmIds.JinDan => 70,
            MclslRealmIds.YuanYing => 78,
            MclslRealmIds.HuaShen => 86,
            MclslRealmIds.HeDao => 92,
            MclslRealmIds.ChangSheng => 96,
            _ => 55
        };
    }

    private static int SafeAge(Actor actor)
    {
        try { return Math.Max(0, (int)Math.Floor(actor?.getAge() ?? 0f)); }
        catch { return 0; }
    }

    private static int PositiveHash(string value)
    {
        unchecked
        {
            int hash = 113;
            foreach (char c in value ?? string.Empty) hash = hash * 41 + c;
            return hash & int.MaxValue;
        }
    }
}
