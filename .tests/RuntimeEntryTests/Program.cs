using System;
using System.Linq;
using MySimulatedLongevityRoad.Core;
static class Program
{
    static int checks;
    static void Check(bool ok, string name) { checks++; if (!ok) throw new Exception(name); }
    static void Fresh(string failure = null)
    {
        Boundary.FailAt = null; MclslRuntime.ClearWorldState();
        World.world = new(); MclslWorldArchiveStore.CanSave = true; MclslRuntimeSettings.CoreEnabled = true;
        Boundary.Calls.Clear(); Boundary.FailAt = failure;
    }
    static void Tick() { UnityEngine.Time.frameCount++; MclslRuntime.Tick(); }
    static bool Saved() => Boundary.Calls.Contains("MclslWorldArchiveStore.SaveNow");
    static void Main()
    {
        // Fail each required boundary independently, including binding and the
        // common post-load work, then exercise the real Tick and save callback.
        foreach (string fail in new[] { "MclslReincarnationProfileStore.EnsureLoaded", "MclslWorldContext.Bind", "MclslWorldRunRepository.EnsureCurrentRun", "MclslNativeHistoryBridge.EnsureRegistered", "MclslNativeKillStatisticsSystem.OnWorldLoaded", "MclslHuanzhenSystem.OnWorldLoaded", "MclslCareerCombat.Load", "MclslTimelineSystem.TickAnnual", "MclslWorldRunRepository.EnsureAnnualWorldState", "MclslRuntimeCadence.InitializeAfterLoad", "MclslWorldSoulSystem.OnWorldLoaded", "MclslBeastWorld.Load", "MclslBeastLoot.Load", "MclslBeastTalentSystem.Load", "MclslSpellCasting.Load", "MclslMapMarkerVisualSystem.OnWorldLoaded", "MclslScheduler.ScheduleAnnualWorld", "MclslFamilySystem.InitializeAfterLoad", "MclslMaobaoArchiveManager.OnWorldLoaded", "MclslAnnualBackpressure.InitializeAfterLoad" })
        {
            Fresh(fail); MclslRuntime.OnWorldLoaded(); Boundary.FailAt = null;
            Tick(); Tick(); MclslRuntime.PrepareForSave();
            Check(Boundary.Calls.Count(x => x == fail) == 1, fail + " not replayed after failure");
            Check(!Saved() && !Boundary.Calls.Contains("MclslScheduler.SavePendingAnnualStates") && !Boundary.Calls.Contains("MclslCareerCombat.Tick"), fail + " blocks writes and gameplay");
            Check(MclslWorldArchiveStore.WriteLocked, fail + " locks direct archive save callers too");
            MclslRuntimeSettings.CoreEnabled = false; Tick(); MclslRuntime.PrepareForSave(); Check(!Saved(), fail + " disabled core cannot bypass fault");
        }
        Fresh(); World.world = null; MclslRuntime.OnWorldLoaded(); MclslRuntime.PrepareForSave();
        Check(!Saved() && !Boundary.Calls.Contains("MclslWorldContext.Bind"), "absent world waits without binding or saving");
        World.world = new(); Tick(); Check(Boundary.Calls.Contains("MclslFamilySystem.InitializeAfterLoad"), "later ready world finishes common entry");
        MclslRuntime.PrepareForSave(); Check(Saved(), "successful delayed initialization permits save");
        Fresh(); MclslWorldArchiveStore.Waiting = true; MclslRuntime.OnWorldLoaded(); Tick(); MclslRuntime.PrepareForSave();
        Check(!Saved() && !Boundary.Calls.Contains("MclslCareerCombat.Load"), "waiting archive gates modules and saves");
        MclslWorldArchiveStore.Waiting = false; Tick(); Tick();
        Check(Boundary.Calls.Count(x => x == "MclslCareerCombat.Load") == 1 && Boundary.Calls.Count(x => x == "MclslWorldContext.Bind") == 1, "retry waiting load initializes modules once");
        Fresh(); MclslWorldArchiveStore.WriteLocked = true; MclslRuntime.OnWorldLoaded(); MclslWorldArchiveStore.WriteLocked = false; Tick(); MclslRuntime.PrepareForSave();
        Check(!Saved() && !Boundary.Calls.Contains("MclslCareerCombat.Load"), "archive fault is terminal until new entry");
        MclslRuntime.OnWorldLoaded(); MclslRuntime.PrepareForSave(); Check(Saved(), "explicit reload restores permission after repaired fault");
        Fresh(); MclslRuntimeSettings.CoreEnabled = false; MclslRuntime.OnWorldLoaded(); MclslRuntime.PrepareForSave();
        Check(!Saved() && !Boundary.Calls.Contains("MclslWorldArchiveStore.Load"), "disabled startup cannot publish an unloaded repository");
        MclslRuntimeSettings.CoreEnabled = true; Tick();
        Check(Boundary.Calls.Count(x => x == "MclslHuanzhenSystem.OnWorldLoaded") == 1 && Boundary.Calls.Count(x => x == "MclslFamilySystem.InitializeAfterLoad") == 1, "enable core later avoids replaying common entry");
        MclslRuntime.PrepareForSave(); Check(Saved(), "enable core later can save after load");
        Console.WriteLine("PASS runtime entry and save gates: " + checks + " checks");
    }
}
