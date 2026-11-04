using System;

namespace MySimulatedLongevityRoad.Core;

// Pure, engine-free rules that keep the annual cursors anchored to the world clock.
//
// A save or a "还真" anchor load can rewind the world year while the annual batch
// and every actor keep a higher "requested/active" year from the previous, later
// world. If those higher years survive, the annual ready queue can never release
// the actors (it only releases years <= world year) and the backlog metric stays
// permanently high, which pins the annual backpressure at its pause level forever.
//
// These helpers are shared by the runtime and the offline regression tests, so
// they intentionally depend on nothing but System.
internal static class MclslAnnualYearReconcilePolicy
{
    internal readonly struct BatchYears
    {
        internal readonly int LatestRequestedYear;
        internal readonly int LastCompletedYear;
        internal readonly int ActiveYear;
        internal readonly bool ResetWorldStage;

        internal BatchYears(int latest, int completed, int active, bool reset)
        {
            LatestRequestedYear = latest;
            LastCompletedYear = completed;
            ActiveYear = active;
            ResetWorldStage = reset;
        }
    }

    // Batch: clamp every forward-looking cursor to the world year. A world that
    // rewound must not keep planning annual years it has not reached. A completed
    // year equal to the world year is legitimate; a world-stage cursor past it is not.
    internal static BatchYears ClampBatch(int latestRequested, int lastCompleted, int activeYear, int worldYear)
    {
        int latest = Math.Max(0, latestRequested);
        int completed = Math.Max(0, lastCompleted);
        int active = Math.Max(0, activeYear);
        if (worldYear <= 0) return new BatchYears(latest, completed, active, false);

        latest = Math.Min(latest, worldYear);
        completed = Math.Min(completed, worldYear);
        bool reset = active > worldYear;
        return new BatchYears(latest, completed, reset ? 0 : active, reset);
    }

    // Per-actor restore: an actor whose active year is beyond the world clock has
    // already caught up to the (rewound) world and must not be re-queued for a
    // future year. Returns false when the pending state must be dropped; otherwise
    // yields the normalized years.
    internal static bool TryRestorePending(int activeYear, int latestYear, int lastCompletedYear, int worldYear,
        out int normalizedActive, out int normalizedLatest, out int normalizedCompleted)
    {
        normalizedActive = Math.Max(0, activeYear);
        normalizedLatest = Math.Max(0, latestYear);
        normalizedCompleted = Math.Max(0, lastCompletedYear);

        if (worldYear > 0)
        {
            normalizedLatest = Math.Min(normalizedLatest, worldYear);
            if (normalizedActive > worldYear) return false; // already caught up to the rewound world
        }

        if (normalizedActive <= 0 || normalizedLatest < normalizedActive || normalizedActive <= normalizedCompleted)
            return false;
        return true;
    }

    // Backpressure lag: measured against the world clock only. Stale "requested"
    // years from a previous, later world must never inflate it, otherwise a world
    // that is actually caught up reports an enormous backlog and stays paused.
    internal static int LagYears(int worldYear, int lastCompletedYear, int activeYear, int latestRequestedYear)
    {
        int world = Math.Max(0, worldYear);
        int completed = Math.Max(0, lastCompletedYear);
        // A brand-new architecture run has no completed annual cursor yet.
        if (completed == 0 && activeYear <= 0 && latestRequestedYear <= 0)
            completed = Math.Max(0, world - 1);
        int frontier = world > 0 ? world : Math.Max(0, latestRequestedYear);
        return Math.Max(0, frontier - completed);
    }
}
