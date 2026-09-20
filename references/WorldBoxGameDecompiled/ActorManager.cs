using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ai;
using ai.behaviours;

public class ActorManager : SimSystemManager<Actor, ActorData>
{
	private JobManagerActors _job_manager;

	public readonly ActorRenderData render_data = new ActorRenderData(4096);

	public readonly ActorVisibleDataArray visible_units_avatars = new ActorVisibleDataArray();

	public readonly ActorVisibleDataArray visible_units = new ActorVisibleDataArray();

	public readonly ActorVisibleDataArray visible_units_alive = new ActorVisibleDataArray();

	public readonly ActorVisibleDataArray visible_units_with_status = new ActorVisibleDataArray();

	public readonly ActorVisibleDataArray visible_units_with_favorite = new ActorVisibleDataArray();

	public readonly ActorVisibleDataArray visible_units_with_banner = new ActorVisibleDataArray();

	public readonly ActorVisibleDataArray visible_units_just_ate = new ActorVisibleDataArray();

	public readonly ActorVisibleDataArray visible_units_socialize = new ActorVisibleDataArray();

	private double _timestamp_sleeping_units;

	public readonly List<Actor> cached_sleeping_units = new List<Actor>();

	private readonly List<ActorVisibleDataArray> _unit_visible_lists = new List<ActorVisibleDataArray>();

	public bool have_dying_units;

	public readonly List<Actor> units_only_wild = new List<Actor>();

	public readonly List<Actor> units_only_civ = new List<Actor>();

	public readonly List<Actor> units_only_alive = new List<Actor>();

	public readonly List<Actor> units_only_dying = new List<Actor>();

	public ActorManager()
	{
		type_id = "unit";
		_job_manager = new JobManagerActors("actors");
		_unit_visible_lists.Add(visible_units);
		_unit_visible_lists.Add(visible_units_avatars);
		_unit_visible_lists.Add(visible_units_alive);
		_unit_visible_lists.Add(visible_units_with_status);
		_unit_visible_lists.Add(visible_units_with_favorite);
		_unit_visible_lists.Add(visible_units_with_banner);
		_unit_visible_lists.Add(visible_units_just_ate);
		_unit_visible_lists.Add(visible_units_socialize);
	}

	public void prepareForMetaChecks()
	{
		units_only_wild.Clear();
		units_only_alive.Clear();
		units_only_dying.Clear();
		units_only_civ.Clear();
		have_dying_units = false;
		List<Actor> simpleList = getSimpleList();
		for (int i = 0; i < simpleList.Count; i++)
		{
			Actor actor = simpleList[i];
			if (actor.isAlive())
			{
				if (actor.kingdom.wild)
				{
					units_only_wild.Add(actor);
				}
				else
				{
					units_only_civ.Add(actor);
				}
				units_only_alive.Add(actor);
			}
			else
			{
				units_only_dying.Add(actor);
				have_dying_units = true;
			}
		}
	}

	public void calculateVisibleActors()
	{
		Bench.bench("actors_prepare_lists", "game_total");
		clearLists();
		prepareLists();
		Bench.benchEnd("actors_prepare_lists", "game_total", pSaveCounter: false, 0L);
		Bench.bench("actors_fill_visible", "game_total");
		fillVisibleObjects();
		Bench.benchEnd("actors_fill_visible", "game_total", pSaveCounter: false, 0L);
		Bench.bench("actors_precalc_render_data_parallel", "game_total");
		precalculateRenderDataParallel();
		Bench.benchEnd("actors_precalc_render_data_parallel", "game_total", pSaveCounter: false, 0L);
		Bench.bench("actors_precalc_render_data_normal", "game_total");
		precalculateRenderDataNormal();
		Bench.benchEnd("actors_precalc_render_data_normal", "game_total", pSaveCounter: false, 0L);
	}

	private void precalculateRenderDataParallel()
	{
		int tDebugItemScale = ((!DebugConfig.isOn(DebugOption.RenderBigItems)) ? 1 : 10);
		bool tShouldRenderUnitShadows = World.world.quality_changer.shouldRenderUnitShadows();
		int tTotalVisibleObjects = visible_units.count;
		Actor[] tArray = visible_units.array;
		int tDynamicBatchSize = 256;
		int toExclusive = ParallelHelper.calcTotalBatches(tTotalVisibleObjects, tDynamicBatchSize);
		Parallel.For(0, toExclusive, World.world.parallel_options, delegate(int pBatchIndex)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0459: Unknown result type (might be due to invalid IL or missing references)
			//IL_0461: Unknown result type (might be due to invalid IL or missing references)
			//IL_0466: Unknown result type (might be due to invalid IL or missing references)
			//IL_0499: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_04db: Unknown result type (might be due to invalid IL or missing references)
			//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_04df: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0504: Unknown result type (might be due to invalid IL or missing references)
			//IL_051b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0520: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			//IL_0245: Unknown result type (might be due to invalid IL or missing references)
			//IL_0246: Unknown result type (might be due to invalid IL or missing references)
			//IL_024b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0250: Unknown result type (might be due to invalid IL or missing references)
			//IL_053c: Unknown result type (might be due to invalid IL or missing references)
			//IL_053e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0260: Unknown result type (might be due to invalid IL or missing references)
			//IL_026f: Unknown result type (might be due to invalid IL or missing references)
			//IL_027e: Unknown result type (might be due to invalid IL or missing references)
			//IL_028d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0292: Unknown result type (might be due to invalid IL or missing references)
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02da: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_037f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0380: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0307: Unknown result type (might be due to invalid IL or missing references)
			//IL_030c: Unknown result type (might be due to invalid IL or missing references)
			//IL_030d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0312: Unknown result type (might be due to invalid IL or missing references)
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0324: Unknown result type (might be due to invalid IL or missing references)
			//IL_032d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0357: Unknown result type (might be due to invalid IL or missing references)
			//IL_035d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0362: Unknown result type (might be due to invalid IL or missing references)
			//IL_0367: Unknown result type (might be due to invalid IL or missing references)
			int num = ParallelHelper.calculateBatchBeg(pBatchIndex, tDynamicBatchSize);
			int num2 = ParallelHelper.calculateBatchEnd(num, tDynamicBatchSize, tTotalVisibleObjects);
			Vector3 point = default(Vector3);
			Vector3 pivot = default(Vector3);
			for (int i = num; i < num2; i++)
			{
				Actor actor = tArray[i];
				Vector3 current_scale = actor.current_scale;
				Vector3 val = actor.updateRotation();
				Vector3 val2 = actor.updatePos();
				bool flag = actor.checkHasRenderedItem();
				bool flag2 = !actor.asset.ignore_generic_render;
				Sprite val3;
				if (flag)
				{
					Sprite renderedItemSprite = actor.getRenderedItemSprite();
					IHandRenderer cachedHandRendererAsset = actor.getCachedHandRendererAsset();
					int pColorID = -900000;
					if (cachedHandRendererAsset.is_colored)
					{
						pColorID = actor.kingdom.getColor().GetHashCode();
					}
					val3 = DynamicSprites.getCachedAtlasItemSprite(DynamicSprites.getItemSpriteID(renderedItemSprite, pColorID), renderedItemSprite);
				}
				else
				{
					val3 = null;
				}
				render_data.positions[i] = val2;
				render_data.scales[i] = current_scale;
				render_data.rotations[i] = val;
				render_data.flip_x_states[i] = actor.flip;
				render_data.colors[i] = actor.color;
				render_data.has_normal_render[i] = flag2;
				render_data.has_item[i] = flag;
				render_data.item_sprites[i] = val3;
				AnimationFrameData animationFrameData = actor.getAnimationFrameData();
				bool flag3 = false;
				if (tShouldRenderUnitShadows && actor.show_shadow)
				{
					ActorTextureSubAsset actorTextureSubAsset = ((!actor.hasSubspecies() || !actor.subspecies.has_mutation_reskin) ? actor.asset.texture_asset : actor.subspecies.mutation_skin_asset.texture_asset);
					flag3 = actorTextureSubAsset.shadow;
					if (actorTextureSubAsset.shadow)
					{
						Vector2 val4;
						if (actor.isEgg())
						{
							render_data.shadow_sprites[i] = actorTextureSubAsset.shadow_sprite_egg;
							val4 = actorTextureSubAsset.shadow_size_egg;
						}
						else if (actor.isBaby())
						{
							render_data.shadow_sprites[i] = actorTextureSubAsset.shadow_sprite_baby;
							val4 = actorTextureSubAsset.shadow_size_baby;
						}
						else
						{
							render_data.shadow_sprites[i] = actorTextureSubAsset.shadow_sprite;
							val4 = actorTextureSubAsset.shadow_size;
						}
						val4 *= Vector2.op_Implicit(current_scale);
						int num3 = (actor.flip ? 1 : (-1));
						float num4 = val4.x / 2f;
						float num5 = val4.y * 0.6f;
						float num6 = Mathf.Abs(val.z);
						Vector2 current_shadow_position = actor.current_shadow_position;
						current_shadow_position.x += num4 * (val.z * (float)num3) / 90f;
						current_shadow_position.y -= num5 * num6 / 90f;
						render_data.shadow_position[i] = Vector2.op_Implicit(current_shadow_position);
						if (animationFrameData != null && animationFrameData.size_unit != default(Vector2))
						{
							float num7 = (animationFrameData.size_unit * Vector2.op_Implicit(current_scale)).y / val4.x * current_scale.x;
							float num8 = Mathf.Lerp(current_scale.x, num7, num6 / 90f);
							render_data.shadow_scales[i] = Vector2.op_Implicit(new Vector2(num8, current_scale.y));
						}
						else
						{
							render_data.shadow_scales[i] = current_scale;
						}
					}
				}
				render_data.shadows[i] = flag3;
				if (flag2)
				{
					if (actor.canParallelSetColoredSprite())
					{
						Sprite val5 = actor.calculateMainSprite();
						render_data.main_sprites[i] = val5;
						if (actor.hasColoredSprite())
						{
							if (!actor.isColoredSpriteNeedsCheck(val5))
							{
								render_data.main_sprite_colored[i] = actor.getLastColoredSprite();
							}
							else
							{
								render_data.main_sprite_colored[i] = null;
							}
						}
						else
						{
							render_data.main_sprite_colored[i] = val5;
						}
					}
					else
					{
						render_data.main_sprites[i] = null;
						render_data.main_sprite_colored[i] = null;
					}
					if (flag)
					{
						render_data.item_scale[i] = current_scale * (float)tDebugItemScale;
						float num9 = 0f;
						float num10 = 0f;
						if (animationFrameData != null)
						{
							num9 = animationFrameData.pos_item.x;
							num10 = animationFrameData.pos_item.y;
						}
						float num11 = val2.x + num9 * current_scale.x;
						float num12 = val2.y + num10 * current_scale.y;
						float z = -0.01f + num10 * current_scale.y;
						((Vector3)(ref point))._002Ector(num11, num12);
						Vector3 angles = val;
						if (angles.y != 0f || angles.z != 0f)
						{
							((Vector3)(ref pivot))._002Ector(val2.x, val2.y, 0f);
							point = Toolbox.RotatePointAroundPivot(ref point, ref pivot, ref angles);
						}
						point.z = z;
						render_data.item_pos[i] = point;
					}
				}
			}
		});
	}

	private void precalculateRenderDataNormal()
	{
		ActorRenderData actorRenderData = render_data;
		int count = visible_units.count;
		Actor[] array = visible_units.array;
		for (int i = 0; i < count; i++)
		{
			Actor actor = array[i];
			if (actorRenderData.has_normal_render[i] && actorRenderData.main_sprite_colored[i] == null)
			{
				Sprite val = actorRenderData.main_sprites[i];
				if (val == null)
				{
					val = actor.calculateMainSprite();
				}
				actorRenderData.main_sprite_colored[i] = actor.calculateColoredSprite(val);
			}
		}
	}

	private void fillVisibleObjects()
	{
		prepareArray();
		Actor[] simpleArray = getSimpleArray();
		int count = Count;
		bool flag = MapBox.isRenderGameplay();
		int count2 = 0;
		int count3 = 0;
		Actor[] array = visible_units.array;
		Actor[] array2 = visible_units_alive.array;
		for (int i = 0; i < count; i++)
		{
			Actor actor = simpleArray[i];
			ActorAsset asset = actor.asset;
			TileZone zone = actor.current_tile.zone;
			if (asset.has_avatar_prefab)
			{
				visible_units_avatars.array[visible_units_avatars.count++] = actor;
			}
			if (actor.isFavorite() && !asset.hide_favorite_icon && zone.visible && !ControllableUnit.isControllingUnit(actor))
			{
				visible_units_with_favorite.array[visible_units_with_favorite.count++] = actor;
			}
			if (!zone.visible || !flag || !actor.is_visible)
			{
				continue;
			}
			array[count2++] = actor;
			if (actor.isAlive())
			{
				array2[count3++] = actor;
				if (actor.is_army_captain)
				{
					visible_units_with_banner.array[visible_units_with_banner.count++] = actor;
				}
				if (asset.render_status_effects && actor.hasAnyStatusEffectToRender())
				{
					visible_units_with_status.array[visible_units_with_status.count++] = actor;
				}
				if (actor.timestamp_session_ate_food > 0.0)
				{
					visible_units_just_ate.array[visible_units_just_ate.count++] = actor;
				}
				BehaviourActionActor action = actor.ai.action;
				if (action != null && action.socialize)
				{
					visible_units_socialize.array[visible_units_socialize.count++] = actor;
				}
				else if (actor.is_forced_socialize_icon && !actor.is_moving && !actor.isLying() && actor.isAttackReady() && Date.getMonthsSince(actor.is_forced_socialize_timestamp) < 1)
				{
					visible_units_socialize.array[visible_units_socialize.count++] = actor;
				}
			}
		}
		visible_units.count = count2;
		visible_units_alive.count = count3;
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		Bench.bench("actors", "game_total");
		checkContainer();
		_job_manager.updateBase(pElapsed);
		checkContainer();
		Bench.benchEnd("actors", "game_total", pSaveCounter: false, 0L);
	}

	private void checkOverrideUnitShooting()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!DebugConfig.isOn(DebugOption.OverrideUnitShooting) || !Input.GetMouseButtonDown(0))
		{
			return;
		}
		Vector2 mousePos = World.world.getMousePos();
		WorldTile mouseTilePos = World.world.getMouseTilePos();
		Actor actorNearCursor = World.world.getActorNearCursor();
		if (mouseTilePos == null)
		{
			return;
		}
		using IEnumerator<Actor> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Actor current = enumerator.Current;
			if (current != actorNearCursor && current.isAlive() && current.hasRangeAttack())
			{
				AttackData pData = new AttackData(current, pKingdom: current.kingdom, pInitiatorPosition: Vector2.op_Implicit(current.current_position), pHitTile: mouseTilePos, pHitPosition: Vector2.op_Implicit(mousePos), pTarget: null, pAttackType: AttackType.Weapon, pMetallicWeapon: false, pSkipShake: true, pProjectile: false, pProjectileID: current.getWeaponAsset().projectile);
				CombatActionLibrary.combat_attack_range.action(pData);
			}
		}
	}

	protected override void destroyObject(Actor pActor)
	{
		base.destroyObject(pActor);
		if (pActor.hasKingdom())
		{
			pActor.setKingdom(null);
		}
		if (pActor.hasSubspecies())
		{
			pActor.setSubspecies(null);
		}
		if (pActor.tile_target != null)
		{
			pActor.clearTileTarget();
		}
		pActor.asset.units.Remove(pActor);
		removeObject(pActor);
		_job_manager.removeObject(pActor, pActor.batch);
		if ((Object)(object)pActor.avatar != (Object)null)
		{
			Object.Destroy((Object)(object)pActor.avatar);
			pActor.avatar = null;
		}
		if (pActor.idle_loop_sound != null)
		{
			pActor.idle_loop_sound.stop();
		}
	}

	internal override void scheduleDestroyOnPlay(Actor pObject)
	{
		triggerActionsOnRemove(pObject);
		base.scheduleDestroyOnPlay(pObject);
	}

	private void triggerActionsOnRemove(Actor pActor)
	{
		foreach (ActorTrait trait in pActor.traits)
		{
			trait.action_on_object_remove?.Invoke(pActor, trait);
		}
	}

	public override void loadFromSave(List<ActorData> pList)
	{
		base.loadFromSave(pList);
		checkContainer();
	}

	public Actor evolutionEvent(Actor pTargetActor, bool pWithBiomeEffect, bool pAscension)
	{
		Subspecies subspecies = pTargetActor.subspecies;
		bool flag = false;
		Subspecies subspecies2 = null;
		string pID = pTargetActor.asset.id;
		if (subspecies.hasEvolvedIntoForm() && !pAscension)
		{
			subspecies2 = subspecies.getEvolvedInto();
			if (subspecies2 != null)
			{
				pID = subspecies2.getActorAsset().id;
			}
		}
		if (subspecies2 == null)
		{
			bool flag2 = false;
			if (pTargetActor.asset.can_evolve_into_new_species)
			{
				flag2 = subspecies.isSapient() || Randy.randomBool();
				if (flag2)
				{
					pID = pTargetActor.asset.evolution_id;
				}
			}
			if (!flag2)
			{
				Subspecies subspecies3 = World.world.subspecies.newSpecies(pTargetActor.asset, pTargetActor.current_tile, pMutation: true);
				flag = true;
				subspecies3.mutateFrom(subspecies);
				subspecies2 = subspecies3;
			}
		}
		if (subspecies2 == null)
		{
			subspecies2 = World.world.subspecies.newSpecies(AssetManager.actor_library.get(pID), pTargetActor.current_tile);
			flag = true;
			subspecies2.mutateFrom(subspecies);
		}
		if (flag)
		{
			subspecies2.addTrait("uplifted");
			subspecies2.makeSapient();
			subspecies2.data.biome_variant = subspecies.data.biome_variant;
		}
		ActorAsset asset = AssetManager.actor_library.get(pID);
		pTargetActor.setAsset(asset);
		pTargetActor.setSubspecies(subspecies2);
		subspecies2.data.parent_subspecies = subspecies.id;
		if (pAscension)
		{
			string name = subspecies2.name;
			if (!name.Contains("Ascentus"))
			{
				name += " Ascentus";
				subspecies2.setName(name);
			}
		}
		else
		{
			subspecies.setEvolutionSubspecies(subspecies2);
		}
		if (pWithBiomeEffect && Randy.randomChance(0.1f))
		{
			BiomeAsset biome = pTargetActor.current_tile.getBiome();
			if (biome != null && biome.evolution_trait_subspecies != null && biome.evolution_trait_subspecies.Count > 0)
			{
				SubspeciesTrait subspeciesTrait = AssetManager.subspecies_traits.get(biome.evolution_trait_subspecies.GetRandom());
				if (subspeciesTrait != null)
				{
					subspecies2.addTrait(subspeciesTrait);
				}
			}
		}
		pTargetActor.afterEvolutionEvents();
		return pTargetActor;
	}

	public bool cloneUnit(Actor pCloneFrom, WorldTile pTileTarget = null)
	{
		if (pCloneFrom == null)
		{
			return false;
		}
		if (!pCloneFrom.asset.can_be_cloned)
		{
			return false;
		}
		pCloneFrom.prepareForSave();
		ActorData data = pCloneFrom.data;
		string name = pCloneFrom.getName();
		ActorData actorData = new ActorData();
		ActorTool.copyImportantData(data, actorData, pCopyAge: true);
		actorData.created_time = World.world.getCurWorldTime();
		actorData.id = World.world.map_stats.getNextId("unit");
		actorData.name = name;
		actorData.custom_name = data.custom_name;
		actorData.age_overgrowth = data.getAge();
		actorData.parent_id_1 = data.id;
		pCloneFrom.increaseBirths();
		if (pTileTarget == null)
		{
			pTileTarget = pCloneFrom.current_tile.neighboursAll.GetRandom();
		}
		actorData.x = pTileTarget.x;
		actorData.y = pTileTarget.y;
		Actor actor = World.world.units.loadObject(actorData);
		actor.created_time_unscaled = Time.time;
		actor.addTrait("fragile_health");
		foreach (ActorTrait trait in pCloneFrom.getTraits())
		{
			actor.addTrait(trait);
		}
		actor.addTrait("miracle_born");
		if (!pCloneFrom.hasFamily() && pCloneFrom.asset.create_family_at_spawn)
		{
			World.world.families.newFamily(pCloneFrom, pCloneFrom.current_tile, null);
		}
		actor.data.cloneCustomDataFrom(pCloneFrom.data);
		actor.setReligion(pCloneFrom.religion);
		actor.setClan(pCloneFrom.clan);
		actor.setCulture(pCloneFrom.culture);
		actor.setSubspecies(pCloneFrom.subspecies);
		actor.joinLanguage(pCloneFrom.language);
		actor.setFamily(pCloneFrom.family);
		actor.setHealth(data.health, pClamp: false);
		actor.setMana(data.mana, pClamp: false);
		actor.setStamina(data.stamina, pClamp: false);
		actor.setHappiness(data.happiness, pClamp: false);
		actor.setNutrition(data.nutrition, pClamp: false);
		actor.addTrait("clone");
		if (data.saved_items != null)
		{
			foreach (long saved_item in data.saved_items)
			{
				Item item = World.world.items.get(saved_item);
				if (item != null)
				{
					Item item2 = World.world.items.generateItem(item.getAsset(), null, null, 1, pCloneFrom);
					item2.data.modifiers.Clear();
					item2.data.modifiers.AddRange(item.data.modifiers);
					item2.data.modifiers.Remove("eternal");
					item2.initItem();
					actor.equipment.setItem(item2, actor);
				}
			}
		}
		actor.applyRandomForce();
		if (actor.isRendered())
		{
			EffectsLibrary.spawn("fx_spawn", pTileTarget);
		}
		if (actor.asset.has_sound_spawn)
		{
			MusicBox.playSound(actor.asset.sound_spawn, pTileTarget);
		}
		return true;
	}

	public Actor createNewUnit(string pStatsID, WorldTile pTile, bool pMiracleSpawn = false, float pSpawnHeight = 0f, Subspecies pSubspecies = null, Subspecies pSubspeciesMutateFrom = null, bool pSpawnWithItems = true, bool pAdultAge = false, bool pGiveOwnerlessItems = false, bool pSapientSubspecies = false)
	{
		ActorAsset actorAsset = AssetManager.actor_library.get(pStatsID);
		if (actorAsset == null)
		{
			return null;
		}
		Actor actor = newObject();
		actor.setAsset(actorAsset);
		if (!pSubspecies.isRekt())
		{
			actor.setSubspecies(pSubspecies);
		}
		else
		{
			checkNewSpecies(actorAsset, pTile, actor, out var pClosestActor, pGlobalSearch: false, pSapientSubspecies, pSubspeciesMutateFrom);
			if (pMiracleSpawn && pClosestActor != null)
			{
				if (pClosestActor.hasCulture())
				{
					actor.setCulture(pClosestActor.culture);
				}
				if (pClosestActor.hasReligion())
				{
					actor.setReligion(pClosestActor.religion);
				}
				if (pClosestActor.hasLanguage())
				{
					actor.setLanguage(pClosestActor.language);
				}
			}
		}
		addRandomTraitFromBiomeToActor(actor, pTile);
		finalizeActor(pStatsID, actor, pTile, pSpawnHeight);
		if (pMiracleSpawn | pAdultAge)
		{
			if (pMiracleSpawn)
			{
				actor.addTrait("miracle_born");
			}
			if (actor.hasSubspecies())
			{
				actor.data.age_overgrowth = (int)Math.Ceiling(actor.subspecies.age_breeding);
			}
			else
			{
				actor.data.age_overgrowth = actorAsset.age_spawn;
			}
			if (HotkeyLibrary.isHoldingAlt())
			{
				actor.data.age_overgrowth = 0;
			}
		}
		actor.newCreature();
		if (pSpawnWithItems)
		{
			actor.generateDefaultSpawnWeapons(pGiveOwnerlessItems);
		}
		actor.clearSprites();
		return actor;
	}

	private void finalizeActor(string pStats, Actor pActor, WorldTile pTile, float pZHeight = 0f)
	{
		ActorAsset asset = AssetManager.actor_library.get(pStats);
		pActor.setAsset(asset);
		ActorData data = pActor.data;
		pActor.spawnOn(pTile, pZHeight);
		if (data.subspecies.hasValue())
		{
			pActor.setSubspecies(World.world.subspecies.get(data.subspecies));
		}
		if (data.family.hasValue())
		{
			pActor.setFamily(World.world.families.get(data.family));
		}
		if (data.language.hasValue())
		{
			pActor.setLanguage(World.world.languages.get(data.language));
		}
		if (data.plot.hasValue())
		{
			pActor.setPlot(World.world.plots.get(data.plot));
		}
		if (data.religion.hasValue())
		{
			pActor.setReligion(World.world.religions.get(data.religion));
		}
		if (data.clan.hasValue())
		{
			pActor.setClan(World.world.clans.get(data.clan));
		}
		if (data.culture.hasValue())
		{
			pActor.setCulture(World.world.cultures.get(data.culture));
		}
		if (data.army.hasValue())
		{
			pActor.setArmy(World.world.armies.get(data.army));
		}
		pActor.create();
		pActor.checkDefaultKingdom();
		pActor.checkDefaultProfession();
		pActor.updateStats();
		if (pActor.asset.can_be_killed_by_stuff)
		{
			pActor.batch.c_main_tile_action.Add(pActor);
		}
	}

	public Actor createBabyActorFromData(ActorData pData, WorldTile pTile, City pCity)
	{
		ActorAsset actorAsset = AssetManager.actor_library.get(pData.asset_id);
		Actor actor = base.loadObject(pData);
		actor.setData(pData);
		actor.created_time_unscaled = Time.time;
		finalizeActor(actorAsset.id, actor, pTile);
		return actor;
	}

	public Actor spawnNewUnitByPlayer(string pStatsID, WorldTile pTile, bool pSpawnSound = false, bool pMiracleSpawn = false, float pSpawnHeight = 6f, Subspecies pSubspecies = null)
	{
		Actor actor = spawnNewUnit(pStatsID, pTile, pSpawnSound, pMiracleSpawn, pSpawnHeight, pSubspecies, pGiveOwnerlessItems: true);
		if (actor.current_zone.hasCity() && actor.isSapient())
		{
			City city = actor.current_zone.city;
			if (!city.isNeutral() && city.isPossibleToJoin(actor))
			{
				actor.joinCity(city);
			}
		}
		return actor;
	}

	public Actor spawnNewUnit(string pActorAssetID, WorldTile pTile, bool pSpawnSound = false, bool pMiracleSpawn = false, float pSpawnHeight = 6f, Subspecies pSubspecies = null, bool pGiveOwnerlessItems = false, bool pAdultAge = false)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		bool pGiveOwnerlessItems2 = pGiveOwnerlessItems;
		Actor actor = createNewUnit(pActorAssetID, pTile, pMiracleSpawn, pSpawnHeight, pSubspecies, null, pSpawnWithItems: true, pAdultAge, pGiveOwnerlessItems2);
		if (pSpawnSound && actor.asset.has_sound_spawn)
		{
			string sound_spawn = actor.asset.sound_spawn;
			Vector2Int pos = pTile.pos;
			float pX = ((Vector2Int)(ref pos)).x;
			pos = pTile.pos;
			MusicBox.playSound(sound_spawn, pX, ((Vector2Int)(ref pos)).y);
		}
		if (actor.kingdom == null)
		{
			Kingdom kingdom = World.world.kingdoms_wild.get(actor.asset.kingdom_id_wild);
			actor.setKingdom(kingdom);
		}
		actor.setStatsDirty();
		actor.setNutrition(SimGlobals.m.nutrition_level_on_spawn);
		return actor;
	}

	private void checkNewSpecies(ActorAsset pAsset, WorldTile pTile, Actor pActor, out Actor pClosestActor, bool pGlobalSearch = false, bool pLookForSapientSubspecies = false, Subspecies pSubspeciesMutateFrom = null)
	{
		pClosestActor = null;
		if (!pAsset.can_have_subspecies)
		{
			return;
		}
		Subspecies subspecies = null;
		if (pGlobalSearch)
		{
			foreach (Subspecies subspecy in World.world.subspecies)
			{
				if (subspecy.isSpecies(pAsset.id))
				{
					subspecies = subspecy;
					break;
				}
			}
		}
		if (subspecies == null)
		{
			subspecies = World.world.subspecies.getNearbySpecies(pAsset, pTile, out var pSubspeciesActor, pLookForSapientSubspecies, pStopAtFirst: true);
			pClosestActor = pSubspeciesActor;
		}
		if (subspecies == null)
		{
			subspecies = World.world.subspecies.newSpecies(pAsset, pTile);
			if (pSubspeciesMutateFrom != null)
			{
				subspecies.mutateFrom(pSubspeciesMutateFrom);
			}
			subspecies.forceRecalcBaseStats();
		}
		pActor.setSubspecies(subspecies);
		pActor.event_full_stats = true;
		pActor.setStatsDirty();
	}

	public ActorTrait addRandomTraitFromBiomeToActor(Actor pActor, WorldTile pTile)
	{
		if (!pTile.Type.is_biome)
		{
			return null;
		}
		BiomeAsset biome_asset = pTile.Type.biome_asset;
		List<string> spawn_trait_actor = biome_asset.spawn_trait_actor;
		if (spawn_trait_actor != null && spawn_trait_actor.Count > 0 && Randy.randomBool())
		{
			string random = biome_asset.spawn_trait_actor.GetRandom();
			ActorTrait actorTrait = AssetManager.traits.get(random);
			pActor.addTrait(actorTrait);
			return actorTrait;
		}
		return null;
	}

	public override Actor loadObject(ActorData pData)
	{
		if (dict.ContainsKey(pData.id))
		{
			Debug.Log((object)("Trying to load unit with same ID, that already is loaded. " + pData.id));
			return null;
		}
		WorldTile tile = World.world.GetTile(pData.x, pData.y);
		if (tile == null)
		{
			return null;
		}
		ActorAsset actorAsset = AssetManager.actor_library.get(pData.asset_id);
		if (actorAsset == null)
		{
			return null;
		}
		int health = pData.health;
		int nutrition = pData.nutrition;
		int stamina = pData.stamina;
		int mana = pData.mana;
		Actor actor = base.loadObject(pData);
		actor.setData(pData);
		finalizeActor(actorAsset.id, actor, tile);
		if (actor.canUseItems())
		{
			actor.equipment.load(pData.saved_items, actor);
		}
		if (actor.isSapient())
		{
			actor.reloadInventory();
		}
		actor.loadFromSave();
		actor.updateStats();
		actor.setHealth(health);
		actor.setNutrition(nutrition);
		actor.setStamina(stamina);
		actor.setMana(mana);
		if (actor.asset.can_have_subspecies && !actor.hasSubspecies())
		{
			checkNewSpecies(actor.asset, actor.current_tile, actor, out var _, pGlobalSearch: true);
		}
		actor.makeWait(Randy.randomFloat(0.1f, 2f));
		return actor;
	}

	protected override void addObject(Actor pObject)
	{
		base.addObject(pObject);
		_job_manager.addNewObject(pObject);
	}

	private void clearLists()
	{
		for (int i = 0; i < _unit_visible_lists.Count; i++)
		{
			_unit_visible_lists[i].count = 0;
		}
	}

	private void prepareLists()
	{
		for (int i = 0; i < _unit_visible_lists.Count; i++)
		{
			_unit_visible_lists[i].prepare(Count);
		}
		render_data.checkSize(Count);
		checkContainer();
	}

	public override void clear()
	{
		_job_manager.clear();
		cached_sleeping_units.Clear();
		clearLists();
		checkContainer();
		scheduleDestroyAllOnWorldClear();
		checkObjectsToDestroy();
		base.clear();
	}

	public void debugJobManager(DebugTool pTool)
	{
		_job_manager.debug(pTool);
	}

	public JobManagerActors getJobManager()
	{
		return _job_manager;
	}

	public void checkSleepingUnits()
	{
		if (World.world.getWorldTimeElapsedSince(_timestamp_sleeping_units) < 10f)
		{
			return;
		}
		cached_sleeping_units.Clear();
		_timestamp_sleeping_units = World.world.getCurWorldTime();
		foreach (Status item in World.world.statuses.list.LoopRandom())
		{
			if (item.is_finished || item.asset.id != "sleeping")
			{
				continue;
			}
			Actor a = item.sim_object.a;
			if (a.isAlive())
			{
				cached_sleeping_units.Add(a);
				if (cached_sleeping_units.Count > 10)
				{
					break;
				}
			}
		}
	}
}
