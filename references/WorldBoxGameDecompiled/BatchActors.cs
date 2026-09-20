using System;
using System.Collections.Generic;
using Unity.Mathematics;

public class BatchActors : Batch<Actor>
{
	public ObjectContainer<Actor> c_main;

	public ObjectContainer<Actor> c_check_attack_target;

	public ObjectContainer<Actor> c_update_movement;

	public ObjectContainer<Actor> c_main_tile_action;

	public ObjectContainer<Actor> c_shake;

	public ObjectContainer<Actor> c_stats_dirty;

	public ObjectContainer<Actor> c_action_landed;

	public ObjectContainer<Actor> c_make_decision;

	public ObjectContainer<Actor> c_sprite_animations;

	public ObjectContainer<Actor> c_update_children;

	public ObjectContainer<Actor> c_check_enemy_target;

	public ObjectContainer<Actor> c_augmentation_effects;

	public ObjectContainer<Actor> c_events_become_adult;

	public ObjectContainer<Actor> c_events_hatched;

	public ObjectContainer<Actor> c_hovering;

	public ObjectContainer<Actor> c_pollinating;

	public ObjectContainer<Actor> c_check_deaths;

	internal List<Actor> l_parallel_update_sprites;

	public Random rnd;

	protected override void createJobs()
	{
		addJob(null, prepare, JobType.Parallel, "prepare");
		createJob(out c_main, updateParallelChecks, JobType.Parallel, "update_timers");
		addJob(c_main, updateVisibility, JobType.Parallel, "update_visibility");
		createJob(out c_stats_dirty, updateStats, JobType.Parallel, "update_stats");
		createJob(out c_events_become_adult, updateEventsBecomeAdult, JobType.Post, "update_events_become_adult");
		createJob(out c_events_hatched, updateEventsEggHatched, JobType.Post, "update_events_hatched");
		createJob(out c_action_landed, updateActionLanded, JobType.Post, "update_action_landed");
		addJob(c_main, updateNutritionDecay, JobType.Post, "update_hunger");
		addJob(c_main, u1_checkInside, JobType.Post, "u1_checkInside");
		createJob(out c_update_children, u2_updateChildren, JobType.Post, "u2_updateChildren");
		createJob(out c_sprite_animations, u3_spriteAnimation, JobType.Post, "u3_spriteAnimation");
		addJob(c_main, u4_deadCheck, JobType.Post, "u4_deadCheck");
		createJob(out c_main_tile_action, u5_curTileAction, JobType.Post, "u5_curTileAction");
		addJob(c_main, u6_checkFrozen, JobType.Post, "u6_checkFrozen");
		createJob(out c_augmentation_effects, u7_checkAugmentationEffects, JobType.Post, "u7_checkAugmentationEffects", 20);
		addJob(c_main, u8_checkUpdateTimers, JobType.Post, "u8_checkUpdateTimers");
		addJob(c_main, b1_checkUnderForce, JobType.Post, "b1_checkUnderForce");
		createJob(out c_check_attack_target, b2_checkCurrentEnemyTarget, JobType.Post, "b2_checkCurrentEnemyTarget");
		addJob(c_main, b3_findEnemyTarget, JobType.Post, "b3_findEnemyTarget", 5);
		addJob(c_main, b4_checkTaskVerifier, JobType.Post, "b4_checkTaskVerifier");
		addJob(c_main, b5_checkPathMovement, JobType.Post, "b5_checkPathMovement");
		createJob(out c_make_decision, b6_0_updateDecision, JobType.Post, "b6_0_update_decision");
		addJob(c_main, b55_updateNaturalDeaths, JobType.Post, "b55_update_natural_death", 20);
		addJob(c_main, b6_updateAI, JobType.Post, "b6_update_ai");
		createJob(out c_update_movement, u10_checkSmoothMovement, JobType.Post, "u10_checkSmoothMovement");
		createJob(out c_shake, updateShake, JobType.Post, "update_shake");
		createJob(out c_hovering, updateHovering, JobType.Post, "update_hovering");
		createJob(out c_pollinating, updatePollinating, JobType.Post, "update_pollinating");
		createJob(out c_check_deaths, updateDeathCheck, JobType.Post, "update_death");
		main = c_main;
		clearParallelResults = (JobUpdater)Delegate.Combine(clearParallelResults, new JobUpdater(clearParallelSprites));
	}

	private void updateParallelChecks()
	{
		if (check(_cur_container))
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].updateParallelChecks(_elapsed);
			}
		}
	}

	private void updateVisibility()
	{
		if (!check(_cur_container))
		{
			return;
		}
		bool flag = MapBox.isRenderGameplay();
		Actor[] array = _array;
		int count = _count;
		for (int i = 0; i < count; i++)
		{
			Actor actor = array[i];
			if (!actor.asset.has_sprite_renderer)
			{
				continue;
			}
			if (actor.isInMagnet() || actor.isInsideSomething())
			{
				actor.is_visible = false;
			}
			else if (flag)
			{
				if (actor.current_tile.zone.visible)
				{
					actor.is_visible = true;
				}
				else
				{
					actor.is_visible = false;
				}
			}
			else if (actor.asset.visible_on_minimap)
			{
				actor.is_visible = true;
			}
			else
			{
				actor.is_visible = false;
			}
		}
	}

	private void updateNutritionDecay()
	{
		if (World.world.timer_nutrition_decay > 0f || World.world.isPaused() || !check(_cur_container))
		{
			return;
		}
		bool pDoStarvationDamage = false;
		if (WorldLawLibrary.world_law_hunger.isEnabled())
		{
			pDoStarvationDamage = true;
		}
		Actor[] array = _array;
		int count = _count;
		for (int i = 0; i < count; i++)
		{
			Actor actor = array[i];
			if (actor.needsFood() && !actor.isEgg() && (!actor.hasSubspecies() || !actor.subspecies.has_trait_energy_preserver || !actor.hasStatus("sleeping")))
			{
				actor.updateNutritionDecay(pDoStarvationDamage);
			}
		}
	}

	private void updateEventsBecomeAdult()
	{
		if (check(_cur_container))
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].eventBecomeAdult();
			}
		}
	}

	private void updateEventsEggHatched()
	{
		if (check(_cur_container))
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].eventHatchFromEgg();
			}
		}
	}

	private void updateActionLanded()
	{
		if (check(_cur_container))
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].actionLanded();
			}
		}
	}

	private void updateStats()
	{
		if (check(_cur_container))
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].updateStats();
			}
		}
	}

	private void u1_checkInside()
	{
		if (check(_cur_container))
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].u1_checkInside(_elapsed);
			}
		}
	}

	private void u2_updateChildren()
	{
		if (check(_cur_container))
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].u2_updateChildren(_elapsed);
			}
		}
	}

	private void u3_spriteAnimation()
	{
		if (check(_cur_container))
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].u3_spriteAnimation(_elapsed);
			}
		}
	}

	private void u4_deadCheck()
	{
		if (check(_cur_container))
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].u4_deadCheck(_elapsed);
			}
		}
	}

	private void u5_curTileAction()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].u5_curTileAction();
			}
		}
	}

	private void u5_checkTileDeath()
	{
		if (check(_cur_container))
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].checkDieOnGroundBoat();
			}
		}
	}

	private void u6_checkFrozen()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].u6_checkFrozen(_elapsed);
			}
		}
	}

	private void u7_checkAugmentationEffects()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].u7_checkAugmentationEffects();
			}
		}
	}

	private void u8_checkUpdateTimers()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].u8_checkUpdateTimers(_elapsed);
			}
		}
	}

	private void b1_checkUnderForce()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].b1_checkUnderForce(_elapsed);
			}
		}
	}

	private void b2_checkCurrentEnemyTarget()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].b2_checkCurrentEnemyTarget(_elapsed);
			}
		}
	}

	private void b3_findEnemyTarget()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].b3_findEnemyTarget(_elapsed);
			}
		}
	}

	private void b4_checkTaskVerifier()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].b4_checkTaskVerifier(_elapsed);
			}
		}
	}

	private void b5_checkPathMovement()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].b5_checkPathMovement(_elapsed);
			}
		}
	}

	private void b6_0_updateDecision()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].b6_0_updateDecision(_elapsed);
			}
			_cur_container.Clear();
		}
	}

	private void b55_updateNaturalDeaths()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].b55_updateNaturalDeaths(_elapsed);
			}
		}
	}

	private void b6_updateAI()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].b6_updateAI(_elapsed);
			}
		}
	}

	private void u10_checkSmoothMovement()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].u10_checkSmoothMovement(_elapsed);
			}
		}
	}

	private void updateShake()
	{
		if (check(_cur_container))
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].updateShake(_elapsed);
			}
			_cur_container.checkAddRemove();
		}
	}

	private void updateHovering()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].updateHover(_elapsed);
			}
		}
	}

	private void updatePollinating()
	{
		if (check(_cur_container) && !World.world.isPaused())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].updatePollinate(_elapsed);
			}
		}
	}

	private void updateDeathCheck()
	{
		if (check(_cur_container) && !World.world.isWindowOnScreen())
		{
			Actor[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].checkDeath();
			}
			_cur_container.Clear();
		}
	}

	internal override void clear()
	{
		base.clear();
		clearParallelResults?.Invoke();
	}

	private void clearParallelSprites()
	{
		l_parallel_update_sprites.Clear();
	}

	internal override void add(Actor pActor)
	{
		base.add(pActor);
		pActor.batch = this;
	}

	internal override void remove(Actor pObject)
	{
		base.remove(pObject);
		pObject.batch = null;
	}

	public BatchActors()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		l_parallel_update_sprites = new List<Actor>();
		rnd = new Random(10u);
		base._002Ector();
	}
}
