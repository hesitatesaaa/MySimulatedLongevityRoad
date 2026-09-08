using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslTechniqueLineageSystem
{
    private sealed class TechniqueSnapshot
    {
        internal string Id = string.Empty;
        internal string Name = string.Empty;
        internal string SystemId = string.Empty;
        internal string MaxRealm = string.Empty;
        internal string LawTags = string.Empty;
        internal int Count;
        internal string PeakRealm = string.Empty;
        internal string FounderName = string.Empty;
        internal long FounderActorId;
    }

    internal static void ResolveAnnual(int year, IReadOnlyList<Actor> actors)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.TechniqueLineages == null || actors == null) return;

        Dictionary<string, TechniqueSnapshot> current = BuildSnapshot(actors);
        bool changed = false;

        foreach (TechniqueSnapshot snapshot in current.Values)
        {
            MclslTechniqueLineageRecord record = FindLineageRecord(run, snapshot.Id);
            if (record == null)
            {
                record = CreateRecord(year, snapshot);
                run.TechniqueLineages.Add(record);
                changed = true;
                MclslWorldRunRepository.AddEvent(year, EventPrefix(record) + "technique_recorded", "《" + record.Name + "》入录", "此法初见于玄黄世间，法脉尚浅，传承由修士自行授受。");
            }
            else
            {
                changed |= UpdateRecord(record, year, snapshot);
            }

            if (record.State == "失传")
            {
                record.State = "复现";
                record.RevivedYear = year;
                record.LifecycleState = "遗法复现";
                record.LifecycleYear = Math.Max(record.LifecycleYear, year);
                if (string.IsNullOrWhiteSpace(record.SectDisplayName))
                    record.SectDisplayName = BuildSectDisplayName(record.Name);
                changed = true;
                MclslWorldRunRepository.AddEvent(year, EventPrefix(record) + "technique_revived", "《" + record.Name + "》复现", "失传旧法再度现世，传者" + record.CurrentPractitioners + "人，最高已至" + MclslRealmIds.Display(record.PeakRealm) + "。");
            }

            int previousPeak = Math.Max(0, record.PeakPractitioners);
            if (snapshot.Count > record.PeakPractitioners)
            {
                record.PeakPractitioners = snapshot.Count;
                changed = true;
            }
            if (record.PeakPractitioners > previousPeak)
                TryRecordFlourish(year, record, previousPeak);
            changed |= TryMarkAncientFamousLineage(year, record);
        }

        foreach (MclslTechniqueLineageRecord record in run.TechniqueLineages)
        {
            if (record == null || current.ContainsKey(record.Id) || record.CurrentPractitioners <= 0) continue;
            record.CurrentPractitioners = 0;
            record.LastSeenYear = Math.Max(record.LastSeenYear, year);
            record.State = "失传";
            record.LostYear = year;
            changed |= EnsureLifecycle(record, year);
            changed = true;
            MclslWorldRunRepository.AddEvent(year, EventPrefix(record) + "technique_lost", "《" + record.Name + "》失传", "此法当世再无传者，昔日最高曾至" + MclslRealmIds.Display(record.PeakRealm) + "。");
            TryCreateLineageRuin(year, run, record);
        }

        if (MclslWorldEpochSystem.IsNewLawActive(year))
            changed |= CarryAncientLineagesIntoNewLaw(year, run);

        while (run.TechniqueLineages.Count > 500)
            RemoveOldestOverflowLineage(run.TechniqueLineages);

        if (changed) MclslWorldArchiveStore.MarkDirty();
    }

    private static Dictionary<string, TechniqueSnapshot> BuildSnapshot(IReadOnlyList<Actor> actors)
    {
        Dictionary<string, TechniqueSnapshot> result = new(StringComparer.Ordinal);
        for (int i = 0; i < actors.Count; i++)
        {
            Actor actor = actors[i];
            if (!MclslActorAccessor.Alive(actor) || !MclslActorAccessor.IsCultivator(actor)) continue;
            string id = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty);
            string name = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty);
            if (string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(name)) continue;

            string key = StableTechniqueKey(id, name);
            if (!result.TryGetValue(key, out TechniqueSnapshot snapshot))
            {
                snapshot = BuildInitialSnapshot(actor, key, id, name);
                result[key] = snapshot;
            }

            snapshot.Count++;
            string realm = MclslActorAccessor.Realm(actor);
            int realmIndex = MclslRealmIds.Index(realm);
            int previousPeakIndex = MclslRealmIds.Index(snapshot.PeakRealm);
            if (realmIndex > previousPeakIndex)
            {
                snapshot.PeakRealm = realm;
                snapshot.FounderActorId = MclslActorAccessor.Id(actor);
                snapshot.FounderName = MclslActorAccessor.DisplayName(actor);
            }
            else if (snapshot.FounderActorId <= 0L || IsPlaceholderFounder(snapshot.FounderName))
            {
                snapshot.FounderActorId = MclslActorAccessor.Id(actor);
                snapshot.FounderName = MclslActorAccessor.DisplayName(actor);
            }
        }
        return result;
    }

    private static TechniqueSnapshot BuildInitialSnapshot(Actor actor, string key, string id, string name)
    {
        string normalized = MclslCultivationCatalog.NormalizeTechniqueId(id);
        string baseTechniqueId = MclslCultivationCatalog.BaseTechniqueId(normalized);
        MclslTechniqueDefinition definition = MclslCultivationCatalog.Technique(baseTechniqueId);
        MclslTechniqueLineageRecord existing = FindLineageRecord(MclslWorldRunRepository.Current, key);
        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, MclslCultivationSystemIds.AncientLaw);
        string tags = existing != null && !string.IsNullOrWhiteSpace(existing.LawTags)
            ? existing.LawTags
            : string.Join(",", definition.LawPool);
        if (string.IsNullOrWhiteSpace(tags))
            tags = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientDaoIntent, string.Empty);
        string maxRealm = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueMaxRealm,
            existing != null && !string.IsNullOrWhiteSpace(existing.MaxRealm) ? existing.MaxRealm : definition.MaxRealm);
        return new TechniqueSnapshot
        {
            Id = key,
            Name = normalized != baseTechniqueId
                ? definition.Name
                : string.IsNullOrWhiteSpace(name)
                    ? (existing != null && !string.IsNullOrWhiteSpace(existing.Name) ? existing.Name : definition.Name)
                    : name,
            SystemId = string.IsNullOrWhiteSpace(system) ? MclslCultivationSystemIds.AncientLaw : system,
            MaxRealm = string.IsNullOrWhiteSpace(maxRealm) ? definition.MaxRealm : maxRealm,
            LawTags = tags,
            FounderActorId = MclslActorAccessor.Id(actor),
            FounderName = MclslActorAccessor.DisplayName(actor)
        };
    }

    private static MclslTechniqueLineageRecord CreateRecord(int year, TechniqueSnapshot snapshot) => new()
    {
        Id = snapshot.Id,
        Name = snapshot.Name,
        SystemId = snapshot.SystemId,
        LawTags = snapshot.LawTags,
        MaxRealm = snapshot.MaxRealm,
        Completeness = 100,
        FirstSeenYear = year,
        LastSeenYear = year,
        CurrentPractitioners = snapshot.Count,
        PeakPractitioners = snapshot.Count,
        PeakRealm = snapshot.PeakRealm,
        FounderActorId = snapshot.FounderActorId,
        FounderName = snapshot.FounderName,
        State = "流传",
        SourceTechniqueId = snapshot.Id,
        SectDisplayName = BuildSectDisplayName(snapshot.Name),
        LifecycleState = snapshot.Count >= 8 ? "法脉流传" : "传承兴起",
        LifecycleYear = year,
        Summary = "此法以" + DisplayTags(snapshot.LawTags) + "为本，可修至" + MclslRealmIds.Display(snapshot.MaxRealm) + "。"
    };

    private static bool UpdateRecord(MclslTechniqueLineageRecord record, int year, TechniqueSnapshot snapshot)
    {
        bool changed = false;
        if (record.Name != snapshot.Name) { record.Name = snapshot.Name; changed = true; }
        if (record.SystemId != snapshot.SystemId) { record.SystemId = snapshot.SystemId; changed = true; }
        if (record.LawTags != snapshot.LawTags) { record.LawTags = snapshot.LawTags; changed = true; }
        if (record.MaxRealm != snapshot.MaxRealm) { record.MaxRealm = snapshot.MaxRealm; changed = true; }
        if (record.FirstSeenYear <= 0) { record.FirstSeenYear = year; changed = true; }
        if (record.LastSeenYear != year) { record.LastSeenYear = year; changed = true; }
        if (record.CurrentPractitioners != snapshot.Count) { record.CurrentPractitioners = snapshot.Count; changed = true; }
        if (MclslRealmIds.Index(snapshot.PeakRealm) > MclslRealmIds.Index(record.PeakRealm)) { record.PeakRealm = snapshot.PeakRealm; changed = true; }
        if (IsPlaceholderFounder(record.FounderName)) { record.FounderName = snapshot.FounderName; changed = true; }
        if (record.FounderActorId <= 0L) { record.FounderActorId = snapshot.FounderActorId; changed = true; }
        if (string.IsNullOrWhiteSpace(record.Summary))
        {
            record.Summary = "此法以" + DisplayTags(record.LawTags) + "为本，可修至" + MclslRealmIds.Display(record.MaxRealm) + "。";
            changed = true;
        }
        changed |= EnsureLifecycle(record, year);
        return changed;
    }

    private static void TryRecordFlourish(int year, MclslTechniqueLineageRecord record, int previousPeak)
    {
        int tier = record.PeakPractitioners >= 50 ? 50 : record.PeakPractitioners >= 20 ? 20 : record.PeakPractitioners >= 8 ? 8 : 0;
        if (tier <= 0 || previousPeak >= tier) return;
        string title = tier >= 50 ? "《" + record.Name + "》大盛" : "《" + record.Name + "》一法兴盛";
        string body = "此法传者已达" + record.PeakPractitioners + "人，最高修至" + MclslRealmIds.Display(record.PeakRealm) + "，法脉渐成一时显学。";
        MclslWorldRunRepository.AddEvent(year, EventPrefix(record) + "technique_flourish", title, body);
    }

    private static bool TryMarkAncientFamousLineage(int year, MclslTechniqueLineageRecord record)
    {
        if (record == null || record.SystemId != MclslCultivationSystemIds.AncientLaw) return false;
        if (MclslWorldEpochSystem.IsNewLawActive(year)) return false;
        if (record.State == "名法流传" || record.State == "后世名法") return false;
        if (!IsFamousAncientLineage(record)) return false;

        record.State = "名法流传";
        record.LifecycleState = "道统鼎盛";
        record.LifecycleYear = Math.Max(record.LifecycleYear, year);
        if (string.IsNullOrWhiteSpace(record.SectDisplayName))
            record.SectDisplayName = BuildSectDisplayName(record.Name);
        record.Summary = "仙道名法。以" + DisplayTags(record.LawTags) + "为本，曾有" + record.PeakPractitioners
            + "人同修，最高至" + MclslRealmIds.Display(record.PeakRealm) + "。";
        MclslWorldRunRepository.AddEvent(year, "ancient_famous_technique", "《" + record.Name + "》名动一时",
            "此法传者渐众，后世或可由残卷、遗府与讲法脉络中寻得其影。");
        return true;
    }

    private static bool CarryAncientLineagesIntoNewLaw(int year, MclslWorldRunState run)
    {
        if (run?.TechniqueLineages == null) return false;
        bool changed = false;
        int announcements = 0;
        int linkedRuinsCreated = 0;
        for (int i = 0; i < run.TechniqueLineages.Count; i++)
        {
            MclslTechniqueLineageRecord record = run.TechniqueLineages[i];
            if (!IsUsableAncientLineage(record) || record.State == "后世遗法" || record.State == "后世名法") continue;

            bool famous = IsFamousAncientLineage(record);
            record.State = famous ? "后世名法" : "后世遗法";
            record.LifecycleState = famous ? "道统鼎盛" : "遗法复现";
            record.LifecycleYear = Math.Max(record.LifecycleYear, year);
            if (string.IsNullOrWhiteSpace(record.SectDisplayName))
                record.SectDisplayName = BuildSectDisplayName(record.Name);
            record.Summary = famous
                ? "仙道名法。传法变世后，其名仍见于残卷、遗府与新法讲义。"
                : "旧法遗篇。传法变世后仍可由旧宗手札、遗府残卷与前人讲义重新取得。";
            if (string.IsNullOrWhiteSpace(record.LinkedRuinId) && linkedRuinsCreated < 8)
            {
                TryCreateLineageRuin(year, run, record);
                if (!string.IsNullOrWhiteSpace(record.LinkedRuinId)) linkedRuinsCreated++;
            }
            if (announcements < 3)
            {
                MclslWorldRunRepository.AddEvent(year, "ancient_lineage_remembered", "《" + record.Name + "》传入后世",
                    famous ? "旧法虽远，此法名号仍留于玄黄。" : "旧宗残篇重见天日，此法由此进入新法时代的功法来源。" );
                announcements++;
            }
            changed = true;
        }
        return changed;
    }

    private static bool IsUsableAncientLineage(MclslTechniqueLineageRecord record)
    {
        if (record == null || record.SystemId != MclslCultivationSystemIds.AncientLaw) return false;
        if (string.IsNullOrWhiteSpace(record.Name)) return false;
        if (record.Completeness <= 0) return false;
        string maxRealm = string.IsNullOrWhiteSpace(record.MaxRealm)
            ? MclslCultivationCatalog.Technique(record.SourceTechniqueId).MaxRealm
            : record.MaxRealm;
        return MclslRealmIds.Index(maxRealm) >= MclslRealmIds.Index(MclslRealmIds.ZhuJi);
    }

    internal static bool TryPickInheritedTechnique(int year, string seed, int aptitude, out MclslTechniqueDefinition technique)
    {
        technique = null;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.TechniqueLineages == null || !MclslWorldEpochSystem.IsNewLawActive(year)) return false;

        MclslTechniqueLineageRecord picked = null;
        int pickedScore = int.MinValue;
        int salt = StableHash((seed ?? string.Empty) + "|inherit|" + year) & int.MaxValue;
        for (int i = 0; i < run.TechniqueLineages.Count; i++)
        {
            MclslTechniqueLineageRecord record = run.TechniqueLineages[i];
            if (!IsUsableAncientLineage(record)) continue;
            int score = Math.Max(1, FameScore(record)) + Math.Max(0, MclslRealmIds.Index(record.MaxRealm)) * 25
                + Math.Clamp(record.Completeness, 0, 100) / 4
                + (StableHash(record.Id + "|" + salt) & int.MaxValue) % 97;
            if (score <= pickedScore) continue;
            picked = record;
            pickedScore = score;
        }

        if (picked == null) return false;
        string sourceTechniqueId = MclslCultivationCatalog.BaseTechniqueId(picked.SourceTechniqueId);
        MclslTechniqueDefinition sourceTechnique = null;
        if (string.IsNullOrWhiteSpace(sourceTechniqueId)
            || !MclslCultivationCatalog.TryTechnique(sourceTechniqueId, out sourceTechnique))
        {
            sourceTechnique = MclslCultivationCatalog.Techniques[(StableHash(picked.Id + "|legacy_source") & int.MaxValue) % MclslCultivationCatalog.Techniques.Count];
            sourceTechniqueId = sourceTechnique.Id;
        }
        string[] laws = MclslGeneratedObjectFactory.SplitTags(picked.LawTags);
        if (laws == null || laws.Length == 0) laws = sourceTechnique.LawPool;
        string maxRealm = InheritedMaxRealm(picked.MaxRealm, aptitude);
        technique = new MclslTechniqueDefinition
        {
            Id = "legacy_" + (StableHash(picked.Id) & int.MaxValue).ToString() + "__" + sourceTechniqueId,
            Name = picked.Name,
            LawPool = laws,
            MaxRealm = maxRealm
        };
        return true;
    }

    internal static bool TryFuseNewLawLineage(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.TechniqueLineages == null || !MclslWorldEpochSystem.IsNewLawActive(year)) return false;
        if (run.TechniqueLineages.Count < 2) return false;

        int start = (StableHash((run.RunId ?? string.Empty) + "|all_laws_one|" + year) & int.MaxValue) % run.TechniqueLineages.Count;
        for (int offset = 0; offset < run.TechniqueLineages.Count; offset++)
        {
            MclslTechniqueLineageRecord primary = run.TechniqueLineages[(start + offset) % run.TechniqueLineages.Count];
            if (!CanFuse(primary)) continue;
            for (int inner = 1; inner < run.TechniqueLineages.Count; inner++)
            {
                MclslTechniqueLineageRecord secondary = run.TechniqueLineages[(start + offset + inner) % run.TechniqueLineages.Count];
                if (!CanFuse(secondary) || primary.Id == secondary.Id) continue;
                if (!ShareAnyLaw(primary.LawTags, secondary.LawTags)) continue;

                primary.State = "融法";
                primary.LastSeenYear = Math.Max(primary.LastSeenYear, year);
                primary.LifecycleState = "道统鼎盛";
                primary.LifecycleYear = Math.Max(primary.LifecycleYear, year);
                if (string.IsNullOrWhiteSpace(primary.SectDisplayName))
                    primary.SectDisplayName = BuildSectDisplayName(primary.Name);
                primary.LawTags = MergeLawTags(primary.LawTags, secondary.LawTags, 6);
                if (MclslRealmIds.Index(secondary.MaxRealm) > MclslRealmIds.Index(primary.MaxRealm))
                    primary.MaxRealm = secondary.MaxRealm;
                if (MclslRealmIds.Index(secondary.PeakRealm) > MclslRealmIds.Index(primary.PeakRealm))
                    primary.PeakRealm = secondary.PeakRealm;
                primary.Summary = "万法归一后，此法吞并相近法脉残意，以" + DisplayTags(primary.LawTags)
                    + "为本，可修至" + MclslRealmIds.Display(primary.MaxRealm) + "。";
                secondary.State = "归流";
                secondary.LastSeenYear = Math.Max(secondary.LastSeenYear, year);
                secondary.LifecycleState = "法脉流传";
                secondary.LifecycleYear = Math.Max(secondary.LifecycleYear, year);
                if (string.IsNullOrWhiteSpace(secondary.SectDisplayName))
                    secondary.SectDisplayName = BuildSectDisplayName(secondary.Name);
                secondary.Summary = "此法部分法意并入《" + primary.Name + "》，余脉仍可独立流传。";
                MclslWorldRunRepository.AddEvent(year, "inverse_truth_all_laws_one_fusion", "《" + primary.Name + "》融合法脉",
                    "《" + primary.Name + "》与《" + secondary.Name + "》法意相通，万法归一之势渐显。");
                MclslWorldArchiveStore.MarkDirty();
                return true;
            }
        }
        return false;
    }

    internal static void MarkAncientTechniqueImprint(Actor actor, int year, string reason, int fameFloor = 0)
    {
        if (actor?.data == null || MclslWorldEpochSystem.IsNewLawActive(year)) return;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.TechniqueLineages == null) return;

        string id = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty);
        string name = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty);
        if (string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(name)) return;

        string key = StableTechniqueKey(id, name);
        MclslTechniqueLineageRecord record = FindLineageRecord(run, key);
        if (record == null)
        {
            record = CreateRecord(year, BuildInitialSnapshot(actor, key, id, name));
            run.TechniqueLineages.Add(record);
        }

        string realm = MclslActorAccessor.Realm(actor);
        record.SystemId = MclslCultivationSystemIds.AncientLaw;
        record.LastSeenYear = Math.Max(record.LastSeenYear, year);
        record.CurrentPractitioners = Math.Max(record.CurrentPractitioners, 1);
        record.PeakPractitioners = Math.Max(record.PeakPractitioners, Math.Max(1, fameFloor));
        if (MclslRealmIds.Index(realm) > MclslRealmIds.Index(record.PeakRealm))
            record.PeakRealm = realm;
        string actorMaxRealm = MclslTechniqueRealmLimit.MaxRealm(actor);
        if (MclslRealmIds.Index(actorMaxRealm) > MclslRealmIds.Index(record.MaxRealm))
            record.MaxRealm = actorMaxRealm;
        bool privateLineage = IsPrivateLineageReason(reason);
        bool secretLineage = IsSecretLineageReason(reason);
        if (privateLineage || secretLineage)
            record.MaxRealm = MaxRealmAtLeast(record.MaxRealm, MclslRealmIds.JinDan);
        if (IsPlaceholderFounder(record.FounderName))
            record.FounderName = MclslActorAccessor.DisplayName(actor);
        if (record.FounderActorId <= 0)
            record.FounderActorId = MclslActorAccessor.Id(actor);

        if (privateLineage || secretLineage)
        {
            record.State = secretLineage ? "秘境传承" : "私法留传";
            record.LifecycleState = secretLineage ? "秘境道统" : "遗府私传";
            record.LifecycleYear = Math.Max(record.LifecycleYear, year);
            string cause = string.IsNullOrWhiteSpace(reason) ? (secretLineage ? "秘境所得" : "遗府旧藏") : reason;
            if (string.IsNullOrWhiteSpace(record.SectDisplayName))
                record.SectDisplayName = secretLineage ? BuildSectDisplayName(record.Name) : BuildPrivateDisplayName(record.Name);
            record.Summary = secretLineage
                ? "秘境中留有旧日宗门讲法痕迹。由" + cause + "入录，以" + DisplayTags(record.LawTags) + "为本，可修至" + MclslRealmIds.Display(record.MaxRealm) + "。"
                : "前人洞府中留有私人修持残卷。由" + cause + "入录，以" + DisplayTags(record.LawTags) + "为本，可修至" + MclslRealmIds.Display(record.MaxRealm) + "。";
        }
        else if (IsFamousAncientLineage(record))
        {
            record.State = "名法流传";
            record.LifecycleState = "道统鼎盛";
            record.LifecycleYear = Math.Max(record.LifecycleYear, year);
            if (string.IsNullOrWhiteSpace(record.SectDisplayName))
                record.SectDisplayName = BuildSectDisplayName(record.Name);
            string cause = string.IsNullOrWhiteSpace(reason) ? "历代修持" : reason;
            record.Summary = "仙道名法。由" + cause + "留名，以" + DisplayTags(record.LawTags) + "为本，可修至" + MclslRealmIds.Display(record.MaxRealm) + "。";
        }
        else
        {
            EnsureLifecycle(record, year);
        }

        MclslWorldArchiveStore.MarkDirty();
    }

    private static string InheritedMaxRealm(string oldMaxRealm, int aptitude)
    {
        int oldIndex = MclslRealmIds.Index(oldMaxRealm);
        int capIndex = aptitude >= 92 ? MclslRealmIds.Index(MclslRealmIds.HeDao)
            : aptitude >= 82 ? MclslRealmIds.Index(MclslRealmIds.HuaShen)
            : aptitude >= 68 ? MclslRealmIds.Index(MclslRealmIds.YuanYing)
            : aptitude >= 50 ? MclslRealmIds.Index(MclslRealmIds.JinDan)
            : MclslRealmIds.Index(MclslRealmIds.ZhuJi);
        int index = Math.Clamp(oldIndex < 0 ? capIndex : Math.Min(oldIndex, capIndex), 0, MclslRealmIds.Index(MclslRealmIds.HeDao));
        return MclslRealmIds.Ordered[index];
    }

    private static bool IsFamousAncientLineage(MclslTechniqueLineageRecord record)
    {
        return record != null
            && (record.PeakPractitioners >= 18
                || MclslRealmIds.Index(record.PeakRealm) >= MclslRealmIds.Index(MclslRealmIds.JinDan)
                || FameScore(record) >= 55);
    }

    private static bool CanFuse(MclslTechniqueLineageRecord record)
    {
        return record != null
            && record.SystemId == MclslCultivationSystemIds.NewLaw
            && record.CurrentPractitioners > 0
            && !string.Equals(record.State, "失传", StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(record.LawTags);
    }

    private static bool ShareAnyLaw(string left, string right)
    {
        string[] a = MclslGeneratedObjectFactory.SplitTags(left);
        string[] b = MclslGeneratedObjectFactory.SplitTags(right);
        for (int i = 0; i < a.Length; i++)
        {
            for (int j = 0; j < b.Length; j++)
            {
                if (string.Equals(a[i], b[j], StringComparison.Ordinal)) return true;
            }
        }
        return false;
    }

    private static string MergeLawTags(string left, string right, int limit)
    {
        List<string> tags = new(limit);
        AddMergedTags(tags, MclslGeneratedObjectFactory.SplitTags(left), limit);
        AddMergedTags(tags, MclslGeneratedObjectFactory.SplitTags(right), limit);
        return string.Join(",", tags);
    }

    private static void AddMergedTags(List<string> target, string[] source, int limit)
    {
        if (target == null || source == null) return;
        for (int i = 0; i < source.Length && target.Count < limit; i++)
        {
            string tag = source[i];
            if (string.IsNullOrWhiteSpace(tag) || target.Contains(tag)) continue;
            target.Add(tag);
        }
    }

    private static int FameScore(MclslTechniqueLineageRecord record)
    {
        if (record == null) return 0;
        int realm = Math.Max(0, MclslRealmIds.Index(record.PeakRealm));
        int max = Math.Max(0, MclslRealmIds.Index(record.MaxRealm));
        return record.PeakPractitioners + realm * 18 + max * 8;
    }

    internal static string RecordRuinTechniqueRevival(int year, Actor actor, MclslSectRuinRecord ruin, MclslTechniqueDefinition technique)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.TechniqueLineages == null || technique == null) return string.Empty;

        string key = !string.IsNullOrWhiteSpace(ruin?.LinkedLineageId)
            ? ruin.LinkedLineageId
            : StableTechniqueKey(technique.Id, technique.Name);
        MclslTechniqueLineageRecord record = FindLineageRecord(run, key);
        if (record == null && ruin != null && !string.IsNullOrWhiteSpace(ruin.SourceTechniqueId))
            record = FindLineageRecord(run, StableTechniqueKey(ruin.SourceTechniqueId, ruin.SourceTechniqueName));

        string actorRealm = MclslActorAccessor.Realm(actor);
        string lineageMaxRealm = MaxRealmAtLeast(technique.MaxRealm, MclslRealmIds.JinDan);
        string recoveredState = IsSecretRuin(ruin) ? "秘境道统" : "遗府私传";
        string recoveredDisplayName = IsSecretRuin(ruin) ? BuildSectDisplayName(technique.Name) : BuildPrivateDisplayName(technique.Name);
        if (record == null)
        {
            record = new MclslTechniqueLineageRecord
            {
                Id = StableTechniqueKey(technique.Id, technique.Name),
                Name = technique.Name,
                SystemId = MclslWorldEpochSystem.IsNewLawActive(year) ? MclslCultivationSystemIds.NewLaw : MclslCultivationSystemIds.AncientLaw,
                LawTags = string.Join(",", technique.LawPool),
                MaxRealm = lineageMaxRealm,
                Completeness = Math.Clamp(80 + Math.Clamp(ruin?.Quality ?? 2, 1, 4) * 3, 83, 92),
                FirstSeenYear = year,
                LastSeenYear = year,
                CurrentPractitioners = 1,
                PeakPractitioners = 1,
                PeakRealm = actorRealm,
                FounderActorId = MclslActorAccessor.Id(actor),
                FounderName = MclslActorAccessor.DisplayName(actor),
                State = "复现",
                SourceTechniqueId = MclslCultivationCatalog.NormalizeTechniqueId(technique.Id),
                LinkedRuinId = ruin?.Id ?? string.Empty,
                SectDisplayName = recoveredDisplayName,
                LifecycleState = recoveredState,
                LifecycleYear = year,
                RevivedYear = year,
                Summary = IsSecretRuin(ruin)
                    ? "此法自秘境复现，原为旧日宗门传承，以" + DisplayTags(string.Join(",", technique.LawPool)) + "为本，可修至" + MclslRealmIds.Display(lineageMaxRealm) + "。"
                    : "此法自遗府复现，原为前人私人修持残卷，以" + DisplayTags(string.Join(",", technique.LawPool)) + "为本，可修至" + MclslRealmIds.Display(lineageMaxRealm) + "。"
            };
            run.TechniqueLineages.Add(record);
        }
        else
        {
            record.Name = string.IsNullOrWhiteSpace(record.Name) ? technique.Name : record.Name;
            record.LastSeenYear = Math.Max(record.LastSeenYear, year);
            record.CurrentPractitioners = Math.Max(record.CurrentPractitioners, 1);
            record.PeakPractitioners = Math.Max(record.PeakPractitioners, 1);
            if (MclslRealmIds.Index(actorRealm) > MclslRealmIds.Index(record.PeakRealm))
                record.PeakRealm = actorRealm;
            if (MclslRealmIds.Index(lineageMaxRealm) > MclslRealmIds.Index(record.MaxRealm))
                record.MaxRealm = lineageMaxRealm;
            if (string.IsNullOrWhiteSpace(record.SourceTechniqueId))
                record.SourceTechniqueId = MclslCultivationCatalog.NormalizeTechniqueId(technique.Id);
            if (string.IsNullOrWhiteSpace(record.LinkedRuinId) && ruin != null)
                record.LinkedRuinId = ruin.Id;
            if (record.State == "失传" || record.RevivedYear <= 0)
                record.RevivedYear = year;
            record.State = "复现";
            record.Completeness = Math.Max(record.Completeness, Math.Clamp(80 + Math.Clamp(ruin?.Quality ?? 2, 1, 4) * 3, 83, 92));
            record.LifecycleState = recoveredState;
            record.LifecycleYear = Math.Max(record.LifecycleYear, year);
            if (string.IsNullOrWhiteSpace(record.SectDisplayName))
                record.SectDisplayName = recoveredDisplayName;
            record.Summary = "此法由“" + (ruin?.Name ?? "遗迹") + "”重入人间，以" + DisplayTags(record.LawTags) + "为本，可修至" + MclslRealmIds.Display(record.MaxRealm) + "。";
        }

        if (ruin != null)
        {
            ruin.SourceTechniqueId = MclslCultivationCatalog.NormalizeTechniqueId(technique.Id);
            ruin.SourceTechniqueName = technique.Name;
            ruin.LinkedLineageId = record.Id;
            ruin.SourceTechniqueRevivedYear = year;
        }
        MclslWorldArchiveStore.MarkDirty();
        return record.Id;
    }

    private static void TryCreateLineageRuin(int year, MclslWorldRunState run, MclslTechniqueLineageRecord record)
    {
        if (run?.SectRuins == null || !string.IsNullOrWhiteSpace(record.LinkedRuinId)) return;
        if (record.PeakPractitioners < 3 && MclslRealmIds.Index(record.PeakRealm) < MclslRealmIds.Index(MclslRealmIds.JinDan)) return;
        int sequence = MclslWorldRunRepository.NextProceduralSequence();
        int quality = Math.Clamp(1 + MclslRealmIds.Index(record.PeakRealm) / 2, 2, 4);
        bool newLaw = MclslWorldEpochSystem.IsNewLawActive(year);
        record.MaxRealm = MaxRealmAtLeast(record.MaxRealm, MclslRealmIds.JinDan);
        MclslSectRuinRecord ruin = MclslGeneratedObjectFactory.CreateSectRuin(year, sequence, newLaw ? "仙道传承断绝处" : "山河遗脉", "无主", MclslGeneratedObjectFactory.SplitTags(record.LawTags), quality, "散修遗府");
        ruin.Description = newLaw
            ? "仙道传承《" + record.Name + "》失传后，前人注疏、残卷与法器碎片沉积成遗府。"
            : "仙道传承《" + record.Name + "》余韵沉入山河，前人注疏、残卷与法器碎片汇成遗府。";
        ruin.Danger = Math.Clamp(ruin.Danger + quality * 8, 20, 95);
        ruin.SourceTechniqueId = string.IsNullOrWhiteSpace(record.SourceTechniqueId) ? record.Id : record.SourceTechniqueId;
        ruin.SourceTechniqueName = record.Name;
        ruin.LinkedLineageId = record.Id;
        ruin.SourceTechniqueLostYear = record.LostYear > 0 ? record.LostYear : year;
        if (!MclslWorldRunRepository.TryRegisterSectRuin(ruin)) return;
        record.LinkedRuinId = ruin.Id;
        record.LifecycleState = "遗府私传";
        record.LifecycleYear = Math.Max(record.LifecycleYear, year);
        if (string.IsNullOrWhiteSpace(record.SectDisplayName))
            record.SectDisplayName = BuildPrivateDisplayName(record.Name);
        MclslWorldRunRepository.AddEvent(year, "ancient_lineage_ruin_born", ruin.Name + "显世", ruin.Description);
    }

    private static bool EnsureLifecycle(MclslTechniqueLineageRecord record, int year)
    {
        if (record == null) return false;
        bool changed = false;
        if (string.IsNullOrWhiteSpace(record.SectDisplayName))
        {
            record.SectDisplayName = BuildSectDisplayName(record.Name);
            changed = true;
        }

        string expected = LifecycleFromState(record);
        if (!string.Equals(record.LifecycleState, expected, StringComparison.Ordinal))
        {
            record.LifecycleState = expected;
            record.LifecycleYear = Math.Max(record.LifecycleYear, year);
            changed = true;
        }
        else if (record.LifecycleYear <= 0)
        {
            record.LifecycleYear = Math.Max(1, record.FirstSeenYear > 0 ? record.FirstSeenYear : year);
            changed = true;
        }
        return changed;
    }

    private static string LifecycleFromState(MclslTechniqueLineageRecord record)
    {
        if (record == null) return "传承兴起";
        if (record.LifecycleState == "遗府私传" || record.LifecycleState == "秘境道统") return record.LifecycleState;
        string state = record.State ?? string.Empty;
        if (state == "名法流传" || state == "后世名法" || state == "融法") return "道统鼎盛";
        if (state == "复现") return "遗法复现";
        if (state == "失传") return string.IsNullOrWhiteSpace(record.LinkedRuinId) ? "道统断绝" : "遗府私传";
        if (!string.IsNullOrWhiteSpace(record.LinkedRuinId) && record.CurrentPractitioners <= 0) return "遗府私传";
        if (record.PeakPractitioners >= 50 || MclslRealmIds.Index(record.PeakRealm) >= MclslRealmIds.Index(MclslRealmIds.YuanYing)) return "道统鼎盛";
        if (record.PeakPractitioners >= 8 || record.CurrentPractitioners >= 8) return "法脉流传";
        return "传承兴起";
    }

    private static string BuildSectDisplayName(string techniqueName)
    {
        string root = SanitizeTechniqueName(techniqueName);
        return string.IsNullOrWhiteSpace(root) ? "无名道统" : root + "道统";
    }

    private static string BuildPrivateDisplayName(string techniqueName)
    {
        string root = SanitizeTechniqueName(techniqueName);
        return string.IsNullOrWhiteSpace(root) ? "无名私法" : root + "私法";
    }

    private static bool IsPrivateLineageReason(string reason)
    {
        return !string.IsNullOrWhiteSpace(reason) && (reason.Contains("遗府") || reason.Contains("洞府") || reason.Contains("坐化"));
    }

    private static bool IsSecretLineageReason(string reason)
    {
        return !string.IsNullOrWhiteSpace(reason) && reason.Contains("秘境");
    }

    private static bool IsSecretRuin(MclslSectRuinRecord ruin)
    {
        return !string.IsNullOrWhiteSpace(ruin?.Category) && ruin.Category.Contains("秘境");
    }

    private static string MaxRealmAtLeast(string realm, string minRealm)
    {
        int current = MclslRealmIds.Index(realm);
        int min = MclslRealmIds.Index(minRealm);
        if (current < min) return minRealm;
        return realm;
    }

    private static string SanitizeTechniqueName(string techniqueName)
    {
        string name = (techniqueName ?? string.Empty).Trim().Trim('《', '》');
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        string[] suffixes =
        {
            "炼形篇", "养神录", "参同契", "归元法", "真经", "玄经", "道书", "秘典", "心法",
            "诀", "经", "法", "篇", "录", "书", "章"
        };
        for (int i = 0; i < suffixes.Length; i++)
        {
            string suffix = suffixes[i];
            if (name.Length > suffix.Length + 1 && name.EndsWith(suffix, StringComparison.Ordinal))
                return name.Substring(0, name.Length - suffix.Length);
        }
        return name;
    }

    private static string StableTechniqueKey(string id, string name)
    {
        // 大人口新法异法在角色修炼与法不可同修计数中保持独立；
        // 玄黄仙录的传承档案按母法汇总，避免生成上千条重复法脉档案。
        if (!string.IsNullOrWhiteSpace(id)) return MclslCultivationCatalog.BaseTechniqueId(id);
        return "name_" + Math.Abs(StableHash(name)).ToString();
    }

    private static MclslTechniqueLineageRecord FindLineageRecord(MclslWorldRunState run, string id)
    {
        if (run?.TechniqueLineages == null || string.IsNullOrWhiteSpace(id)) return null;
        for (int i = 0; i < run.TechniqueLineages.Count; i++)
        {
            MclslTechniqueLineageRecord record = run.TechniqueLineages[i];
            if (record != null && string.Equals(record.Id, id, StringComparison.Ordinal)) return record;
        }
        return null;
    }

    private static void RemoveOldestOverflowLineage(List<MclslTechniqueLineageRecord> records)
    {
        if (records == null || records.Count == 0) return;
        int bestIndex = -1;
        int bestStateRank = int.MaxValue;
        int bestLastSeen = int.MaxValue;
        for (int i = 0; i < records.Count; i++)
        {
            MclslTechniqueLineageRecord record = records[i];
            if (record == null)
            {
                bestIndex = i;
                break;
            }
            int stateRank = record.State == "失传" ? 0 : 1;
            int lastSeen = record.LastSeenYear;
            if (bestIndex < 0 || stateRank < bestStateRank || (stateRank == bestStateRank && lastSeen < bestLastSeen))
            {
                bestIndex = i;
                bestStateRank = stateRank;
                bestLastSeen = lastSeen;
            }
        }
        if (bestIndex >= 0) records.RemoveAt(bestIndex);
    }

    private static string EventPrefix(MclslTechniqueLineageRecord record)
    {
        return record != null && record.SystemId == MclslCultivationSystemIds.NewLaw ? "newlaw_" : "ancient_";
    }

    private static string DisplayTags(string tags)
    {
        return string.IsNullOrWhiteSpace(tags) ? "本命道意" : tags.Replace(",", "、");
    }

    private static bool IsPlaceholderFounder(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return true;
        string value = name.Trim();
        return string.Equals(value, "上古散修", StringComparison.Ordinal)
            || string.Equals(value, "未知修士", StringComparison.Ordinal)
            || string.Equals(value, "无名修士", StringComparison.Ordinal);
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 17;
            foreach (char c in value ?? string.Empty) hash = hash * 31 + c;
            return hash;
        }
    }
}
