using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.Traits;

internal static partial class MclslWorldSoulActorRegistration
{
    private const int ZhenYueBengEquivalentDamageRadius = 480;
    private const int MaxAttackPulseHits = 64;
    private const int MaxAttackScanCandidatesPerPulse = 384;

    internal static void TickWorldSoulAttack(Actor actor)
    {
        if (actor?.data == null || !MclslActorAccessor.Alive(actor)) return;
        if (IsActivelyAttacking(actor)) TryPulseAttack(actor, GetSpriteSet(FolderForActor(actor)));
    }

    private static bool IsActivelyAttacking(Actor actor)
    {
        try
        {
            if (actor.has_attack_target && actor.attack_target != null) return true;
            if (actor.beh_actor_target != null) return true;
            if (actor.attackedBy is Actor attacker && attacker != actor && MclslActorAccessor.Alive(attacker)) return true;
            return actor.isJustAttacked();
        }
        catch
        {
            return false;
        }
    }

    private static void TryPulseAttack(Actor actor, SoulSpriteSet set)
    {
        if (actor?.data == null || !MclslActorAccessor.Alive(actor)) return;
        long actorId = ActorId(actor);
        if (actorId <= 0) return;

        int frame = Time.frameCount;
        if (NextPulseFrame.TryGetValue(actorId, out int nextFrame) && frame < nextFrame)
        {
            AttackUntilFrame[actorId] = Math.Max(AttackUntilFrame.TryGetValue(actorId, out int existingUntil) ? existingUntil : 0, frame + AttackWindowFrames(set));
            return;
        }

        NextPulseFrame[actorId] = frame + 22;
        AttackUntilFrame[actorId] = frame + AttackWindowFrames(set);
        ApplyAreaDamage(actor);
        QueueTerrainDamage(actor);
    }

    private static void ApplyAreaDamage(Actor source)
    {
        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetKnownActorsSnapshot();
        if (actors == null || actors.Count == 0) return;

        long sourceId = ActorId(source);
        int radius = AttackRadius(source);
        int radiusSq = radius * radius;
        int damage = AttackPulseDamage(source);
        int sourceX = SafeX(source);
        int sourceY = SafeY(source);
        int hits = 0;
        int scanned = 0;
        int count = actors.Count;
        int cursor = 0;
        if (sourceId > 0 && AttackScanCursor.TryGetValue(sourceId, out int cachedCursor))
            cursor = Math.Clamp(cachedCursor, 0, Math.Max(0, count - 1));

        while (scanned < MaxAttackScanCandidatesPerPulse && scanned < count)
        {
            Actor target = actors[cursor];
            cursor++;
            if (cursor >= count) cursor = 0;
            scanned++;
            if (target == null || target == source || !MclslActorAccessor.Alive(target)) continue;
            if (MclslWorldSoulActorRegistrationMarker(target)) continue;
            int dx = SafeX(target) - sourceX;
            int dy = SafeY(target) - sourceY;
            if (dx * dx + dy * dy > radiusSq) continue;
            DamageTarget(source, target, damage);
            hits++;
            if (hits >= MaxAttackPulseHits) break;
        }
        if (sourceId > 0) AttackScanCursor[sourceId] = cursor;
    }

    private static void DamageTarget(Actor source, Actor target, int damage)
    {
        if (target?.data == null || damage <= 0) return;
        try { target.attackedBy = source; } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-Attack-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.Attack.cs #1: " + mclslEmptyCatchEx.Message); }
        try
        {
            int current = target.data.health;
            int next = current - damage;
            if (next > 0)
            {
                target.data.health = next;
                return;
            }

            target.data.health = 0;
            try { target.die(true, AttackType.Other, true, true); }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-Attack-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.Attack.cs #2: " + mclslEmptyCatchEx.Message); }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-Attack-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.Attack.cs #3: " + mclslEmptyCatchEx.Message); }
    }

    private static int AttackWindowFrames(SoulSpriteSet set)
    {
        int attackFrames = set.Attack?.Length ?? 0;
        return Math.Clamp(attackFrames * 3, 18, 72);
    }

    private static int AttackRadius(Actor actor)
    {
        return ZhenYueBengEquivalentDamageRadius;
    }

    private static int AttackPulseDamage(Actor actor)
    {
        float damage = GetStatSafe(actor, "damage");
        int quality = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.WorldSoulEntityQuality, 3);
        int pulse = damage > 0f ? Mathf.RoundToInt(damage * 0.08f) : 800 + quality * 260;
        return Math.Clamp(pulse, 450, 4500);
    }

    private static float GetStatSafe(Actor actor, string statId)
    {
        try { return actor?.stats == null ? 0f : Math.Max(0f, actor.stats[statId]); }
        catch { return 0f; }
    }
}
