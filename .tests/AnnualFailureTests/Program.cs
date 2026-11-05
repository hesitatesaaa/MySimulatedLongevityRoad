using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Core;
static class Program
{
    static int checks;
    static void Check(bool ok, string name) { checks++; if (!ok) throw new Exception(name); }
    static void Main()
    {
        var batch = MclslWorldRunRepository.Current.AnnualBatch;
        MclslWorldRunRepository.BlockAnnualSettlement(12, null, null, null, 1, new string('x', 350));
        Check(batch.WorldBlocked && MclslWorldArchiveStore.Revision == 1, "block and revision committed");
        Check(batch.FailureRecords.Count == 1 && batch.FailureRecords[0].Error.Length == 300, "bounded error record");
        Check(MclslWorldRunRepository.Events == 1 && MclslDiagnostics.Errors == 1 && MclslDiagnostics.Fatals == 1, "failure is visible");
        MclslWorldRunRepository.BlockAnnualSettlement(12, null, null, null, 2, "retry");
        Check(batch.FailureRecords.Count == 1 && batch.FailureRecords[0].Attempts == 2 && batch.FailureRecords[0].Error == "retry", "normalized duplicate updates same failure");
        MclslWorldRunRepository.BlockWorldAnnualSettlement(12, 4, "Cave", "failure");
        MclslWorldRunRepository.BlockWorldAnnualSettlement(12, 4, "Cave", "again");
        Check(batch.WorldFailureStage == 4 && batch.WorldFailureCount == 2 && batch.WorldBlocked, "world stage and attempt count owned together");
        Check(batch.FailureRecords.Find(x => x.Step == "Cave").Attempts == 2, "world failure updates durable attempts");
        for (int i = 0; i < 600; i++) MclslWorldRunRepository.RecordAnnualFailure(i, "actor", i.ToString(), "step", 1, "test", "");
        Check(batch.FailureRecords.Count == 512 && batch.FailureRecords[0].Year == 88, "failure history is capped");
        batch.WorldBlocked = false; MclslDiagnostics.Fail = true; long before = MclslWorldArchiveStore.Revision;
        try { MclslWorldRunRepository.BlockAnnualSettlement(999, "actor", "1", "step", 1, "fail"); }
        catch (InvalidOperationException) { }
        Check(batch.WorldBlocked && MclslWorldArchiveStore.Revision == before + 1 && batch.FailureRecords[^1].Year == 999, "logging failure cannot lose block or dirty state");
        Check(MclslDiagnostics.Fatals == 5, "terminal failure remains visible even if record logging throws");
        Console.WriteLine("PASS annual failure boundary: " + checks + " checks");
    }
}
namespace MySimulatedLongevityRoad.Data
{
    // Data shapes only. The failure operations are compiled from production.
    class MclslAnnualFailureRecord { public int Year, Attempts; public string Scope, TargetId, Step, Result, Error; }
    class MclslAnnualBatchState { public bool WorldBlocked; public byte WorldFailureStage; public int WorldFailureCount; public List<MclslAnnualFailureRecord> FailureRecords; }
    class Run { public MclslAnnualBatchState AnnualBatch = new(); }
}
namespace MySimulatedLongevityRoad.Core
{
    static class MclslDiagnostics { public static int Errors, Fatals; public static bool Fail; public static void Fatal(string k, string m) => Fatals++; public static void Alert(string k, string m) { } public static void Probe(string k, string m) { } public static void Error(string k, string m) { Errors++; if (Fail) throw new InvalidOperationException("log injected"); } }
}
namespace MySimulatedLongevityRoad.Systems
{
    static class MclslWorldArchiveStore { public static long Revision; public static void MarkDirty() => Revision++; }
    static partial class MclslWorldRunRepository
    {
        internal static Data.Run Current = new(); internal static int Events;
        static void AddEvent(int y, string type, string title, string body) => Events++;
    }
}
