using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

/// <summary>
/// Pure decision logic for trimming the world event archive. Kept independent of the
/// repository so the counting and the lazy (hysteresis) trim are regression-tested in
/// isolation, mirroring how <see cref="MclslCleanupPolicy"/> and <see cref="MclslEventCatalog"/>
/// are tested.
///
/// Semantics are identical to the historical behavior the repository previously inlined:
/// events are trimmed from the head, keeping the tail; milestone events are capped
/// separately by <paramref name="maxMilestones"/>. The only behavioural addition is a
/// hysteresis factor: in non-forced mode a scan is skipped until a counter overshoots its
/// cap by <c>1.25x</c> (see <see cref="TrimHysteresisMultiplier"/>), so a steady append
/// stream sitting at the cap no longer rescans the whole archive on every insertion.
/// Forced mode recounts the list and enforces the caps unconditionally at the save boundary.
/// </summary>
internal static class MclslEventTrimPolicy
{
    internal const float TrimHysteresisMultiplier = 1.25f;

    /// <summary>
    /// Computes how many events would need removal, or that no trim is needed.
    /// </summary>
    /// <param name="ordinaryCount">Current count of ordinary (non-milestone) events.</param>
    /// <param name="milestoneCount">Current count of milestone events.</param>
    /// <param name="maxOrdinary">Cap for ordinary events.</param>
    /// <param name="maxMilestones">Cap for milestone events.</param>
    /// <param name="force">
    /// When true the caps are enforced unconditionally (used at the save boundary so the
    /// archive never exceeds the caps). When false, an undershoot zone is allowed and no
    /// trim is reported until a cap is overshot by the hysteresis multiplier.
    /// </param>
    /// <returns>True when at least one event needs removal.</returns>
    internal static bool NeedsTrim(int ordinaryCount, int milestoneCount, int maxOrdinary, int maxMilestones, bool force)
    {
        if (!force && ordinaryCount <= Hysteresis(maxOrdinary) && milestoneCount <= Hysteresis(maxMilestones))
            return false;
        if (ordinaryCount <= maxOrdinary && milestoneCount <= maxMilestones) return false;
        return true;
    }

    /// <summary>Number of ordinary events to remove to reach <paramref name="maxOrdinary"/>.</summary>
    internal static int RemoveOrdinary(int ordinaryCount, int maxOrdinary)
        => Math.Max(0, ordinaryCount - maxOrdinary);

    /// <summary>Number of milestone events to remove to reach <paramref name="maxMilestones"/>.</summary>
    internal static int RemoveMilestones(int milestoneCount, int maxMilestones)
        => Math.Max(0, milestoneCount - maxMilestones);

    /// <summary>The count at which a lazy trim is triggered for a cap of <paramref name="cap"/>.</summary>
    internal static int Hysteresis(int cap) => (int)Math.Ceiling(cap * TrimHysteresisMultiplier);
}