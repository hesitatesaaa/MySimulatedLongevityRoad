using System.Collections.Generic;
using UnityEngine;

public static class MapAction
{
	private static List<WorldTile> temp_list_tiles_road_path = new List<WorldTile>();

	private static List<WorldTile> temp_list_tiles_road_tiles_to_build = new List<WorldTile>();

	public static void checkAcidTerraform(WorldTile pTile)
	{
		if (pTile.isTemporaryFrozen())
		{
			pTile.unfreeze(99);
		}
		if (pTile.top_type != null && pTile.top_type.wasteland)
		{
			return;
		}
		if (pTile.top_type != null)
		{
			decreaseTile(pTile, pDamage: true);
		}
		else if (pTile.Type.ground)
		{
			if (pTile.isTileRank(TileRank.Low))
			{
				terraformTop(pTile, TopTileLibrary.wasteland_low);
			}
			else if (pTile.isTileRank(TileRank.High))
			{
				terraformTop(pTile, TopTileLibrary.wasteland_high);
			}
			AchievementLibrary.lets_not.check();
		}
	}

	public static void terraformMain(WorldTile pTile, TileType pType, bool pSkipTerraform = false)
	{
		terraformTile(pTile, pType, null, TerraformLibrary.flash, pSkipTerraform);
	}

	public static void terraformTop(WorldTile pTile, TopTileType pTopType, bool pSkipTerraform = false)
	{
		terraformTile(pTile, pTile.main_type, pTopType, TerraformLibrary.flash, pSkipTerraform);
	}

	public static void terraformMain(WorldTile pTile, TileType pType, TerraformOptions pOptions, bool pSkipTerraform = false)
	{
		terraformTile(pTile, pType, null, pOptions, pSkipTerraform);
	}

	public static void terraformTop(WorldTile pTile, TopTileType pTopType, TerraformOptions pOptions, bool pSkipTerraform = false)
	{
		terraformTile(pTile, pTile.main_type, pTopType, pOptions, pSkipTerraform);
	}

	public static void terraformTile(WorldTile pTile, TileType pNewTypeMain, TopTileType pTopType, TerraformOptions pOptions = null, bool pSkipTerraform = false)
	{
		if (pOptions == null)
		{
			pOptions = TerraformLibrary.flash;
		}
		TileTypeBase type = pTile.Type;
		TileTypeBase type2 = pTile.Type;
		if (pOptions.remove_fire)
		{
			pTile.stopFire();
		}
		if (!pSkipTerraform)
		{
			if (pOptions.remove_water && pTile.Type.ocean)
			{
				pNewTypeMain = pTile.Type.decrease_to;
			}
			if (pOptions.remove_top_tile && pTile.top_type != null)
			{
				pNewTypeMain = pTile.Type.decrease_to;
			}
			if (pOptions.remove_roads && pTile.Type.road)
			{
				pNewTypeMain = pTile.Type.decrease_to;
			}
			if (pOptions.remove_frozen && pTile.isTemporaryFrozen())
			{
				pTile.unfreeze(99);
			}
			if (pNewTypeMain != null)
			{
				pTile.setTileTypes(pNewTypeMain, pTopType);
			}
			else
			{
				pTile.setTopTileType(pTopType);
			}
		}
		if (type2.can_be_farm != pTile.Type.can_be_farm && !pTile.zone.hasCity())
		{
			World.world.city_zone_helper.city_place_finder.setDirty();
		}
		if ((pTile.burned_stages > 0 && !pTile.Type.can_be_set_on_fire) || pOptions.remove_burned)
		{
			pTile.removeBurn();
		}
		World.world.resetRedrawTimer();
		if (pOptions.remove_borders)
		{
			World.world.checkCityZone(pTile);
		}
		if (pOptions.flash)
		{
			World.world.flash_effects.flashPixel(pTile, 20);
		}
		if (pTile.hasBuilding() && !pTile.building.isRuin() && !pTile.building.asset.isOverlaysBiomeTags(pTile.Type))
		{
			if (pTile.building.asset.has_ruins_graphics)
			{
				pTile.building.startMakingRuins();
			}
			else
			{
				pTile.building.startDestroyBuilding();
			}
		}
		if (pOptions.make_ruins && pTile.hasBuilding())
		{
			Building building = pTile.building;
			if (building.asset.has_ruins_graphics)
			{
				building.startMakingRuins();
			}
			else
			{
				building.startDestroyBuilding();
			}
			if (!building.asset.can_be_placed_on_blocks && pTile.Type.rocks)
			{
				building.startDestroyBuilding();
			}
			if (!building.asset.can_be_placed_on_liquid && pTile.Type.liquid)
			{
				building.startDestroyBuilding();
			}
		}
		if (pOptions.destroy_buildings && pTile.hasBuilding())
		{
			bool flag = false;
			if (pOptions.ignore_kingdoms != null)
			{
				flag = true;
				for (int i = 0; i < pOptions.ignore_kingdoms.Length; i++)
				{
					if (!(pOptions.ignore_kingdoms[i] != pTile.building.kingdom?.name))
					{
						flag = false;
						break;
					}
				}
			}
			else if (pOptions.destroy_only == null)
			{
				flag = pOptions.ignore_buildings == null || !pOptions.ignore_buildings.Contains(pTile.building.asset.id);
			}
			else
			{
				flag = false;
				for (int j = 0; j < pOptions.destroy_only.Count; j++)
				{
					if (!(pOptions.destroy_only[j] != pTile.building.asset.group))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				pTile.building.startDestroyBuilding();
			}
		}
		checkTileState(pTile, type);
	}

	public static void checkTileState(WorldTile pTile, TileTypeBase pOldType, bool pForceMapChunk = false)
	{
		if ((pOldType.layer_type != pTile.Type.layer_type) | pForceMapChunk)
		{
			World.world.map_chunk_manager.setDirty(pTile.chunk);
			MapChunk[] neighbours_all = pTile.chunk.neighbours_all;
			foreach (MapChunk pChunk in neighbours_all)
			{
				World.world.map_chunk_manager.setDirty(pChunk, pRegions: false);
			}
		}
		if (pTile.Type.layer_type != TileLayerType.Ground)
		{
			World.world.checkCityZone(pTile);
		}
		if (pTile.hasBuilding() && !pTile.building.asset.can_be_placed_on_liquid && pTile.Type.ocean)
		{
			pTile.building.startDestroyBuilding();
		}
	}

	public static void setOcean(WorldTile pTile)
	{
		if (pTile.Type.fill_to_ocean != null)
		{
			TileType pType = AssetManager.tiles.get(pTile.Type.fill_to_ocean);
			if (pTile.Type.water_fill_sound != string.Empty)
			{
				MusicBox.playSound(pTile.Type.water_fill_sound, pTile);
			}
			terraformMain(pTile, pType, TerraformLibrary.water_fill);
		}
	}

	public static void decreaseTile(WorldTile pTile, bool pDamage, TerraformOptions pTerraformOption)
	{
		if (checkTileDamageGaiaCovenant(pTile, pDamage))
		{
			if (pTile.isTemporaryFrozen())
			{
				pTile.unfreeze(100);
			}
			else if (pTile.top_type != null)
			{
				terraformTile(pTile, pTile.main_type, null, pTerraformOption);
			}
			else if (pTile.Type.decrease_to != null)
			{
				terraformMain(pTile, pTile.Type.decrease_to, pTerraformOption);
			}
		}
	}

	public static bool checkTileDamageGaiaCovenant(WorldTile pTile, bool pDamage)
	{
		bool flag = pTile.Type.life || pTile.Type.explodable;
		if (pDamage && WorldLawLibrary.world_law_gaias_covenant.isEnabled() && !flag)
		{
			return false;
		}
		return true;
	}

	public static void decreaseTile(WorldTile pTile, bool pDamage, string pTerraformOption = "flash")
	{
		if (checkTileDamageGaiaCovenant(pTile, pDamage))
		{
			decreaseTile(pTile, pDamage, AssetManager.terraform.get(pTerraformOption));
		}
	}

	public static void increaseTile(WorldTile pTile, bool pDamage, string pTerraformOption = "flash")
	{
		if (checkTileDamageGaiaCovenant(pTile, pDamage))
		{
			if (pTile.top_type != null)
			{
				terraformTile(pTile, pTile.main_type, null, AssetManager.terraform.get(pTerraformOption));
			}
			else if (pTile.Type.increase_to != null)
			{
				terraformMain(pTile, pTile.Type.increase_to, AssetManager.terraform.get(pTerraformOption));
			}
		}
	}

	public static void removeLiquid(WorldTile pTile)
	{
		if (pTile.Type.liquid)
		{
			decreaseTile(pTile, pDamage: false);
		}
	}

	public static void growGreens(WorldTile pTile, TopTileType pTopType)
	{
		terraformTop(pTile, pTopType, TerraformLibrary.flash);
	}

	public static void removeGreens(WorldTile pTile)
	{
		decreaseTile(pTile, pDamage: false);
	}

	private static void applyLightningEffect(WorldTile pTile)
	{
		if (pTile.Type.lava && pTile.heat > 20)
		{
			decreaseTile(pTile, pDamage: true);
			if (Randy.randomChance(0.9f))
			{
				int num = pTile.heat / 10;
				World.world.drop_manager.spawnParabolicDrop(pTile, "lava", 0f, 0.15f, 33f + (float)(num * 2), 1f, 40f + (float)num);
			}
			AchievementLibrary.lava_strike.check();
		}
		if (pTile.Type.layer_type == TileLayerType.Ocean)
		{
			removeLiquid(pTile);
			if (Randy.randomChance(0.8f))
			{
				World.world.drop_manager.spawnParabolicDrop(pTile, "rain", 0f, 1f, 66f, 1f, 45f);
			}
		}
		if (pTile.hasBuilding() && pTile.building.asset.spawn_drops)
		{
			if (!pTile.building.data.hasFlag("stop_spawn_drops"))
			{
				pTile.building.spawnBurstSpecial(10);
			}
			if (pTile.building.data.hasFlag("stop_spawn_drops"))
			{
				pTile.building.data.removeFlag("stop_spawn_drops");
			}
			else
			{
				pTile.building.data.addFlag("stop_spawn_drops");
			}
		}
	}

	public static void applyTileDamage(WorldTile pTargetTile, float pRad, TerraformOptions pOptions)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		World.world.resetRedrawTimer();
		BrushData brushData = Brush.get((int)pRad);
		World.world.conway_layer.checkKillRange(pTargetTile.pos, brushData.size);
		if (pOptions.remove_tornado)
		{
			tryRemoveTornadoFromTile(pTargetTile);
		}
		WorldBehaviourTileEffects.checkTileForEffectKill(pTargetTile, brushData.size);
		for (int i = 0; i < brushData.pos.Length; i++)
		{
			BrushPixelData brushPixelData = brushData.pos[i];
			Vector2Int pos = pTargetTile.pos;
			int num = ((Vector2Int)(ref pos)).x + brushPixelData.x;
			pos = pTargetTile.pos;
			int num2 = ((Vector2Int)(ref pos)).y + brushPixelData.y;
			if (num < 0 || num >= MapBox.width || num2 < 0 || num2 >= MapBox.height)
			{
				continue;
			}
			WorldTile tileSimple = World.world.GetTileSimple(num, num2);
			if (tileSimple.Type.grey_goo)
			{
				Config.grey_goo_damaged = true;
			}
			if (pOptions.add_burned && !tileSimple.Type.liquid)
			{
				tileSimple.setBurned();
			}
			if (pOptions.lightning_effect)
			{
				applyLightningEffect(tileSimple);
			}
			if (pOptions.add_heat != 0)
			{
				World.world.heat.addTile(tileSimple, pOptions.add_heat);
			}
			if (tileSimple.hasBuilding() && pOptions.damage_buildings)
			{
				bool flag = true;
				if (pOptions.ignore_kingdoms != null && tileSimple.building.isAlive() && !tileSimple.building.kingdom.isNature())
				{
					for (int j = 0; j < pOptions.ignore_kingdoms.Length; j++)
					{
						string pID = pOptions.ignore_kingdoms[j];
						Kingdom kingdom = World.world.kingdoms_wild.get(pID);
						if (tileSimple.building.kingdom == kingdom)
						{
							flag = false;
						}
					}
				}
				if (flag)
				{
					tileSimple.building.getHit(pOptions.damage);
				}
			}
			if (pOptions.set_fire)
			{
				tileSimple.startFire(pForce: true);
			}
			bool flag2 = false;
			if (pOptions.explode_tile)
			{
				flag2 = explodeTile(tileSimple, brushPixelData.dist, pRad, pTargetTile, pOptions);
			}
			if (pOptions.transform_to_wasteland && !flag2)
			{
				checkAcidTerraform(tileSimple);
			}
			if (tileSimple.hasUnits() && !string.IsNullOrEmpty(pOptions.add_trait))
			{
				tileSimple.doUnits(delegate(Actor tActor)
				{
					tActor.addTrait(pOptions.add_trait);
				});
			}
		}
	}

	public static bool explodeTile(WorldTile pTile, float pDist, float pRadius, WorldTile pExplosionCenter, TerraformOptions pOptions)
	{
		if (pOptions.damage > 0)
		{
			pTile.doUnits(delegate(Actor tActor)
			{
				if (!tActor.asset.very_high_flyer || pOptions.applies_to_high_flyers)
				{
					tActor.getHit(pOptions.damage, pFlash: true, AttackType.Explosion);
				}
			});
		}
		if (pTile.isTemporaryFrozen())
		{
			pTile.unfreeze();
		}
		float num = 0f;
		num = pDist / pRadius;
		float num2 = 1f - num;
		int num3 = (int)(30f * num2);
		if (num3 <= 0)
		{
			return false;
		}
		bool liquid = pTile.Type.liquid;
		if (!pTile.Type.explodable && Randy.random() > num2)
		{
			return false;
		}
		World.world.game_stats.data.pixelsExploded++;
		if (pOptions.explosion_pixel_effect)
		{
			World.world.explosion_layer.setDirty(pTile, pDist, pRadius);
		}
		num3 -= (int)((double)num3 * 0.5 * (double)Randy.random() * (double)num);
		if (pTile.Type.explodable && pTile.explosion_wave == 0)
		{
			World.world.explosion_layer.explodeBomb(pTile);
		}
		if (pTile.Type.explodable_delayed)
		{
			World.world.explosion_layer.activateDelayedBomb(pTile);
		}
		if (pTile.Type.strength <= pOptions.explode_strength)
		{
			decreaseTile(pTile, pDamage: true, TerraformLibrary.flash);
		}
		if (pTile.hasBuilding() && pTile.Type.liquid && !pTile.building.asset.can_be_placed_on_liquid)
		{
			pTile.building.startDestroyBuilding();
		}
		if (!liquid)
		{
			pTile.setBurned();
			if (pOptions.explode_and_set_random_fire)
			{
				if ((double)Randy.random() > 0.8)
				{
					pTile.startFire(pForce: true);
				}
				else
				{
					pTile.startFire();
				}
			}
		}
		return true;
	}

	public static void damageWorld(WorldTile pTile, int pRad, TerraformOptions pOptions, BaseSimObject pByWho = null)
	{
		if (pOptions.shake)
		{
			World.world.startShake(pOptions.shake_duration, pOptions.shake_interval, pOptions.shake_intensity);
		}
		if (pOptions.apply_force)
		{
			World.world.applyForceOnTile(pTile, pRad, pOptions.force_power, pForceOut: true, pOptions.damage, pOptions.ignore_kingdoms, pByWho, pOptions);
		}
		applyTileDamage(pTile, pRad, pOptions);
	}

	public static void makeTileChanged(WorldTile pTile)
	{
		World.world.resetRedrawTimer();
	}

	public static void removeLifeFromTile(WorldTile pTile)
	{
		World.world.conway_layer.remove(pTile);
		if (pTile.Type.life)
		{
			decreaseTile(pTile, pDamage: false, "destroy_life");
		}
		double tClickStartedAt = World.world.player_control.click_started_at;
		pTile.doUnits(delegate(Actor tActor)
		{
			if (tActor.a.asset.can_be_killed_by_life_eraser && !(tActor.a.created_time_unscaled >= tClickStartedAt))
			{
				AchievementLibrary.not_on_my_watch.check(tActor);
				tActor.applyRandomForce();
				tActor.getHitFullHealth(AttackType.Divine);
			}
		});
		HashSet<TornadoEffect> tornadoesFromTile = TornadoEffect.getTornadoesFromTile(pTile);
		if (tornadoesFromTile == null)
		{
			return;
		}
		using ListPool<TornadoEffect> listPool = new ListPool<TornadoEffect>(tornadoesFromTile);
		foreach (ref TornadoEffect item in listPool)
		{
			item.die();
		}
	}

	public static void createRoadTile(WorldTile pTile)
	{
		terraformTop(pTile, TopTileLibrary.road, AssetManager.terraform.get("road"));
	}

	public static void createRoadTilesToBuild(List<WorldTile> pPath, WorldTile pFrom, WorldTile pTarget, bool pForceFinished = false)
	{
		if (pPath.Count > 20 || (pTarget.road_island != null && pTarget.road_island == pFrom.road_island))
		{
			return;
		}
		for (int i = 0; i < pPath.Count; i++)
		{
			WorldTile worldTile = pPath[i];
			if (!worldTile.Type.road)
			{
				if (pFrom != worldTile && pFrom.road_island != null && worldTile.road_island == pTarget.road_island)
				{
					return;
				}
				temp_list_tiles_road_tiles_to_build.Add(worldTile);
				if (pForceFinished)
				{
					createRoadTile(worldTile);
				}
			}
		}
		World.world.resetRedrawTimer();
	}

	public static void makeRoadBetween(WorldTile pTile1, WorldTile pTile2, City pCity = null, bool pForceFinished = false)
	{
		if (pTile1.road_island == null || pTile1.road_island != pTile2.road_island)
		{
			temp_list_tiles_road_path.Clear();
			temp_list_tiles_road_tiles_to_build.Clear();
			World.world.pathfinding_param.resetParam();
			World.world.pathfinding_param.roads = true;
			World.world.calcPath(pTile1, pTile2, temp_list_tiles_road_path);
			createRoadTilesToBuild(temp_list_tiles_road_path, pTile1, pTile2, pForceFinished);
			pCity?.addRoads(temp_list_tiles_road_tiles_to_build);
		}
	}

	public static void tryRemoveTornadoFromTile(WorldTile pTile)
	{
		HashSet<TornadoEffect> tornadoesFromTile = TornadoEffect.getTornadoesFromTile(pTile);
		if (tornadoesFromTile == null)
		{
			return;
		}
		using ListPool<TornadoEffect> listPool = new ListPool<TornadoEffect>(tornadoesFromTile);
		foreach (ref TornadoEffect item in listPool)
		{
			item.die();
		}
	}

	public static void checkSantaHit(Vector2Int pPos, int pRad)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		List<BaseEffect> list = World.world.stack_effects.get("fx_santa").getList();
		for (int i = 0; i < list.Count; i++)
		{
			Santa component = ((Component)list[i]).GetComponent<Santa>();
			if (component.active && component.alive)
			{
				Vector3 localPosition = ((Component)component).transform.localPosition;
				if (!(Toolbox.Dist(((Vector2Int)(ref pPos)).x, 0f, localPosition.x, 0f) > (float)pRad) && !(localPosition.y < (float)((Vector2Int)(ref pPos)).y) && !(localPosition.y - 20f > (float)((Vector2Int)(ref pPos)).y))
				{
					component.alive = false;
					AchievementLibrary.mayday.check();
				}
			}
		}
	}

	public static void checkUFOHit(Vector2Int pPos, int pRad, Actor pActor)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		Kingdom kingdom = World.world.kingdoms_wild.get("aliens");
		if (kingdom.units.Count == 0)
		{
			return;
		}
		List<Actor> units = kingdom.units;
		for (int i = 0; i < units.Count; i++)
		{
			Actor actor = units[i];
			if (actor.isAlive())
			{
				Vector3 val = Vector2.op_Implicit(actor.current_position);
				if (!(Toolbox.Dist(((Vector2Int)(ref pPos)).x, 0f, val.x, 0f) > (float)pRad) && !(val.y < (float)((Vector2Int)(ref pPos)).y) && !(val.y - 10f > (float)((Vector2Int)(ref pPos)).y) && actor.asset.flag_ufo)
				{
					actor.getHit(actor.getHealth(), pFlash: true, AttackType.Other, pActor);
				}
			}
		}
	}

	public static void checkTornadoHit(Vector2Int pPos, int pRad)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (!World.world.stack_effects.get("fx_tornado").isAnyActive())
		{
			return;
		}
		using ListPool<BaseEffect> listPool = new ListPool<BaseEffect>(World.world.stack_effects.get("fx_tornado").getList());
		for (int i = 0; i < listPool.Count; i++)
		{
			if (listPool[i].active)
			{
				TornadoEffect tornadoEffect = (TornadoEffect)listPool[i];
				if (!tornadoEffect.isKilled() && !(Toolbox.DistVec2Float(Vector2.op_Implicit(((Component)tornadoEffect).transform.localPosition), Vector2Int.op_Implicit(pPos)) > (float)pRad))
				{
					tornadoEffect.split();
				}
			}
		}
	}

	public static void checkLightningAction(Vector2Int pPos, int pRad, Actor pActor = null, bool pCheckForImmortal = false, bool pCheckMayIInterrupt = false)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		int num = pRad * pRad;
		List<Actor> simpleList = World.world.units.getSimpleList();
		for (int i = 0; i < simpleList.Count; i++)
		{
			Actor actor = simpleList[i];
			if (Toolbox.SquaredDistVec2(actor.current_tile.pos, pPos) > num)
			{
				continue;
			}
			if (actor.asset.flag_finger)
			{
				actor.getActorComponent<GodFinger>().lightAction();
				actor.getHit(1f, pFlash: true, AttackType.Other, pActor);
				continue;
			}
			if (pCheckForImmortal && !flag && !actor.hasTrait("immortal") && Randy.randomChance(0.2f))
			{
				actor.addTrait("immortal");
				actor.addTrait("energized");
				flag = true;
			}
			if (pCheckMayIInterrupt)
			{
				AchievementLibrary.may_i_interrupt.checkBySignal(actor.ai.task?.id);
			}
		}
	}
}
