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
    private const int EquipmentMigrationVersion = 3;
    private const string EquippedStatusPrefix = "mclsl_equipped_artifact_";
    private static readonly MclslArtifactEquipmentSlot[] Slots =
    {
        MclslArtifactEquipmentSlot.Weapon, MclslArtifactEquipmentSlot.Helmet,
        MclslArtifactEquipmentSlot.Armor, MclslArtifactEquipmentSlot.Boots,
        MclslArtifactEquipmentSlot.Ring, MclslArtifactEquipmentSlot.Amulet
    };
    internal static IReadOnlyList<MclslArtifactEquipmentSlot> EquipmentSlots => Slots;
    private static ConditionalWeakTable<Actor, object> _initializedActors = new();

    internal static void RegisterNativeWeapons()
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

            // Keep legacy prototypes registered so old equipment save records can
            // deserialize once and migrate into the independent Qiankun slots.
            if (item.EquipmentSlot == MclslArtifactEquipmentSlot.Weapon && item.Grade <= 4)
            {
                EquipmentAsset legacy = AssetManager.items.get(nativeId);
                if (legacy == null)
                {
                    EquipmentAsset template = AssetManager.items.get("sword_steel");
                    if (template != null) legacy = AssetManager.items.clone(nativeId, template.id);
                    legacy ??= new EquipmentAsset();
                    legacy.id = nativeId;
                    legacy.equipment_type = EquipmentType.Weapon;
                    legacy.equipment_subtype = "sword";
                    legacy.group_id = "sword";
                    legacy.path_icon = item.IconPath;
                    legacy.path_gameplay_sprite = item.IconPath;
                    legacy.material = "basic";
                    legacy.durability = 100;
                    legacy.equipment_value = item.Price;
                    legacy.pool_rate = 0;
                    legacy.is_pool_weapon = false;
                    legacy.name_class = nativeId;
                    legacy.translation_key = nativeId;
                    legacy.special_locale_id = nativeId;
                    legacy.has_locales = true;
                    legacy.rarity = Math.Max(1, item.Grade);
                    legacy.base_stats = new BaseStats();
                    foreach ((string stat, float value) in item.NativeStats)
                        MclslItemUseSystem.TrySetStat(legacy.base_stats, stat, value);
                    // clone already inserts the asset into the native library.
                    if (AssetManager.items.get(nativeId) == null) AssetManager.items.add(legacy);
                }
                // Compatibility records must resolve while loading old saves, but
                // artifacts are equipped only through the Qiankun Bag.
                legacy.show_in_meta_editor = false;
                legacy.can_be_given = false;
                legacy.mod_can_be_given = false;
            }

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

        // B001 was retired from the catalog, but its asset id must resolve while
        // loading old worlds so the first actor migration can remove it cleanly.
        const string retiredThunderId = "mclsl_artifact_b001";
        EquipmentAsset retired = AssetManager.items.get(retiredThunderId);
        if (retired == null)
        {
            EquipmentAsset template = AssetManager.items.get("sword_steel");
            retired = template == null ? new EquipmentAsset() : AssetManager.items.clone(retiredThunderId, template.id);
            retired.id = retiredThunderId;
            retired.equipment_type = EquipmentType.Weapon;
            retired.equipment_subtype = "sword";
            retired.group_id = "sword";
            retired.path_icon = string.Empty;
            retired.path_gameplay_sprite = string.Empty;
            retired.material = "basic";
            retired.durability = 100;
            retired.equipment_value = 0;
            retired.name_class = retiredThunderId;
            retired.translation_key = retiredThunderId;
            retired.special_locale_id = retiredThunderId;
            retired.has_locales = true;
            retired.pool_rate = 0;
            retired.is_pool_weapon = false;
            retired.base_stats = new BaseStats();
            if (AssetManager.items.get(retiredThunderId) == null) AssetManager.items.add(retired);
        }
        retired.show_in_meta_editor = false;
        retired.can_be_given = false;
        retired.mod_can_be_given = false;
    }

    internal static void ClearRuntime() => _initializedActors = new ConditionalWeakTable<Actor, object>();

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
    private static string StatusId(string itemId) => EquippedStatusPrefix + itemId;
    internal static string SlotKey(MclslArtifactEquipmentSlot slot) => slot.ToString();
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
        if (bag?.EquippedArtifactSlots == null || !bag.EquippedArtifactSlots.TryGetValue(SlotKey(slot), out string instanceId)) return string.Empty;
        MclslOwnedItem owned = bag.Items?.FirstOrDefault(x => x != null && x.InstanceId == instanceId);
        return MclslItemCatalog.Get(owned?.ItemId)?.Category == "Artifact" ? owned.ItemId : string.Empty;
    }

    internal static MclslOwnedItem EquippedArtifact(Actor actor, MclslArtifactEquipmentSlot slot)
    {
        MclslBagState bag = MclslBagSystem.Peek(actor);
        if (bag?.EquippedArtifactSlots == null || !bag.EquippedArtifactSlots.TryGetValue(SlotKey(slot), out string instanceId)) return null;
        return bag.Items?.FirstOrDefault(x => x != null && x.InstanceId == instanceId);
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
        if (bag.Items.Any(owned =>
        {
            MclslItemDefinition held = MclslItemCatalog.Get(owned?.ItemId);
            return held?.Category == "Artifact" && held.Id is not ("B080" or "B081")
                && held.EquipmentSlot == preferred.EquipmentSlot && held.Grade > preferred.Grade;
        })) return false;
        string equipped = EquippedArtifactId(actor, preferred.EquipmentSlot);
        if (equipped.Length == 0) return true;
        return EquipmentScore(actor, preferred) > EquipmentScore(actor, MclslItemCatalog.Get(equipped));
    }

    internal static string PreferredArtifactForSlot(Actor actor, MclslArtifactEquipmentSlot slot)
    {
        int gradeCap = Math.Clamp(MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) + 1, 1, 4);
        MclslItemDefinition best = null;
        foreach (MclslItemDefinition candidate in MclslItemCatalog.All)
        {
            if (candidate.Category != "Artifact" || candidate.EquipmentSlot != slot || candidate.Grade > gradeCap) continue;
            if (candidate.Id is "B080" or "B081") continue;
            if (best == null || Compare(actor, candidate, best) > 0) best = candidate;
        }
        return best?.Id ?? string.Empty;
    }

    internal static bool TryEquip(Actor actor, string instanceId, bool replaceExisting = false)
    {
        if (!MclslActorAccessor.Alive(actor) || MclslBagSystem.IsLocked(actor) || string.IsNullOrWhiteSpace(instanceId)) return false;
        MclslBagState bag = MclslBagSystem.Read(actor);
        MclslOwnedItem owned = bag.Items.FirstOrDefault(x => x != null && x.InstanceId == instanceId);
        MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
        if (item?.Category != "Artifact") return false;
        bag.EquippedArtifactSlots ??= new Dictionary<string, string>();
        string key = SlotKey(item.EquipmentSlot);
        if (bag.EquippedArtifactSlots.TryGetValue(key, out string current) && current == instanceId) return true;
        if (!replaceExisting && bag.EquippedArtifactSlots.ContainsKey(key)) return false;
        bag.EquippedArtifactSlots[key] = instanceId;
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
        foreach (MclslArtifactEquipmentSlot slot in Slots)
        {
            MclslOwnedItem best = null;
            foreach (MclslOwnedItem owned in bag.Items)
            {
                MclslItemDefinition candidate = MclslItemCatalog.Get(owned?.ItemId);
                if (candidate?.Category != "Artifact" || candidate.EquipmentSlot != slot) continue;
                if (best == null || Compare( actor, candidate, MclslItemCatalog.Get(best.ItemId)) > 0) best = owned;
            }
            if (best == null) continue;
            string key = SlotKey(slot);
            if (bag.EquippedArtifactSlots.TryGetValue(key, out string currentId))
            {
                MclslOwnedItem current = bag.Items.FirstOrDefault(x => x.InstanceId == currentId);
                if (current != null && Compare(actor, MclslItemCatalog.Get(best.ItemId), MclslItemCatalog.Get(current.ItemId)) <= 0) continue;
            }
            bag.EquippedArtifactSlots[key] = best.InstanceId;
        }
        MclslBagSystem.Write(actor, bag);
        RefreshEquipment(actor, bag);
    }

    internal static void OnActorInitialized(Actor actor)
    {
        if (actor?.data == null || _initializedActors.TryGetValue(actor, out _)) return;
        _initializedActors.Add(actor, new object());
        string savedBag = MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBag, string.Empty);
        string savedWeapon = LegacyArtifactId(actor?.equipment?.weapon?.getItem());
        bool hasWeaponBackup = !string.IsNullOrWhiteSpace(
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.ArtifactNativeWeaponBackup, string.Empty));
        if (string.IsNullOrWhiteSpace(savedBag) && savedWeapon.Length == 0 && !hasWeaponBackup) return;

        MclslBagState bag = MclslBagSystem.Read(actor);
        bag.EquippedArtifactSlots ??= new Dictionary<string, string>();
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ArtifactMigrationVersion) < EquipmentMigrationVersion)
        {
            RemoveLegacyArtifactEffects(actor);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ArtifactMigrationVersion, EquipmentMigrationVersion);
        }
        MigrateLegacyWeapon(actor, bag);
        RestoreNativeWeapon(actor);
        NormalizeOwnership(actor, bag, MclslRuntime.CurrentYear());
        bag.Version = 2;
        MclslBagSystem.Write(actor, bag);
        RefreshEquipment(actor, bag);
        TryEquipBest(actor);
        TryListLowerGradeArtifact(actor, MclslRuntime.CurrentYear());
    }

    internal static void OnArtifactAcquired(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        MclslBagState bag = MclslBagSystem.Read(actor);
        NormalizeOwnership(actor, bag, MclslRuntime.CurrentYear());
        MclslBagSystem.Write(actor, bag);
        TryEquipBest(actor);
        TryListLowerGradeArtifact(actor, MclslRuntime.CurrentYear());
    }

    internal static bool TryListLowerGradeArtifact(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return false;
        MclslBagState bag = MclslBagSystem.Peek(actor);
        if (bag.Items == null || bag.Items.Count < 2) return false;
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
                || string.IsNullOrWhiteSpace(owned.InstanceId)) continue;
            if (item.Grade >= highestGrades[(int)item.EquipmentSlot]) continue;
            if (surplus == null || item.Grade < MclslItemCatalog.Get(surplus.ItemId).Grade
                || item.Grade == MclslItemCatalog.Get(surplus.ItemId).Grade
                    && owned.AcquiredYear < surplus.AcquiredYear) surplus = owned;
        }
        if (surplus == null) return false;

        if (bag.EquippedArtifactSlots?.Values.Contains(surplus.InstanceId, StringComparer.Ordinal) == true)
        {
            TryEquipBest(actor);
            bag = MclslBagSystem.Peek(actor);
            if (bag.EquippedArtifactSlots?.Values.Contains(surplus.InstanceId, StringComparer.Ordinal) == true)
                return false;
        }
        if (!MclslTianxuanMarket.TryListArtifactSurplus(actor, surplus, year, deferWhenFull: false)) return false;
        bag.Items.Remove(surplus);
        MclslBagSystem.Write(actor, bag);
        MclslActorInfoPanel.RefreshOpenForActor(actor);
        return true;
    }

    internal static void NormalizeOwnership(Actor actor, MclslBagState bag, int year)
    {
        if (actor?.data == null || bag?.Items == null) return;
        bag.EquippedArtifactSlots ??= new Dictionary<string, string>();
        bag.Items.RemoveAll(x => x == null || MclslItemCatalog.Get(x.ItemId)?.Category == null);
        List<MclslOwnedItem> duplicates = new();
        foreach (IGrouping<string, MclslOwnedItem> group in bag.Items
                     .Where(x => MclslItemCatalog.Get(x.ItemId)?.Category == "Artifact")
                     .GroupBy(x => MclslItemCatalog.Get(x.ItemId).Id, StringComparer.Ordinal))
        {
            List<MclslOwnedItem> instances = group.ToList();
            if (instances.Count <= 1) continue;
            MclslOwnedItem keep = instances
                .OrderByDescending(x => bag.EquippedArtifactSlots.Values.Contains(x.InstanceId, StringComparer.Ordinal))
                .ThenByDescending(x => x.Durability)
                .ThenBy(x => x.InstanceId, StringComparer.Ordinal).First();
            foreach (MclslOwnedItem duplicate in instances)
            {
                if (ReferenceEquals(duplicate, keep)) continue;
                duplicates.Add(duplicate);
                bag.Items.Remove(duplicate);
                foreach (string key in bag.EquippedArtifactSlots.Where(pair => pair.Value == duplicate.InstanceId).Select(pair => pair.Key).ToArray())
                    bag.EquippedArtifactSlots.Remove(key);
            }
        }

        HashSet<string> liveInstances = bag.Items.Select(x => x.InstanceId).ToHashSet(StringComparer.Ordinal);
        HashSet<string> assignedInstances = new(StringComparer.Ordinal);
        foreach (string key in bag.EquippedArtifactSlots.Keys.ToArray())
        {
            MclslArtifactEquipmentSlot slot;
            if (!Enum.TryParse(key, true, out slot) || !Slots.Contains(slot)
                || !liveInstances.Contains(bag.EquippedArtifactSlots[key])
                || !assignedInstances.Add(bag.EquippedArtifactSlots[key]))
            {
                bag.EquippedArtifactSlots.Remove(key);
                continue;
            }
            MclslOwnedItem equipped = bag.Items.First(x => x.InstanceId == bag.EquippedArtifactSlots[key]);
            if (MclslItemCatalog.Get(equipped.ItemId)?.EquipmentSlot != slot) bag.EquippedArtifactSlots.Remove(key);
        }
        foreach (MclslOwnedItem duplicate in duplicates)
            MclslTianxuanMarket.TryListArtifactSurplus(actor, duplicate, year);
    }

    internal static void SyncEquippedStatuses(Actor actor)
    {
        if (actor?.data == null) return;
        MclslBagState bag = MclslBagSystem.Peek(actor);
        RefreshEquipment(actor, bag, refreshStats: false);
    }

    private static void RefreshEquipment(Actor actor, MclslBagState bag, bool refreshStats = true)
    {
        if (actor?.data == null) return;
        HashSet<string> expected = new(StringComparer.Ordinal);
        if (bag?.EquippedArtifactSlots != null)
        {
            foreach (string instanceId in bag.EquippedArtifactSlots.Values)
            {
                MclslOwnedItem owned = bag.Items?.FirstOrDefault(x => x?.InstanceId == instanceId);
                if (MclslItemCatalog.Get(owned?.ItemId)?.Category == "Artifact") expected.Add(StatusId(owned.ItemId));
            }
        }
        bool changed = false;
        foreach (MclslItemDefinition item in MclslItemCatalog.All)
        {
            if (item.Category != "Artifact") continue;
            string statusId = StatusId(item.Id);
            bool active = actor.hasStatus(statusId);
            if (expected.Contains(statusId) && !active)
            {
                actor.addStatusEffect(statusId, 999999999f);
                changed = true;
            }
            else if (!expected.Contains(statusId) && active)
            {
                actor.finishStatusEffect(statusId);
                changed = true;
            }
        }
        if (changed && refreshStats)
        {
            actor.setStatsDirty();
            actor.updateStats();
        }
    }

    private static void MigrateLegacyWeapon(Actor actor, MclslBagState bag)
    {
        Item native = actor?.equipment?.weapon?.getItem();
        string itemId = LegacyArtifactId(native);
        if (itemId.Length == 0) return;
        MclslOwnedItem existing = bag.Items.FirstOrDefault(x => x.ItemId == itemId);
        if (itemId != "B001")
        {
            MclslOwnedItem migrated = new()
            {
                ItemId = itemId,
                InstanceId = Guid.NewGuid().ToString("N"),
                Count = 1,
                Durability = Math.Clamp(native.data?.durability ?? 100, 1, 100),
                AcquiredYear = MclslRuntime.CurrentYear()
            };
            if (existing == null)
            {
                bag.Items.Add(migrated);
                bag.EquippedArtifactSlots[SlotKey(MclslArtifactEquipmentSlot.Weapon)] = migrated.InstanceId;
            }
            else
            {
                bag.Items.Remove(existing);
                MclslTianxuanMarket.TryListArtifactSurplus(actor, existing, MclslRuntime.CurrentYear());
                bag.Items.Add(migrated);
                bag.EquippedArtifactSlots[SlotKey(MclslArtifactEquipmentSlot.Weapon)] = migrated.InstanceId;
            }
        }

        try { actor.equipment.weapon.takeAwayItem(); } catch { }
        if (actor.equipment.weapon.getItem() == native) return;
        try { World.world?.items?.removeObject(native); } catch { }
        RestoreNativeWeapon(actor);
        MclslActorInfoPanel.RefreshOpenForActor(actor);
    }

    private static string LegacyArtifactId(Item native)
    {
        if (native?.asset == null) return string.Empty;
        string[] keys = { native.asset.id, (native.asset as EquipmentAsset)?.name_class,
            (native.asset as EquipmentAsset)?.translation_key, (native.asset as EquipmentAsset)?.special_locale_id };
        return keys.Select(MclslItemCatalog.NormalizeId).FirstOrDefault(id => id.Length > 0) ?? string.Empty;
    }

    private static void RestoreNativeWeapon(Actor actor)
    {
        if (actor?.equipment?.weapon == null || !actor.equipment.weapon.isEmpty() || World.world?.items == null) return;
        string stored = MclslActorAccessor.GetString(actor, MclslActorDataKeys.ArtifactNativeWeaponBackup);
        if (string.IsNullOrWhiteSpace(stored)) return;
        string[] ids = stored.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = ids.Length - 1; i >= 0; i--)
        {
            if (!long.TryParse(ids[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out long id)) continue;
            Item previous = World.world.items.get(id);
            if (previous == null) continue;
            try { actor.equipment.weapon.setItem(previous, actor); } catch { }
            if (actor.equipment.weapon.getItem() != previous) continue;
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ArtifactNativeWeaponBackup,
                string.Join(";", ids.Where(value => value != ids[i])));
            return;
        }
    }

    private static void RemoveLegacyArtifactEffects(Actor actor)
    {
        foreach (string status in new[] { "mclsl_artifact_fear", "mclsl_artifact_soul", "mclsl_artifact_ink", "mclsl_artifact_slow" })
            if (actor.hasStatus(status)) actor.finishStatusEffect(status);
        foreach (string key in new[] { "mclsl.v020.b008_target", "mclsl.v020.b008_hits", "mclsl.v020.b009_hits", "mclsl.v020.b010_hits", "mclsl.v020.b011_hits" })
            MclslActorAccessor.Set(actor, key, string.Empty);
    }

    private static long EquipmentScore(Actor actor, MclslItemDefinition item)
    {
        if (item == null) return long.MinValue;
        float power = item.NativeStats.Sum(stat => Math.Abs(stat.Value) * (stat.Id.StartsWith("multiplier_", StringComparison.Ordinal) ? 1000f : 1f));
        int realmFit = Math.Clamp(MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) + 1, 0, 4);
        return (long)item.Grade * 1000000000L + (long)(power * 1000f) + (item.Grade <= realmFit ? 100 : 0);
    }

    private static int Compare(Actor actor, MclslItemDefinition candidate, MclslItemDefinition current)
        => EquipmentScore(actor, candidate).CompareTo(EquipmentScore(actor, current));
}
