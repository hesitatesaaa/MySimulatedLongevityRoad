using System;
using System.Collections.Generic;
using strings;

public class SubspeciesTraitLibrary : BaseTraitLibrary<SubspeciesTrait>
{
	private const string TEMPLATE_EGG = "$egg$";

	private const string TEMPLATE_MAGIC_BLOOD = "$magic_blood$";

	private const string TEMPLATE_SKIN_MUTATION = "$skin_mutation$";

	private const string TEMPLATE_ADAPTATION = "$adaptation$";

	private List<SubspeciesTrait> _pot_mutation_traits_add = new List<SubspeciesTrait>();

	private List<SubspeciesTrait> _pot_mutation_traits_remove = new List<SubspeciesTrait>();

	private static List<string> _bad_genes = AssetLibrary<SubspeciesTrait>.l<string>("fragile_health", "weak", "slow", "fat", "ugly");

	protected override string icon_path => "ui/Icons/subspecies_traits/";

	protected override List<string> getDefaultTraitsForMeta(ActorAsset pAsset)
	{
		return pAsset.default_subspecies_traits;
	}

	public override void init()
	{
		base.init();
		addMetamorphosis();
		addSpawnSomething();
		addLimits();
		addMaturation();
		addStats();
		addGenetic();
		addDiet();
		addReproduction();
		addReproductionModes();
		addOther();
		addSleepCycles();
		addMagic();
		addChaos();
		addPhenotypes();
		addAdaptations();
		addMutations();
		addEggs();
	}

	private void addMagic()
	{
		add(new SubspeciesTrait
		{
			id = "$magic_blood$",
			group_id = "talents"
		});
		t.base_stats_meta.addTag("magic");
		SubspeciesTrait subspeciesTrait = t;
		subspeciesTrait.action_death = (WorldAction)Delegate.Combine(subspeciesTrait.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		clone("gift_of_fire", "$magic_blood$");
		t.rarity = Rarity.R2_Epic;
		t.addSpell("cast_fire");
		clone("gift_of_thunder", "$magic_blood$");
		t.addSpell("summon_lightning");
		clone("gift_of_void", "$magic_blood$");
		t.addSpell("teleport");
		clone("gift_of_air", "$magic_blood$");
		t.addSpell("summon_tornado");
		clone("gift_of_blood", "$magic_blood$");
		t.rarity = Rarity.R0_Normal;
		t.addSpell("cast_blood_rain");
		clone("gift_of_harmony", "$magic_blood$");
		t.addSpell("cast_blood_rain");
		t.addSpell("cast_cure");
		clone("gift_of_water", "$magic_blood$");
		t.rarity = Rarity.R1_Rare;
		t.addSpell("cast_shield");
		clone("gift_of_life", "$magic_blood$");
		t.rarity = Rarity.R1_Rare;
		t.addSpell("cast_grass_seeds");
		t.addSpell("spawn_vegetation");
		clone("gift_of_death", "$magic_blood$");
		t.addSpell("spawn_skeleton");
		t.addSpell("cast_curse");
	}

	private void addChaos()
	{
		add(new SubspeciesTrait
		{
			id = "grin_mark",
			group_id = "fate",
			spawn_random_trait_allowed = false,
			priority = -100
		});
		t.setTraitInfoToGrinMark();
		t.show_for_unlockables_ui = true;
		t.setUnlockedWithAchievement("achievementCreaturesExplorer");
		add(new SubspeciesTrait
		{
			id = "annoying_fireworks",
			group_id = "chaos",
			rarity = Rarity.R0_Normal,
			action_death = delegate(BaseSimObject _, WorldTile pTile)
			{
				EffectsLibrary.spawn("fx_fireworks", pTile);
				return true;
			}
		});
		add(new SubspeciesTrait
		{
			id = "spicy_kids",
			group_id = "chaos",
			action_birth = ActionLibrary.fireDropsSpawn
		});
		add(new SubspeciesTrait
		{
			id = "nimble",
			group_id = "chaos",
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true,
			action_attack_target = (BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile) => pTarget.isActor() && pSelf.a.tryToStealItems(pTarget.a)
		});
		t.setUnlockedWithAchievement("achievementNotOnMyWatch");
		t.base_stats_meta.addTag("steal_items");
		add(new SubspeciesTrait
		{
			id = "antimatter_essence",
			group_id = "chaos",
			spawn_random_trait_allowed = false,
			action_death = delegate(BaseSimObject _, WorldTile pTile)
			{
				DropsLibrary.action_antimatter_bomb(pTile);
				return true;
			}
		});
		t.setUnlockedWithAchievement("achievementTntAndHeat");
		add(new SubspeciesTrait
		{
			id = "gaia_roots",
			group_id = "growth",
			rarity = Rarity.R0_Normal,
			action_death = delegate(BaseSimObject _, WorldTile pTile)
			{
				if (!WorldLawLibrary.world_law_clouds.isEnabled())
				{
					return false;
				}
				if (Randy.randomChance(0.3f))
				{
					EffectsLibrary.spawn("fx_cloud", pTile, "cloud_normal");
				}
				return true;
			},
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementZoo");
		add(new SubspeciesTrait
		{
			id = "parental_care",
			group_id = "growth"
		});
	}

	private void addMetamorphosis()
	{
		add(new SubspeciesTrait
		{
			id = "fire_elemental_form",
			group_id = "chaos",
			rarity = Rarity.R2_Epic,
			action_death = delegate(BaseSimObject pSimObject, WorldTile _)
			{
				Actor actor = pSimObject.a;
				if (!actor.isPrettyOld())
				{
					return false;
				}
				ActionLibrary.metamorphInto(actor, SA.fire_elementals.GetRandom());
				return true;
			},
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementEternalChaos");
		t.addOpposite("fenix_born");
		t.addOpposite("metamorphosis_butterfly");
		t.addOpposite("metamorphosis_chicken");
		t.addOpposite("metamorphosis_crab");
		t.addOpposite("metamorphosis_sword");
		t.addOpposite("metamorphosis_wolf");
		add(new SubspeciesTrait
		{
			id = "fenix_born",
			group_id = "rebirth",
			action_death = delegate(BaseSimObject pSimObject, WorldTile _)
			{
				Actor actor = pSimObject.a;
				if (!actor.isPrettyOld())
				{
					return false;
				}
				ActionLibrary.metamorphInto(actor, actor.asset.id, pRemoveAcquiredTraits: true, pUseCurrentSubspecies: true);
				return true;
			},
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementLongLiving");
		t.addOpposite("fire_elemental_form");
		t.addOpposite("metamorphosis_butterfly");
		t.addOpposite("metamorphosis_chicken");
		t.addOpposite("metamorphosis_crab");
		t.addOpposite("metamorphosis_sword");
		t.addOpposite("metamorphosis_wolf");
		add(new SubspeciesTrait
		{
			id = "metamorphosis_crab",
			group_id = "rebirth",
			rarity = Rarity.R1_Rare,
			action_death = delegate(BaseSimObject pSimObject, WorldTile _)
			{
				Actor actor = pSimObject.a;
				if (!actor.isPrettyOld())
				{
					return false;
				}
				ActionLibrary.metamorphInto(actor, "crab");
				return true;
			},
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementEngineeredEvolution");
		t.addOpposite("fire_elemental_form");
		t.addOpposite("fenix_born");
		t.addOpposite("metamorphosis_butterfly");
		t.addOpposite("metamorphosis_chicken");
		t.addOpposite("metamorphosis_sword");
		t.addOpposite("metamorphosis_wolf");
		add(new SubspeciesTrait
		{
			id = "metamorphosis_chicken",
			group_id = "rebirth",
			rarity = Rarity.R0_Normal,
			action_death = delegate(BaseSimObject pSimObject, WorldTile _)
			{
				Actor actor = pSimObject.a;
				if (!actor.isPrettyOld())
				{
					return false;
				}
				ActionLibrary.metamorphInto(actor, "chicken");
				return true;
			},
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.addOpposite("fire_elemental_form");
		t.addOpposite("fenix_born");
		t.addOpposite("metamorphosis_butterfly");
		t.addOpposite("metamorphosis_crab");
		t.addOpposite("metamorphosis_sword");
		t.addOpposite("metamorphosis_wolf");
		add(new SubspeciesTrait
		{
			id = "metamorphosis_wolf",
			group_id = "rebirth",
			rarity = Rarity.R0_Normal,
			action_death = delegate(BaseSimObject pSimObject, WorldTile _)
			{
				Actor actor = pSimObject.a;
				if (!actor.isPrettyOld())
				{
					return false;
				}
				ActionLibrary.metamorphInto(actor, "wolf");
				return true;
			},
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.addOpposite("fire_elemental_form");
		t.addOpposite("fenix_born");
		t.addOpposite("metamorphosis_butterfly");
		t.addOpposite("metamorphosis_chicken");
		t.addOpposite("metamorphosis_crab");
		t.addOpposite("metamorphosis_sword");
		add(new SubspeciesTrait
		{
			id = "metamorphosis_butterfly",
			group_id = "rebirth",
			rarity = Rarity.R0_Normal,
			action_death = delegate(BaseSimObject pSimObject, WorldTile _)
			{
				Actor actor = pSimObject.a;
				if (!actor.isPrettyOld())
				{
					return false;
				}
				ActionLibrary.metamorphInto(actor, "butterfly");
				return true;
			},
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementMasterWeaver");
		t.addOpposite("fire_elemental_form");
		t.addOpposite("fenix_born");
		t.addOpposite("metamorphosis_chicken");
		t.addOpposite("metamorphosis_crab");
		t.addOpposite("metamorphosis_sword");
		t.addOpposite("metamorphosis_wolf");
		add(new SubspeciesTrait
		{
			id = "metamorphosis_sword",
			group_id = "rebirth",
			rarity = Rarity.R1_Rare,
			action_death = delegate(BaseSimObject pSimObject, WorldTile _)
			{
				Actor actor = pSimObject.a;
				if (!actor.isPrettyOld())
				{
					return false;
				}
				ActionLibrary.metamorphInto(actor, "crystal_sword");
				return true;
			},
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.addOpposite("fire_elemental_form");
		t.addOpposite("fenix_born");
		t.addOpposite("metamorphosis_butterfly");
		t.addOpposite("metamorphosis_chicken");
		t.addOpposite("metamorphosis_crab");
		t.addOpposite("metamorphosis_wolf");
	}

	private void addSpawnSomething()
	{
		add(new SubspeciesTrait
		{
			id = "bioproduct_gold",
			group_id = "bioproducts",
			rarity = Rarity.R0_Normal,
			priority = 100,
			is_diet_related = true,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementSmellyCity");
		add(new SubspeciesTrait
		{
			id = "bioproduct_gems",
			group_id = "bioproducts",
			rarity = Rarity.R0_Normal,
			priority = 100,
			is_diet_related = true,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		add(new SubspeciesTrait
		{
			id = "bioproduct_stone",
			group_id = "bioproducts",
			rarity = Rarity.R0_Normal,
			priority = 99,
			is_diet_related = true,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		add(new SubspeciesTrait
		{
			id = "bioproduct_mushrooms",
			group_id = "bioproducts",
			rarity = Rarity.R0_Normal,
			priority = 98,
			is_diet_related = true,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		add(new SubspeciesTrait
		{
			id = "death_grow_mythril",
			group_id = "growth",
			rarity = Rarity.R1_Rare,
			priority = 97,
			action_death = delegate(BaseSimObject pSimObject, WorldTile pTile)
			{
				if (pSimObject.a.isAdult())
				{
					World.world.buildings.addBuilding("mineral_mythril", pTile, pCheckForBuild: true);
				}
				return true;
			},
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementGen5Worlds");
		t.addOpposite("death_grow_tree");
		t.addOpposite("death_grow_plant");
		add(new SubspeciesTrait
		{
			id = "death_grow_tree",
			group_id = "growth",
			rarity = Rarity.R0_Normal,
			priority = 95,
			action_death = ActionLibrary.tryToGrowTree,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementGen50Worlds");
		t.addOpposite("death_grow_plant");
		t.addOpposite("death_grow_mythril");
		add(new SubspeciesTrait
		{
			id = "death_grow_plant",
			group_id = "growth",
			rarity = Rarity.R0_Normal,
			priority = 96,
			action_death = ActionLibrary.tryToCreatePlants,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementGen100Worlds");
		t.addOpposite("death_grow_tree");
		t.addOpposite("death_grow_mythril");
	}

	private void addSleepCycles()
	{
		add(new SubspeciesTrait
		{
			id = "energy_preserver",
			group_id = "sleep_cycles",
			rarity = Rarity.R1_Rare,
			priority = 100
		});
		add(new SubspeciesTrait
		{
			id = "polyphasic_sleep",
			group_id = "sleep_cycles",
			rarity = Rarity.R1_Rare,
			priority = 99
		});
		t.addDecision("polyphasic_sleep");
		t.addOpposite("monophasic_sleep");
		add(new SubspeciesTrait
		{
			id = "monophasic_sleep",
			group_id = "sleep_cycles",
			rarity = Rarity.R1_Rare,
			priority = 98
		});
		t.addDecision("monophasic_sleep");
		t.addOpposite("polyphasic_sleep");
		add(new SubspeciesTrait
		{
			id = "prolonged_rest",
			group_id = "sleep_cycles",
			rarity = Rarity.R1_Rare,
			priority = 97
		});
		add(new SubspeciesTrait
		{
			id = "nocturnal_dormancy",
			group_id = "hibernation",
			rarity = Rarity.R2_Epic,
			priority = 100
		});
		t.addDecision("sleep_at_dark_age");
		t.addOpposite("chaos_driven");
		add(new SubspeciesTrait
		{
			id = "circadian_drift",
			group_id = "hibernation",
			priority = 99
		});
		t.addDecision("sleep_at_light_age");
		t.addOpposite("chaos_driven");
		add(new SubspeciesTrait
		{
			id = "winter_slumberers",
			group_id = "hibernation",
			rarity = Rarity.R2_Epic,
			priority = 98
		});
		t.addDecision("sleep_at_winter_age");
		t.addOpposite("chaos_driven");
		add(new SubspeciesTrait
		{
			id = "chaos_driven",
			group_id = "hibernation",
			priority = 97
		});
		t.addDecision("sleep_when_not_chaos_age");
		t.addOpposite("nocturnal_dormancy");
		t.addOpposite("winter_slumberers");
		t.addOpposite("circadian_drift");
	}

	private void addOther()
	{
		add(new SubspeciesTrait
		{
			id = "shiny_love",
			group_id = "chaos"
		});
		t.setUnlockedWithAchievement("achievementPlanetOfApes");
		t.addDecision("try_to_steal_money");
		add(new SubspeciesTrait
		{
			id = "aggressive",
			group_id = "chaos"
		});
		add(new SubspeciesTrait
		{
			id = "genetic_mirror",
			group_id = "chaos"
		});
		t.setUnlockedWithAchievement("achievementTraitExplorerSubspecies");
		add(new SubspeciesTrait
		{
			id = "unstable_genome",
			group_id = "chaos"
		});
		t.setUnlockedWithAchievement("achievementGenesExplorer");
		add(new SubspeciesTrait
		{
			id = "pure",
			group_id = "mind",
			rarity = Rarity.R2_Epic,
			remove_for_zombies = true
		});
		t.setUnlockedWithAchievement("achievementCantBeTooMuch");
		add(new SubspeciesTrait
		{
			id = "super_positivity",
			group_id = "mind",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true,
			remove_for_zombies = true
		});
		add(new SubspeciesTrait
		{
			id = "dreamweavers",
			group_id = "mind",
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true,
			remove_for_zombies = true
		});
		t.setUnlockedWithAchievement("achievementMindlessHusk");
		t.addDecision("try_affect_dreams");
		add(new SubspeciesTrait
		{
			id = "telepathic_link",
			group_id = "mind",
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true,
			remove_for_zombies = true
		});
		add(new SubspeciesTrait
		{
			id = "inquisitive_nature",
			group_id = "mind",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		add(new SubspeciesTrait
		{
			id = "cautious_instincts",
			group_id = "mind",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		add(new SubspeciesTrait
		{
			id = "aquatic",
			group_id = "body",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = false,
			in_mutation_pot_remove = false
		});
		t.base_stats.addTag("water_creature");
		t.setUnlockedWithAchievement("achievementBoatsDisposal");
		t.addDecision("random_swim");
		add(new SubspeciesTrait
		{
			id = "hovering",
			group_id = "body",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		add(new SubspeciesTrait
		{
			id = "pollinating",
			group_id = "body",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = false,
			in_mutation_pot_remove = false
		});
		t.addDecision("pollinate");
		add(new SubspeciesTrait
		{
			id = "hydrophobia",
			group_id = "body",
			rarity = Rarity.R0_Normal
		});
		t.base_stats_meta.addTag("damaged_by_water");
	}

	private void addReproductionModes()
	{
		add(new SubspeciesTrait
		{
			id = "reproduction_strategy_oviparity",
			group_id = "reproduction_strategy",
			rarity = Rarity.R0_Normal,
			priority = 100,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.action_on_augmentation_remove = delegate(NanoObject pNanoObject, BaseAugmentationAsset _)
		{
			if (pNanoObject.isRekt())
			{
				return false;
			}
			Subspecies subspecies = (Subspecies)pNanoObject;
			using ListPool<string> listPool = new ListPool<string>();
			foreach (SubspeciesTrait trait in subspecies.getTraits())
			{
				if (trait.phenotype_egg)
				{
					listPool.Add(trait.id);
				}
			}
			if (listPool.Count > 0)
			{
				subspecies.removeTraits(listPool);
			}
			foreach (Actor unit in subspecies.getUnits())
			{
				if (unit.isEgg())
				{
					unit.finishStatusEffect("egg");
				}
			}
			return true;
		};
		t.base_stats_meta["maturation"] = 1f;
		t.addOpposite("reproduction_strategy_viviparity");
		t.addOpposite("reproduction_budding");
		t.addOpposite("reproduction_vegetative");
		t.base_stats_meta.addTag("oviparity");
		add(new SubspeciesTrait
		{
			id = "reproduction_strategy_viviparity",
			group_id = "reproduction_strategy",
			rarity = Rarity.R0_Normal,
			priority = 99,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.base_stats_meta["maturation"] = 1f;
		t.addOpposite("reproduction_fission");
		t.addOpposite("reproduction_spores");
		t.addOpposite("reproduction_strategy_oviparity");
		t.addOpposite("reproduction_budding");
		t.addOpposite("reproduction_vegetative");
		t.base_stats_meta.addTag("viviparity");
	}

	private void addReproduction()
	{
		add(new SubspeciesTrait
		{
			id = "reproduction_sexual",
			group_id = "reproductive_methods",
			rarity = Rarity.R0_Normal,
			priority = 100,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.base_stats["birth_rate"] = 3f;
		t.addDecision("sexual_reproduction_try");
		t.addDecision("find_lover");
		t.base_stats_meta.addTag("reproduction_sexual");
		t.base_stats_meta.addTag("needs_mate");
		t.addOpposite("reproduction_hermaphroditic");
		t.addOpposite("reproduction_fission");
		t.addOpposite("reproduction_spores");
		t.addOpposite("reproduction_vegetative");
		add(new SubspeciesTrait
		{
			id = "reproduction_spores",
			group_id = "reproductive_methods",
			priority = 99,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("asexual_reproduction_spores");
		t.base_stats_meta.addTag("reproduction_asexual");
		t.addOpposite("reproduction_strategy_viviparity");
		t.addOpposite("reproduction_sexual");
		t.addOpposite("reproduction_hermaphroditic");
		t.addOpposite("reproduction_parthenogenesis");
		t.addOpposite("reproduction_fission");
		t.addOpposite("reproduction_vegetative");
		t.addOpposite("reproduction_divine");
		t.addOpposite("reproduction_budding");
		add(new SubspeciesTrait
		{
			id = "reproduction_fission",
			group_id = "reproductive_methods",
			rarity = Rarity.R2_Epic,
			priority = 98,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("asexual_reproduction_fission");
		t.base_stats_meta.addTag("reproduction_asexual");
		t.addOpposite("reproduction_strategy_viviparity");
		t.addOpposite("reproduction_sexual");
		t.addOpposite("reproduction_hermaphroditic");
		t.addOpposite("reproduction_parthenogenesis");
		t.addOpposite("reproduction_spores");
		t.addOpposite("reproduction_vegetative");
		t.addOpposite("reproduction_divine");
		t.addOpposite("reproduction_budding");
		add(new SubspeciesTrait
		{
			id = "reproduction_budding",
			group_id = "reproductive_methods",
			rarity = Rarity.R2_Epic,
			priority = 98,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("asexual_reproduction_budding");
		t.base_stats_meta.addTag("reproduction_asexual");
		t.addOpposite("reproduction_strategy_viviparity");
		t.addOpposite("reproduction_strategy_oviparity");
		t.addOpposite("reproduction_parthenogenesis");
		t.addOpposite("reproduction_spores");
		t.addOpposite("reproduction_vegetative");
		t.addOpposite("reproduction_divine");
		t.addOpposite("reproduction_fission");
		add(new SubspeciesTrait
		{
			id = "reproduction_hermaphroditic",
			group_id = "reproductive_methods",
			rarity = Rarity.R0_Normal,
			priority = 97,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("sexual_reproduction_try");
		t.addDecision("find_lover");
		t.base_stats_meta.addTag("reproduction_sexual");
		t.base_stats_meta.addTag("needs_mate");
		t.addOpposite("reproduction_sexual");
		t.addOpposite("reproduction_parthenogenesis");
		t.addOpposite("reproduction_fission");
		t.addOpposite("reproduction_spores");
		t.addOpposite("reproduction_vegetative");
		add(new SubspeciesTrait
		{
			id = "reproduction_parthenogenesis",
			group_id = "reproductive_methods",
			rarity = Rarity.R1_Rare,
			priority = 96,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("asexual_reproduction_parthenogenesis");
		t.base_stats_meta.addTag("reproduction_asexual");
		t.addOpposite("reproduction_hermaphroditic");
		t.addOpposite("reproduction_fission");
		t.addOpposite("reproduction_spores");
		t.addOpposite("reproduction_vegetative");
		t.addOpposite("reproduction_divine");
		t.addOpposite("reproduction_budding");
		add(new SubspeciesTrait
		{
			id = "reproduction_vegetative",
			group_id = "reproductive_methods",
			rarity = Rarity.R0_Normal,
			priority = 95,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.base_stats_meta["maturation"] = 12f;
		t.addDecision("asexual_reproduction_vegetative");
		t.base_stats_meta.addTag("reproduction_asexual");
		t.addOpposite("reproduction_strategy_oviparity");
		t.addOpposite("reproduction_strategy_viviparity");
		t.addOpposite("reproduction_sexual");
		t.addOpposite("reproduction_hermaphroditic");
		t.addOpposite("reproduction_parthenogenesis");
		t.addOpposite("reproduction_fission");
		t.addOpposite("reproduction_spores");
		t.addOpposite("reproduction_divine");
		t.addOpposite("reproduction_budding");
		add(new SubspeciesTrait
		{
			id = "reproduction_divine",
			group_id = "reproductive_methods",
			rarity = Rarity.R2_Epic,
			priority = 94,
			remove_for_zombies = true
		});
		t.addDecision("asexual_reproduction_divine");
		t.addOpposite("reproduction_parthenogenesis");
		t.addOpposite("reproduction_fission");
		t.addOpposite("reproduction_spores");
		t.addOpposite("reproduction_vegetative");
		t.addOpposite("reproduction_budding");
		add(new SubspeciesTrait
		{
			id = "reproduction_soulborne",
			group_id = "reproductive_methods",
			priority = 93,
			remove_for_zombies = true,
			action_attack_target = delegate(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
			{
				if (!pTarget.isActor())
				{
					return false;
				}
				if (!pTarget.a.asset.has_soul)
				{
					return false;
				}
				pSelf.addStatusEffect("soul_harvested");
				return true;
			}
		});
		add(new SubspeciesTrait
		{
			id = "reproduction_metamorph",
			group_id = "reproductive_methods",
			priority = 92,
			remove_for_zombies = true,
			action_attack_target = delegate(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
			{
				if (!pTarget.isActor())
				{
					return false;
				}
				if (!pTarget.a.canTurnIntoColdOne())
				{
					return false;
				}
				if (pTarget.a.subspecies == pSelf.a.subspecies)
				{
					return false;
				}
				Actor actor = ActionLibrary.turnIntoMetamorph(pTarget.a, pSelf.a.asset.id);
				if (actor != null)
				{
					actor.setParent1(pSelf.a);
					BabyHelper.applyParentsMeta(pSelf.a, null, actor);
				}
				return true;
			}
		});
	}

	private void addDiet()
	{
		add(new SubspeciesTrait
		{
			id = "stomach",
			group_id = "body",
			rarity = Rarity.R0_Normal,
			priority = 100,
			remove_for_zombies = true
		});
		t.addDecision("try_to_eat_city_food");
		t.action_on_augmentation_add = delegate(NanoObject pNanoObject, BaseAugmentationAsset _)
		{
			((Subspecies)pNanoObject).addTrait("diet_omnivore");
			return true;
		};
		t.action_on_augmentation_remove = delegate(NanoObject pNanoObject, BaseAugmentationAsset _)
		{
			if (pNanoObject.isRekt())
			{
				return false;
			}
			Subspecies subspecies = (Subspecies)pNanoObject;
			using ListPool<string> listPool = new ListPool<string>();
			foreach (SubspeciesTrait trait in subspecies.getTraits())
			{
				if (trait.is_diet_related)
				{
					listPool.Add(trait.id);
				}
			}
			if (listPool.Count > 0)
			{
				subspecies.removeTraits(listPool);
			}
			return true;
		};
		t.base_stats_meta.addTag("needs_food");
		add(new SubspeciesTrait
		{
			id = "big_stomach",
			group_id = "body",
			rarity = Rarity.R1_Rare,
			priority = 99,
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.base_stats_meta["max_nutrition"] = 100f;
		add(new SubspeciesTrait
		{
			id = "voracious",
			group_id = "body",
			rarity = Rarity.R0_Normal,
			priority = 98
		});
		t.base_stats_meta["metabolic_rate"] = 10f;
		add(new SubspeciesTrait
		{
			id = "diet_frugivore",
			group_id = "diet",
			rarity = Rarity.R0_Normal,
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_fruits");
		t.addOpposite("diet_herbivore");
		t.addOpposite("diet_omnivore");
		t.base_stats_meta.addTag("diet_fruits");
		add(new SubspeciesTrait
		{
			id = "diet_granivore",
			group_id = "diet",
			rarity = Rarity.R0_Normal,
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_crops");
		t.addOpposite("diet_herbivore");
		t.addOpposite("diet_omnivore");
		t.base_stats_meta.addTag("diet_crops");
		add(new SubspeciesTrait
		{
			id = "diet_florivore",
			group_id = "diet",
			rarity = Rarity.R0_Normal,
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_flowers");
		t.addOpposite("diet_herbivore");
		t.addOpposite("diet_omnivore");
		t.base_stats_meta.addTag("diet_flowers");
		add(new SubspeciesTrait
		{
			id = "diet_graminivore",
			group_id = "diet",
			rarity = Rarity.R1_Rare,
			is_diet_related = true,
			in_mutation_pot_add = false,
			remove_for_zombies = true
		});
		t.addDecision("diet_grass");
		t.base_stats_meta.addTag("diet_grass");
		add(new SubspeciesTrait
		{
			id = "diet_xylophagy",
			group_id = "diet",
			rarity = Rarity.R2_Epic,
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_wood");
		t.base_stats_meta.addTag("diet_wood");
		add(new SubspeciesTrait
		{
			id = "diet_geophagy",
			group_id = "diet",
			rarity = Rarity.R2_Epic,
			is_diet_related = true,
			remove_for_zombies = true,
			spawn_random_trait_allowed = false
		});
		t.addDecision("diet_tiles");
		t.base_stats_meta.addTag("diet_tiles");
		add(new SubspeciesTrait
		{
			id = "diet_folivore",
			group_id = "diet",
			rarity = Rarity.R0_Normal,
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_vegetation");
		t.addOpposite("diet_herbivore");
		t.addOpposite("diet_omnivore");
		t.base_stats_meta.addTag("diet_vegetation");
		add(new SubspeciesTrait
		{
			id = "diet_carnivore",
			group_id = "diet",
			rarity = Rarity.R0_Normal,
			priority = 98,
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_meat");
		t.addOpposite("diet_omnivore");
		t.base_stats_meta.addTag("diet_meat");
		add(new SubspeciesTrait
		{
			id = "diet_piscivore",
			group_id = "diet",
			rarity = Rarity.R0_Normal,
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_fish");
		t.addOpposite("diet_omnivore");
		t.addOpposite("diet_herbivore");
		t.base_stats_meta.addTag("diet_fish");
		add(new SubspeciesTrait
		{
			id = "diet_lithotroph",
			group_id = "diet",
			rarity = Rarity.R1_Rare,
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_minerals");
		t.base_stats_meta.addTag("diet_minerals");
		add(new SubspeciesTrait
		{
			id = "diet_insectivore",
			group_id = "diet",
			rarity = Rarity.R0_Normal,
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_meat_insect");
		t.base_stats_meta.addTag("diet_meat_insect");
		add(new SubspeciesTrait
		{
			id = "diet_algivore",
			group_id = "diet",
			rarity = Rarity.R0_Normal,
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_algae");
		t.base_stats_meta.addTag("diet_algae");
		add(new SubspeciesTrait
		{
			id = "diet_cannibalism",
			group_id = "diet",
			priority = 1,
			is_diet_related = true,
			rarity = Rarity.R1_Rare,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.setUnlockedWithAchievement("achievementClannibals");
		t.addDecision("diet_same_species");
		t.base_stats_meta.addTag("diet_same_species");
		add(new SubspeciesTrait
		{
			id = "diet_nectarivore",
			group_id = "diet",
			is_diet_related = true,
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_nectar");
		t.base_stats_meta.addTag("diet_nectar");
		add(new SubspeciesTrait
		{
			id = "diet_hematophagy",
			group_id = "diet",
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_blood");
		t.base_stats_meta.addTag("diet_blood");
		add(new SubspeciesTrait
		{
			id = "diet_herbivore",
			group_id = "diet",
			rarity = Rarity.R1_Rare,
			priority = 99,
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_fruits");
		t.addDecision("diet_vegetation");
		t.addDecision("diet_flowers");
		t.addDecision("diet_grass");
		t.addDecision("diet_crops");
		t.addOpposite("diet_frugivore");
		t.addOpposite("diet_granivore");
		t.addOpposite("diet_florivore");
		t.addOpposite("diet_folivore");
		t.addOpposite("diet_piscivore");
		t.addOpposite("diet_omnivore");
		t.base_stats_meta.addTag("diet_flowers");
		t.base_stats_meta.addTag("diet_fruits");
		t.base_stats_meta.addTag("diet_crops");
		t.base_stats_meta.addTag("diet_vegetation");
		t.base_stats_meta.addTag("diet_grass");
		add(new SubspeciesTrait
		{
			id = "diet_omnivore",
			group_id = "diet",
			rarity = Rarity.R1_Rare,
			priority = 100,
			is_diet_related = true,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("diet_fruits");
		t.addDecision("diet_vegetation");
		t.addDecision("diet_meat");
		t.addOpposite("diet_frugivore");
		t.addOpposite("diet_granivore");
		t.addOpposite("diet_florivore");
		t.addOpposite("diet_folivore");
		t.addOpposite("diet_carnivore");
		t.addOpposite("diet_piscivore");
		t.addOpposite("diet_herbivore");
		t.base_stats_meta.addTag("diet_flowers");
		t.base_stats_meta.addTag("diet_fruits");
		t.base_stats_meta.addTag("diet_crops");
		t.base_stats_meta.addTag("diet_vegetation");
		t.base_stats_meta.addTag("diet_meat");
		t.base_stats_meta.addTag("diet_fish");
	}

	private void addGenetic()
	{
		add(new SubspeciesTrait
		{
			id = "advanced_hippocampus",
			group_id = "advanced_brain",
			rarity = Rarity.R1_Rare,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("try_to_read");
		t.base_stats_meta.addTag("has_advanced_memory");
		add(new SubspeciesTrait
		{
			id = "wernicke_area",
			group_id = "advanced_brain",
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("socialize_initial_check");
		t.base_stats_meta.addTag("has_advanced_communication");
		add(new SubspeciesTrait
		{
			id = "amygdala",
			group_id = "advanced_brain",
			rarity = Rarity.R2_Epic,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addDecision("run_away_from_carnivore");
		t.addDecision("run_away");
		t.addDecision("reflection");
		t.base_stats_meta.addTag("has_emotions");
		add(new SubspeciesTrait
		{
			id = "prefrontal_cortex",
			group_id = "advanced_brain",
			in_mutation_pot_add = true,
			remove_for_zombies = true,
			priority = 100
		});
		t.addDecision("check_lover_city");
		t.addDecision("find_city_job");
		t.addDecision("build_civ_city_here");
		t.addDecision("try_to_return_to_home_city");
		t.addDecision("try_to_start_new_civilization");
		t.addDecision("check_join_city");
		t.addDecision("check_join_empty_nearby_city");
		t.base_stats_meta.addTag("has_sapience");
		add(new SubspeciesTrait
		{
			id = "bad_genes",
			group_id = "body",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true,
			remove_for_zombies = true,
			action_growth = delegate(BaseSimObject pSimObject, WorldTile _)
			{
				if (Randy.randomChance(0.01f))
				{
					string random = _bad_genes.GetRandom();
					pSimObject.a.addTrait(random);
				}
				return true;
			}
		});
		t.setUnlockedWithAchievement("achievementFastLiving");
		add(new SubspeciesTrait
		{
			id = "photosynthetic_skin",
			group_id = "diet",
			rarity = Rarity.R2_Epic,
			in_mutation_pot_add = true,
			remove_for_zombies = true,
			special_effect_interval = 10f,
			action_special_effect = delegate(BaseSimObject pSimObject, WorldTile _)
			{
				if (World.world.era_manager.getCurrentAge().flag_night)
				{
					return false;
				}
				int pVal = Randy.randomInt(2, 10);
				pSimObject.a.addNutritionFromEating(pVal);
				return true;
			}
		});
		add(new SubspeciesTrait
		{
			id = "genetic_psychosis",
			group_id = "mind",
			rarity = Rarity.R2_Epic,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true,
			remove_for_zombies = true,
			action_growth = delegate(BaseSimObject pSimObject, WorldTile _)
			{
				if (pSimObject.a.isPrettyOld() && Randy.randomChance(0.01f))
				{
					pSimObject.a.addTrait("madness");
				}
				return true;
			}
		});
		add(new SubspeciesTrait
		{
			id = "bioluminescence",
			group_id = "body",
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.base_stats.addTag("generate_light");
		add(new SubspeciesTrait
		{
			id = "accelerated_healing",
			group_id = "body",
			rarity = Rarity.R1_Rare,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true,
			remove_for_zombies = true,
			action_growth = delegate(BaseSimObject pSimObject, WorldTile _)
			{
				Actor actor = pSimObject.a;
				IReadOnlyCollection<ActorTrait> traits = pSimObject.a.getTraits();
				using ListPool<ActorTrait> listPool = new ListPool<ActorTrait>();
				foreach (ActorTrait item in traits)
				{
					if (item.can_be_removed_by_accelerated_healing)
					{
						listPool.Add(item);
					}
				}
				if (listPool.Count > 0)
				{
					actor.removeTraits(listPool);
					actor.setStatsDirty();
				}
				return true;
			}
		});
		add(new SubspeciesTrait
		{
			id = "rapid_aging",
			group_id = "growth",
			rarity = Rarity.R1_Rare,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true,
			remove_for_zombies = true,
			action_growth = delegate(BaseSimObject pSimObject, WorldTile _)
			{
				if (Randy.randomChance(0.5f))
				{
					pSimObject.a.data.age_overgrowth++;
				}
				if (Randy.randomChance(0.5f))
				{
					pSimObject.a.data.age_overgrowth++;
				}
				return true;
			}
		});
		add(new SubspeciesTrait
		{
			id = "good_throwers",
			group_id = "body",
			rarity = Rarity.R1_Rare,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementBallToBall");
		t.base_stats["throwing_range"] = 6f;
		add(new SubspeciesTrait
		{
			id = "fast_builders",
			group_id = "mind",
			rarity = Rarity.R1_Rare,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementCustomWorld");
		t.addOpposite("slow_builders");
		t.base_stats_meta["construction_speed"] = 2f;
		add(new SubspeciesTrait
		{
			id = "slow_builders",
			group_id = "mind",
			rarity = Rarity.R1_Rare,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.addOpposite("fast_builders");
		t.base_stats_meta["construction_speed"] = -1f;
		add(new SubspeciesTrait
		{
			id = "fins",
			group_id = "body",
			rarity = Rarity.R1_Rare,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementPiranhaLand");
		t.base_stats.addTag("fast_swimming");
		add(new SubspeciesTrait
		{
			id = "heat_resistance",
			group_id = "body",
			rarity = Rarity.R1_Rare,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementFlickIt");
		t.base_stats.addTag("immunity_fire");
		t.base_stats_meta.addTag("can_build_in_biome_infernal");
		add(new SubspeciesTrait
		{
			id = "cold_resistance",
			group_id = "body",
			rarity = Rarity.R1_Rare,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.base_stats.addTag("immunity_cold");
		t.base_stats_meta.addTag("can_build_in_biome_permafrost");
	}

	private void addStats()
	{
		add(new SubspeciesTrait
		{
			id = "exoskeleton",
			group_id = "body",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.base_stats["armor"] = 10f;
		add(new SubspeciesTrait
		{
			id = "long_lifespan",
			group_id = "body",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true,
			remove_for_zombies = true
		});
		t.base_stats["lifespan"] = 100f;
		add(new SubspeciesTrait
		{
			id = "hyper_intelligence",
			group_id = "mind",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true,
			remove_for_zombies = true
		});
		t.base_stats["intelligence"] = 30f;
		add(new SubspeciesTrait
		{
			id = "enhanced_strength",
			group_id = "body",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true
		});
		t.setUnlockedWithAchievement("achievementSuperMushroom");
		t.base_stats["damage"] = 50f;
		add(new SubspeciesTrait
		{
			id = "high_fecundity",
			group_id = "body",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = true,
			in_mutation_pot_remove = true,
			remove_for_zombies = true
		});
		t.setUnlockedWithAchievement("achievement10000Creatures");
		t.base_stats["birth_rate"] = 5f;
		add(new SubspeciesTrait
		{
			id = "unmoving",
			group_id = "body",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_add = false,
			remove_for_zombies = false,
			spawn_random_trait_allowed = false
		});
		t.setUnlockedWithAchievement("achievementSimpleStupidGenetics");
		t.base_stats.addTag("immovable");
	}

	private void addLimits()
	{
		add(new SubspeciesTrait
		{
			id = "population_minimal",
			group_id = "harmony",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_remove = false,
			in_mutation_pot_add = false,
			spawn_random_trait_allowed = false,
			priority = 100
		});
		t.addOpposite("population_small");
		t.addOpposite("population_moderate");
		t.addOpposite("population_large");
		t.addOpposite("population_expansive");
		t.base_stats_meta["limit_population"] = 50f;
		add(new SubspeciesTrait
		{
			id = "population_small",
			group_id = "harmony",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_remove = false,
			in_mutation_pot_add = false,
			spawn_random_trait_allowed = false,
			priority = 99
		});
		t.addOpposite("population_minimal");
		t.addOpposite("population_moderate");
		t.addOpposite("population_large");
		t.addOpposite("population_expansive");
		t.base_stats_meta["limit_population"] = 100f;
		add(new SubspeciesTrait
		{
			id = "population_moderate",
			group_id = "harmony",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_remove = false,
			in_mutation_pot_add = false,
			spawn_random_trait_allowed = false,
			priority = 98
		});
		t.addOpposite("population_small");
		t.addOpposite("population_minimal");
		t.addOpposite("population_large");
		t.addOpposite("population_expansive");
		t.base_stats_meta["limit_population"] = 500f;
		add(new SubspeciesTrait
		{
			id = "population_large",
			group_id = "harmony",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_remove = false,
			in_mutation_pot_add = false,
			spawn_random_trait_allowed = false,
			priority = 97
		});
		t.addOpposite("population_small");
		t.addOpposite("population_minimal");
		t.addOpposite("population_moderate");
		t.addOpposite("population_expansive");
		t.base_stats_meta["limit_population"] = 1000f;
		add(new SubspeciesTrait
		{
			id = "population_expansive",
			group_id = "harmony",
			rarity = Rarity.R0_Normal,
			in_mutation_pot_remove = false,
			in_mutation_pot_add = false,
			spawn_random_trait_allowed = false,
			priority = 96
		});
		t.addOpposite("population_small");
		t.addOpposite("population_minimal");
		t.addOpposite("population_moderate");
		t.addOpposite("population_large");
		t.base_stats_meta["limit_population"] = 3000f;
	}

	private void addMaturation()
	{
		add(new SubspeciesTrait
		{
			id = "gestation_short",
			group_id = "gestation",
			rarity = Rarity.R0_Normal,
			priority = 100,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addOpposite("gestation_moderate");
		t.addOpposite("gestation_long");
		t.addOpposite("gestation_very_long");
		t.addOpposite("gestation_extremely_long");
		t.base_stats_meta["maturation"] = 2f;
		add(new SubspeciesTrait
		{
			id = "gestation_moderate",
			group_id = "gestation",
			rarity = Rarity.R0_Normal,
			priority = 98,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addOpposite("gestation_short");
		t.addOpposite("gestation_long");
		t.addOpposite("gestation_very_long");
		t.addOpposite("gestation_extremely_long");
		t.base_stats_meta["maturation"] = 4f;
		add(new SubspeciesTrait
		{
			id = "gestation_long",
			group_id = "gestation",
			rarity = Rarity.R0_Normal,
			priority = 97,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addOpposite("gestation_short");
		t.addOpposite("gestation_moderate");
		t.addOpposite("gestation_very_long");
		t.addOpposite("gestation_extremely_long");
		t.base_stats_meta["maturation"] = 9f;
		add(new SubspeciesTrait
		{
			id = "gestation_very_long",
			group_id = "gestation",
			rarity = Rarity.R0_Normal,
			priority = 96,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addOpposite("gestation_short");
		t.addOpposite("gestation_moderate");
		t.addOpposite("gestation_long");
		t.addOpposite("gestation_extremely_long");
		t.base_stats_meta["maturation"] = 20f;
		add(new SubspeciesTrait
		{
			id = "gestation_extremely_long",
			group_id = "gestation",
			rarity = Rarity.R1_Rare,
			priority = 95,
			in_mutation_pot_add = true,
			remove_for_zombies = true
		});
		t.addOpposite("gestation_short");
		t.addOpposite("gestation_moderate");
		t.addOpposite("gestation_long");
		t.addOpposite("gestation_very_long");
		t.base_stats_meta["maturation"] = 50f;
		add(new SubspeciesTrait
		{
			id = "gmo",
			group_id = "special",
			priority = 94,
			can_be_removed = false,
			can_be_given = false,
			spawn_random_trait_allowed = false
		});
		add(new SubspeciesTrait
		{
			id = "uplifted",
			group_id = "special",
			priority = 93,
			can_be_removed = false,
			can_be_given = false,
			spawn_random_trait_allowed = false
		});
	}

	private void addAdaptations()
	{
		add(new SubspeciesTrait
		{
			id = "$adaptation$",
			group_id = "adaptations",
			remove_for_zombies = true
		});
		clone("adaptation_desert", "$adaptation$");
		t.rarity = Rarity.R0_Normal;
		t.base_stats_meta.addTag("can_build_in_biome_desert");
		t.base_stats.addTag("walk_adaptation_sand");
		clone("adaptation_swamp", "$adaptation$");
		t.rarity = Rarity.R0_Normal;
		t.base_stats_meta.addTag("can_build_in_biome_swamp");
		t.base_stats.addTag("walk_adaptation_swamp");
		clone("adaptation_wasteland", "$adaptation$");
		t.rarity = Rarity.R1_Rare;
		t.base_stats_meta.addTag("can_build_in_biome_wasteland");
		clone("adaptation_corruption", "$adaptation$");
		t.rarity = Rarity.R2_Epic;
		t.base_stats_meta.addTag("can_build_in_biome_corruption");
		clone("adaptation_permafrost", "$adaptation$");
		t.rarity = Rarity.R1_Rare;
		t.base_stats_meta.addTag("can_build_in_biome_permafrost");
		t.base_stats.addTag("walk_adaptation_snow");
		clone("adaptation_infernal", "$adaptation$");
		t.rarity = Rarity.R2_Epic;
		t.base_stats_meta.addTag("can_build_in_biome_infernal");
	}

	private void addMutations()
	{
		add(new SubspeciesTrait
		{
			id = "$skin_mutation$",
			group_id = "mutations",
			remove_for_zombies = true,
			is_mutation_skin = true,
			animation_walk = ActorAnimationSequences.walk_0_3,
			animation_swim = ActorAnimationSequences.swim_0_3,
			skin_citizen_male = AssetLibrary<SubspeciesTrait>.l<string>("male_1"),
			skin_citizen_female = AssetLibrary<SubspeciesTrait>.l<string>("female_1"),
			skin_warrior = AssetLibrary<SubspeciesTrait>.l<string>("warrior_1"),
			render_heads_for_children = true
		});
		clone("mutation_skin_burger", "$skin_mutation$");
		t.setUnlockedWithAchievement("achievementBurger");
		t.priority = 92;
		t.sprite_path = "actors/species/mutations/mutation_skin_burger";
		t.render_heads_for_children = false;
		loadSpritesPaths(t);
		clone("mutation_skin_light_orb", "$skin_mutation$");
		t.rarity = Rarity.R1_Rare;
		t.priority = 93;
		t.animation_idle = ActorAnimationSequences.walk_0_3;
		t.sprite_path = "actors/species/mutations/mutation_skin_light_orb";
		t.prevent_unconscious_rotation = true;
		t.base_stats_meta.addTag("always_idle_animation");
		t.shadow = false;
		loadSpritesPaths(t);
		clone("mutation_skin_living_rock", "$skin_mutation$");
		t.rarity = Rarity.R0_Normal;
		t.priority = 92;
		t.sprite_path = "actors/species/mutations/mutation_skin_living_rock";
		loadSpritesPaths(t);
		clone("mutation_skin_tentacle_horror", "$skin_mutation$");
		t.rarity = Rarity.R2_Epic;
		t.priority = 92;
		t.animation_idle = ActorAnimationSequences.walk_0_3;
		t.sprite_path = "actors/species/mutations/mutation_skin_tentacle_horror";
		t.prevent_unconscious_rotation = true;
		loadSpritesPaths(t);
		clone("mutation_skin_abomination", "$skin_mutation$");
		t.rarity = Rarity.R1_Rare;
		t.priority = 92;
		t.sprite_path = "actors/species/mutations/mutation_skin_abomination";
		loadSpritesPaths(t);
		clone("mutation_skin_fractal", "$skin_mutation$");
		t.priority = 92;
		t.animation_walk = ActorAnimationSequences.walk_0_5;
		t.animation_idle = ActorAnimationSequences.walk_0_5;
		t.animation_swim = ActorAnimationSequences.swim_0_5;
		t.sprite_path = "actors/species/mutations/mutation_skin_fractal";
		loadSpritesPaths(t);
		clone("mutation_skin_void", "$skin_mutation$");
		t.priority = 92;
		t.sprite_path = "actors/species/mutations/mutation_skin_void";
		loadSpritesPaths(t);
		clone("mutation_skin_metalic_orb", "$skin_mutation$");
		t.setUnlockedWithAchievement("achievementBackToBetaTesting");
		t.rarity = Rarity.R2_Epic;
		t.priority = 92;
		t.animation_idle = ActorAnimationSequences.walk_0_3;
		t.sprite_path = "actors/species/mutations/mutation_skin_metalic_orb";
		t.prevent_unconscious_rotation = true;
		loadSpritesPaths(t);
		clone("mutation_skin_blood_vortex", "$skin_mutation$");
		t.priority = 92;
		t.sprite_path = "actors/species/mutations/mutation_skin_blood_vortex";
		t.shadow_texture = "unitShadow_6";
		t.shadow_texture_baby = "unitShadow_5";
		loadSpritesPaths(t);
		clone("mutation_skin_energy", "$skin_mutation$");
		t.rarity = Rarity.R0_Normal;
		t.priority = 92;
		t.animation_idle = ActorAnimationSequences.walk_0_3;
		t.sprite_path = "actors/species/mutations/mutation_skin_energy";
		t.prevent_unconscious_rotation = true;
		t.base_stats_meta.addTag("always_idle_animation");
		t.shadow = false;
		loadSpritesPaths(t);
		addMutationOpposites();
	}

	private void addEggs()
	{
		add(new SubspeciesTrait
		{
			id = "$egg$",
			group_id = "eggs",
			phenotype_egg = true
		});
		t.action_on_augmentation_add = delegate(NanoObject pNanoObject, BaseAugmentationAsset _)
		{
			Subspecies subspecies = (Subspecies)pNanoObject;
			if (!subspecies.hasTrait("reproduction_strategy_oviparity"))
			{
				subspecies.addTrait("reproduction_strategy_oviparity", pRemoveOpposites: true);
			}
			return true;
		};
		clone("egg_shell_plain", "$egg$");
		t.rarity = Rarity.R0_Normal;
		t.base_stats_meta["maturation"] = 3f;
		clone("egg_shell_spotted", "$egg$");
		t.rarity = Rarity.R0_Normal;
		t.base_stats_meta["maturation"] = 3f;
		clone("egg_colored", "$egg$");
		t.rarity = Rarity.R0_Normal;
		t.base_stats_meta["maturation"] = 3f;
		clone("egg_roe", "$egg$");
		t.rarity = Rarity.R0_Normal;
		t.base_stats_meta["maturation"] = 3f;
		clone("egg_face", "$egg$");
		t.base_stats_meta["maturation"] = 5f;
		clone("egg_orb", "$egg$");
		t.rarity = Rarity.R2_Epic;
		t.base_stats_meta["maturation"] = 6f;
		clone("egg_eyeball", "$egg$");
		t.setUnlockedWithAchievement("achievementGodMode");
		t.rarity = Rarity.R1_Rare;
		t.animation_idle = ActorAnimationSequences.walk_0_3;
		t.base_stats_meta["maturation"] = 4f;
		clone("egg_alien", "$egg$");
		t.rarity = Rarity.R1_Rare;
		t.base_stats_meta["maturation"] = 7f;
		clone("egg_cocoon", "$egg$");
		t.rarity = Rarity.R0_Normal;
		t.base_stats_meta["maturation"] = 6f;
		clone("egg_metal_box", "$egg$");
		t.rarity = Rarity.R2_Epic;
		t.base_stats_meta["maturation"] = 15f;
		clone("egg_crystal", "$egg$");
		t.rarity = Rarity.R1_Rare;
		t.base_stats_meta["maturation"] = 10f;
		clone("egg_ice", "$egg$");
		t.rarity = Rarity.R1_Rare;
		t.after_hatch_from_egg_action = delegate(Actor pActor)
		{
			ActionLibrary.snowDropsSpawn(pActor);
		};
		t.base_stats_meta["maturation"] = 8f;
		clone("egg_blob", "$egg$");
		t.rarity = Rarity.R1_Rare;
		t.base_stats_meta["maturation"] = 2f;
		clone("egg_candy", "$egg$");
		t.rarity = Rarity.R1_Rare;
		t.base_stats_meta["maturation"] = 3f;
		clone("egg_bubble", "$egg$");
		t.rarity = Rarity.R1_Rare;
		t.base_stats_meta["maturation"] = 1f;
		clone("egg_rainbow", "$egg$");
		t.rarity = Rarity.R2_Epic;
		t.base_stats_meta["maturation"] = 3f;
		clone("egg_pumpkin", "$egg$");
		t.setUnlockedWithAchievement("achievementSocialNetwork");
		t.base_stats_meta["maturation"] = 5f;
		clone("egg_flames", "$egg$");
		t.rarity = Rarity.R2_Epic;
		t.after_hatch_from_egg_action = delegate(Actor pActor)
		{
			ActionLibrary.fireDropsSpawn(pActor);
		};
		t.base_stats_meta["maturation"] = 6f;
		addEggOpposites();
	}

	public override void post_init()
	{
		base.post_init();
		foreach (SubspeciesTrait item in list)
		{
			if (item.phenotype_egg)
			{
				if (string.IsNullOrEmpty(item.id_egg))
				{
					item.id_egg = item.id;
				}
				item.sprite_path = "eggs/" + item.id_egg;
			}
			if (item.shadow && item.is_mutation_skin)
			{
				item.texture_asset.loadShadow();
			}
		}
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (SubspeciesTrait item in list)
		{
			if (item.spawn_random_trait_allowed)
			{
				_pot_allowed_to_be_given_randomly.Add(item);
			}
			if (item.in_mutation_pot_add)
			{
				int rate = item.rarity.GetRate();
				_pot_mutation_traits_add.AddTimes(rate, item);
			}
			if (item.in_mutation_pot_remove)
			{
				int rate2 = item.rarity.GetRate();
				_pot_mutation_traits_remove.AddTimes(rate2, item);
			}
			if (item.phenotype_egg && item.after_hatch_from_egg_action != null)
			{
				item.has_after_hatch_from_egg_action = true;
			}
		}
	}

	public override void editorDiagnostic()
	{
		base.editorDiagnostic();
		foreach (SubspeciesTrait item in list)
		{
			checkSpriteExists("sprite_path", item.sprite_path, item);
		}
	}

	public SubspeciesTrait getRandomMutationTraitToAdd()
	{
		return _pot_mutation_traits_add.GetRandom();
	}

	public SubspeciesTrait getRandomMutationTraitToRemove()
	{
		return _pot_mutation_traits_remove.GetRandom();
	}

	private void addPhenotypes()
	{
		string text = "phenotype_skin";
		for (int i = 0; i < AssetManager.phenotype_library.list.Count; i++)
		{
			PhenotypeAsset phenotypeAsset = AssetManager.phenotype_library.list[i];
			string subspecies_trait_id = text + "_" + phenotypeAsset.id;
			phenotypeAsset.subspecies_trait_id = subspecies_trait_id;
		}
		foreach (PhenotypeAsset item in AssetManager.phenotype_library.list)
		{
			add(new SubspeciesTrait
			{
				id = text + "_" + item.id,
				group_id = "phenotypes",
				id_phenotype = item.id,
				phenotype_skin = true,
				priority = item.priority,
				special_icon_logic = true,
				special_locale_id = "subspecies_trait_phenotype",
				special_locale_description = "subspecies_trait_phenotype_info",
				has_description_2 = false,
				path_icon = "ui/Icons/iconPhenotype",
				spawn_random_trait_allowed = false
			});
		}
	}

	private void addMutationOpposites()
	{
		using ListPool<string> listPool = new ListPool<string>();
		foreach (SubspeciesTrait item in list)
		{
			if (item.is_mutation_skin)
			{
				listPool.Add(item.id);
			}
		}
		foreach (SubspeciesTrait item2 in list)
		{
			if (item2.is_mutation_skin)
			{
				item2.addOpposites(listPool);
				item2.removeOpposite(item2.id);
			}
		}
	}

	private void addEggOpposites()
	{
		using ListPool<string> listPool = new ListPool<string>();
		foreach (SubspeciesTrait item in list)
		{
			if (item.phenotype_egg)
			{
				listPool.Add(item.id);
			}
		}
		foreach (SubspeciesTrait item2 in list)
		{
			if (item2.phenotype_egg)
			{
				item2.addOpposites(listPool);
				item2.removeOpposite(item2.id);
			}
		}
	}

	private void loadSpritesPaths(SubspeciesTrait pAsset)
	{
		if (pAsset.is_mutation_skin)
		{
			string pBasePath = pAsset.sprite_path + "/";
			pAsset.texture_asset = new ActorTextureSubAsset(pBasePath, pHasAdvancedTextures: true);
			pAsset.texture_asset.prevent_unconscious_rotation = pAsset.prevent_unconscious_rotation;
			pAsset.texture_asset.render_heads_for_children = pAsset.render_heads_for_children;
			pAsset.texture_asset.shadow = pAsset.shadow;
			pAsset.texture_asset.shadow_texture = pAsset.shadow_texture;
			pAsset.texture_asset.shadow_texture_egg = pAsset.shadow_texture_egg;
			pAsset.texture_asset.shadow_texture_baby = pAsset.shadow_texture_baby;
		}
	}

	public void preloadMainUnitSprites()
	{
		foreach (SubspeciesTrait item in list)
		{
			if (item.is_mutation_skin)
			{
				item.texture_asset.preloadSprites(pCivTextures: true, pHasBabyForm: true, item);
			}
		}
	}
}
