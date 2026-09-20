using System;
using System.Collections.Generic;
using Beebyte.Obfuscator;

[Serializable]
[ObfuscateLiterals]
public class ActorTraitLibrary : BaseTraitLibrary<ActorTrait>
{
	public const int COMBAT_SKILLS_AMOUNT = 5;

	[NonSerialized]
	public List<ActorTrait> pot_traits_mutation_box = new List<ActorTrait>();

	[NonSerialized]
	public List<ActorTrait> pot_traits_birth = new List<ActorTrait>();

	[NonSerialized]
	public List<ActorTrait> pot_traits_growup = new List<ActorTrait>();

	[NonSerialized]
	public List<ActorTrait> pot_traits_combat = new List<ActorTrait>();

	protected override List<string> getDefaultTraitsForMeta(ActorAsset pAsset)
	{
		return pAsset.traits;
	}

	public override void init()
	{
		base.init();
		addTraitsSpecial();
		addTraitsBody();
		addTraitsMind();
		addTraitsSpirit();
		addTraitsAcquired();
		addTraitsFun();
		addTraitsMisc();
	}

	private void addTraitsBody()
	{
		add(new ActorTrait
		{
			id = "dash",
			path_icon = "ui/Icons/skills/iconSkillDash",
			group_id = "skills",
			in_training_dummy_combat_pot = true
		});
		t.addCombatAction("combat_dash");
		add(new ActorTrait
		{
			id = "block",
			path_icon = "ui/Icons/skills/iconSkillBlock",
			group_id = "skills",
			in_training_dummy_combat_pot = true
		});
		t.addCombatAction("combat_block");
		add(new ActorTrait
		{
			id = "dodge",
			path_icon = "ui/Icons/skills/iconSkillDodge",
			group_id = "skills",
			in_training_dummy_combat_pot = true
		});
		t.addCombatAction("combat_dodge");
		add(new ActorTrait
		{
			id = "backstep",
			path_icon = "ui/Icons/skills/iconSkillBackstep",
			group_id = "skills",
			in_training_dummy_combat_pot = true
		});
		t.addCombatAction("combat_backstep");
		add(new ActorTrait
		{
			id = "deflect_projectile",
			path_icon = "ui/Icons/skills/iconSkillDeflectProjectile",
			group_id = "skills",
			in_training_dummy_combat_pot = true
		});
		t.addCombatAction("combat_deflect_projectile");
		add(new ActorTrait
		{
			id = "mute",
			path_icon = "ui/Icons/actor_traits/iconMute",
			group_id = "body",
			rate_birth = 1,
			rate_inherit = 5,
			likeability = -0.1f
		});
		add(new ActorTrait
		{
			id = "sunblessed",
			path_icon = "ui/Icons/actor_traits/iconSunblessed",
			group_id = "body",
			rate_birth = 2,
			rate_inherit = 5
		});
		t.special_effect_interval = 5f;
		ActorTrait actorTrait = t;
		actorTrait.action_special_effect = (WorldAction)Delegate.Combine(actorTrait.action_special_effect, new WorldAction(ActionLibrary.sunblessedEffect));
		add(new ActorTrait
		{
			id = "clumsy",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconClumsy",
			group_id = "physique",
			rate_birth = 5,
			rate_inherit = 5
		});
		t.addOpposite("long_liver");
		t.base_stats["multiplier_lifespan"] = -0.5f;
		add(new ActorTrait
		{
			id = "fragile_health",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconFrail",
			group_id = "health",
			rate_birth = 5,
			rate_inherit = 5
		});
		t.addOpposite("boosted_vitality");
		t.base_stats["multiplier_health"] = -0.5f;
		add(new ActorTrait
		{
			id = "boosted_vitality",
			path_icon = "ui/Icons/actor_traits/iconBoostedVitality",
			group_id = "health",
			rate_birth = 5,
			rate_inherit = 5
		});
		t.addOpposite("fragile_health");
		t.base_stats["multiplier_health"] = 0.5f;
		add(new ActorTrait
		{
			id = "hard_skin",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconHardSkin",
			group_id = "physique",
			rate_birth = 5,
			rate_inherit = 5
		});
		t.addOpposite("soft_skin");
		t.base_stats["armor"] = 5f;
		add(new ActorTrait
		{
			id = "soft_skin",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconSoftSkin",
			group_id = "physique",
			rate_birth = 5,
			rate_inherit = 5
		});
		t.addOpposite("hard_skin");
		t.base_stats["armor"] = -5f;
		add(new ActorTrait
		{
			id = "long_liver",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconLongLiver",
			group_id = "health",
			rate_birth = 2,
			rate_inherit = 5
		});
		t.addOpposite("clumsy");
		t.base_stats["multiplier_lifespan"] = 0.5f;
		add(new ActorTrait
		{
			id = "acid_touch",
			path_icon = "ui/Icons/actor_traits/iconAcidTouch",
			unlocked_with_achievement = true,
			achievement_id = "achievementLetsNot",
			group_id = "body",
			likeability = -0.1f
		});
		ActorTrait actorTrait2 = t;
		actorTrait2.action_special_effect = (WorldAction)Delegate.Combine(actorTrait2.action_special_effect, new WorldAction(ActionLibrary.acidTouchEffect));
		add(new ActorTrait
		{
			id = "acid_blood",
			path_icon = "ui/Icons/actor_traits/iconAcidBlood",
			unlocked_with_achievement = true,
			achievement_id = "achievementLetsNot",
			group_id = "body",
			rate_inherit = 5,
			likeability = -0.1f
		});
		ActorTrait actorTrait3 = t;
		actorTrait3.action_death = (WorldAction)Delegate.Combine(actorTrait3.action_death, new WorldAction(ActionLibrary.acidBloodEffect));
		add(new ActorTrait
		{
			id = "acid_proof",
			path_icon = "ui/Icons/actor_traits/iconAcidProof",
			unlocked_with_achievement = true,
			achievement_id = "achievementLetsNot",
			group_id = "protection",
			rate_inherit = 5
		});
		add(new ActorTrait
		{
			id = "fire_blood",
			path_icon = "ui/Icons/actor_traits/iconFireBlood",
			group_id = "body",
			rate_inherit = 5
		});
		ActorTrait actorTrait4 = t;
		actorTrait4.action_death = (WorldAction)Delegate.Combine(actorTrait4.action_death, new WorldAction(ActionLibrary.fireDropsSpawn));
		add(new ActorTrait
		{
			id = "fire_proof",
			path_icon = "ui/Icons/actor_traits/iconFireProof",
			group_id = "protection",
			rate_inherit = 5
		});
		t.base_stats.addTag("immunity_fire");
		add(new ActorTrait
		{
			id = "freeze_proof",
			path_icon = "ui/Icons/actor_traits/iconFreezeProof",
			group_id = "protection",
			rate_inherit = 5
		});
		t.base_stats.addTag("immunity_cold");
		add(new ActorTrait
		{
			id = "regeneration",
			path_icon = "ui/Icons/actor_traits/iconRegeneration",
			rate_birth = 1,
			rate_inherit = 5,
			group_id = "health",
			type = TraitType.Positive,
			special_effect_interval = 3f
		});
		ActorTrait actorTrait5 = t;
		actorTrait5.action_special_effect = (WorldAction)Delegate.Combine(actorTrait5.action_special_effect, new WorldAction(ActionLibrary.regenerationEffect));
		add(new ActorTrait
		{
			id = "heliophobia",
			path_icon = "ui/Icons/actor_traits/iconHeliophobia",
			rate_inherit = 10,
			group_id = "body",
			type = TraitType.Negative,
			special_effect_interval = 10f
		});
		ActorTrait actorTrait6 = t;
		actorTrait6.action_special_effect = (WorldAction)Delegate.Combine(actorTrait6.action_special_effect, new WorldAction(ActionLibrary.heliophobiaEffect));
		add(new ActorTrait
		{
			id = "ugly",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconUgly",
			rate_birth = 7,
			same_trait_mod = 5,
			opposite_trait_mod = -15,
			likeability = -0.1f,
			group_id = "appearance",
			type = TraitType.Negative
		});
		t.base_stats["multiplier_offspring"] = -0.3f;
		t.addOpposite("attractive");
		add(new ActorTrait
		{
			id = "fat",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconFat",
			rate_birth = 7,
			rate_inherit = 5,
			opposite_trait_mod = -10,
			same_trait_mod = 10,
			likeability = -0.1f,
			group_id = "physique",
			type = TraitType.Negative
		});
		t.addOpposite("agile");
		t.addOpposite("weightless");
		t.base_stats["multiplier_mass"] = 0.3f;
		t.base_stats["scale"] = 0.02f;
		t.base_stats["multiplier_stamina"] = -0.5f;
		t.base_stats["multiplier_damage"] = 0.1f;
		add(new ActorTrait
		{
			id = "attractive",
			path_icon = "ui/Icons/actor_traits/iconAttractive",
			rate_birth = 3,
			rate_inherit = 5,
			same_trait_mod = 10,
			likeability = 0.1f,
			group_id = "appearance",
			type = TraitType.Positive
		});
		t.addOpposite("ugly");
		t.base_stats["diplomacy"] = 2f;
		t.base_stats["stewardship"] = 1f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["multiplier_offspring"] = 0.6f;
		add(new ActorTrait
		{
			id = "fast",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconFast",
			rate_birth = 4,
			rate_inherit = 5,
			remove_for_zombie_actor_asset = true,
			group_id = "physique",
			type = TraitType.Positive
		});
		t.addOpposite("slow");
		t.base_stats["multiplier_speed"] = 0.3f;
		t.base_stats["attack_speed"] = 5f;
		add(new ActorTrait
		{
			id = "slow",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconSlow",
			rate_birth = 6,
			rate_inherit = 5,
			group_id = "physique",
			type = TraitType.Negative
		});
		t.addOpposite("fast");
		t.addOpposite("agile");
		t.base_stats["multiplier_speed"] = -0.5f;
		t.base_stats["attack_speed"] = -5f;
		add(new ActorTrait
		{
			id = "gluttonous",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconGluttonous",
			rate_birth = 4,
			rate_inherit = 5,
			same_trait_mod = 5,
			group_id = "mind",
			type = TraitType.Negative
		});
		add(new ActorTrait
		{
			id = "giant",
			path_icon = "ui/Icons/actor_traits/iconGiant",
			group_id = "physique",
			type = TraitType.Positive,
			rate_birth = 2,
			rate_inherit = 7,
			unlocked_with_achievement = true,
			achievement_id = "achievementTORNADO"
		});
		t.addOpposite("tiny");
		t.base_stats["scale"] = 0.05f;
		t.base_stats["multiplier_health"] = 0.5f;
		t.base_stats["multiplier_speed"] = -0.25f;
		add(new ActorTrait
		{
			id = "tiny",
			path_icon = "ui/Icons/actor_traits/iconTiny",
			group_id = "physique",
			type = TraitType.Negative,
			rate_birth = 4,
			rate_inherit = 7,
			unlocked_with_achievement = true,
			achievement_id = "achievementBabyTornado"
		});
		t.addOpposite("giant");
		t.base_stats["diplomacy"] = -1f;
		t.base_stats["scale"] = -0.02f;
		t.base_stats["multiplier_health"] = -0.25f;
		t.base_stats["multiplier_speed"] = 0.25f;
		add(new ActorTrait
		{
			id = "eagle_eyed",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconEagleEye",
			rate_birth = 3,
			rate_inherit = 5,
			group_id = "cognitive",
			type = TraitType.Positive
		});
		t.addOpposite("short_sighted");
		t.base_stats["accuracy"] = 5f;
		t.base_stats["critical_chance"] = 0.15f;
		add(new ActorTrait
		{
			id = "short_sighted",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconShortsighted",
			rate_birth = 5,
			rate_inherit = 5,
			group_id = "cognitive",
			type = TraitType.Negative
		});
		t.addOpposite("eagle_eyed");
		t.base_stats["accuracy"] = -5f;
		t.base_stats["critical_chance"] = -0.05f;
		add(new ActorTrait
		{
			id = "infertile",
			path_icon = "ui/Icons/actor_traits/iconInfertile",
			rate_birth = 1,
			rate_inherit = 5,
			group_id = "health",
			type = TraitType.Negative
		});
		t.addOpposite("fertile");
		t.base_stats["offspring"] = -99999f;
		add(new ActorTrait
		{
			id = "fertile",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconFertile",
			rate_birth = 3,
			rate_inherit = 7,
			group_id = "health",
			type = TraitType.Positive,
			likeability = 0.1f
		});
		t.addOpposite("infertile");
		t.base_stats["multiplier_offspring"] = 0.8f;
		t.base_stats["birth_rate"] = 4f;
		add(new ActorTrait
		{
			id = "thorns",
			path_icon = "ui/Icons/actor_traits/iconThorns",
			group_id = "protection",
			rate_inherit = 5
		});
		ActorTrait actorTrait7 = t;
		actorTrait7.action_get_hit = (GetHitAction)Delegate.Combine(actorTrait7.action_get_hit, new GetHitAction(ActionLibrary.thornsDefense));
		add(new ActorTrait
		{
			id = "bubble_defense",
			path_icon = "ui/Icons/actor_traits/iconBubbleDefense",
			group_id = "protection",
			rate_inherit = 3
		});
		ActorTrait actorTrait8 = t;
		actorTrait8.action_get_hit = (GetHitAction)Delegate.Combine(actorTrait8.action_get_hit, new GetHitAction(ActionLibrary.bubbleDefense));
		add(new ActorTrait
		{
			id = "immune",
			path_icon = "ui/Icons/actor_traits/iconImmune",
			rate_birth = 1,
			rate_inherit = 10,
			group_id = "health",
			type = TraitType.Positive
		});
		t.addOpposite("plague");
		t.addOpposite("tumor_infection");
		t.addOpposite("mush_spores");
		t.addOpposite("infected");
		add(new ActorTrait
		{
			id = "agile",
			path_icon = "ui/Icons/actor_traits/iconAgile",
			rate_birth = 3,
			rate_inherit = 5,
			same_trait_mod = 5,
			remove_for_zombie_actor_asset = true,
			group_id = "physique",
			type = TraitType.Positive
		});
		t.addOpposite("fat");
		t.addOpposite("slow");
		t.base_stats["lifespan"] = 3f;
		t.base_stats["scale"] = -0.01f;
		t.base_stats["stamina"] = 20f;
		t.base_stats["skill_combat"] = 0.2f;
		add(new ActorTrait
		{
			id = "weightless",
			path_icon = "ui/Icons/actor_traits/iconWeightless",
			rate_birth = 1,
			rate_inherit = 5,
			group_id = "physique"
		});
		t.addOpposite("fat");
		add(new ActorTrait
		{
			id = "poisonous",
			path_icon = "ui/Icons/actor_traits/iconPoisonous",
			group_id = "body",
			rate_inherit = 5
		});
		add(new ActorTrait
		{
			id = "venomous",
			path_icon = "ui/Icons/actor_traits/iconVenomous",
			group_id = "body",
			rate_inherit = 5
		});
		t.action_attack_target = ActionLibrary.addPoisonedEffectOnTarget;
		add(new ActorTrait
		{
			id = "poison_immune",
			path_icon = "ui/Icons/actor_traits/iconPoisonImmune",
			group_id = "protection",
			rate_inherit = 5
		});
		add(new ActorTrait
		{
			id = "tough",
			path_icon = "ui/Icons/actor_traits/iconTough",
			rate_birth = 2,
			group_id = "physique",
			type = TraitType.Positive,
			same_trait_mod = -5,
			unlocked_with_achievement = true,
			achievement_id = "achievementDestroyWorldBox"
		});
		t.base_stats["armor"] = 10f;
		t.base_stats["warfare"] = 1f;
		t.base_stats["lifespan"] = 4f;
		add(new ActorTrait
		{
			id = "strong",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconStrong",
			rate_birth = 4,
			opposite_trait_mod = -10,
			same_trait_mod = 5,
			group_id = "physique",
			type = TraitType.Positive
		});
		t.addOpposite("weak");
		t.base_stats["multiplier_damage"] = 0.5f;
		t.base_stats["warfare"] = 2f;
		t.base_stats["lifespan"] = 3f;
		add(new ActorTrait
		{
			id = "weak",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconWeak",
			rate_birth = 5,
			opposite_trait_mod = -10,
			group_id = "physique",
			type = TraitType.Negative
		});
		t.addOpposite("strong");
		t.base_stats["multiplier_damage"] = -0.5f;
		t.base_stats["warfare"] = -2f;
		t.base_stats["diplomacy"] = -2f;
		t.base_stats["lifespan"] = -6f;
	}

	private void addTraitsMind()
	{
		add(new ActorTrait
		{
			id = "lustful",
			path_icon = "ui/Icons/actor_traits/iconLustful",
			group_id = "mind",
			rate_acquire_grow_up = 5,
			rate_birth = 1,
			likeability = 0.1f
		});
		t.base_stats["diplomacy"] = -2f;
		t.base_stats["multiplier_offspring"] = 0.2f;
		add(new ActorTrait
		{
			id = "miner",
			path_icon = "ui/Icons/actor_traits/iconMiner",
			group_id = "miscellaneous",
			type = TraitType.Positive,
			rate_acquire_grow_up = 5
		});
		add(new ActorTrait
		{
			id = "psychopath",
			path_icon = "ui/Icons/actor_traits/iconPsychopath",
			group_id = "mind",
			rate_birth = 1,
			type = TraitType.Negative
		});
		add(new ActorTrait
		{
			id = "strong_minded",
			path_icon = "ui/Icons/actor_traits/iconStrongMinded",
			group_id = "mind",
			type = TraitType.Positive,
			remove_for_zombie_actor_asset = true
		});
		t.base_stats.addTag("strong_mind");
		t.addOpposite("madness");
		t.addOpposite("desire_golden_egg");
		t.addOpposite("desire_harp");
		add(new ActorTrait
		{
			id = "peaceful",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconPeaceful",
			remove_for_zombie_actor_asset = true,
			group_id = "mind",
			type = TraitType.Positive
		});
		t.base_stats.addTag("love_peace");
		add(new ActorTrait
		{
			id = "evil",
			path_icon = "ui/Icons/actor_traits/iconEvil",
			group_id = "mind",
			likeability = -0.2f
		});
		t.addOpposite("blessed");
		t.base_stats["cities"] = -2f;
		t.base_stats["warfare"] = 10f;
		add(new ActorTrait
		{
			id = "hotheaded",
			path_icon = "ui/Icons/actor_traits/iconHotheaded",
			rate_birth = 1,
			same_trait_mod = -10,
			group_id = "mind",
			type = TraitType.Negative
		});
		add(new ActorTrait
		{
			id = "thief",
			path_icon = "ui/Icons/actor_traits/iconThief",
			rate_birth = 1,
			same_trait_mod = 10,
			group_id = "cognitive",
			type = TraitType.Negative
		});
		t.setUnlockedWithAchievement("achievementNotOnMyWatch");
		t.addOpposite("honest");
		t.addOpposite("content");
		t.base_stats.addTag("steal_items");
		t.addDecision("try_to_steal_money");
		add(new ActorTrait
		{
			id = "stupid",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconStupid",
			rate_birth = 3,
			same_trait_mod = 30,
			group_id = "cognitive",
			type = TraitType.Negative
		});
		t.addOpposite("genius");
		t.addOpposite("wise");
		t.base_stats["damage"] = 5f;
		t.base_stats["cities"] = -3f;
		t.base_stats["intelligence"] = -5f;
		t.base_stats["diplomacy"] = -2f;
		t.base_stats["warfare"] = -2f;
		t.base_stats["stewardship"] = -5f;
		t.base_stats["loyalty_traits"] = -15f;
		t.base_stats["personality_rationality"] = -0.5f;
		add(new ActorTrait
		{
			id = "genius",
			path_icon = "ui/Icons/actor_traits/iconGenius",
			rate_birth = 1,
			remove_for_zombie_actor_asset = true,
			same_trait_mod = 20,
			opposite_trait_mod = -20,
			unlocked_with_achievement = true,
			achievement_id = "achievementTraitsExplorer60",
			group_id = "cognitive",
			type = TraitType.Positive
		});
		t.base_stats.addTag("can_read_any_book");
		t.addOpposite("stupid");
		t.base_stats["intelligence"] = 10f;
		t.base_stats["diplomacy"] = 5f;
		t.base_stats["warfare"] = 5f;
		t.base_stats["stewardship"] = 7f;
		t.base_stats["loyalty_traits"] = -10f;
		t.base_stats["cities"] = 3f;
		add(new ActorTrait
		{
			id = "deceitful",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconDeceitful",
			rate_acquire_grow_up = 5,
			same_trait_mod = -15,
			opposite_trait_mod = -5,
			likeability = 0.1f,
			group_id = "mind",
			type = TraitType.Negative
		});
		t.addOpposite("honest");
		t.base_stats["diplomacy"] = 1f;
		t.base_stats["stewardship"] = 4f;
		t.base_stats["loyalty_traits"] = -20f;
		add(new ActorTrait
		{
			id = "ambitious",
			path_icon = "ui/Icons/actor_traits/iconAmbitious",
			rate_acquire_grow_up = 5,
			rate_birth = 1,
			same_trait_mod = -10,
			group_id = "mind",
			achievement_id = "achievement4RaceCities",
			unlocked_with_achievement = true
		});
		t.addOpposite("content");
		t.base_stats["diplomacy"] = 2f;
		t.base_stats["warfare"] = 4f;
		t.base_stats["stewardship"] = 1f;
		t.base_stats["loyalty_traits"] = -15f;
		t.base_stats["cities"] = 5f;
		add(new ActorTrait
		{
			id = "content",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconContent",
			rate_acquire_grow_up = 5,
			rate_birth = 2,
			same_trait_mod = 15,
			likeability = 0.1f,
			group_id = "mind",
			type = TraitType.Positive
		});
		t.addOpposite("ambitious");
		t.addOpposite("greedy");
		t.addOpposite("thief");
		t.base_stats["multiplier_supply_timer"] = -0.3f;
		t.base_stats["loyalty_traits"] = 10f;
		t.base_stats["diplomacy"] = 2f;
		t.base_stats["stewardship"] = 2f;
		t.base_stats["warfare"] = -2f;
		add(new ActorTrait
		{
			id = "honest",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconHonest",
			rate_acquire_grow_up = 5,
			rate_birth = 2,
			same_trait_mod = 10,
			opposite_trait_mod = -10,
			likeability = -0.1f,
			group_id = "mind",
			type = TraitType.Positive
		});
		t.addOpposite("deceitful");
		t.addOpposite("thief");
		t.base_stats["stewardship"] = 3f;
		t.base_stats["diplomacy"] = 2f;
		t.base_stats["warfare"] = -2f;
		t.base_stats["loyalty_traits"] = 5f;
		add(new ActorTrait
		{
			id = "paranoid",
			path_icon = "ui/Icons/actor_traits/iconParanoid",
			rate_acquire_grow_up = 5,
			rate_birth = 1,
			group_id = "mind",
			type = TraitType.Negative
		});
		t.base_stats["diplomacy"] = -2f;
		t.base_stats["warfare"] = 4f;
		t.base_stats["multiplier_supply_timer"] = 0.5f;
		t.base_stats["loyalty_traits"] = -15f;
		t.base_stats["cities"] = -1f;
		add(new ActorTrait
		{
			id = "greedy",
			path_icon = "ui/Icons/actor_traits/iconGreedy",
			rate_acquire_grow_up = 5,
			rate_birth = 1,
			likeability = -0.1f,
			group_id = "mind",
			type = TraitType.Negative
		});
		t.addOpposite("content");
		t.base_stats["diplomacy"] = -2f;
		t.base_stats["stewardship"] = -3f;
		t.base_stats["warfare"] = 4f;
		t.base_stats["multiplier_supply_timer"] = 4f;
		t.base_stats["loyalty_traits"] = -5f;
		t.base_stats["cities"] = 2f;
	}

	private void addTraitsSpirit()
	{
		add(new ActorTrait
		{
			id = "chosen_one",
			path_icon = "ui/Icons/actor_traits/iconChosenOne",
			likeability = 0.25f,
			group_id = "fate",
			achievement_id = "achievementLavaStrike",
			unlocked_with_achievement = true
		});
		t.is_mutation_box_allowed = false;
		t.base_stats["stamina"] = 1000f;
		t.base_stats["mana"] = 1000f;
		t.addCombatAction("combat_backstep");
		t.addCombatAction("combat_block");
		t.addCombatAction("combat_dash");
		t.addCombatAction("combat_deflect_projectile");
		t.addCombatAction("combat_dodge");
		t.addSpell("cast_fire");
		t.addSpell("summon_lightning");
		t.addSpell("summon_tornado");
		t.addSpell("cast_blood_rain");
		t.addSpell("cast_blood_rain");
		t.addSpell("cast_cure");
		t.addSpell("cast_shield");
		t.addSpell("cast_grass_seeds");
		t.addSpell("spawn_vegetation");
		t.addSpell("cast_curse");
		ActorTrait actorTrait = t;
		actorTrait.action_death = (WorldAction)Delegate.Combine(actorTrait.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
		add(new ActorTrait
		{
			id = "moonchild",
			path_icon = "ui/Icons/actor_traits/iconMoonchild",
			only_active_on_era_flag = true,
			era_active_moon = true,
			group_id = "spirit",
			rate_inherit = 5,
			rate_birth = 1
		});
		t.base_stats["multiplier_damage"] = 0.5f;
		t.base_stats["multiplier_speed"] = 0.1f;
		t.base_stats["armor"] = 1f;
		t.base_stats["intelligence"] = 3f;
		add(new ActorTrait
		{
			id = "nightchild",
			path_icon = "ui/Icons/actor_traits/iconNightchild",
			only_active_on_era_flag = true,
			era_active_night = true,
			group_id = "spirit",
			rate_inherit = 5,
			rate_birth = 1
		});
		t.base_stats["multiplier_damage"] = 0.5f;
		t.base_stats["multiplier_speed"] = 0.1f;
		t.base_stats["critical_chance"] = 0.03f;
		t.base_stats["warfare"] = 3f;
		add(new ActorTrait
		{
			id = "flesh_eater",
			path_icon = "ui/Icons/actor_traits/iconFleshEater",
			group_id = "spirit",
			rate_inherit = 5,
			rate_birth = 1
		});
		t.action_attack_target = ActionLibrary.restoreHealthOnHit;
		add(new ActorTrait
		{
			id = "titan_lungs",
			path_icon = "ui/Icons/actor_traits/iconTitanLungs",
			group_id = "body",
			rate_inherit = 5
		});
		t.setUnlockedWithAchievement("achievementNinjaTurtle");
		t.base_stats["multiplier_stamina"] = 10f;
		add(new ActorTrait
		{
			id = "heart_of_wizard",
			path_icon = "ui/Icons/actor_traits/iconHeartWizard",
			group_id = "spirit",
			rate_inherit = 5
		});
		t.base_stats["multiplier_mana"] = 10f;
		add(new ActorTrait
		{
			id = "battle_reflexes",
			path_icon = "ui/Icons/actor_traits/iconBattleReflexes",
			group_id = "mind",
			rate_inherit = 5
		});
		t.base_stats["skill_combat"] = 0.5f;
		add(new ActorTrait
		{
			id = "arcane_reflexes",
			path_icon = "ui/Icons/actor_traits/iconArcaneReflexes",
			group_id = "mind",
			rate_inherit = 5
		});
		t.base_stats["skill_spell"] = 0.5f;
		add(new ActorTrait
		{
			id = "healing_aura",
			path_icon = "ui/Icons/actor_traits/iconHealingAura",
			group_id = "spirit",
			rate_inherit = 5,
			special_effect_interval = 2f,
			likeability = 0.1f
		});
		ActorTrait actorTrait2 = t;
		actorTrait2.action_special_effect = (WorldAction)Delegate.Combine(actorTrait2.action_special_effect, new WorldAction(ActionLibrary.healingAuraEffect));
		add(new ActorTrait
		{
			id = "savage",
			path_icon = "ui/Icons/actor_traits/iconSavage",
			group_id = "spirit",
			type = TraitType.Positive,
			same_trait_mod = 5,
			rate_acquire_grow_up = 2,
			rate_inherit = 5
		});
		add(new ActorTrait
		{
			id = "immortal",
			path_icon = "ui/Icons/actor_traits/iconImmortal",
			same_trait_mod = -20,
			type = TraitType.Positive,
			unlocked_with_achievement = true,
			achievement_id = "achievementTheKing",
			group_id = "health"
		});
		t.addOpposite("plague");
		t.addOpposite("boat");
		t.base_stats["loyalty_traits"] = -20f;
		add(new ActorTrait
		{
			id = "burning_feet",
			path_icon = "ui/Icons/actor_traits/iconBurningFeet",
			unlocked_with_achievement = true,
			achievement_id = "achievementTheHell",
			group_id = "spirit",
			rate_inherit = 3
		});
		ActorTrait actorTrait3 = t;
		actorTrait3.action_special_effect = (WorldAction)Delegate.Combine(actorTrait3.action_special_effect, new WorldAction(ActionLibrary.burningFeetEffect));
		add(new ActorTrait
		{
			id = "cold_aura",
			path_icon = "ui/Icons/actor_traits/iconColdAura",
			group_id = "spirit",
			rate_inherit = 3
		});
		ActorTrait actorTrait4 = t;
		actorTrait4.action_special_effect = (WorldAction)Delegate.Combine(actorTrait4.action_special_effect, new WorldAction(ActionLibrary.coldAuraEffect));
		add(new ActorTrait
		{
			id = "lucky",
			path_icon = "ui/Icons/actor_traits/iconLucky",
			rate_birth = 2,
			rate_inherit = 5,
			likeability = 0.1f,
			group_id = "spirit",
			type = TraitType.Positive
		});
		t.addOpposite("unlucky");
		t.base_stats["lifespan"] = 7f;
		t.base_stats["accuracy"] = 4f;
		t.base_stats["critical_chance"] = 0.3f;
		t.base_stats["birth_rate"] = 5f;
		add(new ActorTrait
		{
			id = "unlucky",
			path_icon = "ui/Icons/actor_traits/iconUnlucky",
			rate_birth = 3,
			rate_inherit = 5,
			likeability = -0.1f,
			special_effect_interval = 20f,
			group_id = "spirit",
			type = TraitType.Negative
		});
		t.addOpposite("lucky");
		t.base_stats["lifespan"] = -13f;
		ActorTrait actorTrait5 = t;
		actorTrait5.action_special_effect = (WorldAction)Delegate.Combine(actorTrait5.action_special_effect, new WorldAction(ActionLibrary.unluckyFall));
		t.base_stats["accuracy"] = -4f;
		t.base_stats["critical_chance"] = -0.3f;
		add(new ActorTrait
		{
			id = "bloodlust",
			path_icon = "ui/Icons/actor_traits/iconBloodlust",
			rate_acquire_grow_up = 4,
			rate_birth = 1,
			rate_inherit = 5,
			group_id = "spirit",
			type = TraitType.Negative,
			unlocked_with_achievement = true,
			achievement_id = "achievementTheDemon"
		});
		t.addOpposite("pacifist");
		t.base_stats["multiplier_supply_timer"] = 1.5f;
		t.base_stats["loyalty_traits"] = -20f;
		t.base_stats["warfare"] = 5f;
		t.base_stats["diplomacy"] = -2f;
		t.base_stats["cities"] = 3f;
		add(new ActorTrait
		{
			id = "pacifist",
			path_icon = "ui/Icons/actor_traits/iconPacifist",
			rate_acquire_grow_up = 3,
			rate_inherit = 5,
			likeability = 0.1f,
			group_id = "spirit",
			type = TraitType.Positive
		});
		t.addOpposite("bloodlust");
		t.base_stats["multiplier_supply_timer"] = -0.1f;
		t.base_stats["loyalty_traits"] = 50f;
		t.base_stats["diplomacy"] = 10f;
		t.base_stats["warfare"] = -4f;
	}

	private void addTraitsAcquired()
	{
		add(new ActorTrait
		{
			id = "veteran",
			path_icon = "ui/Icons/actor_traits/iconVeteran",
			group_id = "merits",
			type = TraitType.Positive,
			same_trait_mod = 5,
			is_mutation_box_allowed = false
		});
		t.base_stats["skill_combat"] = 0.1f;
		t.base_stats["multiplier_damage"] = 0.1f;
		t.base_stats["multiplier_health"] = 0.1f;
		add(new ActorTrait
		{
			id = "wise",
			path_icon = "ui/Icons/actor_traits/iconWise",
			group_id = "cognitive",
			type = TraitType.Positive,
			is_mutation_box_allowed = false
		});
		t.addOpposite("stupid");
		t.addOpposite("boat");
		t.base_stats["diplomacy"] = 1f;
		t.base_stats["stewardship"] = 1f;
		t.base_stats["warfare"] = 1f;
		t.base_stats["intelligence"] = 1f;
		add(new ActorTrait
		{
			id = "infected",
			path_icon = "ui/Icons/actor_traits/iconInfected",
			rate_inherit = 20,
			group_id = "acquired",
			can_be_removed_by_divine_light = true,
			can_be_removed_by_accelerated_healing = true,
			is_mutation_box_allowed = false
		});
		t.addOpposite("immune");
		t.addOpposite("boat");
		t.can_be_cured = true;
		ActorTrait actorTrait = t;
		actorTrait.action_special_effect = (WorldAction)Delegate.Combine(actorTrait.action_special_effect, new WorldAction(ActionLibrary.infectedEffect));
		t.special_effect_interval = 1.5f;
		ActorTrait actorTrait2 = t;
		actorTrait2.action_death = (WorldAction)Delegate.Combine(actorTrait2.action_death, new WorldAction(ActionLibrary.turnIntoZombie));
		t.base_stats["multiplier_speed"] = 0.1f;
		t.base_stats["loyalty_traits"] = -15f;
		add(new ActorTrait
		{
			id = "mush_spores",
			path_icon = "ui/Icons/actor_traits/iconMushSpores",
			rate_inherit = 30,
			can_be_removed_by_divine_light = true,
			can_be_removed_by_accelerated_healing = true,
			group_id = "acquired",
			is_mutation_box_allowed = false
		});
		t.addOpposite("immune");
		t.addOpposite("boat");
		t.can_be_cured = true;
		ActorTrait actorTrait3 = t;
		actorTrait3.action_death = (WorldAction)Delegate.Combine(actorTrait3.action_death, new WorldAction(ActionLibrary.mushSporesEffect));
		ActorTrait actorTrait4 = t;
		actorTrait4.action_death = (WorldAction)Delegate.Combine(actorTrait4.action_death, new WorldAction(ActionLibrary.turnIntoMush));
		t.base_stats["multiplier_speed"] = 0.3f;
		t.base_stats["loyalty_traits"] = -15f;
		add(new ActorTrait
		{
			id = "tumor_infection",
			path_icon = "ui/Icons/actor_traits/iconTumorInfection",
			rate_inherit = 30,
			can_be_removed_by_divine_light = true,
			can_be_removed_by_accelerated_healing = true,
			group_id = "acquired",
			is_mutation_box_allowed = false
		});
		t.addOpposite("immune");
		t.addOpposite("boat");
		t.can_be_cured = true;
		ActorTrait actorTrait5 = t;
		actorTrait5.action_special_effect = (WorldAction)Delegate.Combine(actorTrait5.action_special_effect, new WorldAction(ActionLibrary.tumorEffect));
		ActorTrait actorTrait6 = t;
		actorTrait6.action_death = (WorldAction)Delegate.Combine(actorTrait6.action_death, new WorldAction(ActionLibrary.turnIntoTumorMonster));
		t.base_stats["multiplier_speed"] = 0.3f;
		t.base_stats["loyalty_traits"] = -15f;
		add(new ActorTrait
		{
			id = "plague",
			path_icon = "ui/Icons/actor_traits/iconPlague",
			rate_inherit = 30,
			unlocked_with_achievement = true,
			achievement_id = "achievementGreatPlague",
			can_be_removed_by_divine_light = true,
			can_be_removed_by_accelerated_healing = true,
			group_id = "acquired",
			is_mutation_box_allowed = false
		});
		t.addOpposite("immune");
		t.addOpposite("immortal");
		t.addOpposite("contagious");
		t.addOpposite("boat");
		t.can_be_cured = true;
		ActorTrait actorTrait7 = t;
		actorTrait7.action_special_effect = (WorldAction)Delegate.Combine(actorTrait7.action_special_effect, new WorldAction(ActionLibrary.plagueEffect));
		t.base_stats["multiplier_speed"] = -0.3f;
		t.base_stats["multiplier_damage"] = -0.5f;
		t.base_stats["stamina"] = -10f;
		t.base_stats["armor"] = -2f;
		t.base_stats["loyalty_traits"] = -15f;
		t.base_stats["lifespan"] = -30f;
		add(new ActorTrait
		{
			id = "blessed",
			likeability = 0.1f,
			path_icon = "ui/Icons/actor_traits/iconBlessing",
			group_id = "acquired",
			is_mutation_box_allowed = false
		});
		t.addOpposite("evil");
		t.base_stats["multiplier_damage"] = 0.5f;
		t.base_stats["multiplier_health"] = 0.5f;
		t.base_stats["multiplier_speed"] = 0.5f;
		t.base_stats["multiplier_diplomacy"] = 0.2f;
		t.base_stats["multiplier_crit"] = 0.1f;
		t.base_stats["lifespan"] = 5f;
		add(new ActorTrait
		{
			id = "kingslayer",
			path_icon = "ui/Icons/actor_traits/iconKingslayer",
			group_id = "merits",
			is_mutation_box_allowed = false
		});
		t.base_stats["multiplier_supply_timer"] = 2f;
		t.base_stats["loyalty_traits"] = -25f;
		t.base_stats["diplomacy"] = -5f;
		t.base_stats["warfare"] = 5f;
		add(new ActorTrait
		{
			id = "mageslayer",
			group_id = "merits",
			path_icon = "ui/Icons/actor_traits/iconMageslayer",
			is_mutation_box_allowed = false
		});
		t.base_stats["loyalty_traits"] = -10f;
		t.base_stats["warfare"] = 5f;
		t.base_stats["critical_chance"] = 0.03f;
		add(new ActorTrait
		{
			id = "dragonslayer",
			group_id = "merits",
			path_icon = "ui/Icons/actor_traits/iconDragonslayer",
			is_mutation_box_allowed = false
		});
		t.base_stats["warfare"] = 5f;
		t.base_stats["critical_chance"] = 0.04f;
		t.base_stats["multiplier_diplomacy"] = 0.1f;
		add(new ActorTrait
		{
			id = "crippled",
			path_icon = "ui/Icons/actor_traits/iconCrippled",
			same_trait_mod = 10,
			can_be_removed_by_divine_light = true,
			can_be_removed_by_accelerated_healing = true,
			type = TraitType.Negative,
			group_id = "acquired",
			is_mutation_box_allowed = false
		});
		t.base_stats["multiplier_speed"] = -0.5f;
		t.base_stats["diplomacy"] = -3f;
		t.base_stats["multiplier_offspring"] = -0.5f;
		add(new ActorTrait
		{
			id = "golden_tooth",
			path_icon = "ui/Icons/actor_traits/iconGoldenTooth",
			same_trait_mod = 5,
			type = TraitType.Positive,
			group_id = "appearance",
			is_mutation_box_allowed = false
		});
		t.base_stats["diplomacy"] = 2f;
		add(new ActorTrait
		{
			id = "eyepatch",
			path_icon = "ui/Icons/actor_traits/iconEyePatch",
			same_trait_mod = 20,
			can_be_removed_by_divine_light = true,
			can_be_removed_by_accelerated_healing = true,
			type = TraitType.Negative,
			group_id = "appearance",
			is_mutation_box_allowed = false
		});
		t.base_stats["accuracy"] = -5f;
		t.base_stats["diplomacy"] = 1f;
		t.base_stats["warfare"] = -1f;
		t.base_stats["critical_chance"] = -0.15f;
		add(new ActorTrait
		{
			id = "skin_burns",
			path_icon = "ui/Icons/actor_traits/iconSkinBurns",
			same_trait_mod = 40,
			can_be_removed_by_divine_light = true,
			can_be_removed_by_accelerated_healing = true,
			type = TraitType.Negative,
			group_id = "appearance",
			is_mutation_box_allowed = false
		});
		t.base_stats["diplomacy"] = -2f;
		t.base_stats["warfare"] = 2f;
		t.base_stats["multiplier_speed"] = -0.25f;
		t.base_stats["lifespan"] = -5f;
	}

	private void addTraitsFun()
	{
		add(new ActorTrait
		{
			id = "super_health",
			path_icon = "ui/Icons/actor_traits/iconSuperHealth",
			unlocked_with_achievement = true,
			achievement_id = "achievementTraitsExplorer90",
			group_id = "health",
			rate_inherit = 3
		});
		t.base_stats["lifespan"] = 100f;
		t.base_stats["multiplier_health"] = 100f;
		add(new ActorTrait
		{
			id = "death_nuke",
			path_icon = "ui/Icons/actor_traits/iconDeathNuke",
			unlocked_with_achievement = true,
			achievement_id = "achievementFinalResolution",
			group_id = "fun",
			rate_inherit = 1,
			is_mutation_box_allowed = false
		});
		t.addOpposite("death_bomb");
		ActorTrait actorTrait = t;
		actorTrait.action_death = (WorldAction)Delegate.Combine(actorTrait.action_death, new WorldAction(ActionLibrary.deathNuke));
		add(new ActorTrait
		{
			id = "death_bomb",
			path_icon = "ui/Icons/actor_traits/iconDeathBomb",
			unlocked_with_achievement = true,
			achievement_id = "achievementManyBombs",
			group_id = "fun",
			rate_inherit = 1
		});
		t.addOpposite("death_nuke");
		ActorTrait actorTrait2 = t;
		actorTrait2.action_death = (WorldAction)Delegate.Combine(actorTrait2.action_death, new WorldAction(ActionLibrary.deathBomb));
		add(new ActorTrait
		{
			id = "death_mark",
			path_icon = "ui/Icons/actor_traits/iconDeathMark",
			unlocked_with_achievement = true,
			achievement_id = "achievementTraitsExplorer40",
			group_id = "fate",
			is_mutation_box_allowed = false
		});
		ActorTrait actorTrait3 = t;
		actorTrait3.action_special_effect = (WorldAction)Delegate.Combine(actorTrait3.action_special_effect, new WorldAction(ActionLibrary.deathMark));
		add(new ActorTrait
		{
			id = "energized",
			path_icon = "ui/Icons/actor_traits/iconLightning",
			group_id = "fun",
			spawn_random_trait_allowed = false
		});
		t.addOpposite("boat");
		t.base_stats["multiplier_speed"] = 2f;
		t.base_stats["lifespan"] = 7f;
		ActorTrait actorTrait4 = t;
		actorTrait4.action_death = (WorldAction)Delegate.Combine(actorTrait4.action_death, new WorldAction(ActionLibrary.energizedLightning));
		add(new ActorTrait
		{
			id = "mega_heartbeat",
			path_icon = "ui/Icons/actor_traits/iconMegaHeartbeat",
			group_id = "fun",
			rate_inherit = 4,
			unlocked_with_achievement = true,
			achievement_id = "achievementPrintHeart",
			special_effect_interval = 5f,
			likeability = 0.1f,
			spawn_random_trait_allowed = false,
			is_mutation_box_allowed = false
		});
		t.addOpposite("whirlwind");
		ActorTrait actorTrait5 = t;
		actorTrait5.action_special_effect = (WorldAction)Delegate.Combine(actorTrait5.action_special_effect, new WorldAction(ActionLibrary.megaHeartbeat));
		add(new ActorTrait
		{
			id = "bomberman",
			path_icon = "ui/Icons/actor_traits/iconGrenade",
			group_id = "fun"
		});
		t.addCombatAction("combat_throw_bomb");
		add(new ActorTrait
		{
			id = "pyromaniac",
			path_icon = "ui/Icons/actor_traits/iconPyromaniac",
			rate_acquire_grow_up = 1,
			achievement_id = "achievementWorldWar",
			unlocked_with_achievement = true,
			group_id = "mind",
			acquire_grow_up_sapient_only = true,
			rate_inherit = 1
		});
		t.addCombatAction("combat_throw_torch");
		add(new ActorTrait
		{
			id = "whirlwind",
			path_icon = "ui/Icons/iconTornado",
			group_id = "fun",
			action_special_effect = ActionLibrary.whirlwind,
			special_effect_interval = 0.1f,
			unlocked_with_achievement = true,
			spawn_random_trait_allowed = false,
			is_mutation_box_allowed = false,
			achievement_id = "achievementRainTornado"
		});
		t.addOpposite("mega_heartbeat");
	}

	private void addTraitsMisc()
	{
		add(new ActorTrait
		{
			id = "light_lamp",
			path_icon = "ui/Icons/actor_traits/iconLightLamp",
			group_id = "miscellaneous"
		});
		t.base_stats.addTag("generate_light");
		add(new ActorTrait
		{
			id = "shiny",
			path_icon = "ui/Icons/actor_traits/iconShiny",
			group_id = "miscellaneous",
			rate_inherit = 10
		});
		t.base_stats["diplomacy"] = 5f;
		t.action_special_effect = ActionLibrary.shiny;
		add(new ActorTrait
		{
			id = "flower_prints",
			path_icon = "ui/Icons/actor_traits/iconFlowerPrints",
			unlocked_with_achievement = true,
			achievement_id = "achievementTouchTheGrass",
			group_id = "miscellaneous",
			rate_inherit = 10
		});
		ActorTrait actorTrait = t;
		actorTrait.action_special_effect = (WorldAction)Delegate.Combine(actorTrait.action_special_effect, new WorldAction(ActionLibrary.flowerPrintsEffect));
	}

	private void addTraitsSpecial()
	{
		add(new ActorTrait
		{
			id = "metamorphed",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconMetamorphed",
			can_be_given = false,
			group_id = "special",
			is_mutation_box_allowed = false
		});
		add(new ActorTrait
		{
			id = "clone",
			rarity = Rarity.R0_Normal,
			path_icon = "ui/Icons/actor_traits/iconClone",
			can_be_given = false,
			group_id = "special",
			is_mutation_box_allowed = false
		});
		add(new ActorTrait
		{
			id = "boat",
			path_icon = "ui/Icons/iconBoat",
			can_be_given = false,
			group_id = "special",
			is_mutation_box_allowed = false
		});
		t.addOpposite("infected");
		t.addOpposite("tumor_infection");
		t.addOpposite("mush_spores");
		t.addOpposite("plague");
		t.addOpposite("immortal");
		t.addOpposite("energized");
		t.addOpposite("wise");
		add(new ActorTrait
		{
			id = "scar_of_divinity",
			path_icon = "ui/Icons/actor_traits/iconDivineScar",
			can_be_removed = false,
			can_be_given = false,
			rate_inherit = 0,
			group_id = "special",
			is_mutation_box_allowed = false
		});
		add(new ActorTrait
		{
			id = "miracle_born",
			path_icon = "ui/Icons/actor_traits/iconMiracleBorn",
			group_id = "special",
			can_be_given = false,
			can_be_removed = false,
			is_mutation_box_allowed = false
		});
		t.base_stats["lifespan"] = 20f;
		t.base_stats["multiplier_offspring"] = 2f;
		t.base_stats["birth_rate"] = 2f;
		add(new ActorTrait
		{
			id = "miracle_bearer",
			path_icon = "ui/Icons/actor_traits/iconMiracleBearer",
			group_id = "special",
			can_be_given = false,
			can_be_removed = false,
			is_mutation_box_allowed = false
		});
		add(new ActorTrait
		{
			id = "contagious",
			path_icon = "ui/Icons/iconRat",
			group_id = "miscellaneous",
			is_mutation_box_allowed = false
		});
		t.addOpposite("plague");
		ActorTrait actorTrait = t;
		actorTrait.action_special_effect = (WorldAction)Delegate.Combine(actorTrait.action_special_effect, new WorldAction(ActionLibrary.contagiousEffect));
		add(new ActorTrait
		{
			id = "zombie",
			path_icon = "ui/Icons/iconZombie",
			can_be_given = false,
			group_id = "special",
			is_mutation_box_allowed = false
		});
		t.action_special_effect = ActionLibrary.zombieEffect;
		t.action_attack_target = ActionLibrary.zombieInfectAttack;
		add(new ActorTrait
		{
			id = "madness",
			path_icon = "ui/Icons/actor_traits/iconMadness",
			group_id = "special",
			can_be_removed_by_divine_light = true,
			can_be_given = false,
			can_be_removed = false,
			is_kingdom_affected = true,
			affects_mind = true,
			forced_kingdom = "mad",
			likeability = -1f,
			is_mutation_box_allowed = false
		});
		ActorTrait actorTrait2 = t;
		actorTrait2.action_on_augmentation_add = (WorldActionTrait)Delegate.Combine(actorTrait2.action_on_augmentation_add, new WorldActionTrait(ActionLibrary.forcedKingdomAdd));
		ActorTrait actorTrait3 = t;
		actorTrait3.action_on_augmentation_remove = (WorldActionTrait)Delegate.Combine(actorTrait3.action_on_augmentation_remove, new WorldActionTrait(ActionLibrary.forcedKingdomEffectRemove));
		ActorTrait actorTrait4 = t;
		actorTrait4.action_on_augmentation_load = (WorldActionTrait)Delegate.Combine(actorTrait4.action_on_augmentation_load, new WorldActionTrait(ActionLibrary.madnessEffectLoad));
		t.traits_to_remove_ids = new string[4] { "desire_alien_mold", "desire_computer", "desire_golden_egg", "desire_harp" };
		t.addOpposite("strong_minded");
		t.addOpposite("desire_alien_mold");
		t.addOpposite("desire_computer");
		t.addOpposite("desire_golden_egg");
		t.addOpposite("desire_harp");
		t.addDecision("madness_random_emotion");
		t.base_stats["multiplier_speed"] = 0.1f;
		t.base_stats["diplomacy"] = -100f;
		t.base_stats["loyalty_traits"] = -100f;
		add(new ActorTrait
		{
			id = "desire_alien_mold",
			path_icon = "ui/Icons/actor_traits/iconWaypointAlienMoldDrop",
			group_id = "special",
			can_be_removed_by_divine_light = true,
			can_be_given = false,
			can_be_removed = false,
			is_kingdom_affected = true,
			affects_mind = true,
			forced_kingdom = "alien_mold",
			is_mutation_box_allowed = false
		});
		ActorTrait actorTrait5 = t;
		actorTrait5.action_on_augmentation_add = (WorldActionTrait)Delegate.Combine(actorTrait5.action_on_augmentation_add, new WorldActionTrait(ActionLibrary.forcedKingdomAdd));
		ActorTrait actorTrait6 = t;
		actorTrait6.action_on_augmentation_remove = (WorldActionTrait)Delegate.Combine(actorTrait6.action_on_augmentation_remove, new WorldActionTrait(ActionLibrary.forcedKingdomEffectRemove));
		ActorTrait actorTrait7 = t;
		actorTrait7.action_on_augmentation_load = (WorldActionTrait)Delegate.Combine(actorTrait7.action_on_augmentation_load, new WorldActionTrait(ActionLibrary.madnessEffectLoad));
		t.addDecision("follow_desire_target");
		t.traits_to_remove_ids = new string[4] { "desire_computer", "desire_golden_egg", "desire_harp", "madness" };
		t.addOpposite("madness");
		t.addOpposite("desire_computer");
		t.addOpposite("desire_golden_egg");
		t.addOpposite("desire_harp");
		t.base_stats["targets"] = 1f;
		t.base_stats["multiplier_crit"] = 0.3f;
		t.base_stats["damage_range"] = 0.3f;
		t.base_stats["armor"] = 10f;
		add(new ActorTrait
		{
			id = "desire_computer",
			path_icon = "ui/Icons/actor_traits/iconWaypointComputerDrop",
			group_id = "special",
			can_be_removed_by_divine_light = true,
			can_be_given = false,
			can_be_removed = false,
			is_kingdom_affected = true,
			affects_mind = true,
			forced_kingdom = "computer",
			is_mutation_box_allowed = false
		});
		ActorTrait actorTrait8 = t;
		actorTrait8.action_on_augmentation_add = (WorldActionTrait)Delegate.Combine(actorTrait8.action_on_augmentation_add, new WorldActionTrait(ActionLibrary.forcedKingdomAdd));
		ActorTrait actorTrait9 = t;
		actorTrait9.action_on_augmentation_remove = (WorldActionTrait)Delegate.Combine(actorTrait9.action_on_augmentation_remove, new WorldActionTrait(ActionLibrary.forcedKingdomEffectRemove));
		ActorTrait actorTrait10 = t;
		actorTrait10.action_on_augmentation_load = (WorldActionTrait)Delegate.Combine(actorTrait10.action_on_augmentation_load, new WorldActionTrait(ActionLibrary.madnessEffectLoad));
		t.addDecision("follow_desire_target");
		t.traits_to_remove_ids = new string[4] { "desire_alien_mold", "desire_golden_egg", "desire_harp", "madness" };
		t.addOpposite("madness");
		t.addOpposite("desire_alien_mold");
		t.addOpposite("desire_golden_egg");
		t.addOpposite("desire_harp");
		t.base_stats["multiplier_health"] = 0.3f;
		t.base_stats["multiplier_lifespan"] = 0.5f;
		add(new ActorTrait
		{
			id = "desire_golden_egg",
			path_icon = "ui/Icons/actor_traits/iconWaypointGoldenEggDrop",
			group_id = "special",
			can_be_removed_by_divine_light = true,
			can_be_given = false,
			can_be_removed = false,
			is_kingdom_affected = true,
			affects_mind = true,
			forced_kingdom = "golden_egg",
			is_mutation_box_allowed = false
		});
		ActorTrait actorTrait11 = t;
		actorTrait11.action_on_augmentation_add = (WorldActionTrait)Delegate.Combine(actorTrait11.action_on_augmentation_add, new WorldActionTrait(ActionLibrary.forcedKingdomAdd));
		ActorTrait actorTrait12 = t;
		actorTrait12.action_on_augmentation_remove = (WorldActionTrait)Delegate.Combine(actorTrait12.action_on_augmentation_remove, new WorldActionTrait(ActionLibrary.forcedKingdomEffectRemove));
		ActorTrait actorTrait13 = t;
		actorTrait13.action_on_augmentation_load = (WorldActionTrait)Delegate.Combine(actorTrait13.action_on_augmentation_load, new WorldActionTrait(ActionLibrary.madnessEffectLoad));
		t.addDecision("follow_desire_target");
		t.traits_to_remove_ids = new string[4] { "desire_alien_mold", "desire_computer", "desire_harp", "madness" };
		t.addOpposite("strong_minded");
		t.addOpposite("madness");
		t.addOpposite("desire_alien_mold");
		t.addOpposite("desire_computer");
		t.addOpposite("desire_harp");
		t.base_stats["multiplier_damage"] = 0.5f;
		add(new ActorTrait
		{
			id = "desire_harp",
			path_icon = "ui/Icons/actor_traits/iconWaypointHarpDrop",
			group_id = "special",
			can_be_removed_by_divine_light = true,
			can_be_given = false,
			can_be_removed = false,
			is_kingdom_affected = true,
			affects_mind = true,
			forced_kingdom = "harp",
			is_mutation_box_allowed = false
		});
		ActorTrait actorTrait14 = t;
		actorTrait14.action_on_augmentation_add = (WorldActionTrait)Delegate.Combine(actorTrait14.action_on_augmentation_add, new WorldActionTrait(ActionLibrary.forcedKingdomAdd));
		ActorTrait actorTrait15 = t;
		actorTrait15.action_on_augmentation_remove = (WorldActionTrait)Delegate.Combine(actorTrait15.action_on_augmentation_remove, new WorldActionTrait(ActionLibrary.forcedKingdomEffectRemove));
		ActorTrait actorTrait16 = t;
		actorTrait16.action_on_augmentation_load = (WorldActionTrait)Delegate.Combine(actorTrait16.action_on_augmentation_load, new WorldActionTrait(ActionLibrary.madnessEffectLoad));
		t.addDecision("follow_desire_target");
		t.traits_to_remove_ids = new string[4] { "desire_alien_mold", "desire_computer", "desire_golden_egg", "madness" };
		t.addOpposite("strong_minded");
		t.addOpposite("madness");
		t.addOpposite("desire_alien_mold");
		t.addOpposite("desire_computer");
		t.addOpposite("desire_golden_egg");
		t.base_stats["multiplier_speed"] = 0.3f;
		t.base_stats["multiplier_attack_speed"] = 0.3f;
	}

	public override void post_init()
	{
		base.post_init();
		foreach (ActorTrait item in list)
		{
			if (item.base_stats["health"] > 0f || item.base_stats["mana"] > 0f || item.base_stats["stamina"] > 0f || item.base_stats["multiplier_health"] > 0f || item.base_stats["multiplier_mana"] > 0f || item.base_stats["multiplier_stamina"] > 0f)
			{
				item.action_on_augmentation_add = (WorldActionTrait)Delegate.Combine(item.action_on_augmentation_add, new WorldActionTrait(ActionLibrary.restoreFullStats));
			}
			if (item.base_stats["health"] < 0f || item.base_stats["mana"] < 0f || item.base_stats["stamina"] < 0f || item.base_stats["multiplier_health"] < 0f || item.base_stats["multiplier_mana"] < 0f || item.base_stats["multiplier_stamina"] < 0f)
			{
				item.action_on_augmentation_remove = (WorldActionTrait)Delegate.Combine(item.action_on_augmentation_remove, new WorldActionTrait(ActionLibrary.restoreFullStats));
			}
		}
	}

	public override ActorTrait add(ActorTrait pAsset)
	{
		base.add(pAsset);
		checkDefault(pAsset);
		return pAsset;
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (ActorTrait item in list)
		{
			if (item.is_mutation_box_allowed)
			{
				pot_traits_mutation_box.Add(item);
			}
		}
		foreach (ActorTrait item2 in list)
		{
			if (item2.rate_birth != 0)
			{
				for (int i = 0; i < item2.rate_birth; i++)
				{
					pot_traits_birth.Add(item2);
				}
			}
		}
		foreach (ActorTrait item3 in list)
		{
			if (item3.rate_acquire_grow_up != 0)
			{
				for (int j = 0; j < item3.rate_acquire_grow_up; j++)
				{
					pot_traits_growup.Add(item3);
				}
			}
		}
		foreach (ActorTrait item4 in list)
		{
			if (item4.in_training_dummy_combat_pot)
			{
				pot_traits_combat.Add(item4);
			}
		}
	}

	private void checkDefault(ActorTrait pAsset)
	{
		if (pAsset.rate_inherit == 0)
		{
			pAsset.rate_inherit = pAsset.rate_birth * 10;
		}
	}

	public int checkTraitsMod(Actor pMain, Actor pTarget)
	{
		int num = 0;
		foreach (ActorTrait trait in pMain.getTraits())
		{
			if (trait.same_trait_mod != 0 && pTarget.hasTrait(trait))
			{
				num += trait.same_trait_mod;
			}
			if (trait.opposite_trait_mod == 0)
			{
				continue;
			}
			foreach (ActorTrait opposite_trait in trait.opposite_traits)
			{
				if (pTarget.hasTrait(opposite_trait))
				{
					num += trait.opposite_trait_mod;
				}
			}
		}
		return num;
	}
}
