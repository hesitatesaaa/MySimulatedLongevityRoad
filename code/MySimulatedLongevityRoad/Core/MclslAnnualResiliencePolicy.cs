using System;
using System.Collections.Generic;
using System.Linq;

namespace MySimulatedLongevityRoad.Core;

internal static class MclslAnnualResiliencePolicy
{
    internal const int MaxSafeAttempts = 3;
    internal const int RetryFrames = 30;

    internal static bool ShouldRetry(int attempts, bool sideEffectsStarted)
        => !sideEffectsStarted && attempts < MaxSafeAttempts;

    internal static int RetryAtFrame(int currentFrame)
        => currentFrame > int.MaxValue - RetryFrames ? int.MaxValue : currentFrame + RetryFrames;
}

internal enum MclslRecoverySource : byte
{
    Primary,
    Backup,
    Locked
}

internal static class MclslPrimaryBackupRecovery
{
    internal static MclslRecoverySource Select<T>(string primary, string backup,
        Func<string, T?> parse, out T? value) where T : class
    {
        value = parse(primary);
        if (value != null) return MclslRecoverySource.Primary;
        value = string.IsNullOrWhiteSpace(backup) ? null : parse(backup);
        return value == null ? MclslRecoverySource.Locked : MclslRecoverySource.Backup;
    }
}

internal static class MclslAnnualTargetCursor
{
    internal static List<string> Create(IEnumerable<string> targetIds)
        => targetIds.Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();

    internal static void Complete(List<string> pendingIds, string targetId)
    {
        if (pendingIds.Count == 0 || pendingIds[0] != targetId)
            throw new InvalidOperationException("年度目标游标已改变");
        pendingIds.RemoveAt(0);
    }
}
