using System;
using System.Collections.Generic;

public class ClanTraitLibrary : BaseTraitLibrary<ClanTrait>
{
	protected override string icon_path => "ui/Icons/clan_traits/";

	protected override List<string> getDefaultTraitsForMeta(ActorAsset pAsset)
	{
		return pAsset.default_clan_traits;
	}

	public override void init()
	{
		base.init();
		add(new ClanTrait
		{
			id = "mark_of_becoming",
			group_id = "special",
			can_be_given = false,
			can_be_removed = false,
			spawn_random_trait_allowed = false
		});
		add(new ClanTrait
		{
			id = "blood_pact",
			group_id = "spirit"
		});
		t.base_stats["warfare"] = 1f;
		t.addDecision("banish_unruly_clan_members");
		t.addOpposite("deathbound");
		add(new ClanTrait
		{
			id = "deathbound",
			group_id = "spirit"
		});
		t.base_stats["warfare"] = 5f;
		t.addDecision("kill_unruly_clan_members");
		t.addOpposite("blood_pact");
		add(new ClanTrait
		{
			id = "bonebreakers",
			group_id = "body"
		});
		t.setUnlockedWithAchievement("achievementSegregator");
		t.base_stats["damage"] = 5f;
		ClanTrait clanTrait = t;
		clanTrait.action_attack_target = (AttackAction)Delegate.Combine(clanTrait.action_attack_target, new AttackAction(ActionLibrary.breakBones));
		add(new ClanTrait
		{
			id = "stonefists",
			group_id = "body"
		});
		t.base_stats["damage"] = 30f;
		add(new ClanTrait
		{
			id = "blood_of_sea",
			group_id = "body"
		});
		t.base_stats["stamina"] = 20f;
		t.base_stats.addTag("fast_swimming");
		add(new ClanTrait
		{
			id = "gaia_shield",
			group_id = "body"
		});
		t.base_stats["armor"] = 10f;
		t.base_stats["multiplier_health"] = 0.1f;
		t.base_stats.addTag("immunity_fire");
		t.base_stats.addTag("immunity_cold");
		add(new ClanTrait
		{
			id = "iron_will",
			group_id = "mind"
		});
		t.base_stats["intelligence"] = 5f;
		t.base_stats.addTag("strong_mind");
		add(new ClanTrait
		{
			id = "flesh_weavers",
			group_id = "body",
			special_effect_interval = 2f
		});
		t.base_stats["multiplier_health"] = 0.2f;
		ClanTrait clanTrait2 = t;
		clanTrait2.action_special_effect = (WorldAction)Delegate.Combine(clanTrait2.action_special_effect, new WorldAction(ActionLibrary.regenerationEffectClan));
		add(new ClanTrait
		{
			id = "endurance_of_titans",
			group_id = "body"
		});
		t.base_stats["multiplier_stamina"] = 3f;
		add(new ClanTrait
		{
			id = "combat_instincts",
			group_id = "mind"
		});
		t.setUnlockedWithAchievement("achievementMasterOfCombat");
		t.base_stats["warfare"] = 10f;
		t.addCombatAction("combat_dash");
		t.addCombatAction("combat_block");
		t.addCombatAction("combat_dodge");
		t.addCombatAction("combat_backstep");
		t.addCombatAction("combat_deflect_projectile");
		add(new ClanTrait
		{
			id = "void_ban",
			group_id = "chaos",
			spawn_random_trait_allowed = false
		});
		t.base_stats["multiplier_mana"] = -10f;
		add(new ClanTrait
		{
			id = "warlocks_vein",
			group_id = "spirit"
		});
		t.base_stats_male["multiplier_mana"] = 2f;
		add(new ClanTrait
		{
			id = "witchs_vein",
			group_id = "spirit"
		});
		t.base_stats_female["multiplier_mana"] = 2f;
		add(new ClanTrait
		{
			id = "magic_blood",
			group_id = "spirit"
		});
		t.setUnlockedWithAchievement("achievementTheAccomplished");
		t.base_stats["multiplier_mana"] = 3f;
		add(new ClanTrait
		{
			id = "blood_of_eons",
			group_id = "body",
			spawn_random_trait_allowed = false
		});
		t.addOpposite("cursed_blood");
		t.base_stats["lifespan"] = 1E+09f;
		add(new ClanTrait
		{
			id = "blood_of_giants",
			group_id = "body"
		});
		t.base_stats["scale"] = 0.05f;
		add(new ClanTrait
		{
			id = "silver_tongues",
			group_id = "mind"
		});
		t.base_stats["opinion"] = 20f;
		t.base_stats["diplomacy"] = 5f;
		add(new ClanTrait
		{
			id = "masters_of_propaganda",
			group_id = "mind"
		});
		t.base_stats["loyalty_traits"] = 20f;
		add(new ClanTrait
		{
			id = "gods_chosen",
			group_id = "spirit"
		});
		t.base_stats["stewardship"] = 10f;
		t.base_stats["diplomacy"] = 5f;
		t.base_stats["armor"] = 20f;
		add(new ClanTrait
		{
			id = "cursed_blood",
			group_id = "chaos",
			spawn_random_trait_allowed = false
		});
		t.setUnlockedWithAchievement("achievementTheBroken");
		t.base_stats["lifespan"] = -666f;
		t.addOpposite("blood_of_eons");
		add(new ClanTrait
		{
			id = "divine_dozen",
			group_id = "harmony"
		});
		t.addOpposite("we_are_legion");
		t.addOpposite("best_five");
		t.base_stats_meta["limit_clan_members"] = 12f;
		add(new ClanTrait
		{
			id = "best_five",
			group_id = "harmony"
		});
		t.addOpposite("we_are_legion");
		t.addOpposite("divine_dozen");
		t.base_stats_meta["limit_clan_members"] = 5f;
		add(new ClanTrait
		{
			id = "we_are_legion",
			group_id = "harmony"
		});
		t.setUnlockedWithAchievement("achievementMegapolis");
		t.addOpposite("best_five");
		t.addOpposite("divine_dozen");
		t.base_stats_meta["limit_clan_members"] = 1000f;
		add(new ClanTrait
		{
			id = "nitroglycerin_blood",
			group_id = "chaos",
			action_death = delegate(BaseSimObject _, WorldTile pTile)
			{
				DropsLibrary.action_grenade(pTile);
				return true;
			}
		});
		t.setUnlockedWithAchievement("achievementMinefield");
		t.base_stats["health"] = -1f;
		add(new ClanTrait
		{
			id = "antimatter_blood",
			group_id = "chaos",
			spawn_random_trait_allowed = false,
			action_death = delegate(BaseSimObject _, WorldTile pTile)
			{
				DropsLibrary.action_antimatter_bomb(pTile);
				return true;
			}
		});
		t.setUnlockedWithAchievement("achievementTraitExplorerClan");
		t.base_stats["damage"] = 1f;
		add(new ClanTrait
		{
			id = "gaia_blood",
			group_id = "spirit",
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
			}
		});
		t.setUnlockedWithAchievement("achievementThePrincess");
		t.base_stats["multiplier_health"] = 0.05f;
		add(new ClanTrait
		{
			id = "grin_mark",
			group_id = "fate",
			spawn_random_trait_allowed = false,
			priority = -100
		});
		t.setTraitInfoToGrinMark();
		t.setUnlockedWithAchievement("achievementCreaturesExplorer");
		add(new ClanTrait
		{
			id = "geb",
			group_id = "special",
			can_be_given = false,
			can_be_removed = false,
			spawn_random_trait_allowed = false
		});
	}
}
