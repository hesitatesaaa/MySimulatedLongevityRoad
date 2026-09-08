using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 传法天尊证道前的新法先行阶段。旧法仍是世界主流，但少量修士已经
/// 接触、试修并传播新法，使传法变世发生时不会从零开始。
/// </summary>
internal static class MclslNewLawPioneerSystem
{
    private const int MaxTotalPioneers = 64;
    private static int _lastProcessedYear = -1;

    internal static int PioneerLeadYears
    {
        get
        {
            int duration = Math.Max(1, MclslWorldEpochSystem.AncientLawDurationYears);
            return Math.Clamp(duration * 3 / 5, 300, 600);
        }
    }

    internal static int PioneerStartYear
    {
        get
        {
            MclslWorldRunState run = MclslWorldRunRepository.Current;
            if (run == null || string.IsNullOrWhiteSpace(run.RunId)) return int.MaxValue;
            int calculated = Math.Max(run.EraOriginYear + 80, run.AncientLawEndYear - PioneerLeadYears);
            return run.NewLawPioneerStartYear > 0 ? run.NewLawPioneerStartYear : calculated;
        }
    }

    internal static bool IsPioneerEra(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        return run != null && !string.IsNullOrWhiteSpace(run.RunId)
            && year >= PioneerStartYear && year < run.AncientLawEndYear;
    }

    internal static bool CanPracticeNewLaw(int year) => MclslWorldEpochSystem.IsNewLawActive(year) || IsPioneerEra(year);

    internal static bool CanUseNewLawResources(int year) => CanPracticeNewLaw(year);

    internal static bool CanUseWorldChanges(int year)
    {
        if (MclslWorldEpochSystem.IsNewLawActive(year)) return true;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        return IsPioneerEra(year) && run != null && run.AncientLawEndYear - year <= 300;
    }

    internal static void ProcessAnnual(int year)
    {
        if (_lastProcessedYear == year || !IsPioneerEra(year)) return;
        _lastProcessedYear = year;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null) return;
        if (run.NewLawPioneerStartYear <= 0) run.NewLawPioneerStartYear = PioneerStartYear;

        const string eventId = "epoch_new_law_pioneers";
        run.FiredHistoricalEvents ??= new List<string>();
        if (!run.FiredHistoricalEvents.Contains(eventId))
        {
            run.FiredHistoricalEvents.Add(eventId);
            MclslWorldRunRepository.AddEvent(year, eventId, "新法初传",
                "旧法仍盛，已有少数修士暗中试行一条不同于旧仙道的新路。此时新法尚未定世，亦无仙法不可同修与仙凡瘴。");
            if (MclslRuntimeSettings.PioneerAnnouncementsEnabled)
                MclslAnnouncementSystem.Enqueue("新法初传：少数修士已开始试行新路。", "#8FD8D8", 8f, 0);
        }

        // 先行阶段只维持少量资源，供早期金丹、元婴继续推进；不会开启
        // 万仙盟、五老会、仙凡瘴、逆理或法不可同修。
        MclslWorldCaveSystem.EnsureWorldCaves(year);
        int remaining = Math.Max(0, run.AncientLawEndYear - year);
        if (remaining <= 240) MclslWorldChangeSystem.EnsureWorldChanges(year);

        TryConvertAncientPioneer(year, remaining);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static bool TryBeginMortalPioneer(Actor actor, int year)
    {
        if (actor?.data == null || !IsPioneerEra(year)) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor))) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty))) return false;

        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || run.NewLawPioneerCount >= MaxTotalPioneers) return false;
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0), 0, 100);
        if (aptitude < 65) return false;

        int remaining = Math.Max(0, run.AncientLawEndYear - year);
        int interval = remaining <= 120 ? 6 : remaining <= 300 ? 10 : 18;
        if (run.LastNewLawPioneerYear > 0 && year - run.LastNewLawPioneerYear < interval) return false;

        long actorId = MclslActorAccessor.Id(actor);
        int gate = PositiveHash((run.RunId ?? string.Empty) + "|mortal_pioneer|" + actorId + "|" + year) % 100;
        int chance = remaining <= 120 ? 78 : remaining <= 300 ? 62 : 48;
        if (gate >= chance) return false;

        MclslNewLawEntrySystem.BeginCultivation(actor, year, "得闻未定新法，成为先行修士");
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NewLawPioneer, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NewLawPioneerYear, year);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NewLawPioneerOrigin, "早期传法");
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Contribution, 0);
        run.LastNewLawPioneerYear = year;
        run.NewLawPioneerCount++;
        if (run.NewLawPioneerCount == 1 || run.NewLawPioneerCount % 6 == 0)
        {
            MclslWorldRunRepository.AddEvent(year, "newlaw_pioneer_entry", MclslActorAccessor.DisplayName(actor) + "试修新法",
                MclslActorAccessor.DisplayName(actor) + "得闻尚未定世的新法，自感气起另辟修途。", actor);
        }
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static void Clear()
    {
        _lastProcessedYear = -1;
    }

    private static void TryConvertAncientPioneer(int year, int remaining)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || run.NewLawPioneerCount >= MaxTotalPioneers) return;
        int interval = remaining <= 120 ? 10 : remaining <= 300 ? 16 : 24;
        if (run.LastNewLawPioneerConversionYear > 0 && year - run.LastNewLawPioneerConversionYear < interval) return;
        if (year < PioneerStartYear + 60) return;

        const int candidateLimit = 6;
        Actor[] candidates = new Actor[candidateLimit];
        int[] scores = new int[candidateLimit];
        for (int i = 0; i < candidateLimit; i++) scores[i] = int.MinValue;

        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot();
        for (int i = 0; i < actors.Count; i++)
        {
            Actor actor = actors[i];
            if (!MclslActorAccessor.Alive(actor)) continue;
            if (MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) != MclslCultivationSystemIds.AncientLaw) continue;
            string realm = MclslActorAccessor.Realm(actor);
            int realmIndex = MclslRealmIds.Index(realm);
            if (realmIndex < MclslRealmIds.Index(MclslRealmIds.ZhuJi) || realmIndex > MclslRealmIds.Index(MclslRealmIds.YuanYing)) continue;
            int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0), 0, 100);
            if (aptitude < 68) continue;
            int score = realmIndex * 10000 + aptitude * 100
                + PositiveHash(MclslActorAccessor.Id(actor) + "|pioneer_conversion|" + year) % 100;
            for (int slot = 0; slot < candidateLimit; slot++)
            {
                if (score <= scores[slot]) continue;
                for (int move = candidateLimit - 1; move > slot; move--)
                {
                    scores[move] = scores[move - 1];
                    candidates[move] = candidates[move - 1];
                }
                scores[slot] = score;
                candidates[slot] = actor;
                break;
            }
        }

        run.LastNewLawPioneerConversionYear = year;
        for (int i = 0; i < candidates.Length; i++)
        {
            Actor candidate = candidates[i];
            if (!MclslActorAccessor.Alive(candidate)) continue;
            if (!MclslWorldEpochSystem.TryConvertAncientPioneer(candidate, year)) continue;
            MclslActorAccessor.Set(candidate, MclslActorDataKeys.NewLawPioneer, 1);
            MclslActorAccessor.Set(candidate, MclslActorDataKeys.NewLawPioneerYear, year);
            MclslActorAccessor.Set(candidate, MclslActorDataKeys.NewLawPioneerOrigin, "旧法转试新法");
            run.NewLawPioneerCount++;
            break;
        }
    }

    private static int PositiveHash(string value)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in value ?? string.Empty) hash = hash * 37 + c;
            return hash & int.MaxValue;
        }
    }
}
