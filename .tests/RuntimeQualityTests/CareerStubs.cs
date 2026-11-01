namespace MySimulatedLongevityRoad.Systems.Combat;

internal static partial class MclslCombatCareerSystem
{
    // Roll lives in the gameplay partial; randomness is outside persistence.
    private static int Roll(long id,int year,int salt)=>5000;
    internal static void ResetCache()=>States=new();
    internal static bool Blocked(Actor actor)=>IsBlocked(actor);
}
