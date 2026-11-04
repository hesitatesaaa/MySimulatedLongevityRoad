using System;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslActorAccessor
{
    internal static bool Alive(Actor actor) => actor?.data != null && actor.isAlive();
    internal static long Id(Actor actor) => actor?.data == null ? 0L : ((BaseSystemData)actor.data).id;

    internal static string GetString(Actor actor, string key, string fallback = "")
    {
        if (actor?.data == null) return fallback;
        if (key == MclslActorDataKeys.QiankunBag) return MclslBagSystem.SnapshotJson(actor);
        ((BaseSystemData)actor.data).get(key, out string value, fallback);
        return value ?? fallback;
    }

    internal static int GetInt(Actor actor, string key, int fallback = 0)
    {
        if (actor?.data == null) return fallback;
        if (MclslEconomyCommands.IsCurrencyKey(key)) return checked((int)GetMoney(actor, key));
        ((BaseSystemData)actor.data).get(key, out int value, fallback);
        return value;
    }

    internal static long GetMoney(Actor actor, string key, long fallback = 0)
        => actor?.data == null ? fallback : MclslEconomyCommands.Balance(actor, key);

    internal static void Set(Actor actor, string key, long value)
    {
        if (actor?.data == null) return;
        MclslEconomyCommands.SetBalance(actor, key, value);
        MclslRuntimeChanges.OnWrite(actor, key);
    }

    internal static float GetFloat(Actor actor, string key, float fallback = 0f)
    {
        if (actor?.data == null) return fallback;
        ((BaseSystemData)actor.data).get(key, out float value, fallback);
        return value;
    }

    internal static void Set(Actor actor, string key, string value)
    {
        if (actor?.data == null) return;
        value ??= string.Empty;
        if (string.Equals(GetString(actor, key), value, StringComparison.Ordinal)) return;
        string oldTechniqueId = string.Empty;
        bool techniqueChanged = string.Equals(key, MclslActorDataKeys.TechniqueId, StringComparison.Ordinal);
        bool cultivationStateChanged = string.Equals(key, MclslActorDataKeys.Realm, StringComparison.Ordinal);
        bool professionChanged = string.Equals(key, MclslActorDataKeys.Profession, StringComparison.Ordinal);
        string oldRealm = cultivationStateChanged ? GetString(actor, key, string.Empty) : string.Empty;
        bool honorificSourceChanged = IsHonorificSourceKey(key);
        if (techniqueChanged)
            oldTechniqueId = GetString(actor, key, string.Empty);

        ((BaseSystemData)actor.data).set(key, value ?? string.Empty);
        MclslRuntimeChanges.OnWrite(actor, key);
        bool wildBeast = MySimulatedLongevityRoad.Traits.MclslBeastActorRegistration.IsOrdinary(actor);
        if (!wildBeast && (cultivationStateChanged || professionChanged)) MclslProfessionSystem.ReconcileAndPromote(actor);

        if (techniqueChanged)
            MclslTechniqueOccupationSystem.OnTechniqueChanged(actor, oldTechniqueId, value ?? string.Empty);
        if (cultivationStateChanged)
        {
            if (!wildBeast)
            {
                MclslTechniqueOccupationSystem.OnCultivationStateChanged(actor);
                MclslSpellSystem.OnRealmAdvanced(actor, oldRealm, value);
            }
            MySimulatedLongevityRoad.Traits.MclslBeastActorRegistration.ReconcileForm(actor);
            if (Combat.MclslCombatCareerSystem.Applicable(actor)) Combat.MclslCombatCareerSystem.SyncTraits(actor);
        }
        if (honorificSourceChanged)
        {
            string realm = Realm(actor);
            if (MclslRealmIds.Index(realm) >= MclslRealmIds.Index(MclslRealmIds.HuaShen))
                ApplyDisplayName(actor, realm);
        }
    }
    private static bool IsHonorificSourceKey(string key)
    {
        return string.Equals(key, MclslActorDataKeys.DivineMarrowTags, StringComparison.Ordinal)
            || string.Equals(key, MclslActorDataKeys.DivineChangeTags, StringComparison.Ordinal)
            || string.Equals(key, MclslActorDataKeys.NascentEssenceTags, StringComparison.Ordinal)
            || string.Equals(key, MclslActorDataKeys.GoldenCoreLaws, StringComparison.Ordinal)
            || string.Equals(key, MclslActorDataKeys.SpiritualRootPrimary, StringComparison.Ordinal)
            || string.Equals(key, MclslActorDataKeys.SpiritualRootAttributes, StringComparison.Ordinal)
            || string.Equals(key, MclslActorDataKeys.AncientDivineIntent, StringComparison.Ordinal)
            || string.Equals(key, MclslActorDataKeys.AncientDaoIntent, StringComparison.Ordinal)
            || string.Equals(key, MclslActorDataKeys.AncientDaoName, StringComparison.Ordinal)
            || string.Equals(key, MclslActorDataKeys.WorldSoulId, StringComparison.Ordinal)
            || string.Equals(key, MclslActorDataKeys.WorldSoulName, StringComparison.Ordinal)
            || string.Equals(key, MclslActorDataKeys.InverseTruthId, StringComparison.Ordinal)
            || string.Equals(key, MclslActorDataKeys.InverseTruthName, StringComparison.Ordinal);
    }

    internal static void Set(Actor actor, string key, int value)
    {
        if (MclslEconomyCommands.IsCurrencyKey(key)) { Set(actor, key, (long)value); return; }
        if (key == MclslActorDataKeys.ProfessionGrade)
            value = MclslProfessionCraftingPolicy.ClampGrade(value, MclslRealmIds.Index(Realm(actor)));
        if (actor?.data == null || GetInt(actor, key, int.MinValue) == value) return;
        ((BaseSystemData)actor.data).set(key, value);
        MclslRuntimeChanges.OnWrite(actor, key);
        if (key == MclslActorDataKeys.ProfessionExperience)
            MclslProfessionSystem.ReconcileAndPromote(actor);
    }
    internal static void Set(Actor actor, string key, float value)
    {
        if (actor?.data == null || GetFloat(actor, key, float.NaN).Equals(value)) return;
        ((BaseSystemData)actor.data).set(key, value);
        MclslRuntimeChanges.OnWrite(actor, key);
    }

    internal static string Realm(Actor actor) => GetString(actor, MclslActorDataKeys.Realm, string.Empty);
    internal static bool IsCultivator(Actor actor) => !string.IsNullOrWhiteSpace(Realm(actor));

    internal static bool HasCultivationPath(Actor actor)
    {
        if (actor?.data == null || !Alive(actor)) return false;
        if (IsCultivator(actor)) return true;
        if (MclslSpiritualRootSystem.HasCultivationPotential(actor)) return true;
        string system = GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        if (system == MclslCultivationSystemIds.AncientLaw
            || system == MclslCultivationSystemIds.NewLaw) return true;
        return GetInt(actor, MclslActorDataKeys.TrueEssence, 0) > 0
            || GetFloat(actor, MclslActorDataKeys.CultivationProgress, 0f) > 0f;
    }

    internal static string DisplayName(Actor actor) =>
        MySimulatedLongevityRoad.Traits.MclslBeastActorRegistration.DisplayName(actor, Realm(actor))
        ?? MclslHonorificNameCatalog.Format(actor, ResolveNameStage(actor, Realm(actor)));

    internal static string DisplayName(Actor actor, string realmId) =>
        MySimulatedLongevityRoad.Traits.MclslBeastActorRegistration.DisplayName(actor, realmId)
        ?? MclslHonorificNameCatalog.Format(actor, ResolveNameStage(actor, realmId));

    internal static string SafeName(Actor actor, string fallback)
    {
        try { return DisplayName(actor); } catch { return fallback; }
    }

    /// <summary>Floor age in whole years; a null or throwing actor reads as 0.</summary>
    internal static int Age(Actor actor)
    {
        try { return Math.Max(0, (int)Math.Floor(actor?.getAge() ?? 0f)); }
        catch { return 0; }
    }

    /// <summary>Clamped stat read; a missing stat or throwing actor reads as 0.</summary>
    internal static float StatValue(Actor actor, string statId)
    {
        try { return actor?.stats == null ? 0f : Math.Max(0f, actor.stats[statId]); }
        catch { return 0f; }
    }

    /// <summary>Registry lookup that yields null instead of a false-returning out parameter.</summary>
    internal static Actor Resolve(long actorId)
        => MclslActorRegistry.ResolveKnownOrWorld(actorId, out Actor actor) ? actor : null;

    internal static void ApplyDisplayName(Actor actor, string realmId)
    {
        if (actor?.data == null) return;
        string displayName = DisplayName(actor, realmId);
        if (string.IsNullOrWhiteSpace(displayName)) return;
        if (!string.Equals(actor.getName(), displayName, StringComparison.Ordinal)) actor.name = displayName;
    }


    private static string ResolveNameStage(Actor actor, string realmId)
    {
        if (MclslRealmIds.Index(realmId) >= 0) return realmId;
        return HasCultivationPath(actor)
            ? MclslHonorificNameCatalog.SensingQiStage
            : string.Empty;
    }

}
