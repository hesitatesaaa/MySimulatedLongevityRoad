using System;
using System.Collections.Generic;

public class ReligionTraitLibrary : BaseTraitLibrary<ReligionTrait>
{
	private const string TEMPLATE_MAGIC_SPELL = "$magic_spell$";

	private const string TEMPLATE_TRANSFORMATION = "$transformation$";

	protected override string icon_path => "ui/Icons/religion_traits/";

	protected override List<string> getDefaultTraitsForMeta(ActorAsset pAsset)
	{
		return pAsset.default_religion_traits;
	}

	public override void init()
	{
		base.init();
		addMagicSpells();
		addTransformations();
		addHarmony();
		addRites();
		addSpecial();
		add(new ReligionTrait
		{
			id = "grin_mark",
			group_id = "fate",
			spawn_random_trait_allowed = false,
			priority = -100
		});
		t.setTraitInfoToGrinMark();
		t.setUnlockedWithAchievement("achievementCreaturesExplorer");
	}

	private void addTransformations()
	{
		add(new ReligionTrait
		{
			id = "$transformation$",
			group_id = "transformation",
			spawn_random_trait_allowed = false
		});
		clone("sands_of_ruin", "$transformation$");
		t.transformation_biome_id = "biome_desert";
		clone("shadowroot", "$transformation$");
		t.transformation_biome_id = "biome_corrupted";
		clone("echo_of_the_void", "$transformation$");
		t.transformation_biome_id = "biome_singularity";
		clone("infernal_rot", "$transformation$");
		t.transformation_biome_id = "biome_infernal";
		clone("cosmic_radiation", "$transformation$");
		t.transformation_biome_id = "biome_wasteland";
	}

	private void addSpecial()
	{
		add(new ReligionTrait
		{
			id = "divine_insight",
			group_id = "special",
			can_be_given = false,
			can_be_removed = false,
			can_be_in_book = false,
			spawn_random_trait_allowed = false
		});
		add(new ReligionTrait
		{
			id = "bloodline_bond",
			group_id = "the_void"
		});
	}

	private void addRites()
	{
		add(new ReligionTrait
		{
			id = "rite_of_change",
			group_id = "the_void",
			plot_id = "clan_ascension",
			priority = -1
		});
		ReligionTrait religionTrait = t;
		religionTrait.action_death = (WorldAction)Delegate.Combine(religionTrait.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_shattered_earth",
			group_id = "destruction",
			plot_id = "summon_earthquake",
			priority = -1
		});
		t.setUnlockedWithAchievement("achievementWatchYourMouth");
		ReligionTrait religionTrait2 = t;
		religionTrait2.action_death = (WorldAction)Delegate.Combine(religionTrait2.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_falling_stars",
			group_id = "destruction",
			plot_id = "summon_meteor_rain",
			priority = -1
		});
		t.setUnlockedWithAchievement("achievementMayday");
		ReligionTrait religionTrait3 = t;
		religionTrait3.action_death = (WorldAction)Delegate.Combine(religionTrait3.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_roaring_skies",
			group_id = "destruction",
			plot_id = "summon_thunderstorm",
			priority = -1
		});
		t.setUnlockedWithAchievement("achievementMayIInterrupt");
		ReligionTrait religionTrait4 = t;
		religionTrait4.action_death = (WorldAction)Delegate.Combine(religionTrait4.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_tempest_call",
			group_id = "destruction",
			plot_id = "summon_stormfront",
			priority = -1
		});
		ReligionTrait religionTrait5 = t;
		religionTrait5.action_death = (WorldAction)Delegate.Combine(religionTrait5.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_infernal_wrath",
			group_id = "destruction",
			plot_id = "summon_hellstorm",
			priority = -1
		});
		ReligionTrait religionTrait6 = t;
		religionTrait6.action_death = (WorldAction)Delegate.Combine(religionTrait6.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_the_abyss",
			group_id = "destruction",
			plot_id = "summon_demons",
			priority = -1
		});
		ReligionTrait religionTrait7 = t;
		religionTrait7.action_death = (WorldAction)Delegate.Combine(religionTrait7.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_infinite_edges",
			group_id = "protection",
			plot_id = "summon_angles",
			priority = -1
		});
		ReligionTrait religionTrait8 = t;
		religionTrait8.action_death = (WorldAction)Delegate.Combine(religionTrait8.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_restless_dead",
			group_id = "necromancy",
			plot_id = "summon_skeletons",
			priority = -1
		});
		ReligionTrait religionTrait9 = t;
		religionTrait9.action_death = (WorldAction)Delegate.Combine(religionTrait9.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_fractured_minds",
			group_id = "the_void",
			plot_id = "big_cast_madness",
			priority = -1
		});
		t.setUnlockedWithAchievement("achievementGreg");
		ReligionTrait religionTrait10 = t;
		religionTrait10.action_death = (WorldAction)Delegate.Combine(religionTrait10.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_dissent",
			group_id = "the_void",
			plot_id = "cause_rebellion",
			priority = -1
		});
		t.setUnlockedWithAchievement("achievementNotJustACult");
		ReligionTrait religionTrait11 = t;
		religionTrait11.action_death = (WorldAction)Delegate.Combine(religionTrait11.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_unbroken_shield",
			group_id = "protection",
			plot_id = "big_cast_bubble_shield",
			priority = -1
		});
		ReligionTrait religionTrait12 = t;
		religionTrait12.action_death = (WorldAction)Delegate.Combine(religionTrait12.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_eternal_brew",
			group_id = "necromancy",
			plot_id = "big_cast_coffee",
			priority = -1
		});
		t.setUnlockedWithAchievement("achievementGodFingerLightning");
		ReligionTrait religionTrait13 = t;
		religionTrait13.action_death = (WorldAction)Delegate.Combine(religionTrait13.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_entanglement",
			group_id = "creation",
			plot_id = "big_cast_slowness",
			priority = -1
		});
		ReligionTrait religionTrait14 = t;
		religionTrait14.action_death = (WorldAction)Delegate.Combine(religionTrait14.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ReligionTrait
		{
			id = "rite_of_living_harvest",
			group_id = "creation",
			plot_id = "summon_living_plants",
			priority = -1
		});
		ReligionTrait religionTrait15 = t;
		religionTrait15.action_death = (WorldAction)Delegate.Combine(religionTrait15.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
	}

	private void addHarmony()
	{
		add(new ReligionTrait
		{
			id = "minds_awakening",
			group_id = "harmony"
		});
		t.base_stats["intelligence"] = 10f;
		add(new ReligionTrait
		{
			id = "zeal_of_conquest",
			group_id = "harmony"
		});
		t.base_stats["warfare"] = 10f;
		add(new ReligionTrait
		{
			id = "path_of_unity",
			group_id = "harmony"
		});
		t.base_stats["diplomacy"] = 10f;
		add(new ReligionTrait
		{
			id = "hand_of_order",
			group_id = "harmony"
		});
		t.base_stats["stewardship"] = 10f;
	}

	private void addMagicSpells()
	{
		add(new ReligionTrait
		{
			id = "$magic_spell$",
			group_id = "creation"
		});
		ReligionTrait religionTrait = t;
		religionTrait.action_death = (WorldAction)Delegate.Combine(religionTrait.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		clone("teleport", "$magic_spell$");
		t.setUnlockedWithAchievement("achievementTraitExplorerReligion");
		t.group_id = "the_void";
		t.addSpell("teleport");
		clone("cast_silence", "$magic_spell$");
		t.group_id = "the_void";
		t.addSpell("cast_silence");
		clone("summon_lightning", "$magic_spell$");
		t.group_id = "destruction";
		t.addSpell("summon_lightning");
		clone("summon_tornado", "$magic_spell$");
		t.group_id = "destruction";
		t.addSpell("summon_tornado");
		clone("cast_curse", "$magic_spell$");
		t.addSpell("cast_curse");
		clone("cast_fire", "$magic_spell$");
		t.group_id = "destruction";
		t.addSpell("cast_fire");
		clone("cast_blood_rain", "$magic_spell$");
		t.group_id = "restoration";
		t.addSpell("cast_blood_rain");
		clone("cast_grass_seeds", "$magic_spell$");
		t.group_id = "creation";
		t.addSpell("cast_grass_seeds");
		clone("spawn_vegetation", "$magic_spell$");
		t.group_id = "creation";
		t.addSpell("spawn_vegetation");
		clone("spawn_skeleton", "$magic_spell$");
		t.setUnlockedWithAchievement("achievementTheCorruptedTrees");
		t.group_id = "necromancy";
		t.addSpell("spawn_skeleton");
		clone("cast_shield", "$magic_spell$");
		t.group_id = "protection";
		t.addSpell("cast_shield");
		clone("cast_cure", "$magic_spell$");
		t.group_id = "restoration";
		t.addSpell("cast_cure");
		add(new ReligionTrait
		{
			id = "blessed_by_ashes",
			group_id = "protection"
		});
		t.setUnlockedWithAchievement("achievementSacrifice");
		t.base_stats_meta.addTag("building_immunity_fire");
	}
}
