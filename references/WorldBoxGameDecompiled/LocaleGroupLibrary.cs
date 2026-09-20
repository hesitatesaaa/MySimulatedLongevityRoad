using System.Collections.Generic;
using strings;

public class LocaleGroupLibrary : AssetLibrary<LocaleGroupAsset>
{
	private static Dictionary<string, LocaleGroupAsset> _already_found = new Dictionary<string, LocaleGroupAsset>();

	public override void init()
	{
		base.init();
		add(new LocaleGroupAsset
		{
			id = "achievements"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("achievement_groups", "achievements");
		t.starts_with.Add("achievement");
		add(new LocaleGroupAsset
		{
			id = "api_discord"
		});
		t.starts_with.Add("discordsocial");
		t.starts_with.Add("discord_");
		add(new LocaleGroupAsset
		{
			id = "api_steam"
		});
		t.starts_with_priority.Add("steam");
		t.starts_with.Add("promo_steam");
		add(new LocaleGroupAsset
		{
			id = "biomes"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("biome_library");
		add(new LocaleGroupAsset
		{
			id = "books"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("book_types_library");
		t.starts_with.Add("book");
		add(new LocaleGroupAsset
		{
			id = "debug"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("debug_tool_library", "tester_jobs", "tester_tasks");
		t.contains.Add("debug");
		t.starts_with.Add("search_by_");
		t.starts_with.Add("dt_");
		t.starts_with_priority.Add("tab_debug");
		add(new LocaleGroupAsset
		{
			id = "genes"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("chromosome_type_library", "gene_library");
		t.starts_with.Add("neuro");
		t.starts_with.Add("dna");
		t.starts_with.Add("gene");
		t.starts_with.Add("nucleo_");
		t.contains.Add("amplif");
		t.starts_with.Add("locus");
		t.starts_with.Add("chromosomes");
		t.starts_with.Add("sequence_synergy");
		add(new LocaleGroupAsset
		{
			id = "history"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("world_log_library", "history_groups");
		t.matches.Add("diplomacy_peace");
		t.starts_with.Add("race_dead_");
		add(new LocaleGroupAsset
		{
			id = "hotkeys"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("hotkey_library");
		t.starts_with.Add("hotkey_");
		add(new LocaleGroupAsset
		{
			id = "creatures"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("units");
		t.starts_with.Add("creature_statistics");
		add(new LocaleGroupAsset
		{
			id = "traits_units"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("traits", "trait_groups");
		add(new LocaleGroupAsset
		{
			id = "traits_cultures"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("culture_traits", "culture_trait_groups");
		add(new LocaleGroupAsset
		{
			id = "traits_languages"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("language_traits", "language_trait_groups");
		add(new LocaleGroupAsset
		{
			id = "traits_clans"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("clan_traits", "clan_trait_groups");
		add(new LocaleGroupAsset
		{
			id = "traits_subspecies"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("subspecies_traits", "subspecies_trait_groups");
		add(new LocaleGroupAsset
		{
			id = "traits_religions"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("religion_traits", "religion_trait_groups");
		add(new LocaleGroupAsset
		{
			id = "traits_kingdoms"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("kingdom_traits", "kingdom_trait_group");
		add(new LocaleGroupAsset
		{
			id = "meta_traits"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("rarity_library");
		t.contains.Add("trait");
		add(new LocaleGroupAsset
		{
			id = "meta_alliances"
		});
		t.starts_with.Add("alliance");
		t.starts_with.Add("unity_");
		t.starts_with.Add("whisper_");
		add(new LocaleGroupAsset
		{
			id = "meta_clans"
		});
		t.starts_with.Add("clan");
		add(new LocaleGroupAsset
		{
			id = "meta_religions"
		});
		t.starts_with.Add("religion");
		add(new LocaleGroupAsset
		{
			id = "meta_families"
		});
		t.starts_with.Add("families");
		add(new LocaleGroupAsset
		{
			id = "meta_cultures"
		});
		t.starts_with.Add("culture");
		add(new LocaleGroupAsset
		{
			id = "meta_languages"
		});
		t.starts_with_priority.Add("language");
		add(new LocaleGroupAsset
		{
			id = "meta_plots"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("plots", "plot_group");
		t.starts_with.Add("plot");
		t.starts_with.Add("can_be_done");
		add(new LocaleGroupAsset
		{
			id = "meta_subspecies"
		});
		t.starts_with.Add("subspecies");
		t.starts_with.Add("race");
		t.contains.Add("species");
		t.checker = delegate(string pKey)
		{
			if (typeof(S_SocialStructure).hasField(pKey))
			{
				return true;
			}
			return typeof(S_TaxonomyRank).hasField(pKey) ? true : false;
		};
		add(new LocaleGroupAsset
		{
			id = "meta_wars"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("war_types_library");
		t.matches.Add("war");
		t.starts_with.Add("war_");
		t.starts_with.Add("wars");
		t.starts_with.Add("attacke");
		t.starts_with.Add("defende");
		add(new LocaleGroupAsset
		{
			id = "metas"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("meta_customization_library", "knowledge_library");
		add(new LocaleGroupAsset
		{
			id = "happiness"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("happiness_library");
		add(new LocaleGroupAsset
		{
			id = "loyalty"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("loyalty_library");
		add(new LocaleGroupAsset
		{
			id = "opinion"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("opinion_library");
		add(new LocaleGroupAsset
		{
			id = "meta_reports"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("meta_text_report_library");
		add(new LocaleGroupAsset
		{
			id = "moods"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("architect_mood_library");
		t.starts_with.Add("mood_");
		t.starts_with.Add("architect_mood_");
		add(new LocaleGroupAsset
		{
			id = "onomastics"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("onomastics_library", "onomastics_evolution_library");
		t.matches.Add("naming_parts");
		t.matches.Add("naming_examples");
		t.matches.Add("group_editor");
		t.starts_with.Add("onomastic");
		add(new LocaleGroupAsset
		{
			id = "possession"
		});
		t.starts_with.Add("possession");
		t.starts_with.Add("crabzilla");
		add(new LocaleGroupAsset
		{
			id = "resources"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("resources");
		add(new LocaleGroupAsset
		{
			id = "powers"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("powers");
		t.starts_with.Add("Infinity Coin");
		add(new LocaleGroupAsset
		{
			id = "ui_options"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("options_library");
		t.matches.Add("resolution");
		t.matches.Add("additional_sounds");
		t.matches.Add("windowed_mode");
		t.starts_with.Add("setting");
		t.starts_with.Add("option_");
		t.starts_with_priority.Add("button_option");
		t.starts_with_priority.Add("graphics");
		t.starts_with.Add("username");
		add(new LocaleGroupAsset
		{
			id = "ui_tabs"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("power_tab_library");
		t.starts_with.Add("tab_");
		add(new LocaleGroupAsset
		{
			id = "base_stats"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("base_stats_library");
		add(new LocaleGroupAsset
		{
			id = "personalities"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("personalities");
		add(new LocaleGroupAsset
		{
			id = "statistics"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("graph_time_library", "history_data_library", "history_meta_data_library", "statistics_library");
		t.starts_with_priority.Add("chart_");
		t.starts_with_priority.Add("graph");
		t.starts_with_priority.Add("world_statistics_");
		t.starts_with_priority.Add("statistics_");
		t.contains.Add("_stat");
		add(new LocaleGroupAsset
		{
			id = "information"
		});
		t.matches.Add("loyalty_world_law");
		t.matches.Add("males_single");
		t.matches.Add("population_pyramid");
		t.matches.Add("status_waiting_for_passengers");
		t.matches.Add("task");
		t.matches.Add("total_deaths");
		t.matches.Add("total_knowledge");
		t.matches.Add("total_knowledge_info");
		t.matches.Add("total_population_money");
		t.matches.Add("upkeep_buildings");
		t.matches.Add("upkeep_homeless");
		t.matches.Add("yearly_gain");
		t.matches.Add("taxonomy_description_tooltip");
		t.matches.Add("vegetation");
		t.matches.Add("ownerless_item_info");
		t.matches.Add("ownerless_item_tip");
		t.matches.Add("capital");
		t.matches.Add("alpha");
		t.matches.Add("age");
		t.matches.Add("ages");
		t.matches.Add("amount");
		t.matches.Add("amount");
		t.matches.Add("ancestor_families");
		t.matches.Add("ancestor_family");
		t.matches.Add("area");
		t.matches.Add("best_friend");
		t.matches.Add("biggest_level");
		t.matches.Add("birthday");
		t.matches.Add("birthplace");
		t.matches.Add("carrying");
		t.matches.Add("couples");
		t.matches.Add("created");
		t.matches.Add("creator");
		t.matches.Add("creators_clan");
		t.matches.Add("creature");
		t.matches.Add("deity");
		t.matches.Add("deity");
		t.matches.Add("durability");
		t.matches.Add("fastest");
		t.matches.Add("females_single");
		t.matches.Add("fertility");
		t.matches.Add("food_consumed");
		t.matches.Add("founded");
		t.matches.Add("founded_in");
		t.matches.Add("founder");
		t.matches.Add("founder_clan");
		t.matches.Add("fullest");
		t.matches.Add("grandparents");
		t.matches.Add("great_clan_of");
		t.matches.Add("happiest");
		t.matches.Add("happy_units");
		t.matches.Add("happy_units_description");
		t.matches.Add("heir");
		t.matches.Add("home_village");
		t.matches.Add("house");
		t.matches.Add("hunger");
		t.matches.Add("hungriest");
		t.matches.Add("influence");
		t.matches.Add("instigator");
		t.matches.Add("instigator_from");
		t.matches.Add("inventory");
		t.matches.Add("level");
		t.matches.Add("lifespan_female");
		t.matches.Add("lifespan_male");
		t.matches.Add("locate");
		t.matches.Add("lover");
		t.matches.Add("max_age");
		t.matches.Add("max_children");
		t.matches.Add("members");
		t.matches.Add("mobs");
		t.matches.Add("nutrition");
		t.matches.Add("oldest");
		t.matches.Add("origin");
		t.matches.Add("origin_families");
		t.matches.Add("origin_family");
		t.matches.Add("parents");
		t.matches.Add("passengers");
		t.matches.Add("past_kings");
		t.matches.Add("past_leaders");
		t.matches.Add("resources");
		t.matches.Add("richest");
		t.matches.Add("ruler");
		t.matches.Add("ruler_money");
		t.matches.Add("saddest");
		t.matches.Add("sex");
		t.matches.Add("siblings");
		t.matches.Add("loot");
		t.matches.Add("smartest");
		t.matches.Add("speakers");
		t.matches.Add("started_at");
		t.matches.Add("started_by");
		t.matches.Add("status");
		t.matches.Add("statuses");
		t.matches.Add("strongest");
		t.matches.Add("tax");
		t.matches.Add("tribute");
		t.matches.Add("unhappy_units");
		t.matches.Add("unhappy_units_description");
		t.matches.Add("unit_age");
		t.matches.Add("unit_age_description");
		t.matches.Add("upkeep_army");
		t.matches.Add("year");
		t.matches.Add("year_era");
		t.matches.Add("years_ago");
		t.matches.Add("youngest");
		t.matches.Add("zone_range");
		t.matches.Add("zones");
		t.matches.Add("zones_description");
		t.matches.Add("residents");
		t.matches.Add("family_heads");
		t.matches.Add("family_members");
		t.starts_with.Add("most_");
		add(new LocaleGroupAsset
		{
			id = "status"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("status");
		add(new LocaleGroupAsset
		{
			id = "technologies"
		});
		t.starts_with.Add("tech_");
		add(new LocaleGroupAsset
		{
			id = "ui_brushes"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("brush_library");
		add(new LocaleGroupAsset
		{
			id = "ui_buttons"
		});
		t.starts_with.Add("button_");
		add(new LocaleGroupAsset
		{
			id = "ui_clicks"
		});
		t.starts_with.Add("click_");
		add(new LocaleGroupAsset
		{
			id = "ui_loading_screen"
		});
		t.starts_with.Add("loading_");
		add(new LocaleGroupAsset
		{
			id = "ui_new_world"
		});
		t.starts_with.Add("maptype");
		t.starts_with.Add("generat");
		t.starts_with.Add("custom_world");
		t.starts_with.Add("template_config");
		add(new LocaleGroupAsset
		{
			id = "ui_premium"
		});
		t.starts_with.Add("free_");
		t.starts_with.Add("ios_ad");
		t.starts_with.Add("ad_");
		t.starts_with.Add("waiting_for_ad");
		t.starts_with.Add("unlock_");
		t.starts_with.Add("prem_");
		t.starts_with.Add("premium_");
		t.starts_with.Add("restore");
		add(new LocaleGroupAsset
		{
			id = "ui_sort"
		});
		t.starts_with_priority.Add("sort_by_");
		t.starts_with_priority.Add("default_sort");
		add(new LocaleGroupAsset
		{
			id = "ui_speed"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("world_time_scales_library");
		add(new LocaleGroupAsset
		{
			id = "ui_tips"
		});
		t.starts_with.Add("tip_");
		add(new LocaleGroupAsset
		{
			id = "ui_tutorial"
		});
		t.starts_with.Add("tut_");
		add(new LocaleGroupAsset
		{
			id = "ui_worlds"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("map_sizes", "map_gen_templates", "map_gen_settings");
		t.matches.Add("world");
		t.matches.Add("worlds");
		t.matches.Add("your_world");
		t.matches.Add("your_worlds");
		t.matches.Add("no_world_found");
		t.matches.Add("name_your_world");
		t.matches.Add("describe_your_world");
		t.starts_with.Add("map_size");
		t.starts_with.Add("new_world");
		t.starts_with.Add("modded");
		t.starts_with.Add("mods_");
		t.starts_with.Add("save");
		t.starts_with.Add("load_");
		t.starts_with.Add("world_");
		t.starts_with.Add("create_worlds");
		t.starts_with.Add("future_save_version");
		add(new LocaleGroupAsset
		{
			id = "ui_windows"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("list_window_library");
		t.starts_with.Add("window_");
		t.starts_with.Add("title_");
		t.starts_with.Add("official_");
		t.starts_with.Add("community");
		t.starts_with.Add("facebook");
		t.starts_with.Add("link_");
		t.starts_with.Add("thank");
		t.starts_with.Add("ui_class_");
		t.starts_with.Add("under_development");
		add(new LocaleGroupAsset
		{
			id = "ui_workshop"
		});
		t.starts_with.Add("report");
		t.starts_with.Add("upload");
		t.starts_with.Add("workshop");
		t.starts_with.Add("sharing_your_world");
		t.starts_with.Add("world_uploaded");
		add(new LocaleGroupAsset
		{
			id = "ui_worldnet"
		});
		t.matches.Add("pasted_world_from_clipboard");
		t.matches.Add("your_account");
		t.matches.Add("logout");
		t.matches.Add("logging_in");
		t.matches.Add("email");
		t.matches.Add("missing_email");
		t.matches.Add("email_already_in_use");
		t.contains.Add("fav_error");
		t.contains.Add("authentication_error");
		t.contains.Add("worldnet");
		t.contains.Add("logout_");
		t.contains.Add("login");
		t.contains.Add("invalid");
		t.contains.Add("find_world");
		t.contains.Add("password");
		t.contains.Add("register");
		t.starts_with.Add("generic");
		t.starts_with.Add("getting_your");
		t.starts_with_priority.Add("load_a_world");
		t.starts_with_priority.Add("load_world_with_id");
		t.starts_with_priority.Add("welcome_worldnet");
		t.starts_with.Add("share_your_world");
		t.starts_with.Add("status_logged_in");
		t.starts_with_priority.Add("username_taken");
		t.starts_with_priority.Add("user_");
		t.starts_with_priority.Add("world_net");
		t.starts_with_priority.Add("worlds_max_maps_uploaded");
		t.starts_with_priority.Add("worlds_no");
		t.starts_with_priority.Add("worlds_error");
		t.starts_with_priority.Add("thank_you_for_your_report");
		t.starts_with_priority.Add("num_comments");
		t.starts_with_priority.Add("num_subscriptions");
		t.starts_with_priority.Add("num_upvotes");
		add(new LocaleGroupAsset
		{
			id = "ui_welcome"
		});
		t.starts_with_priority.Add("tip0");
		t.starts_with_priority.Add("update_");
		t.starts_with_priority.Add("feature_");
		t.starts_with.Add("vote");
		t.starts_with.Add("rate");
		t.starts_with.Add("welcome");
		t.starts_with.Add("your_heroes");
		add(new LocaleGroupAsset
		{
			id = "ui_general"
		});
		t.matches.Add("king");
		t.matches.Add("knowledge");
		t.matches.Add("knowledge_description");
		t.matches.Add("show_ui_description");
		t.matches.Add("translators");
		t.matches.Add("try_again");
		t.matches.Add("unlocks_goodie");
		t.matches.Add("unlocks_goodies");
		t.matches.Add("other");
		t.matches.Add("epic_items");
		t.matches.Add("disasters");
		t.matches.Add("description");
		t.matches.Add("help");
		t.matches.Add("about");
		t.matches.Add("browse_worlds");
		t.matches.Add("auto_saved_worlds");
		t.matches.Add("auto_saves_tip");
		t.matches.Add("auto_saves_tip_description");
		t.matches.Add("brush");
		t.matches.Add("cancel");
		t.matches.Add("canceled");
		t.matches.Add("changed_brush");
		t.matches.Add("changed_worldspeed");
		t.matches.Add("close");
		t.matches.Add("color");
		t.matches.Add("continue");
		t.matches.Add("copied_world_to_clipboard");
		t.matches.Add("date");
		t.matches.Add("delete");
		t.matches.Add("delete_confirmation");
		t.matches.Add("delete_confirmation_warning");
		t.matches.Add("downloads");
		t.matches.Add("enjoy_the_game");
		t.matches.Add("error_description");
		t.matches.Add("error_happened_logs");
		t.matches.Add("game_created_by");
		t.matches.Add("game_paused");
		t.matches.Add("game_unpaused");
		t.matches.Add("get_it");
		t.matches.Add("getting_worlds");
		t.matches.Add("go_back");
		t.matches.Add("history");
		t.matches.Add("leave_feedback");
		t.matches.Add("logs_folder");
		t.matches.Add("mouse_wheel");
		t.matches.Add("not_found");
		t.matches.Add("open_console");
		t.matches.Add("open_in_steam");
		t.matches.Add("open_workshop");
		t.matches.Add("patch_log");
		t.matches.Add("pause_ages");
		t.matches.Add("pause_ages_description");
		t.matches.Add("premium");
		t.matches.Add("press_for_next_tip");
		t.matches.Add("quit_game_desktop");
		t.matches.Add("reset");
		t.matches.Add("send_feedback");
		t.matches.Add("show_ui");
		t.matches.Add("short_on");
		t.matches.Add("short_off");
		t.matches.Add("sounds");
		t.matches.Add("sounds_ambient");
		t.matches.Add("load");
		t.matches.Add("actor_locked_tooltip_text_achievement");
		t.matches.Add("actor_locked_tooltip_text_exploration");
		t.matches.Add("news");
		t.matches.Add("outdated_version");
		t.matches.Add("overview");
		t.matches.Add("warning");
		t.matches.Add("stats");
		t.matches.Add("statistics");
		t.matches.Add("special_thanks");
		t.matches.Add("slot");
		t.matches.Add("plays");
		t.matches.Add("empty_dead_list");
		t.matches.Add("empty_non_sapient_list");
		t.matches.Add("empty_sapient_list");
		t.matches.Add("infinity_coin_used");
		t.matches.Add("watch_ad");
		t.matches.Add("play_now");
		t.starts_with.Add("news_");
		add(new LocaleGroupAsset
		{
			id = "world_ages"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("month_library", "era_library");
		t.matches.Add("randomize_ages");
		t.matches.Add("randomize_ages_description");
		t.starts_with.Add("next_age_");
		t.starts_with.Add("age_");
		t.starts_with.Add("ages_");
		t.starts_with.Add("all_ages");
		t.starts_with_priority.Add("world_age");
		add(new LocaleGroupAsset
		{
			id = "world_laws"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("world_laws_library", "world_law_groups");
		t.starts_with_priority.Add("forbidden_knowledg");
		t.starts_with_priority.Add("world_curse");
		t.starts_with_priority.Add("world_law");
		add(new LocaleGroupAsset
		{
			id = "unit_tasks"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("decisions_library", "beh_actor", "job_actor");
		add(new LocaleGroupAsset
		{
			id = "mind"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("neural_layer_library");
		t.contains.Add("toggle_all_neurons");
		t.contains.Add("toggle_all_neurons_description");
		add(new LocaleGroupAsset
		{
			id = "favorites"
		});
		t.contains.Add("favorite");
		add(new LocaleGroupAsset
		{
			id = "items"
		});
		t.libraries = AssetLibrary<LocaleGroupAsset>.a<string>("items", "items_modifiers", "item_groups");
		t.starts_with.Add("item_");
		t.contains.Add("equipment");
		add(new LocaleGroupAsset
		{
			id = "kingdoms"
		});
		t.contains.Add("_kingdom_");
		t.starts_with.Add("kingdom");
		t.starts_with.Add("village");
		add(new LocaleGroupAsset
		{
			id = "_others"
		});
	}

	public override void editorDiagnostic()
	{
	}
}
