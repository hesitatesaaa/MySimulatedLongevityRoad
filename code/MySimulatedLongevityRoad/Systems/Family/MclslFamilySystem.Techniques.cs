using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static partial class MclslFamilySystem
{
    // 家族功法键保留原始完整Id（仅去首尾空白）；规范化只用于内部分类与静态/异法解析。
    private static void RecordTechnique(MclslFamilyRecord family, Actor actor)
    {
        if (family == null || actor?.data == null) return;
        string id = TrimTechniqueId(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty));
        if (id.Length == 0) return;
        // 只在该族功法资料字段实际变化时失效本族功法视图；不改全局 Revision。
        if (!StoreTechnique(family, id, BuildTechniqueRecord(id, actor), overwrite: true)) return;
        MclslWorldArchiveStore.MarkDirty();
        InvalidateFamilyTechniqueView(family);
    }

    /// <summary>
    /// 初始化与旧档补全：用现有成员刷新实际资料，并对仅存Id的旧档按静态/异法定义与
    /// 可证明的完整来源补齐名称、上限、体系、法则；无法恢复的保持空，不伪造。
    /// </summary>
    private static bool InitializeFamilyTechniques(MclslFamilyRecord family)
    {
        if (family == null) return false;
        family.TechniqueIds ??= new List<string>();
        family.TechniqueDetails ??= new Dictionary<string, MclslFamilyTechniqueRecord>(StringComparer.Ordinal);
        bool dirty = false;

        string[] origin = family.TechniqueIds.ToArray();
        family.TechniqueIds.Clear();
        foreach (string raw in origin)
        {
            string id = TrimTechniqueId(raw);
            if (id.Length == 0) { dirty = true; continue; }
            if (!string.Equals(id, raw, StringComparison.Ordinal)) dirty = true;
            if (!family.TechniqueIds.Contains(id)) family.TechniqueIds.Add(id);
        }
        if (family.TechniqueDetails.Count > 0)
        {
            foreach (KeyValuePair<string, MclslFamilyTechniqueRecord> pair in family.TechniqueDetails.ToArray())
            {
                string id = TrimTechniqueId(pair.Key);
                if (id.Length == 0) { family.TechniqueDetails.Remove(pair.Key); dirty = true; continue; }
                if (string.Equals(id, pair.Key, StringComparison.Ordinal)) continue;
                family.TechniqueDetails.Remove(pair.Key);
                if (!family.TechniqueDetails.ContainsKey(id)) family.TechniqueDetails[id] = pair.Value;
                dirty = true;
            }
        }
        foreach (string id in family.TechniqueDetails.Keys)
            if (!family.TechniqueIds.Contains(id)) { family.TechniqueIds.Add(id); dirty = true; }

        foreach (MclslFamilyMemberRecord member in family.Members)
        {
            if (member == null || member.ActorId <= 0) continue;
            if (!MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor actor) || actor?.data == null) continue;
            string id = TrimTechniqueId(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty));
            if (id.Length == 0) continue;
            dirty |= StoreTechnique(family, id, BuildTechniqueRecord(id, actor), overwrite: true);
        }

        foreach (string id in family.TechniqueIds.ToArray())
        {
            MclslFamilyTechniqueRecord fresh = BuildTechniqueRecord(id, null);
            if (string.IsNullOrWhiteSpace(fresh.Name) && string.IsNullOrWhiteSpace(fresh.MaxRealm)
                && string.IsNullOrWhiteSpace(fresh.SystemId) && string.IsNullOrWhiteSpace(fresh.LawTags)) continue;
            dirty |= StoreTechnique(family, id, fresh, overwrite: false);
        }
        // 旧档补全只做一次；此后显示只读 TechniqueDetails。登记仍缺字段条目的法脉依赖。
        InvalidateFamilyTechniqueView(family);
        return dirty;
    }

    /// <summary>
    /// 绝嗣遗产清算前，把家族功法按母法聚合补入全球法脉档案。BaseTechniqueId 只用于此处聚合；
    /// 新建记录用母法定义的名字/上限，绝不把某个 derived 的随机资料冠给母法。
    /// 不覆盖仍有在世传人的有效法脉，也不重建/清空全球档案。
    /// </summary>
    private static void SyncEstateTechniques(MclslWorldRunState run, MclslFamilyRecord family)
    {
        if (run == null || family == null) return;
        IReadOnlyList<KeyValuePair<string, MclslFamilyTechniqueRecord>> entries = TechniqueEntries(family);
        if (entries.Count == 0) return;
        run.TechniqueLineages ??= new List<MclslTechniqueLineageRecord>();

        Dictionary<string, MclslFamilyTechniqueRecord> aggregates = new(StringComparer.Ordinal);
        foreach (KeyValuePair<string, MclslFamilyTechniqueRecord> entry in entries)
        {
            MclslFamilyTechniqueRecord detail = entry.Value;
            if (detail == null) continue;
            string baseId = MclslCultivationCatalog.BaseTechniqueId(entry.Key);
            if (string.IsNullOrWhiteSpace(baseId)) continue;
            bool isBase = string.Equals(entry.Key, baseId, StringComparison.Ordinal);
            MclslTechniqueDefinition baseDefinition = StaticDefinition(baseId);
            if (baseDefinition == null && !isBase) continue;
            MclslFamilyTechniqueRecord aggregate = new()
            {
                Name = isBase && !string.IsNullOrWhiteSpace(detail.Name) ? detail.Name : baseDefinition?.Name ?? string.Empty,
                MaxRealm = isBase && !string.IsNullOrWhiteSpace(detail.MaxRealm) ? detail.MaxRealm : baseDefinition?.MaxRealm ?? string.Empty,
                LawTags = baseDefinition != null && !(isBase && !string.IsNullOrWhiteSpace(detail.LawTags))
                    ? JoinTags(baseDefinition.LawPool) : detail.LawTags,
                SystemId = detail.SystemId
            };
            if (!aggregates.TryGetValue(baseId, out MclslFamilyTechniqueRecord held)) aggregates[baseId] = aggregate;
            else if (isBase) { held.Name = aggregate.Name; if (!string.IsNullOrWhiteSpace(aggregate.SystemId)) held.SystemId = aggregate.SystemId; }
        }

        int year = Math.Max(0, MclslRuntime.CurrentYear());
        bool changed = false;
        List<MclslTechniqueLineageRecord> touched = new();
        foreach (KeyValuePair<string, MclslFamilyTechniqueRecord> pair in aggregates)
        {
            MclslTechniqueLineageRecord record = FindLineageRecord(run, pair.Key);
            if (record == null)
            {
                if (string.IsNullOrWhiteSpace(pair.Value.Name) && string.IsNullOrWhiteSpace(pair.Value.MaxRealm)) continue;
                MclslTechniqueLineageRecord added = new()
                {
                    Id = pair.Key,
                    Name = pair.Value.Name,
                    SystemId = pair.Value.SystemId,
                    LawTags = pair.Value.LawTags,
                    MaxRealm = pair.Value.MaxRealm,
                    Completeness = 100,
                    FirstSeenYear = year,
                    LastSeenYear = year,
                    PeakRealm = MclslRealmIds.Mortal,
                    State = "散藏",
                    SourceTechniqueId = pair.Key,
                    Summary = BuildEstateSummary(pair.Value)
                };
                run.TechniqueLineages.Add(added);
                touched.Add(added);
                changed = true;
                continue;
            }
            if (record.CurrentPractitioners > 0) continue;
            if (!FillLineageBlanks(record, pair.Value)) continue;
            touched.Add(record);
            changed = true;
        }
        if (changed) MclslWorldArchiveStore.MarkDirty();
        // 绝嗣遗产补录法脉同样走定向通知：只刷新实际依赖这些法脉的家族，不扫世界档案。
        for (int i = 0; i < touched.Count; i++) NotifyTechniqueLineageChanged(touched[i]);
    }

    private static MclslFamilyTechniqueRecord BuildTechniqueRecord(string rawId, Actor actor)
    {
        MclslFamilyTechniqueRecord record = new();
        if (actor?.data != null)
        {
            record.Name = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty);
            record.SystemId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        }

        string normalized = MclslCultivationCatalog.NormalizeTechniqueId(rawId);
        // 前缀别名可按同一本解析；历史 branch/line 后缀不能借母法资料冒充自身。
        bool sameDefinition = rawId.EndsWith(normalized, StringComparison.Ordinal);
        MclslTechniqueDefinition definition = null;
        if (sameDefinition && !TryDerivedDefinition(normalized, out definition)) definition = StaticDefinition(normalized);
        if (definition != null)
        {
            // 静态目录与现有异法的上限、法则以定义为唯一权威，不沿用带个人下限的缓存。
            if (string.IsNullOrWhiteSpace(record.Name)) record.Name = definition.Name;
            record.MaxRealm = definition.MaxRealm;
            record.LawTags = JoinTags(definition.LawPool);
        }
        else if (actor?.data != null)
        {
            record.MaxRealm = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueMaxRealm, string.Empty);
        }

        if (string.IsNullOrWhiteSpace(record.Name) || string.IsNullOrWhiteSpace(record.MaxRealm)
            || string.IsNullOrWhiteSpace(record.LawTags) || string.IsNullOrWhiteSpace(record.SystemId))
        {
            // 只接受能精确证明完整来源的法脉记录，绝不用母法记录冒充 legacy/branch/derived 的真实资料。
            MclslTechniqueLineageRecord exact = FindExactLineage(rawId, sameDefinition ? normalized : rawId);
            if (exact != null)
            {
                if (string.IsNullOrWhiteSpace(record.Name)) record.Name = exact.Name;
                if (definition == null && string.IsNullOrWhiteSpace(record.MaxRealm)) record.MaxRealm = exact.MaxRealm;
                if (string.IsNullOrWhiteSpace(record.LawTags)) record.LawTags = exact.LawTags;
                if (string.IsNullOrWhiteSpace(record.SystemId)) record.SystemId = exact.SystemId;
            }
        }
        return record;
    }

    private static bool StoreTechnique(MclslFamilyRecord family, string id, MclslFamilyTechniqueRecord fresh, bool overwrite)
    {
        if (family == null || string.IsNullOrWhiteSpace(id) || fresh == null) return false;
        family.TechniqueIds ??= new List<string>();
        family.TechniqueDetails ??= new Dictionary<string, MclslFamilyTechniqueRecord>(StringComparer.Ordinal);
        bool changed = false;
        if (!family.TechniqueIds.Contains(id)) { family.TechniqueIds.Add(id); changed = true; }
        if (!family.TechniqueDetails.TryGetValue(id, out MclslFamilyTechniqueRecord existing) || existing == null)
        {
            family.TechniqueDetails[id] = fresh;
            return true;
        }
        string value;
        if (MergeValue(existing.Name, fresh.Name, overwrite, out value)) { existing.Name = value; changed = true; }
        if (MergeValue(existing.MaxRealm, fresh.MaxRealm, overwrite, out value)) { existing.MaxRealm = value; changed = true; }
        if (MergeValue(existing.SystemId, fresh.SystemId, overwrite, out value)) { existing.SystemId = value; changed = true; }
        if (MergeValue(existing.LawTags, fresh.LawTags, overwrite, out value)) { existing.LawTags = value; changed = true; }
        return changed;
    }

    private static bool MergeValue(string current, string incoming, bool overwrite, out string result)
    {
        result = current;
        if (string.IsNullOrWhiteSpace(incoming) || string.Equals(current, incoming, StringComparison.Ordinal)) return false;
        if (!overwrite && !string.IsNullOrWhiteSpace(current)) return false;
        result = incoming;
        return true;
    }

    private static bool FillLineageBlanks(MclslTechniqueLineageRecord record, MclslFamilyTechniqueRecord detail)
    {
        if (record == null || detail == null) return false;
        bool changed = false;
        if (string.IsNullOrWhiteSpace(record.Name) && !string.IsNullOrWhiteSpace(detail.Name)) { record.Name = detail.Name; changed = true; }
        if (string.IsNullOrWhiteSpace(record.SystemId) && !string.IsNullOrWhiteSpace(detail.SystemId)) { record.SystemId = detail.SystemId; changed = true; }
        if (string.IsNullOrWhiteSpace(record.LawTags) && !string.IsNullOrWhiteSpace(detail.LawTags)) { record.LawTags = detail.LawTags; changed = true; }
        if (string.IsNullOrWhiteSpace(record.MaxRealm) && !string.IsNullOrWhiteSpace(detail.MaxRealm)) { record.MaxRealm = detail.MaxRealm; changed = true; }
        if (string.IsNullOrWhiteSpace(record.Summary)) { record.Summary = BuildEstateSummary(detail); changed = true; }
        return changed;
    }

    private static MclslTechniqueLineageRecord FindExactLineage(string rawId, string normalized)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.TechniqueLineages == null) return null;
        for (int i = 0; i < run.TechniqueLineages.Count; i++)
        {
            MclslTechniqueLineageRecord record = run.TechniqueLineages[i];
            if (record == null) continue;
            if (string.Equals(record.Id, rawId, StringComparison.Ordinal) || string.Equals(record.Id, normalized, StringComparison.Ordinal)
                || string.Equals(record.SourceTechniqueId, rawId, StringComparison.Ordinal)
                || string.Equals(record.SourceTechniqueId, normalized, StringComparison.Ordinal)) return record;
        }
        return null;
    }

    private static bool TryDerivedDefinition(string normalized, out MclslTechniqueDefinition definition)
    {
        definition = null;
        const string marker = "_derived_";
        int index = normalized.LastIndexOf(marker, StringComparison.Ordinal);
        if (index <= 0 || !int.TryParse(normalized.Substring(index + marker.Length), out int variant) || variant <= 0) return false;
        MclslTechniqueDefinition baseDefinition = StaticDefinition(normalized.Substring(0, index));
        if (baseDefinition == null) return false;
        definition = MclslTechniqueOccupationSystem.VariantDefinition(baseDefinition, variant);
        return definition != null;
    }

    private static MclslTechniqueDefinition StaticDefinition(string normalized)
        => string.IsNullOrWhiteSpace(normalized) ? null
            : MclslCultivationCatalog.Techniques.FirstOrDefault(x => x != null && string.Equals(x.Id, normalized, StringComparison.Ordinal));

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

    private static string BuildEstateSummary(MclslFamilyTechniqueRecord detail)
    {
        string tags = string.IsNullOrWhiteSpace(detail?.LawTags) ? "本命道意" : detail.LawTags.Replace(",", "、");
        string maxRealm = detail?.MaxRealm ?? string.Empty;
        string realm = string.IsNullOrWhiteSpace(maxRealm) || MclslRealmIds.Index(maxRealm) < 0
            ? "上限不详" : "可修至" + MclslRealmIds.Display(maxRealm);
        return "此散藏功法以" + tags + "为本，" + realm + "。";
    }

    private static string TrimTechniqueId(string id) => (id ?? string.Empty).Trim();

    private static string JoinTags(IReadOnlyList<string> tags)
        => tags == null || tags.Count == 0 ? string.Empty
            : string.Join(",", tags.Where(tag => !string.IsNullOrWhiteSpace(tag)));
}
