public class WorldBehaviourLibrary : AssetLibrary<WorldBehaviourAsset>
{
	public override void init()
	{
		base.init();
		add(new WorldBehaviourAsset
		{
			id = "grin_reaper",
			interval = 0.01f,
			interval_random = 0.01f,
			stop_when_world_on_pause = false,
			action = WorldBehaviourActions.updateGrinReaper,
			action_world_clear = WorldBehaviourActions.clearGrinReaper
		});
		add(new WorldBehaviourAsset
		{
			id = "zones_meta_data_visualizer",
			interval = 0.4f,
			interval_random = 0f,
			stop_when_world_on_pause = false,
			action = ZoneMetaDataVisualizer.updateMetaZones
		});
		add(new WorldBehaviourAsset
		{
			id = "debug_highlight",
			interval = 0.1f,
			interval_random = 0f,
			stop_when_world_on_pause = false,
			action = DebugHighlight.updateDebugHighlights
		});
		add(new WorldBehaviourAsset
		{
			id = "disasters",
			interval = 80f,
			interval_random = 0f,
			action = WorldBehaviourActions.updateDisasters
		});
		add(new WorldBehaviourAsset
		{
			id = "migrants",
			interval = 80f,
			interval_random = 0f,
			action = WorldBehaviourActions.updateMigrants
		});
		add(new WorldBehaviourAsset
		{
			id = "decay_roads",
			interval = 1f,
			interval_random = 1f,
			action = WorldBehaviourActions.updateRoadDecay
		});
		add(new WorldBehaviourAsset
		{
			id = "decay_farms",
			interval = 1f,
			interval_random = 1f,
			action = WorldBehaviourActions.updateFarmDecay,
			action_world_clear = WorldBehaviourActions.clearFarmDecay
		});
		add(new WorldBehaviourAsset
		{
			id = "spawn_random_vegetation",
			interval = 10f,
			interval_random = 10f,
			action = WorldBehaviourActions.spawnRandomVegetation
		});
		add(new WorldBehaviourAsset
		{
			id = "spawn_minerals",
			interval = 2f,
			interval_random = 3f,
			action = WorldBehaviourActions.growMinerals
		});
		add(new WorldBehaviourAsset
		{
			id = "building_sparks",
			interval = 0.1f,
			interval_random = 1f,
			enabled_on_minimap = false,
			action = WorldBehaviourActions.buildingSparks
		});
		add(new WorldBehaviourAsset
		{
			id = "unit_spawn",
			interval = 24.5f,
			interval_random = 30f,
			action = WorldBehaviourActions.updateUnitSpawn
		});
		add(new WorldBehaviourAsset
		{
			id = "ruins_rat_spawn",
			interval = 30.5f,
			interval_random = 3f,
			action = WorldBehaviourActions.updateRuinRatSpawn
		});
		add(new WorldBehaviourAsset
		{
			id = "creep_decay",
			interval = 1f,
			interval_random = 0.3f,
			action = WorldBehaviourActionCreepDecay.checkCreep,
			action_world_clear = WorldBehaviourActionCreepDecay.clear
		});
		add(new WorldBehaviourAsset
		{
			id = "ocean",
			interval = 0.2f,
			interval_random = 0f,
			action = WorldBehaviourOcean.updateOcean,
			action_world_clear = WorldBehaviourOcean.clear
		});
		add(new WorldBehaviourAsset
		{
			id = "tiles_freeze",
			interval = 0.1f,
			interval_random = 0f,
			action = WorldBehaviourTilesTemperatureFreeze.update,
			action_world_clear = WorldBehaviourTilesRunner.clearTilesToCheck
		});
		add(new WorldBehaviourAsset
		{
			id = "tiles_unfreeze",
			interval = 0.01f,
			interval_random = 0f,
			action = WorldBehaviourTilesTemperatureUnFreeze.update,
			action_world_clear = WorldBehaviourTilesRunner.clearTilesToCheck
		});
		add(new WorldBehaviourAsset
		{
			id = "biomes",
			interval = 0.05f,
			interval_random = 0.05f,
			action = WorldBehaviourActionBiomes.update,
			action_world_clear = WorldBehaviourActionBiomes.clear
		});
		add(new WorldBehaviourAsset
		{
			id = "lava",
			interval = 0.05f,
			interval_random = 0.05f,
			action = WorldBehaviourActionLava.update,
			action_world_clear = WorldBehaviourTilesRunner.clearTilesToCheck
		});
		add(new WorldBehaviourAsset
		{
			id = "fire",
			interval = 1f,
			interval_random = 0f,
			action = WorldBehaviourActionFire.updateFire,
			action_world_clear = WorldBehaviourActionFire.clear
		});
		add(new WorldBehaviourAsset
		{
			id = "burned_tiles",
			interval = 3f,
			interval_random = 5f,
			action = WorldBehaviourActionBurnedTiles.updateBurnedTiles,
			action_world_clear = WorldBehaviourActionBurnedTiles.clear
		});
		add(new WorldBehaviourAsset
		{
			id = "erosion",
			interval = 3f,
			interval_random = 1f,
			action = WorldBehaviourActionErosion.updateErosion,
			action_world_clear = WorldBehaviourActionErosion.clear
		});
		add(new WorldBehaviourAsset
		{
			id = "biome_inferno_fires",
			interval = 0.5f,
			interval_random = 1f,
			action = WorldBehaviourActionInferno.updateInfernoFireAction,
			enabled_on_minimap = false
		});
		add(new WorldBehaviourAsset
		{
			id = "biome_inferno_tile_low_animations",
			interval = 0.5f,
			interval_random = 0.5f,
			action = WorldBehaviourActionInferno.updateInfernalLowAnimations,
			enabled_on_minimap = false
		});
		add(new WorldBehaviourAsset
		{
			id = "biome_singularity_tile_animations",
			interval = 0.5f,
			interval_random = 0.5f,
			action = WorldBehaviourActionSingularity.updateSingularityTiles,
			enabled_on_minimap = false
		});
		add(new WorldBehaviourAsset
		{
			id = "creep_biomass",
			interval = 0.1f,
			interval_random = 0.1f,
			action = WorldBehaviourActionCreepBiomass.updateBiomassTiles,
			enabled_on_minimap = false
		});
		add(new WorldBehaviourAsset
		{
			id = "biome_low_swamp_animations",
			interval = 0.1f,
			interval_random = 0.1f,
			action = WorldBehaviourActionSwampAnimation.updateSwampTiles,
			enabled_on_minimap = false
		});
		add(new WorldBehaviourAsset
		{
			id = "unit_temperatures",
			interval = 0.1f,
			interval_random = 0.1f,
			action = WorldBehaviourUnitTemperatures.updateUnitTemperatures,
			action_world_clear = WorldBehaviourUnitTemperatures.clear
		});
		add(new WorldBehaviourAsset
		{
			id = "waves",
			interval = 0.1f,
			interval_random = 0.1f,
			enabled_on_minimap = false,
			action = WorldBehaviourTileEffects.tryToStartTileEffects
		});
		add(new WorldBehaviourAsset
		{
			id = "clouds",
			interval = 1f,
			interval_random = 1f,
			action = WorldBehaviourClouds.spawnRandomCloud
		});
		add(new WorldBehaviourAsset
		{
			id = "achievements_checks",
			interval = 5f,
			action = delegate
			{
				AchievementLibrary.great_plague.check();
				AchievementLibrary.eternal_chaos.checkBySignal();
				AchievementLibrary.minefield.checkBySignal();
				AchievementLibrary.rain_tornado.checkBySignal();
				AchievementLibrary.ancient_war_of_geometry_and_evil.checkBySignal();
			}
		});
	}

	public void createManagers()
	{
		foreach (WorldBehaviourAsset item in list)
		{
			item.manager = new WorldBehaviour(item);
		}
	}
}
