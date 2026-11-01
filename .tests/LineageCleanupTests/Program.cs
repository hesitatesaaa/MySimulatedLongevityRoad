// Repeatable offline checks for the lineage native-history toggle classification
// and the persistent-cleanup policy. Compiles the real production types from
// code/Data, so a regression in either helper fails here.
// Run: dotnet run --project build/verification/LineageCleanupTests -c Release
using System;
using MySimulatedLongevityRoad.Data;

namespace LineageCleanupTests
{
    internal static class Program
    {
        private static int _failures;

        private static void Check(bool condition, string name)
        {
            if (condition) Console.WriteLine("PASS " + name);
            else { Console.WriteLine("FAIL " + name); _failures++; }
        }

        private static int Main()
        {
            CheckLineageClassification();
            CheckMirrorGate();
            CheckCleanupPolicy();
            Console.WriteLine(_failures == 0 ? "ALL TESTS PASSED" : (_failures + " TEST(S) FAILED"));
            return _failures == 0 ? 0 : 1;
        }

        private static void CheckLineageClassification()
        {
            string[] controlled =
            {
                "sect_lifecycle_peak", "sect_lifecycle_fall", "sect_lifecycle_relic",
                "ancient_technique_recorded", "ancient_technique_revived", "ancient_technique_lost", "ancient_technique_flourish",
                "newlaw_technique_recorded", "newlaw_technique_revived", "newlaw_technique_lost", "newlaw_technique_flourish",
                "ancient_lineage_flourish", "ancient_lineage_remembered", "ancient_famous_technique",
                "ancient_master_teaching", "ancient_technique_deduction", "ancient_found_method", "ancient_new_method_branch",
                "epoch_ancient_sects_retired", "epoch_ancient_law_end", "epoch_chuanfa_dao", "epoch_new_law_spread",
                "epoch_law_conflict", "epoch_ancient_remnants", "epoch_new_law_era", "epoch_new_law_pioneers"
            };
            foreach (string type in controlled)
                Check(MclslEventCatalog.IsLineageNativeHistoryEvent(type), "controlled by lineage toggle: " + type);

            string[] independent =
            {
                "epoch_wanxian_alliance_founded", "epoch_five_elders_founded", "epoch_mortal_miasma_truth",
                "epoch_unrelated_future_event",
                "world_calamity_black_tide", "world_calamity_spirit_tide", "world_calamity_white_mist",
                "reincarnation_unbroken", "inverse_truth_begin", "inverse_truth_reversed", "longevity_achieved",
                "beast_tide", "cultivator_death", "ancient_law_breakthrough", "ancient_ruin_born", "ruin_born",
                "faction_mission", "material_discovery", "ancient_convert_new_law"
            };
            foreach (string type in independent)
                Check(!MclslEventCatalog.IsLineageNativeHistoryEvent(type), "independent of lineage toggle: " + type);
        }

        private static void CheckMirrorGate()
        {
            // Toggle OFF: controlled high-importance events are blocked; non-道统 events still mirror.
            Check(!MclslEventCatalog.ShouldMirrorToNativeHistory("epoch_ancient_law_end", 5, false),
                "toggle off blocks controlled epoch event");
            Check(!MclslEventCatalog.ShouldMirrorToNativeHistory("sect_lifecycle_peak", 4, false),
                "toggle off blocks controlled sect lifecycle event");
            Check(MclslEventCatalog.ShouldMirrorToNativeHistory("world_calamity_black_tide", 4, false),
                "toggle off keeps calamity event in native history");
            Check(MclslEventCatalog.ShouldMirrorToNativeHistory("epoch_wanxian_alliance_founded", 5, false),
                "toggle off keeps alliance epoch event in native history");
            Check(!MclslEventCatalog.ShouldMirrorToNativeHistory("ancient_master_teaching", 3, false),
                "sub-threshold event never mirrors regardless of toggle");

            // Toggle ON: controlled high-importance events mirror; sub-threshold ones do not.
            Check(MclslEventCatalog.ShouldMirrorToNativeHistory("epoch_ancient_law_end", 5, true),
                "toggle on mirrors controlled epoch event");
            Check(MclslEventCatalog.ShouldMirrorToNativeHistory("ancient_new_method_branch", 4, true),
                "toggle on mirrors new-method branch");
            Check(!MclslEventCatalog.ShouldMirrorToNativeHistory("ancient_technique_deduction", 2, true),
                "toggle on still respects importance threshold");
            Check(!MclslEventCatalog.ShouldMirrorToNativeHistory("sect_lifecycle_peak", 3, true),
                "importance rule applies before toggle");
        }

        private static void CheckCleanupPolicy()
        {
            Check(!MclslCleanupPolicy.ResolvePersistentCleanup(null), "missing marker resolves to off");
            Check(!MclslCleanupPolicy.ResolvePersistentCleanup(false), "explicit false stays off");
            Check(MclslCleanupPolicy.ResolvePersistentCleanup(true), "explicit true is on");

            Check(MclslCleanupPolicy.ShouldRemoveExtinctFamily(true, false, true, true, false, false),
                "new world removes fully settled extinct family");
            Check(!MclslCleanupPolicy.ShouldRemoveExtinctFamily(false, false, true, true, false, false),
                "old archive without marker never removes");
            Check(MclslCleanupPolicy.ShouldRemoveExtinctFamily(false, true, true, true, false, false),
                "legacy RemoveAfterLiquidation still removes when cleanup disabled");
            Check(!MclslCleanupPolicy.ShouldRemoveExtinctFamily(true, false, false, true, false, false),
                "non-extinct family is kept");
            Check(!MclslCleanupPolicy.ShouldRemoveExtinctFamily(true, false, true, false, false, false),
                "unliquidated estate is kept");
            Check(!MclslCleanupPolicy.ShouldRemoveExtinctFamily(true, false, true, true, true, false),
                "family with living or missing member is kept");
            Check(!MclslCleanupPolicy.ShouldRemoveExtinctFamily(true, false, true, true, false, true),
                "family with unsettled death estate is kept");
        }
    }
}
