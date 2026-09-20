using System;

[Serializable]
public class SimGlobalAsset : Asset
{
	public int people_before_meta_divide = 1000;

	public int years_before_meta_divide = 500;

	public float baby_mass_multiplier = 0.4f;

	public float MANA_PER_INTELLIGENCE = 10f;

	public float base_tax_rate_local = 0.5f;

	public float base_tax_rate_tribute = 0.5f;

	public int festive_fireworks_cost = 10;

	public float empty_city_borders_shrink_time = 3f;

	public float water_damage_multiplier = 0.1f;

	public float starvation_damage_multiplier = 0.05f;

	public int base_metabolic_rate = 1;

	public int unit_chunk_sight_range = 1;

	public int child_work_age = 8;

	public int diplomacy_years_war_timeout = 5;

	public int diplomacy_years_war_min_warriors = 10;

	public int rebellions_min_age = 15;

	public int rebellions_min_warriors = 10;

	public float rebellions_unhappy_multiplier = 0.1f;

	public int alliance_months_per_level = 30;

	public int alliance_timeout = 3;

	public float level_mod_bonus_health = 0.05f;

	public float level_mod_bonus_stamina = 0.02f;

	public float level_mod_bonus_mana = 0.02f;

	public float unexplored_sprite_animation_speed = 22f;

	public int minimum_kingdom_age_for_attack = 5;

	public int minimum_age_before_war_stop = 10;

	public int minimum_years_between_wars = 5;

	public int biomes_growth_speed = 5;

	public float fire_spread_time = 2f;

	public float fire_stop_time = 5f;

	public float fire_time = 30f;

	public bool allow_different_species_buildings = true;

	public float interval_nutrition_decay = 15f;

	public float interval_happiness = 2f;

	public float interval_stamina = 2f;

	public float interval_mana = 10f;

	public int stamina_change = 1;

	public int mana_change = 1;

	public float unit_speed_multiplier = 0.5f;

	public float unit_force_multiplier = 1f;

	public float gravity = 9.8f;

	public float min_people_for_civilization = 2f;

	public float new_civilization_range = 20f;

	public float nomad_check_far_city_range = 20f;

	public float forgotten_plot_time = 20f;

	public int nutrition_cost_new_baby = 50;

	public float nutrition_level_hungry = 0.5f;

	public int nutrition_start_level_baby = 25;

	public int nutrition_level_on_spawn = 100;

	public int min_coins_before_city_food = 5;

	public int months_till_pool_turns_into_flora = 30;

	public float item_repair_cost_multiplier = 0.5f;

	public int coins_for_road = 1;

	public int coins_for_zone = 5;

	public int coins_for_field = 1;

	public int coins_for_planting = 1;

	public int coins_for_fertilize = 1;

	public int coins_for_mine = 1;

	public int coins_for_cleaning = 1;

	public int coins_for_building = 6;
}
