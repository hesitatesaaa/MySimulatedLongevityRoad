using System;

namespace MySimulatedLongevityRoad.Systems;

// Presentation policy for the ten support spells that gained
// staged cast/hit atlases. Kept free of Unity types so the stage, duration and
// beneficiary rules can be exercised by an offline regression script.
internal static class MclslSpellSupportStage
{
    internal const float CastSeconds = 0.24f;
    internal const float HitSeconds = 0.72f;
    internal const int CastFrames = 8;
    internal const int HitFrames = 24;

    internal static bool Applies(string id) => id is
        "S002" or "S003" or "S005" or "S006" or "S011" or "S014" or "S018" or "S020" or "S024" or "S025";

    internal static bool IsStage(string id, string phase)
    {
        return Applies(id) && (phase == "cast" || phase == "hit");
    }

    // Staged support playback runs for its authored seconds; every other spell
    // keeps the existing max(frameCount/24, requested) * 1.5 display duration.
    internal static float ResolveDuration(string id, string phase, int frameCount, float requested, float fallback)
    {
        if (IsStage(id, phase))
            return requested > 0f ? requested : (phase == "cast" ? CastSeconds : HitSeconds);
        float seconds = requested > 0f ? requested : fallback;
        return Math.Max(seconds, frameCount / 24f) * 1.5f;
    }

}
