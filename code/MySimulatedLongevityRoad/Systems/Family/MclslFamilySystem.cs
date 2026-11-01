using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static partial class MclslFamilySystem
{
    private static string TreasuryAccount(MclslFamilyRecord family) => "family/" + family.Id;

    internal static long TreasuryStones(MclslFamilyRecord family)
        => family == null ? 0 : MclslWorldRunRepository.Current.Economy.Balance(TreasuryAccount(family), MclslCurrency.SpiritStone);

    private static MclslEconomicResult MoveStones(MclslFamilyRecord family, Actor actor, long amount,
        bool deposit, int year, string source = null, long cursor = 0)
    {
        string actorAccount = MclslEconomyCommands.Account(actor);
        string familyAccount = TreasuryAccount(family);
        return MclslEconomyCommands.Commit(year, new[]
        {
            new MclslEconomicOperation(MclslEconomicKind.Transfer, MclslCurrency.SpiritStone, amount,
                deposit ? actorAccount : familyAccount, deposit ? familyAccount : actorAccount)
        }, source, cursor);
    }
    private static MclslWorldRunState _indexedRun;
    private static readonly Dictionary<string, MclslFamilyRecord> ById = new(StringComparer.Ordinal);
    private static readonly Dictionary<long, MclslFamilyRecord> ByActor = new();
    private static readonly Dictionary<long, MclslFamilyRecord> ByNative = new();
    // 父系模式下的运行时索引：以“已知根祖”角色 ID 归并父系家族。
    private static readonly Dictionary<long, MclslFamilyRecord> ByPaternalRoot = new();
    private static readonly List<MclslFamilyRecord> VisibleFamilies = new();
    private static readonly Dictionary<(string Family, long Actor), MclslFamilyMemberRecord> MembersById = new();
    // 被持续清理删除的原生家族 ID 墓碑：阻止正常观察逻辑在删除后立刻重建同一家族。
    private static readonly HashSet<long> RemovedNativeFamilyIds = new();

    internal static void Clear()
    {
        _indexedRun = null;
        ResetReconciliation();
        ResetFamilyStocks();
        ById.Clear(); ByActor.Clear(); ByNative.Clear(); ByPaternalRoot.Clear(); VisibleFamilies.Clear(); MembersById.Clear();
        RemovedNativeFamilyIds.Clear();
        ClearFamilyViews();
    }

    private static MclslWorldRunState EnsureIndex()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null) return null;
        if (ReferenceEquals(_indexedRun, run)) return run;
        Clear();
        _indexedRun = run;
        run.Families ??= new List<MclslFamilyRecord>();
        bool dirty = false;
        foreach (MclslFamilyRecord family in run.Families.OrderBy(x => x?.FoundedYear ?? 0))
        {
            if (family == null || string.IsNullOrWhiteSpace(family.Id)) continue;
            family.Members ??= new List<MclslFamilyMemberRecord>();
            family.Items ??= new List<MclslOwnedItem>();
            family.TechniqueIds ??= new List<string>();
            family.TechniqueDetails ??= new Dictionary<string, MclslFamilyTechniqueRecord>();
            family.CityInfluence ??= new Dictionary<long, int>();
            family.History ??= new List<string>();
            // 迁移：家族纪事只保留最近 15 条，旧档超出部分丢弃。
            if (family.History.Count > HistoryLimit)
            {
                family.History.RemoveRange(0, family.History.Count - HistoryLimit);
                dirty = true;
            }
            VisibleFamilies.Add(family);
            ById[family.Id] = family;
            if (family.NativeFamilyId > 0 && !ByNative.ContainsKey(family.NativeFamilyId)) ByNative[family.NativeFamilyId] = family;
            if (family.PaternalRootActorId > 0 && !ByPaternalRoot.ContainsKey(family.PaternalRootActorId))
                ByPaternalRoot[family.PaternalRootActorId] = family;
            // 迁移：凡人不再逐人记录。先移除旧档中残留的凡人/空成员记录（不进入索引），
            // 其数量改由成员视图派生的 MortalCount 统计；修士记录保持不变。
            int mortalRecords = family.Members.RemoveAll(m => m == null || !m.IsCultivator);
            if (mortalRecords > 0) dirty = true;
            foreach (MclslFamilyMemberRecord member in family.Members)
                if (member != null && member.ActorId > 0)
                { ByActor[member.ActorId] = family; MembersById[(family.Id, member.ActorId)] = member; QueueMember(member.ActorId); }
            QueueFamily(family);
            dirty |= RepairFamilyNames(family);
            dirty |= InitializeFamilyTechniques(family);
            // 加载即建立家库运行时索引；家库为空时也建空索引，后续写入口直接增量维护。
            FamilyStock(family);
        }
        if (run.FamilyFeatureVersion < 1)
        {
            // 一次性迁移：把既有非零关注对象视作手动指定，并复位扶持计时。
            foreach (MclslFamilyRecord family in VisibleFamilies)
            {
                if (family.FocusActorId != 0) family.FocusIsManual = true;
                family.LastAidYear = -1;
                family.LastAidAttemptYear = -1;
            }
            run.FamilyFeatureVersion = 1;
            dirty = true;
        }
        if (dirty) MclslWorldArchiveStore.MarkDirty();
        return run;
    }

    internal static IReadOnlyList<MclslFamilyRecord> Families
        => EnsureIndex() is MclslWorldRunState ? VisibleFamilies : Array.Empty<MclslFamilyRecord>();

    internal static MclslFamilyRecord Find(string familyId)
    {
        EnsureIndex();
        return familyId != null && ById.TryGetValue(familyId, out MclslFamilyRecord family) ? family : null;
    }

    internal static MclslFamilyRecord FamilyOf(Actor actor)
    {
        EnsureIndex();
        long id = MclslActorAccessor.Id(actor);
        return id > 0 && ByActor.TryGetValue(id, out MclslFamilyRecord family) ? family : null;
    }

    internal static void Observe(Actor actor)
    {
        // 家主/扶持重算只由 Observe(Actor,int) 在成员实际变化或新增时 QueueFamily 入队，
        // 避免 UI 刷新或普通功法元数据观察每次都重选家主（ChangedFamilies 已按族去重）。
        Observe(actor, 0);
    }

    private static MclslFamilyRecord Observe(Actor actor, int depth)
    {
        if(MySimulatedLongevityRoad.Traits.MclslBeastActorRegistration.IsOrdinary(actor))return null;
        if (MclslWorldArchiveStore.WriteLocked || EnsureIndex() is not MclslWorldRunState run
            || !MclslActorAccessor.Alive(actor) || depth > 8) return null;
        long id = MclslActorAccessor.Id(actor);
        if (id <= 0) return null;
        if (run.FamilyRuleMode == MclslFamilyRuleMode.Patrilineal) return ObservePaternal(actor, id, run);
        bool cultivator = MclslActorAccessor.IsCultivator(actor);
        // 凡人不再逐人记录/判定：不建立成员记录、不刷新、不参与家主/结算/遗产。
        // 仅返回其已知家族（若有），供调用方判断归属；数量由成员视图派生的 MortalCount 统计。
        if (!cultivator)
            return ByActor.TryGetValue(id, out MclslFamilyRecord mortalKnown) ? mortalKnown : null;
        long nativeFamily = actor.data.family > 0 ? actor.data.family : actor.data.ancestor_family;
        ByActor.TryGetValue(id, out MclslFamilyRecord known);
        if (known != null && (nativeFamily <= 0 || known.NativeFamilyId == nativeFamily))
        {
            RefreshMember(known, actor);
            return known;
        }

        long first = actor.data.parent_id_1, second = actor.data.parent_id_2;
        Actor firstActor = first > 0 ? World.world?.units?.get(first) : null;
        Actor secondActor = second > 0 ? World.world?.units?.get(second) : null;
        long father = ParentOfSex(firstActor, first, ActorSex.Male) ? first
            : ParentOfSex(secondActor, second, ActorSex.Male) ? second : 0;
        long mother = ParentOfSex(firstActor, first, ActorSex.Female) ? first
            : ParentOfSex(secondActor, second, ActorSex.Female) ? second : 0;
        MclslFamilyRecord family = null;
        if (nativeFamily > 0) ByNative.TryGetValue(nativeFamily, out family);
        else family = ParentFamily(father, depth) ?? ParentFamily(mother, depth);
        if (family == null)
        {
            // 被持续清理删除的原生家族不能被下一次正常观察立刻重建：
            // 保留角色已有的家族归属，否则让该角色暂时脱离模组家族索引。
            if (nativeFamily > 0 && RemovedNativeFamilyIds.Contains(nativeFamily))
            {
                if (known != null) { RefreshMember(known, actor); return known; }
                return null;
            }
            if (!cultivator && known == null) return null;
            string familyId = run.RunId + ":family:" + Guid.NewGuid().ToString("N");
            family = new MclslFamilyRecord
            {
                Id = familyId, Surname = MclslHonorificNameCatalog.StableFamilySurname(SafeName(actor), id),
                FounderActorId = id,
                NativeFamilyId = Math.Max(0, nativeFamily), FoundedYear = Math.Max(0, MclslRuntime.CurrentYear()),
                LastAidYear = -1, LastAidAttemptYear = -1
            };
            family.Name = family.Surname + "氏";
            run.Families.Add(family);
            VisibleFamilies.Add(family);
            ById[familyId] = family;
            if (nativeFamily > 0) ByNative[nativeFamily] = family;
            QueueRelatives(family, id);
            AddHistory(family, MclslRuntime.CurrentYear() + "年 · " + family.Name + "立族");
        }
        if (known != null && known != family)
        {
            MclslFamilyMemberRecord moved = Member(known, id);
            if (moved != null)
            {
                known.Members.Remove(moved);
                if (!known.Members.Any(member => member != null && (member.Alive || member.Missing)))
                    known.RemoveAfterLiquidation = true;
                MembersById.Remove((known.Id, id));
                MembersById[(family.Id, id)] = moved;
                QueueFamily(known);
                family.Members.Add(moved);
                InvalidateMemberView(known); InvalidateMemberView(family);
                ByActor[id] = family;
                if (known.PreferredHeadActorId == id) known.PreferredHeadActorId = 0;
                if (known.FocusActorId == id) known.FocusActorId = 0;
                MclslActorAccessor.Set(actor, MclslActorDataKeys.FamilyId, family.Id);
                RefreshMember(family, actor);
                QueueFamily(family);
                NotifyChanged();
                return family;
            }
        }
        int generation = 1;
        if (father > 0 && ByActor.TryGetValue(father, out MclslFamilyRecord fatherFamily) && fatherFamily == family)
            generation = Math.Max(generation, Member(family, father)?.Generation + 1 ?? 1);
        else if (mother > 0 && ByActor.TryGetValue(mother, out MclslFamilyRecord motherFamily) && motherFamily == family)
            generation = Math.Max(generation, Member(family, mother)?.Generation + 1 ?? 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FamilyId, family.Id);
        string memberName = EnsureActorSurname(family, actor);
        var addedMember = new MclslFamilyMemberRecord
        {
            ActorId = id, Name = memberName, ParentId1 = first, ParentId2 = second,
            Sex = (int)actor.data.sex,
            Generation = generation, RealmIndex = MclslRealmIds.Index(MclslActorAccessor.Realm(actor)),
            Aptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50),
            CityId = actor.city?.data?.id ?? 0,
            IsCultivator = cultivator
        };
        family.Members.Add(addedMember);
        InvalidateMemberView(family);
        MembersById[(family.Id, id)] = addedMember;
        foreach (long child in MclslActorRelations.Descendants(id)) QueueMember(child);
        ByActor[id] = family;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FamilyId, family.Id);
        RecordTechnique(family, actor);
        family.Extinct = false;
        family.RemoveAfterLiquidation = false;
        // 普通补录/新增成员不立即重算家主，统一交给 ChangedFamilies 每族一次。
        QueueFamily(family);
        NotifyChanged();
        return family;
    }

    private static bool ParentOfSex(Actor actor, long id, ActorSex sex)
        => id > 0 && (actor?.data?.sex == sex ||
            ByActor.TryGetValue(id, out MclslFamilyRecord family) && Member(family, id)?.Sex == (int)sex);

    private static MclslFamilyRecord ParentFamily(long parentId, int depth)
    {
        if (parentId <= 0) return null;
        if (ByActor.TryGetValue(parentId, out MclslFamilyRecord family)) return family;
        Actor parent = World.world?.units?.get(parentId);
        return parent != null && MclslActorAccessor.IsCultivator(parent) ? Observe(parent, depth + 1) : null;
    }

    private static bool RepairFamilyNames(MclslFamilyRecord family)
    {
        MclslFamilyMemberRecord founder = family.Members.Find(x => x?.ActorId == family.FounderActorId);
        string source = founder?.Name;
        if (string.IsNullOrWhiteSpace(source) && family.FounderActorId > 0
            && MclslActorRegistry.ResolveKnownOrWorld(family.FounderActorId, out Actor actor)) source = SafeName(actor);
        if (string.IsNullOrWhiteSpace(source)) source = family.Name?.TrimEnd('氏');
        string surname = string.IsNullOrWhiteSpace(family.Surname)
            ? MclslHonorificNameCatalog.StableFamilySurname(source, family.FounderActorId)
            : family.Surname;
        bool changed = !string.Equals(family.Surname, surname, StringComparison.Ordinal);
        family.Surname = surname;
        string previousFamilyName = family.Name;
        string familyName = surname + "氏";
        if (!string.Equals(family.Name, familyName, StringComparison.Ordinal))
        {
            family.Name = familyName;
            ReplaceFamilyHistoryText(family, previousFamilyName, familyName);
            changed = true;
        }
        foreach (MclslFamilyMemberRecord member in family.Members)
        {
            if (member == null || member.ActorId <= 0) continue;
            string previousName = member.Name;
            string corrected = MclslHonorificNameCatalog.ApplyFamilySurname(member.ActorId, member.Name, surname);
            if (!string.Equals(member.Name, corrected, StringComparison.Ordinal))
            {
                member.Name = corrected;
                ReplaceFamilyHistoryText(family, previousName, corrected);
                changed = true;
            }
            if (!member.Alive || !MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor living)
                || !MclslActorAccessor.Alive(living)) continue;
            string liveName = EnsureActorSurname(family, living);
            if (!string.Equals(member.Name, liveName, StringComparison.Ordinal))
            {
                ReplaceFamilyHistoryText(family, member.Name, liveName);
                member.Name = liveName;
                changed = true;
            }
        }
        return changed;
    }

    private static void ReplaceFamilyHistoryText(MclslFamilyRecord family, string before, string after)
    {
        if (string.IsNullOrWhiteSpace(before) || string.IsNullOrWhiteSpace(after) || before == after) return;
        for (int i = 0; i < family.History.Count; i++)
            if (family.History[i]?.Contains(before, StringComparison.Ordinal) == true)
                family.History[i] = family.History[i].Replace(before, after, StringComparison.Ordinal);
    }

    private static string EnsureActorSurname(MclslFamilyRecord family, Actor actor)
    {
        if (actor == null) return string.Empty;
        string current = SafeName(actor);
        string corrected = MclslHonorificNameCatalog.ApplyFamilySurname(MclslActorAccessor.Id(actor), current, family.Surname);
        if (!string.Equals(current, corrected, StringComparison.Ordinal))
            MclslActorAccessor.ApplyDisplayName(actor, MclslActorAccessor.Realm(actor));
        return SafeName(actor);
    }

    private static string SafeName(Actor actor)
    {
        try { return actor?.getName() ?? "无名修士"; }
        catch { return "无名修士"; }
    }

    private static MclslFamilyMemberRecord Member(MclslFamilyRecord family, long actorId)
        => family != null && MembersById.TryGetValue((family.Id, actorId), out var member) ? member : null;

    private static void RefreshMember(MclslFamilyRecord family, Actor actor)
    {
        MclslFamilyMemberRecord member = Member(family, MclslActorAccessor.Id(actor));
        if (member == null || member.DeathSettled) return;
        bool wasAlive = member.Alive, wasMissing = member.Missing, wasCultivator = member.IsCultivator;
        bool wasExtinct = family.Extinct;
        string previousName = member.Name;
        long previousCity = member.CityId;
        int previousRealm = member.RealmIndex;
        int previousAptitude = member.Aptitude;
        member.IsCultivator = MclslActorAccessor.IsCultivator(actor);
        member.Name = EnsureActorSurname(family, actor);
        member.Sex = (int)actor.data.sex;
        member.RealmIndex = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        member.Aptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50);
        member.CityId = actor.city?.data?.id ?? 0;
        member.Alive = true;
        member.Missing = false;
        family.Extinct = false;
        family.RemoveAfterLiquidation = false;
        if (MclslActorAccessor.GetString(actor, MclslActorDataKeys.FamilyId) != family.Id)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.FamilyId, family.Id);
        if (member.IsCultivator) RecordTechnique(family, actor);
        // 家主/扶持只在实际影响任职资格的字段（姓名/状态/realm/资质/家族绝嗣复位）变化时重算；
        // 普通功法元数据刷新不触发 QueueFamily。
        bool headRelevant = !wasAlive || wasMissing || wasExtinct || wasCultivator != member.IsCultivator
            || previousRealm != member.RealmIndex || previousAptitude != member.Aptitude
            || !string.Equals(previousName, member.Name, StringComparison.Ordinal);
        bool visualChange = headRelevant || previousCity != member.CityId;
        if (headRelevant) { InvalidateMemberView(family); QueueFamily(family); }
        if (visualChange) NotifyChanged();
    }

    private static void EnsureHead(MclslFamilyRecord family)
    {
        if (family == null) return;
        long sample = MclslPerformanceProbe.Begin();
        try
        {
            long next = 0;
            if (IsAdultLiveMember(family, family.PreferredHeadActorId)) next = family.PreferredHeadActorId;
            if (next == 0)
            {
                int bestRealm = -1;
                double bestPower = -1;
                foreach (MclslFamilyMemberRecord member in family.Members)
                {
                    if (member == null || !IsAdultLiveMember(family, member.ActorId)) continue;
                    MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor candidate);
                    double power = FamilyPower(candidate);
                    if (member.RealmIndex > bestRealm || member.RealmIndex == bestRealm
                        && (power > bestPower || power == bestPower && (next == 0 || member.ActorId < next)))
                    { bestRealm = member.RealmIndex; bestPower = power; next = member.ActorId; }
                }
            }
            if (family.HeadActorId == next) return;
            family.HeadActorId = next;
            if (next > 0) AddHistory(family, MclslRuntime.CurrentYear() + "年 · " + (Member(family, next)?.Name ?? "修士") + "继任家主");
            NotifyChanged();
        }
        finally { MclslPerformanceProbe.End("family.head.recompute", sample); }
    }

    private static double FamilyPower(Actor actor)
    {
        return actor?.stats == null ? 0d : Math.Max(0f, actor.stats["damage"]) + Math.Max(0f, actor.stats["health"]);
    }

    private static bool IsAdultLiveMember(MclslFamilyRecord family, long id)
    {
        if (id <= 0 || Member(family, id) is not { Alive: true, Missing: false, IsCultivator: true }
            || !MclslActorRegistry.ResolveKnownOrWorld(id, out Actor actor)
            || !MclslActorAccessor.Alive(actor) || !MclslEligibility.CanCultivate(actor)) return false;
        return actor.getAge() >= 16f;
    }

    internal static bool SetHead(MclslFamilyRecord family, long actorId)
    {
        if (family == null || !IsAdultLiveMember(family, actorId)) return false;
        family.PreferredHeadActorId = actorId;
        EnsureHead(family);
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    private static bool IsActiveCultivator(MclslFamilyRecord family, long id)
        => Member(family, id) is { Alive: true, Missing: false, IsCultivator: true }
            && MclslActorRegistry.ResolveKnownOrWorld(id, out Actor actor)
            && MclslActorAccessor.Alive(actor) && MclslEligibility.CanCultivate(actor);

    internal static bool SetFocus(MclslFamilyRecord family, long actorId)
    {
        if (family == null || !IsActiveCultivator(family, actorId)) return false;
        family.FocusActorId = actorId;
        family.FocusIsManual = true;
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    /// <summary>
    /// 长期扶持对象选择：有效的手动对象原样保留；否则按资质自动选择，
    /// 平资质保留当前有效对象，首次出现同分时按 ActorId 决胜，只有严格更高资质才切换。
    /// </summary>
    internal static long EnsureFocus(MclslFamilyRecord family)
    {
        if (family == null) return 0;
        long current = family.FocusActorId;
        bool currentValid = IsActiveCultivator(family, current);
        if (family.FocusIsManual && currentValid) return current;

        long best = currentValid ? current : 0;
        int bestAptitude = currentValid ? Member(family, current)?.Aptitude ?? int.MinValue : int.MinValue;
        foreach (MclslFamilyMemberRecord member in family.Members)
        {
            if (member == null || member.ActorId == best || !IsActiveCultivator(family, member.ActorId)) continue;
            if (best == 0 || member.Aptitude > bestAptitude)
            {
                best = member.ActorId;
                bestAptitude = member.Aptitude;
            }
            else if (member.Aptitude == bestAptitude && best != current && member.ActorId < best)
            {
                best = member.ActorId;
            }
        }
        bool manual = family.FocusIsManual;
        if (family.FocusActorId != best || manual)
        {
            family.FocusActorId = best;
            family.FocusIsManual = false;
            MclslWorldArchiveStore.MarkDirty();
        }
        return best;
    }

    /// <summary>清除手动指定并重新按资质选择长期扶持对象。</summary>
    internal static void SetAutomaticFocus(MclslFamilyRecord family)
    {
        if (family == null) return;
        family.FocusIsManual = false;
        EnsureFocus(family);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static bool TickAnnual(int year)
    {
        if (year <= 0 || year % 5 != 0) return true;
        MclslWorldRunState run = EnsureIndex();
        if (run == null) return true;
        if (HasWork) return false;
        MclslAnnualBatchState batch = run.AnnualBatch;
        if (ActiveFamilies.Count == 0 && batch.FamilySettlementYear != year) return true;
        if (batch.FamilySettlementYear != year)
        {
            batch.FamilySettlementYear = year;
            batch.FamilyCursor = batch.FamilyMemberCursor = 0;
            batch.FamilySettlementIds.Clear();
            foreach (string id in ActiveFamilies) batch.FamilySettlementIds.Add(id);
            batch.FamilySettlementIds.Sort(StringComparer.Ordinal);
        }
        while (batch.FamilyCursor < batch.FamilySettlementIds.Count && MclslAnnualFrameBudget.TryConsumeOperation())
        {
            ById.TryGetValue(batch.FamilySettlementIds[batch.FamilyCursor], out MclslFamilyRecord family);
            if (family?.Extinct == true)
            {
                TryLiquidateEstate(run, family);
                batch.FamilyCursor++; batch.FamilyMemberCursor = 0; continue;
            }
            if (family == null || family.LastSettledYear == year)
            { batch.FamilyCursor++; batch.FamilyMemberCursor = 0; continue; }
            if (batch.FamilyMemberCursor == 0)
            {
                EnsureHead(family);
                family.CityInfluence.Clear();
                InvalidateCityInfluenceViews();
                family.SettlementDuesStones = 0;
            }
            while (batch.FamilyMemberCursor < family.Members.Count && MclslAnnualFrameBudget.TryConsumeOperation())
            {
                MclslFamilyMemberRecord member = family.Members[batch.FamilyMemberCursor++];
                if (member == null || !member.Alive || !MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor actor)
                    || !MclslActorAccessor.Alive(actor)) continue;
                RefreshMember(family, actor);
                if (!member.IsCultivator || member.Missing || !MclslEligibility.CanCultivate(actor)) continue;
                if (member.CityId > 0)
                {
                    family.CityInfluence.TryGetValue(member.CityId, out int score);
                    family.CityInfluence[member.CityId] = score + 1 + 2 * Math.Max(0, member.RealmIndex)
                        + (member.ActorId == family.HeadActorId ? 3 : 0);
                }
                if (member.LastDuesYear != year)
                {
                    long stones = MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.SpiritStones, 0);
                    if (stones >= 30)
                    {
                        long dues = Math.Min(Math.Min(20, stones / 10), long.MaxValue - TreasuryStones(family));
                        if (dues > 0)
                        {
                            MclslEconomicResult result = MoveStones(family, actor, dues, true, year,
                                "dues/" + family.Id + "/" + member.ActorId, year);
                            if (result == MclslEconomicResult.Applied)
                            {
                                family.SettlementDuesStones = checked(family.SettlementDuesStones + dues);
                                MclslEconomyCommands.NotifyWallet(actor);
                            }
                            else if (result != MclslEconomicResult.AlreadyApplied)
                                throw new InvalidOperationException("族费结算失败：" + result);
                        }
                    }
                    member.LastDuesYear = year;
                }
            }
            if (batch.FamilyMemberCursor < family.Members.Count) break;
            if (family.SettlementDuesStones > 0)
                AddHistory(family, year + "年 · 族费收入灵石" + family.SettlementDuesStones);
            family.SettlementDuesStones = 0;
            family.LastSettledYear = year;
            InvalidateCityInfluenceViews();
            batch.FamilyCursor++;
            batch.FamilyMemberCursor = 0;
        }
        MclslWorldArchiveStore.MarkDirty();
        if (batch.FamilyCursor < batch.FamilySettlementIds.Count) return false;
        batch.FamilyCursor = batch.FamilyMemberCursor = 0;
        return true;
    }

    internal static bool TransferStones(MclslFamilyRecord family, Actor actor, int amount, bool deposit)
    {
        if (family == null || amount <= 0 || FamilyOf(actor) != family || !MclslActorAccessor.Alive(actor)) return false;
        if (MoveStones(family, actor, amount, deposit, Math.Max(0, MclslRuntime.CurrentYear())) != MclslEconomicResult.Applied)
            return false;
        MclslEconomyCommands.NotifyWallet(actor);
        AddHistory(family, MclslRuntime.CurrentYear() + "年 · " + SafeName(actor) + (deposit ? "存入" : "领取") + "灵石" + amount);
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static bool TransferItem(MclslFamilyRecord family, Actor actor, string itemId, bool deposit)
    {
        if (family == null || FamilyOf(actor) != family || !MclslActorAccessor.Alive(actor)
            || MclslItemCatalog.Get(itemId) == null || MclslBagSystem.IsLocked(actor)) return false;
        MclslOwnedItem held;
        if (deposit)
        {
            MclslBagState source = MclslBagSystem.Read(actor);
            held = source.Items.Find(x => x?.ItemId == itemId && x.Count > 0
                && !MclslInventoryDataRules.IsEquipped(source, x));
        }
        else held = FamilyStock(family).FindHeld(itemId);
        if (held == null) return false;
        return TransferOne(family, actor, held, deposit);
    }

    /// <summary>
    /// 指定具体存货实例取用/存入。held 必须是真实来源集合（乾坤袋或家库）中的引用。
    /// 具体 held 手动接口保留；内部由单件转移核心就地拆分，不再整表复制家库。
    /// </summary>
    internal static bool TransferItem(MclslFamilyRecord family, Actor actor, MclslOwnedItem held, bool deposit)
    {
        if (family == null || FamilyOf(actor) != family || !MclslActorAccessor.Alive(actor)
            || held == null || MclslItemCatalog.Get(held.ItemId) == null || MclslBagSystem.IsLocked(actor)) return false;
        return TransferOne(family, actor, held, deposit);
    }

    private static int IndexOfHeld(List<MclslOwnedItem> list, MclslOwnedItem held)
    {
        if (list == null || held == null) return -1;
        for (int i = 0; i < list.Count; i++)
            if (ReferenceEquals(list[i], held)) return i;
        if (!string.IsNullOrEmpty(held.InstanceId))
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null && string.Equals(list[i].InstanceId, held.InstanceId, StringComparison.Ordinal)) return i;
        return -1;
    }

    private static MclslBagState CopyBag(MclslBagState source) => new()
    {
        Version = source.Version,
        Items = source.Items.Select(MclslFamilyTransferPolicy.CopyItem).ToList(),
        EquippedArtifactSlots = new Dictionary<string, string>(source.EquippedArtifactSlots),
        Books = new List<MclslMentorshipBook>(source.Books)
    };

    internal static void OnDeath(Actor actor)
    {
        if (MclslWorldArchiveStore.WriteLocked || actor?.data == null || MclslActorAccessor.Alive(actor)) return;
        MclslFamilyRecord family = FamilyOf(actor);
        long id = MclslActorAccessor.Id(actor);
        MclslFamilyMemberRecord member = Member(family, id);
        if (member == null)
        {
            MclslEconomyCommands.CloseDeadActorWallet(actor);
            MclslTianxuanMarket.ReleaseUnclaimedSellerEscrow(id, MclslRuntime.CurrentYear());
            return;
        }
        if (member.DeathSettled)
        {
            MclslEconomyCommands.CloseDeadActorWallet(actor);
            return;
        }
        bool firstDeath = member.Alive || member.Missing;
        if (firstDeath) InvalidateMemberView(family);
        QueueFamily(family);
        member.Alive = false;
        member.Missing = false;
        if (member.DeathYear < 0) member.DeathYear = Math.Max(0, MclslRuntime.CurrentYear());
        if (family.PreferredHeadActorId == id) family.PreferredHeadActorId = 0;
        if (family.FocusActorId == id) family.FocusActorId = 0;
        if (firstDeath) AddHistory(family, member.DeathYear + "年 · " + member.Name + "陨落");
        EnsureHead(family);
        NotifyChanged();
        long stones = MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.SpiritStones, 0);
        long inherited = Math.Min(stones / 2, long.MaxValue - TreasuryStones(family));
        bool settled = false;
        MclslBagState originalBag = null;
        int originalItemCount = family.Items.Count;
        try
        {
            if (MclslBagSystem.IsLocked(actor)) throw new InvalidOperationException("乾坤袋已锁定");
            originalBag = CopyBag(MclslBagSystem.Read(actor));
            MclslBagState remainingBag = CopyBag(originalBag);
            List<MclslOwnedItem> inheritedItems = remainingBag.Items
                .Where(x => x != null && !MclslInventoryDataRules.IsEquipped(remainingBag, x))
                .Select(MclslFamilyTransferPolicy.CopyItem).ToList();
            remainingBag.Items.RemoveAll(x => x != null && !MclslInventoryDataRules.IsEquipped(remainingBag, x));
            if (!MclslInventoryDataRules.IsValid(remainingBag))
                throw new InvalidOperationException("遗物转移后乾坤袋无效");
            MclslBagSystem.Write(actor, remainingBag);
            family.Items.AddRange(inheritedItems);
            MclslTianxuanMarket.TransferSellerEscrowToFamily(id, family.Items, () =>
            {
                MclslEconomicResult result = MoveStones(family, actor, inherited, true,
                    Math.Max(0, MclslRuntime.CurrentYear()), "estate/" + MclslEconomyCommands.Account(actor), 1);
                if (result is not (MclslEconomicResult.Applied or MclslEconomicResult.AlreadyApplied))
                    throw new InvalidOperationException("遗产资金结算失败：" + result);
            });
            // 死亡遗产 AddRange 与市场托管遗产写入均为批量入口：一次整建该族家库索引。
            RebuildFamilyStock(family);
            member.DeathSettled = true;
            settled = true;
        }
        catch (Exception ex)
        {
            member.DeathSettled = false;
            if (family.Items.Count > originalItemCount)
                family.Items.RemoveRange(originalItemCount, family.Items.Count - originalItemCount);
            // 失败回滚同样以一次整建恢复索引与真实家库一致。
            RebuildFamilyStock(family);
            try
            {
                if (originalBag != null) MclslBagSystem.Write(actor, originalBag);
            }
            catch (Exception rollback) { MclslDiagnostics.Error("family-inheritance-rollback", "家族遗物回滚失败：" + rollback.Message); }
            MclslDiagnostics.Error("family-inheritance", "家族遗物转移失败：" + ex.Message);
            // Preserve the unsettled estate for an explicit repair or save-load event.
            // Retrying a corrupt inventory every frame cannot repair its contents.
        }
        member.Alive = false;
        if (settled) MclslEconomyCommands.CloseDeadActorWallet(actor);
        if (family.PreferredHeadActorId == id) family.PreferredHeadActorId = 0;
        if (family.FocusActorId == id) family.FocusActorId = 0;
        if (settled) AddHistory(family, MclslRuntime.CurrentYear() + "年 · " + member.Name + "遗产入库，灵石" + inherited);
        if ((_populationComplete || MclslWorldBootstrapLane.Completed) && !family.Members.Any(x => x != null && (x.Alive || x.Missing)))
        {
            family.Extinct = true;
            TryLiquidateEstate(EnsureIndex(), family);
        }
        NotifyChanged();
    }

    private static void TryLiquidateEstate(MclslWorldRunState run, MclslFamilyRecord family)
    {
        if (run == null || family == null || !family.Extinct) return;
        bool hasLivingOrMissing = family.Members.Any(x => x != null && (x.Alive || x.Missing));
        bool hasUnsettledDeath = family.Members.Any(x => x != null && !x.Alive && !x.DeathSettled);
        if (!family.EstateLiquidated)
        {
            // 有在世或失踪成员、或有未完成的死亡遗产转移时，既不清算也不删除。
            if (hasLivingOrMissing) return;
            if (hasUnsettledDeath)
            {
                MclslDiagnostics.Error("family-estate-unsettled:" + family.Id, "绝嗣家族尚有未完成的死亡遗产转移，家库已保留。");
                return;
            }
            if (!MclslTianxuanMarket.QueueEstateItems(family.Items, MclslRuntime.CurrentYear(), () =>
            {
                MclslEconomicResult liquidation = MclslEconomyCommands.Commit(Math.Max(0, MclslRuntime.CurrentYear()),
                    new[] { new MclslEconomicOperation(MclslEconomicKind.Destroy, MclslCurrency.SpiritStone,
                        TreasuryStones(family), fromAccount: TreasuryAccount(family)) }, "family-liquidation/" + family.Id, 1);
                return liquidation is MclslEconomicResult.Applied or MclslEconomicResult.AlreadyApplied;
            }))
            {
                MclslDiagnostics.Error("family-estate:" + family.Id, "绝嗣家族遗物未能上架，已保留家库供后续重试。");
                return;
            }
            // The queued items and the family inventory now cross the same commit boundary.
            // Mark this before later lineage/UI work so a retry cannot queue them twice.
            family.Items.Clear();
            ReleaseFamilyStock(family);
            family.EstateLiquidated = true;
            MclslWorldArchiveStore.MarkDirty();
        }
        // 功法传承散藏由功法组按家族功法明细统一写入；旧档 EstateLiquidated 路径同样需要同步。
        SyncEstateTechniques(run, family);
        // 遗产已清算（EstateLiquidated 为 true 的存档路径同样走到这里），
        // 只有满足全部删除条件才移除模组家族记录、族史、索引与 UI 缓存。
        if (!ShouldRemoveExtinctFamily(
                MclslWorldRunRepository.PersistentCleanupEnabled, family.RemoveAfterLiquidation,
                family.Extinct, family.EstateLiquidated, hasLivingOrMissing, hasUnsettledDeath)) return;
        RemoveFamilyRecord(run, family);
    }

    private static bool ShouldRemoveExtinctFamily(
        bool persistentCleanupEnabled,
        bool legacyRemoveAfterLiquidation,
        bool extinct,
        bool estateLiquidated,
        bool hasLivingOrMissingMember,
        bool hasUnsettledDeath)
        => MclslCleanupPolicy.ShouldRemoveExtinctFamily(
            persistentCleanupEnabled, legacyRemoveAfterLiquidation,
            extinct, estateLiquidated, hasLivingOrMissingMember, hasUnsettledDeath);

    /// <summary>
    /// 删除一个已绝嗣且遗产清算完成的模组家族：记录、族史、索引与 UI 缓存。
    /// 同时登记原生家族墓碑，避免正常观察逻辑立刻重建同一家族。幂等。
    /// </summary>
    private static void RemoveFamilyRecord(MclslWorldRunState run, MclslFamilyRecord family)
    {
        if (run == null || family == null) return;
        bool changed = false;
        if (run.Families != null) changed |= run.Families.Remove(family);
        changed |= VisibleFamilies.Remove(family);
        changed |= ById.Remove(family.Id);
        if (family.NativeFamilyId > 0
            && ByNative.TryGetValue(family.NativeFamilyId, out var native) && native == family)
            changed |= ByNative.Remove(family.NativeFamilyId);
        if (family.PaternalRootActorId > 0
            && ByPaternalRoot.TryGetValue(family.PaternalRootActorId, out var paternal) && paternal == family)
            changed |= ByPaternalRoot.Remove(family.PaternalRootActorId);
        // 只有持续清理开启的新世界才登记墓碑，阻止正常观察立刻重建；
        // 缺省标记的既有 v24 存档保持原样的可重建行为。
        if (MclslWorldRunRepository.PersistentCleanupEnabled && family.NativeFamilyId > 0)
            RemovedNativeFamilyIds.Add(family.NativeFamilyId);
        foreach (var member in family.Members)
        {
            if (member == null) continue;
            changed |= MembersById.Remove((family.Id, member.ActorId));
            if (ByActor.TryGetValue(member.ActorId, out var owner) && owner == family)
                changed |= ByActor.Remove(member.ActorId);
        }
        changed |= ActiveFamilies.Remove(family.Id);
        if (family.History.Count > 0) { family.History.Clear(); changed = true; }
        ForgetFamilyViews(family);
        if (family.Members.Count > 0) { family.Members.Clear(); changed = true; }
        if (family.Items.Count > 0) { family.Items.Clear(); changed = true; }
        ReleaseFamilyStock(family);
        if (changed) NotifyChanged();
    }

    // 家族纪事只保留最近 15 条，更早的直接丢弃。
    internal const int HistoryLimit = 15;

    private static void AddHistory(MclslFamilyRecord family, string text)
    {
        family.History.Add(text);
        if (family.History.Count > HistoryLimit) family.History.RemoveRange(0, family.History.Count - HistoryLimit);
    }
}
