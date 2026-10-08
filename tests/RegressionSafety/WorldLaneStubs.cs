using MySimulatedLongevityRoad.Core;
namespace MySimulatedLongevityRoad.Data
{
    internal sealed class MclslAnnualClaimRecord { public int Year { get; set; } public string TargetId = ""; }
}
namespace MySimulatedLongevityRoad.Core
{
    internal static class MclslUnityProfiler
    {
        internal readonly struct Scope : IDisposable { public void Dispose() { } }
        internal static Scope Sample(string _) => new();
    }
}
namespace MySimulatedLongevityRoad.Systems
{
    internal static class WorldLaneSink
    {
        internal static readonly List<string> Events = new();
        internal static Func<string, bool> Action = _ => true;
        internal static bool Step(string name) { Events.Add(name); return Action(name); }
    }
    internal static class MclslHotPathPolicy
    {
        internal static bool IsActorHotPathSafe(Actor actor) => actor != null;
    }
    internal static class MclslActorRegistry
    {
        internal static readonly Dictionary<long, Actor> Actors = new();
        internal static IReadOnlyList<Actor> CreateView(List<long> ids) => ids.Select(id => Actors[id]).ToArray();
    }
    internal static class MclslTianxuanMarket
    {
        internal static void PublishPendingArtifactListings(int _) => WorldLaneSink.Step("Publish");
    }
    internal static class MclslWorldEraCycleSystem { internal static void TickAnnual(int _) => WorldLaneSink.Step("Era"); }
    internal static class MclslWorldCalamitySystem { internal static void TickAnnual(int _) => WorldLaneSink.Step("Calamity"); }
    internal static class MclslFactionMissionSystem { internal static void TickAnnual(int _) => WorldLaneSink.Step("Mission"); }
    internal static class MclslFactionPressureSystem { internal static void TickAnnual(int _) => WorldLaneSink.Step("Pressure"); }
    internal static class MclslWorldCaveSystem { internal static bool TickResolveAnnual(int _) => WorldLaneSink.Step("Cave"); internal static void BeginAnnual(int _) => WorldLaneSink.Step("BeginCave"); }
    internal static class MclslWorldChangeSystem { internal static bool TickResolveAnnual(int _) => WorldLaneSink.Step("Change"); internal static void BeginAnnual(int _) => WorldLaneSink.Step("BeginChange"); }
    internal static class MclslAdventureSystem { internal static bool TickResolveAnnual(int _) => WorldLaneSink.Step("Adventure"); internal static void BeginAnnual(int _) => WorldLaneSink.Step("BeginAdventure"); }
    internal static class MclslWorldSoulSystem { internal static bool TickAnnual(int _) => WorldLaneSink.Step("Soul");  }
    internal static class MclslInverseTruthSystem { internal static bool TickAnnual(int _) => WorldLaneSink.Step("Inverse");  }
    internal static class MclslSectLifecycleSystem { internal static bool TickResolveAnnual(int _) => WorldLaneSink.Step("Sect");  }
    internal static class MclslFamilySystem { internal static bool TickAnnual(int _) => WorldLaneSink.Step("Family");  }
    internal static class MclslTechniqueLineageSystem
    {
        internal static bool TickResolveAnnual(int _, IReadOnlyList<Actor> __) => WorldLaneSink.Step("Lineage");
        internal static void ClearRuntime() { }
    }
}
