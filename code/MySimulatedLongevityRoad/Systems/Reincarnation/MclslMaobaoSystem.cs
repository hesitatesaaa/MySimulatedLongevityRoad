using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 猫宝是按需工作的时序留影册。候选人只在玩家打开或刷新界面时从现有修士索引中选出，
/// 留名记录最多二十四条；平时没有 Update、年度扫描或额外文件。
/// </summary>
internal static class MclslMaobaoSystem
{
    internal const int MaxRecords = MclslMaobaoArchiveManager.SavedActorLimit;
    internal const int MaxCandidates = 12;

    internal static List<MclslMaobaoCandidate> BuildCandidates()
    {
        MclslWorldRunRepository.EnsureCurrentRun(MclslRuntime.CurrentYear());
        List<Actor> leaders = new(MaxCandidates);
        IReadOnlyList<Actor> indexed = MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot();
        for (int i = 0; i < indexed.Count; i++)
        {
            Actor actor = indexed[i];
            if (!MclslActorAccessor.Alive(actor)) continue;
            InsertLeader(leaders, actor);
        }

        List<MclslMaobaoCandidate> result = new(leaders.Count);
        for (int i = 0; i < leaders.Count; i++)
        {
            Actor actor = leaders[i];
            MclslMaobaoCandidate candidate = BuildCandidate(actor);
            if (candidate != null) result.Add(candidate);
        }
        return result;
    }

    internal static bool TryRecord(MclslMaobaoCandidate candidate, out string message)
    {
        if (candidate?.Actor == null || !MclslActorAccessor.Alive(candidate.Actor))
        {
            message = "猫宝未能照见这名修士。";
            return false;
        }
        bool wasSaved = MclslMaobaoArchiveManager.IsActorSaved(candidate.Actor);
        if (!MclslMaobaoArchiveManager.SaveActor(candidate.Actor, out message)) return false;
        MclslTraitRegistration.TryAutoFavoriteMaobaoInscription(candidate.Actor);
        if (wasSaved && string.IsNullOrWhiteSpace(message)) message = "已刷新猫宝中的完整人物快照。";
        return true;
    }

    internal static bool TryRecordActor(Actor actor, out string message)
    {
        MclslMaobaoCandidate candidate = BuildCandidate(actor);
        return TryRecord(candidate, out message);
    }

    /// <summary>
    /// 人物窗口快捷“登名”按钮使用的切换入口。
    /// 未留名时立即记录当前人物，已留名时再次点击移除该人物记录，
    /// 与玄鉴仙族登名石的单击行为保持一致。
    /// </summary>
    internal static bool ToggleRecordActor(Actor actor, out string message)
    {
        if (actor?.data == null || !MclslActorAccessor.Alive(actor))
        {
            message = "猫宝未能照见这名修士。";
            return false;
        }

        return MclslMaobaoArchiveManager.ToggleActor(actor, out message);
    }

    internal static int RefreshRecords()
    {
        return MclslMaobaoArchiveManager.RefreshLiveSnapshots();
    }

    internal static bool Remove(string id, out string message)
    {
        return MclslMaobaoArchiveManager.Remove(id, out message);
    }

    internal static bool IsRecorded(long actorId)
    {
        return MclslMaobaoArchiveManager.IsActorSavedById(actorId);
    }

    private static void InsertLeader(List<Actor> leaders, Actor actor)
    {
        long score = Score(actor);
        int position = leaders.Count;
        for (int i = 0; i < leaders.Count; i++)
        {
            if (score <= Score(leaders[i])) continue;
            position = i;
            break;
        }
        if (position >= MaxCandidates && leaders.Count >= MaxCandidates) return;
        leaders.Insert(position, actor);
        if (leaders.Count > MaxCandidates) leaders.RemoveAt(leaders.Count - 1);
    }

    private static MclslMaobaoCandidate BuildCandidate(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor)) return null;
        MclslActorCultivationView view = MclslActorCultivationQuery.Build(actor);
        if (!view.Alive) return null;
        return new MclslMaobaoCandidate
        {
            Actor = actor,
            ActorId = MclslActorAccessor.Id(actor),
            Name = view.Name,
            RealmId = view.RealmId,
            RealmName = view.RealmName,
            TechniqueName = view.TechniqueName,
            CultivationSystemName = view.CultivationSystemName,
            FactionName = view.FactionAffiliation,
            TrueEssence = view.TrueEssence
        };
    }

    private static long Score(Actor actor)
    {
        int realm = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        int essence = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence, 0));
        return realm * 10000000L + essence;
    }

    private static void UpdateRecord(MclslMaobaoRecord record, Actor actor, bool countObservation)
    {
        MclslActorCultivationView view = MclslActorCultivationQuery.Build(actor);
        int year = MclslRuntime.CurrentYear();
        record.ActorName = view.Name;
        record.RealmId = view.RealmId;
        record.RealmName = view.RealmName;
        record.CultivationSystemName = view.CultivationSystemName;
        record.TechniqueName = view.TechniqueName;
        record.FactionName = view.FactionAffiliation;
        record.LastObservedYear = year;
        record.MapX = actor.data.x;
        record.MapY = actor.data.y;
        record.Alive = true;
        if (countObservation && record.FirstRecordedYear != year) record.ObservationCount++;
        record.Inscription = "猫宝照影：" + record.ActorName + "以" + Blank(record.RealmName, "凡俗")
            + "之身，于" + year + "年映入时序。";
    }

    private static string Blank(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value;
}

internal sealed class MclslMaobaoCandidate
{
    internal Actor Actor;
    internal long ActorId;
    internal string Name = string.Empty;
    internal string RealmId = string.Empty;
    internal string RealmName = string.Empty;
    internal string CultivationSystemName = string.Empty;
    internal string TechniqueName = string.Empty;
    internal string FactionName = string.Empty;
    internal int TrueEssence;
}
