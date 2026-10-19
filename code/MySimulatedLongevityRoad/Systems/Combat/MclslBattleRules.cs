using System;

namespace MySimulatedLongevityRoad.Systems.Combat;

// Pure combat policy, shared by the Unity pipeline and the offline regression tests.
internal static class MclslBattleRules
{
    internal static bool IsProfession(string id) => id != null && id.Length == 4 && id[0] == 'C'
        && int.TryParse(id.Substring(1), out int value) && value >= 1 && value <= 16;
    internal static int Rank(string id) => !IsProfession(id) ? 0 : (int.Parse(id.Substring(1)) - 1) % 4;
    internal static float Windup(string id) => IsProfession(id) ? Rank(id) switch
        { 2 => 0.4f, 3 => 0.85f, _ => 0.18f } : HeavyTerrain(id) ? 0.4f : 0.15f;
    internal static float Recovery(string id) => IsProfession(id) ? Rank(id) switch
        { 2 => 0.3f, 3 => 0.5f, _ => 0.18f } : 0.15f;
    internal static bool IsProjectile(string id) => id is "S001" or "S004" or "S013" or "S015" or "S017" or "C009";
    internal static bool IsBeam(string id) => id is "S009" or "S026";
    internal static bool IsArea(string id) => id is "S010" or "S012" or "S019" or "S022" or "S023"
        or "C004" or "C008" or "C012" or "C016";
    internal static float Travel(string id, float distance) => IsProjectile(id)
        ? Math.Clamp(distance / 30f, 0.15f, 0.6f) : IsArea(id) ? 0.25f : IsBeam(id) ? 0.12f : 0.08f;
    internal static bool IsSelf(string id) => id is "S002" or "S003" or "S005" or "S006" or "S011"
        or "S014" or "S018" or "S020" or "S024" or "S025" or "C002" or "C006" or "C010" or "C013" or "C015";
    internal static bool IsHardControl(string id) => id is "S007" or "S008" or "S012" or "S016" or "C003" or "C007";
    internal static bool IsChanneled(string id) => id is "S009" or "S026" or "C003" or "C007" or "C011" or "C012";
    internal static int Beats(string id) => id is "C003" or "C007" or "C011" or "C012" or "S009" or "S026" ? 3 : 1;
    internal static bool HeavyTerrain(string id) => id is "S010" or "S012" or "S019" or "S022" or "S023" or "S026"
        or "C004" or "C007" or "C008" or "C012" or "C016";
    internal static int TerrainRadius(int realm) => realm < 3 ? 0 : Math.Min(5, realm - 1);
    internal static int SpiritLimit(int realm) => realm < 0 ? 0 : realm < 2 ? 1 : realm == 2 ? 2 : realm < 5 ? 3 : 4;
    internal static bool InsightEligible(double seconds, int ownRealm, int enemyRealm, bool hostile, bool projection)
        => seconds >= 20 && ownRealm >= 0 && enemyRealm >= 0 && Math.Abs(ownRealm - enemyRealm) <= 1 && hostile && !projection;
    internal static string Visual(string id) => id switch
    {
        "C001" or "C002" or "C003" or "C004" => "S001",
        "C005" or "C007" or "C008" => "S017",
        "C006" => "S020", "C009" => "S004", "C010" => "S003",
        "C011" or "C012" => "S010", "C013" or "C014" or "C015" or "C016" => "S016",
        _ => id
    };
    internal static double ControlDuration(double requested, double now, ref double windowStart,
        ref double used, ref double protectedUntil)
    {
        if (now < protectedUntil) return 0;
        if (now >= windowStart + used + 0.05) { windowStart = now; used = 0; }
        double granted = Math.Min(Math.Max(0, requested), Math.Max(0, 1.2 - used));
        used += granted;
        if (used >= 1.2 - 0.0001) protectedUntil = now + granted + 2;
        return granted;
    }
}
