using System.Diagnostics;
using MySimulatedLongevityRoad.Core;

internal static class AnnualThroughputTests
{
    private static (int CompletedYears, int RemainingSteps, int TotalSteps, int MaxStepsPerFrame) Simulate(int actorLimit)
    {
        const int actors = 930;
        const int stepsPerActor = 28;
        const int frames = 1800;
        const int nativeFramesPerYear = 150;
        long clock = 0;
        long stepTicks = Math.Max(1, Stopwatch.Frequency / 100000);
        MclslAnnualFrameBudget.Timestamp = () => clock;
        int pending = 0, completedYears = 0, totalSteps = 0, maxSteps = 0;
        for (int frame = 1; frame <= frames; frame++)
        {
            if (frame % nativeFramesPerYear == 1) pending += actors * stepsPerActor;
            MclslAnnualFrameBudget.Begin(2.5);
            int steps = 0;
            while (pending > 0 && steps < actorLimit && MclslAnnualFrameBudget.TryConsumeOperation())
            {
                pending--;
                steps++;
                totalSteps++;
                clock += stepTicks;
                if (pending % (actors * stepsPerActor) == 0) completedYears++;
            }
            maxSteps = Math.Max(maxSteps, steps);
            MclslAnnualFrameBudget.End();
        }
        return (completedYears, pending, totalSteps, maxSteps);
    }

    internal static void Run()
    {
        Func<long> original = MclslAnnualFrameBudget.Timestamp;
        try
        {
            var oldLimit = Simulate(80);
            var deadlineOnly = Simulate(int.MaxValue);
            if (deadlineOnly.CompletedYears <= oldLimit.CompletedYears
                || deadlineOnly.RemainingSteps >= oldLimit.RemainingSteps
                || deadlineOnly.MaxStepsPerFrame <= 80)
                throw new Exception("removing the 80-step cap must increase annual throughput within the same 2.5 ms budget");
            const int simulatedSeconds = 30; // 1800 frames at 60 FPS, 12 years requested.
            Console.WriteLine("Annual synthetic, 930 actors x 28 steps/year, 60 FPS, 2.5 ms/frame:");
            Console.WriteLine($"  80-step cap: {oldLimit.TotalSteps / simulatedSeconds} steps/s, "
                + $"{oldLimit.CompletedYears}/12 years completed, "
                + $"{oldLimit.RemainingSteps} pending steps, "
                + $"{oldLimit.MaxStepsPerFrame * 0.01:F2} ms max annual work/frame.");
            Console.WriteLine($"  Deadline only: {deadlineOnly.TotalSteps / simulatedSeconds} steps/s, "
                + $"{deadlineOnly.CompletedYears}/12 years completed, "
                + $"{deadlineOnly.RemainingSteps} pending steps, "
                + $"{deadlineOnly.MaxStepsPerFrame * 0.01:F2} ms max annual work/frame.");
        }
        finally { MclslAnnualFrameBudget.Timestamp = original; }
    }
}
