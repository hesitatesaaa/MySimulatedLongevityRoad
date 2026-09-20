public class BaseStatsLibrary : AssetLibrary<BaseStatAsset>
{
	public override void init()
	{
		base.init();
		add(new BaseStatAsset
		{
			id = "personality_aggression",
			hidden = true,
			normalize = true,
			normalize_min = 0f,
			normalize_max = 1f,
			used_only_for_civs = true
		});
		add(new BaseStatAsset
		{
			id = "personality_administration",
			hidden = true,
			normalize = true,
			normalize_min = 0f,
			normalize_max = 1f,
			used_only_for_civs = true
		});
		add(new BaseStatAsset
		{
			id = "personality_diplomatic",
			hidden = true,
			normalize = true,
			normalize_min = 0f,
			normalize_max = 1f,
			used_only_for_civs = true
		});
		add(new BaseStatAsset
		{
			id = "personality_rationality",
			hidden = true,
			normalize = true,
			normalize_min = 0f,
			normalize_max = 1f,
			used_only_for_civs = true
		});
		add(new BaseStatAsset
		{
			id = "diplomacy",
			actor_data_attribute = true,
			normalize = true,
			normalize_max = 999f,
			used_only_for_civs = true,
			sort_rank = 900
		});
		add(new BaseStatAsset
		{
			id = "warfare",
			actor_data_attribute = true,
			normalize = true,
			normalize_max = 999f,
			used_only_for_civs = true,
			sort_rank = 900
		});
		add(new BaseStatAsset
		{
			id = "stewardship",
			actor_data_attribute = true,
			normalize = true,
			normalize_max = 999f,
			used_only_for_civs = true,
			sort_rank = 900
		});
		add(new BaseStatAsset
		{
			id = "intelligence",
			actor_data_attribute = true,
			normalize = true,
			normalize_max = 999f,
			used_only_for_civs = true,
			sort_rank = 900
		});
		add(new BaseStatAsset
		{
			id = "lifespan",
			sort_rank = 997,
			normalize = true,
			normalize_min = 1f
		});
		add(new BaseStatAsset
		{
			id = "mutation",
			sort_rank = 996
		});
		add(new BaseStatAsset
		{
			id = "offspring",
			normalize = true,
			normalize_min = 0f,
			normalize_max = 1000f
		});
		add(new BaseStatAsset
		{
			id = "multiplier_offspring",
			show_as_percents = true,
			multiplier = true,
			main_stat_to_multiply = "offspring",
			tooltip_multiply_for_visual_number = 100f,
			translation_key = "offspring"
		});
		add(new BaseStatAsset
		{
			id = "army",
			normalize = true,
			normalize_min = 0.1f,
			used_only_for_civs = true
		});
		add(new BaseStatAsset
		{
			id = "cities",
			used_only_for_civs = true
		});
		add(new BaseStatAsset
		{
			id = "range"
		});
		add(new BaseStatAsset
		{
			id = "bonus_towers",
			normalize = true,
			normalize_max = 2f,
			used_only_for_civs = true
		});
		add(new BaseStatAsset
		{
			id = "damage",
			normalize = true,
			normalize_min = 1f,
			sort_rank = 999
		});
		add(new BaseStatAsset
		{
			id = "speed",
			normalize = true,
			normalize_min = 1f,
			sort_rank = 998
		});
		add(new BaseStatAsset
		{
			id = "health",
			normalize = true,
			normalize_min = 1f,
			sort_rank = 1000
		});
		add(new BaseStatAsset
		{
			id = "armor",
			normalize = true,
			normalize_min = 0f,
			normalize_max = 99f
		});
		add(new BaseStatAsset
		{
			id = "stamina",
			normalize = true,
			normalize_min = 1f,
			sort_rank = 1000
		});
		add(new BaseStatAsset
		{
			id = "mana",
			normalize = true,
			normalize_min = 0f,
			sort_rank = 1000
		});
		add(new BaseStatAsset
		{
			id = "accuracy",
			normalize_min = 1f,
			normalize_max = 10f,
			normalize = true
		});
		add(new BaseStatAsset
		{
			id = "targets",
			hidden = true
		});
		add(new BaseStatAsset
		{
			id = "projectiles",
			normalize = true,
			normalize_min = 1f,
			hidden = true
		});
		add(new BaseStatAsset
		{
			id = "experience"
		});
		add(new BaseStatAsset
		{
			id = "happiness",
			normalize = true,
			normalize_min = 0f
		});
		add(new BaseStatAsset
		{
			id = "critical_chance",
			normalize = true,
			normalize_min = 0f,
			show_as_percents = true,
			tooltip_multiply_for_visual_number = 100f
		});
		add(new BaseStatAsset
		{
			id = "critical_damage_multiplier",
			show_as_percents = true,
			tooltip_multiply_for_visual_number = 100f
		});
		add(new BaseStatAsset
		{
			id = "size",
			hidden = true
		});
		add(new BaseStatAsset
		{
			id = "area_of_effect",
			hidden = true
		});
		add(new BaseStatAsset
		{
			id = "attack_speed",
			normalize = true,
			normalize_min = 0.5f,
			normalize_max = 10f
		});
		add(new BaseStatAsset
		{
			id = "throwing_range",
			normalize = true,
			normalize_min = 1f,
			normalize_max = 100f
		});
		add(new BaseStatAsset
		{
			id = "construction_speed",
			normalize = true,
			normalize_min = 1f,
			normalize_max = 100f
		});
		add(new BaseStatAsset
		{
			id = "loyalty_traits",
			translation_key = "loyalty",
			used_only_for_civs = true
		});
		add(new BaseStatAsset
		{
			id = "birth_rate",
			translation_key = "birth_rate"
		});
		add(new BaseStatAsset
		{
			id = "maturation",
			translation_key = "maturation"
		});
		add(new BaseStatAsset
		{
			id = "age_adult",
			translation_key = "age_adult",
			hidden = true
		});
		add(new BaseStatAsset
		{
			id = "age_breeding",
			translation_key = "age_breeding",
			hidden = true
		});
		add(new BaseStatAsset
		{
			id = "max_nutrition",
			translation_key = "max_nutrition"
		});
		add(new BaseStatAsset
		{
			id = "metabolic_rate",
			translation_key = "metabolic_rate",
			normalize_min = 1f,
			hidden = true
		});
		add(new BaseStatAsset
		{
			id = "loyalty_mood",
			translation_key = "loyalty",
			used_only_for_civs = true
		});
		add(new BaseStatAsset
		{
			id = "opinion",
			used_only_for_civs = true
		});
		add(new BaseStatAsset
		{
			id = "skill_combat",
			show_as_percents = true,
			tooltip_multiply_for_visual_number = 100f
		});
		add(new BaseStatAsset
		{
			id = "skill_spell",
			show_as_percents = true,
			tooltip_multiply_for_visual_number = 100f
		});
		add(new BaseStatAsset
		{
			id = "knockback"
		});
		add(new BaseStatAsset
		{
			id = "recoil"
		});
		add(new BaseStatAsset
		{
			id = "mass"
		});
		add(new BaseStatAsset
		{
			id = "mass_2"
		});
		add(new BaseStatAsset
		{
			id = "multiplier_mass",
			show_as_percents = true,
			multiplier = true,
			main_stat_to_multiply = "mass_2",
			hidden = true
		});
		add(new BaseStatAsset
		{
			id = "limit_population"
		});
		add(new BaseStatAsset
		{
			id = "multiplier_health",
			show_as_percents = true,
			multiplier = true,
			main_stat_to_multiply = "health",
			tooltip_multiply_for_visual_number = 100f,
			translation_key = "health"
		});
		add(new BaseStatAsset
		{
			id = "multiplier_lifespan",
			show_as_percents = true,
			multiplier = true,
			main_stat_to_multiply = "lifespan",
			tooltip_multiply_for_visual_number = 100f,
			translation_key = "lifespan"
		});
		add(new BaseStatAsset
		{
			id = "multiplier_stamina",
			show_as_percents = true,
			multiplier = true,
			main_stat_to_multiply = "stamina",
			tooltip_multiply_for_visual_number = 100f,
			translation_key = "stamina"
		});
		add(new BaseStatAsset
		{
			id = "multiplier_mana",
			show_as_percents = true,
			multiplier = true,
			main_stat_to_multiply = "mana",
			tooltip_multiply_for_visual_number = 100f,
			translation_key = "mana"
		});
		add(new BaseStatAsset
		{
			id = "multiplier_damage",
			show_as_percents = true,
			multiplier = true,
			main_stat_to_multiply = "damage",
			tooltip_multiply_for_visual_number = 100f,
			translation_key = "damage"
		});
		add(new BaseStatAsset
		{
			id = "multiplier_crit",
			show_as_percents = true,
			multiplier = true,
			main_stat_to_multiply = "critical_chance",
			tooltip_multiply_for_visual_number = 100f,
			translation_key = "critical_chance_multiplier"
		});
		add(new BaseStatAsset
		{
			id = "multiplier_diplomacy",
			show_as_percents = true,
			multiplier = true,
			main_stat_to_multiply = "diplomacy",
			tooltip_multiply_for_visual_number = 100f,
			translation_key = "diplomacy",
			used_only_for_civs = true
		});
		add(new BaseStatAsset
		{
			id = "multiplier_speed",
			show_as_percents = true,
			multiplier = true,
			main_stat_to_multiply = "speed",
			tooltip_multiply_for_visual_number = 100f,
			translation_key = "speed"
		});
		add(new BaseStatAsset
		{
			id = "multiplier_attack_speed",
			show_as_percents = true,
			multiplier = true,
			main_stat_to_multiply = "attack_speed",
			tooltip_multiply_for_visual_number = 100f,
			translation_key = "attack_speed"
		});
		add(new BaseStatAsset
		{
			id = "scale",
			show_as_percents = true,
			tooltip_multiply_for_visual_number = 1000f,
			translation_key = "size"
		});
		add(new BaseStatAsset
		{
			id = "multiplier_supply_timer",
			hidden = true,
			used_only_for_civs = true
		});
		add(new BaseStatAsset
		{
			id = "limit_clan_members",
			used_only_for_civs = true
		});
		add(new BaseStatAsset
		{
			id = "status_chance"
		});
		add(new BaseStatAsset
		{
			id = "damage_range",
			hidden = true,
			normalize = true,
			normalize_min = 0.1f
		});
	}

	public override void editorDiagnosticLocales()
	{
		foreach (BaseStatAsset item in list)
		{
			if (!item.hidden)
			{
				checkLocale(item, item.getLocaleID());
			}
		}
		base.editorDiagnosticLocales();
	}
}
