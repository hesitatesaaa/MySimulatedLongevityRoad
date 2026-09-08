using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static partial class MclslWorldSoulSystem
{
    private static string SafeName(Actor actor)
    {
        try { return MclslActorAccessor.DisplayName(actor); }
        catch { return "无名者"; }
    }

    private static string SafeNameOrEmpty(Actor actor)
    {
        try { return actor?.data == null ? string.Empty : MclslActorAccessor.DisplayName(actor); }
        catch { return string.Empty; }
    }

    private static void AnnounceWorldSoul(string message, string color, float duration)
    {
        if (!MclslRuntimeSettings.WorldSoulAnnouncementsEnabled) return;
        MclslAnnouncementSystem.Enqueue(message, color, duration, 1);
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int h = 53;
            foreach (char c in value ?? string.Empty)
                h = h * 43 + c;
            return h & int.MaxValue;
        }
    }
}
