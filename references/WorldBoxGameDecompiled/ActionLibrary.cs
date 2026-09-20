using System.Collections.Generic;
using UnityEngine;
using ai;

public static class ActionLibrary
{
	public static bool unluckyMeteorite(BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (!WorldLawLibrary.world_law_disasters_nature.isEnabled())
		{
			return false;
		}
		if (World.world.cities.Count < 5)
		{
			return false;
		}
		if (pTarget.a.getAge() < 30)
		{
			return false;
		}
		if (!Randy.randomChance(5E-05f))
		{
			return false;
		}
		Meteorite.spawnMeteoriteDisaster(pTarget.current_tile, pTarget.a);
		return true;
	}

	public static bool unluckyFall(BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (Randy.randomChance(0.8f))
		{
			return false;
		}
		pTarget.a.makeStunned();
		return true;
	}

	public static bool flamingWeapon(BaseSimObject pTarget, WorldTile pTile = null)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		if (!MapBox.isRenderGameplay())
		{
			return false;
		}
		if (pTarget.isBuilding())
		{
			return false;
		}
		Actor a = pTarget.a;
		if (!a.a.is_visible)
		{
			return false;
		}
		Sprite renderedItemSprite = a.getRenderedItemSprite();
		if ((Object)(object)renderedItemSprite == (Object)null)
		{
			return false;
		}
		AnimationFrameData animationFrameData = a.getAnimationFrameData();
		if (animationFrameData == null)
		{
			return false;
		}
		Vector3 point = new Vector3
		{
			x = a.cur_transform_position.x + animationFrameData.pos_item.x * a.current_scale.x,
			y = a.cur_transform_position.y + animationFrameData.pos_item.y * a.current_scale.y,
			z = -0.01f
		};
		Rect rect = renderedItemSprite.rect;
		float num = ((Rect)(ref rect)).height * a.current_scale.y;
		if (a.is_moving)
		{
			point.y += num;
			point.x += Randy.randomFloat(-0.1f, 0.1f);
			point.y += Randy.randomFloat(-0.1f, 0.2f);
		}
		else
		{
			point.x += Randy.randomFloat(-0.05f, 0.05f);
			float num2 = Randy.randomFloat(0f, num * 1.5f);
			if ((double)num2 < (double)num * 0.5)
			{
				point.x += Randy.randomFloat(-0.15f, 0.15f);
			}
			point.y += num2;
		}
		if (a.current_rotation.y != 0f || a.current_rotation.z != 0f)
		{
			point = Toolbox.RotatePointAroundPivot(ref point, ref a.cur_transform_position, ref a.current_rotation);
		}
		BaseEffect baseEffect = EffectsLibrary.spawn("fx_weapon_particle");
		if ((Object)(object)baseEffect != (Object)null)
		{
			((StatusParticle)baseEffect).spawnParticle(point, Toolbox.colors_fire.GetRandom());
			return true;
		}
		return false;
	}

	public static bool shiny(BaseSimObject pTarget, WorldTile pTile = null)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!pTile.has_tile_up)
		{
			return false;
		}
		if (!MapBox.isRenderGameplay())
		{
			return false;
		}
		Vector3 posV = pTile.tile_up.posV3;
		posV.x += Randy.randomFloat(-0.3f, 0.3f);
		posV.y += Randy.randomFloat(-0.3f, 0.3f);
		EffectsLibrary.spawnAt("fx_building_sparkle", posV, 0.1f);
		return true;
	}

	public static bool restoreHealthOnHit(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget == null)
		{
			return false;
		}
		if (!pTarget.isActor())
		{
			return false;
		}
		if (!pSelf.isActor())
		{
			return false;
		}
		if (!pSelf.isAlive())
		{
			return false;
		}
		int maxHealthPercent = pTarget.getMaxHealthPercent(0.05f);
		pSelf.a.restoreHealth(maxHealthPercent);
		return true;
	}

	public static void throwTorchAtTile(BaseSimObject pSelf, WorldTile pTile)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int pos = pTile.pos;
		Vector3 val = Vector2.op_Implicit(pSelf.current_position);
		float pDist = Vector2.Distance(Vector2.op_Implicit(val), Vector2Int.op_Implicit(pos));
		Vector3 newPoint = Toolbox.getNewPoint(val.x, val.y, ((Vector2Int)(ref pos)).x, ((Vector2Int)(ref pos)).y, pDist);
		Vector3 newPoint2 = Toolbox.getNewPoint(val.x, val.y, ((Vector2Int)(ref pos)).x, ((Vector2Int)(ref pos)).y, pSelf.a.stats["size"]);
		newPoint2.y += 0.5f;
		World.world.projectiles.spawn(pSelf, null, "torch", newPoint2, newPoint);
	}

	public static bool canThrowBomb(BaseSimObject pTarget, WorldTile pTile)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		float x = pTarget.a.current_position.x;
		float y = pTarget.a.current_position.y;
		Vector2Int pos = pTile.pos;
		float x2 = ((Vector2Int)(ref pos)).x;
		pos = pTile.pos;
		float num = Toolbox.Dist(x, y, x2, ((Vector2Int)(ref pos)).y);
		if (num > 3f && num < 26f)
		{
			return true;
		}
		return false;
	}

	public static void throwBombAtTile(BaseSimObject pSelf, WorldTile pTile)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int pos = pTile.pos;
		Vector3 val = Vector2.op_Implicit(pSelf.current_position);
		float pDist = Vector2.Distance(Vector2.op_Implicit(val), Vector2Int.op_Implicit(pos));
		Vector3 newPoint = Toolbox.getNewPoint(val.x, val.y, ((Vector2Int)(ref pos)).x, ((Vector2Int)(ref pos)).y, pDist);
		Vector3 newPoint2 = Toolbox.getNewPoint(val.x, val.y, ((Vector2Int)(ref pos)).x, ((Vector2Int)(ref pos)).y, pSelf.a.stats["size"]);
		newPoint2.y += 0.5f;
		World.world.projectiles.spawn(pSelf, null, "firebomb", newPoint2, newPoint);
	}

	public static bool zombieInfectAttack(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!pTarget.isAlive())
		{
			return false;
		}
		if (!pTarget.isActor())
		{
			return false;
		}
		if (Randy.randomChance(0.25f))
		{
			pTarget.a.startShake(0.2f, 0.05f, pHorizontal: true, pVertical: false);
		}
		pTarget.a.spawnParticle(Toolbox.color_infected);
		if (pTarget.a.asset.can_turn_into_zombie && Randy.randomChance(0.5f))
		{
			pTarget.a.addTrait("infected");
		}
		return true;
	}

	public static bool zombieEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		pTarget.a.spawnParticle(Toolbox.color_infected);
		if (Randy.randomChance(0.25f))
		{
			pTarget.a.startShake(0.2f, 0.05f, pHorizontal: true, pVertical: false);
		}
		return true;
	}

	public static bool infectedEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		int num = pTarget.getHealth() / 10;
		if (num < 10)
		{
			num = 10;
		}
		pTarget.a.getHit(num, pFlash: true, AttackType.Infection, null, pSkipIfShake: false);
		pTarget.a.spawnParticle(Toolbox.color_infected);
		pTarget.a.startShake(0.4f, 0.2f, pHorizontal: true, pVertical: false);
		return true;
	}

	public static bool mushSporesEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		int num = 3;
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f, pRandom: true))
		{
			if (item != pTarget.a && !Randy.randomChance(0.7f) && item.addTrait("mush_spores"))
			{
				item.spawnParticle(Toolbox.color_mushSpores);
				num--;
				if (num == 0)
				{
					break;
				}
			}
		}
		return true;
	}

	public static bool tumorEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		pTarget.a.startShake(0.4f, 0.2f, pHorizontal: true, pVertical: false);
		if (Randy.randomChance(0.1f))
		{
			pTarget.getHit(pTarget.getMaxHealthPercent(0.1f), pFlash: false, AttackType.Tumor, null, pSkipIfShake: false);
		}
		return true;
	}

	public static bool healingAuraEffect(BaseSimObject pSelf, WorldTile pTile = null)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (!Randy.randomChance(0.2f))
		{
			return false;
		}
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 4f, pRandom: true))
		{
			if (item != pSelf.a && !item.hasMaxHealth() && !pSelf.areFoes(item))
			{
				item.restoreHealth(10);
				item.spawnParticle(Toolbox.color_heal);
				item.removeTrait("plague");
			}
		}
		return true;
	}

	public static bool heliophobiaEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		Actor a = pTarget.a;
		BiomeAsset biome = a.current_tile.getBiome();
		if (biome != null)
		{
			if (biome.cold_biome)
			{
				return false;
			}
			if (biome.dark_biome)
			{
				return false;
			}
		}
		if (!World.world_era.flag_light_damage)
		{
			return false;
		}
		int num = (int)((float)a.getMaxHealth() * 0.1f) + 1;
		a.getHit(num, pFlash: true, AttackType.Other, null, pSkipIfShake: true, pMetallicWeapon: false, pCheckDamageReduction: true);
		return true;
	}

	public static bool regenerationEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Actor a = pTarget.a;
		if (a.hasTrait("infected"))
		{
			return true;
		}
		if (!a.hasMaxHealth() && !a.isHungry() && Randy.randomChance(0.2f))
		{
			int maxHealthPercent = a.getMaxHealthPercent(0.02f);
			a.restoreHealth(maxHealthPercent);
			a.spawnParticle(Toolbox.color_heal);
		}
		checkRegenerationTraits(a);
		return true;
	}

	private static void checkRegenerationTraits(Actor pActorTarget)
	{
		if (pActorTarget.hasTrait("crippled") && Randy.randomChance(0.05f))
		{
			pActorTarget.removeTrait("crippled");
		}
		if (pActorTarget.hasTrait("skin_burns") && Randy.randomChance(0.05f))
		{
			pActorTarget.removeTrait("skin_burns");
		}
		if (pActorTarget.hasTrait("eyepatch") && Randy.randomChance(0.05f))
		{
			pActorTarget.removeTrait("eyepatch");
		}
	}

	public static bool regenerationEffectClan(BaseSimObject pTarget, WorldTile pTile = null)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Actor a = pTarget.a;
		if (a.hasTrait("infected"))
		{
			return true;
		}
		if (!a.hasMaxHealth() && !a.isHungry() && Randy.randomChance(0.2f))
		{
			int maxHealthPercent = a.getMaxHealthPercent(0.01f);
			a.restoreHealth(maxHealthPercent);
			a.spawnParticle(Toolbox.color_heal);
		}
		checkRegenerationTraits(a);
		return true;
	}

	public static bool suprisedByArchitector(BaseSimObject _, WorldTile pTile)
	{
		if (World.world.isPaused())
		{
			return false;
		}
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 8f))
		{
			item.tryToGetSurprised(pTile);
		}
		return true;
	}

	public static bool coldAuraEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		World.world.loopWithBrush(pTarget.current_tile, Brush.get(4), PowerLibrary.drawTemperatureMinus);
		return true;
	}

	public static bool megaHeartbeat(BaseSimObject pTarget, WorldTile pTile = null)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		World.world.applyForceOnTile(pTile, 3, 0.3f, pForceOut: true, 0, null, pTarget);
		EffectsLibrary.spawnExplosionWave(pTile.posV3, 3f, 0.5f);
		return true;
	}

	public static bool thornsDefense(BaseSimObject pSelf, BaseSimObject pAttackedBy, WorldTile pTile = null)
	{
		if (pSelf.isAlive() && Randy.randomChance(0.5f))
		{
			if (pAttackedBy != null && pAttackedBy.isActor() && pAttackedBy.isAlive())
			{
				Actor a = pAttackedBy.a;
				if (Toolbox.DistTile(pSelf.a.current_tile, a.a.current_tile) < 2f)
				{
					float pDamage = a.stats["damage"] * 0.2f;
					a.getHit(pDamage, pFlash: true, AttackType.Weapon, pSelf);
				}
			}
			return true;
		}
		return false;
	}

	public static bool bubbleDefense(BaseSimObject pSelf, BaseSimObject pAttackedBy, WorldTile pTile = null)
	{
		if (pSelf.hasHealth() && Randy.randomChance(0.1f))
		{
			pSelf.addStatusEffect("shield", 5f);
			return true;
		}
		return false;
	}

	public static bool plagueEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		tickPlagueInfection(pTarget.a);
		pTarget.a.startShake(0.4f, 0.2f, pHorizontal: true, pVertical: false);
		if (Randy.randomChance(0.1f))
		{
			int num = pTarget.getMaxHealthPercent(0.15f) + 1;
			pTarget.a.getHit(num, pFlash: false, AttackType.Plague, null, pSkipIfShake: false);
		}
		return true;
	}

	public static bool energizedLightning(BaseSimObject pTarget, WorldTile pTile = null)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!Toolbox.inMapBorder(ref pTarget.current_position))
		{
			EffectsLibrary.spawnAt("fx_lightning_small", pTarget.current_position, 0.25f);
			return true;
		}
		MapBox.spawnLightningSmall(pTarget.current_tile, 0.25f, pTarget.a);
		return true;
	}

	public static bool contagiousEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (!WorldLawLibrary.world_law_rat_plague.isEnabled())
		{
			return false;
		}
		if (Randy.randomChance(0.7f) && ActorTool.countContagiousNearby(pTarget.a) > 20 && Randy.randomChance(0.2f))
		{
			tickPlagueInfection(pTarget.a);
		}
		return true;
	}

	public static bool deathMark(BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (Randy.randomChance(0.2f))
		{
			pTarget.a.getHitFullHealth(AttackType.Divine);
		}
		return true;
	}

	private static void tickPlagueInfection(Actor pActor)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		pActor.spawnParticle(Toolbox.color_plague);
		if (!Randy.randomChance(0.05f))
		{
			return;
		}
		int num = 3;
		foreach (Actor item in Finder.getUnitsFromChunk(pActor.current_tile, 0, 6f, pRandom: true))
		{
			if (item != pActor)
			{
				if (item.addTrait("plague"))
				{
					break;
				}
				num--;
				if (num <= 0)
				{
					break;
				}
			}
		}
	}

	public static bool burningFeetEffectTileDraw(WorldTile pTile, string pPowerID)
	{
		if (pTile.isTemporaryFrozen() && Randy.randomBool())
		{
			pTile.unfreeze();
		}
		return true;
	}

	public static bool burningFeetEffect(BaseSimObject pSelf, WorldTile pTile = null)
	{
		WorldTile current_tile = pSelf.current_tile;
		if (!current_tile.Type.can_be_set_on_fire_by_burning_feet)
		{
			return false;
		}
		Actor a = pSelf.a;
		if (a.isInLiquid())
		{
			return false;
		}
		if (!a.has_attack_target && !a.hasTag("moody"))
		{
			return false;
		}
		World.world.loopWithBrush(current_tile, Brush.get(4), burningFeetEffectTileDraw);
		current_tile.startFire(pForce: true);
		for (int i = 0; i < current_tile.neighbours.Length; i++)
		{
			WorldTile obj = current_tile.neighbours[i];
			obj.startFire();
			obj.setBurned();
		}
		return true;
	}

	public static bool flowerPrintsEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (!Randy.randomChance(0.3f))
		{
			return false;
		}
		WorldTile current_tile = pTarget.a.current_tile;
		BiomeAsset biome_asset = current_tile.Type.biome_asset;
		if (biome_asset == null)
		{
			return false;
		}
		if (!biome_asset.grow_vegetation_auto)
		{
			return false;
		}
		if (biome_asset.grow_type_selector_plants != null)
		{
			BuildingActions.tryGrowVegetationRandom(current_tile, VegetationType.Plants, pOnStart: false, pCheckLimit: false);
		}
		return true;
	}

	public static bool acidBloodEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		for (int i = 0; i < 5; i++)
		{
			if (Randy.randomBool())
			{
				World.world.drop_manager.spawnParabolicDrop(pTarget.a.current_tile, "acid", 0f, 0.1f, 5f, 0.5f, 4f, 0.15f);
			}
		}
		if (!pTarget.isActor())
		{
			return true;
		}
		if (pTarget.a.asset.actor_size < ActorSize.S17_Dragon)
		{
			return true;
		}
		for (int j = 0; j < 25; j++)
		{
			if (Randy.randomBool())
			{
				World.world.drop_manager.spawnParabolicDrop(pTarget.a.current_tile, "acid", 0f, 0.1f, 10f, 0.5f, 10f, 0.15f);
			}
			for (int k = 0; k < pTarget.a.current_tile.neighboursAll.Length; k++)
			{
				WorldTile pTile2 = pTarget.a.current_tile.neighboursAll[k];
				if (Randy.randomBool())
				{
					World.world.drop_manager.spawnParabolicDrop(pTile2, "acid", 0f, 0.1f, 10f, 0.5f, 7f, 0.15f);
				}
			}
		}
		return true;
	}

	public static bool acidTouchEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (!Randy.randomChance(0.3f))
		{
			return false;
		}
		MapAction.checkAcidTerraform(pTarget.a.current_tile);
		return true;
	}

	public static bool sunblessedEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (!Randy.randomChance(0.5f))
		{
			return false;
		}
		if (World.world.era_manager.getCurrentAge().flag_night)
		{
			return false;
		}
		float pVal = Randy.randomFloat(0.05f, 0.1f);
		pTarget.a.restoreHealthPercent(pVal);
		return true;
	}

	public static bool castSpawnSkeleton(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget != null)
		{
			pTile = pTarget.current_tile;
		}
		int num = 0;
		foreach (Actor item in Finder.findSpeciesAroundTileChunk(pTile, "skeleton"))
		{
			_ = item;
			if (num++ > 6)
			{
				return false;
			}
		}
		WorldTile worldTile = pTile?.region?.getRandomTile();
		if (worldTile == null)
		{
			return false;
		}
		spawnSkeleton(pSelf, worldTile);
		return true;
	}

	public static bool spawnSkeleton(BaseSimObject pCaster, WorldTile pTile = null)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (pTile == null)
		{
			return false;
		}
		BaseEffect baseEffect = EffectsLibrary.spawnAt("fx_create_skeleton", pTile.posV3, 0.1f);
		Actor tActorCaster = pCaster.a;
		Subspecies tTargetSubspecies = null;
		TileZone current_zone = tActorCaster.current_zone;
		bool tNeedNewSkeletonForm = false;
		Subspecies tSubspeciesTargetForNewSkeleton = null;
		City city = current_zone.city;
		if (city != null && !city.kingdom.isNeutral())
		{
			Subspecies mainSubspecies = city.getMainSubspecies();
			tTargetSubspecies = mainSubspecies?.getSkeletonForm();
			if (tTargetSubspecies == null)
			{
				tNeedNewSkeletonForm = true;
				tSubspeciesTargetForNewSkeleton = mainSubspecies;
			}
		}
		else if (tActorCaster.hasCity())
		{
			city = tActorCaster.city;
			tTargetSubspecies = city.getSubspecies("skeleton");
		}
		baseEffect.setCallback(19, delegate
		{
			Actor actor = World.world.units.createNewUnit("skeleton", pTile, pMiracleSpawn: false, 0f, tTargetSubspecies, null, pSpawnWithItems: true, pAdultAge: true);
			actor.makeWait(1f);
			if (!tActorCaster.isRekt())
			{
				if (actor.subspecies.isJustCreated() && tActorCaster.isKingdomCiv())
				{
					actor.subspecies.addTrait("prefrontal_cortex");
				}
				if (actor.subspecies.isJustCreated() && tNeedNewSkeletonForm && !tSubspeciesTargetForNewSkeleton.isRekt())
				{
					tSubspeciesTargetForNewSkeleton.setSkeletonForm(actor.subspecies);
				}
				if (tActorCaster.isKingdomCiv() && actor.subspecies.hasTrait("prefrontal_cortex"))
				{
					City city2 = tActorCaster.city;
					Kingdom kingdom = tActorCaster.kingdom;
					if (!city2.isRekt() && city2.kingdom == kingdom)
					{
						actor.joinCity(tActorCaster.city);
					}
					else
					{
						actor.joinKingdom(kingdom);
					}
				}
			}
		});
		return true;
	}

	public static bool castFire(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget != null)
		{
			pTile = pTarget.current_tile;
		}
		if (pTile == null)
		{
			return false;
		}
		World.world.drop_manager.spawn(pTile, "fire", 15f, -1f, -1L);
		for (int i = 0; i < 3; i++)
		{
			World.world.drop_manager.spawn(pTile.neighboursAll.GetRandom(), "fire", 15f, -1f, -1L);
		}
		return true;
	}

	public static bool castSpellSilence(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget != null)
		{
			pTile = pTarget.current_tile;
		}
		if (pTile == null)
		{
			return false;
		}
		World.world.drop_manager.spawn(pTile, "spell_silence", 15f, -1f, -1L);
		return true;
	}

	public static bool castBloodRain(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget != null)
		{
			pTile = pTarget.current_tile;
		}
		if (pTile == null)
		{
			return false;
		}
		World.world.drop_manager.spawn(pTile, "blood_rain", 15f, -1f, pSelf.id);
		return true;
	}

	public static bool castSpawnGrassSeeds(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTile == null)
		{
			pTile = pTarget.current_tile;
		}
		if (pTile == null)
		{
			return false;
		}
		if (WorldLawLibrary.world_law_gaias_covenant.isEnabled())
		{
			return false;
		}
		World.world.drop_manager.spawn(pTile, "seeds_grass", 15f, -1f, -1L);
		return true;
	}

	public static bool castSpawnFertilizer(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTile == null)
		{
			pTile = pTarget.current_tile;
		}
		if (pTile == null)
		{
			return false;
		}
		World.world.drop_manager.spawn(pTile, "fertilizer_trees", 15f, -1f, -1L);
		return true;
	}

	public static bool castCurses(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget != null)
		{
			if (pTarget.a.hasStatus("cursed"))
			{
				return false;
			}
			pTile = pTarget.current_tile;
		}
		if (pTile == null)
		{
			return false;
		}
		World.world.drop_manager.spawn(pTile, "curse", 15f, -1f, -1L);
		return true;
	}

	public static bool castLightning(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget != null)
		{
			pTile = pTarget.current_tile;
		}
		MapBox.spawnLightningMedium(pTile, 0.15f, pSelf.a);
		return true;
	}

	public static bool castTornado(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget != null)
		{
			pTile = pTarget.current_tile;
		}
		if (pTile == null)
		{
			return false;
		}
		(EffectsLibrary.spawnAtTile("fx_tornado", pTile, 1f / 12f) as TornadoEffect).resizeTornado(1f / 6f);
		return true;
	}

	public static bool castCure(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTile == null)
		{
			pTile = pTarget.current_tile;
		}
		if (pTile == null)
		{
			return false;
		}
		World.world.drop_manager.spawn(pTile, "cure", 15f, -1f, -1L);
		return true;
	}

	public static bool castShieldOnHimself(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		return addShieldEffectOnTarget(pSelf, pTarget);
	}

	public static bool addShieldEffectOnTarget(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget.hasStatus("shield"))
		{
			return false;
		}
		pTarget.a.addStatusEffect("shield", 30f);
		return true;
	}

	public static bool addBurningEffectOnTarget(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (!pTarget.isAlive())
		{
			return false;
		}
		if (pTarget.isBuilding() && pTarget.b.isBurnable())
		{
			pTarget.addStatusEffect("burning");
			return true;
		}
		if (pTarget.isActor())
		{
			pTarget.addStatusEffect("burning");
			return true;
		}
		return false;
	}

	public static bool addFrozenEffectOnTarget20(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (!pTarget.isAlive())
		{
			return false;
		}
		if (pTarget.isBuilding())
		{
			return false;
		}
		if (Randy.randomChance(0.2f))
		{
			return addFrozenEffectOnTarget(pSelf, pTarget, pTile);
		}
		return false;
	}

	public static bool addStunnedEffectOnTarget20(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget.isRekt())
		{
			return false;
		}
		if (pTarget.isBuilding())
		{
			return false;
		}
		if (Randy.randomChance(0.2f))
		{
			return addStunnedEffectOnTarget(pSelf, pTarget, pTile);
		}
		return false;
	}

	public static bool addStunnedEffectOnTarget(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget.isRekt())
		{
			return false;
		}
		if (pTarget.isBuilding())
		{
			return false;
		}
		pTarget.addStatusEffect("stunned");
		return true;
	}

	public static bool addFrozenEffectOnTarget(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget.isBuilding())
		{
			return false;
		}
		if (pTarget.current_tile.Type.lava)
		{
			return false;
		}
		if (pTarget.current_tile.isOnFire())
		{
			return false;
		}
		pTarget.addStatusEffect("frozen");
		return true;
	}

	public static bool addSlowEffectOnTarget20(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (!pTarget.isAlive())
		{
			return false;
		}
		if (pTarget.isBuilding())
		{
			return false;
		}
		if (Randy.randomChance(0.2f))
		{
			return addSlowEffectOnTarget(pSelf, pTarget, pTile);
		}
		return false;
	}

	public static bool addSlowEffectOnTarget(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget.isBuilding())
		{
			return false;
		}
		pTarget.addStatusEffect("slowness");
		return true;
	}

	public static bool addPoisonedEffectOnTarget(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (!pTarget.isActor())
		{
			return false;
		}
		if (!pTarget.isAlive())
		{
			return false;
		}
		if (pTarget.a.hasTrait("poison_immune"))
		{
			return false;
		}
		if (!pTarget.a.asset.has_skin)
		{
			return false;
		}
		if (pTarget.a.asset.immune_to_injuries)
		{
			return false;
		}
		if (Randy.randomChance(0.3f))
		{
			pTarget.a.addStatusEffect("poisoned");
		}
		return false;
	}

	public static void increaseDroppedBombsCounter(WorldTile pTile = null, string pDropID = null)
	{
		World.world.game_stats.data.bombsDropped++;
		AchievementLibrary.many_bombs.check();
	}

	public static bool giveCursed(WorldTile pTile, Actor pActor)
	{
		if (pActor.hasSubspecies() && pActor.subspecies.hasTrait("adaptation_corruption"))
		{
			return false;
		}
		bool num = pActor.addStatusEffect("cursed");
		if (num)
		{
			pActor.removeTrait("blessed");
		}
		return num;
	}

	public static bool singularityTeleportation(WorldTile pTile, Actor pActor)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		BiomeAsset biomeAsset = AssetManager.biome_library.get("biome_singularity");
		WorldTile worldTile = null;
		if (biomeAsset.getTileHigh().hashset.Count > 0 && Randy.randomBool())
		{
			worldTile = biomeAsset.getTileHigh().hashset.GetRandom();
		}
		else if (biomeAsset.getTileLow().hashset.Count > 0)
		{
			worldTile = biomeAsset.getTileLow().hashset.GetRandom();
		}
		if (worldTile == null)
		{
			return false;
		}
		EffectsLibrary.spawnAt("fx_teleport_singularity", worldTile.posV3, pActor.stats["scale"] * 1.2f);
		EffectsLibrary.spawnAt("fx_teleport_singularity", pActor.current_position, pActor.stats["scale"] * 1.2f);
		pActor.cancelAllBeh();
		pActor.spawnOn(worldTile);
		pActor.makeStunned();
		return true;
	}

	public static bool timeParadox(WorldTile pTile, Actor pActor)
	{
		if (pActor.isAlive())
		{
			pActor.data.age_overgrowth++;
			return true;
		}
		return false;
	}

	public static bool giveEnchanted(WorldTile pTile, Actor pActor)
	{
		pActor.finishStatusEffect("cursed");
		return pActor.addStatusEffect("enchanted");
	}

	public static bool spawnGhost(BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (!pTarget.isActor())
		{
			return false;
		}
		if (!pTarget.a.asset.has_soul)
		{
			return false;
		}
		Actor actor = World.world.units.createNewUnit("ghost", pTile);
		actor.removeTrait("blessed");
		ActorTool.copyUnitToOtherUnit(pTarget.a, actor);
		return true;
	}

	public static bool tryToGrowBiomeGrass(BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (!pTile.Type.can_be_biome)
		{
			return false;
		}
		if (pTile.Type.is_biome)
		{
			return false;
		}
		DropsLibrary.useSeedOn(pTile, TopTileLibrary.grass_low, TopTileLibrary.grass_high);
		return true;
	}

	public static bool tryToGrowTree(BaseSimObject pTarget, WorldTile pTile = null)
	{
		BuildingActions.tryGrowVegetationRandom(pTile, VegetationType.Trees, pOnStart: false, pCheckLimit: false);
		return true;
	}

	public static bool tryToCreatePlants(BaseSimObject pTarget, WorldTile pTile = null)
	{
		BiomeAsset biome_asset = pTarget.current_tile.Type.biome_asset;
		if (biome_asset == null)
		{
			return false;
		}
		if (biome_asset.grow_type_selector_plants != null)
		{
			BuildingActions.tryGrowVegetationRandom(pTarget.current_tile, VegetationType.Plants);
		}
		return true;
	}

	public static bool startNuke(BaseSimObject pTarget, WorldTile pTile = null)
	{
		pTarget.a.findCurrentTile();
		EffectsLibrary.spawn("fx_nuke_flash", pTile, "atomic_bomb");
		return true;
	}

	public static bool clearCrabzilla(BaseSimObject pTarget, WorldTile pTile = null)
	{
		MusicBox.inst.stopDrawingSound("event:/SFX/UNIQUE/Crabzilla/CrabzillaLazer");
		MusicBox.inst.stopDrawingSound("event:/SFX/UNIQUE/Crabzilla/CrabzillaVoice");
		if (Config.joyControls)
		{
			UltimateJoystick.ResetJoysticks();
		}
		return true;
	}

	public static bool startCrabzillaNuke(BaseSimObject pTarget, WorldTile pTile = null)
	{
		pTarget.a.findCurrentTile();
		EffectsLibrary.spawn("fx_nuke_flash", pTile, "crabzilla_bomb");
		return true;
	}

	public static bool deathNuke(BaseSimObject pTarget, WorldTile pTile = null)
	{
		pTarget.a.findCurrentTile();
		DropsLibrary.action_atomic_bomb(pTarget.current_tile);
		return true;
	}

	public static bool deathBomb(BaseSimObject pTarget, WorldTile pTile = null)
	{
		pTarget.a.findCurrentTile();
		DropsLibrary.action_bomb(pTarget.current_tile);
		return true;
	}

	public static bool spawnAliens(BaseSimObject pTarget, WorldTile pTile = null)
	{
		Actor a = pTarget.a;
		a.findCurrentTile();
		if (!a.inMapBorder())
		{
			return false;
		}
		int num = 1;
		if (Randy.randomChance(0.5f))
		{
			num++;
		}
		if (Randy.randomChance(0.1f))
		{
			num++;
		}
		for (int i = 0; i < num; i++)
		{
			World.world.units.createNewUnit("alien", pTarget.a.current_tile, pMiracleSpawn: false, pTarget.a.position_height, null, null, pSpawnWithItems: true, pAdultAge: true);
		}
		return true;
	}

	public static bool fireDropsSpawn(BaseSimObject pTarget, WorldTile pTile = null)
	{
		for (int i = 0; i < 5; i++)
		{
			if (Randy.randomBool())
			{
				World.world.drop_manager.spawnParabolicDrop(pTarget.a.current_tile, "fire", 0f, 0.1f, 5f, 0.5f, 4f, 0.15f);
			}
		}
		if (!pTarget.isActor())
		{
			return true;
		}
		if (pTarget.a.asset.actor_size < ActorSize.S17_Dragon)
		{
			return true;
		}
		for (int j = 0; j < 25; j++)
		{
			if (Randy.randomBool())
			{
				World.world.drop_manager.spawnParabolicDrop(pTarget.a.current_tile, "fire", 0f, 0.1f, 10f, 0.5f, 10f, 0.15f);
			}
			for (int k = 0; k < pTarget.a.current_tile.neighboursAll.Length; k++)
			{
				WorldTile pTile2 = pTarget.a.current_tile.neighboursAll[k];
				if (Randy.randomBool())
				{
					World.world.drop_manager.spawnParabolicDrop(pTile2, "fire", 0f, 0.1f, 10f, 0.5f, 7f, 0.15f);
				}
			}
		}
		return true;
	}

	public static bool snowDropsSpawn(BaseSimObject pTarget, WorldTile pTile = null)
	{
		for (int i = 0; i < 20; i++)
		{
			if (Randy.randomBool())
			{
				World.world.drop_manager.spawnParabolicDrop(pTarget.a.current_tile, "snow", 0f, 0.1f, 5f, 0.5f, 4f, 0.15f);
			}
		}
		return true;
	}

	public static bool teleportRandom(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		WorldTile worldTile = World.world.islands_calculator.getRandomIslandGround()?.regions.GetRandom()?.tiles.GetRandom();
		if (worldTile == null)
		{
			return false;
		}
		if (worldTile.Type.block)
		{
			return false;
		}
		if (!worldTile.Type.ground)
		{
			return false;
		}
		teleportEffect(pTarget.a, worldTile);
		pTarget.a.cancelAllBeh();
		pTarget.a.spawnOn(worldTile);
		return true;
	}

	public static void teleportEffect(Actor pActor, WorldTile pTile)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		string text = pActor.asset.effect_teleport;
		if (string.IsNullOrEmpty(text))
		{
			text = "fx_teleport_blue";
		}
		EffectsLibrary.spawnAt(text, pActor.current_position, pActor.stats["scale"]);
		BaseEffect baseEffect = EffectsLibrary.spawnAt(text, pTile.posV3, pActor.stats["scale"]);
		if ((Object)(object)baseEffect != (Object)null)
		{
			baseEffect.sprite_animation.setFrameIndex(9);
		}
	}

	public static bool metamorphInto(Actor pTarget, string pAsset, bool pRemoveAcquiredTraits = false, bool pUseCurrentSubspecies = false)
	{
		if (pTarget == null)
		{
			return false;
		}
		if (!pTarget.inMapBorder())
		{
			return false;
		}
		if (pTarget.isAlreadyTransformed())
		{
			return false;
		}
		pTarget.finishStatusEffect("cursed");
		pTarget.removeTrait("infected");
		pTarget.removeTrait("mush_spores");
		pTarget.removeTrait("tumor_infection");
		if (pRemoveAcquiredTraits)
		{
			IReadOnlyCollection<ActorTrait> traits = pTarget.getTraits();
			using ListPool<ActorTrait> listPool = new ListPool<ActorTrait>(traits.Count);
			foreach (ActorTrait item in traits)
			{
				if (item.group_id == "acquired")
				{
					listPool.Add(item);
				}
			}
			pTarget.removeTraits(listPool);
		}
		Subspecies pSubspecies = null;
		if (pUseCurrentSubspecies)
		{
			pSubspecies = pTarget.subspecies;
		}
		Actor actor = World.world.units.createNewUnit(pAsset, pTarget.current_tile, pMiracleSpawn: false, 0f, pSubspecies, null, pSpawnWithItems: false);
		ActorTool.copyUnitToOtherUnit(pTarget, actor, pCopyAge: false);
		EffectsLibrary.spawn("fx_spawn", actor.current_tile);
		removeUnit(pTarget);
		pTarget.setTransformed();
		actor.addTrait("metamorphed");
		return true;
	}

	public static bool turnIntoMush(BaseSimObject pTarget, WorldTile pTile = null)
	{
		Actor a = pTarget.a;
		if (a == null)
		{
			return false;
		}
		if (!a.hasTrait("mush_spores"))
		{
			return false;
		}
		if (!a.inMapBorder())
		{
			return false;
		}
		if (!a.asset.can_turn_into_mush)
		{
			return false;
		}
		if (a.isAlreadyTransformed())
		{
			return false;
		}
		a.finishStatusEffect("cursed");
		a.removeTrait("infected");
		a.removeTrait("mush_spores");
		a.removeTrait("tumor_infection");
		a.removeTrait("peaceful");
		Actor actor = World.world.units.createNewUnit(a.asset.mush_id, a.current_tile, pMiracleSpawn: false, 0f, null, null, pSpawnWithItems: false);
		ActorTool.copyUnitToOtherUnit(a, actor);
		if (MapBox.isRenderGameplay())
		{
			EffectsLibrary.spawn("fx_spawn", actor.current_tile);
		}
		removeUnit(pTarget.a);
		a.setTransformed();
		return true;
	}

	public static Actor turnIntoMetamorph(BaseSimObject pTarget, string pAssetID)
	{
		Actor a = pTarget.a;
		if (a == null)
		{
			return null;
		}
		if (!a.inMapBorder())
		{
			return null;
		}
		if (a.isAlreadyTransformed())
		{
			return null;
		}
		a.finishStatusEffect("cursed");
		a.removeTrait("infected");
		a.removeTrait("mush_spores");
		a.removeTrait("tumor_infection");
		a.removeTrait("peaceful");
		Actor actor = World.world.units.createNewUnit(pAssetID, a.current_tile, pMiracleSpawn: false, 0f, null, null, pSpawnWithItems: false);
		ActorTool.copyUnitToOtherUnit(a, actor);
		EffectsLibrary.spawn("fx_spawn", actor.current_tile);
		removeUnit(pTarget.a);
		a.setTransformed();
		return actor;
	}

	public static Actor turnIntoIceOne(BaseSimObject pTarget, WorldTile pTile = null)
	{
		return turnIntoMetamorph(pTarget, "cold_one");
	}

	public static bool turnIntoDemon(BaseSimObject pTarget, WorldTile pTile = null)
	{
		Actor a = pTarget.a;
		if (a == null)
		{
			return false;
		}
		if (!a.inMapBorder())
		{
			return false;
		}
		if (a.isAlreadyTransformed())
		{
			return false;
		}
		a.finishStatusEffect("cursed");
		a.removeTrait("infected");
		a.removeTrait("mush_spores");
		a.removeTrait("tumor_infection");
		a.removeTrait("peaceful");
		Actor actor = World.world.units.createNewUnit("demon", a.current_tile, pMiracleSpawn: false, 0f, null, null, pSpawnWithItems: false);
		actor.addTrait("metamorphed");
		ActorTool.copyUnitToOtherUnit(a, actor);
		EffectsLibrary.spawn("fx_spawn", actor.current_tile);
		removeUnit(pTarget.a);
		a.setTransformed();
		return true;
	}

	public static bool turnIntoTumorMonster(BaseSimObject pTarget, WorldTile pTile = null)
	{
		Actor a = pTarget.a;
		if (a == null)
		{
			return false;
		}
		if (!a.hasTrait("tumor_infection"))
		{
			return false;
		}
		if (!a.inMapBorder())
		{
			return false;
		}
		if (!a.asset.can_turn_into_tumor)
		{
			return false;
		}
		if (a.isAlreadyTransformed())
		{
			return false;
		}
		a.finishStatusEffect("cursed");
		a.removeTrait("infected");
		a.removeTrait("mush_spores");
		a.removeTrait("tumor_infection");
		a.removeTrait("peaceful");
		Actor actor = World.world.units.createNewUnit(a.asset.tumor_id, a.current_tile, pMiracleSpawn: false, 0f, null, null, pSpawnWithItems: false);
		ActorTool.copyUnitToOtherUnit(a, actor);
		EffectsLibrary.spawn("fx_spawn", actor.current_tile);
		removeUnit(pTarget.a);
		a.setTransformed();
		return true;
	}

	public static bool turnIntoZombie(BaseSimObject pTarget, WorldTile pTile = null)
	{
		Actor a = pTarget.a;
		if (a == null)
		{
			return false;
		}
		if (!a.hasTrait("infected"))
		{
			return false;
		}
		if (!a.inMapBorder())
		{
			return false;
		}
		if (!a.asset.can_turn_into_zombie)
		{
			return false;
		}
		if (a.isAlreadyTransformed())
		{
			return false;
		}
		a.finishStatusEffect("cursed");
		a.removeTrait("infected");
		a.removeTrait("mush_spores");
		a.removeTrait("tumor_infection");
		string zombieID = a.asset.getZombieID();
		if (a.asset.id == "dragon")
		{
			a.removeTrait("fire_blood");
			a.removeTrait("fire_proof");
		}
		Actor actor = World.world.units.createNewUnit(zombieID, a.current_tile, pMiracleSpawn: false, 0f, null, a.subspecies, pSpawnWithItems: false);
		ActorTool.copyUnitToOtherUnit(a, actor);
		actor.removeTrait("fast");
		actor.removeTrait("agile");
		actor.removeTrait("genius");
		actor.removeTrait("peaceful");
		if (!a.getName().StartsWith("Un"))
		{
			actor.setName("Un" + Toolbox.LowerCaseFirst(a.getName()));
		}
		EffectsLibrary.spawn("fx_spawn", actor.current_tile);
		removeUnit(pTarget.a);
		a.setTransformed();
		return true;
	}

	public static bool turnIntoSkeleton(BaseSimObject pTarget, WorldTile pTile = null)
	{
		Actor a = pTarget.a;
		if (string.IsNullOrEmpty(a.asset.skeleton_id))
		{
			return false;
		}
		if (a == null)
		{
			return false;
		}
		if (!a.hasStatus("cursed"))
		{
			return false;
		}
		if (!a.inMapBorder())
		{
			return false;
		}
		if (a.isAlreadyTransformed())
		{
			return false;
		}
		string skeleton_id = a.asset.skeleton_id;
		a.finishStatusEffect("cursed");
		a.removeTrait("infected");
		a.removeTrait("mush_spores");
		a.removeTrait("tumor_infection");
		Subspecies subspecies = null;
		if (a.hasSubspecies())
		{
			subspecies = a.subspecies.getSkeletonForm();
		}
		Actor actor = World.world.units.createNewUnit(skeleton_id, a.current_tile, pMiracleSpawn: false, 0f, subspecies, null, pSpawnWithItems: false);
		Subspecies subspecies2 = actor.subspecies;
		if (subspecies2.isJustCreated())
		{
			subspecies?.setSkeletonForm(subspecies2);
		}
		ActorTool.copyUnitToOtherUnit(a, actor);
		if (!a.getName().StartsWith("Un"))
		{
			actor.setName("Un" + Toolbox.LowerCaseFirst(a.getName()));
		}
		EffectsLibrary.spawn("fx_spawn", actor.current_tile);
		removeUnit(pTarget.a);
		a.setTransformed();
		return true;
	}

	public static Actor getActorNearPos(Vector2 pPos)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Actor result = null;
		float num = float.MaxValue;
		Actor[] array = World.world.units.visible_units.array;
		int count = World.world.units.visible_units.count;
		for (int i = 0; i < count; i++)
		{
			Actor actor = array[i];
			if (actor.isAlive() && actor.asset.can_be_inspected && !actor.isInsideSomething())
			{
				float num2 = Toolbox.DistVec2Float(actor.current_position, pPos);
				if (!(num2 > 3f) && num2 < num)
				{
					result = actor;
					num = num2;
				}
			}
		}
		return result;
	}

	public static Actor getActorFromTile(WorldTile pTile = null)
	{
		if (pTile == null)
		{
			return null;
		}
		Actor result = null;
		float num = float.MaxValue;
		List<Actor> simpleList = World.world.units.getSimpleList();
		for (int i = 0; i < simpleList.Count; i++)
		{
			Actor actor = simpleList[i];
			if (actor.isAlive())
			{
				float num2 = Toolbox.SquaredDistTile(actor.current_tile, pTile);
				if (!(num2 > num) && !(num2 > 9f) && actor.asset.can_be_inspected && !actor.isInsideSomething())
				{
					result = actor;
					num = num2;
				}
			}
		}
		return result;
	}

	public static void openUnitWindow(Actor pActor)
	{
		if (!pActor.isRekt())
		{
			SelectedUnit.clear();
			SelectedUnit.select(pActor);
		}
		else if (!SelectedUnit.isSet())
		{
			return;
		}
		ScrollWindow.showWindow("unit");
	}

	public static bool inspectUnit(WorldTile pTile = null, string pPower = null)
	{
		Actor actor = null;
		actor = ((pTile != null) ? getActorFromTile(pTile) : World.world.getActorNearCursor());
		if (actor == null)
		{
			return false;
		}
		openUnitWindow(actor);
		return true;
	}

	public static bool inspectUnitSelectedMeta(WorldTile pTile = null, string pPower = null)
	{
		Actor actor = null;
		actor = ((pTile != null) ? getActorFromTile(pTile) : World.world.getActorNearCursor());
		if (actor == null)
		{
			return false;
		}
		MetaTypeAsset asset = Zones.getCurrentMapBorderMode().getAsset();
		if (asset == null)
		{
			return false;
		}
		if (asset.check_unit_has_meta(actor))
		{
			asset.set_unit_set_meta_for_meta_for_window(actor);
			ScrollWindow.showWindow(asset.window_name);
			return true;
		}
		openUnitWindow(actor);
		return true;
	}

	public static bool inspectCity(WorldTile pTile = null, string pPower = null)
	{
		if (pTile == null)
		{
			return false;
		}
		NanoObject nanoObjectFromTile = getNanoObjectFromTile(pTile, MetaTypeLibrary.city);
		if (nanoObjectFromTile == null)
		{
			return false;
		}
		MetaType.City.getAsset().selectAndInspect(nanoObjectFromTile);
		return true;
	}

	public static bool inspectKingdom(WorldTile pTile = null, string pPower = null)
	{
		if (pTile == null)
		{
			return false;
		}
		NanoObject nanoObjectFromTile = getNanoObjectFromTile(pTile, MetaTypeLibrary.kingdom);
		if (nanoObjectFromTile == null)
		{
			return false;
		}
		if (((Kingdom)nanoObjectFromTile).isNeutral())
		{
			return false;
		}
		MetaType.Kingdom.getAsset().selectAndInspect(nanoObjectFromTile);
		return true;
	}

	public static bool inspectAlliance(WorldTile pTile = null, string pPower = null)
	{
		if (pTile == null)
		{
			return false;
		}
		City city = pTile.zone.city;
		if (city.isRekt())
		{
			return false;
		}
		Kingdom kingdom = city.kingdom;
		if (kingdom.isRekt())
		{
			return false;
		}
		if (kingdom.isNeutral())
		{
			return false;
		}
		if (kingdom.hasAlliance())
		{
			MetaType.Alliance.getAsset().selectAndInspect(kingdom.getAlliance());
		}
		else
		{
			inspectKingdom(pTile, pPower);
		}
		return true;
	}

	public static bool inspectCulture(WorldTile pTile, string pPower = null)
	{
		if (pTile == null)
		{
			return false;
		}
		NanoObject nanoObjectFromTile = getNanoObjectFromTile(pTile, MetaTypeLibrary.culture);
		if (nanoObjectFromTile == null)
		{
			return false;
		}
		MetaType.Culture.getAsset().selectAndInspect(nanoObjectFromTile);
		return true;
	}

	public static bool inspectReligion(WorldTile pTile, string pPower = null)
	{
		if (pTile == null)
		{
			return false;
		}
		NanoObject nanoObjectFromTile = getNanoObjectFromTile(pTile, MetaTypeLibrary.religion);
		if (nanoObjectFromTile == null)
		{
			return false;
		}
		MetaType.Religion.getAsset().selectAndInspect(nanoObjectFromTile);
		return true;
	}

	public static bool inspectSubspecies(WorldTile pTile, string pPower = null)
	{
		if (pTile == null)
		{
			return false;
		}
		NanoObject nanoObjectFromTile = getNanoObjectFromTile(pTile, MetaTypeLibrary.subspecies);
		if (nanoObjectFromTile == null)
		{
			return false;
		}
		MetaType.Subspecies.getAsset().selectAndInspect(nanoObjectFromTile);
		return true;
	}

	public static bool inspectFamily(WorldTile pTile, string pPower = null)
	{
		if (pTile == null)
		{
			return false;
		}
		NanoObject nanoObjectFromTile = getNanoObjectFromTile(pTile, MetaTypeLibrary.family);
		if (nanoObjectFromTile == null)
		{
			return false;
		}
		MetaType.Family.getAsset().selectAndInspect(nanoObjectFromTile);
		return true;
	}

	public static bool inspectArmy(WorldTile pTile, string pPower = null)
	{
		if (pTile == null)
		{
			return false;
		}
		NanoObject nanoObjectFromTile = getNanoObjectFromTile(pTile, MetaTypeLibrary.army);
		if (nanoObjectFromTile == null)
		{
			return false;
		}
		MetaType.Army.getAsset().selectAndInspect(nanoObjectFromTile);
		return true;
	}

	public static bool inspectLanguage(WorldTile pTile, string pPower = null)
	{
		if (pTile == null)
		{
			return false;
		}
		NanoObject nanoObjectFromTile = getNanoObjectFromTile(pTile, MetaTypeLibrary.language);
		if (nanoObjectFromTile == null)
		{
			return false;
		}
		MetaType.Language.getAsset().selectAndInspect(nanoObjectFromTile);
		return true;
	}

	public static bool inspectClan(WorldTile pTile, string pPower = null)
	{
		if (pTile == null)
		{
			return false;
		}
		NanoObject nanoObjectFromTile = getNanoObjectFromTile(pTile, MetaTypeLibrary.clan);
		if (nanoObjectFromTile == null)
		{
			return false;
		}
		MetaType.Clan.getAsset().selectAndInspect(nanoObjectFromTile);
		return true;
	}

	public static bool burnTile(BaseSimObject pSelf, BaseSimObject pTarget = null, WorldTile pTile = null)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!World.world.flash_effects.contains(pTile) && Randy.randomChance(0.2f))
		{
			World.world.particles_fire.spawn(pTile.posV3);
		}
		pTile.startFire(pForce: true);
		return true;
	}

	public static bool tryToEvolveUnitViaMonolith(Actor pActor)
	{
		pActor.startShake();
		pActor.startColorEffect();
		if (!pActor.hasSubspecies())
		{
			return false;
		}
		if (pActor.hasSubspeciesTrait("pure"))
		{
			return false;
		}
		float pVal = 1f;
		if (pActor.asset.can_evolve_into_new_species)
		{
			pVal = 1f;
		}
		else if (pActor.hasSubspeciesTrait("uplifted") && pActor.subspecies.isSapient())
		{
			pVal = 0.1f;
		}
		if (!Randy.randomChance(pVal))
		{
			return false;
		}
		World.world.units.evolutionEvent(pActor, pWithBiomeEffect: true, pAscension: false);
		return true;
	}

	public static bool tryToEvolveUnitViaAscension(Actor pActor, out Actor pEvolvedActorForm)
	{
		pEvolvedActorForm = null;
		pActor.startShake();
		pActor.startColorEffect();
		if (!pActor.hasSubspecies())
		{
			return false;
		}
		if (pActor.hasSubspeciesTrait("pure"))
		{
			return false;
		}
		Actor actor = World.world.units.evolutionEvent(pActor, pWithBiomeEffect: true, pAscension: true);
		pEvolvedActorForm = actor;
		return true;
	}

	public static void startBurningObjects(BaseSimObject pSelf, BaseSimObject pTarget = null, WorldTile pTile = null)
	{
		List<BaseSimObject> allObjectsInChunks = Finder.getAllObjectsInChunks(pTile);
		for (int i = 0; i < allObjectsInChunks.Count; i++)
		{
			BaseSimObject baseSimObject = allObjectsInChunks[i];
			if (baseSimObject.isAlive() && !baseSimObject.current_tile.Type.ocean)
			{
				addBurningEffectOnTarget(pSelf, baseSimObject);
			}
		}
	}

	public static void action_growTornadoes(WorldTile pTile = null, string pDropID = null)
	{
		TornadoEffect.growTornados(pTile);
	}

	public static void action_shrinkTornadoes(WorldTile pTile = null, string pDropID = null)
	{
		TornadoEffect.shrinkTornados(pTile);
	}

	public static bool dragonSlayer(BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget == null)
		{
			return false;
		}
		if (!pTarget.isActor())
		{
			return false;
		}
		BaseSimObject attackedBy = pTarget.a.attackedBy;
		if (attackedBy != null && attackedBy.isActor() && attackedBy.isAlive())
		{
			attackedBy.a.addTrait("dragonslayer");
			return true;
		}
		return false;
	}

	public static bool mageSlayerCheck(BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget == null)
		{
			return false;
		}
		if (!pTarget.isActor())
		{
			return false;
		}
		if (!pTarget.a.hasSpells())
		{
			return false;
		}
		BaseSimObject attackedBy = pTarget.a.attackedBy;
		if (attackedBy != null && attackedBy.isActor() && attackedBy.isAlive())
		{
			attackedBy.a.addTrait("mageslayer");
			return true;
		}
		return false;
	}

	public static bool checkPiranhaAchievement(BaseSimObject pTarget, WorldTile pTile = null)
	{
		AchievementLibrary.piranha_land.check(pTarget.a);
		return true;
	}

	public static bool clickRelations(WorldTile pTile, string pPowerID)
	{
		City city = pTile.zone.city;
		if (city.isRekt())
		{
			return false;
		}
		Kingdom kingdom = city.kingdom;
		if (kingdom.isRekt())
		{
			return false;
		}
		if (kingdom.isNeutral())
		{
			return false;
		}
		if (SelectedMetas.selected_kingdom != kingdom)
		{
			SelectedMetas.selected_kingdom = kingdom;
		}
		else
		{
			ScrollWindow.showWindow("kingdom");
		}
		return true;
	}

	public static bool clickWhisperOfWar(WorldTile pTile, string pPowerID)
	{
		City city = pTile.zone.city;
		if (city.isRekt())
		{
			return false;
		}
		Kingdom kingdom = city.kingdom;
		if (kingdom.isRekt())
		{
			return false;
		}
		if (kingdom.isNeutral())
		{
			return false;
		}
		if (Config.whisper_A == null)
		{
			Config.whisper_A = kingdom;
			showWhisperTip("whisper_selected_first");
			return false;
		}
		if (Config.whisper_B == null && Config.whisper_A == kingdom)
		{
			showWhisperTip("whisper_cancelled");
			Config.whisper_A = null;
			Config.whisper_B = null;
			return false;
		}
		if (Config.whisper_B == null)
		{
			Config.whisper_B = kingdom;
		}
		if (Config.whisper_B != Config.whisper_A)
		{
			if (Config.whisper_A.isEnemy(Config.whisper_B))
			{
				showWhisperTip("whisper_already_in_war");
				Config.whisper_B = null;
				return false;
			}
			if (Config.whisper_A.isInWarOnSameSide(Config.whisper_B))
			{
				using ListPool<War> listPool = new ListPool<War>(Config.whisper_A.getWars());
				foreach (ref War item in listPool)
				{
					War current = item;
					if (!current.isTotalWar() && current.onTheSameSide(Config.whisper_A, Config.whisper_B))
					{
						current.leaveWar(Config.whisper_B);
					}
				}
			}
			bool flag = World.world.wars.haveCommonEnemy(Config.whisper_A, Config.whisper_B);
			Alliance alliance = Config.whisper_A.getAlliance();
			if (alliance != null && Alliance.isSame(alliance, Config.whisper_B.getAlliance()))
			{
				alliance.leave(Config.whisper_A);
			}
			War randomWarFor = World.world.wars.getRandomWarFor(Config.whisper_B);
			if (randomWarFor != null && !randomWarFor.isTotalWar() && !flag)
			{
				if (randomWarFor.isAttacker(Config.whisper_B))
				{
					randomWarFor.joinDefenders(Config.whisper_A);
				}
				else
				{
					randomWarFor.joinAttackers(Config.whisper_A);
				}
				showWhisperTip("whisper_joined_war");
			}
			else
			{
				World.world.diplomacy.startWar(Config.whisper_A, Config.whisper_B, WarTypeLibrary.whisper_of_war);
				showWhisperTip("whisper_new_war");
			}
			Config.whisper_A.affectKingByPowers();
			Config.whisper_A = null;
			Config.whisper_B = null;
		}
		return true;
	}

	public static bool clickUnity(WorldTile pTile, string pPowerID)
	{
		City city = pTile.zone.city;
		if (city.isRekt())
		{
			return false;
		}
		Kingdom kingdom = city.kingdom;
		if (kingdom.isRekt())
		{
			return false;
		}
		if (kingdom.isNeutral())
		{
			return false;
		}
		if (Config.unity_A == null)
		{
			Config.unity_A = kingdom;
			showWhisperTip("unity_selected_first");
			return false;
		}
		if (Config.whisper_B == null && Config.unity_A == kingdom)
		{
			showWhisperTip("unity_cancelled");
			Config.unity_A = null;
			Config.unity_B = null;
			return false;
		}
		if (Config.unity_A.hasAlliance() && kingdom.hasAlliance() && Config.unity_A.getAlliance() == kingdom.getAlliance())
		{
			showWhisperTip("unity_cancelled");
			Config.unity_A = null;
			Config.unity_B = null;
			return false;
		}
		if (Config.unity_B == null)
		{
			Config.unity_B = kingdom;
		}
		if (Config.unity_B == Config.unity_A)
		{
			return false;
		}
		if (Config.unity_A.isEnemy(Config.unity_B))
		{
			showWhisperTip("unity_in_war");
			Config.unity_B = null;
			return false;
		}
		if (Config.unity_A.hasAlliance())
		{
			if (Config.unity_A.getAlliance() == Config.unity_B.getAlliance())
			{
				showWhisperTip("unity_cancelled");
				Config.unity_B = null;
				return false;
			}
			if (Config.unity_B.hasAlliance())
			{
				Config.unity_A.getAlliance().leave(Config.unity_A);
			}
		}
		if (World.world.alliances.forceAlliance(Config.unity_A, Config.unity_B))
		{
			showWhisperTip("unity_new_alliance");
		}
		else
		{
			showWhisperTip("unity_joined_alliance");
		}
		Config.unity_A.affectKingByPowers();
		Config.unity_A = null;
		Config.unity_B = null;
		World.world.zone_calculator.dirtyAndClear();
		return true;
	}

	private static void showWhisperTip(string pText)
	{
		string text = LocalizedTextManager.getText(pText);
		if (Config.whisper_A != null)
		{
			text = text.Replace("$kingdom_A$", Config.whisper_A.name);
		}
		if (Config.whisper_B != null)
		{
			text = text.Replace("$kingdom_B$", Config.whisper_B.name);
		}
		WorldTip.showNow(text, pTranslate: false, "top", 6f);
	}

	public static bool selectWhisperOfWar(string pPowerID)
	{
		WorldTip.showNow("whisper_selected", pTranslate: true, "top");
		Config.whisper_A = null;
		Config.whisper_B = null;
		return false;
	}

	public static bool selectUnity(string pPowerID)
	{
		WorldTip.showNow("unity_selected", pTranslate: true, "top");
		Config.unity_A = null;
		Config.unity_B = null;
		return false;
	}

	public static bool selectRelations(string pPowerID)
	{
		SelectedMetas.selected_kingdom = World.world.kingdoms.getRandom();
		return false;
	}

	public static bool whirlwind(BaseSimObject pSelf, WorldTile pTile)
	{
		World.world.applyForceOnTile(pTile, 10, 3f, pForceOut: false, 0, null, pSelf);
		return true;
	}

	public static void removeUnit(Actor pActor)
	{
		pActor.removeByMetamorphosis();
	}

	public static bool breakBones(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
	{
		if (!pTarget.isAlive())
		{
			return false;
		}
		if (pTarget.isActor())
		{
			pTarget.a.addInjuryTrait("crippled");
		}
		return true;
	}

	public static bool restoreMana(WorldTile pTile, Actor pSelf)
	{
		if (pSelf.isManaFull())
		{
			return false;
		}
		int pValue = (int)((float)pSelf.getMaxMana() * 0.01f);
		pSelf.addMana(pValue);
		return true;
	}

	public static bool restoreStamina(WorldTile pTile, Actor pSelf)
	{
		if (pSelf.isStaminaFull())
		{
			return false;
		}
		int pValue = (int)((float)pSelf.getMaxStamina() * 0.01f);
		pSelf.addStamina(pValue);
		return true;
	}

	public static bool restoreFullStats(NanoObject pTarget, BaseAugmentationAsset pTrait)
	{
		if (pTarget.isRekt())
		{
			return false;
		}
		((Actor)pTarget).event_full_stats = true;
		return true;
	}

	public static bool forcedKingdomAdd(NanoObject pTarget, BaseAugmentationAsset pTrait)
	{
		if (!pTarget.isAlive())
		{
			return false;
		}
		ActorTrait actorTrait = (ActorTrait)pTrait;
		Actor actor = (Actor)pTarget;
		if (actor.asset.is_boat)
		{
			actor.getHitFullHealth(AttackType.Explosion);
			return false;
		}
		actor.applyForcedKingdomTrait();
		actor.setForcedKingdom(actorTrait.getForcedKingdom());
		return true;
	}

	public static bool forcedKingdomEffectRemove(NanoObject pTarget, BaseAugmentationAsset pTrait)
	{
		if (pTarget.isRekt())
		{
			return false;
		}
		((Actor)pTarget).setDefaultKingdom();
		return true;
	}

	public static bool madnessEffectLoad(NanoObject pTarget, BaseAugmentationAsset pTrait)
	{
		if (pTarget.isRekt())
		{
			return false;
		}
		((Actor)pTarget).setForcedKingdom(((ActorTrait)pTrait).getForcedKingdom());
		return true;
	}

	public static bool tryToMakeBuildingAlive(Building pBuilding)
	{
		if (!pBuilding.isAlive())
		{
			return false;
		}
		if (pBuilding.isRuin())
		{
			return false;
		}
		if (pBuilding.isUnderConstruction())
		{
			return false;
		}
		if (!pBuilding.asset.can_be_living_house)
		{
			return false;
		}
		Actor actor = World.world.units.createNewUnit("living_house", pBuilding.current_tile);
		actor.data.set("special_sprite_id", pBuilding.asset.id);
		actor.data.set("special_sprite_index", pBuilding.animData_index);
		actor.data.created_time = pBuilding.data.created_time;
		pBuilding.removeBuildingFinal();
		actor.startColorEffect();
		return true;
	}

	public static bool tryToMakeFloraAlive(Building pBuilding, bool pFullyGrownOnly = true)
	{
		if (!pBuilding.isAlive())
		{
			return false;
		}
		if (pBuilding.isRuin())
		{
			return false;
		}
		if (!pBuilding.asset.can_be_living_plant)
		{
			return false;
		}
		if (pBuilding.chopped)
		{
			return false;
		}
		if (pBuilding.isUnderConstruction())
		{
			return false;
		}
		if (pFullyGrownOnly && !pBuilding.isFullyGrown())
		{
			return false;
		}
		Actor actor = World.world.units.createNewUnit("living_plants", pBuilding.current_tile, pMiracleSpawn: false, 0f, null, null, pSpawnWithItems: false);
		actor.data.set("special_sprite_id", pBuilding.asset.id);
		actor.data.set("special_sprite_index", pBuilding.animData_index);
		actor.data.created_time = pBuilding.data.created_time;
		pBuilding.removeBuildingFinal();
		actor.startColorEffect();
		return true;
	}

	public static void growRandomVegetation(WorldTile pTile, BiomeAsset pBiomeAsset)
	{
		switch (Randy.randomInt(0, 3))
		{
		case 0:
			if (pBiomeAsset.grow_type_selector_trees != null)
			{
				BuildingActions.tryGrowVegetationRandom(pTile, VegetationType.Trees);
			}
			break;
		case 1:
			if (pBiomeAsset.grow_type_selector_plants != null)
			{
				BuildingActions.tryGrowVegetationRandom(pTile, VegetationType.Plants);
			}
			break;
		case 2:
			if (pBiomeAsset.grow_type_selector_bushes != null)
			{
				BuildingActions.tryGrowVegetationRandom(pTile, VegetationType.Bushes);
			}
			break;
		}
	}

	private static NanoObject getNanoObjectFromTile(WorldTile pTile, MetaTypeAsset pMetaTypeAsset)
	{
		if (pTile == null)
		{
			return null;
		}
		NanoObject nanoObject = pMetaTypeAsset.tile_get_metaobject(pTile.zone, pMetaTypeAsset.getZoneOptionState()) as NanoObject;
		if (nanoObject.isRekt())
		{
			return null;
		}
		return nanoObject;
	}
}
