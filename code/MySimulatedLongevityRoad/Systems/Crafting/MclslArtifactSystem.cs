using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Data;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslArtifactSystem
{
    private static readonly Dictionary<(long ActorId, string ArtifactId), float> LastSpecialTime = new();
    private static readonly Queue<((long ActorId, string ArtifactId) Key, float Last)> SpecialExpiry = new();

    internal static void RegisterNativeWeapons()
    {
        foreach (MclslItemDefinition item in MclslItemCatalog.All)
        {
            if (item.Category != "Artifact") continue;
            string nativeId = NativeId(item.Id);
            if (AssetManager.items.get(nativeId) != null) continue;
            EquipmentAsset asset = new()
            {
                id = nativeId,
                equipment_type = EquipmentType.Weapon,
                equipment_subtype = nativeId,
                group_id = "sword",
                path_icon = item.IconPath,
                path_gameplay_sprite = item.IconPath,
                material = "basic",
                durability = 100,
                equipment_value = item.Price,
                pool_rate = 0,
                is_pool_weapon = false,
                name_class = item.Name,
                translation_key = item.Id,
                rarity = Math.Max(1, item.Grade)
            };
            asset.base_stats ??= new BaseStats();
            MclslItemUseSystem.TrySetStat(asset.base_stats, "damage", item.Grade switch { 1 => 18f, 2 => 42f, 3 => 90f, _ => 180f });
            if (item.Id is "B003" or "B009" or "B008")
                MclslItemUseSystem.TrySetStat(asset.base_stats, "multiplier_damage", item.Id == "B003" ? 0.60f : item.Id == "B009" ? 0.35f : 0.70f);
            if (item.Id == "B008") MclslItemUseSystem.TrySetStat(asset.base_stats, "accuracy", 95f);
            if (item.Id == "B009") MclslItemUseSystem.TrySetStat(asset.base_stats, "attack_speed", 0.15f);
            AssetManager.items.add(asset);
        }
        RegisterStatuses();
    }

    private static void RegisterStatuses()
    {
        AddStatus("mclsl_artifact_fear", 4f, ("multiplier_speed", -0.35f));
        AddStatus("mclsl_artifact_soul", 12f, ("multiplier_damage", -0.15f), ("armor", -10f));
        AddStatus("mclsl_artifact_ink", 15f, ("multiplier_damage", -0.15f), ("armor", -15f));
        AddStatus("mclsl_artifact_slow", 8f, ("multiplier_speed", -0.5f));
    }

    private static void AddStatus(string id, float duration, params (string Name, float Value)[] stats)
    {
        if (AssetManager.status.get(id) != null) return;
        StatusAsset status = new() { id = id, duration = duration, base_stats = new BaseStats(), path_icon = "ui/Items/B002" };
        foreach ((string name, float value) in stats) MclslItemUseSystem.TrySetStat(status.base_stats, name, value);
        AssetManager.status.add(status);
    }

    internal static void OnSuccessfulAttack(Actor attacker, BaseSimObject target)
    {
        if (attacker?.equipment?.weapon == null || target == null) return;
        Item weapon = attacker.equipment?.weapon?.getItem();
        string nativeId = weapon?.asset?.id;
        if (nativeId == null || !nativeId.StartsWith("mclsl_artifact_B", StringComparison.Ordinal)) return;
        if (!MclslActorAccessor.Alive(attacker) || !target.isAlive()) return;
        string id = nativeId.Substring("mclsl_artifact_".Length);
        float attack = Math.Max(1f, attacker.stats?["damage"] ?? 1f);
        switch (id)
        {
            case "B001":
                if (!Ready(attacker, id, 5f)) return;
                MclslArtifactEffectDriver.Ensure();
                MclslArtifactEffectDriver.Instance.StartExplosion(attacker, target.current_tile, target.isBuilding() ? target : null, attack);
                return;
            case "B002":
                if (!Ready(attacker, id, 30f)) return;
                AreaHit(attacker, target.current_tile, 6, attack * 1.8f + 150f, "mclsl_artifact_fear", "mclsl_artifact_soul");
                return;
            case "B003":
                if (!Ready(attacker, id, 20f)) return;
                float riftDamage = (attack * 4f + 500f) * (target.isBuilding() ? 2f : 1f);
                target.getHit(riftDamage * 0.35f, false, AttackType.Weapon, attacker, false, false, false);
                Hit(target, attacker, riftDamage * 0.65f);
                return;
            case "B004":
                if (!Ready(attacker, id, 12f)) return;
                KnifeVolley(attacker, target, attack);
                return;
            case "B005":
                if (!Ready(attacker, id, 18f)) return;
                Hit(target, attacker, attack * 2.2f + 300f);
                target.addStatusEffect("mclsl_artifact_ink", 15f);
                return;
            case "B006":
                if (!Ready(attacker, id, 35f)) return;
                AreaHit(attacker, target.current_tile, 8, attack * 4.5f + 800f, stunSeconds: 2f);
                if (target.isBuilding()) Hit(target, attacker, attack * 4.5f + 800f, 2f);
                return;
            case "B007":
                if (!Ready(attacker, id, 30f)) return;
                TideHit(attacker, target, attack * 3.5f + 650f);
                return;
            case "B008":
                if (Ready(attacker, id, 25f))
                {
                    Hit(target, attacker, attack * 5.5f + 1000f);
                    if (target.a != null) target.a.makeStunned(1.5f);
                }
                long targetId = target.a != null ? MclslActorAccessor.Id(target.a) : -1L;
                string previousId = MclslActorAccessor.GetString(attacker, "mclsl.v020.b008_target", "-2");
                int consecutive = previousId == targetId.ToString() ? MclslActorAccessor.GetInt(attacker, "mclsl.v020.b008_hits") + 1 : 1;
                if (consecutive >= 3)
                {
                    Hit(target, attacker, target.a != null ? Math.Min(2000f, target.a.getMaxHealth() * 0.05f) : 0f);
                    consecutive = 0;
                }
                MclslActorAccessor.Set(attacker, "mclsl.v020.b008_target", targetId.ToString());
                MclslActorAccessor.Set(attacker, "mclsl.v020.b008_hits", consecutive);
                return;
            case "B009":
                int count = MclslActorAccessor.GetInt(attacker, "mclsl.v020.b009_hits") + 1;
                if (count >= 4)
                {
                    Hit(target, attacker, attack);
                    if (target.a != null) target.a.makeStunned(MclslRealmIds.Index(MclslActorAccessor.Realm(target.a)) >= 3 ? 0.8f : 2f);
                    count = 0;
                }
                MclslActorAccessor.Set(attacker, "mclsl.v020.b009_hits", count);
                return;
        }
    }

    private static bool Ready(Actor attacker, string id, float cooldown)
    {
        var key = (MclslActorAccessor.Id(attacker), id);
        float now = Time.time;
        int cleaned = 0;
        while (SpecialExpiry.Count > 0 && cleaned < 32
            && (now - SpecialExpiry.Peek().Last >= 120f || SpecialExpiry.Count > 10000))
        {
            var expired = SpecialExpiry.Dequeue();
            if (LastSpecialTime.TryGetValue(expired.Key, out float current) && current == expired.Last)
                LastSpecialTime.Remove(expired.Key);
            cleaned++;
        }
        if (LastSpecialTime.TryGetValue(key, out float last) && now - last < cooldown) return false;
        LastSpecialTime[key] = now;
        SpecialExpiry.Enqueue((key, now));
        return true;
    }

    internal static void ClearRuntime()
    {
        LastSpecialTime.Clear();
        SpecialExpiry.Clear();
        MclslArtifactEffectDriver.Instance?.StopAllCoroutines();
    }

    private static void Hit(BaseSimObject target, Actor attacker, float damage, float multiplier = 1f)
    {
        if (target == null || !target.isAlive() || damage <= 0f) return;
        target.getHit(damage * multiplier, false, AttackType.Weapon, attacker);
    }

    internal static void Explosion(Actor attacker, WorldTile center, BaseSimObject building, float attack)
    {
        AreaHit(attacker, center, 3, 300f + attack * 2.5f);
        if (building != null && building.isAlive()) Hit(building, attacker, 300f + attack * 2.5f, 1.5f);
    }

    private static void AreaHit(Actor attacker, WorldTile center, int radius, float damage,
        string firstStatus = null, string secondStatus = null, float stunSeconds = 0f)
    {
        if (center == null || !MclslActorAccessor.Alive(attacker)) return;
        int radiusSquared = radius * radius;
        foreach (WorldTile tile in center.getTilesAround(radius))
        {
            int dx = tile.x - center.x;
            int dy = tile.y - center.y;
            int distanceSquared = dx * dx + dy * dy;
            if (distanceSquared > radiusSquared) continue;
            float falloff = Math.Max(0.5f, 1f - 0.5f * Mathf.Sqrt(distanceSquared) / radius);
            if (tile._units == null) continue;
            foreach (Actor victim in tile._units.ToArray())
            {
                if (!MclslActorAccessor.Alive(victim) || victim == attacker || !attacker.canAttackTarget(victim, false, false)) continue;
                Hit(victim, attacker, damage * falloff);
                if (firstStatus != null) victim.addStatusEffect(firstStatus);
                if (firstStatus == "mclsl_artifact_fear" && AssetManager.status.get("fear") != null)
                    victim.addStatusEffect("fear", 4f);
                if (secondStatus != null) victim.addStatusEffect(secondStatus);
                if (stunSeconds > 0f)
                {
                    victim.makeStunned(stunSeconds);
                    victim.applyRandomForce(4f, 4f);
                }
            }
        }
    }

    private static void KnifeVolley(Actor attacker, BaseSimObject primary, float attack)
    {
        List<Actor> targets = new();
        if (primary.a != null) targets.Add(primary.a);
        WorldTile center = attacker.current_tile;
        if (center != null)
        {
            foreach (WorldTile tile in center.getTilesAround(12))
            {
                if (tile._units == null) continue;
                int dx = tile.x - center.x;
                int dy = tile.y - center.y;
                if (dx * dx + dy * dy > 144) continue;
                foreach (Actor victim in tile._units)
                {
                    if (targets.Count >= 5) break;
                    if (victim != null && !targets.Contains(victim) && attacker.canAttackTarget(victim, false, false)) targets.Add(victim);
                }
                if (targets.Count >= 5) break;
            }
        }
        for (int i = 0; i < 5; i++)
        {
            BaseSimObject victim = targets.Count > 0 ? targets[i % targets.Count] : primary;
            Hit(victim, attacker, attack * 1.2f + 80f);
        }
    }

    private static void TideHit(Actor attacker, BaseSimObject primary, float damage)
    {
        WorldTile start = attacker.current_tile;
        WorldTile end = primary.current_tile;
        if (start == null || end == null) return;
        float dx = end.x - start.x;
        float dy = end.y - start.y;
        float length = Math.Max(1f, Mathf.Sqrt(dx * dx + dy * dy));
        foreach (WorldTile tile in start.getTilesAround(14))
        {
            float x = tile.x - start.x;
            float y = tile.y - start.y;
            float forward = (x * dx + y * dy) / length;
            float side = Math.Abs(x * dy - y * dx) / length;
            if (forward < 0f || forward > 14f || side > 3f || tile._units == null) continue;
            foreach (Actor victim in tile._units.ToArray())
            {
                if (!MclslActorAccessor.Alive(victim) || victim == attacker || !attacker.canAttackTarget(victim, false, false)) continue;
                Hit(victim, attacker, damage * (tile.is_liquid ? 1.25f : 1f));
                victim.addStatusEffect("mclsl_artifact_slow", 8f);
                victim.applyRandomForce(4f, 4f);
            }
        }
    }

    internal static string NativeId(string itemId) => "mclsl_artifact_" + itemId;

    internal static bool TryEquip(Actor actor, string instanceId)
    {
        if (!MclslActorAccessor.Alive(actor) || actor.equipment?.weapon == null || !actor.equipment.weapon.isEmpty()) return false;
        MclslBagState bag = MclslBagSystem.Read(actor);
        MclslOwnedItem owned = bag.Items.FirstOrDefault(x => x.InstanceId == instanceId);
        MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
        if (item?.Category != "Artifact") return false;
        EquipmentAsset asset = AssetManager.items.get(NativeId(item.Id));
        if (asset == null || World.world?.items == null) return false;
        Item native = World.world.items.newItem(asset);
        if (native?.data == null) return false;
        native.data.durability = Math.Clamp(owned.Durability, 1, 100);
        actor.equipment.weapon.setItem(native, actor);
        bag.Items.Remove(owned);
        MclslBagSystem.Write(actor, bag);
        return true;
    }

    internal static void TryEquipBest(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor) || actor.equipment?.weapon == null || !actor.equipment.weapon.isEmpty()) return;
        MclslOwnedItem best = MclslBagSystem.Read(actor).Items
            .Where(x => MclslItemCatalog.Get(x.ItemId)?.Category == "Artifact")
            .OrderByDescending(x => MclslItemCatalog.Get(x.ItemId).Grade)
            .FirstOrDefault();
        if (best != null) TryEquip(actor, best.InstanceId);
    }
}

internal sealed class MclslArtifactEffectDriver : MonoBehaviour
{
    internal static MclslArtifactEffectDriver Instance;

    internal static void Ensure()
    {
        if (Instance != null) return;
        GameObject host = new("MclslArtifactEffectDriver");
        DontDestroyOnLoad(host);
        Instance = host.AddComponent<MclslArtifactEffectDriver>();
    }

    internal void StartExplosion(Actor attacker, WorldTile center, BaseSimObject building, float attack) => StartCoroutine(Explode(attacker, center, building, attack));

    private static IEnumerator Explode(Actor attacker, WorldTile center, BaseSimObject building, float attack)
    {
        yield return new WaitForSeconds(1.5f);
        MclslArtifactSystem.Explosion(attacker, center, building, attack);
    }
}
