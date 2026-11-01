using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Systems;

// Game storage/UI and neighbouring subsystem boundaries only. The action,
// accessor (including index notifications), catalog, progress and realm limits
// are linked from production sources by the project file.
public class BaseSystemData
{
    public long id = 1;
    public string name = "测试角色";
    public Dictionary<string, object> Values = new();
    public void get<T>(string key, out T value, T fallback) => value = Values.TryGetValue(key, out var x) && x is T t ? t : fallback;
    public void set<T>(string key, T value) { Values[key] = value; Writes++; }
    public int Writes;
}
public sealed class ActorAsset { public string id = "human"; }
public sealed class Actor
{
    public BaseSystemData data = new();
    public ActorAsset asset = new();
    public object city;
    public bool Living = true;
    public HashSet<string> Traits = new();
    public bool isAlive() => Living;
    public bool hasTrait(string id) => Traits.Contains(id);
    public void removeTrait(string id)
    {
        if (!MclslTraitGrantRouter.IsSuppressed) throw new Exception("unsuppressed action removal");
        Traits.Remove(id);
    }
    public string getName() => data.name;
    public string name { set => data.name = value; }
}
public sealed class World
{
    public static World world = new();
    public object map_stats = new();
}
public static class WorldTip
{
    public static int Count;
    public static bool Throw;
    public static string Last;
    public static void showNowTop(string message) { if(Throw)throw new Exception("UI unavailable");Count++; Last = message; }
}
namespace MySimulatedLongevityRoad.Core
{
    internal static class MclslWorldContext { internal static string WorldId = "world"; }
    [Flags] internal enum MclslActorChange { Data=1, Membership=2, Traits=8, Presentation=32 }
    internal static class MclslRuntimeChanges
    {
        internal static int Publishes;
        internal static void Publish(Actor actor, MclslActorChange flags) => Publishes++;
        internal static void OnWrite(Actor actor, string key) { }
    }
    internal static class MclslDiagnostics
    {
        internal static int Errors;
        internal static void Error(string id, string text) => Errors++;
        internal static void Cultivation(string id, string text) { }
    }
}
namespace MySimulatedLongevityRoad.UI
{
    internal static class MclslActorInfoPanel
    {
        internal static int Refreshes;
        internal static void RefreshOpenForActor(Actor actor) => Refreshes++;
    }
}
namespace MySimulatedLongevityRoad.Traits
{
    internal static class MclslTraitRegistration
    {
        internal static bool ThrowSync;
        internal static string GiftTraitIdForAptitude(int n) => "gifts_6";
        internal static void SyncGiftTrait(Actor actor, int n, bool allowEntry=true, bool reconcileIdentity=true)
        {
            if (allowEntry || reconcileIdentity || !MclslTraitGrantRouter.IsSuppressed)
                throw new Exception("unsafe aptitude sync");
            if (ThrowSync) throw new Exception("injected boundary failure");
            actor.Traits.RemoveWhere(x => x.StartsWith("gifts_", StringComparison.Ordinal));
            actor.Traits.Add("gifts_6");
        }
    }
    internal static class MclslBeastActorRegistration
    {
        internal static bool IsOrdinary(Actor a) => false;
        internal static string DisplayName(Actor a,string r) => null;
        internal static void ReconcileForm(Actor a) { }
    }
}
namespace MySimulatedLongevityRoad.Systems
{
    internal static class MclslTraitGrantRouter
    {
        private static int depth;
        internal static bool IsSuppressed => depth > 0;
        internal static void SuppressRouting(Action action) { depth++; try { action(); } finally { depth--; } }
    }
    internal static class MclslEconomyCommands
    {
        internal static bool IsCurrencyKey(string key) => false;
        internal static long Balance(Actor a,string k) => 0;
        internal static void SetBalance(Actor a,string k,long v) { }
    }
    internal static class MclslBagSystem { internal static string SnapshotJson(Actor a) => ""; }
    internal static class MclslProfessionSystem { internal static void ReconcileAndPromote(Actor a) { } }
    internal static class MclslProfessionCraftingPolicy { internal static int ClampGrade(int x,int r) => x; }
    internal static class MclslTechniqueOccupationSystem
    {
        internal static int Changes;
        internal static void OnTechniqueChanged(Actor a,string before,string after) => Changes++;
        internal static void OnCultivationStateChanged(Actor a) { }
    }
    internal static class MclslSpellSystem { internal static void OnRealmAdvanced(Actor a,string before,string after) { } }
    internal static class MclslSpiritualRootSystem { internal static bool HasCultivationPotential(Actor a) => false; }
    internal static class MclslHonorificNameCatalog
    {
        internal const string SensingQiStage="sensing";
        internal static string Format(Actor a,string realm) => a.getName();
    }
    internal static class MclslImmortalPathSystem
    {
        internal static string HighestTargetRealm(Actor a) => "";
        internal static bool CanFillTechniqueLimit(Actor a,string r) => false;
    }
    internal static class MclslMindSystem { internal static int EnsureMindState(Actor a) => 50; }
}
namespace MySimulatedLongevityRoad.Systems.Combat
{
    internal static class MclslCombatCareerSystem
    {
        internal static bool Applicable(Actor a) => false;
        internal static void SyncTraits(Actor a) { }
    }
}
