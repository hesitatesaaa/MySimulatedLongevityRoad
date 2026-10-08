using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Core;

internal sealed class Actor
{
    internal object data = new();
    internal long Id;
    internal bool Eligible = true, Potential = true;
    internal float Scale = 1;
    internal readonly Dictionary<string, object> Values = new();
    internal bool hasTrait(string id) => false;
}
namespace MySimulatedLongevityRoad.Data
{
    internal static class MclslActorDataKeys
    {
        internal const string CultivationStartYear = "start", LastCultivationYear = "last",
            TrueEssenceRemainder = "fraction", TrueEssence = "essence", Aptitude = "aptitude",
            CultivationSystem = "system", LastBreakthroughResult = "result", TechniqueInsight = "insight";
    }
    internal static class MclslCultivationSystemIds
    {
        internal const string AncientLaw = "ancient", NewLaw = "new";
    }
    internal static class MclslRealmIds
    {
        internal const string ChangSheng = "changsheng", LianQi = "lianqi";
        internal static readonly string[] Ordered = { LianQi, ChangSheng };
        internal static int Index(string id) => Array.IndexOf(Ordered, id);
    }
    internal static class MclslRealmProgress { internal const int LianQiEntryMinimum = 100; }
}
namespace MySimulatedLongevityRoad.Core
{
    internal static class MclslFrameDeadline { internal static double RemainingMs = double.MaxValue; }
    internal static class MclslDiagnostics { internal static void Cultivation(string key, string text) { } }
}
namespace MySimulatedLongevityRoad.Traits
{
    internal static class MclslTraitRegistration { internal const string HeavenFavorTraitId = "favor"; }
}
namespace MySimulatedLongevityRoad.Queries
{
    internal static class MclslWorldActorQuery
    {
        internal static void Track(Actor actor) { }
        internal static void MarkDirty() { }
    }
}
namespace MySimulatedLongevityRoad.UI
{
    internal static class MclslRankSnapshotSource { internal static void Invalidate() { } }
}
namespace MySimulatedLongevityRoad.Systems
{
    internal static class MclslActorAccessor
    {
        internal static long Id(Actor actor) => actor?.Id ?? 0;
        internal static bool Alive(Actor actor) => actor?.data != null;
        internal static string Realm(Actor actor) => GetString(actor, "realm");
        internal static int GetInt(Actor actor, string key, int fallback = 0)
            => actor?.Values.TryGetValue(key, out object value) == true ? (int)value : fallback;
        internal static float GetFloat(Actor actor, string key, float fallback = 0)
            => actor?.Values.TryGetValue(key, out object value) == true ? (float)value : fallback;
        internal static string GetString(Actor actor, string key, string fallback = "")
            => actor?.Values.TryGetValue(key, out object value) == true ? (string)value : fallback;
        internal static void Set(Actor actor, string key, int value) => actor.Values[key] = value;
        internal static void Set(Actor actor, string key, float value) => actor.Values[key] = value;
        internal static void Set(Actor actor, string key, string value) => actor.Values[key] = value;
    }
    internal static class MclslScheduler
    {
        internal static readonly MclslFirstCultivationQueue Pending = new();
        internal static void RequestFirstCultivation(Actor actor) => Pending.Add(actor.Id, 0);
        internal static void CompleteFirstCultivation(Actor actor) => Pending.Remove(actor.Id, 0);
    }
    internal static class MclslEligibility { internal static bool CanCultivate(Actor actor) => actor.Eligible; }
    internal static class MclslCultivationSystem
    {
        // Tests below keep all cursors within the real world year; no rewind.
        internal static int NormalizeLastCultivationYear(Actor actor, int year, int last)
            => MclslInitialCultivationPolicy.NormalizeCompletedYear(last, year, 110);
    }
    internal static class MclslSpiritualRootSystem
    {
        internal static bool HasCultivationPotential(Actor actor) => actor.Potential;
        internal static object Profile(Actor actor) => null;
        internal static float CountCultivationMultiplier(Actor actor) => actor.Scale;
        internal static MclslAptitudeGiftDefinition GiftForCultivation(Actor actor)
            => actor.Potential ? MclslAptitudeGiftCatalog.ForAptitude(50) : null;
    }
    internal static class MclslMindSystem { internal static float CultivationMultiplier(Actor actor) => 1; }
    internal static class MclslAncientLawSystem
    {
        internal static int AnnualFieldBonusPercent(Actor actor, string realm) => 0;
    }
    internal static class MclslTechniqueStageSystem { internal static float AnnualMultiplier(Actor actor) => 1; }
    internal static class MclslCultivationGrowthSystem
    {
        internal static bool ThrowOnGrant;
        internal static int Grants;
        internal static int CurrentTrueEssence(Actor actor) => MclslActorAccessor.GetInt(actor, "essence");
        internal static float RealmGainScale(string realm) => 1;
        internal static int GrantTrueEssence(Actor actor, string realm, float gain, int year, bool ancient,
            bool applyWorldState, bool applySameLaw, bool annualCultivation)
        {
            if (ThrowOnGrant) throw new InvalidOperationException("injected grant failure");
            Grants++;
            float total = gain + MclslActorAccessor.GetFloat(actor, "fraction");
            int integer = (int)total;
            MclslActorAccessor.Set(actor, "fraction", total - integer);
            MclslActorAccessor.Set(actor, "essence", CurrentTrueEssence(actor) + integer);
            return integer;
        }
    }
}
