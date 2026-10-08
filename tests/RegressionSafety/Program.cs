using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using Newtonsoft.Json;

static void Check(bool value, string message)
{
    if (!value) throw new Exception(message);
}

EconomyTests.Run();
EconomyTests.RunLongLedgerCheck();
AtomicFileTests.Run();
AnnualThroughputTests.Run();
BackpressureRecoveryTests.Run();
PriorityAnnualWorkTests.Run();
PhysiqueLifespanTests.Run();
var speed = new MclslAnnualSpeedPolicy();
Check(speed.Factor == 1f, "no backlog must retain native speed");
speed.Update(3, 0, true);
Check(speed.Factor == 0.5f, "three-year backlog must halve native speed");
speed.Update(6, 1, true);
Check(speed.Factor == 0.25f, "six-year backlog must quarter native speed");
speed.Update(8, 2, true);
Check(speed.Factor == 0f, "eight-year backlog must stop native simulation");
speed.Update(0, 3, false);
Check(speed.Factor == 1f, "disabling backpressure must restore native speed");
Check(MclslInitialCultivationPolicy.IsPending(10, 9)
    && !MclslInitialCultivationPolicy.IsPending(10, 10),
    "first cultivation must be issued once per entry year");

World.world = new MapBox();
MapBox.current_world_seed_id = 53;
World.world.map_stats.custom_data = new SaveCustomData();
MclslWorldArchiveStore.Clear();
MclslWorldRunRepository.Current = new();
MclslWorldArchiveStore.Load();
MclslWorldRunRepository.Current.RunId = "v053-new-world";
MclslWorldArchiveStore.SaveNow();
const string primary = "mclsl.architecture.v1";
const string backup = "mclsl.architecture.v1.backup";
SaveCustomData data = World.world.map_stats.custom_data;
Check(data.Values.ContainsKey(primary) && data.Values.ContainsKey(backup),
    "ArchitectureV1 must save primary and backup");
MclslWorldArchiveBundle? bundle = JsonConvert.DeserializeObject<MclslWorldArchiveBundle>(data.Values[primary]);
Check(bundle?.CurrentRun?.RunId == "v053-new-world", "new world archive must round-trip");
World.world = new MapBox();
World.world.map_stats.custom_data = new SaveCustomData();
World.world.map_stats.custom_data.set(primary, "{broken-old-save");
MclslWorldArchiveStore.Load();
MclslWorldArchiveStore.SaveNow();
Check(MclslWorldArchiveStore.WriteLocked
    && World.world.map_stats.custom_data.Values[primary] == "{broken-old-save",
    "incompatible or damaged old worlds must not be overwritten");
World.world = new MapBox();
World.world.map_stats.custom_data = new SaveCustomData();
MclslWorldArchiveStore.Load();
Check(!MclslWorldArchiveStore.WriteLocked, "a new world with the same seed must be writable");

Console.WriteLine("0.5.4 regression: economy, longevity, physique multipliers, lifespan transfer, annual recovery, first cultivation and ArchitectureV1 save passed.");
