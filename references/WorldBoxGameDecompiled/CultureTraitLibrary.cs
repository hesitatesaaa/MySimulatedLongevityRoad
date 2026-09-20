using System.Collections.Generic;

public class CultureTraitLibrary : BaseTraitLibrary<CultureTrait>
{
	protected override string icon_path => "ui/Icons/culture_traits/";

	protected override List<string> getDefaultTraitsForMeta(ActorAsset pAsset)
	{
		return pAsset.default_culture_traits;
	}

	public override void init()
	{
		base.init();
		addWeaponRelatedTraits();
		addTownPlansZones();
		addTownPlansTilePlacements();
		add(new CultureTrait
		{
			id = "roads",
			group_id = "architecture"
		});
		add(new CultureTrait
		{
			id = "expansionists",
			group_id = "kingdom",
			priority = 100
		});
		add(new CultureTrait
		{
			id = "tiny_legends",
			group_id = "kingdom",
			priority = 99
		});
		t.addOpposite("youth_reverence");
		add(new CultureTrait
		{
			id = "animal_whisperers",
			group_id = "miscellaneous"
		});
		add(new CultureTrait
		{
			id = "ancestors_knowledge",
			group_id = "knowledge"
		});
		t.setUnlockedWithAchievement("achievementTraitExplorerCulture");
		add(new CultureTrait
		{
			id = "statue_lovers",
			value = 4f,
			group_id = "buildings"
		});
		add(new CultureTrait
		{
			id = "tower_lovers",
			value = 4f,
			group_id = "buildings"
		});
		add(new CultureTrait
		{
			id = "training_potential",
			group_id = "knowledge"
		});
		add(new CultureTrait
		{
			id = "elder_reverence",
			value = 2f,
			group_id = "happiness"
		});
		t.addOpposite("youth_reverence");
		add(new CultureTrait
		{
			id = "youth_reverence",
			group_id = "happiness"
		});
		t.addOpposite("elder_reverence");
		t.addOpposite("tiny_legends");
		add(new CultureTrait
		{
			id = "conscription_female_only",
			group_id = "kingdom",
			priority = 90
		});
		t.addOpposite("conscription_male_only");
		add(new CultureTrait
		{
			id = "conscription_male_only",
			group_id = "kingdom",
			priority = 89
		});
		t.addOpposite("conscription_female_only");
		add(new CultureTrait
		{
			id = "fast_learners",
			group_id = "knowledge",
			value = 2f
		});
		add(new CultureTrait
		{
			id = "pep_talks",
			group_id = "happiness"
		});
		add(new CultureTrait
		{
			id = "expertise_exchange",
			value = 10f,
			group_id = "knowledge"
		});
		add(new CultureTrait
		{
			id = "happiness_from_war",
			group_id = "happiness"
		});
		add(new CultureTrait
		{
			id = "dense_dwellings",
			value = 2f,
			group_id = "harmony"
		});
		t.addOpposite("solitude_seekers");
		t.addOpposite("hive_society");
		add(new CultureTrait
		{
			id = "solitude_seekers",
			value = 0.5f,
			group_id = "harmony"
		});
		t.addOpposite("dense_dwellings");
		t.addOpposite("hive_society");
		add(new CultureTrait
		{
			id = "hive_society",
			value = 10f,
			group_id = "harmony"
		});
		t.setUnlockedWithAchievement("achievementAntWorld");
		t.addOpposite("dense_dwellings");
		t.addOpposite("solitude_seekers");
		add(new CultureTrait
		{
			id = "gossip_lovers",
			group_id = "happiness"
		});
		add(new CultureTrait
		{
			id = "reading_lovers",
			group_id = "knowledge"
		});
		add(new CultureTrait
		{
			id = "attentive_readers",
			value = 2f,
			group_id = "knowledge"
		});
		add(new CultureTrait
		{
			id = "join_or_die",
			group_id = "worldview",
			priority = 100,
			action_attack_target = delegate(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
			{
				if (!pSelf.a.isKingdomCiv())
				{
					return false;
				}
				if (!pTarget.isActor())
				{
					return false;
				}
				if (!pTarget.isAlive())
				{
					return false;
				}
				if (pTarget.a.kingdom == pSelf.kingdom)
				{
					return false;
				}
				if (!pTarget.isKingdomCiv() && !pTarget.kingdom.isNomads())
				{
					return false;
				}
				if (!pTarget.a.isSapient())
				{
					return false;
				}
				if (pTarget.a.hasTag("strong_mind"))
				{
					return false;
				}
				if (pTarget.a.getHealthRatio() > 0.3f)
				{
					return false;
				}
				if (!Randy.randomChance(0.3f))
				{
					return false;
				}
				pTarget.a.removeFromPreviousFaction();
				pTarget.a.joinKingdom(pSelf.kingdom);
				pSelf.a.clearAttackTarget();
				pTarget.a.clearAttackTarget();
				pTarget.a.applyRandomForce();
				return true;
			}
		});
		add(new CultureTrait
		{
			id = "true_roots",
			group_id = "miscellaneous"
		});
		add(new CultureTrait
		{
			id = "legacy_keepers",
			group_id = "miscellaneous"
		});
		add(new CultureTrait
		{
			id = "serenity_now",
			group_id = "worldview"
		});
		t.base_stats.addTag("love_peace");
		t.addOpposite("xenophobic");
		add(new CultureTrait
		{
			id = "weaponsmith_mastery",
			value = 5f,
			group_id = "warfare"
		});
		add(new CultureTrait
		{
			id = "armorsmith_mastery",
			value = 5f,
			group_id = "warfare"
		});
		add(new CultureTrait
		{
			id = "patriarchy",
			group_id = "succession",
			has_description_2 = false,
			priority = 97
		});
		t.addOpposite("matriarchy");
		add(new CultureTrait
		{
			id = "matriarchy",
			group_id = "succession",
			has_description_2 = false,
			priority = 96
		});
		t.addOpposite("patriarchy");
		add(new CultureTrait
		{
			id = "ultimogeniture",
			group_id = "succession",
			priority = 96
		});
		add(new CultureTrait
		{
			id = "diplomatic_ascension",
			group_id = "succession",
			priority = 100
		});
		t.base_stats.addTag("love_peace");
		t.addOpposite("warriors_ascension");
		t.addOpposite("fames_crown");
		t.addOpposite("golden_rule");
		add(new CultureTrait
		{
			id = "fames_crown",
			group_id = "succession",
			priority = 100
		});
		t.addOpposite("warriors_ascension");
		t.addOpposite("diplomatic_ascension");
		t.addOpposite("golden_rule");
		add(new CultureTrait
		{
			id = "golden_rule",
			group_id = "succession",
			priority = 100
		});
		t.addOpposite("warriors_ascension");
		t.addOpposite("fames_crown");
		t.addOpposite("diplomatic_ascension");
		add(new CultureTrait
		{
			id = "shattered_crown",
			group_id = "succession",
			priority = 60
		});
		add(new CultureTrait
		{
			id = "unbroken_chain",
			group_id = "succession",
			priority = 59
		});
		t.setUnlockedWithAchievement("achievementSuccession");
		add(new CultureTrait
		{
			id = "warriors_ascension",
			group_id = "succession",
			priority = 99
		});
		t.addOpposite("diplomatic_ascension");
		t.addOpposite("fames_crown");
		t.addOpposite("golden_rule");
		add(new CultureTrait
		{
			id = "xenophobic",
			group_id = "worldview"
		});
		t.addOpposite("xenophiles");
		t.addOpposite("serenity_now");
		add(new CultureTrait
		{
			id = "ethnocentric_guard",
			group_id = "worldview"
		});
		add(new CultureTrait
		{
			id = "xenophiles",
			group_id = "worldview"
		});
		t.base_stats.addTag("love_peace");
		t.addOpposite("xenophobic");
		add(new CultureTrait
		{
			id = "ethno_sculpted",
			group_id = "special",
			can_be_given = false,
			can_be_removed = false,
			can_be_in_book = false,
			spawn_random_trait_allowed = false
		});
		add(new CultureTrait
		{
			id = "grin_mark",
			group_id = "fate",
			spawn_random_trait_allowed = false,
			priority = -100
		});
		t.setTraitInfoToGrinMark();
		t.setUnlockedWithAchievement("achievementCreaturesExplorer");
	}

	private void addTownPlansZones()
	{
		add(new CultureTrait
		{
			id = "city_layout_architects_eye",
			group_id = "town_plan"
		});
		t.setTownLayoutPlan(TownPlans.isInPassableRingMap);
		add(new CultureTrait
		{
			id = "city_layout_madman_labyrinth",
			group_id = "town_plan"
		});
		t.setTownLayoutPlan(TownPlans.isPassableMadmanLabyrinth);
		add(new CultureTrait
		{
			id = "city_layout_honeycomb",
			group_id = "town_plan"
		});
		t.setUnlockedWithAchievement("achievementSwarm");
		t.setTownLayoutPlan(TownPlans.isPassableHoneycomb);
		add(new CultureTrait
		{
			id = "city_layout_bricks",
			group_id = "town_plan"
		});
		t.setTownLayoutPlan(TownPlans.isPassableBrickHorizontal);
		add(new CultureTrait
		{
			id = "city_layout_raindrops",
			group_id = "town_plan"
		});
		t.setTownLayoutPlan(TownPlans.isPassableBrickVertical);
		add(new CultureTrait
		{
			id = "city_layout_cross",
			group_id = "town_plan"
		});
		t.setTownLayoutPlan(TownPlans.isPassableCross);
		add(new CultureTrait
		{
			id = "city_layout_claws",
			group_id = "town_plan"
		});
		t.setTownLayoutPlan(TownPlans.isPassableDiagonal);
		add(new CultureTrait
		{
			id = "city_layout_pebbles",
			group_id = "town_plan",
			priority = 50
		});
		t.setTownLayoutPlan(TownPlans.isPassableClustersSmall);
		add(new CultureTrait
		{
			id = "city_layout_stone_garden",
			group_id = "town_plan",
			priority = 49
		});
		t.setTownLayoutPlan(TownPlans.isPassableClustersMedium);
		add(new CultureTrait
		{
			id = "city_layout_titan_footprints",
			group_id = "town_plan",
			priority = 48
		});
		t.setTownLayoutPlan(TownPlans.isPassableClustersBig);
		add(new CultureTrait
		{
			id = "city_layout_silk_web",
			group_id = "town_plan",
			priority = 47
		});
		t.setTownLayoutPlan(TownPlans.isPassableLatticeSmall);
		add(new CultureTrait
		{
			id = "city_layout_iron_weave",
			group_id = "town_plan",
			priority = 46
		});
		t.setTownLayoutPlan(TownPlans.isPassableLatticeMedium);
		add(new CultureTrait
		{
			id = "city_layout_monolith_mesh",
			group_id = "town_plan",
			priority = 45
		});
		t.setTownLayoutPlan(TownPlans.isPassableLatticeBig);
		add(new CultureTrait
		{
			id = "city_layout_royal_checkers",
			group_id = "town_plan",
			priority = 44
		});
		t.setTownLayoutPlan(TownPlans.isPassableDiamondCluster);
		add(new CultureTrait
		{
			id = "city_layout_diamond",
			group_id = "town_plan",
			priority = 43
		});
		t.setTownLayoutPlan(TownPlans.isPassableDiamond);
		add(new CultureTrait
		{
			id = "city_layout_parallels",
			group_id = "town_plan",
			priority = 42
		});
		t.setTownLayoutPlan(TownPlans.isPassableLineHorizontal);
		add(new CultureTrait
		{
			id = "city_layout_pillars",
			group_id = "town_plan",
			priority = 41
		});
		t.setTownLayoutPlan(TownPlans.isPassableLineVertical);
		add(new CultureTrait
		{
			id = "city_layout_rings",
			group_id = "town_plan"
		});
		t.setTownLayoutPlan(TownPlans.isInPassableRing);
		addTownLayoutOpposites();
	}

	private void addTownPlansTilePlacements()
	{
		add(new CultureTrait
		{
			id = "buildings_spread",
			group_id = "town_plan",
			priority = 100
		});
		add(new CultureTrait
		{
			id = "city_layout_tile_wobbly_pattern",
			group_id = "town_plan",
			priority = 98
		});
		t.addOpposite("city_layout_the_grand_arrangement");
		t.addOpposite("city_layout_tile_moonsteps");
		add(new CultureTrait
		{
			id = "city_layout_the_grand_arrangement",
			group_id = "town_plan",
			priority = 99
		});
		t.addOpposite("city_layout_tile_wobbly_pattern");
		t.addOpposite("city_layout_tile_moonsteps");
		add(new CultureTrait
		{
			id = "city_layout_tile_moonsteps",
			group_id = "town_plan",
			priority = 97
		});
		t.addOpposite("city_layout_tile_wobbly_pattern");
		t.addOpposite("city_layout_the_grand_arrangement");
	}

	private void addTownLayoutOpposites()
	{
		using ListPool<string> listPool = new ListPool<string>();
		foreach (CultureTrait item in list)
		{
			if (item.town_layout_plan)
			{
				listPool.Add(item.id);
			}
		}
		foreach (CultureTrait item2 in list)
		{
			if (item2.town_layout_plan)
			{
				item2.addOpposites(listPool);
				item2.removeOpposite(item2.id);
			}
		}
	}

	private void addWeaponRelatedTraits()
	{
		add(new CultureTrait
		{
			id = "axe_lovers",
			value = 10f,
			group_id = "weapons",
			is_weapon_trait = true
		});
		t.addWeaponSubtype("axe");
		add(new CultureTrait
		{
			id = "sword_lovers",
			value = 10f,
			group_id = "weapons",
			is_weapon_trait = true
		});
		t.addWeaponSubtype("sword");
		add(new CultureTrait
		{
			id = "bow_lovers",
			value = 6f,
			group_id = "weapons",
			is_weapon_trait = true
		});
		t.addWeaponSubtype("bow");
		add(new CultureTrait
		{
			id = "hammer_lovers",
			value = 10f,
			group_id = "weapons",
			is_weapon_trait = true
		});
		t.addWeaponSubtype("hammer");
		add(new CultureTrait
		{
			id = "spear_lovers",
			value = 10f,
			group_id = "weapons",
			is_weapon_trait = true
		});
		t.addWeaponSubtype("spear");
		add(new CultureTrait
		{
			id = "craft_flame_weapon",
			value = 10f,
			group_id = "craft",
			is_weapon_trait = true
		});
		t.addWeaponSpecial("flame_sword");
		t.addWeaponSpecial("flame_hammer");
		add(new CultureTrait
		{
			id = "craft_ice_weapon",
			value = 10f,
			group_id = "craft",
			is_weapon_trait = true
		});
		t.addWeaponSpecial("ice_hammer");
		add(new CultureTrait
		{
			id = "craft_evil_staff",
			value = 10f,
			group_id = "craft",
			is_weapon_trait = true
		});
		t.addWeaponSpecial("evil_staff");
		add(new CultureTrait
		{
			id = "craft_white_staff",
			value = 10f,
			group_id = "craft",
			is_weapon_trait = true
		});
		t.addWeaponSpecial("white_staff");
		add(new CultureTrait
		{
			id = "craft_necro_staff",
			value = 10f,
			group_id = "craft",
			is_weapon_trait = true
		});
		t.addWeaponSpecial("necromancer_staff");
		add(new CultureTrait
		{
			id = "craft_druid_staff",
			value = 10f,
			group_id = "craft",
			is_weapon_trait = true
		});
		t.addWeaponSpecial("druid_staff");
		add(new CultureTrait
		{
			id = "craft_doctor_staff",
			value = 10f,
			group_id = "craft",
			is_weapon_trait = true
		});
		t.addWeaponSpecial("plague_doctor_staff");
		add(new CultureTrait
		{
			id = "craft_shotgun",
			value = 10f,
			group_id = "craft",
			is_weapon_trait = true,
			priority = -1,
			spawn_random_trait_allowed = false
		});
		t.setUnlockedWithAchievement("achievementSwordWithShotgun");
		t.addWeaponSpecial("shotgun");
		add(new CultureTrait
		{
			id = "craft_blaster",
			value = 10f,
			group_id = "craft",
			is_weapon_trait = true,
			priority = -2,
			spawn_random_trait_allowed = false
		});
		t.addWeaponSpecial("alien_blaster");
	}

	public static float getValueFloat(string pID)
	{
		return AssetManager.culture_traits.get(pID).value;
	}

	public static int getValue(string pID)
	{
		return (int)AssetManager.culture_traits.get(pID).value;
	}
}
