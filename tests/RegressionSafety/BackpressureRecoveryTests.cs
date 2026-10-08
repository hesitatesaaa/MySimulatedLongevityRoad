using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;

internal static class BackpressureRecoveryTests
{
    private static void Check(bool value, string reason)
    {
        if (!value) throw new Exception(reason);
    }

    internal static void Run()
    {
        MclslDeveloperBridge.IsAvailable = false;
        MclslAnnouncementSystem.Count = 0;
        MclslWorldRunRepository.Current = new();
        MclslWorldRunRepository.Current.AnnualBatch.LastCompletedYear = 10;
        MclslWorldRunRepository.Current.AnnualBatch.LatestRequestedYear = 18;
        MclslRuntime.Year = 18;
        UnityEngine.Time.unscaledTime = 0;
        Config.paused = false;
        UnityEngine.Time.timeScale = 1f;
        MclslAnnualBackpressure.SetPatchAvailable(true);
        MclslAnnualBackpressure.InitializeAfterLoad();
        MclslAnnualBackpressure.TickStatus();
        float elapsed = 1f;
        Check(!MclslAnnualBackpressure.Apply(ref elapsed) && MclslAnnualBackpressure.AppliedFactor == 0f,
            "an eight-year backlog must stop native simulation");
        Check(!MclslAnnualBackpressure.ShowStatus && MclslAnnouncementSystem.Count == 0,
            "player package must hide automatic slowdown UI and notices");

        UnityEngine.Time.unscaledTime = 5f;
        MclslAnnualBackpressure.RecordStep();
        MclslAnnualBackpressure.TickStatus();
        Check(MclslAnnualBackpressure.AppliedFactor == 0f,
            "actual annual progress must keep protected hard pause active");
        UnityEngine.Time.unscaledTime = 16f;
        MclslAnnualBackpressure.TickStatus();
        elapsed = 1f;
        Check(MclslAnnualBackpressure.Apply(ref elapsed) && elapsed == 0.25f,
            "ten seconds without progress must fall back to quarter speed");
        Check(!Config.paused && UnityEngine.Time.timeScale == 1f,
            "fallback must not alter the player's pause or Unity time scale");

        MclslWorldRunRepository.Current.AnnualBatch.LastCompletedYear = 18;
        for (int time = 18; time <= 30; time += 2)
        {
            UnityEngine.Time.unscaledTime = time;
            MclslAnnualBackpressure.TickStatus();
        }
        Check(MclslAnnualBackpressure.AppliedFactor == 1f,
            "completed backlog must restore native speed after hysteresis");

        MclslAnnualBackpressure.Clear();
        MclslWorldRunRepository.Current.AnnualBatch.LastCompletedYear = 10;
        MclslWorldRunRepository.Current.AnnualBatch.WorldBlocked = true;
        UnityEngine.Time.unscaledTime = 30f;
        MclslAnnualBackpressure.InitializeAfterLoad();
        MclslAnnualBackpressure.TickStatus();
        Check(MclslAnnualBackpressure.AppliedFactor == 0.25f,
            "blocked annual work must fail open immediately");

        MclslDeveloperBridge.IsAvailable = true;
        MclslAnnualBackpressure.Clear();
        MclslWorldRunRepository.Current.AnnualBatch.WorldBlocked = false;
        UnityEngine.Time.unscaledTime = 40f;
        MclslAnnualBackpressure.InitializeAfterLoad();
        MclslAnnualBackpressure.TickStatus();
        Check(MclslAnnualBackpressure.ShowStatus && MclslAnnouncementSystem.Count > 0,
            "developer package must retain backlog UI and notices");
        MclslAnnualBackpressure.Clear();
    }
}
