using System;
using HarmonyLib;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems.Combat;

internal static class MclslRealmSuppressionSystem
{
    private static readonly AccessTools.FieldRef<Projectile, BaseSimObject> ProjectileInitiatorRef =
        AccessTools.FieldRefAccess<Projectile, BaseSimObject>("by_who");

    internal static void Apply(ref float damage, Actor defender, BaseSimObject attackerSource)
    {
        if (!MclslRuntimeSettings.CoreEnabled || damage <= 0f || defender?.data == null) return;

        Actor attacker = ResolveAttackSourceActor(attackerSource);
        if (attacker?.data == null || attacker == defender) return;

        int defenderTier = RealmTier(defender);
        int attackerTier = RealmTier(attacker);
        if (defenderTier <= 0 || attackerTier <= 0 || defenderTier == attackerTier) return;

        float multiplier = DamageMultiplier(attackerTier, defenderTier);
        damage = Math.Max(0f, damage * multiplier);
    }

    internal static float DamageMultiplier(int attackerTier, int defenderTier)
    {
        int diff = attackerTier - defenderTier;
        if (diff == 0) return 1f;
        if (diff < 0) return diff == -1 ? 0.2f : 0f;
        return 1.2f * diff;
    }

    private static int RealmTier(Actor actor)
    {
        if (actor?.data == null) return 0;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulEntityId, string.Empty))) return 0;

        int realmIndex = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        return realmIndex < 0 ? 0 : realmIndex + 1;
    }

    private static Actor ResolveAttackSourceActor(BaseSimObject source)
    {
        if (source == null) return null;

        try
        {
            Actor direct = source.a;
            if (direct != null) return direct;

            if (TryAsProjectile(source, out Projectile projectile))
            {
                return ProjectileInitiatorRef(projectile)?.a;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static bool TryAsProjectile(object value, out Projectile projectile)
    {
        projectile = value as Projectile;
        return projectile != null;
    }
}
