using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ai.behaviours;
using db;

public class AssetManager
{
	public static TileEffectsLibrary tile_tile_effects;

	public static KingdomBannerLibrary kingdom_banners_library;

	public static CultureBannerLibrary culture_banners_library;

	public static ClanBannerLibrary clan_banners_library;

	public static ReligionBannerLibrary religion_banners_library;

	public static LanguageBannerLibrary language_banners_library;

	public static SubspeciesBannerLibrary subspecies_banners_library;

	public static FamilysBannerLibrary family_banners_library;

	public static WorldTimeScaleLibrary time_scales;

	public static OptionsLibrary options_library;

	public static WorldLawLibrary world_laws_library;

	public static WorldLawGroupLibrary world_law_groups;

	public static OnomasticsLibrary onomastics_library;

	public static OnomasticsEvolutionLibrary onomastics_evolution_library;

	public static LinguisticsLibrary linguistics_library;

	public static WordsLibrary words_library;

	public static SentencesLibrary sentences_library;

	public static StoryLibrary story_library;

	public static RarityLibrary rarity_library;

	public static GraphTimeLibrary graph_time_library;

	public static HistoryDataLibrary history_data_library;

	public static HistoryMetaDataLibrary history_meta_data_library;

	public static BaseStatsLibrary base_stats_library;

	public static ChromosomeTypeLibrary chromosome_type_library;

	public static GeneLibrary gene_library;

	public static NameplateLibrary nameplates_library;

	public static MetaTypeLibrary meta_type_library;

	public static MetaCustomizationLibrary meta_customization_library;

	public static MetaRepresentationLibrary meta_representation_library;

	public static DecisionsLibrary decisions_library;

	public static NeuralLayerLibrary neural_layers;

	public static LoyaltyLibrary loyalty_library;

	public static OpinionLibrary opinion_library;

	public static HappinessLibrary happiness_library;

	public static KingdomJobLibrary job_kingdom;

	public static BehaviourTaskKingdomLibrary tasks_kingdom;

	public static WorldLogLibrary world_log_library;

	public static HistoryGroupLibrary history_groups;

	public static CityJobLibrary job_city;

	public static BehaviourTaskCityLibrary tasks_city;

	public static ActorJobLibrary job_actor;

	public static BehaviourTaskActorLibrary tasks_actor;

	public static CitizenJobLibrary citizen_job_library;

	public static CultureTraitLibrary culture_traits;

	public static CultureTraitGroupLibrary culture_trait_groups;

	public static LanguageTraitLibrary language_traits;

	public static LanguageTraitGroupLibrary language_trait_groups;

	public static SubspeciesTraitLibrary subspecies_traits;

	public static SubspeciesTraitGroupLibrary subspecies_trait_groups;

	public static ClanTraitLibrary clan_traits;

	public static ClanTraitGroupLibrary clan_trait_groups;

	public static ReligionTraitLibrary religion_traits;

	public static ReligionTraitGroupLibrary religion_trait_groups;

	public static TraitRainLibrary trait_rains;

	public static CommunicationLibrary communication_library;

	public static CommunicationTopicLibrary communication_topic_library;

	public static BookTypeLibrary book_types;

	public static PersonalityLibrary personalities;

	public static ProfessionLibrary professions;

	public static DropsLibrary drops;

	public static BuildingLibrary buildings;

	public static ActorAssetLibrary actor_library;

	public static ActorTraitLibrary traits;

	public static ActorTraitGroupLibrary trait_groups;

	public static KingdomLibrary kingdoms;

	public static KingdomTraitLibrary kingdoms_traits;

	public static KingdomTraitGroupLibrary kingdoms_traits_groups;

	public static NameGeneratorLibrary name_generator;

	public static NameSetsLibrary name_sets;

	public static DisasterLibrary disasters;

	public static PhenotypeLibrary phenotype_library;

	public static BiomeLibrary biome_library;

	public static ResourceLibrary resources;

	public static ItemLibrary items;

	public static ItemModifierLibrary items_modifiers;

	public static ItemGroupLibrary item_groups;

	public static UnitHandToolLibrary unit_hand_tools;

	public static ProjectileLibrary projectiles;

	public static BuildOrderLibrary city_build_orders;

	public static ArchitectureLibrary architecture_library;

	public static CloudLibrary clouds;

	public static MonthLibrary months;

	public static TileLibrary tiles;

	public static TopTileLibrary top_tiles;

	public static TerraformLibrary terraform;

	public static PowerLibrary powers;

	public static SpellLibrary spells;

	public static StatusLibrary status;

	public static TesterJobLibrary tester_jobs;

	public static TesterBehTaskLibrary tester_tasks;

	public static MusicBoxLibrary music_box;

	public static AchievementLibrary achievements;

	public static AchievementGroupLibrary achievement_groups;

	public static SignalLibrary signals;

	public static MapGenSettingsLibrary map_gen_settings;

	public static MapGenTemplateLibrary map_gen_templates;

	public static QuantumSpriteLibrary quantum_sprites;

	public static WorldBehaviourLibrary world_behaviours;

	public static MapSizeLibrary map_sizes;

	public static WorldAgeLibrary era_library;

	public static EffectsLibrary effects_library;

	public static SimGlobals sim_globals_library;

	public static ColorStyleLibrary color_style_library;

	public static ClanColorsLibrary clan_colors_library;

	public static SubspeciesColorsLibrary subspecies_colors_library;

	public static FamiliesColorsLibrary families_colors_library;

	public static ArmiesColorsLibrary armies_colors_library;

	public static LanguagesColorsLibrary languages_colors_library;

	public static KingdomColorsLibrary kingdom_colors_library;

	public static CultureColorsLibrary culture_colors_library;

	public static ReligionColorsLibrary religion_colors_library;

	public static ArchitectMoodLibrary architect_mood_library;

	public static PlotsLibrary plots_library;

	public static PlotCategoryLibrary plot_category_library;

	public static TooltipLibrary tooltips;

	public static WarTypeLibrary war_types_library;

	public static HotkeyLibrary hotkey_library;

	public static StatisticsLibrary statistics_library;

	public static BrushLibrary brush_library;

	public static DebugToolLibrary debug_tool_library;

	public static CombatActionLibrary combat_action_library;

	public static DynamicSpritesLibrary dynamic_sprites_library;

	public static KnowledgeLibrary knowledge_library;

	public static WindowLibrary window_library;

	public static MetaTextReportLibrary meta_text_report_library;

	public static ListWindowLibrary list_window_library;

	public static PowerTabLibrary power_tab_library;

	public static LocaleGroupLibrary locale_groups_library;

	public static GameLanguageLibrary game_language_library;

	private static AssetManager _instance;

	private readonly List<BaseAssetLibrary> _list = new List<BaseAssetLibrary>();

	private readonly Dictionary<string, BaseAssetLibrary> _dict = new Dictionary<string, BaseAssetLibrary>();

	private string _assetgv;

	public static HashSet<string> missing_locale_keys = new HashSet<string>();

	public static void clear()
	{
		_instance = null;
	}

	public static void initMain()
	{
		if (_instance == null)
		{
			_instance = new AssetManager();
		}
	}

	public static void init()
	{
		_instance.initLibs();
	}

	public AssetManager()
	{
		_assetgv = Config.gv;
		add(game_language_library = new GameLanguageLibrary(), "game_languages");
		add(options_library = new OptionsLibrary(), "options_library");
	}

	private void initLibs()
	{
		add(tile_tile_effects = new TileEffectsLibrary(), "tile_effects");
		add(base_stats_library = new BaseStatsLibrary(), "base_stats_library");
		add(world_log_library = new WorldLogLibrary(), "world_log_library");
		add(history_groups = new HistoryGroupLibrary(), "history_groups");
		add(decisions_library = new DecisionsLibrary(), "decisions_library");
		add(neural_layers = new NeuralLayerLibrary(), "neural_layer_library");
		add(graph_time_library = new GraphTimeLibrary(), "graph_time_library");
		add(history_data_library = new HistoryDataLibrary(), "history_data_library");
		add(history_meta_data_library = new HistoryMetaDataLibrary(), "history_meta_data_library");
		add(world_laws_library = new WorldLawLibrary(), "world_laws_library");
		add(world_law_groups = new WorldLawGroupLibrary(), "world_law_groups");
		add(meta_type_library = new MetaTypeLibrary(), "meta_type_library");
		add(meta_text_report_library = new MetaTextReportLibrary(), "meta_text_report_library");
		add(meta_customization_library = new MetaCustomizationLibrary(), "meta_customization_library");
		add(meta_representation_library = new MetaRepresentationLibrary(), "meta_representation_library");
		add(culture_banners_library = new CultureBannerLibrary(), "culture_banners_library");
		add(kingdom_banners_library = new KingdomBannerLibrary(), "kingdom_banners_library");
		add(clan_banners_library = new ClanBannerLibrary(), "clan_banners_library");
		add(religion_banners_library = new ReligionBannerLibrary(), "religion_banners_library");
		add(language_banners_library = new LanguageBannerLibrary(), "language_banners_library");
		add(subspecies_banners_library = new SubspeciesBannerLibrary(), "subspecies_banners_library");
		add(family_banners_library = new FamilysBannerLibrary(), "family_banners_library");
		add(time_scales = new WorldTimeScaleLibrary(), "world_time_scales_library");
		add(communication_library = new CommunicationLibrary(), "communication_library");
		add(communication_topic_library = new CommunicationTopicLibrary(), "communication_topic_library");
		add(city_build_orders = new BuildOrderLibrary(), "city_build_orders");
		add(architecture_library = new ArchitectureLibrary(), "architecture_library");
		add(book_types = new BookTypeLibrary(), "book_types_library");
		add(nameplates_library = new NameplateLibrary(), "nameplates_library");
		add(combat_action_library = new CombatActionLibrary(), "combat_action_library");
		add(biome_library = new BiomeLibrary(), "biome_library");
		add(phenotype_library = new PhenotypeLibrary(), "phenotype_library");
		add(dynamic_sprites_library = new DynamicSpritesLibrary(), "dynamic_sprites_library");
		add(debug_tool_library = new DebugToolLibrary(), "debug_tool_library");
		add(brush_library = new BrushLibrary(), "brush_library");
		add(chromosome_type_library = new ChromosomeTypeLibrary(), "chromosome_type_library");
		add(gene_library = new GeneLibrary(), "gene_library");
		add(loyalty_library = new LoyaltyLibrary(), "loyalty_library");
		add(opinion_library = new OpinionLibrary(), "opinion_library");
		add(happiness_library = new HappinessLibrary(), "happiness_library");
		add(hotkey_library = new HotkeyLibrary(), "hotkey_library");
		add(tooltips = new TooltipLibrary(), "tooltips");
		add(war_types_library = new WarTypeLibrary(), "war_types_library");
		add(sim_globals_library = new SimGlobals(), "sim_globals_library");
		add(color_style_library = new ColorStyleLibrary(), "color_style_library");
		add(effects_library = new EffectsLibrary(), "effects_library");
		add(kingdom_colors_library = new KingdomColorsLibrary(), "kingdom_colors_library");
		add(clan_colors_library = new ClanColorsLibrary(), "clan_colors_library");
		add(subspecies_colors_library = new SubspeciesColorsLibrary(), "species_colors_library");
		add(languages_colors_library = new LanguagesColorsLibrary(), "language_colors_library");
		add(families_colors_library = new FamiliesColorsLibrary(), "families_colors_library");
		add(armies_colors_library = new ArmiesColorsLibrary(), "armies_colors_library");
		add(culture_colors_library = new CultureColorsLibrary(), "culture_colors_library");
		add(religion_colors_library = new ReligionColorsLibrary(), "religion_colors_library");
		add(months = new MonthLibrary(), "month_library");
		add(era_library = new WorldAgeLibrary(), "era_library");
		add(clouds = new CloudLibrary(), "cloud_library");
		add(map_sizes = new MapSizeLibrary(), "map_sizes");
		add(music_box = new MusicBoxLibrary(), "music_box");
		add(tiles = new TileLibrary(), "tiles");
		add(top_tiles = new TopTileLibrary(), "top_tiles");
		add(culture_traits = new CultureTraitLibrary(), "culture_traits");
		add(culture_trait_groups = new CultureTraitGroupLibrary(), "culture_trait_groups");
		add(language_traits = new LanguageTraitLibrary(), "language_traits");
		add(language_trait_groups = new LanguageTraitGroupLibrary(), "language_trait_groups");
		add(clan_traits = new ClanTraitLibrary(), "clan_traits");
		add(clan_trait_groups = new ClanTraitGroupLibrary(), "clan_trait_groups");
		add(subspecies_traits = new SubspeciesTraitLibrary(), "subspecies_traits");
		add(subspecies_trait_groups = new SubspeciesTraitGroupLibrary(), "subspecies_trait_groups");
		add(religion_traits = new ReligionTraitLibrary(), "religion_traits");
		add(religion_trait_groups = new ReligionTraitGroupLibrary(), "religion_trait_groups");
		add(trait_rains = new TraitRainLibrary(), "trait_rains");
		add(professions = new ProfessionLibrary(), "professions");
		add(quantum_sprites = new QuantumSpriteLibrary(), "quantum_sprites");
		add(world_behaviours = new WorldBehaviourLibrary(), "world_behaviours");
		add(personalities = new PersonalityLibrary(), "personalities");
		add(drops = new DropsLibrary(), "drops");
		add(status = new StatusLibrary(), "status");
		add(spells = new SpellLibrary(), "spells");
		add(citizen_job_library = new CitizenJobLibrary(), "citizen_job_library");
		add(tasks_actor = new BehaviourTaskActorLibrary(), "beh_actor");
		add(tasks_city = new BehaviourTaskCityLibrary(), "beh_city");
		add(tasks_kingdom = new BehaviourTaskKingdomLibrary(), "beh_kingdom");
		add(traits = new ActorTraitLibrary(), "traits");
		add(trait_groups = new ActorTraitGroupLibrary(), "trait_groups");
		add(plots_library = new PlotsLibrary(), "plots");
		add(plot_category_library = new PlotCategoryLibrary(), "plot_group");
		add(kingdoms = new KingdomLibrary(), "kingdoms");
		add(kingdoms_traits_groups = new KingdomTraitGroupLibrary(), "kingdom_trait_group");
		add(kingdoms_traits = new KingdomTraitLibrary(), "kingdom_traits");
		add(actor_library = new ActorAssetLibrary(), "units");
		add(buildings = new BuildingLibrary(), "buildings");
		add(name_generator = new NameGeneratorLibrary(), "name_generator");
		add(name_sets = new NameSetsLibrary(), "name_sets");
		add(disasters = new DisasterLibrary(), "disasters");
		add(job_actor = new ActorJobLibrary(), "job_actor");
		add(job_city = new CityJobLibrary(), "job_city");
		add(job_kingdom = new KingdomJobLibrary(), "job_kingdom");
		add(powers = new PowerLibrary(), "powers");
		add(items = new ItemLibrary(), "items");
		add(items_modifiers = new ItemModifierLibrary(), "items_modifiers");
		add(item_groups = new ItemGroupLibrary(), "item_groups");
		add(unit_hand_tools = new UnitHandToolLibrary(), "tools");
		add(resources = new ResourceLibrary(), "resources");
		add(terraform = new TerraformLibrary(), "terraform");
		add(projectiles = new ProjectileLibrary(), "projectiles");
		add(signals = new SignalLibrary(), "signals");
		add(achievement_groups = new AchievementGroupLibrary(), "achievement_groups");
		add(map_gen_templates = new MapGenTemplateLibrary(), "map_gen_templates");
		add(map_gen_settings = new MapGenSettingsLibrary(), "map_gen_settings");
		add(statistics_library = new StatisticsLibrary(), "statistics_library");
		add(linguistics_library = new LinguisticsLibrary(), "linguistics_library");
		add(words_library = new WordsLibrary(), "words_library");
		add(sentences_library = new SentencesLibrary(), "sentences_library");
		add(story_library = new StoryLibrary(), "story_library");
		add(onomastics_library = new OnomasticsLibrary(), "onomastics_library");
		add(onomastics_evolution_library = new OnomasticsEvolutionLibrary(), "onomastics_evolution_library");
		add(rarity_library = new RarityLibrary(), "rarity_library");
		add(knowledge_library = new KnowledgeLibrary(), "knowledge_library");
		add(window_library = new WindowLibrary(), "window_library");
		add(list_window_library = new ListWindowLibrary(), "list_window_library");
		add(power_tab_library = new PowerTabLibrary(), "power_tab_library");
		add(architect_mood_library = new ArchitectMoodLibrary(), "architect_mood_library");
		add(achievements = new AchievementLibrary(), "achievements");
		if (DebugConfig.isOn(DebugOption.TesterLibs))
		{
			loadAutoTester();
		}
		add(locale_groups_library = new LocaleGroupLibrary(), "locale_groups");
		foreach (BaseAssetLibrary item in _list)
		{
			item.post_init();
		}
		foreach (BaseAssetLibrary item2 in _list)
		{
			item2.linkAssets();
		}
	}

	public static void loadAutoTester()
	{
		if (tester_jobs == null)
		{
			_instance.add(tester_jobs = new TesterJobLibrary(), "tester_jobs");
			_instance.add(tester_tasks = new TesterBehTaskLibrary(), "tester_tasks");
		}
	}

	internal static void generateMissingLocalesFile()
	{
	}

	public void exportAssets()
	{
		if (DebugConfig.isOn(DebugOption.ExportAssetLibraries))
		{
			MiniBench miniBench = new MiniBench("exportAssets", 25L);
			string path = "GenAssets/wbassets";
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			ParallelOptions parallelOptions = new ParallelOptions
			{
				MaxDegreeOfParallelism = 3
			};
			Parallel.ForEach(_list, parallelOptions, delegate(BaseAssetLibrary pLib)
			{
				pLib.exportAssets();
			});
			miniBench.Dispose();
		}
	}

	private void add(BaseAssetLibrary pLibrary, string pId)
	{
		if (_assetgv[0] == '0')
		{
			pLibrary.init();
			_list.Add(pLibrary);
			_dict.Add(pId, pLibrary);
			pLibrary.id = pId;
		}
	}

	public static IEnumerable<BaseAssetLibrary> getList()
	{
		return _instance._list;
	}

	public static bool has(string pLibraryID)
	{
		return _instance._dict.ContainsKey(pLibraryID);
	}

	public static BaseAssetLibrary get(string pLibraryID)
	{
		return _instance._dict[pLibraryID];
	}
}
