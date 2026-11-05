using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;

static class Program
{
    const string Key = "mclsl.080.world-state.v1", Backup = Key + ".backup";
    const string Good = "{\"Version\":1,\"CurrentRun\":{\"RunId\":\"old\"}}";
    static int checks;
    static void Check(bool ok, string name) { checks++; if (!ok) throw new Exception(name); }
    static SaveCustomData Fresh(string primary = null, string backup = null)
    {
        MclslWorldArchiveStore.Clear(); MclslWorldContext.Generation++;
        MapBox.FailSeed = false; World.world = new();
        var data = World.world.map_stats.custom_data = new();
        if (primary != null) data.Values[Key] = primary;
        if (backup != null) data.Values[Backup] = backup;
        MclslWorldRunRepository.ResetWorld(); MclslWorldRunRepository.FailExport = false;
        MclslWorldRunRepository.Imports = 0;
        MySimulatedLongevityRoad.Data.MclslWorldArchiveBundle.Serializations = 0;
        MySimulatedLongevityRoad.Data.MclslWorldArchiveBundle.Deserializations = 0; return data;
    }
    static void Main()
    {
        Fresh(); World.world = null;
        Check(MclslWorldArchiveStore.Load() == MclslWorldArchiveStatus.Waiting, "no world waits");
        Check(MclslWorldArchiveStore.SaveNow() == MclslWorldArchiveSaveResult.NotReady, "no world cannot save");
        Fresh(); World.world.map_stats = null;
        Check(MclslWorldArchiveStore.Load() == MclslWorldArchiveStatus.Waiting, "no stats waits");
        Fresh(); World.world.map_stats.custom_data = null;
        Check(MclslWorldArchiveStore.Load() == MclslWorldArchiveStatus.Missing, "ready world with no custom data is empty");
        var data = Fresh(Good);
        Check(MclslWorldArchiveStore.Load() == MclslWorldArchiveStatus.Primary, "primary load");
        MclslWorldArchiveStore.Load(); Check(MclslWorldRunRepository.Imports == 1, "load only once");
        MclslWorldRunRepository.Current.RunId = "new";
        Check(MclslWorldArchiveStore.SaveNow() == MclslWorldArchiveSaveResult.Saved, "normal save");
        Check(data.Values[Backup] == Good && data.Writes[0] == Backup, "valid primary rotates first");
        Check(MySimulatedLongevityRoad.Data.MclslWorldArchiveBundle.Serializations == 1
            && MySimulatedLongevityRoad.Data.MclslWorldArchiveBundle.Deserializations == 1,
            "normal save serializes once and never reparses primary for backup");
        MclslWorldArchiveStore.Clear(); MclslWorldArchiveStore.Load();
        Check(MclslWorldRunRepository.Current.RunId == "new", "saved state reloads");

        foreach (string bad in new[] { "broken", "null", "{\"Version\":99}", " \t\r\n" })
        {
            data = Fresh(bad, Good);
            Check(MclslWorldArchiveStore.Load() == MclslWorldArchiveStatus.Backup, "backup recovery: " + bad);
            data.FailWrite = Key;
            Check(MclslWorldArchiveStore.SaveNow() == MclslWorldArchiveSaveResult.Failed, "recovery write failure");
            Check(data.Values[Backup] == Good && data.Values[Key] == bad && data.Writes.Count == 0, "backup preserved after failure");
            data.FailWrite = null;
            Check(MclslWorldArchiveStore.SaveNow() == MclslWorldArchiveSaveResult.Saved && data.Values[Backup] == Good, "first repair keeps recovery backup");
            MclslWorldRunRepository.Current.RunId = "later"; MclslWorldArchiveStore.SaveNow();
            Check(data.Values[Backup].Contains("old"), "second save rotates repaired primary");
        }
        data = Fresh(Good); MclslWorldArchiveStore.Load(); MclslWorldArchiveStore.LockWrites();
        Check(MclslWorldArchiveStore.SaveNow() == MclslWorldArchiveSaveResult.Locked && data.Writes.Count == 0,
            "runtime initialization lock blocks direct archive callers after a successful load");
        data = Fresh("bad", "also bad");
        Check(MclslWorldArchiveStore.Load() == MclslWorldArchiveStatus.Faulted, "both corrupt lock");
        Check(MclslWorldArchiveStore.SaveNow() == MclslWorldArchiveSaveResult.Locked && data.Writes.Count == 0, "corrupt state never overwritten");
        data = Fresh(" \t\r\n");
        Check(MclslWorldArchiveStore.Load() == MclslWorldArchiveStatus.Faulted, "whitespace archive is not missing");
        Check(MclslWorldArchiveStore.SaveNow() == MclslWorldArchiveSaveResult.Locked && data.Values[Key] == " \t\r\n"
            && data.Writes.Count == 0, "whitespace archive without a backup stays untouched");
        data = Fresh(Good); data.FailRead = Key;
        Check(MclslWorldArchiveStore.Load() == MclslWorldArchiveStatus.Faulted, "unreadable primary plus missing backup is not empty");
        data.FailRead = null;
        Check(MclslWorldArchiveStore.SaveNow() == MclslWorldArchiveSaveResult.Locked, "fault not retried from save");
        data = Fresh(Good, Good); data.FailRead = Key;
        Check(MclslWorldArchiveStore.Load() == MclslWorldArchiveStatus.Backup, "read exception still attempts backup");
        Fresh(); MapBox.FailSeed = true;
        Check(MclslWorldArchiveStore.Load() == MclslWorldArchiveStatus.Faulted, "outer identity exception locks");
        MapBox.FailSeed = false;
        Check(MclslWorldArchiveStore.SaveNow() == MclslWorldArchiveSaveResult.Locked, "identity fault remains locked until boundary");

        data = Fresh(Good); MclslWorldArchiveStore.Load(); data.FailWrite = Backup;
        Check(MclslWorldArchiveStore.SaveNow() == MclslWorldArchiveSaveResult.Failed && data.Values[Key] == Good && data.Writes.Count == 0, "backup failure prevents primary change");
        data.FailWrite = Key;
        Check(MclslWorldArchiveStore.SaveNow() == MclslWorldArchiveSaveResult.Failed && data.Values[Backup] == Good, "primary failure retains valid backup");
        data = Fresh(); MclslWorldArchiveStore.Load(); data.FailWrite = Key;
        Check(MclslWorldArchiveStore.SaveNow() == MclslWorldArchiveSaveResult.Failed && data.Values.ContainsKey(Backup), "fresh save establishes backup first");
        MclslWorldArchiveStore.Clear(); data.FailWrite = null;
        Check(MclslWorldArchiveStore.Load() == MclslWorldArchiveStatus.Backup, "failed fresh save is recoverable");
        data = Fresh(Good); MclslWorldArchiveStore.Load(); MclslWorldRunRepository.FailExport = true;
        Check(MclslWorldArchiveStore.SaveNow() == MclslWorldArchiveSaveResult.Failed && data.Writes.Count == 0, "export failure has no writes");
        Check(MclslPerformanceProbe.Open == 0, "all profiling scopes close on failure");
        data = Fresh(Good); MclslWorldArchiveStore.Load();
        data.Values[Key] = Good.Replace("old", "other"); MclslWorldContext.Generation++;
        MclslWorldArchiveStore.Load();
        Check(MclslWorldRunRepository.Current.RunId == "other", "same map and seed rebind on generation");
        MclslWorldArchiveStore.Clear();
        Check(!MclslWorldArchiveStore.CanSave, "clear revokes write permission");
        var initialization = new MclslWorldInitialization();
        initialization.Complete();
        Check(!initialization.IsReady && initialization.NeedsAttempt, "waiting cannot complete before begin");
        Check(initialization.TryBegin() && !initialization.IsReady && !initialization.TryBegin(), "initializing is exclusive and not ready");
        initialization.Fail(); initialization.Complete();
        Check(!initialization.IsReady && !initialization.NeedsAttempt && !initialization.TryBegin(), "partial initialization failure cannot replay");
        initialization.Reset();
        Check(initialization.TryBegin(), "new entry permits retry"); initialization.Complete();
        Check(initialization.IsReady && !initialization.TryBegin(), "successful initialization is ready exactly once");
        initialization.Reset(); Check(!initialization.IsReady, "clear revokes initialization");
        Console.WriteLine("PASS archive persistence: " + checks + " checks");
    }
}
