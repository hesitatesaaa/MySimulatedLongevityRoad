using System;
using System.Reflection;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslActorAccessor
{
    internal static bool Alive(Actor actor) => actor?.data != null && actor.isAlive();
    internal static long Id(Actor actor) => actor?.data == null ? 0L : ((BaseSystemData)actor.data).id;

    internal static string GetString(Actor actor, string key, string fallback = "")
    {
        if (actor?.data == null) return fallback;
        ((BaseSystemData)actor.data).get(key, out string value, fallback);
        return value ?? fallback;
    }

    internal static int GetInt(Actor actor, string key, int fallback = 0)
    {
        if (actor?.data == null) return fallback;
        ((BaseSystemData)actor.data).get(key, out int value, fallback);
        return value;
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
        string oldTechniqueId = string.Empty;
        bool techniqueChanged = string.Equals(key, MclslActorDataKeys.TechniqueId, StringComparison.Ordinal);
        bool cultivationStateChanged = string.Equals(key, MclslActorDataKeys.Realm, StringComparison.Ordinal);
        bool honorificSourceChanged = IsHonorificSourceKey(key);
        if (techniqueChanged)
            oldTechniqueId = GetString(actor, key, string.Empty);

        ((BaseSystemData)actor.data).set(key, value ?? string.Empty);

        if (techniqueChanged)
            MclslTechniqueOccupationSystem.OnTechniqueChanged(actor, oldTechniqueId, value ?? string.Empty);
        if (cultivationStateChanged)
            MclslTechniqueOccupationSystem.OnCultivationStateChanged(actor);
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

    internal static void Set(Actor actor, string key, int value) { if (actor?.data != null) ((BaseSystemData)actor.data).set(key, value); }
    internal static void Set(Actor actor, string key, float value) { if (actor?.data != null) ((BaseSystemData)actor.data).set(key, value); }

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
        MclslHonorificNameCatalog.Format(actor, ResolveNameStage(actor, Realm(actor)));

    internal static string DisplayName(Actor actor, string realmId) =>
        MclslHonorificNameCatalog.Format(actor, ResolveNameStage(actor, realmId));

    internal static void ApplyDisplayName(Actor actor, string realmId)
    {
        if (actor?.data == null) return;
        string displayName = DisplayName(actor, realmId);
        if (string.IsNullOrWhiteSpace(displayName)) return;
        try
        {
            string current = actor.getName();
            if (string.Equals(current, displayName, StringComparison.Ordinal)) return;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Runtime-MclslActorAccessor-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Runtime/MclslActorAccessor.cs #1: " + mclslEmptyCatchEx.Message); }

        if (TryInvokeSetName(actor, displayName)) return;
        TrySetDataName(actor, displayName);
    }


    private static string ResolveNameStage(Actor actor, string realmId)
    {
        if (MclslRealmIds.Index(realmId) >= 0) return realmId;
        return HasCultivationPath(actor)
            ? MclslHonorificNameCatalog.SensingQiStage
            : string.Empty;
    }

    private static bool TryInvokeSetName(Actor actor, string displayName)
    {
        try
        {
            MethodInfo method = actor.GetType().GetMethod("setName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
            if (method != null)
            {
                method.Invoke(actor, new object[] { displayName });
                return true;
            }
        }
        catch { return false; }
        try
        {
            MethodInfo method = actor.GetType().GetMethod("setName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(bool) }, null);
            if (method == null) return false;
            method.Invoke(actor, new object[] { displayName, true });
            return true;
        }
        catch { return false; }
    }

    private static void TrySetDataName(Actor actor, string displayName)
    {
        try
        {
            object data = actor.data;
            Type type = data.GetType();
            TrySetStringMember(type, data, "name", displayName);
            TrySetStringMember(type, data, "customName", displayName);
            TrySetStringMember(type, data, "nameLocale", displayName);
            TrySetStringMember(type, data, "nameTemplate", displayName);
            TrySetStringMember(type, data, "localizedName", displayName);
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Runtime-MclslActorAccessor-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Runtime/MclslActorAccessor.cs #2: " + mclslEmptyCatchEx.Message); }
    }

    private static void TrySetStringMember(Type type, object target, string memberName, string value)
    {
        try
        {
            FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(string)) field.SetValue(target, value);
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Runtime-MclslActorAccessor-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Runtime/MclslActorAccessor.cs #3: " + mclslEmptyCatchEx.Message); }
        try
        {
            PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property?.CanWrite == true && property.PropertyType == typeof(string)) property.SetValue(target, value, null);
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Runtime-MclslActorAccessor-cs-4", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Runtime/MclslActorAccessor.cs #4: " + mclslEmptyCatchEx.Message); }
    }
}
