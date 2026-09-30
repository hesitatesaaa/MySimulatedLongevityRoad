using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// Stores artifact equipment in Qiankun Bag slot references. Artifact bonuses
/// are represented by vanilla BaseStats status assets, leaving native gear intact.
/// </summary>
internal static class MclslArtifactSystem
{
    private const string EquippedStatusPrefix = "mclsl_equipped_artifact_";
    private const int MaxOwnershipTransfersPerPass = 32;
    private static readonly MclslArtifactEquipmentSlot[] Slots =
    {
        MclslArtifactEquipmentSlot.Weapon, MclslArtifactEquipmentSlot.Helmet,
        MclslArtifactEquipmentSlot.Armor, MclslArtifactEquipmentSlot.Boots,
        MclslArtifactEquipmentSlot.Ring, MclslArtifactEquipmentSlot.Amulet
    };
    internal static IReadOnlyList<MclslArtifactEquipmentSlot> EquipmentSlots => Slots;
    private sealed class EquipmentState
    {
        internal bool Initialized, Restored, NeedsOwnershipSweep;
        internal long Revision = long.MinValue;
        internal readonly string[] Items = new string[6], ActiveStatuses = new string[6];
    }
    private static ConditionalWeakTable<Actor, EquipmentState> _initializedActors = new();
    private static readonly string[] SlotKeys = { "Weapon", "Helmet", "Armor", "Boots", "Ring", "Amulet" };
    private static readonly Dictionary<string, string> StatusIds = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, long> ItemPower = new(StringComparer.Ordinal);
    private static EquipmentState ReadEquipment(Actor actor, MclslBagState bag)
    {
        EquipmentState state = _initializedActors.GetOrCreateValue(actor);
        if (state.Revision == bag.MutationVersion) return state;
        for (int i = 0; i < SlotKeys.Length; i++)
        {
            MclslOwnedItem owned = bag.EquippedArtifactSlots.TryGetValue(SlotKeys[i], out string id) ? MclslBagSystem.FindInstance(bag, id) : null;
            state.Items[i] = owned?.ItemId ?? string.Empty;
        }
        state.Revision = bag.MutationVersion;
        return state;
    }

    internal static void RegisterEquipmentStatuses()
    {
        foreach (MclslItemDefinition item in MclslItemCatalog.All)
        {
            if (item.Category != "Artifact") continue;
            PrepareNativeStats(item);
            string nativeId = NativeId(item.Id);
            MclslLocalizationBridge.RegisterKey(item.Id.ToLowerInvariant(), item.Name);
            MclslLocalizationBridge.RegisterKey(item.Id.ToLowerInvariant() + "_description", item.EffectText);
            MclslLocalizationBridge.RegisterKey(item.Id, item.Name);
            MclslLocalizationBridge.RegisterKey(item.Id + " Description", item.EffectText);
            MclslLocalizationBridge.RegisterKey(nativeId, item.Name);
            MclslLocalizationBridge.RegisterKey(nativeId + "_description", item.EffectText);

            string statusId = StatusId(item.Id);
            StatusAsset status = AssetManager.status.get(statusId);
            if (status == null)
            {
                status = new StatusAsset
                {
                    id = statusId,
                    duration = 999999999f,
                    base_stats = new BaseStats(),
                    path_icon = item.IconPath,
                    locale_id = item.Id,
                    locale_description = item.Id + " Description"
                };
                AssetManager.status.add(status);
            }
            status.base_stats = new BaseStats();
            foreach ((string stat, float value) in item.NativeStats)
                MclslItemUseSystem.TrySetStat(status.base_stats, stat, value);
        }

    }

    internal static void ClearRuntime() => _initializedActors = new();
    internal static void Forget(Actor actor) { if (actor != null) _initializedActors.Remove(actor); }

    private static void PrepareNativeStats(MclslItemDefinition item)
    {
        List<(string Id, float Value)> supported = new(item.NativeStats.Length);
        foreach ((string id, float value) in item.NativeStats)
        {
            string actualId = id;
            if (AssetManager.base_stats_library?.get(actualId) == null
                && actualId == "multiplier_attack_speed"
                && AssetManager.base_stats_library?.get("attack_speed") != null)
                actualId = "attack_speed";
            if (AssetManager.base_stats_library?.get(actualId) != null) supported.Add((actualId, value));
        }
        item.NativeStats = supported.ToArray();
        string statText = item.NativeStats.Length == 0
            ? "该法宝未配置可用的原生属性。"
            : string.Join("；", item.NativeStats.Select(FormatStat)) + "。";
        if (!item.EffectText.Contains(statText, StringComparison.Ordinal))
            item.EffectText = item.EffectText.TrimEnd() + "\n" + statText;
    }

    private static string FormatStat((string Id, float Value) stat)
    {
        string number = stat.Value.ToString("0.##", CultureInfo.InvariantCulture);
        string percent = (stat.Value * 100f).ToString("0.##", CultureInfo.InvariantCulture) + "%";
        return stat.Id switch
        {
            "damage" => "固定伤害 +" + number,
            "health" => "生命 +" + number,
            "armor" => "护甲 +" + number + "（上限99）",
            "range" => "射程 +" + number,
            "throwing_range" => "投掷射程 +" + number,
            "accuracy" => "命中 +" + number + "（上限10）",
            "critical_chance" => "暴击率 +" + percent,
            "critical_damage_multiplier" => "暴击伤害倍率 +" + percent,
            "multiplier_damage" => "伤害倍率 +" + percent,
            "multiplier_health" => "生命倍率 +" + percent,
            "multiplier_speed" => "速度倍率 +" + percent,
            "multiplier_attack_speed" => "攻速倍率 +" + percent,
            "attack_speed" => "攻速属性 +" + number + "（上限10）",
            "stamina" => "体力 +" + number,
            "multiplier_stamina" => "体力倍率 +" + percent,
            "lifespan" => "寿命 +" + number,
            "multiplier_lifespan" => "寿命倍率 +" + percent,
            "knockback" => "击退 +" + number,
            "mana" => "法力 +" + number,
            _ => stat.Id + " +" + number
        };
    }

    internal static string NativeId(string itemId) => "mclsl_artifact_" + itemId.ToLowerInvariant();
    private static string StatusId(string itemId)
    {
        if (!StatusIds.TryGetValue(itemId, out string id)) StatusIds[itemId] = id = EquippedStatusPrefix + itemId;
        return id;
    }
    internal static string SlotKey(MclslArtifactEquipmentSlot slot) => SlotKeys[(int)slot];
    internal static string SlotDisplayName(MclslArtifactEquipmentSlot slot) => slot switch
    {
        MclslArtifactEquipmentSlot.Weapon => "武器",
        MclslArtifactEquipmentSlot.Helmet => "头盔",
        MclslArtifactEquipmentSlot.Armor => "盔甲",
        MclslArtifactEquipmentSlot.Boots => "靴子",
        MclslArtifactEquipmentSlot.Ring => "戒指",
        _ => "护符"
    };

    internal static string EquippedArtifactId(Actor actor) => EquippedArtifactId(actor, MclslArtifactEquipmentSlot.Weapon);

    internal static string EquippedArtifactId(Actor actor, MclslArtifactEquipmentSlot slot)
    {
        MclslBagState bag = MclslBagSystem.Peek(actor);
        return bag == null || actor == null ? string.Empty : ReadEquipment(actor, bag).Items[(int)slot];
    }

    internal static MclslOwnedItem EquippedArtifact(Actor actor, MclslArtifactEquipmentSlot slot)
    {
        MclslBagState bag = MclslBagSystem.Peek(actor);
        if (bag?.EquippedArtifactSlots == null || !bag.EquippedArtifactSlots.TryGetValue(SlotKey(slot), out string instanceId)) return null;
        return MclslBagSystem.FindInstance(bag, instanceId);
    }

    internal static bool HasEquippedArtifact(Actor actor)
        => MclslBagSystem.Peek(actor)?.EquippedArtifactSlots?.Count > 0;

    internal static void ApplyNativeStatCaps(Actor actor)
    {
        if (actor?.data == null || actor.stats == null || !HasEquippedArtifact(actor)) return;
        ClampStat(actor, "armor", 99f);
        ClampStat(actor, "accuracy", 10f);
        ClampStat(actor, "attack_speed", 10f);
    }

    private static void ClampStat(Actor actor, string id, float maximum)
    {
        if (AssetManager.base_stats_library?.get(id) == null) return;
        try
        {
            float current = actor.stats[id];
            if (!float.IsNaN(current) && !float.IsInfinity(current) && current > maximum)
                actor.stats[id] = maximum;
        }
        catch { }
    }

    internal static bool NeedsSuitableArtifact(Actor actor, MclslBagState bag, string preferredItemId)
    {
        MclslItemDefinition preferred = MclslItemCatalog.Get(preferredItemId);
        if (!MclslActorAccessor.Alive(actor) || preferred?.Category != "Artifact") return false;
        if (MclslBagSystem.Count(bag, preferred.Id) > 0) return false;
        foreach (MclslOwnedItem owned in bag.Items)
        {
            MclslItemDefinition held = MclslItemCatalog.Get(owned.ItemId);
            if (held != null && held.Id is not ("B080" or "B081")
                && held.EquipmentSlot == preferred.EquipmentSlot && held.Grade > preferred.Grade) return false;
        }
        string equipped = EquippedArtifactId(actor, preferred.EquipmentSlot);
        if (equipped.Length == 0) return true;
        return EquipmentScore(actor, preferred) > EquipmentScore(actor, MclslItemCatalog.Get(equipped));
    }

    internal static bool TryEquip(Actor actor, string instanceId, bool replaceExisting = false)
    {
        if (!MclslActorAccessor.Alive(actor) || MclslBagSystem.IsLocked(actor) || string.IsNullOrWhiteSpace(instanceId)) return false;
        MclslBagState bag = MclslBagSystem.Read(actor);
        MclslOwnedItem owned = MclslBagSystem.FindInstance(bag, instanceId);
        MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
        if (item?.Category != "Artifact") return false;
        bag.EquippedArtifactSlots ??= new Dictionary<string, string>();
        string key = SlotKey(item.EquipmentSlot);
        if (bag.EquippedArtifactSlots.TryGetValue(key, out string current) && current == instanceId) return true;
        if (!replaceExisting && bag.EquippedArtifactSlots.ContainsKey(key)) return false;
        owned = MclslInventoryDataRules.MaterializeArtifact(bag, owned);
        bag.EquippedArtifactSlots[key] = owned.InstanceId;
        bag.MutationVersion++;
        MclslBagSystem.Write(actor, bag);
        RefreshEquipment(actor, bag);
        MclslActorInfoPanel.RefreshOpenForActor(actor);
        return true;
    }

    internal static bool TryUnequip(Actor actor) => TryUnequip(actor, MclslArtifactEquipmentSlot.Weapon);

    internal static bool TryUnequip(Actor actor, MclslArtifactEquipmentSlot slot)
    {
        if (!MclslActorAccessor.Alive(actor) || MclslBagSystem.IsLocked(actor)) return false;
        MclslBagState bag = MclslBagSystem.Read(actor);
        if (bag?.EquippedArtifactSlots == null || !bag.EquippedArtifactSlots.Remove(SlotKey(slot))) return false;
        MclslBagSystem.Write(actor, bag);
        RefreshEquipment(actor, bag);
        MclslActorInfoPanel.RefreshOpenForActor(actor);
        return true;
    }

    internal static void TryEquipBest(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor) || MclslBagSystem.IsLocked(actor)) return;
        MclslBagState bag = MclslBagSystem.Read(actor);
        if (!EquipBestInBag(actor, bag)) return;
        MclslBagSystem.Write(actor, bag);
        RefreshEquipment(actor, bag);
    }

    private static bool EquipBestInBag(Actor actor, MclslBagState bag)
    {
        bool changed = false;
        bag.EquippedArtifactSlots ??= new Dictionary<string, string>();
        for (int i = 0; i < Slots.Length; i++)
        {
            MclslOwnedItem best = null;
            foreach (MclslOwnedItem owned in bag.Items)
            {
                MclslItemDefinition candidate = MclslItemCatalog.Get(owned?.ItemId);
                if (candidate?.Category != "Artifact" || candidate.EquipmentSlot != Slots[i]) continue;
                if (best == null || Compare(actor, candidate, MclslItemCatalog.Get(best.ItemId)) > 0)
                    best = owned;
            }
            if (best == null) continue;
            string key = SlotKeys[i];
            if (bag.EquippedArtifactSlots.TryGetValue(key, out string currentId))
            {
                MclslOwnedItem current = MclslBagSystem.FindInstance(bag, currentId);
                if (current != null && Compare(actor, MclslItemCatalog.Get(best.ItemId),
                    MclslItemCatalog.Get(current.ItemId)) <= 0) continue;
            }
            MclslOwnedItem selected = MclslInventoryDataRules.MaterializeArtifact(bag, best);
            bag.EquippedArtifactSlots[key] = selected.InstanceId;
            changed = true;
        }
        if (changed) bag.MutationVersion++;
        return changed;
    }

    internal static void OnActorInitialized(Actor actor)
    {
        if (actor?.data == null) return;
        EquipmentState state = _initializedActors.GetOrCreateValue(actor);
        if (state.Initialized)
        {
            if (!state.NeedsOwnershipSweep || !MclslTianxuanMarket.HasDeferredArtifactCapacity) return;
            MclslBagState pendingBag = MclslBagSystem.Peek(actor);
            bool moved = NormalizeOwnership(actor, pendingBag, MclslRuntime.CurrentYear(), out bool needsRetry);
            state.NeedsOwnershipSweep = needsRetry;
            if (moved) MclslBagSystem.Write(actor, pendingBag);
            return;
        }
        state.Initialized = true;
        MclslBagState bag = MclslBagSystem.Peek(actor);
        if (bag.Items.Count > 0)
        {
            bool moved = NormalizeOwnership(actor, bag, MclslRuntime.CurrentYear(), out bool needsRetry);
            state.NeedsOwnershipSweep = needsRetry;
            if (moved) MclslBagSystem.Write(actor, bag);
            TryEquipBest(actor);
            TryListLowerGradeArtifact(actor, MclslRuntime.CurrentYear());
        }
        RefreshEquipment(actor, bag);
    }

    internal static void OnArtifactAcquired(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        MclslBagState bag = MclslBagSystem.Read(actor);
        bool moved = NormalizeOwnership(actor, bag, MclslRuntime.CurrentYear(), out bool needsRetry);
        _initializedActors.GetOrCreateValue(actor).NeedsOwnershipSweep = needsRetry;
        if (moved) MclslBagSystem.Write(actor, bag);
        TryEquipBest(actor);
        TryListLowerGradeArtifact(actor, MclslRuntime.CurrentYear());
    }

    internal static bool TryListLowerGradeArtifact(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor) || MclslBagSystem.IsLocked(actor)) return false;
        MclslBagState bag = MclslBagSystem.Read(actor);
        if (bag.Items.Count < 2) return false;
        int[] highestGrades = new int[6];
        foreach (MclslOwnedItem owned in bag.Items)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
            if (item?.Category != "Artifact" || item.Id is "B080" or "B081") continue;
            int slot = (int)item.EquipmentSlot;
            highestGrades[slot] = Math.Max(highestGrades[slot], item.Grade);
        }
        MclslOwnedItem surplus = null;
        foreach (MclslOwnedItem owned in bag.Items)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
            if (item?.Category != "Artifact" || item.Id is "B080" or "B081"
                || string.IsNullOrWhiteSpace(owned.InstanceId)
                || item.Grade >= highestGrades[(int)item.EquipmentSlot]) continue;
            if (surplus == null || item.Grade < MclslItemCatalog.Get(surplus.ItemId).Grade
                || item.Grade == MclslItemCatalog.Get(surplus.ItemId).Grade
                    && owned.AcquiredYear < surplus.AcquiredYear) surplus = owned;
        }
        if (surplus == null) return false;
        if (MclslInventoryDataRules.IsEquipped(bag, surplus))
        {
            TryEquipBest(actor);
            bag = MclslBagSystem.Peek(actor);
            if (MclslInventoryDataRules.IsEquipped(bag, surplus)) return false;
        }
        MclslOwnedItem offer = surplus.Count == 1 ? surplus : MclslInventoryDataRules.SingleArtifact(surplus);
        if (!MclslTianxuanMarket.TryListArtifactSurplus(actor, offer, year, deferWhenFull: false)) return false;
        if (--surplus.Count == 0) bag.Items.Remove(surplus);
        MclslBagSystem.Write(actor, bag);
        MclslActorInfoPanel.RefreshOpenForActor(actor);
        return true;
    }

    private static readonly Dictionary<string, MclslOwnedItem> ArtifactKeepers = new(StringComparer.Ordinal);
    private static bool NormalizeOwnership(Actor actor, MclslBagState bag, int year, out bool needsRetry)
    {
        needsRetry = false;
        if (actor?.data == null || bag?.Items == null) return false;
        bool changed = false;
        bool hasDuplicate = false;
        int transferred = 0;
        ArtifactKeepers.Clear();
        try
        {
            foreach (MclslOwnedItem owned in bag.Items)
            {
                MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
                if (item?.Category != "Artifact" || item.Id is "B080" or "B081") continue;
                bool seen = ArtifactKeepers.TryGetValue(item.Id, out MclslOwnedItem previous);
                if (seen || owned.Count > 1) hasDuplicate = true;
                if (!seen || MclslInventoryDataRules.IsPreferredInstance(bag, owned, previous))
                    ArtifactKeepers[item.Id] = owned;
            }
            for (int i = bag.Items.Count - 1; i >= 0; i--)
            {
                if (transferred >= MaxOwnershipTransfersPerPass || MclslFrameDeadline.Expired)
                { needsRetry = hasDuplicate; break; }
                MclslOwnedItem owned = bag.Items[i];
                if (!ArtifactKeepers.TryGetValue(owned.ItemId, out MclslOwnedItem keeper)) continue;
                int reserved = ReferenceEquals(keeper, owned) ? 1 : 0;
                while (owned.Count > reserved)
                {
                    if (transferred >= MaxOwnershipTransfersPerPass || MclslFrameDeadline.Expired)
                    { needsRetry = true; break; }
                    MclslOwnedItem offer = owned.Count == 1 ? owned : MclslInventoryDataRules.SingleArtifact(owned);
                    if (!MclslTianxuanMarket.TryListArtifactSurplus(actor, offer, year))
                    {
                        if (!MclslTianxuanMarket.HasDeferredArtifactCapacity) needsRetry = true;
                        break;
                    }
                    owned.Count--;
                    changed = true;
                    transferred++;
                }
                if (owned.Count == 0) bag.Items.RemoveAt(i);
                if (needsRetry) break;
            }
            return changed;
        }
        finally { ArtifactKeepers.Clear(); }
    }

    private static void RefreshEquipment(Actor actor, MclslBagState bag, bool refreshStats = true)
    {
        if (actor?.data == null) return;
        EquipmentState state = ReadEquipment(actor, bag);
        bool changed = false;
        if (!state.Restored)
        {
            // Native status data is restored once after load. All later equipment
            // transactions touch at most six old/new status IDs.
            foreach (MclslItemDefinition item in MclslItemCatalog.All)
            {
                if (item.Category != "Artifact") continue;
                bool expected = false;
                for (int i = 0; i < state.Items.Length; i++) if (state.Items[i] == item.Id) { expected = true; break; }
                string status = StatusId(item.Id);
                if (!expected && actor.hasStatus(status)) { actor.finishStatusEffect(status); changed = true; }
            }
            state.Restored = true;
        }
        for (int i = 0; i < state.Items.Length; i++)
        {
            string next = state.Items[i].Length > 0 ? StatusId(state.Items[i]) : string.Empty;
            string previous = state.ActiveStatuses[i];
            if (!string.IsNullOrEmpty(previous) && previous != next && actor.hasStatus(previous))
            { actor.finishStatusEffect(previous); changed = true; }
            if (next.Length > 0 && !actor.hasStatus(next)) { actor.addStatusEffect(next, 999999999f); changed = true; }
            state.ActiveStatuses[i] = next;
        }
        if (changed && refreshStats)
        {
            actor.setStatsDirty();
            actor.updateStats();
        }
    }

    private static long EquipmentScore(Actor actor, MclslItemDefinition item)
    {
        if (item == null) return long.MinValue;
        if (!ItemPower.TryGetValue(item.Id, out long power))
        {
            float total = 0;
            foreach (var stat in item.NativeStats) total += Math.Abs(stat.Value) * (stat.Id.StartsWith("multiplier_", StringComparison.Ordinal) ? 1000f : 1f);
            ItemPower[item.Id] = power = (long)(total * 1000f);
        }
        int realmFit = actor == null ? 0
            : Math.Clamp(MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) + 1, 0, 4);
        return (long)item.Grade * 1000000000L + power + (item.Grade <= realmFit ? 100 : 0);
    }

    private static int Compare(Actor actor, MclslItemDefinition candidate, MclslItemDefinition current)
        => EquipmentScore(actor, candidate).CompareTo(EquipmentScore(actor, current));

}
