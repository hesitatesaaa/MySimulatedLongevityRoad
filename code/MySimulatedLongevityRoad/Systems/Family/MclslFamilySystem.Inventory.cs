using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>批量家库转移结果：Pending 表示预算耗尽、已提交已取部分，调用方应保留后续步骤。</summary>
internal enum MclslFamilyTransferOutcome : byte { Completed = 0, Pending = 1 }

/// <summary>
/// 单个家族的家库运行时索引（不落盘）。以 canonical ItemId 汇总数量，按 <c>family.Items</c>
/// 的真实引用顺序保存手持栈，并维护法宝 / 卷轴 / 妖兽用品三类候选的 ordinal 顺序。
/// 读取方通过 <see cref="MclslFamilySystem.FamilyStock"/> 取得，不再每次全量扫描家库。
/// 所有 <c>family.Items</c> 写入口都必须同步本索引：单件转移增量更新，批量遗产一次 <see cref="Rebuild"/>。
/// </summary>
internal sealed class MclslFamilyStock
{
    private readonly MclslFamilyRecord _family;
    private readonly Dictionary<string, int> _counts = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<MclslOwnedItem>> _held = new(StringComparer.Ordinal);
    private readonly List<string> _artifactCandidates = new();
    private readonly List<(MclslSpellDefinition Spell, string ScrollId)> _scrollCandidates = new();
    private readonly List<string> _beastCandidates = new();
    // 藏品展示用：按 canonical ItemId 汇总后的 (id,total) 列表，按物品名 ordinal 排序。
    // 懒重建，仅在读写计数发生变化后于下次读取时重排，绘制时不扫描、不去重。
    private List<KeyValuePair<string, int>> _aggregate;
    private bool _aggregateDirty = true;

    internal MclslFamilyStock(MclslFamilyRecord family) { _family = family; }

    /// <summary>按 canonical ItemId 汇总的数量视图（同名物品合并为一条），按名称稳定排序。</summary>
    internal IReadOnlyList<KeyValuePair<string, int>> Aggregate
    {
        get
        {
            if (_aggregateDirty) { RebuildAggregate(); _aggregateDirty = false; }
            return _aggregate ?? (IReadOnlyList<KeyValuePair<string, int>>)Array.Empty<KeyValuePair<string, int>>();
        }
    }

    private void RebuildAggregate()
    {
        _aggregate ??= new List<KeyValuePair<string, int>>();
        _aggregate.Clear();
        foreach (KeyValuePair<string, int> pair in _counts)
            if (pair.Value > 0) _aggregate.Add(pair);
        _aggregate.Sort(CompareAggregate);
    }

    private static int CompareAggregate(KeyValuePair<string, int> left, KeyValuePair<string, int> right)
    {
        string leftName = MclslItemCatalog.Get(left.Key)?.Name ?? left.Key;
        string rightName = MclslItemCatalog.Get(right.Key)?.Name ?? right.Key;
        int compare = string.CompareOrdinal(leftName, rightName);
        return compare != 0 ? compare : string.CompareOrdinal(left.Key, right.Key);
    }

    internal int Count(string itemId)
    {
        string canonical = MclslItemCatalog.NormalizeId(itemId);
        return canonical.Length > 0 && _counts.TryGetValue(canonical, out int count) ? count : 0;
    }

    internal MclslOwnedItem FindHeld(string itemId)
    {
        string canonical = MclslItemCatalog.NormalizeId(itemId);
        if (canonical.Length == 0 || !_held.TryGetValue(canonical, out List<MclslOwnedItem> list)) return null;
        for (int i = 0; i < list.Count; i++)
            if (list[i] != null && list[i].Count > 0) return list[i];
        return null;
    }

    internal IReadOnlyList<MclslOwnedItem> Held(string itemId)
    {
        string canonical = MclslItemCatalog.NormalizeId(itemId);
        return canonical.Length > 0 && _held.TryGetValue(canonical, out List<MclslOwnedItem> list)
            ? (IReadOnlyList<MclslOwnedItem>)list
            : Array.Empty<MclslOwnedItem>();
    }

    internal IReadOnlyList<string> ArtifactCandidates => _artifactCandidates;
    internal IReadOnlyList<(MclslSpellDefinition Spell, string ScrollId)> ScrollCandidates => _scrollCandidates;
    internal IReadOnlyList<string> BeastCandidates => _beastCandidates;

    /// <summary>整族重建。加载、批量遗产、失败回滚等允许一次整建；普通单件转移必须走增量。</summary>
    internal void Rebuild()
    {
        long sample = MclslPerformanceProbe.Begin();
        try
        {
            _counts.Clear();
            _held.Clear();
            _artifactCandidates.Clear();
            _scrollCandidates.Clear();
            _beastCandidates.Clear();
            if (_family?.Items != null)
            {
                for (int i = 0; i < _family.Items.Count; i++)
                {
                    MclslOwnedItem held = _family.Items[i];
                    if (held == null || held.Count <= 0) continue;
                    string canonical = MclslItemCatalog.NormalizeId(held.ItemId);
                    if (canonical.Length == 0) continue;
                    _counts.TryGetValue(canonical, out int count);
                    _counts[canonical] = count + held.Count;
                    if (!_held.TryGetValue(canonical, out List<MclslOwnedItem> list))
                        _held[canonical] = list = new List<MclslOwnedItem>();
                    list.Add(held);
                }
            }
            foreach (string canonical in _counts.Keys) AddCandidate(canonical);
            _aggregateDirty = true;
        }
        finally { MclslPerformanceProbe.End("family.stock.build", sample); }
    }

    /// <summary>家库新增一个手持引用（已追加到 family.Items 末尾）。</summary>
    internal void NoticeAdded(MclslOwnedItem held)
    {
        if (held == null || held.Count <= 0) return;
        string canonical = MclslItemCatalog.NormalizeId(held.ItemId);
        if (canonical.Length == 0) return;
        bool wasEmpty = !_counts.TryGetValue(canonical, out int count) || count <= 0;
        _counts[canonical] = Math.Max(0, count) + held.Count;
        if (!_held.TryGetValue(canonical, out List<MclslOwnedItem> list))
            _held[canonical] = list = new List<MclslOwnedItem>();
        list.Add(held);
        if (wasEmpty) AddCandidate(canonical);
        _aggregateDirty = true;
    }

    /// <summary>家库减少一个手持实例。<paramref name="stackRemains"/> 为该栈拆分后是否仍在列表中。
    /// 单件减少即按计划数量 1 扣减，与批量提交共用同一扣减/候选同步逻辑。</summary>
    internal void NoticeRemoved(MclslOwnedItem held, bool stackRemains)
        => ApplyRemoval(held, 1, stackRemains);

    /// <summary>批量提交时对一个手持按计划数量扣减（<paramref name="takenCount"/>），并同步候选。</summary>
    internal void ApplyRemoval(MclslOwnedItem held, int takenCount, bool stackRemains)
    {
        if (held == null || takenCount <= 0) return;
        string canonical = MclslItemCatalog.NormalizeId(held.ItemId);
        if (canonical.Length == 0 || !_counts.TryGetValue(canonical, out int count)) return;
        count -= takenCount;
        if (count <= 0) _counts.Remove(canonical);
        else _counts[canonical] = count;
        if (!stackRemains && _held.TryGetValue(canonical, out List<MclslOwnedItem> list))
        {
            for (int i = 0; i < list.Count; i++)
                if (ReferenceEquals(list[i], held)) { list.RemoveAt(i); break; }
            if (list.Count == 0) _held.Remove(canonical);
        }
        if (count <= 0) RemoveCandidate(canonical);
        _aggregateDirty = true;
    }

    private void AddCandidate(string canonical)
    {
        MclslItemDefinition definition = MclslItemCatalog.Get(canonical);
        if (definition == null) return;
        if (definition.Category == "Artifact")
        {
            if (definition.Id is "B080" or "B081") return;
            InsertOrdinal(_artifactCandidates, definition.Id);
        }
        else if (definition.Category == "SpellScroll"
            && definition.Id.EndsWith("_SCROLL", StringComparison.Ordinal)
            && MclslSpellSystem.TryGet(definition.Id.Substring(0, definition.Id.Length - 7), out MclslSpellDefinition spell))
        {
            int index = _scrollCandidates.Count;
            for (int i = 0; i < _scrollCandidates.Count; i++)
                if (string.CompareOrdinal(_scrollCandidates[i].ScrollId, definition.Id) > 0) { index = i; break; }
            _scrollCandidates.Insert(index, (spell, definition.Id));
        }
        else if (MclslFamilySystem.FamilyStockBeastIds.Contains(definition.Id))
        {
            InsertOrdinal(_beastCandidates, definition.Id);
        }
    }

    private void RemoveCandidate(string canonical)
    {
        MclslItemDefinition definition = MclslItemCatalog.Get(canonical);
        if (definition == null) return;
        if (definition.Category == "Artifact") _artifactCandidates.Remove(definition.Id);
        else if (definition.Category == "SpellScroll" && definition.Id.EndsWith("_SCROLL", StringComparison.Ordinal))
            _scrollCandidates.RemoveAll(x => string.Equals(x.ScrollId, definition.Id, StringComparison.Ordinal));
        else if (MclslFamilySystem.FamilyStockBeastIds.Contains(definition.Id)) _beastCandidates.Remove(definition.Id);
    }

    private static void InsertOrdinal(List<string> list, string id)
    {
        int lo = 0, hi = list.Count;
        while (lo < hi)
        {
            int mid = (lo + hi) >> 1;
            if (string.CompareOrdinal(list[mid], id) < 0) lo = mid + 1; else hi = mid;
        }
        if (lo < list.Count && string.Equals(list[lo], id, StringComparison.Ordinal)) return;
        list.Insert(lo, id);
    }
}

internal static partial class MclslFamilySystem
{
    // 家库运行时索引以家族记录为弱键：记录被回收即随之释放，不进入存档。
    private static ConditionalWeakTable<MclslFamilyRecord, MclslFamilyStock> _familyStocks = new();

    internal static readonly HashSet<string> FamilyStockBeastIds = BuildFamilyStockBeastIds();

    private static HashSet<string> BuildFamilyStockBeastIds()
    {
        HashSet<string> set = new(StringComparer.Ordinal);
        foreach (MclslBeastConsumableDefinition definition in MclslBeastConsumables.All)
            if (definition != null && !string.IsNullOrWhiteSpace(definition.Id)) set.Add(definition.Id);
        return set;
    }

    /// <summary>取得（必要时建立）某家族的运行时家库索引。</summary>
    internal static MclslFamilyStock FamilyStock(MclslFamilyRecord family)
    {
        if (family == null) return null;
        if (_familyStocks.TryGetValue(family, out MclslFamilyStock stock)) return stock;
        stock = new MclslFamilyStock(family);
        _familyStocks.Add(family, stock);
        stock.Rebuild();
        return stock;
    }

    /// <summary>批量写入口（死亡遗产 AddRange、市场托管遗产、失败回滚）后一次整建。</summary>
    internal static void RebuildFamilyStock(MclslFamilyRecord family)
    {
        if (family == null) return;
        if (!_familyStocks.TryGetValue(family, out MclslFamilyStock stock))
        {
            stock = new MclslFamilyStock(family);
            _familyStocks.Add(family, stock);
        }
        stock.Rebuild();
    }

    /// <summary>清世界 / 删族 / 绝嗣清算时释放索引。</summary>
    internal static void ReleaseFamilyStock(MclslFamilyRecord family)
    {
        if (family != null) _familyStocks.Remove(family);
    }

    internal static void ResetFamilyStocks() => _familyStocks = new ConditionalWeakTable<MclslFamilyRecord, MclslFamilyStock>();

    internal static void NoticeFamilyStockAdded(MclslFamilyRecord family, MclslOwnedItem held)
    {
        if (family == null) return;
        if (_familyStocks.TryGetValue(family, out MclslFamilyStock stock)) stock.NoticeAdded(held);
        else RebuildFamilyStock(family);
    }

    internal static void NoticeFamilyStockRemoved(MclslFamilyRecord family, MclslOwnedItem held, bool stackRemains)
    {
        if (family == null) return;
        if (_familyStocks.TryGetValue(family, out MclslFamilyStock stock)) stock.NoticeRemoved(held, stackRemains);
        else RebuildFamilyStock(family);
    }

    // ---- 单件转移核心 -------------------------------------------------------
    // 取消旧实现对家库整表复制：只复制一次乾坤袋工作副本，家库直接就地拆分/追加，
    // 校验通过后 Write、更新真实家库与索引，最后再做回调与纪事。

    /// <summary>对已定位的真实 held 执行一次存入/取出。</summary>
    internal static bool TransferOne(MclslFamilyRecord family, Actor actor, MclslOwnedItem held, bool deposit)
    {
        MclslBagState source = MclslBagSystem.Read(actor);
        return deposit ? DepositOne(family, actor, source, held) : WithdrawOne(family, actor, source, held);
    }

    private static bool DepositOne(MclslFamilyRecord family, Actor actor, MclslBagState source, MclslOwnedItem held)
    {
        if (held == null || MclslInventoryDataRules.IsEquipped(source, held)) return false;
        int index = IndexOfHeld(source.Items, held);
        if (index < 0) return false;
        MclslBagState bag = CopyBag(source);            // 唯一一次工作副本
        if (index >= bag.Items.Count) return false;
        MclslOwnedItem target = bag.Items[index];
        if (target == null || target.ItemId != held.ItemId || target.Count <= 0) return false;
        MclslOwnedItem moved = MclslFamilyTransferPolicy.TakeOne(bag.Items, target);
        if (moved == null) return false;
        if (!MclslInventoryDataRules.IsValid(bag)) return false;
        MclslBagSystem.Write(actor, bag);
        family.Items.Add(moved);
        NoticeFamilyStockAdded(family, moved);
        AddHistory(family, MclslRuntime.CurrentYear() + "年 · " + SafeName(actor) + "存入"
            + (MclslItemCatalog.Get(moved.ItemId)?.Name ?? moved.ItemId));
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    private static bool WithdrawOne(MclslFamilyRecord family, Actor actor, MclslBagState source, MclslOwnedItem held)
    {
        int familyIndex = IndexOfHeld(family.Items, held);
        if (familyIndex < 0) return false;
        MclslOwnedItem real = family.Items[familyIndex];
        if (real == null || real.Count <= 0) return false;
        bool wasStack = real.Count > 1;
        MclslBagState bag = CopyBag(source);            // 唯一一次工作副本
        // 先计划校验：用与 TakeOne 相同的拆分语义构造探针，不改动真实家库。
        MclslOwnedItem probe = MclslFamilyTransferPolicy.CopyItem(real);
        probe.Count = 1;
        if (wasStack && !string.IsNullOrWhiteSpace(probe.InstanceId)) probe.InstanceId = Guid.NewGuid().ToString("N");
        bag.Items.Add(probe);
        if (!MclslInventoryDataRules.IsValid(bag)) return false;
        bag.Items.Remove(probe);
        // 提交：真实家库按 TakeOne 拆分/移除；袋工作副本加入实际搬迁实例。
        MclslOwnedItem moved = MclslFamilyTransferPolicy.TakeOne(family.Items, real);
        if (moved == null) return false;
        bag.Items.Add(moved);
        try
        {
            MclslBagSystem.Write(actor, bag);
        }
        catch
        {
            RollbackWithdraw(family, real, wasStack, familyIndex);
            throw;
        }
        NoticeFamilyStockRemoved(family, real, wasStack);
        if (MclslItemCatalog.Get(moved.ItemId)?.Category == "Artifact")
        {
            try { MclslArtifactSystem.OnArtifactAcquired(actor); }
            catch (Exception ex) { MclslDiagnostics.Error("family-artifact", "家库法宝领取后处理失败：" + ex.Message); }
        }
        AddHistory(family, MclslRuntime.CurrentYear() + "年 · " + SafeName(actor) + "领取"
            + (MclslItemCatalog.Get(moved.ItemId)?.Name ?? moved.ItemId));
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    private static void RollbackWithdraw(MclslFamilyRecord family, MclslOwnedItem real, bool wasStack, int familyIndex)
    {
        if (wasStack) real.Count += 1;
        else if (familyIndex >= 0 && familyIndex <= family.Items.Count) family.Items.Insert(familyIndex, real);
        else family.Items.Add(real);
    }

    // ---- 批量转移（供供给工单使用） -----------------------------------------

    /// <summary>
    /// 一次多 needs 的家库领取：全程共用一份乾坤袋工作副本，逐件计费
    /// <see cref="MclslAnnualFrameBudget.TryConsumeOperation()"/>；预算耗尽时提交已取部分并返回 Pending。
    /// 同一次调用涉及多件法宝只触发一次 <see cref="MclslArtifactSystem.OnArtifactAcquired"/>。
    /// 不依赖天玄市场系统，只消费 <see cref="MclslMarketPurchaseNeed"/> 这一共享 Data DTO。
    /// </summary>
    internal static MclslFamilyTransferOutcome TransferNeeds(
        MclslFamilyRecord family, Actor actor, IReadOnlyList<MclslMarketPurchaseNeed> needs, out int movedItems,
        int maxItems = int.MaxValue)
    {
        movedItems = 0;
        if (family == null || actor?.data == null || needs == null || needs.Count == 0
            || FamilyOf(actor) != family || !MclslActorAccessor.Alive(actor) || MclslBagSystem.IsLocked(actor)
            || family.Items == null || family.Items.Count == 0)
            return MclslFamilyTransferOutcome.Completed;

        long sample = MclslPerformanceProbe.Begin();
        try
        {
            MclslFamilyStock stock = FamilyStock(family);
            MclslBagState bag = null;
            var plans = new List<HeldPlan>();
            var planByHeld = new Dictionary<MclslOwnedItem, HeldPlan>();
            List<string> history = null;
            bool anyArtifact = false;
            bool pending = false;

            // 规划阶段只操作「被触及 held」的局部影子栈（CopyItem），不触碰真实家库、不做全库扫描。
            for (int n = 0; n < needs.Count && !pending && movedItems < maxItems; n++)
            {
                MclslMarketPurchaseNeed need = needs[n];
                if (need == null || string.IsNullOrEmpty(need.ItemId) || need.Remaining <= 0) continue;
                IReadOnlyList<MclslOwnedItem> heldList = stock.Held(need.ItemId);
                for (int h = 0; h < heldList.Count && need.Remaining > 0 && movedItems < maxItems && !pending; h++)
                {
                    MclslOwnedItem real = heldList[h];
                    if (real == null || real.Count <= 0) continue;
                    if (!planByHeld.TryGetValue(real, out HeldPlan plan))
                    {
                        plan = new HeldPlan(real, real.Count);
                        planByHeld[real] = plan;
                        plans.Add(plan);
                    }
                    while (need.Remaining > 0 && plan.Shadow.Count > 0 && movedItems < maxItems)
                    {
                        if (!MclslAnnualFrameBudget.TryConsumeOperation()) { pending = true; break; }
                        MclslOwnedItem moved = MclslFamilyTransferPolicy.TakeOne(plan.Shadow, plan.Shadow[0]);
                        if (moved == null) break;
                        (bag ??= PrepareSupplyBag(actor)).Items.Add(moved);
                        plan.Taken++;
                        need.Remaining--;
                        movedItems++;
                        if (MclslItemCatalog.Get(moved.ItemId)?.Category == "Artifact") anyArtifact = true;
                        (history ??= new List<string>()).Add(MclslRuntime.CurrentYear() + "年 · " + SafeName(actor)
                            + "领取" + (MclslItemCatalog.Get(moved.ItemId)?.Name ?? moved.ItemId));
                    }
                }
            }

            if (movedItems > 0)
            {
                // 先校验后提交：规划阶段真实家库未动，校验失败只需丢弃工作副本。
                if (bag == null || !MclslInventoryDataRules.IsValid(bag))
                {
                    movedItems = 0;
                    return MclslFamilyTransferOutcome.Completed;
                }
                // 提交：按计划改真实数量/标记移除；压缩与「被移除实例原索引」在同一遍遍历完成。
                HashSet<MclslOwnedItem> removed = null;
                for (int i = 0; i < plans.Count; i++)
                {
                    HeldPlan plan = plans[i];
                    bool stackRemains = plan.Taken < plan.Original;
                    if (stackRemains) plan.Held.Count -= plan.Taken;
                    else (removed ??= new HashSet<MclslOwnedItem>()).Add(plan.Held);
                    stock.ApplyRemoval(plan.Held, plan.Taken, stackRemains);
                }
                List<(MclslOwnedItem Item, int Index)> removedAt = null;
                if (removed != null && removed.Count > 0)
                {
                    int write = 0;
                    for (int read = 0; read < family.Items.Count; read++)
                    {
                        MclslOwnedItem cur = family.Items[read];
                        if (cur != null && removed.Contains(cur))
                        {
                            (removedAt ??= new List<(MclslOwnedItem Item, int Index)>()).Add((cur, read));
                            continue;
                        }
                        family.Items[write++] = cur;
                    }
                    if (write < family.Items.Count) family.Items.RemoveRange(write, family.Items.Count - write);
                }
                try
                {
                    MclslBagSystem.Write(actor, bag);
                }
                catch
                {
                    // 提交异常：仅按实例记录局部回滚（按原索引恢复，保持选择顺序），不做整库备份。
                    for (int i = plans.Count - 1; i >= 0; i--)
                    {
                        HeldPlan plan = plans[i];
                        if (plan.Taken < plan.Original) plan.Held.Count += plan.Taken;
                    }
                    if (removedAt != null)
                        for (int i = 0; i < removedAt.Count; i++)   // 已按原索引升序采集，插入即还原顺序
                        {
                            var entry = removedAt[i];
                            family.Items.Insert(Math.Min(entry.Index, family.Items.Count), entry.Item);
                        }
                    RebuildFamilyStock(family);
                    throw;
                }
                if (history != null)
                    for (int i = 0; i < history.Count; i++) AddHistory(family, history[i]);
                if (anyArtifact)
                {
                    try { MclslArtifactSystem.OnArtifactAcquired(actor); }
                    catch (Exception ex) { MclslDiagnostics.Error("family-artifact", "家库批量法宝领取后处理失败：" + ex.Message); }
                }
                MclslWorldArchiveStore.MarkDirty();
            }
            return pending ? MclslFamilyTransferOutcome.Pending : MclslFamilyTransferOutcome.Completed;
        }
        finally { MclslPerformanceProbe.End("family.stock.transfer", sample); }
    }

    private static MclslBagState PrepareSupplyBag(Actor actor)
    {
        long sample = MclslPerformanceProbe.Begin();
        try { return CopyBag(MclslBagSystem.Read(actor)); }
        finally { MclslPerformanceProbe.End("family.stock.bag_copy", sample); }
    }

    /// <summary>批量规划期对单个真实 held 的影子栈与累计取用数。</summary>
    private sealed class HeldPlan
    {
        internal readonly MclslOwnedItem Held;
        internal readonly int Original;
        internal readonly List<MclslOwnedItem> Shadow;
        internal int Taken;
        internal HeldPlan(MclslOwnedItem held, int original)
        {
            Held = held;
            Original = original;
            Shadow = new List<MclslOwnedItem> { MclslFamilyTransferPolicy.CopyItem(held) };
        }
    }
}
