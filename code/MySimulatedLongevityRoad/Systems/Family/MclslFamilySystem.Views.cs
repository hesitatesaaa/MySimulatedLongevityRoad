using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal enum MclslFamilyMemberFilter { All = 0, Living = 1, Gone = 2 }

internal readonly struct MclslFamilyMemberSummary
{
    internal readonly int Living, Cultivators, Mortals, Gone, HighestRealmIndex;
    internal MclslFamilyMemberSummary(int living, int cultivators, int mortals, int gone, int highestRealmIndex)
    { Living = living; Cultivators = cultivators; Mortals = mortals; Gone = gone; HighestRealmIndex = highestRealmIndex; }
}

/// <summary>
/// 家族资料与界面读取的运行时缓存（只存在于内存，不写入 Data 存档字段）。
/// 功法条目以 TechniqueDetails 持久资料为唯一来源；成员、城市影响、城市名按族/城缓存，
/// 由实际变化入口定向失效，绘制时不再扫描成员表、世界档案或全部家族。
/// </summary>
internal static partial class MclslFamilySystem
{
    private sealed class FamilyTechniqueView
    {
        internal IReadOnlyList<KeyValuePair<string, MclslFamilyTechniqueRecord>> Entries =
            Array.Empty<KeyValuePair<string, MclslFamilyTechniqueRecord>>();
    }

    private sealed class FamilyMemberView
    {
        internal readonly List<MclslFamilyMemberRecord> All = new();
        internal readonly List<MclslFamilyMemberRecord> Living = new();
        internal readonly List<MclslFamilyMemberRecord> Gone = new();
        internal readonly Dictionary<long, MclslFamilyMemberRecord> ByActor = new();
        internal readonly Dictionary<long, List<MclslFamilyMemberRecord>> Children = new();
        internal MclslFamilyMemberSummary Summary = new(0, 0, 0, 0, -1);
        internal bool ChildrenBuilt;
    }

    private static readonly Dictionary<string, FamilyTechniqueView> TechniqueViews = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, FamilyMemberView> MemberViews = new(StringComparer.Ordinal);
    // 法脉 Id（原始/规范化/SourceTechniqueId）→ 依赖该法脉补全资料的家族。
    private static readonly Dictionary<string, HashSet<string>> TechniqueDependents = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, HashSet<string>> FamilyTechniqueKeys = new(StringComparer.Ordinal);
    private static readonly HashSet<string> PendingTechniqueDetailFamilies = new(StringComparer.Ordinal);
    private static readonly Dictionary<long, List<KeyValuePair<MclslFamilyRecord, int>>> CityLeaders = new();
    private static bool _cityLeadersDirty = true;
    // 仅在 TechniqueDetails 缺少该条目时使用的只读占位，避免绘制时写入持久资料。
    private static readonly MclslFamilyTechniqueRecord MissingTechniqueRecord = new();

    internal static void ClearFamilyViews()
    {
        TechniqueViews.Clear();
        MemberViews.Clear();
        TechniqueDependents.Clear();
        FamilyTechniqueKeys.Clear();
        PendingTechniqueDetailFamilies.Clear();
        CityLeaders.Clear();
        _cityLeadersDirty = true;
    }

    internal static void ForgetFamilyViews(MclslFamilyRecord family)
    {
        if (family == null) return;
        TechniqueViews.Remove(family.Id);
        MemberViews.Remove(family.Id);
        PendingTechniqueDetailFamilies.Remove(family.Id);
        if (FamilyTechniqueKeys.TryGetValue(family.Id, out HashSet<string> keys))
        {
            foreach (string key in keys)
            {
                if (!TechniqueDependents.TryGetValue(key, out HashSet<string> families)) continue;
                families.Remove(family.Id);
                if (families.Count == 0) TechniqueDependents.Remove(key);
            }
            FamilyTechniqueKeys.Remove(family.Id);
        }
        InvalidateCityInfluenceViews();
    }

    internal static void InvalidateMemberView(MclslFamilyRecord family)
    {
        if (family != null) MemberViews.Remove(family.Id);
    }

    internal static void InvalidateCityInfluenceViews()
    {
        _cityLeadersDirty = true;
        CityLeaders.Clear();
    }

    // ── 功法资料视图 ────────────────────────────────────────────────

    /// <summary>家族功法只读视图：按原始完整 Id 去重，顺序固定，全部来自 TechniqueDetails。</summary>
    internal static IReadOnlyList<KeyValuePair<string, MclslFamilyTechniqueRecord>> TechniqueEntries(MclslFamilyRecord family)
    {
        if (family == null) return Array.Empty<KeyValuePair<string, MclslFamilyTechniqueRecord>>();
        if (TechniqueViews.TryGetValue(family.Id, out FamilyTechniqueView cached)) return cached.Entries;
        long sample = MclslPerformanceProbe.Begin();
        try
        {
            FamilyTechniqueView view = new() { Entries = BuildTechniqueEntries(family) };
            TechniqueViews[family.Id] = view;
            RefreshTechniqueDependencies(family);
            return view.Entries;
        }
        finally { MclslPerformanceProbe.End("family.tech.view", sample); }
    }

    private static IReadOnlyList<KeyValuePair<string, MclslFamilyTechniqueRecord>> BuildTechniqueEntries(MclslFamilyRecord family)
    {
        Dictionary<string, MclslFamilyTechniqueRecord> values = new(StringComparer.Ordinal);
        if (family.TechniqueDetails != null)
        {
            foreach (KeyValuePair<string, MclslFamilyTechniqueRecord> pair in family.TechniqueDetails)
            {
                string id = TrimTechniqueId(pair.Key);
                if (id.Length == 0 || pair.Value == null) continue;
                values[id] = pair.Value;
            }
        }

        List<string> order = new();
        HashSet<string> seen = new(StringComparer.Ordinal);
        if (family.TechniqueIds != null)
        {
            foreach (string raw in family.TechniqueIds)
            {
                string id = TrimTechniqueId(raw);
                if (id.Length == 0 || !seen.Add(id)) continue;
                order.Add(id);
            }
        }
        foreach (string id in values.Keys)
            if (seen.Add(id)) order.Add(id);
        if (order.Count == 0) return Array.Empty<KeyValuePair<string, MclslFamilyTechniqueRecord>>();

        List<KeyValuePair<string, MclslFamilyTechniqueRecord>> result = new(order.Count);
        foreach (string id in order)
        {
            values.TryGetValue(id, out MclslFamilyTechniqueRecord record);
            result.Add(new KeyValuePair<string, MclslFamilyTechniqueRecord>(id, record ?? MissingTechniqueRecord));
        }
        return result;
    }

    /// <summary>功法条目实际变化后失效该族功法视图（不提升全局 Revision）。</summary>
    internal static void InvalidateFamilyTechniqueView(MclslFamilyRecord family)
    {
        if (family == null) return;
        TechniqueViews.Remove(family.Id);
        RefreshTechniqueDependencies(family);
    }

    /// <summary>
    /// 队列内资料补全：ChangedFamilies 循环对每个出队家族调用；若该族没有待补全标记则立即返回。
    /// 只有实际补齐字段才刷新依赖、存盘并失效本族功法视图；未变化不重建。
    /// </summary>
    internal static void RefreshQueuedTechniqueDetails(MclslFamilyRecord family)
    {
        if (family == null) return;
        if (!PendingTechniqueDetailFamilies.Remove(family.Id)) return;
        if (!FillTechniqueDetailsFromLineage(family)) return;
        TechniqueViews.Remove(family.Id);
        RefreshTechniqueDependencies(family);
        MclslWorldArchiveStore.MarkDirty();
    }

    /// <summary>法脉记录新增/资料字段更新/删除后的定向通知：只刷新实际依赖该法脉的家族。</summary>
    internal static void NotifyTechniqueLineageChanged(MclslTechniqueLineageRecord record)
    {
        if (record == null) return;
        NotifyTechniqueLineageChanged(record.Id);
        if (!string.IsNullOrWhiteSpace(record.SourceTechniqueId))
            NotifyTechniqueLineageChanged(record.SourceTechniqueId);
    }

    internal static void NotifyTechniqueLineageChanged(string lineageId)
    {
        if (string.IsNullOrWhiteSpace(lineageId)) return;
        if (!TechniqueDependents.TryGetValue(lineageId, out HashSet<string> familyIds) || familyIds.Count == 0) return;
        string[] snapshot = new string[familyIds.Count];
        familyIds.CopyTo(snapshot);
        for (int i = 0; i < snapshot.Length; i++)
        {
            if (!ById.TryGetValue(snapshot[i], out MclslFamilyRecord family)) continue;
            // 登记待补全标记并以“非状态变更”入队：共用 ChangedFamilies 去重，但主循环据 StateChangedFamilyIds
            // 跳过家主/扶持重算；此处不失效视图，仅实际补齐字段时才失效。
            PendingTechniqueDetailFamilies.Add(family.Id);
            QueueFamily(family, stateChanged: false);
        }
    }

    /// <summary>按当前 TechniqueDetails 重建该族的法脉依赖登记；已补齐的条目不再登记。</summary>
    private static void RefreshTechniqueDependencies(MclslFamilyRecord family)
    {
        HashSet<string> next = new(StringComparer.Ordinal);
        if (family.TechniqueDetails != null)
        {
            foreach (KeyValuePair<string, MclslFamilyTechniqueRecord> pair in family.TechniqueDetails)
            {
                string raw = TrimTechniqueId(pair.Key);
                MclslFamilyTechniqueRecord detail = pair.Value;
                if (raw.Length == 0 || detail == null || !TechniqueNeedsCompletion(detail)) continue;
                next.Add(raw);
                string normalized = MclslCultivationCatalog.NormalizeTechniqueId(raw);
                if (normalized.Length > 0 && !string.Equals(normalized, raw, StringComparison.Ordinal)) next.Add(normalized);
            }
        }

        if (FamilyTechniqueKeys.TryGetValue(family.Id, out HashSet<string> previous))
        {
            foreach (string key in previous)
            {
                if (next.Contains(key)) continue;
                if (!TechniqueDependents.TryGetValue(key, out HashSet<string> families)) continue;
                families.Remove(family.Id);
                if (families.Count == 0) TechniqueDependents.Remove(key);
            }
        }

        foreach (string key in next)
        {
            if (!TechniqueDependents.TryGetValue(key, out HashSet<string> families))
                TechniqueDependents[key] = families = new HashSet<string>(StringComparer.Ordinal);
            families.Add(family.Id);
        }
        if (next.Count == 0) FamilyTechniqueKeys.Remove(family.Id);
        else FamilyTechniqueKeys[family.Id] = next;
    }

    private static bool FillTechniqueDetailsFromLineage(MclslFamilyRecord family)
    {
        if (family.TechniqueDetails == null || family.TechniqueDetails.Count == 0) return false;
        bool changed = false;
        foreach (KeyValuePair<string, MclslFamilyTechniqueRecord> pair in family.TechniqueDetails)
        {
            string raw = TrimTechniqueId(pair.Key);
            MclslFamilyTechniqueRecord detail = pair.Value;
            if (raw.Length == 0 || detail == null || !TechniqueNeedsCompletion(detail)) continue;
            string normalized = MclslCultivationCatalog.NormalizeTechniqueId(raw);
            bool sameDefinition = raw.EndsWith(normalized, StringComparison.Ordinal);
            MclslTechniqueLineageRecord exact = FindExactLineage(raw, sameDefinition ? normalized : raw);
            if (exact == null) continue;
            changed |= FillTechniqueDetailBlanks(detail, exact);
        }
        return changed;
    }

    private static bool TechniqueNeedsCompletion(MclslFamilyTechniqueRecord detail)
        => string.IsNullOrWhiteSpace(detail.Name) || string.IsNullOrWhiteSpace(detail.MaxRealm)
            || string.IsNullOrWhiteSpace(detail.SystemId) || string.IsNullOrWhiteSpace(detail.LawTags);

    private static bool FillTechniqueDetailBlanks(MclslFamilyTechniqueRecord detail, MclslTechniqueLineageRecord lineage)
    {
        bool changed = false;
        if (string.IsNullOrWhiteSpace(detail.Name) && !string.IsNullOrWhiteSpace(lineage.Name))
        { detail.Name = lineage.Name; changed = true; }
        if (string.IsNullOrWhiteSpace(detail.MaxRealm) && !string.IsNullOrWhiteSpace(lineage.MaxRealm))
        { detail.MaxRealm = lineage.MaxRealm; changed = true; }
        if (string.IsNullOrWhiteSpace(detail.SystemId) && !string.IsNullOrWhiteSpace(lineage.SystemId))
        { detail.SystemId = lineage.SystemId; changed = true; }
        if (string.IsNullOrWhiteSpace(detail.LawTags) && !string.IsNullOrWhiteSpace(lineage.LawTags))
        { detail.LawTags = lineage.LawTags; changed = true; }
        return changed;
    }

    // ── 成员视图 ────────────────────────────────────────────────────

    internal static IReadOnlyList<MclslFamilyMemberRecord> MemberView(MclslFamilyRecord family, MclslFamilyMemberFilter filter)
    {
        FamilyMemberView view = MemberViewOf(family, false);
        if (view == null) return Array.Empty<MclslFamilyMemberRecord>();
        return filter == MclslFamilyMemberFilter.Living ? view.Living
            : filter == MclslFamilyMemberFilter.Gone ? view.Gone : view.All;
    }

    internal static MclslFamilyMemberSummary MemberSummary(MclslFamilyRecord family)
    {
        FamilyMemberView view = MemberViewOf(family, false);
        return view != null ? view.Summary : new MclslFamilyMemberSummary(0, 0, 0, 0, -1);
    }

    internal static MclslFamilyMemberRecord FindFamilyMember(MclslFamilyRecord family, long actorId)
    {
        if (actorId <= 0) return null;
        FamilyMemberView view = MemberViewOf(family, false);
        return view != null && view.ByActor.TryGetValue(actorId, out MclslFamilyMemberRecord member) ? member : null;
    }

    internal static IReadOnlyList<MclslFamilyMemberRecord> FamilyChildren(MclslFamilyRecord family, MclslFamilyMemberRecord member)
    {
        if (member == null) return Array.Empty<MclslFamilyMemberRecord>();
        FamilyMemberView view = MemberViewOf(family, true);
        return view != null && view.Children.TryGetValue(member.ActorId, out List<MclslFamilyMemberRecord> children)
            ? children : Array.Empty<MclslFamilyMemberRecord>();
    }

    private static FamilyMemberView MemberViewOf(MclslFamilyRecord family, bool withChildren)
    {
        if (family == null) return null;
        if (!MemberViews.TryGetValue(family.Id, out FamilyMemberView view))
        {
            long sample = MclslPerformanceProbe.Begin();
            try
            {
                view = BuildMemberView(family);
                MemberViews[family.Id] = view;
            }
            finally { MclslPerformanceProbe.End("family.members.view", sample); }
        }
        if (withChildren && !view.ChildrenBuilt)
        {
            long sample = MclslPerformanceProbe.Begin();
            try
            {
                BuildChildren(view);
                view.ChildrenBuilt = true;
            }
            finally { MclslPerformanceProbe.End("family.members.view", sample); }
        }
        return view;
    }

    private static FamilyMemberView BuildMemberView(MclslFamilyRecord family)
    {
        FamilyMemberView view = new();
        if (family.Members != null)
        {
            foreach (MclslFamilyMemberRecord member in family.Members)
            {
                if (member == null) continue;
                // 凡人不再作为成员记录参与；名录/子女只保留修士。旧档中残留的
                // 凡人记录在此被忽略（不展示、不计入修士统计、不参与家主/结算）。
                if (!member.IsCultivator) continue;
                view.All.Add(member);
                if (member.ActorId > 0) view.ByActor[member.ActorId] = member;
            }
        }
        SortMembers(view.All);
        int living = 0, cultivators = 0, gone = 0, highest = -1;
        foreach (MclslFamilyMemberRecord member in view.All)
        {
            if (member.Alive && !member.Missing)
            {
                living++;
                view.Living.Add(member);
                cultivators++;
                if (member.RealmIndex > highest && member.RealmIndex < MclslRealmIds.Ordered.Length)
                    highest = member.RealmIndex;
            }
            else
            {
                gone++;
                view.Gone.Add(member);
            }
        }
        // 凡人不逐人记录/判定，只统计在世数量（派生视图，不写入成员表）。
        int mortals = CountLivingMortals(family, view);
        family.MortalCount = mortals;
        view.Summary = new MclslFamilyMemberSummary(living + mortals, cultivators, mortals, gone, highest);
        return view;
    }

    /// <summary>
    /// 统计该族在世凡人（非修士）数量：从各修士成员沿子女关系下溯整个父系树，
    /// 凡存活、非修士、且不属于其它家族的节点计为一名凡人。不建立任何凡人成员
    /// 记录、不做逐人判定/结算；只返回一个数量。遍历有深度上限，防止病态族谱。
    /// </summary>
    private static int CountLivingMortals(MclslFamilyRecord family, FamilyMemberView view)
    {
        if (view == null || view.All.Count == 0) return 0;
        HashSet<long> visited = new();
        var stack = new Stack<(long Id, int Depth)>();
        foreach (MclslFamilyMemberRecord member in view.All)
            if (member.ActorId > 0 && visited.Add(member.ActorId)) stack.Push((member.ActorId, 0));
        int mortals = 0;
        const int MaxDepth = 24;
        while (stack.Count > 0)
        {
            (long current, int depth) = stack.Pop();
            if (depth >= MaxDepth) continue;
            foreach (long childId in MclslActorRelations.Descendants(current))
            {
                if (childId <= 0 || !visited.Add(childId)) continue;
                // 已属其它家族者不计入本族。
                if (ByActor.TryGetValue(childId, out MclslFamilyRecord other) && other != family) continue;
                if (view.ByActor.ContainsKey(childId)) { stack.Push((childId, depth + 1)); continue; }
                if (!MclslActorRegistry.ResolveKnownOrWorld(childId, out Actor child) || child?.data == null) continue;
                if (!MclslActorAccessor.Alive(child)) continue;
                if (MclslActorAccessor.IsCultivator(child)) { stack.Push((childId, depth + 1)); continue; }
                mortals++;
                stack.Push((childId, depth + 1));
            }
        }
        return mortals;
    }

    // 与原 OrderBy(Generation).ThenBy(Name, StringComparer.Ordinal) 完全一致：稳定排序 + 序号比较。
    private static void SortMembers(List<MclslFamilyMemberRecord> members)
    {
        if (members == null || members.Count < 2) return;
        List<MclslFamilyMemberRecord> sorted = members
            .OrderBy(x => x.Generation)
            .ThenBy(x => x.Name, StringComparer.Ordinal)
            .ToList();
        members.Clear();
        members.AddRange(sorted);
    }

    // 子女映射只建一次：先读 ActorRelations 的直接子女，再用族谱父母补齐已故/失联者。
    private static void BuildChildren(FamilyMemberView view)
    {
        Dictionary<long, HashSet<long>> seen = new();
        foreach (MclslFamilyMemberRecord member in view.All)
        {
            if (member.ActorId <= 0) continue;
            foreach (long childId in MclslActorRelations.Descendants(member.ActorId))
                if (view.ByActor.TryGetValue(childId, out MclslFamilyMemberRecord child))
                    AddChild(view, seen, member.ActorId, child);
        }
        foreach (MclslFamilyMemberRecord member in view.All)
        {
            AddChild(view, seen, member.ParentId1, member);
            AddChild(view, seen, member.ParentId2, member);
        }
        foreach (KeyValuePair<long, List<MclslFamilyMemberRecord>> pair in view.Children)
            SortMembers(pair.Value);
    }

    private static void AddChild(FamilyMemberView view, Dictionary<long, HashSet<long>> seen,
        long parentId, MclslFamilyMemberRecord child)
    {
        if (parentId <= 0 || child == null || child.ActorId <= 0 || parentId == child.ActorId) return;
        if (!seen.TryGetValue(parentId, out HashSet<long> ids)) seen[parentId] = ids = new HashSet<long>();
        if (!ids.Add(child.ActorId)) return;
        if (!view.Children.TryGetValue(parentId, out List<MclslFamilyMemberRecord> children))
            view.Children[parentId] = children = new List<MclslFamilyMemberRecord>();
        children.Add(child);
    }

    // ── 城市影响视图 ────────────────────────────────────────────────

    /// <summary>某城影响前 3 家族；五年结算、影响清空、删族后失效，绘制时不再遍历全部家族。</summary>
    internal static IReadOnlyList<KeyValuePair<MclslFamilyRecord, int>> CityInfluenceLeaders(long cityId)
    {
        EnsureCityLeaders();
        return CityLeaders.TryGetValue(cityId, out List<KeyValuePair<MclslFamilyRecord, int>> leaders)
            ? leaders : Array.Empty<KeyValuePair<MclslFamilyRecord, int>>();
    }

    private static void EnsureCityLeaders()
    {
        if (!_cityLeadersDirty) return;
        long sample = MclslPerformanceProbe.Begin();
        try
        {
            CityLeaders.Clear();
            IReadOnlyList<MclslFamilyRecord> families = Families;
            for (int i = 0; i < families.Count; i++)
            {
                MclslFamilyRecord family = families[i];
                if (family?.CityInfluence == null) continue;
                foreach (KeyValuePair<long, int> pair in family.CityInfluence)
                {
                    if (pair.Value <= 0) continue;
                    if (!CityLeaders.TryGetValue(pair.Key, out List<KeyValuePair<MclslFamilyRecord, int>> leaders))
                        CityLeaders[pair.Key] = leaders = new List<KeyValuePair<MclslFamilyRecord, int>>();
                    leaders.Add(new KeyValuePair<MclslFamilyRecord, int>(family, pair.Value));
                }
            }
            foreach (List<KeyValuePair<MclslFamilyRecord, int>> leaders in CityLeaders.Values)
            {
                leaders.Sort(CompareCityLeaders);
                if (leaders.Count > 3) leaders.RemoveRange(3, leaders.Count - 3);
            }
            _cityLeadersDirty = false;
        }
        finally { MclslPerformanceProbe.End("family.cities.view", sample); }
    }

    private static int CompareCityLeaders(KeyValuePair<MclslFamilyRecord, int> left, KeyValuePair<MclslFamilyRecord, int> right)
    {
        int compare = right.Value.CompareTo(left.Value);
        return compare != 0 ? compare : string.Compare(left.Key?.Id, right.Key?.Id, StringComparison.Ordinal);
    }

    // 城名不持久缓存：当前没有城市改名/迁移的核心里通知入口，持久缓存会有陈旧风险。
    // 每次按需从世界城市/本族成员当前城市解析（原 UI 行为），城名变化实时可见。
    internal static string FamilyCityName(MclslFamilyRecord family, long cityId)
    {
        if (cityId <= 0) return "未知城池";
        if (World.world?.cities != null)
        {
            foreach (City city in World.world.cities)
            {
                if (city?.data?.id != cityId) continue;
                return string.IsNullOrWhiteSpace(city.data.name) ? "未名城镇" : city.data.name;
            }
        }
        if (family?.Members != null)
        {
            foreach (MclslFamilyMemberRecord member in family.Members)
            {
                if (member == null || member.CityId != cityId) continue;
                if (!MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor actor)
                    || actor.city?.data?.id != cityId) continue;
                return string.IsNullOrWhiteSpace(actor.city.data.name) ? "未名城镇" : actor.city.data.name;
            }
        }
        return "城池 " + cityId;
    }
}
