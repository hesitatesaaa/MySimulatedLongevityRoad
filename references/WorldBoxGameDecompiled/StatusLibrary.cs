using System;
using System.Collections.Generic;
using Beebyte.Obfuscator;
using UnityEngine;

[ObfuscateLiterals]
public class StatusLibrary : AssetLibrary<StatusAsset>
{
	public const float DURATION_STRANGE_URGE = 100f;

	private string[] _pot_dreams = new string[23]
	{
		"had_bad_dream", "had_good_dream", "had_nightmare", null, null, null, null, null, null, null,
		null, null, null, null, null, null, null, null, null, null,
		null, null, null
	};

	public Vector2 mouse_pos
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return ControllableUnit.getClickVector();
		}
	}

	public override void init()
	{
		base.init();
		add(new StatusAsset
		{
			id = "handsome_migrant",
			locale_id = "status_title_handsome_migrant",
			locale_description = "status_description_handsome_migrant",
			duration = 360f,
			path_icon = "ui/Icons/iconStatisticsGoodLookingMigrants"
		});
		add(new StatusAsset
		{
			id = "recovery_plot",
			locale_id = "status_title_recovery_plot",
			locale_description = "status_description_recovery_plot",
			duration = 60f,
			path_icon = "ui/Icons/iconRecoveryPlot"
		});
		add(new StatusAsset
		{
			id = "voices_in_my_head",
			locale_id = "status_title_voices_in_my_head",
			locale_description = "status_description_voices_in_my_head",
			duration = 180f,
			path_icon = "ui/Icons/iconVoicesInMyHead"
		});
		t.base_stats["diplomacy"] = -5f;
		t.base_stats["personality_rationality"] = -0.3f;
		t.base_stats["opinion"] = -5f;
		add(new StatusAsset
		{
			id = "recovery_spell",
			locale_id = "status_title_recovery_spell",
			locale_description = "status_description_recovery_spell",
			duration = 5f,
			path_icon = "ui/Icons/iconRecoverySpell"
		});
		add(new StatusAsset
		{
			id = "recovery_social",
			locale_id = "status_title_recovery_social",
			locale_description = "status_description_recovery_social",
			duration = 60f,
			path_icon = "ui/Icons/iconRecoverySocial"
		});
		add(new StatusAsset
		{
			id = "recovery_combat_action",
			locale_id = "status_title_recovery_combat_action",
			locale_description = "status_description_recovery_combat_action",
			duration = 1f,
			path_icon = "ui/Icons/iconRecoveryCombatAction"
		});
		add(new StatusAsset
		{
			id = "starving",
			locale_id = "status_title_starvation",
			locale_description = "status_description_starvation",
			duration = 60f,
			path_icon = "ui/Icons/iconHungry",
			action_on_receive = (BaseSimObject pActor, WorldTile _) => pActor.a.changeHappiness("starving"),
			action_interval = 5f,
			action = delegate(BaseSimObject pObject, WorldTile pTile)
			{
				Actor actor = pObject.a;
				if (!actor.isAlive())
				{
					return false;
				}
				if (!actor.hasCity())
				{
					return false;
				}
				if (actor.isFighting())
				{
					return false;
				}
				City city = actor.city;
				if (actor.hasTask() && actor.ai.task.diet)
				{
					return false;
				}
				if (!city.hasSuitableFood(actor.subspecies))
				{
					actor.cancelAllBeh();
					return false;
				}
				actor.setTask("try_to_eat_city_food");
				return true;
			}
		});
		t.tier = StatusTier.Advanced;
		add(new StatusAsset
		{
			id = "drowning",
			locale_id = "status_title_drowning",
			locale_description = "status_description_drowning",
			duration = 1f,
			path_icon = "ui/Icons/iconDrowning",
			action_interval = 0.5f,
			action = delegate(BaseSimObject pObject, WorldTile pTile)
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				Actor actor = pObject.a;
				if (!actor.isAlive())
				{
					return false;
				}
				actor.getHit(1f, pFlash: true, AttackType.Drowning);
				EffectsLibrary.spawnAt("fx_drowning", actor.current_position, 0.1f);
				return true;
			},
			action_death = delegate(BaseSimObject pObject, WorldTile pTile)
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				Actor actor = pObject.a;
				EffectsLibrary.spawnAt("fx_drowning", actor.current_position, 0.1f);
				return true;
			}
		});
		t.tier = StatusTier.Advanced;
		t.base_stats.addTag("ignore_fights");
		add(new StatusAsset
		{
			id = "sleeping",
			render_priority = 8,
			locale_id = "status_title_sleeping",
			locale_description = "status_description_sleeping",
			duration = 60f,
			animated = true,
			is_animated_in_pause = true,
			can_be_flipped = false,
			use_parent_rotation = false,
			removed_on_damage = true,
			texture = "fx_status_sleeping_t",
			path_icon = "ui/Icons/iconSleep",
			action_finish = delegate(BaseSimObject pActor, WorldTile _)
			{
				Actor actor = pActor.a;
				string random = _pot_dreams.GetRandom();
				if (!string.IsNullOrEmpty(random))
				{
					pActor.a.addStatusEffect(random);
				}
				actor.restoreStaminaPercent(0.2f);
				return pActor.a.changeHappiness("just_slept");
			}
		});
		t.remove_status = AssetLibrary<StatusAsset>.a<string>("tantrum", "surprised");
		t.tier = StatusTier.Advanced;
		t.base_stats.addTag("unconscious");
		add(new StatusAsset
		{
			id = "laughing",
			render_priority = 4,
			locale_id = "status_title_laughing",
			locale_description = "status_description_laughing",
			duration = 60f,
			animated = true,
			is_animated_in_pause = true,
			can_be_flipped = false,
			use_parent_rotation = false,
			removed_on_damage = true,
			texture = "fx_status_laughing_t",
			path_icon = "ui/Icons/iconLaughing",
			action_on_receive = delegate(BaseSimObject pActor, WorldTile _)
			{
				pActor.a.playIdleSound();
				return true;
			}
		});
		t.tier = StatusTier.Advanced;
		add(new StatusAsset
		{
			id = "singing",
			render_priority = 4,
			locale_id = "status_title_singing",
			locale_description = "status_description_singing",
			duration = 60f,
			animated = true,
			is_animated_in_pause = true,
			can_be_flipped = false,
			use_parent_rotation = false,
			removed_on_damage = true,
			texture = "fx_status_singing_t",
			path_icon = "ui/Icons/iconSinging",
			action_on_receive = delegate(BaseSimObject pActor, WorldTile _)
			{
				pActor.a.playIdleSound();
				return true;
			}
		});
		t.tier = StatusTier.Advanced;
		add(new StatusAsset
		{
			id = "swearing",
			render_priority = 4,
			locale_id = "status_title_swearing",
			locale_description = "status_description_swearing",
			duration = 60f,
			animated = true,
			is_animated_in_pause = true,
			can_be_flipped = false,
			use_parent_rotation = false,
			removed_on_damage = true,
			texture = "fx_status_swearing_t",
			path_icon = "ui/Icons/iconSwearing",
			action_on_receive = delegate(BaseSimObject pActor, WorldTile _)
			{
				pActor.a.playIdleSound();
				return true;
			}
		});
		t.opposite_traits = AssetLibrary<StatusAsset>.a<string>("peaceful");
		t.tier = StatusTier.Advanced;
		t.base_stats.addTag("moody");
		add(new StatusAsset
		{
			id = "crying",
			render_priority = 4,
			locale_id = "status_title_crying",
			locale_description = "status_description_crying",
			duration = 60f,
			animated = true,
			is_animated_in_pause = true,
			can_be_flipped = false,
			use_parent_rotation = false,
			removed_on_damage = true,
			texture = "fx_status_crying_t",
			path_icon = "ui/Icons/iconCrying",
			action_on_receive = delegate(BaseSimObject pActor, WorldTile _)
			{
				pActor.a.playIdleSound();
				return true;
			}
		});
		t.tier = StatusTier.Advanced;
		t.base_stats.addTag("moody");
		add(new StatusAsset
		{
			id = "possessed",
			locale_id = "status_title_possessed",
			locale_description = "status_description_possessed",
			duration = 10f,
			animated = true,
			path_icon = "ui/Icons/iconPossessed",
			action_on_receive = (BaseSimObject pActor, WorldTile _) => pActor.a.changeHappiness("just_possessed"),
			action_interval = 0f,
			action = possessedAction
		});
		add(new StatusAsset
		{
			id = "possessed_follower",
			locale_id = "status_title_possessed_follower",
			locale_description = "status_description_possessed_follower",
			duration = 200f,
			affects_mind = true,
			animated = true,
			is_animated_in_pause = true,
			texture = "fx_status_possessed_follower_t",
			path_icon = "ui/Icons/iconPossessed",
			decision_id = "possessed_following"
		});
		add(new StatusAsset
		{
			id = "strange_urge",
			locale_id = "status_title_strange_urge",
			locale_description = "status_description_strange_urge",
			duration = 100f,
			animated = true,
			path_icon = "ui/Icons/iconStrangeUrge",
			action_on_receive = (BaseSimObject pActor, WorldTile _) => !Randy.randomChance(0.7f) && pActor.a.changeHappiness("strange_urge")
		});
		add(new StatusAsset
		{
			id = "tantrum",
			locale_id = "status_title_tantrum",
			locale_description = "status_description_tantrum",
			duration = 120f,
			affects_mind = true,
			path_icon = "ui/Icons/iconTantrum",
			action_finish = (BaseSimObject pActor, WorldTile _) => pActor.a.changeHappiness("just_had_tantrum"),
			decision_id = "do_tantrum"
		});
		add(new StatusAsset
		{
			id = "egg",
			locale_id = "status_egg",
			locale_description = "status_description_egg",
			duration = 5f,
			animated = false,
			path_icon = "ui/Icons/iconEgg",
			action_finish = delegate(BaseSimObject pActor, WorldTile _)
			{
				pActor.a.makeStunned();
				if (pActor.a.isRendered())
				{
					EffectsLibrary.spawn("fx_spawn", pActor.current_tile, null, null, 0f, pActor.current_position.x, pActor.current_position.y);
				}
				return true;
			}
		});
		t.base_stats.addTag("immovable");
		t.base_stats.addTag("frozen_ai");
		t.base_stats["armor"] = 10f;
		add(new StatusAsset
		{
			id = "cursed",
			locale_id = "status_title_cursed",
			locale_description = "status_description_cursed",
			duration = 300f,
			animated = false,
			path_icon = "ui/Icons/iconCursed",
			can_be_cured = true,
			action_on_receive = (BaseSimObject pActor, WorldTile _) => pActor.a.changeHappiness("just_cursed")
		});
		t.opposite_traits = AssetLibrary<StatusAsset>.a<string>("evil");
		StatusAsset statusAsset = t;
		statusAsset.action_death = (WorldAction)Delegate.Combine(statusAsset.action_death, new WorldAction(ActionLibrary.turnIntoSkeleton));
		t.base_stats["loyalty_traits"] = -100f;
		t.base_stats["multiplier_damage"] = -0.5f;
		t.base_stats["multiplier_health"] = -0.5f;
		t.base_stats["multiplier_speed"] = -0.2f;
		t.base_stats["multiplier_diplomacy"] = -0.9f;
		t.base_stats["lifespan"] = -10f;
		t.base_stats["multiplier_offspring"] = -2f;
		add(new StatusAsset
		{
			id = "spell_silence",
			locale_id = "status_spell_silence",
			locale_description = "status_description_spell_silence",
			duration = 60f,
			animated = false,
			path_icon = "ui/Icons/iconSpellSilence"
		});
		add(new StatusAsset
		{
			id = "spell_boost",
			locale_id = "status_spell_boost",
			locale_description = "status_description_spell_boost",
			duration = 180f,
			animated = false,
			path_icon = "ui/Icons/iconSpellBoost"
		});
		t.base_stats["mana"] = 100f;
		t.base_stats["skill_spell"] = 0.2f;
		add(new StatusAsset
		{
			id = "inspired",
			locale_id = "status_title_inspired",
			locale_description = "status_description_inspired",
			duration = 60f,
			animated = false,
			path_icon = "ui/Icons/iconInspired",
			action_finish = (BaseSimObject pActor, WorldTile _) => pActor.a.changeHappiness("just_inspired")
		});
		t.base_stats["multiplier_speed"] = 0.3f;
		t.base_stats["multiplier_crit"] = 0.3f;
		t.base_stats["multiplier_attack_speed"] = 0.3f;
		add(new StatusAsset
		{
			id = "confused",
			locale_id = "status_title_confused",
			locale_description = "status_description_confused",
			duration = 10f,
			animated = false,
			affects_mind = true,
			path_icon = "ui/Icons/iconConfused",
			decision_id = "status_confused"
		});
		t.opposite_status = AssetLibrary<StatusAsset>.a<string>("egg");
		add(new StatusAsset
		{
			id = "soul_harvested",
			locale_id = "status_title_soul_harvested",
			locale_description = "status_description_soul_harvested",
			duration = 666f,
			animated = false,
			path_icon = "ui/Icons/iconSoulHarvested",
			decision_id = "status_soul_harvested"
		});
		add(new StatusAsset
		{
			id = "magnetized",
			locale_id = "status_title_magnetized",
			locale_description = "status_description_magnetized",
			duration = 4f,
			animated = false,
			path_icon = "ui/Icons/iconMagnetized",
			action_on_receive = (BaseSimObject pSelf, WorldTile _) => pSelf.a.changeHappiness("just_magnetised")
		});
		add(new StatusAsset
		{
			id = "rage",
			locale_id = "status_title_rage",
			locale_description = "status_description_rage",
			duration = 300f,
			animated = false,
			affects_mind = true,
			path_icon = "ui/Icons/iconRage"
		});
		t.base_stats["multiplier_damage"] = 1f;
		add(new StatusAsset
		{
			id = "surprised",
			render_priority = 9,
			locale_id = "status_title_surprised",
			locale_description = "status_description_surprised",
			duration = 2f,
			texture = "fx_status_surprised_t",
			animated = true,
			is_animated_in_pause = true,
			loop = false,
			use_parent_rotation = false,
			path_icon = "ui/Icons/iconSurprised",
			offset_y = 1f,
			offset_y_ui = 0.65f,
			action_on_receive = (BaseSimObject pSelf, WorldTile _) => pSelf.a.changeHappiness("just_surprised")
		});
		add(new StatusAsset
		{
			id = "on_guard",
			render_priority = 8,
			locale_id = "status_title_on_guard",
			locale_description = "status_description_on_guard",
			duration = 60f,
			use_parent_rotation = false,
			path_icon = "ui/Icons/iconOnGuard"
		});
		add(new StatusAsset
		{
			id = "angry",
			locale_id = "status_title_angry",
			locale_description = "status_description_angry",
			duration = 60f,
			use_parent_rotation = false,
			path_icon = "ui/Icons/iconAngry",
			animated = true,
			is_animated_in_pause = true,
			texture = "fx_status_angry_t",
			action_finish = delegate(BaseSimObject pActor, WorldTile _)
			{
				pActor.a.finishAngryStatus();
				return true;
			}
		});
		t.opposite_traits = AssetLibrary<StatusAsset>.a<string>("peaceful");
		add(new StatusAsset
		{
			id = "just_ate",
			locale_id = "status_title_just_ate",
			locale_description = "status_description_just_ate",
			duration = 200f,
			path_icon = "ui/Icons/iconHunger",
			decision_id = "try_to_poop"
		});
		t.remove_status = AssetLibrary<StatusAsset>.a<string>("starving");
		add(new StatusAsset
		{
			id = "festive_spirit",
			locale_id = "status_title_festive_spirit",
			locale_description = "status_description_festive_spirit",
			duration = 100f,
			path_icon = "ui/Icons/iconFireworks",
			decision_id = "try_to_launch_fireworks"
		});
		add(new StatusAsset
		{
			id = "being_suspicious",
			locale_id = "status_title_being_suspicious",
			locale_description = "status_description_being_suspicious",
			duration = 20f,
			path_icon = "ui/Icons/iconSuspicious",
			decision_id = "run_away_being_sus"
		});
		add(new StatusAsset
		{
			id = "had_good_dream",
			locale_id = "status_title_had_good_dream",
			locale_description = "status_description_had_good_dream",
			duration = 90f,
			path_icon = "ui/Icons/iconDreamGood",
			action_on_receive = (BaseSimObject pSelf, WorldTile _) => pSelf.a.changeHappiness("had_good_dream")
		});
		add(new StatusAsset
		{
			id = "had_bad_dream",
			locale_id = "status_title_had_bad_dream",
			locale_description = "status_description_had_bad_dream",
			duration = 90f,
			path_icon = "ui/Icons/iconDreamBad",
			action_on_receive = (BaseSimObject pSelf, WorldTile _) => pSelf.a.changeHappiness("had_bad_dream")
		});
		add(new StatusAsset
		{
			id = "had_nightmare",
			locale_id = "status_title_had_nightmare",
			locale_description = "status_description_had_nightmare",
			duration = 90f,
			path_icon = "ui/Icons/iconDreamNightmare",
			action_on_receive = (BaseSimObject pSelf, WorldTile _) => pSelf.a.changeHappiness("had_nightmare")
		});
		add(new StatusAsset
		{
			id = "stunned",
			render_priority = 10,
			locale_id = "status_title_stunned",
			locale_description = "status_description_stunned",
			duration = 4f,
			texture = "fx_status_stunned_t",
			animated = true,
			is_animated_in_pause = true,
			use_parent_rotation = false,
			path_icon = "ui/Icons/iconStunned"
		});
		t.remove_status = AssetLibrary<StatusAsset>.a<string>("sleeping", "tantrum", "surprised", "singing", "laughing", "being_suspicious");
		t.tier = StatusTier.Advanced;
		t.opposite_traits = AssetLibrary<StatusAsset>.a<string>("tough");
		t.opposite_status = AssetLibrary<StatusAsset>.a<string>("egg");
		t.base_stats.addTag("unconscious");
		add(new StatusAsset
		{
			id = "afterglow",
			locale_id = "status_title_afterglow",
			locale_description = "status_description_afterglow",
			duration = 90f,
			animated = true,
			is_animated_in_pause = true,
			use_parent_rotation = false,
			path_icon = "ui/Icons/iconAfterglow",
			offset_y = 1f
		});
		add(new StatusAsset
		{
			id = "fell_in_love",
			locale_id = "status_title_fell_in_love",
			locale_description = "status_description_fell_in_love",
			duration = 180f,
			use_parent_rotation = false,
			path_icon = "ui/Icons/iconLovers",
			offset_y = 1f,
			action_interval = 2f,
			action_on_receive = (BaseSimObject pSelf, WorldTile _) => pSelf.a.changeHappiness("fallen_in_love"),
			action = delegate(BaseSimObject pTarget, WorldTile _)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				EffectsLibrary.spawnAt("fx_hearts", pTarget.current_position, pTarget.current_scale.y);
				return true;
			}
		});
		add(new StatusAsset
		{
			id = "pregnant",
			locale_id = "status_title_pregnant",
			locale_description = "status_description_pregnant",
			duration = 45f,
			path_icon = "ui/Icons/iconPregnant",
			animated = true,
			is_animated_in_pause = true,
			use_parent_rotation = false,
			texture = "fx_status_pregnant_t",
			action_finish = actionPregnancyFinish
		});
		t.base_stats["multiplier_speed"] = -0.2f;
		add(new StatusAsset
		{
			id = "pregnant_parthenogenesis",
			locale_id = "status_title_pregnant",
			locale_description = "status_description_pregnant",
			duration = 45f,
			path_icon = "ui/Icons/iconPregnant",
			animated = true,
			is_animated_in_pause = true,
			use_parent_rotation = false,
			texture = "fx_status_pregnant_t",
			action_finish = actionPregnancyParthenogenesisFinish
		});
		t.base_stats["multiplier_speed"] = -0.2f;
		add(new StatusAsset
		{
			id = "powerup",
			locale_id = "status_title_powerup",
			locale_description = "status_description_powerup",
			duration = 300f,
			animated = true,
			path_icon = "ui/Icons/iconPowerup"
		});
		t.draw_light_area = true;
		t.draw_light_size = 0.05f;
		t.base_stats["armor"] = 1f;
		t.base_stats["damage"] = 5f;
		t.base_stats["multiplier_damage"] = 0.5f;
		t.base_stats["multiplier_crit"] = 0.5f;
		t.base_stats["scale"] = 0.1f;
		add(new StatusAsset
		{
			id = "enchanted",
			locale_id = "status_title_enchanted",
			locale_description = "status_description_enchanted",
			duration = 120f,
			path_icon = "ui/Icons/iconEnchanted",
			action_on_receive = (BaseSimObject pSelf, WorldTile _) => pSelf.a.changeHappiness("just_enchanted")
		});
		t.draw_light_area = true;
		t.draw_light_size = 0.05f;
		t.base_stats["multiplier_damage"] = 0.77f;
		t.base_stats["multiplier_speed"] = 0.1f;
		t.base_stats["multiplier_diplomacy"] = 0.1f;
		t.base_stats["multiplier_crit"] = 0.1f;
		t.base_stats["lifespan"] = 10f;
		add(new StatusAsset
		{
			id = "slowness",
			locale_id = "status_title_slowness",
			locale_description = "status_description_slowness",
			texture = "fx_status_slowness_t",
			duration = 30f,
			animated = true,
			path_icon = "ui/Icons/iconSlowness"
		});
		t.base_stats["speed"] = -100f;
		t.remove_status = AssetLibrary<StatusAsset>.a<string>("caffeinated");
		add(new StatusAsset
		{
			id = "motivated",
			locale_id = "status_title_motivated",
			locale_description = "status_description_motivated",
			texture = "fx_status_motivated_t",
			duration = 120f,
			animated = true,
			is_animated_in_pause = true,
			path_icon = "ui/Icons/iconMotivated"
		});
		t.base_stats["speed"] = 10f;
		t.base_stats["attack_speed"] = 2f;
		add(new StatusAsset
		{
			id = "cough",
			locale_id = "status_title_cough",
			locale_description = "status_description_cough",
			duration = 200f,
			path_icon = "ui/Icons/iconCough",
			can_be_cured = true
		});
		t.opposite_traits = AssetLibrary<StatusAsset>.a<string>("immune");
		t.base_stats["lifespan"] = -15f;
		t.base_stats["multiplier_speed"] = -0.1f;
		t.base_stats["multiplier_health"] = -0.1f;
		t.tier = StatusTier.Advanced;
		add(new StatusAsset
		{
			id = "ash_fever",
			locale_id = "status_title_ash_fever",
			locale_description = "status_description_ash_fever",
			duration = 500f,
			path_icon = "ui/Icons/iconAshFever",
			action = ashFeverEffect,
			action_interval = 10f,
			can_be_cured = true
		});
		t.opposite_traits = AssetLibrary<StatusAsset>.a<string>("immune");
		t.tier = StatusTier.Advanced;
		t.base_stats["diplomacy"] = -5f;
		t.base_stats["intelligence"] = -5f;
		t.base_stats["stewardship"] = -5f;
		t.base_stats["warfare"] = 5f;
		t.base_stats["multiplier_speed"] = -0.1f;
		t.base_stats["multiplier_health"] = -0.6f;
		t.base_stats["lifespan"] = -45f;
		add(new StatusAsset
		{
			id = "caffeinated",
			locale_id = "status_title_caffeinated",
			locale_description = "status_description_caffeinated",
			texture = "fx_status_caffeinated_t",
			duration = 60f,
			animated = true,
			is_animated_in_pause = true,
			path_icon = "ui/Icons/iconCoffee"
		});
		t.base_stats["intelligence"] = 222f;
		t.base_stats["speed"] = 200f;
		t.remove_status = AssetLibrary<StatusAsset>.a<string>("frozen");
		add(new StatusAsset
		{
			id = "frozen",
			locale_id = "status_title_frozen",
			locale_description = "status_description_frozen",
			texture = "fx_status_frozen_t",
			duration = 15f,
			allow_timer_reset = false,
			random_frame = true,
			sound_idle = "event:/SFX/STATUS/StatusFrozen",
			path_icon = "ui/Icons/iconFrozen"
		});
		t.base_stats["mass"] = 100f;
		t.base_stats["armor"] = -20f;
		t.base_stats["speed"] = -10000f;
		t.base_stats.addTag("immovable");
		t.base_stats.addTag("frozen_ai");
		t.base_stats.addTag("stop_idle_animation");
		t.remove_status = AssetLibrary<StatusAsset>.a<string>("burning", "tantrum");
		t.opposite_status = AssetLibrary<StatusAsset>.a<string>("shield");
		t.opposite_traits = AssetLibrary<StatusAsset>.a<string>("freeze_proof");
		t.opposite_tags = AssetLibrary<StatusAsset>.a<string>("immunity_cold");
		t.tier = StatusTier.Advanced;
		add(new StatusAsset
		{
			id = "shield",
			locale_id = "status_title_shield",
			locale_description = "status_description_shield",
			texture = "fx_status_shield_t",
			duration = 60f,
			animated = true,
			is_animated_in_pause = true,
			sound_idle = "event:/SFX/STATUS/StatusShield",
			path_icon = "ui/Icons/iconBubbleShield"
		});
		t.draw_light_area = true;
		t.draw_light_size = 0.1f;
		t.base_stats["mass"] = 100f;
		t.base_stats["armor"] = 90f;
		t.remove_status = AssetLibrary<StatusAsset>.a<string>("burning");
		t.opposite_status = AssetLibrary<StatusAsset>.a<string>("frozen");
		StatusAsset statusAsset2 = t;
		statusAsset2.action_get_hit = (GetHitAction)Delegate.Combine(statusAsset2.action_get_hit, new GetHitAction(spawnShieldHitEffect));
		t.tier = StatusTier.Advanced;
		add(new StatusAsset
		{
			id = "burning",
			locale_id = "status_title_burning",
			locale_description = "status_description_burning",
			texture = "fx_status_burning_t",
			duration = 30f,
			render_priority = 100,
			allow_timer_reset = false,
			action = burningEffect,
			action_interval = 2f,
			animated = true,
			animation_speed = 0.1f,
			animation_speed_random = 0.08f,
			random_frame = true,
			random_flip = true,
			cancel_actor_job = true,
			material_id = "mat_world_object_lit",
			draw_light_area = true,
			draw_light_size = 0.2f,
			sound_idle = "event:/SFX/STATUS/StatusBurningBuilding",
			path_icon = "ui/Icons/iconFire",
			decision_id = "run_to_water_when_on_fire"
		});
		t.opposite_status = AssetLibrary<StatusAsset>.a<string>("shield", "tantrum");
		t.opposite_traits = AssetLibrary<StatusAsset>.a<string>("fire_proof");
		t.opposite_tags = AssetLibrary<StatusAsset>.a<string>("immunity_fire");
		t.remove_status = AssetLibrary<StatusAsset>.a<string>("frozen");
		t.tier = StatusTier.Advanced;
		add(new StatusAsset
		{
			id = "poisoned",
			locale_id = "status_title_poisoned",
			locale_description = "status_description_poisoned",
			duration = 90f,
			allow_timer_reset = false,
			action = poisonedEffect,
			action_interval = 1f,
			path_icon = "ui/Icons/iconPoisoned"
		});
		t.tier = StatusTier.Advanced;
		t.opposite_traits = AssetLibrary<StatusAsset>.a<string>("poison_immune");
		add(new StatusAsset
		{
			id = "invincible",
			locale_id = "status_title_invincible",
			locale_description = "status_description_invincible",
			duration = 5f,
			path_icon = "ui/Icons/iconInvincible"
		});
		add(new StatusAsset
		{
			id = "dodge",
			locale_id = "status_title_dodge",
			locale_description = "status_description_dodge",
			duration = 1f,
			path_icon = "ui/Icons/skills/iconSkillDodge"
		});
		add(new StatusAsset
		{
			id = "dash",
			locale_id = "status_title_dash",
			locale_description = "status_description_dash",
			duration = 2f,
			path_icon = "ui/Icons/skills/iconSkillDash"
		});
		t.base_stats["multiplier_speed"] = 1.55f;
		add(new StatusAsset
		{
			id = "taking_roots",
			locale_id = "status_title_taking_roots",
			locale_description = "status_description_taking_roots",
			sound_idle = "event:/SFX/NATURE/TreeFall",
			duration = 120f,
			path_icon = "ui/Icons/iconTakingRoots",
			texture = "fx_status_taking_roots_t",
			animated = true,
			use_parent_rotation = false
		});
		t.base_stats["mass"] = 100f;
		t.base_stats["armor"] = 10f;
		t.base_stats["speed"] = -10000f;
		t.base_stats["multiplier_speed"] = -0.3f;
		t.base_stats["multiplier_damage"] = -0.3f;
		t.base_stats["multiplier_health"] = -0.3f;
		t.base_stats.addTag("immovable");
		t.base_stats.addTag("stop_idle_animation");
		t.base_stats.addTag("frozen_ai");
		t.tier = StatusTier.Advanced;
		StatusAsset statusAsset3 = t;
		statusAsset3.action_finish = (WorldAction)Delegate.Combine(statusAsset3.action_finish, new WorldAction(actionTakingRootsFinish));
		add(new StatusAsset
		{
			id = "uprooting",
			locale_id = "status_title_uprooting",
			locale_description = "status_description_uprooting",
			sound_idle = "event:/SFX/CIVILIZATIONS/CropsGrow",
			duration = 120f,
			path_icon = "ui/Icons/iconUprooting",
			texture = "fx_status_uprooting_t",
			animated = true,
			use_parent_rotation = false,
			offset_y_ui = 0.1f
		});
		t.base_stats["armor"] = 20f;
		t.base_stats["speed"] = -10000f;
		t.base_stats["multiplier_speed"] = -0.3f;
		t.base_stats["multiplier_damage"] = -0.3f;
		t.base_stats["multiplier_health"] = -0.3f;
		t.base_stats.addTag("immovable");
		t.base_stats.addTag("frozen_ai");
		t.base_stats.addTag("stop_idle_animation");
		t.tier = StatusTier.Advanced;
		add(new StatusAsset
		{
			id = "budding",
			locale_id = "status_title_budding",
			locale_description = "status_description_budding",
			duration = 60f,
			path_icon = "ui/Icons/iconStatusBudding",
			animated = true,
			animation_speed = 0f,
			scale = 0.7f,
			position_z = -0.01f,
			rotation_z = -30f,
			use_parent_rotation = true,
			get_override_sprite = delegate(BaseSimObject pActor, int pIndex)
			{
				Sprite pMainSprite = pActor.a.animation_container.walking.frames[0];
				return pActor.a.calculateColoredSprite(pMainSprite, pUpdateFrameData: false);
			},
			get_override_sprite_position = delegate(BaseSimObject pActor, int pIndex)
			{
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_0065: Unknown result type (might be due to invalid IL or missing references)
				//IL_006a: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00be: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
				AnimationFrameData animationFrameData = pActor.a.getAnimationFrameData();
				ref Vector3 current_scale = ref pActor.a.current_scale;
				if (animationFrameData.show_head)
				{
					ref Vector2 pos_head_new = ref animationFrameData.pos_head_new;
					float scaleMod = pActor.a.getScaleMod();
					return Vector2.op_Implicit(new Vector2(-0.4f * scaleMod + pos_head_new.x * current_scale.x, -0.25f * scaleMod + pos_head_new.y * current_scale.y));
				}
				float num = Mathf.Max((float)pActor.a.asset.actor_size, 3f) / 13f;
				float num2 = ((!pActor.a.isInLiquid()) ? 5f : 1f);
				return Vector2.op_Implicit(new Vector2(0f, num2 * num * current_scale.y));
			},
			get_override_sprite_rotation_z = (BaseSimObject pActor, int pIndex) => pActor.a.has_rendered_sprite_head ? (-30f) : 0f,
			get_override_sprite_ui = delegate(AvatarEffect pEffect, int pIndex)
			{
				UnitAvatarLoader avatar = pEffect.getAvatar();
				ActorAvatarData data = avatar.getData();
				AnimationContainerUnit animationContainer = avatar.getAnimationContainer();
				Sprite pSprite = animationContainer.walking.frames[0];
				return data.getColoredSprite(pSprite, animationContainer);
			},
			get_override_sprite_position_ui = delegate(AvatarEffect pEffect, int pIndex)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
				//IL_0100: Unknown result type (might be due to invalid IL or missing references)
				UnitAvatarLoader avatar = pEffect.getAvatar();
				ActorAvatarData data = avatar.getData();
				Vector2 val = default(Vector2);
				int actualSpriteIndex = avatar.getActualSpriteIndex();
				if (data.hasRenderedSpriteHead())
				{
					AnimationContainerUnit animationContainer = avatar.getAnimationContainer();
					string key = ((data.is_touching_liquid && animationContainer.has_swimming) ? ((Object)animationContainer.swimming.frames[actualSpriteIndex]).name : ((Object)animationContainer.walking.frames[actualSpriteIndex]).name);
					ref Vector2 pos_head = ref animationContainer.dict_frame_data[key].pos_head;
					ref float scale = ref pEffect.getAsset().scale;
					((Vector2)(ref val))._002Ector(-0.4f + pos_head.x * scale, -0.25f + pos_head.y * scale);
				}
				else
				{
					float num = Mathf.Max((float)data.asset.actor_size, 3f) / 13f;
					float num2 = ((!data.is_touching_liquid) ? 5f : 1f);
					((Vector2)(ref val))._002Ector(0f, num2 * num);
				}
				return Vector2.op_Implicit(val);
			},
			get_override_sprite_rotation_z_ui = (AvatarEffect pEffect, int pIndex) => pEffect.getAvatar().getData().hasRenderedSpriteHead() ? (-30f) : 0f,
			get_sprites_count = (BaseSimObject _, StatusAsset _) => 1
		});
		t.render_check = (ActorAsset pAsset) => pAsset.render_budding;
		StatusAsset statusAsset4 = t;
		statusAsset4.action_finish = (WorldAction)Delegate.Combine(statusAsset4.action_finish, new WorldAction(actionBuddingFinish));
		add(new StatusAsset
		{
			id = "flicked",
			locale_id = "status_title_flicked",
			locale_description = "status_description_flicked",
			duration = 3f,
			path_icon = "ui/Icons/iconFingerFlick",
			action_death = delegate(BaseSimObject pActor, WorldTile pTile)
			{
				if (pActor.a.getActorAsset().id != "beetle")
				{
					return false;
				}
				if (!pActor.current_tile.Type.lava)
				{
					return false;
				}
				AchievementLibrary.flick_it.check();
				return true;
			}
		});
	}

	public override void linkAssets()
	{
		base.linkAssets();
		setupBools();
		setupCachedSprites();
	}

	private void setupCachedSprites()
	{
		foreach (StatusAsset item in list)
		{
			if (item.texture != null && item.sprite_list == null)
			{
				item.sprite_list = SpriteTextureLoader.getSpriteList("effects/" + item.texture);
			}
		}
	}

	public override void editorDiagnostic()
	{
		base.editorDiagnostic();
		foreach (StatusAsset item in list)
		{
			if (!string.IsNullOrEmpty(item.texture))
			{
				checkSpriteExists("texture", "effects/" + item.texture, item);
			}
			checkSpriteExists("path_icon", item.path_icon, item);
		}
	}

	public override void editorDiagnosticLocales()
	{
		foreach (StatusAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
		}
		base.editorDiagnosticLocales();
	}

	private void setupBools()
	{
		foreach (StatusAsset item in list)
		{
			if (item.get_override_sprite != null)
			{
				item.has_override_sprite = true;
				item.need_visual_render = true;
			}
			if (item.get_override_sprite_position != null)
			{
				item.has_override_sprite_position = true;
			}
			if (item.get_override_sprite_rotation_z != null)
			{
				item.has_override_sprite_rotation_z = true;
			}
			if (item.texture != null)
			{
				item.need_visual_render = true;
			}
		}
	}

	private bool actionPregnancyFinish(BaseSimObject pTarget, WorldTile pTile)
	{
		if (!pTarget.isAlive())
		{
			return false;
		}
		BabyMaker.makeBabyFromPregnancy(pTarget.a);
		return true;
	}

	private bool actionPregnancyParthenogenesisFinish(BaseSimObject pTarget, WorldTile pTile)
	{
		if (!pTarget.isAlive())
		{
			return false;
		}
		BabyMaker.makeBabyViaParthenogenesis(pTarget.a);
		return true;
	}

	private bool actionTakingRootsFinish(BaseSimObject pTarget, WorldTile pTile)
	{
		if (!pTarget.isAlive())
		{
			return false;
		}
		BabyMaker.makeBabyViaVegetative(pTarget.a);
		return true;
	}

	private bool actionBuddingFinish(BaseSimObject pTarget, WorldTile pTile)
	{
		if (!pTarget.isAlive())
		{
			return false;
		}
		Actor actor = pTarget.a;
		BabyMaker.makeBabyViaBudding(pTarget as Actor).applyRandomForce();
		actor.applyRandomForce();
		return true;
	}

	public static bool ashFeverEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		int maxHealthPercent = pTarget.getMaxHealthPercent(0.01f);
		pTarget.getHit(maxHealthPercent, pFlash: true, AttackType.AshFever);
		return true;
	}

	public static bool burningEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget.isActor() && pTarget.a.asset.has_skin && Randy.randomBool())
		{
			pTarget.a.addInjuryTrait("skin_burns");
		}
		int num = pTarget.getMaxHealthPercent(0.1f);
		if (pTarget.isBuilding() && pTarget.b.isRuin())
		{
			num = (int)((float)num * 0.25f + 1f);
		}
		pTarget.getHit(num, pFlash: true, AttackType.Fire);
		if (MapBox.isRenderGameplay() && Randy.randomChance(0.1f))
		{
			World.world.particles_fire.spawn(pTarget.current_position.x, pTarget.current_position.y, pRemoveCooldown: true);
		}
		return true;
	}

	public static bool spawnShieldHitEffect(BaseSimObject pSelf, BaseSimObject pAttackedBy, WorldTile pTile = null)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!pSelf.isAlive())
		{
			return false;
		}
		BaseEffect baseEffect = EffectsLibrary.spawnAt("fx_shield_hit", pSelf.current_position, 1f);
		if ((Object)(object)baseEffect == (Object)null)
		{
			return false;
		}
		baseEffect.attachTo(pSelf.a);
		return true;
	}

	public static bool poisonedEffect(BaseSimObject pTarget, WorldTile pTile = null)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		float pDamage = Mathf.Max((float)pTarget.getHealth() * 0.01f, 1f);
		if (Randy.randomBool() && pTarget.getHealth() > 1)
		{
			pTarget.getHit(pDamage, pFlash: true, AttackType.Poison);
		}
		pTarget.a.spawnParticle(Toolbox.color_infected);
		pTarget.a.startShake(0.4f, 0.2f, pHorizontal: true, pVertical: false);
		return true;
	}

	public void linkMaterials()
	{
		foreach (StatusAsset item in list)
		{
			Material material = LibraryMaterials.instance.dict[item.material_id];
			item.material = material;
		}
	}

	public string addToGameplayReport(string pWhat)
	{
		string empty = string.Empty;
		empty = empty + pWhat + "\n";
		foreach (StatusAsset item in list)
		{
			string text = item.getLocaleID().Localize();
			string text2 = item.getDescriptionID().Localize();
			string text3 = "\n" + text;
			text3 += "\n";
			if (!string.IsNullOrEmpty(text2))
			{
				text3 = text3 + "1: " + text2;
			}
			empty += text3;
		}
		return empty + "\n\n";
	}

	private bool possessedAction(BaseSimObject pTarget, WorldTile _)
	{
		Actor actor = pTarget.a;
		if (actor.asset.id == "crabzilla")
		{
			return false;
		}
		if (actor.is_unconscious)
		{
			return false;
		}
		checkPossessedMovement(actor);
		checkPossessedAttackLeft(actor);
		checkPossessedAttackRight(actor);
		checkPossessedFlip(actor);
		checkBonusActions(actor);
		return true;
	}

	private void checkBonusActions(Actor pActor)
	{
		if (!pActor.is_immovable)
		{
			if (ControllableUnit.isActionPressedJump())
			{
				checkJump(pActor);
			}
			if (ControllableUnit.isActionPressedTalk())
			{
				checkTalk(pActor);
			}
			if (ControllableUnit.isActionPressedDash())
			{
				checkDash(pActor);
			}
			if (ControllableUnit.isActionPressedBackstep())
			{
				checkBackstep(pActor);
			}
			if (ControllableUnit.isActionPressedSteal())
			{
				checkSteal(pActor);
			}
			if (ControllableUnit.isActionPressedSwear())
			{
				checkSwear(pActor);
			}
		}
	}

	private void checkTalk(Actor pActor)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		if (pActor.under_forces || !pActor.asset.control_can_talk || pActor.hasTrait("mute") || !pActor.isAttackReady())
		{
			return;
		}
		Vector2 possessionControlTargetPosition = pActor.getPossessionControlTargetPosition();
		pActor.spawnSlashTalk(possessionControlTargetPosition);
		pActor.punchTargetAnimation(Vector2.op_Implicit(possessionControlTargetPosition), pFlip: false, pReverse: false, -20f);
		pActor.lookTowardsPosition(possessionControlTargetPosition);
		pActor.setActionTimeout(1f);
		Actor actor = getActorTargetRaycast(pActor);
		if (actor == null)
		{
			actor = getActorTargetNearCursor(pActor);
		}
		if (actor != null && !actor.is_unconscious && actor.asset.can_talk_with && !actor.hasStatus("possessed") && !ControllableUnit.isControllingUnit(actor))
		{
			if (Randy.randomChance(0.3f))
			{
				actor.getSurprised(pActor.current_tile, pForceJump: true);
			}
			actor.cancelAllBeh();
			actor.tryToConvertActorToMetaFromActor(pActor, pStunOnSuccess: false);
			actor.addStatusEffect("possessed_follower");
			actor.lookTowardsPosition(pActor.current_position);
			actor.setTask("socialize_receiving", pClean: true, pCleanJob: false, pForceAction: true);
			pActor.clearLastTopicSprite();
			actor.clearLastTopicSprite();
			float pValue = 2f + Randy.randomFloat(0f, 3f);
			actor.makeWait(pValue);
			pActor.setTask("socialize_receiving", pClean: true, pCleanJob: false, pForceAction: true);
			pActor.makeWait(pValue);
		}
	}

	private void checkSteal(Actor pActor)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (!pActor.under_forces && pActor.asset.control_can_steal && pActor.isAttackReady())
		{
			Vector2 possessionControlTargetPosition = pActor.getPossessionControlTargetPosition();
			pActor.spawnSlashSteal(possessionControlTargetPosition);
			pActor.punchTargetAnimation(Vector2.op_Implicit(possessionControlTargetPosition), pFlip: false);
			pActor.lookTowardsPosition(possessionControlTargetPosition);
			pActor.setActionTimeout(1f);
			Actor actorTargetRaycast = getActorTargetRaycast(pActor);
			if (actorTargetRaycast != null && Vector2.Distance(pActor.current_position, actorTargetRaycast.current_position) < 2f)
			{
				pActor.stealActionFrom(actorTargetRaycast, 5f, 1f, pAddAggro: true, pPossessedSteal: true);
			}
		}
	}

	private void checkSwear(Actor pActor)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (pActor.asset.control_can_swear && !pActor.hasTrait("mute") && pActor.isAttackReady())
		{
			Vector2 possessionControlTargetPosition = pActor.getPossessionControlTargetPosition();
			pActor.addStatusEffect("swearing", 2f, pColorEffect: false);
			pActor.punchTargetAnimation(Vector2.op_Implicit(possessionControlTargetPosition), pFlip: false, pReverse: false, -40f);
			pActor.spawnSlashYell(possessionControlTargetPosition);
			pActor.lookTowardsPosition(possessionControlTargetPosition);
			pActor.setActionTimeout(1f);
			Actor actor = getActorTargetRaycast(pActor);
			if (actor == null)
			{
				actor = getActorTargetNearCursor(pActor);
			}
			if (actor != null)
			{
				actor.tryToGetSurprised(pActor.current_tile, pForceJump: true);
				actor.addAggro(pActor);
			}
		}
	}

	private void checkBackstep(Actor pActor)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (pActor.asset.control_can_backstep)
		{
			CombatActionAsset combat_action_backstep = CombatActionLibrary.combat_action_backstep;
			Vector2 possessionControlTargetPositionMovementVector = pActor.getPossessionControlTargetPositionMovementVector();
			combat_action_backstep.action_actor_target_position(pActor, possessionControlTargetPositionMovementVector);
		}
	}

	private void checkDash(Actor pActor)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (pActor.asset.control_can_dash)
		{
			CombatActionAsset combat_action_dash = CombatActionLibrary.combat_action_dash;
			Vector2 possessionControlTargetPositionMovementVector = pActor.getPossessionControlTargetPositionMovementVector();
			combat_action_dash.action_actor_target_position(pActor, possessionControlTargetPositionMovementVector);
		}
	}

	private void checkJump(Actor pActor)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (!pActor.under_forces && pActor.asset.control_can_jump)
		{
			float num = SimGlobals.m.gravity * 0.5f;
			float num2 = pActor.stats["mass_2"] * pActor.actor_scale;
			float num3 = num / num2;
			num3 = Mathf.Clamp(num3, 1.5f, 2.5f);
			if (pActor.hasTrait("weightless"))
			{
				num3 += num3 * 0.5f;
			}
			float pForceHeight = num3;
			Vector2 val = pActor.current_position - ControllableUnit.getMovementVector();
			Vector2 current_position = pActor.current_position;
			EffectsLibrary.spawnAt("fx_jump", current_position, 0.1f);
			pActor.calculateForce(current_position.x, current_position.y, val.x, val.y, num3, pForceHeight, pCheckCancelJobOnLand: true);
		}
	}

	private void checkPossessedAttackLeft(Actor pActor)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (!ControllableUnit.isAttackPressedLeft() || (Config.joyControls && !TouchPossessionController.isSelectedActionAttack()) || pActor.asset.skip_fight_logic || !pActor.isAttackReady())
		{
			return;
		}
		Actor actorTargetAttack = getActorTargetAttack(pActor);
		float attackRange = pActor.getAttackRange();
		Vector3 val = Vector2.op_Implicit(getAttackPos(pActor));
		if (actorTargetAttack != null && pActor.hasMeleeAttack())
		{
			Vector3 val2 = Vector2.op_Implicit(actorTargetAttack.current_position);
			float num = Vector2.Distance(pActor.current_position, Vector2.op_Implicit(val2));
			if (num > attackRange)
			{
				num = attackRange;
			}
			val = Vector2.op_Implicit(Toolbox.getNewPointVec2(pActor.current_position, Vector2.op_Implicit(val), num));
		}
		Kingdom pForceKingdom = World.world.kingdoms_wild.get("possessed");
		pActor.tryToAttack(null, pDoChecks: false, null, val, pForceKingdom, actorTargetAttack?.current_tile, attackRange * 0.2f);
		pActor.setPossessionAttackHappened();
	}

	private void checkPossessedAttackRight(Actor pActor)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		if (!ControllableUnit.isAttackJustPressedRight() || !pActor.asset.control_can_kick || pActor.asset.skip_fight_logic || !pActor.isAttackReady())
		{
			return;
		}
		Vector2 possessionControlTargetPosition = pActor.getPossessionControlTargetPosition();
		pActor.punchTargetAnimation(Vector2.op_Implicit(possessionControlTargetPosition), pFlip: false, pReverse: false, -20f);
		pActor.setActionTimeout(1f);
		pActor.spawnSlashKick(possessionControlTargetPosition);
		pActor.lookTowardsPosition(possessionControlTargetPosition);
		pActor.setPossessionAttackHappened();
		Actor actorTargetAttack = getActorTargetAttack(pActor, 2f);
		if (actorTargetAttack != null && Vector2.Distance(pActor.current_position, actorTargetAttack.current_position) < 2f)
		{
			actorTargetAttack.makeStunned();
			Vector2 current_position = pActor.current_position;
			current_position.y += pActor.current_scale.y;
			Vector2 val = possessionControlTargetPosition;
			float num = Randy.randomFloat(1.5f, 3f);
			float num2 = Randy.randomFloat(1.5f, 3f);
			if (pActor.under_forces || pActor.hasStatus("dash"))
			{
				num *= 2f;
				num2 *= 1.5f;
			}
			actorTargetAttack.calculateForce(val.x, val.y, current_position.x, current_position.y, num, num2, pCheckCancelJobOnLand: true);
			actorTargetAttack.addAggro(pActor);
		}
	}

	private Vector2 getAttackPos(Actor pActorFor, float pRange = 0f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Vector2 current_position = pActorFor.current_position;
		Vector2 val = mouse_pos;
		Vector2 result = val;
		float pDist = ((pRange != 0f) ? pRange : pActorFor.getAttackRange());
		if (pActorFor.hasMeleeAttack())
		{
			result = Toolbox.getNewPointVec2(current_position, val, pDist);
		}
		return result;
	}

	private Actor getActorTargetAttack(Actor pActorFor, float pRange = 0f)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		float num = ((pRange != 0f) ? pRange : pActorFor.getAttackRange());
		Vector2 attackPos = getAttackPos(pActorFor, num);
		WorldTile worldTile = World.world.GetTile((int)attackPos.x, (int)attackPos.y);
		if (worldTile == null)
		{
			worldTile = pActorFor.current_tile;
		}
		float num2 = float.MaxValue;
		Actor result = null;
		float num3 = num * num;
		foreach (Actor item in Finder.getUnitsFromChunk(worldTile, 0, num, pRandom: true))
		{
			if (pActorFor != item)
			{
				float num4 = Toolbox.SquaredDistVec2Float(attackPos, item.current_position);
				if (!(num4 > num3) && num4 < num2)
				{
					result = item;
					num2 = num4;
				}
			}
		}
		return result;
	}

	private Actor getActorTargetRaycast(Actor pActorFor)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Vector2 tActorPos = pActorFor.current_position;
		Vector2 pTarget = mouse_pos;
		List<WorldTile> obj = PathfinderTools.raycast(tActorPos, pTarget);
		float tDistance = float.MaxValue;
		Actor tActorTarget = null;
		foreach (WorldTile item in obj)
		{
			if (!item.hasUnits())
			{
				continue;
			}
			item.doUnits(delegate(Actor tActor)
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				if (tActor != pActorFor)
				{
					float num = Toolbox.SquaredDistVec2Float(tActorPos, tActor.current_position);
					if (num < tDistance)
					{
						tDistance = num;
						tActorTarget = tActor;
					}
				}
			});
			if (tActorTarget != null)
			{
				break;
			}
		}
		if (tActorTarget == null || tActorTarget == pActorFor)
		{
			return null;
		}
		return tActorTarget;
	}

	private Actor getActorTargetNearCursor(Actor pActorFor)
	{
		Actor actorNearCursor = World.world.getActorNearCursor();
		if (actorNearCursor == null || actorNearCursor == pActorFor)
		{
			return null;
		}
		return actorNearCursor;
	}

	private void checkPossessedMovement(Actor pActor)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (!ControllableUnit.isMovementActionActive())
		{
			pActor.setPossessedMovement(pValue: false);
		}
		else
		{
			if (pActor.is_immovable)
			{
				return;
			}
			Vector2 movementVector = ControllableUnit.getMovementVector();
			if (movementVector == Vector2.zero)
			{
				return;
			}
			pActor.setPossessedMovement(pValue: true);
			Vector2 current_position = pActor.current_position;
			pActor.next_step_position_possession = current_position + movementVector * 2f;
			movementVector += current_position;
			float num = pActor.updatePossessedMovementTowards(World.world.elapsed, movementVector);
			if (!pActor.isInAir() && !pActor.under_forces && !(num < 0.03f))
			{
				BaseEffect baseEffect = EffectsLibrary.spawnAt("fx_walk", current_position, 0.1f);
				if ((Object)(object)baseEffect != (Object)null)
				{
					((Component)baseEffect).GetComponent<SpriteRenderer>().flipX = !pActor.flip;
				}
			}
		}
	}

	private void checkPossessedFlip(Actor pActor)
	{
		pActor.updateMovementPossessedFlip();
	}
}
