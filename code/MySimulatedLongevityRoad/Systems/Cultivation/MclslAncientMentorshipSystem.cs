using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        Actor candidate = MclslActorProjectionIndex.PickMentorshipStudent(teacher);
        if (candidate != null)
        {
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
        if (actor?.data == null) return string.Empty;
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

    internal static bool HasLivingTeacher(Actor student)
    {
        string raw = MclslActorAccessor.GetString(student, MclslActorDataKeys.AncientMentorTeacherId);
        return long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long id)
            && MclslActorRegistry.Resolve(id, out Actor teacher) && IsAncientCultivator(teacher);
    }

    internal static void OnActorRemoved(Actor teacher)
    {
        if (teacher?.data == null) return;
        long teacherId = MclslActorAccessor.Id(teacher);
        foreach (long id in GetStudentIds(teacher))
        {
            if (!MclslActorRegistry.Resolve(id, out Actor student)) continue;
            if (MclslActorAccessor.GetString(student, MclslActorDataKeys.AncientMentorTeacherId)
                != teacherId.ToString(CultureInfo.InvariantCulture)) continue;
            ClearTeacher(student);
            MclslActorProjectionIndex.Update(student);
        }
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
        if (!IsAncientCultivator(teacher)) return false;
        if (MclslRealmIds.Index(MclslActorAccessor.Realm(teacher)) < MclslRealmIds.Index(MclslRealmIds.ZhuJi)) return false;
        CleanupTeacherStudents(teacher);
        if (GetStudentIds(teacher).Count >= MaxStudents) return false;
        int last = MclslActorAccessor.GetInt(teacher, MclslActorDataKeys.AncientMentorLastRecruitYear, -1);
        return last <= 0 || year - last >= RecruitIntervalYears;
    }

    internal static bool IsValidStudent(Actor teacher, Actor candidate)
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
        MclslActorProjectionIndex.Update(student);

        string teacherTechniqueId = MclslActorAccessor.GetString(teacher, MclslActorDataKeys.TechniqueId, string.Empty);
        string teacherTechniqueName = MclslActorAccessor.GetString(teacher, MclslActorDataKeys.TechniqueName, string.Empty);
        if (!string.IsNullOrWhiteSpace(teacherTechniqueId)) MclslActorAccessor.Set(student, MclslActorDataKeys.TechniqueId, teacherTechniqueId);
        if (!string.IsNullOrWhiteSpace(teacherTechniqueName)) MclslActorAccessor.Set(student, MclslActorDataKeys.TechniqueName, teacherTechniqueName);
        MclslTechniqueRealmLimit.SetMaxRealm(student, MclslTechniqueRealmLimit.MaxRealm(teacher));
        MclslTechniqueStageSystem.AddProgress(student, 8);
        AddClamped(student, MclslActorDataKeys.AncientLineageStrength, 6, 0, 100);
        AddClamped(student, MclslActorDataKeys.MindState, 2, 0, 100);
        EnsureMentorshipBooks(teacher, student, year);
    }

    internal static void ProcessAnnual(Actor actor, int year)
    {
        if (!IsAncientCultivator(actor)) return;
        if (TryGetTeacher(actor, out Actor teacher))
        {
            EnsureMentorshipBooks(teacher, actor, year);
            UpdateBookProgress(actor, year);
        }

        List<Actor> students = GetLiveStudents(actor);
        if (students.Count == 0) return;
        int last = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientMentorLastTransmissionYear, 0);
        int recruited = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientMentorLastRecruitYear, 0);
        if (year - Math.Max(last, recruited) < RecruitIntervalYears) return;
        foreach (Actor student in students)
        {
            EnsureMentorshipBooks(actor, student, year);
            UpdateBookProgress(student, year);
            TransmitProfessionGift(actor, student, year);
        }
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientMentorLastTransmissionYear, year);
    }

    private static void EnsureMentorshipBooks(Actor teacher, Actor student, int year)
    {
        if (teacher?.data == null || student?.data == null) return;
        long teacherId = MclslActorAccessor.Id(teacher);
        long studentId = MclslActorAccessor.Id(student);
        MclslBagState bag = MclslBagSystem.Read(student);
        bag.Books ??= new List<MclslMentorshipBook>();
        string techniqueId = MclslActorAccessor.GetString(teacher, MclslActorDataKeys.TechniqueId, string.Empty);
        string techniqueName = MclslActorAccessor.GetString(teacher, MclslActorDataKeys.TechniqueName, "师传仙法");
        int recruitYear = MclslActorAccessor.GetInt(teacher, MclslActorDataKeys.AncientMentorLastRecruitYear, year);
        bool changed = UpsertBook(bag, "formula:" + teacherId + ":" + studentId, teacher, student, techniqueId,
            "师传法诀·" + techniqueName, recruitYear);
        changed |= UpsertBook(bag, "insight:" + teacherId + ":" + studentId, teacher, student, techniqueId,
            "修行心得·" + SafeName(teacher), recruitYear);
        if (changed) MclslBagSystem.Write(student, bag);
    }

    private static bool UpsertBook(MclslBagState bag, string id, Actor teacher, Actor student,
        string techniqueId, string name, int year)
    {
        MclslMentorshipBook book = bag.Books.FirstOrDefault(x => x.BookId == id);
        bool changed = false;
        if (book == null)
        {
            book = new MclslMentorshipBook { BookId = id, StartYear = Math.Max(0, year), Progress = 0 };
            bag.Books.Add(book);
            changed = true;
        }
        string teacherName = SafeName(teacher);
        if (book.TeacherId != MclslActorAccessor.Id(teacher)) { book.TeacherId = MclslActorAccessor.Id(teacher); changed = true; }
        if (book.TeacherName != teacherName) { book.TeacherName = teacherName; changed = true; }
        if (book.TargetId != MclslActorAccessor.Id(student)) { book.TargetId = MclslActorAccessor.Id(student); changed = true; }
        if (book.TechniqueId != techniqueId) { book.TechniqueId = techniqueId; changed = true; }
        if (book.TechniqueName != name) { book.TechniqueName = name; changed = true; }
        return changed;
    }

    private static void UpdateBookProgress(Actor student, int year)
    {
        MclslBagState bag = MclslBagSystem.Read(student);
        if (bag?.Books == null) return;
        bool changed = false;
        foreach (MclslMentorshipBook book in bag.Books)
        {
            if (book == null || book.TargetId != MclslActorAccessor.Id(student)) continue;
            int progress = Math.Clamp((year - book.StartYear) * 2, 0, 100);
            if (progress <= book.Progress) continue;
            book.Progress = progress;
            changed = true;
        }
        if (changed) MclslBagSystem.Write(student, bag);
        if (changed) MclslTechniqueStageSystem.AddProgress(student, 1);
    }

    private static void TransmitProfessionGift(Actor teacher, Actor student, int year)
    {
        string profession = MclslProfessionSystem.FromTrait(teacher);
        if (string.IsNullOrWhiteSpace(profession))
            profession = MclslActorAccessor.GetString(teacher, MclslActorDataKeys.Profession, string.Empty);
        int grade = Math.Clamp(MclslProfessionSystem.GetGrade(teacher), 1, 4);
        string itemId;
        if (profession == MclslProfessionSystem.Alchemist)
            itemId = grade switch { 1 => "D006", 2 => "D010", 3 => "D007", _ => "D011" };
        else if (profession == MclslProfessionSystem.TalismanMaker)
            itemId = grade switch { 1 => "F002", 2 => "F001", 3 => "F005", _ => "F007" };
        else if (profession == MclslProfessionSystem.Refiner)
        {
            string preferred = grade switch { 1 => "B010", 2 => "B002", 3 => "B003", _ => "B008" };
            itemId = MclslBagSystem.Count(student, preferred) == 0 ? preferred : "R0" + Math.Min(6, grade + 2);
        }
        else itemId = grade >= 3 ? "A04" : "A07";

        if (itemId.StartsWith("R", StringComparison.Ordinal))
        {
            // R-tier craft resources are translated into the matching stored material.
            itemId = grade switch { 1 => "A07", 2 => "A08", 3 => "A09", _ => "A05" };
        }
        MclslBagSystem.Add(student, itemId, acquiredYear: year);
        MclslWorldRunRepository.AddItemAcquisitionEvent(year, student, itemId, 1, "师徒赠予");
    }

    private static bool TryRecruitUninitiatedStudent(Actor teacher, int year, out Actor student)
    {
        student = null;
        Actor best = MclslActorProjectionIndex.PickUninitiatedStudent(teacher);
        if (best == null) return false;

        BeginStudentCultivationFromTeacher(teacher, best, year);
        Recruit(teacher, best, year);
        student = best;
        return true;
    }

    internal static bool IsAvailableUninitiatedStudent(Actor actor)
    {
        return MclslActorAccessor.Alive(actor) && MclslEligibility.CanCultivate(actor)
            && string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor))
            && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem))
            && SafeAge(actor) >= 12f
            && (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude) > 0
                || MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ImmortalFate) > 0);
    }
    internal static void OnCultivationChanged(Actor actor)
    {
        if (!IsAncientCultivator(actor)) OnActorRemoved(actor);
    }

    internal static bool IsValidUninitiatedStudent(Actor teacher, Actor candidate)
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

    internal static int ScoreStudent(Actor actor)
    {
        return MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50) * 100
            + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MindState, 50) * 4
            + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLegacyPotential, 0)
            + Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor))) * 10;
    }

    internal static int ScoreUninitiatedStudent(Actor actor)
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
