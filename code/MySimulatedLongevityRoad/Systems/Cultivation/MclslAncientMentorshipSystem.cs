using System;
using System.Collections.Generic;
using System.Globalization;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslAncientMentorshipSystem
{
    private const int MaxStudents = 3;
    private const int RecruitIntervalYears = 10;
    private static readonly StringComparer Ordinal = StringComparer.Ordinal;

    internal static bool TryRecruitFromTeacher(Actor teacher, int year, out Actor student)
    {
        student = null;
        if (!CanRecruit(teacher, year)) return false;

        CleanupTeacherStudents(teacher);
        IReadOnlyList<Actor> candidates = MclslCultivatorCandidateIndex.SelectCultivators(
            32,
            candidate => IsValidStudent(teacher, candidate),
            ScoreStudent);

        for (int i = 0; i < candidates.Count; i++)
        {
            Actor candidate = candidates[i];
            if (candidate == null || candidate == teacher) continue;
            Recruit(teacher, candidate, year);
            student = candidate;
            return true;
        }

        if (TryRecruitUninitiatedStudent(teacher, year, out student))
            return true;

        return false;
    }

    internal static string BuildSummary(Actor actor)
    {
        if (actor?.data == null || MclslWorldEpochSystem.IsNewLawActive(MclslRuntime.CurrentYear())) return string.Empty;
        List<string> parts = new(2);
        if (TryGetTeacher(actor, out Actor teacher))
            parts.Add("师承：" + SafeName(teacher));

        List<Actor> students = GetLiveStudents(actor);
        if (students.Count > 0)
        {
            int shown = Math.Min(3, students.Count);
            List<string> names = new(shown);
            for (int i = 0; i < shown; i++) names.Add(SafeName(students[i]));
            string suffix = students.Count > shown ? "等" + students.Count.ToString(CultureInfo.InvariantCulture) + "人" : string.Empty;
            parts.Add("弟子：" + string.Join("、", names) + suffix);
        }
        return parts.Count == 0 ? string.Empty : string.Join("｜", parts);
    }

    internal static List<(Actor Teacher, List<Actor> Students)> SnapshotActiveMentors(int limit)
    {
        List<(Actor Teacher, List<Actor> Students)> result = new();
        if (MclslWorldEpochSystem.IsNewLawActive(MclslRuntime.CurrentYear())) return result;
        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot();
        for (int i = 0; i < actors.Count; i++)
        {
            Actor teacher = actors[i];
            if (!IsAncientCultivator(teacher)) continue;
            List<Actor> students = GetLiveStudents(teacher);
            if (students.Count <= 0) continue;
            result.Add((teacher, students));
        }
        result.Sort((left, right) =>
        {
            int students = right.Students.Count.CompareTo(left.Students.Count);
            if (students != 0) return students;
            int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(right.Teacher)).CompareTo(MclslRealmIds.Index(MclslActorAccessor.Realm(left.Teacher)));
            if (realm != 0) return realm;
            return MclslActorAccessor.Id(left.Teacher).CompareTo(MclslActorAccessor.Id(right.Teacher));
        });
        if (limit > 0 && result.Count > limit)
            result.RemoveRange(limit, result.Count - limit);
        return result;
    }

    internal static bool TryGetTeacher(Actor student, out Actor teacher)
    {
        teacher = null;
        if (student?.data == null) return false;
        string raw = MclslActorAccessor.GetString(student, MclslActorDataKeys.AncientMentorTeacherId, string.Empty);
        if (!long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long teacherId) || teacherId <= 0L)
            return false;
        teacher = ResolveActor(teacherId);
        if (!IsAncientCultivator(teacher) || !MclslActorAccessor.Alive(teacher))
        {
            ClearTeacher(student);
            return false;
        }
        return true;
    }

    internal static List<Actor> GetLiveStudents(Actor teacher)
    {
        List<long> ids = GetStudentIds(teacher);
        List<Actor> result = new(ids.Count);
        bool changed = false;
        for (int i = ids.Count - 1; i >= 0; i--)
        {
            Actor student = ResolveActor(ids[i]);
            if (!IsAncientCultivator(student) || !HasTeacher(student, MclslActorAccessor.Id(teacher)))
            {
                ids.RemoveAt(i);
                changed = true;
                continue;
            }
            result.Add(student);
        }
        if (changed) SetStudentIds(teacher, ids);
        return result;
    }

    private static bool CanRecruit(Actor teacher, int year)
    {
        if (MclslWorldEpochSystem.IsNewLawActive(year)) return false;
        if (!IsAncientCultivator(teacher)) return false;
        if (MclslRealmIds.Index(MclslActorAccessor.Realm(teacher)) < MclslRealmIds.Index(MclslRealmIds.ZhuJi)) return false;
        CleanupTeacherStudents(teacher);
        if (GetStudentIds(teacher).Count >= MaxStudents) return false;
        int last = MclslActorAccessor.GetInt(teacher, MclslActorDataKeys.AncientMentorLastRecruitYear, -1);
        return last <= 0 || year - last >= RecruitIntervalYears;
    }

    private static bool IsValidStudent(Actor teacher, Actor candidate)
    {
        if (!IsAncientCultivator(teacher) || !IsAncientCultivator(candidate) || teacher == candidate) return false;
        if (TryGetTeacher(candidate, out _)) return false;
        int teacherRealm = MclslRealmIds.Index(MclslActorAccessor.Realm(teacher));
        int studentRealm = MclslRealmIds.Index(MclslActorAccessor.Realm(candidate));
        if (studentRealm < 0 || teacherRealm - studentRealm < 1) return false;
        return SameCity(teacher, candidate) || SameKingdom(teacher, candidate);
    }

    private static void Recruit(Actor teacher, Actor student, int year)
    {
        long teacherId = MclslActorAccessor.Id(teacher);
        long studentId = MclslActorAccessor.Id(student);
        List<long> ids = GetStudentIds(teacher);
        if (!ids.Contains(studentId)) ids.Add(studentId);
        if (ids.Count > MaxStudents) ids.RemoveRange(MaxStudents, ids.Count - MaxStudents);
        SetStudentIds(teacher, ids);
        MclslActorAccessor.Set(teacher, MclslActorDataKeys.AncientMentorLastRecruitYear, Math.Max(0, year));
        MclslActorAccessor.Set(student, MclslActorDataKeys.AncientMentorTeacherId, teacherId.ToString(CultureInfo.InvariantCulture));
        MclslActorAccessor.Set(student, MclslActorDataKeys.AncientMentorTeacherName, SafeName(teacher));

        string teacherTechniqueId = MclslActorAccessor.GetString(teacher, MclslActorDataKeys.TechniqueId, string.Empty);
        string teacherTechniqueName = MclslActorAccessor.GetString(teacher, MclslActorDataKeys.TechniqueName, string.Empty);
        if (!string.IsNullOrWhiteSpace(teacherTechniqueId)) MclslActorAccessor.Set(student, MclslActorDataKeys.TechniqueId, teacherTechniqueId);
        if (!string.IsNullOrWhiteSpace(teacherTechniqueName)) MclslActorAccessor.Set(student, MclslActorDataKeys.TechniqueName, teacherTechniqueName);
        MclslTechniqueRealmLimit.SetMaxRealm(student, MclslTechniqueRealmLimit.MaxRealm(teacher));
        MclslTechniqueStageSystem.AddProgress(student, 8);
        AddClamped(student, MclslActorDataKeys.AncientLineageStrength, 6, 0, 100);
        AddClamped(student, MclslActorDataKeys.MindState, 2, 0, 100);
    }

    private static bool TryRecruitUninitiatedStudent(Actor teacher, int year, out Actor student)
    {
        student = null;
        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetKnownActorsSnapshot();
        Actor best = null;
        int bestScore = int.MinValue;
        if (actors.Count == 0) return false;
        long seed = unchecked(MclslActorAccessor.Id(teacher) * 397L + year * 31L);
        int start = (int)((seed & long.MaxValue) % actors.Count);
        int scanBudget = Math.Min(256, actors.Count);
        for (int i = 0; i < scanBudget; i++)
        {
            Actor candidate = actors[(start + i) % actors.Count];
            if (!IsValidUninitiatedStudent(teacher, candidate)) continue;
            int score = ScoreUninitiatedStudent(candidate);
            if (score <= bestScore) continue;
            best = candidate;
            bestScore = score;
        }
        if (best == null) return false;

        BeginStudentCultivationFromTeacher(teacher, best, year);
        Recruit(teacher, best, year);
        student = best;
        return true;
    }

    private static bool IsValidUninitiatedStudent(Actor teacher, Actor candidate)
    {
        if (!IsAncientCultivator(teacher) || candidate == null || candidate == teacher) return false;
        if (!MclslActorAccessor.Alive(candidate) || !MclslEligibility.CanCultivate(candidate)) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(candidate))) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(candidate, MclslActorDataKeys.CultivationSystem, string.Empty))) return false;
        if (SafeAge(candidate) < 12f) return false;
        if (!SameCity(teacher, candidate) && !SameKingdom(teacher, candidate)) return false;
        int aptitude = MclslActorAccessor.GetInt(candidate, MclslActorDataKeys.Aptitude, 0);
        int fate = MclslActorAccessor.GetInt(candidate, MclslActorDataKeys.ImmortalFate, 0);
        return aptitude > 0 || fate > 0;
    }

    private static void BeginStudentCultivationFromTeacher(Actor teacher, Actor student, int year)
    {
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(student, MclslActorDataKeys.Aptitude, 0), 1, 100);
        if (aptitude <= 1)
            aptitude = Math.Clamp(MclslActorAccessor.GetInt(student, MclslActorDataKeys.ImmortalFate, 35), 1, 100);
        string techniqueId = MclslActorAccessor.GetString(teacher, MclslActorDataKeys.TechniqueId, string.Empty);
        string techniqueName = MclslActorAccessor.GetString(teacher, MclslActorDataKeys.TechniqueName, "师传仙法");
        MclslCultivationStateTransitions.TrySetCultivationSystem(student, MclslCultivationSystemIds.AncientLaw);
        MclslActorAccessor.Set(student, MclslActorDataKeys.AncientLawStatus, "仙道正修");
        MclslActorAccessor.Set(student, MclslActorDataKeys.Aptitude, aptitude);
        MclslActorAccessor.Set(student, MclslActorDataKeys.ImmortalFate, Math.Max(aptitude, MclslActorAccessor.GetInt(student, MclslActorDataKeys.ImmortalFate, 0)));
        MclslActorAccessor.Set(student, MclslActorDataKeys.MortalSeparationChecked, 1);
        MclslActorAccessor.Set(student, MclslActorDataKeys.TechniqueId, techniqueId);
        MclslActorAccessor.Set(student, MclslActorDataKeys.TechniqueName, techniqueName);
        MclslTechniqueRealmLimit.SetMaxRealm(student, MclslTechniqueRealmLimit.MaxRealm(teacher));
        MclslActorAccessor.Set(student, MclslActorDataKeys.AncientLineageStrength, Math.Clamp(22 + aptitude / 3, 15, 70));
        MclslActorAccessor.Set(student, MclslActorDataKeys.AncientLegacyPotential, Math.Clamp(18 + aptitude / 4, 12, 60));
        MclslTechniqueStageSystem.SetProgress(student, Math.Clamp(12 + aptitude / 10, 10, 28));
        MclslMindSystem.EnsureMindState(student);
        MySimulatedLongevityRoad.Traits.MclslTraitRegistration.SyncGiftTrait(student, aptitude);
        MclslActorAccessor.Set(student, MclslActorDataKeys.LastBreakthroughResult, "仙师授法，开始感气");
        MclslSensingQiSystem.ProcessAnnual(student, year, true);
    }

    private static void CleanupTeacherStudents(Actor teacher)
    {
        if (teacher?.data == null) return;
        List<long> ids = GetStudentIds(teacher);
        if (ids.Count == 0) return;
        bool changed = false;
        for (int i = ids.Count - 1; i >= 0; i--)
        {
            Actor student = ResolveActor(ids[i]);
            if (!IsAncientCultivator(student) || !HasTeacher(student, MclslActorAccessor.Id(teacher)))
            {
                ids.RemoveAt(i);
                changed = true;
            }
        }
        if (changed) SetStudentIds(teacher, ids);
    }

    private static bool HasTeacher(Actor actor, long teacherId)
    {
        string raw = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientMentorTeacherId, string.Empty);
        return long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long id) && id == teacherId;
    }

    private static void ClearTeacher(Actor actor)
    {
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientMentorTeacherId, string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientMentorTeacherName, string.Empty);
    }

    private static List<long> GetStudentIds(Actor teacher)
    {
        List<long> result = new(MaxStudents);
        string raw = MclslActorAccessor.GetString(teacher, MclslActorDataKeys.AncientMentorStudentIds, string.Empty);
        if (string.IsNullOrWhiteSpace(raw)) return result;
        string[] parts = raw.Split(',');
        for (int i = 0; i < parts.Length && result.Count < MaxStudents; i++)
            if (long.TryParse((parts[i] ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long id) && id > 0L && !result.Contains(id))
                result.Add(id);
        return result;
    }

    private static void SetStudentIds(Actor teacher, IReadOnlyList<long> ids)
    {
        if (teacher?.data == null || ids == null || ids.Count == 0)
        {
            MclslActorAccessor.Set(teacher, MclslActorDataKeys.AncientMentorStudentIds, string.Empty);
            return;
        }
        List<string> parts = new(ids.Count);
        for (int i = 0; i < ids.Count; i++)
            if (ids[i] > 0L) parts.Add(ids[i].ToString(CultureInfo.InvariantCulture));
        MclslActorAccessor.Set(teacher, MclslActorDataKeys.AncientMentorStudentIds, string.Join(",", parts));
    }

    private static bool IsAncientCultivator(Actor actor)
    {
        return MclslActorAccessor.Alive(actor)
            && MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) == MclslCultivationSystemIds.AncientLaw
            && !string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor));
    }

    private static int ScoreStudent(Actor actor)
    {
        return MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50) * 100
            + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MindState, 50) * 4
            + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLegacyPotential, 0)
            + Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor))) * 10;
    }

    private static int ScoreUninitiatedStudent(Actor actor)
    {
        return MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0) * 100
            + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ImmortalFate, 0) * 20
            + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MindState, 50);
    }

    private static float SafeAge(Actor actor)
    {
        try { return actor?.getAge() ?? 0f; }
        catch { return 0f; }
    }

    private static bool SameCity(Actor left, Actor right)
    {
        return left?.city != null && right?.city != null && ReferenceEquals(left.city, right.city);
    }

    private static bool SameKingdom(Actor left, Actor right)
    {
        return left?.kingdom != null && right?.kingdom != null && ReferenceEquals(left.kingdom, right.kingdom);
    }

    private static Actor ResolveActor(long actorId)
    {
        if (actorId <= 0L) return null;
        if (MclslCultivatorCandidateIndex.Resolve(actorId, out Actor indexed))
            return indexed;
        try { return World.world?.units?.get(actorId); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslAncientMentorshipSystem-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslAncientMentorshipSystem.cs #1: " + mclslEmptyCatchEx.Message); }
        return null;
    }

    private static void AddClamped(Actor actor, string key, int delta, int min, int max)
    {
        int value = MclslActorAccessor.GetInt(actor, key, min);
        MclslActorAccessor.Set(actor, key, Math.Clamp(value + delta, min, max));
    }

    private static string SafeName(Actor actor)
    {
        string name = actor?.getName();
        return string.IsNullOrWhiteSpace(name) ? "无名仙修" : name.Trim();
    }
}
