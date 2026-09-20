using System;
using System.Collections.Generic;
using Beebyte.Obfuscator;

[Serializable]
[ObfuscateLiterals]
public class StatisticsLibrary : AssetLibrary<StatisticsAsset>
{
	internal static readonly List<StatisticsAsset> power_tracker_pool = new List<StatisticsAsset>();

	private static readonly string _unknown_text = Toolbox.coloredString("???", ColorStyleLibrary.m.color_dead_text);

	public override void init()
	{
		base.init();
		addStatsGeneralMain();
		addStats();
		addStatsNoos();
		addStatsDeaths();
		addStatsTiles();
		addStatsBiomes();
	}

	private void addStats()
	{
		add(new StatisticsAsset
		{
			id = "world_name",
			rarity = 1,
			string_action = (StatisticsAsset _) => World.world.map_stats.name ?? ""
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_infected",
			list_window_meta_type = MetaType.Unit,
			localized_key = "world_statistics_infected",
			steam_activity = "#Status_stat_value",
			rarity = 1,
			path_icon = "ui/Icons/actor_traits/iconInfected",
			long_action = delegate
			{
				long num = 0L;
				List<Actor> simpleList = World.world.units.getSimpleList();
				for (int i = 0; i < simpleList.Count; i++)
				{
					if (simpleList[i].isSick())
					{
						num++;
					}
				}
				return num;
			},
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_houses",
			localized_key_description = "houses".Description(),
			list_window_meta_type = MetaType.City,
			path_icon = "ui/Icons/iconBuildings",
			long_action = delegate
			{
				long num = 0L;
				List<Building> simpleList = World.world.buildings.getSimpleList();
				for (int i = 0; i < simpleList.Count; i++)
				{
					if (simpleList[i].asset.city_building)
					{
						num++;
					}
				}
				return num;
			},
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_houses_built",
			list_window_meta_type = MetaType.World,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobBuilder",
			long_action = (StatisticsAsset _) => World.world.map_stats.housesBuilt
		});
		add(new StatisticsAsset
		{
			id = "houses",
			rarity = 4,
			path_icon = "ui/Icons/iconBuildings",
			string_action = delegate
			{
				string text = "";
				long num = get("world_statistics_houses").long_action(null);
				long num2 = get("world_statistics_houses_destroyed").long_action(null);
				if (num < 1 && num2 < 1)
				{
					return "";
				}
				text += LocalizedTextManager.getText("world_statistics_houses_all");
				text = text.Replace("$houses$", num.ToText());
				return text.Replace("$destroyed$", num2.ToText());
			}
		});
		add(new StatisticsAsset
		{
			id = "alliances",
			localized_key = "statistics_alliances",
			list_window_meta_type = MetaType.Alliance,
			path_icon = "ui/Icons/iconAllianceList",
			long_action = (StatisticsAsset _) => World.world.alliances.Count
		});
		add(new StatisticsAsset
		{
			id = "books",
			localized_key = "books",
			list_window_meta_type = MetaType.Language,
			path_icon = "ui/Icons/iconBooks",
			long_action = (StatisticsAsset _) => World.world.books.Count
		});
		add(new StatisticsAsset
		{
			id = "clans",
			localized_key = "statistics_clans",
			list_window_meta_type = MetaType.Clan,
			path_icon = "ui/Icons/iconClanList",
			long_action = (StatisticsAsset _) => World.world.clans.Count
		});
		add(new StatisticsAsset
		{
			id = "cultures",
			localized_key = "statistics_cultures",
			list_window_meta_type = MetaType.Culture,
			path_icon = "ui/Icons/iconCultureList",
			long_action = (StatisticsAsset _) => World.world.cultures.Count
		});
		add(new StatisticsAsset
		{
			id = "families",
			localized_key = "statistics_families",
			list_window_meta_type = MetaType.Family,
			path_icon = "ui/Icons/iconFamilyList",
			long_action = (StatisticsAsset _) => World.world.families.Count
		});
		add(new StatisticsAsset
		{
			id = "plots",
			localized_key = "statistics_plots",
			list_window_meta_type = MetaType.Plot,
			path_icon = "ui/Icons/iconPlotList",
			long_action = (StatisticsAsset _) => World.world.plots.Count
		});
		add(new StatisticsAsset
		{
			id = "languages",
			localized_key = "statistics_languages",
			list_window_meta_type = MetaType.Language,
			path_icon = "ui/Icons/iconLanguageList",
			long_action = (StatisticsAsset _) => World.world.languages.Count
		});
		add(new StatisticsAsset
		{
			id = "religions",
			localized_key = "statistics_religions",
			list_window_meta_type = MetaType.Religion,
			path_icon = "ui/Icons/iconReligionList",
			long_action = (StatisticsAsset _) => World.world.religions.Count
		});
		add(new StatisticsAsset
		{
			id = "subspecies",
			localized_key = "statistics_subspecies",
			list_window_meta_type = MetaType.Subspecies,
			path_icon = "ui/Icons/iconSpecies",
			long_action = (StatisticsAsset _) => World.world.subspecies.Count
		});
		add(new StatisticsAsset
		{
			id = "wars",
			localized_key = "statistics_wars",
			list_window_meta_type = MetaType.War,
			path_icon = "ui/Icons/iconWar",
			long_action = (StatisticsAsset _) => World.world.wars.countActiveWars()
		});
		add(new StatisticsAsset
		{
			id = "kingdoms",
			localized_key = "statistics_kingdoms",
			list_window_meta_type = MetaType.Kingdom,
			path_icon = "ui/Icons/iconKingdomList",
			long_action = (StatisticsAsset _) => World.world.kingdoms.Count
		});
		add(new StatisticsAsset
		{
			id = "villages",
			localized_key = "statistics_villages",
			list_window_meta_type = MetaType.City,
			path_icon = "ui/Icons/iconCitySelect",
			long_action = (StatisticsAsset _) => World.world.cities.Count
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_population_total",
			localized_key = "world_statistics_population_total",
			steam_activity = "#Status_stat_value",
			rarity = 2,
			list_window_meta_type = MetaType.Unit,
			path_icon = "ui/Icons/iconPopulation",
			long_action = (StatisticsAsset _) => World.world.units.Count
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_beasts",
			list_window_meta_type = MetaType.Unit,
			path_icon = "ui/Icons/worldrules/icon_animalspawn",
			long_action = delegate
			{
				long num = 0L;
				List<Actor> simpleList = World.world.units.getSimpleList();
				for (int i = 0; i < simpleList.Count; i++)
				{
					if (!simpleList[i].isSapient())
					{
						num++;
					}
				}
				return num;
			},
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_trees",
			path_icon = "ui/Icons/iconFertilizerTrees",
			long_action = delegate
			{
				long num = 0L;
				List<Building> simpleList = World.world.buildings.getSimpleList();
				for (int i = 0; i < simpleList.Count; i++)
				{
					if (simpleList[i].asset.building_type == BuildingType.Building_Tree)
					{
						num++;
					}
				}
				return num;
			},
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_vegetation",
			path_icon = "ui/Icons/iconFertilizerPlants",
			long_action = delegate
			{
				long num = 0L;
				List<Building> simpleList = World.world.buildings.getSimpleList();
				for (int i = 0; i < simpleList.Count; i++)
				{
					Building building = simpleList[i];
					if (building.asset.building_type == BuildingType.Building_Tree || building.asset.building_type == BuildingType.Building_Plant)
					{
						num++;
					}
				}
				return num;
			}
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_islands",
			path_icon = "ui/Icons/iconZones",
			long_action = (StatisticsAsset _) => World.world.islands_calculator.countLandIslands(),
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Everything
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_creatures_born",
			list_window_meta_type = MetaType.Unit,
			path_icon = "ui/Icons/iconBirths",
			long_action = (StatisticsAsset _) => World.world.map_stats.creaturesBorn,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_creatures_created",
			list_window_meta_type = MetaType.Unit,
			path_icon = "ui/Icons/actor_traits/iconMiracleBorn",
			long_action = (StatisticsAsset _) => World.world.map_stats.creaturesCreated,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_subspecies_created",
			localized_key_description = "statistics_subspecies".Description(),
			list_window_meta_type = MetaType.Subspecies,
			path_icon = "ui/Icons/iconSpecies",
			long_action = (StatisticsAsset _) => World.world.map_stats.subspeciesCreated,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "statistics_total_playtime",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconClock",
			string_action = (StatisticsAsset _) => Toolbox.formatTime((float)World.world.game_stats.data.gameTime)
		});
		add(new StatisticsAsset
		{
			id = "statistics_trees_grown",
			path_icon = "ui/Icons/worldrules/icon_grow_trees",
			is_game_statistics = true,
			long_action = (StatisticsAsset _) => World.world.game_stats.data.treesGrown
		});
		add(new StatisticsAsset
		{
			id = "statistics_flora_grown",
			path_icon = "ui/Icons/worldrules/icon_flora_density_high",
			is_game_statistics = true,
			long_action = (StatisticsAsset _) => World.world.game_stats.data.floraGrown
		});
		add(new StatisticsAsset
		{
			id = "statistics_meteorites_launched",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconMeteorite",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.meteoritesLaunched
		});
		add(new StatisticsAsset
		{
			id = "statistics_pixels_exploded",
			is_game_statistics = true,
			path_icon = "ui/Icons/worldrules/icon_exploding_mushrooms",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.pixelsExploded
		});
		add(new StatisticsAsset
		{
			id = "statistics_creatures_created",
			is_game_statistics = true,
			path_icon = "ui/Icons/actor_traits/iconMiracleBorn",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.creaturesCreated
		});
		add(new StatisticsAsset
		{
			id = "statistics_creatures_born",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconBirths",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.creaturesBorn
		});
		add(new StatisticsAsset
		{
			id = "statistics_creatures_died",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconDead",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.creaturesDied
		});
		add(new StatisticsAsset
		{
			id = "statistics_bombs_dropped",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconBomb",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.bombsDropped
		});
		add(new StatisticsAsset
		{
			id = "statistics_subspecies_created",
			localized_key_description = "statistics_subspecies".Description(),
			is_game_statistics = true,
			path_icon = "ui/Icons/iconSpecies",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.subspeciesCreated
		});
		add(new StatisticsAsset
		{
			id = "statistics_subspecies_extinct",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconSpeciesExtinct",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.subspeciesExtinct
		});
		add(new StatisticsAsset
		{
			id = "statistics_languages_created",
			localized_key_description = "statistics_languages".Description(),
			is_game_statistics = true,
			path_icon = "ui/Icons/iconLanguage",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.languagesCreated
		});
		add(new StatisticsAsset
		{
			id = "statistics_languages_forgotten",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconLanguageForgotten",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.languagesForgotten
		});
		add(new StatisticsAsset
		{
			id = "statistics_cultures_created",
			localized_key_description = "statistics_cultures".Description(),
			is_game_statistics = true,
			path_icon = "ui/Icons/iconCulture",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.culturesCreated
		});
		add(new StatisticsAsset
		{
			id = "statistics_cultures_forgotten",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconCultureForgotten",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.culturesForgotten
		});
		add(new StatisticsAsset
		{
			id = "statistics_families_created",
			localized_key_description = "statistics_families".Description(),
			is_game_statistics = true,
			path_icon = "ui/Icons/iconNewFamily",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.familiesCreated
		});
		add(new StatisticsAsset
		{
			id = "statistics_families_destroyed",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconFamilyDestroyed",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.familiesDestroyed
		});
		add(new StatisticsAsset
		{
			id = "statistics_clans_created",
			localized_key_description = "statistics_clans".Description(),
			is_game_statistics = true,
			path_icon = "ui/Icons/iconClan",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.clansCreated
		});
		add(new StatisticsAsset
		{
			id = "statistics_clans_destroyed",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconClanDestroyed",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.clansDestroyed
		});
		add(new StatisticsAsset
		{
			id = "statistics_books_written",
			localized_key_description = "books".Description(),
			is_game_statistics = true,
			path_icon = "ui/Icons/iconBooksWritten",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.booksWritten
		});
		add(new StatisticsAsset
		{
			id = "statistics_books_read",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconBooksRead",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.booksRead
		});
		add(new StatisticsAsset
		{
			id = "statistics_books_burnt",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconBooksDestroyed",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.booksBurnt
		});
		add(new StatisticsAsset
		{
			id = "statistics_religions_created",
			localized_key_description = "statistics_religions".Description(),
			is_game_statistics = true,
			path_icon = "ui/Icons/iconReligion",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.religionsCreated
		});
		add(new StatisticsAsset
		{
			id = "statistics_religions_forgotten",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconReligionForgotten",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.religionsForgotten
		});
		add(new StatisticsAsset
		{
			id = "statistics_kingdoms_created",
			localized_key_description = "statistics_kingdoms".Description(),
			is_game_statistics = true,
			path_icon = "ui/Icons/iconKingdom",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.kingdomsCreated
		});
		add(new StatisticsAsset
		{
			id = "statistics_kingdoms_destroyed",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconKingdomDestroyed",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.kingdomsDestroyed
		});
		add(new StatisticsAsset
		{
			id = "statistics_cities_created",
			localized_key_description = "statistics_villages".Description(),
			is_game_statistics = true,
			path_icon = "ui/Icons/iconCity",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.citiesCreated
		});
		add(new StatisticsAsset
		{
			id = "statistics_cities_destroyed",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconCityDestroyed",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.citiesDestroyed
		});
		add(new StatisticsAsset
		{
			id = "statistics_wars_started",
			localized_key_description = "statistics_wars".Description(),
			is_game_statistics = true,
			path_icon = "ui/Icons/iconWhisperOfWar",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.warsStarted
		});
		add(new StatisticsAsset
		{
			id = "statistics_peaces_made",
			is_game_statistics = true,
			path_icon = "ui/Icons/actor_traits/iconPacifist",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.peacesMade
		});
		add(new StatisticsAsset
		{
			id = "statistics_plots_started",
			localized_key_description = "statistics_plots".Description(),
			is_game_statistics = true,
			path_icon = "ui/Icons/iconPlot",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.plotsStarted
		});
		add(new StatisticsAsset
		{
			id = "statistics_plots_succeeded",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconPlotSucceeded",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.plotsSucceeded
		});
		add(new StatisticsAsset
		{
			id = "statistics_plots_forgotten",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconPlotForgotten",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.plotsForgotten
		});
		add(new StatisticsAsset
		{
			id = "statistics_creatures_sacrificed",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconVolcano",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.creaturesSacrificed
		});
		add(new StatisticsAsset
		{
			id = "statistics_elves_sacrificed",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconHateElf",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.elvesSacrificed
		});
		add(new StatisticsAsset
		{
			id = "statistics_boats_destroyed_by_magnet",
			is_game_statistics = true,
			path_icon = "ui/Icons/iconBoat",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.boatsDestroyedByMagnet
		});
	}

	private void addStatsGeneralMain()
	{
		add(new StatisticsAsset
		{
			id = "world_statistics_time",
			localized_key = "world_statistics_time",
			steam_activity = "#Status_stat_value",
			rarity = 2,
			path_icon = "ui/Icons/iconClock",
			string_action = (StatisticsAsset _) => Date.getUIStringYearMonthShort(),
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Everything
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_population",
			localized_key = "world_statistics_population",
			steam_activity = "#Status_stat_value",
			rarity = 2,
			list_window_meta_type = MetaType.Unit,
			path_icon = "ui/Icons/iconPopulationCiv",
			long_action = (StatisticsAsset _) => World.world.getCivWorldPopulation(),
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
	}

	public override void post_init()
	{
		base.post_init();
		foreach (StatisticsAsset item in list)
		{
			if (item.locale_getter != null)
			{
				item.localized_key = item.locale_getter();
			}
		}
	}

	public override void editorDiagnosticLocales()
	{
		foreach (StatisticsAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
		}
		foreach (StatisticsAsset item2 in list)
		{
			if (!item2.is_world_statistics)
			{
				continue;
			}
			string pID = item2.id.Replace("world_", "");
			if (has(pID))
			{
				StatisticsAsset statisticsAsset = get(pID);
				if (statisticsAsset.path_icon != item2.path_icon)
				{
					BaseAssetLibrary.logAssetError("<e>StatisticsLibrary</e>: World Stat <b>" + item2.path_icon + "</b> has different icon than Game Stat <b>" + statisticsAsset.path_icon + "</b>", item2.id);
				}
			}
		}
		foreach (StatisticsAsset item3 in list)
		{
			if (!item3.is_game_statistics)
			{
				continue;
			}
			string pID2 = "world_" + item3.id;
			if (has(pID2))
			{
				StatisticsAsset statisticsAsset2 = get(pID2);
				if (statisticsAsset2.path_icon != item3.path_icon)
				{
					BaseAssetLibrary.logAssetError("<e>StatisticsLibrary</e>: Game Stat <b>" + item3.path_icon + "</b> has different icon than World Stat <b>" + statisticsAsset2.path_icon + "</b>", item3.id);
				}
			}
		}
		base.editorDiagnosticLocales();
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (StatisticsAsset item in list)
		{
			for (int i = 0; i < item.rarity; i++)
			{
				power_tracker_pool.Add(item);
			}
		}
	}

	private string getDominatingMetaRow(MetaType pType)
	{
		MetaTypeAsset asset = AssetManager.meta_type_library.getAsset(pType);
		long dominatingMetaId = getDominatingMetaId(asset);
		if (!(asset.get(dominatingMetaId) is IMetaObject metaObject) || metaObject.countUnits() == 0)
		{
			return _unknown_text;
		}
		string color_text = metaObject.getColor().color_text;
		return Toolbox.coloredText(metaObject.name + Toolbox.coloredGreyPart(metaObject.countUnits(), color_text), color_text);
	}

	private long getDominatingMetaId(MetaType pType)
	{
		MetaTypeAsset asset = AssetManager.meta_type_library.getAsset(pType);
		return getDominatingMetaId(asset);
	}

	private long getDominatingMetaId(MetaTypeAsset pMetaAsset)
	{
		IMetaObject metaObject = null;
		foreach (IMetaObject item in pMetaAsset.get_list())
		{
			if (metaObject == null || item.countUnits() > metaObject.countUnits())
			{
				metaObject = item;
			}
		}
		return metaObject?.getID() ?? (-1);
	}

	private string getOldestMetaRow(MetaType pType)
	{
		MetaTypeAsset asset = AssetManager.meta_type_library.getAsset(pType);
		long oldestMetaId = getOldestMetaId(asset);
		if (!(asset.get(oldestMetaId) is IMetaObject metaObject))
		{
			return _unknown_text;
		}
		string color_text = metaObject.getColor().color_text;
		return Toolbox.coloredText(metaObject.name + Toolbox.coloredGreyPart(metaObject.getAge(), color_text), color_text);
	}

	private long getOldestMetaId(MetaType pType)
	{
		MetaTypeAsset asset = AssetManager.meta_type_library.getAsset(pType);
		return getOldestMetaId(asset);
	}

	private long getOldestMetaId(MetaTypeAsset pMetaAsset)
	{
		IMetaObject metaObject = null;
		foreach (IMetaObject item in pMetaAsset.get_list())
		{
			if (metaObject == null || item.getAge() > metaObject.getAge())
			{
				metaObject = item;
			}
		}
		return metaObject?.getID() ?? (-1);
	}

	public string addToGameplayReport(string pWhatFor)
	{
		string empty = string.Empty;
		empty = empty + pWhatFor + "\n";
		foreach (StatisticsAsset item in list)
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

	public void addStatsBiomes()
	{
		addNormalBiomes();
		addCreepBiomes();
		addSpecialBiomes();
	}

	public void addNormalBiomes()
	{
		add(new StatisticsAsset
		{
			id = "world_statistics_grass",
			locale_getter = () => getBiomeLocale("biome_grass"),
			path_icon = "ui/Icons/iconSeedGrass",
			long_action = (StatisticsAsset _) => TopTileLibrary.grass_high.hashset.Count + TopTileLibrary.grass_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_savanna",
			locale_getter = () => getBiomeLocale("biome_savanna"),
			path_icon = "ui/Icons/iconSeedSavanna",
			long_action = (StatisticsAsset _) => TopTileLibrary.savanna_high.hashset.Count + TopTileLibrary.savanna_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_jungle",
			locale_getter = () => getBiomeLocale("biome_jungle"),
			path_icon = "ui/Icons/iconSeedJungle",
			long_action = (StatisticsAsset _) => TopTileLibrary.jungle_high.hashset.Count + TopTileLibrary.jungle_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_desert",
			locale_getter = () => getBiomeLocale("biome_desert"),
			path_icon = "ui/Icons/iconSeedDesert",
			long_action = (StatisticsAsset _) => TopTileLibrary.desert_high.hashset.Count + TopTileLibrary.desert_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_lemon",
			locale_getter = () => getBiomeLocale("biome_lemon"),
			path_icon = "ui/Icons/iconSeedLemon",
			long_action = (StatisticsAsset _) => TopTileLibrary.lemon_high.hashset.Count + TopTileLibrary.lemon_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_permafrost",
			locale_getter = () => getBiomeLocale("biome_permafrost"),
			path_icon = "ui/Icons/iconSeedPermafrost",
			long_action = (StatisticsAsset _) => TopTileLibrary.permafrost_high.hashset.Count + TopTileLibrary.permafrost_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_swamp",
			locale_getter = () => getBiomeLocale("biome_swamp"),
			path_icon = "ui/Icons/iconSeedSwamp",
			long_action = (StatisticsAsset _) => TopTileLibrary.swamp_high.hashset.Count + TopTileLibrary.swamp_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_crystal",
			locale_getter = () => getBiomeLocale("biome_crystal"),
			path_icon = "ui/Icons/iconSeedCrystal",
			long_action = (StatisticsAsset _) => TopTileLibrary.crystal_high.hashset.Count + TopTileLibrary.crystal_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_enchanted",
			locale_getter = () => getBiomeLocale("biome_enchanted"),
			path_icon = "ui/Icons/iconSeedEnchanted",
			long_action = (StatisticsAsset _) => TopTileLibrary.enchanted_high.hashset.Count + TopTileLibrary.enchanted_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_corruption",
			locale_getter = () => getBiomeLocale("biome_corrupted"),
			path_icon = "ui/Icons/iconSeedCorrupted",
			long_action = (StatisticsAsset _) => TopTileLibrary.corruption_high.hashset.Count + TopTileLibrary.corruption_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_infernal",
			locale_getter = () => getBiomeLocale("biome_infernal"),
			path_icon = "ui/Icons/iconSeedInfernal",
			long_action = (StatisticsAsset _) => TopTileLibrary.infernal_high.hashset.Count + TopTileLibrary.infernal_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_candy",
			locale_getter = () => getBiomeLocale("biome_candy"),
			path_icon = "ui/Icons/iconSeedCandy",
			long_action = (StatisticsAsset _) => TopTileLibrary.candy_high.hashset.Count + TopTileLibrary.candy_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_mushroom",
			locale_getter = () => getBiomeLocale("biome_mushroom"),
			path_icon = "ui/Icons/iconSeedMushroom",
			long_action = (StatisticsAsset _) => TopTileLibrary.mushroom_high.hashset.Count + TopTileLibrary.mushroom_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_wasteland",
			locale_getter = () => getBiomeLocale("biome_wasteland"),
			path_icon = "ui/Icons/achievements/achievements_wastelandbiome",
			long_action = (StatisticsAsset _) => TopTileLibrary.wasteland_high.hashset.Count + TopTileLibrary.wasteland_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_birch",
			locale_getter = () => getBiomeLocale("biome_birch"),
			path_icon = "ui/Icons/iconSeedBirch",
			long_action = (StatisticsAsset _) => TopTileLibrary.birch_high.hashset.Count + TopTileLibrary.birch_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_maple",
			locale_getter = () => getBiomeLocale("biome_maple"),
			path_icon = "ui/Icons/iconSeedMaple",
			long_action = (StatisticsAsset _) => TopTileLibrary.maple_high.hashset.Count + TopTileLibrary.maple_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_rocklands",
			locale_getter = () => getBiomeLocale("biome_rocklands"),
			path_icon = "ui/Icons/iconSeedRocklands",
			long_action = (StatisticsAsset _) => TopTileLibrary.rocklands_high.hashset.Count + TopTileLibrary.rocklands_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_garlic",
			locale_getter = () => getBiomeLocale("biome_garlic"),
			path_icon = "ui/Icons/iconSeedGarlic",
			long_action = (StatisticsAsset _) => TopTileLibrary.garlic_high.hashset.Count + TopTileLibrary.garlic_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_flower",
			locale_getter = () => getBiomeLocale("biome_flower"),
			path_icon = "ui/Icons/iconSeedFlower",
			long_action = (StatisticsAsset _) => TopTileLibrary.flower_high.hashset.Count + TopTileLibrary.flower_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_celestial",
			locale_getter = () => getBiomeLocale("biome_celestial"),
			path_icon = "ui/Icons/iconSeedCelestial",
			long_action = (StatisticsAsset _) => TopTileLibrary.celestial_high.hashset.Count + TopTileLibrary.celestial_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_clover",
			locale_getter = () => getBiomeLocale("biome_clover"),
			path_icon = "ui/Icons/iconSeedClover",
			long_action = (StatisticsAsset _) => TopTileLibrary.clover_high.hashset.Count + TopTileLibrary.clover_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_singularity",
			locale_getter = () => getBiomeLocale("biome_singularity"),
			path_icon = "ui/Icons/iconSeedSingularity",
			long_action = (StatisticsAsset _) => TopTileLibrary.singularity_high.hashset.Count + TopTileLibrary.singularity_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_paradox",
			locale_getter = () => getBiomeLocale("biome_paradox"),
			path_icon = "ui/Icons/iconSeedParadox",
			long_action = (StatisticsAsset _) => TopTileLibrary.paradox_high.hashset.Count + TopTileLibrary.paradox_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
	}

	public void addSpecialBiomes()
	{
		add(new StatisticsAsset
		{
			id = "world_statistics_sand",
			locale_getter = () => getPowerLocale("tile_sand"),
			path_icon = "ui/Icons/iconTileSand",
			long_action = (StatisticsAsset _) => TileLibrary.sand.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
	}

	public void addCreepBiomes()
	{
		add(new StatisticsAsset
		{
			id = "world_statistics_biomass",
			locale_getter = () => getBiomeLocale("biome_biomass"),
			path_icon = "ui/Icons/iconBiomass",
			long_action = (StatisticsAsset _) => TopTileLibrary.biomass_high.hashset.Count + TopTileLibrary.biomass_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_cybertile",
			locale_getter = () => getBiomeLocale("biome_cybertile"),
			path_icon = "ui/Icons/iconCybercore",
			long_action = (StatisticsAsset _) => TopTileLibrary.cybertile_high.hashset.Count + TopTileLibrary.cybertile_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_pumpkin",
			locale_getter = () => getBiomeLocale("biome_pumpkin"),
			path_icon = "ui/Icons/iconSuperPumpkin",
			long_action = (StatisticsAsset _) => TopTileLibrary.pumpkin_high.hashset.Count + TopTileLibrary.pumpkin_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_tumor",
			locale_getter = () => getBiomeLocale("biome_tumor"),
			path_icon = "ui/Icons/iconTumor",
			long_action = (StatisticsAsset _) => TopTileLibrary.tumor_high.hashset.Count + TopTileLibrary.tumor_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Biomes
		});
	}

	public static string getBiomeLocale(string pBiomeID)
	{
		return AssetManager.biome_library.get(pBiomeID).getLocaleID();
	}

	public static string getPowerLocale(string pPowerID)
	{
		return AssetManager.powers.get(pPowerID).getLocaleID();
	}

	public void addStatsDeaths()
	{
		add(new StatisticsAsset
		{
			id = "world_statistics_deaths_total",
			localized_key = "world_statistics_deaths_total",
			steam_activity = "#Status_stat_value",
			rarity = 3,
			list_window_meta_type = MetaType.Unit,
			path_icon = "ui/Icons/iconDead",
			long_action = (StatisticsAsset _) => World.world.map_stats.deaths,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Deaths
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_deaths_natural",
			list_window_meta_type = MetaType.Unit,
			path_icon = "ui/Icons/iconClock",
			long_action = (StatisticsAsset _) => World.world.map_stats.deaths_age,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Deaths
		});
		clone("world_statistics_deaths_hunger", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_hunger;
		t.path_icon = "ui/Icons/iconDeathsHunger";
		clone("world_statistics_deaths_eaten", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_eaten;
		t.path_icon = "ui/Icons/iconDeathsEaten";
		clone("world_statistics_deaths_plague", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_plague;
		t.path_icon = "ui/Icons/actor_traits/iconPlague";
		clone("world_statistics_deaths_poison", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_poison;
		t.path_icon = "ui/Icons/iconPoisoned";
		clone("world_statistics_deaths_infection", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_infection;
		t.path_icon = "ui/Icons/actor_traits/iconInfected";
		clone("world_statistics_deaths_tumor", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_tumor;
		t.path_icon = "ui/Icons/iconTumor";
		clone("world_statistics_deaths_acid", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_acid;
		t.path_icon = "ui/Icons/iconAcid";
		clone("world_statistics_deaths_fire", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_fire;
		t.path_icon = "ui/Icons/iconFire";
		clone("world_statistics_deaths_divine", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_divine;
		t.path_icon = "ui/Icons/iconDivineLight";
		clone("world_statistics_deaths_weapon", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_weapon;
		t.path_icon = "ui/Icons/actor_traits/iconBloodlust";
		clone("world_statistics_deaths_gravity", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_gravity;
		t.path_icon = "ui/Icons/worldrules/icon_grow_trees";
		clone("world_statistics_deaths_drowning", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_drowning;
		t.path_icon = "ui/Icons/iconTileDeepOcean";
		clone("world_statistics_deaths_water", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_water;
		t.path_icon = "ui/Icons/iconRain";
		clone("world_statistics_deaths_explosion", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_explosion;
		t.path_icon = "ui/Icons/worldrules/icon_exploding_mushrooms";
		clone("world_statistics_deaths_other", "world_statistics_deaths_natural");
		t.long_action = (StatisticsAsset _) => World.world.map_stats.deaths_other;
		t.path_icon = "ui/Icons/iconDead";
		add(new StatisticsAsset
		{
			id = "world_statistics_metamorphosis",
			list_window_meta_type = MetaType.Unit,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_reproduction_metamorph",
			long_action = (StatisticsAsset _) => World.world.map_stats.metamorphosis,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_evolutions",
			list_window_meta_type = MetaType.Unit,
			path_icon = "ui/Icons/iconMonolith",
			long_action = (StatisticsAsset _) => World.world.map_stats.evolutions,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_houses_destroyed",
			list_window_meta_type = MetaType.City,
			path_icon = "ui/Icons/actor_traits/iconPyromaniac",
			long_action = (StatisticsAsset _) => World.world.map_stats.housesDestroyed,
			is_world_statistics = true,
			world_stats_tabs = (WorldStatsTabs.General | WorldStatsTabs.Deaths)
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_subspecies_extinct",
			list_window_meta_type = MetaType.Subspecies,
			path_icon = "ui/Icons/iconSpeciesExtinct",
			long_action = (StatisticsAsset _) => World.world.map_stats.subspeciesExtinct,
			is_world_statistics = true,
			world_stats_tabs = (WorldStatsTabs.Noosphere | WorldStatsTabs.Deaths)
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_kingdoms_destroyed",
			list_window_meta_type = MetaType.Kingdom,
			path_icon = "ui/Icons/iconKingdomDestroyed",
			long_action = (StatisticsAsset _) => World.world.map_stats.kingdomsDestroyed,
			is_world_statistics = true,
			world_stats_tabs = (WorldStatsTabs.Noosphere | WorldStatsTabs.Deaths)
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_armies_destroyed",
			list_window_meta_type = MetaType.Army,
			path_icon = "ui/Icons/iconArmiesDestroyed",
			long_action = (StatisticsAsset _) => World.world.map_stats.armiesDestroyed,
			is_world_statistics = true,
			world_stats_tabs = (WorldStatsTabs.Noosphere | WorldStatsTabs.Deaths)
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_cities_destroyed",
			list_window_meta_type = MetaType.City,
			path_icon = "ui/Icons/iconCityDestroyed",
			long_action = (StatisticsAsset _) => World.world.map_stats.citiesDestroyed,
			is_world_statistics = true,
			world_stats_tabs = (WorldStatsTabs.Noosphere | WorldStatsTabs.Deaths)
		});
	}

	public void addStatsNoos()
	{
		add(new StatisticsAsset
		{
			id = "world_statistics_languages_created",
			localized_key_description = "statistics_languages".Description(),
			list_window_meta_type = MetaType.Language,
			path_icon = "ui/Icons/iconLanguage",
			long_action = (StatisticsAsset _) => World.world.map_stats.languagesCreated,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_languages_forgotten",
			list_window_meta_type = MetaType.Language,
			path_icon = "ui/Icons/iconLanguageForgotten",
			long_action = (StatisticsAsset _) => World.world.map_stats.languagesForgotten,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_families_created",
			list_window_meta_type = MetaType.Family,
			path_icon = "ui/Icons/iconNewFamily",
			long_action = (StatisticsAsset _) => World.world.map_stats.familiesCreated,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_families_destroyed",
			list_window_meta_type = MetaType.Family,
			path_icon = "ui/Icons/iconFamilyDestroyed",
			long_action = (StatisticsAsset _) => World.world.map_stats.familiesDestroyed,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_clans_created",
			localized_key_description = "statistics_clans".Description(),
			list_window_meta_type = MetaType.Clan,
			path_icon = "ui/Icons/iconClan",
			long_action = (StatisticsAsset _) => World.world.map_stats.clansCreated,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_clans_destroyed",
			list_window_meta_type = MetaType.Clan,
			path_icon = "ui/Icons/iconClanDestroyed",
			long_action = (StatisticsAsset _) => World.world.map_stats.clansDestroyed,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_cultures_created",
			localized_key_description = "statistics_cultures".Description(),
			list_window_meta_type = MetaType.Culture,
			path_icon = "ui/Icons/iconCulture",
			long_action = (StatisticsAsset _) => World.world.map_stats.culturesCreated,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_cultures_forgotten",
			list_window_meta_type = MetaType.Culture,
			path_icon = "ui/Icons/iconCultureForgotten",
			long_action = (StatisticsAsset _) => World.world.map_stats.culturesForgotten,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_books_written",
			localized_key_description = "books".Description(),
			list_window_meta_type = MetaType.Language,
			path_icon = "ui/Icons/iconBooksWritten",
			long_action = (StatisticsAsset _) => World.world.map_stats.booksWritten,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_books_read",
			list_window_meta_type = MetaType.Language,
			path_icon = "ui/Icons/iconBooksRead",
			long_action = (StatisticsAsset _) => World.world.map_stats.booksRead,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_books_burnt",
			list_window_meta_type = MetaType.Language,
			path_icon = "ui/Icons/iconBooksDestroyed",
			long_action = (StatisticsAsset _) => World.world.map_stats.booksBurnt,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_religions_created",
			localized_key_description = "statistics_religions".Description(),
			list_window_meta_type = MetaType.Religion,
			path_icon = "ui/Icons/iconReligion",
			long_action = (StatisticsAsset _) => World.world.map_stats.religionsCreated,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_religions_forgotten",
			list_window_meta_type = MetaType.Religion,
			path_icon = "ui/Icons/iconReligionForgotten",
			long_action = (StatisticsAsset _) => World.world.map_stats.religionsForgotten,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_kingdoms_created",
			list_window_meta_type = MetaType.Kingdom,
			path_icon = "ui/Icons/iconKingdom",
			long_action = (StatisticsAsset _) => World.world.map_stats.kingdomsCreated,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_cities_created",
			localized_key_description = "statistics_villages".Description(),
			list_window_meta_type = MetaType.City,
			path_icon = "ui/Icons/iconCity",
			long_action = (StatisticsAsset _) => World.world.map_stats.citiesCreated,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_cities_conquered",
			list_window_meta_type = MetaType.City,
			path_icon = "ui/Icons/iconCityConquered",
			long_action = (StatisticsAsset _) => World.world.map_stats.citiesConquered,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "statistics_cities_conquered",
			path_icon = "ui/Icons/iconCityConquered",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.citiesConquered,
			is_game_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_cities_rebelled",
			list_window_meta_type = MetaType.City,
			path_icon = "ui/Icons/worldrules/icon_rebellion",
			long_action = (StatisticsAsset _) => World.world.map_stats.citiesRebelled,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "statistics_cities_rebelled",
			path_icon = "ui/Icons/worldrules/icon_rebellion",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.citiesRebelled,
			is_game_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_alliances_made",
			localized_key_description = "statistics_alliances".Description(),
			list_window_meta_type = MetaType.Alliance,
			path_icon = "ui/Icons/iconAlliance",
			long_action = (StatisticsAsset _) => World.world.map_stats.alliancesMade,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_alliances_dissolved",
			list_window_meta_type = MetaType.Alliance,
			path_icon = "ui/Icons/iconAllianceDissolved",
			long_action = (StatisticsAsset _) => World.world.map_stats.alliancesDissolved,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "statistics_alliances_made",
			path_icon = "ui/Icons/iconAlliance",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.alliancesMade,
			is_game_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "statistics_alliances_dissolved",
			path_icon = "ui/Icons/iconAllianceDissolved",
			long_action = (StatisticsAsset _) => World.world.game_stats.data.alliancesDissolved,
			is_game_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_wars_started",
			localized_key_description = "statistics_wars".Description(),
			list_window_meta_type = MetaType.War,
			path_icon = "ui/Icons/iconWhisperOfWar",
			long_action = (StatisticsAsset _) => World.world.map_stats.warsStarted,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_peaces_made",
			list_window_meta_type = MetaType.War,
			path_icon = "ui/Icons/actor_traits/iconPacifist",
			long_action = (StatisticsAsset _) => World.world.map_stats.peacesMade,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_plots_started",
			localized_key_description = "statistics_plots".Description(),
			list_window_meta_type = MetaType.Plot,
			path_icon = "ui/Icons/iconPlot",
			long_action = (StatisticsAsset _) => World.world.map_stats.plotsStarted,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_plots_succeeded",
			list_window_meta_type = MetaType.Plot,
			path_icon = "ui/Icons/iconPlotSucceeded",
			long_action = (StatisticsAsset _) => World.world.map_stats.plotsSucceeded,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_plots_forgotten",
			list_window_meta_type = MetaType.Plot,
			path_icon = "ui/Icons/iconPlotForgotten",
			long_action = (StatisticsAsset _) => World.world.map_stats.plotsForgotten,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_most_populated_village",
			path_icon = "ui/Icons/iconCity",
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere,
			world_stats_meta_type = MetaType.City,
			list_window_meta_type = MetaType.City,
			string_action = (StatisticsAsset _) => getDominatingMetaRow(MetaType.City),
			get_meta_id = (StatisticsAsset _) => getDominatingMetaId(MetaType.City)
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_biggest_village",
			path_icon = "ui/Icons/iconCity",
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere,
			world_stats_meta_type = MetaType.City,
			list_window_meta_type = MetaType.City,
			string_action = delegate(StatisticsAsset pAsset)
			{
				long pID = pAsset.get_meta_id(pAsset);
				City city = World.world.cities.get(pID);
				if (city == null)
				{
					return _unknown_text;
				}
				string color_text = city.getColor().color_text;
				return Toolbox.coloredText(city.name + Toolbox.coloredGreyPart(city.zones.Count, color_text), color_text);
			},
			get_meta_id = delegate
			{
				City city = null;
				foreach (City city2 in World.world.cities)
				{
					if (city == null || city2.zones.Count > city.zones.Count)
					{
						city = city2;
					}
				}
				return city?.id ?? (-1);
			}
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_most_populated_kingdom",
			path_icon = "ui/Icons/iconKingdom",
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere,
			world_stats_meta_type = MetaType.Kingdom,
			list_window_meta_type = MetaType.Kingdom,
			string_action = (StatisticsAsset _) => getDominatingMetaRow(MetaType.Kingdom),
			get_meta_id = (StatisticsAsset _) => getDominatingMetaId(MetaType.Kingdom)
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_dominating_culture",
			path_icon = "ui/Icons/iconCulture",
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere,
			world_stats_meta_type = MetaType.Culture,
			list_window_meta_type = MetaType.Culture,
			string_action = (StatisticsAsset _) => getDominatingMetaRow(MetaType.Culture),
			get_meta_id = (StatisticsAsset _) => getDominatingMetaId(MetaType.Culture)
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_dominating_language",
			path_icon = "ui/Icons/iconLanguage",
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere,
			world_stats_meta_type = MetaType.Language,
			list_window_meta_type = MetaType.Language,
			string_action = (StatisticsAsset _) => getDominatingMetaRow(MetaType.Language),
			get_meta_id = (StatisticsAsset _) => getDominatingMetaId(MetaType.Language)
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_dominating_religion",
			path_icon = "ui/Icons/iconReligion",
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere,
			world_stats_meta_type = MetaType.Religion,
			list_window_meta_type = MetaType.Religion,
			string_action = (StatisticsAsset _) => getDominatingMetaRow(MetaType.Religion),
			get_meta_id = (StatisticsAsset _) => getDominatingMetaId(MetaType.Religion)
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_dominating_subspecies",
			path_icon = "ui/Icons/iconSpecies",
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere,
			world_stats_meta_type = MetaType.Subspecies,
			list_window_meta_type = MetaType.Subspecies,
			string_action = (StatisticsAsset _) => getDominatingMetaRow(MetaType.Subspecies),
			get_meta_id = (StatisticsAsset _) => getDominatingMetaId(MetaType.Subspecies)
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_oldest_clan",
			path_icon = "ui/Icons/iconClan",
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.Noosphere,
			world_stats_meta_type = MetaType.Clan,
			list_window_meta_type = MetaType.Clan,
			string_action = (StatisticsAsset _) => getOldestMetaRow(MetaType.Clan),
			get_meta_id = (StatisticsAsset _) => getOldestMetaId(MetaType.Clan)
		});
	}

	public void addStatsTiles()
	{
		add(new StatisticsAsset
		{
			id = "world_statistics_water",
			localized_key = "Water",
			path_icon = "ui/Icons/iconTileDeepOcean",
			long_action = (StatisticsAsset _) => TileLibrary.deep_ocean.hashset.Count + TileLibrary.close_ocean.hashset.Count + TileLibrary.shallow_waters.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_soil",
			locale_getter = () => getPowerLocale("tile_soil"),
			path_icon = "ui/Icons/iconTileSoil",
			long_action = (StatisticsAsset _) => TileLibrary.soil_low.hashset.Count + TileLibrary.soil_high.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_summit",
			locale_getter = () => getPowerLocale("tile_summit"),
			path_icon = "ui/Icons/iconTileSummit",
			long_action = (StatisticsAsset _) => TileLibrary.summit.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_mountains",
			locale_getter = () => getPowerLocale("tile_mountains"),
			path_icon = "ui/Icons/iconTileMountains",
			long_action = (StatisticsAsset _) => TileLibrary.mountains.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_hills",
			locale_getter = () => getPowerLocale("tile_hills"),
			path_icon = "ui/Icons/iconTileHills",
			long_action = (StatisticsAsset _) => TileLibrary.hills.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_lava",
			locale_getter = () => getPowerLocale("lava"),
			path_icon = "ui/Icons/iconLava",
			long_action = (StatisticsAsset _) => TileLibrary.lava0.hashset.Count + TileLibrary.lava1.hashset.Count + TileLibrary.lava2.hashset.Count + TileLibrary.lava3.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_pit",
			localized_key = "Pit",
			path_icon = "ui/Icons/iconTileShallowWater",
			long_action = (StatisticsAsset _) => TileLibrary.pit_deep_ocean.hashset.Count + TileLibrary.pit_close_ocean.hashset.Count + TileLibrary.pit_shallow_waters.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_field",
			localized_key = "fields",
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobFarmer",
			long_action = (StatisticsAsset _) => TopTileLibrary.field.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_fireworks",
			locale_getter = () => getPowerLocale("fireworks"),
			path_icon = "ui/Icons/iconFireworks",
			long_action = (StatisticsAsset _) => TopTileLibrary.fireworks.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_frozen",
			localized_key = "Frozen",
			path_icon = "ui/Icons/iconFrozen",
			long_action = (StatisticsAsset _) => TopTileLibrary.frozen_high.hashset.Count + TopTileLibrary.frozen_low.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_fuse",
			locale_getter = () => getPowerLocale("fuse"),
			path_icon = "ui/Icons/iconFuse",
			long_action = (StatisticsAsset _) => TopTileLibrary.fuse.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_ice",
			localized_key = "Ice",
			path_icon = "ui/Icons/iconIceberg",
			long_action = (StatisticsAsset _) => TopTileLibrary.ice.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_landmine",
			locale_getter = () => getPowerLocale("landmine"),
			path_icon = "ui/Icons/iconLandmine",
			long_action = (StatisticsAsset _) => TopTileLibrary.landmine.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_road",
			localized_key = "Roads",
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobRoadBuilder",
			long_action = (StatisticsAsset _) => TopTileLibrary.road.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_snow",
			localized_key = "Snow",
			path_icon = "ui/Icons/iconSnow",
			long_action = (StatisticsAsset _) => TopTileLibrary.snow_hills.hashset.Count + TopTileLibrary.snow_block.hashset.Count + TopTileLibrary.snow_summit.hashset.Count + TopTileLibrary.snow_sand.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_tnt",
			locale_getter = () => getPowerLocale("tnt"),
			path_icon = "ui/Icons/iconTnt",
			long_action = (StatisticsAsset _) => TopTileLibrary.tnt.hashset.Count + TopTileLibrary.tnt_timed.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_wall",
			localized_key = "Walls",
			path_icon = "ui/Icons/iconWallIron",
			long_action = (StatisticsAsset _) => TopTileLibrary.wall_evil.hashset.Count + TopTileLibrary.wall_order.hashset.Count + TopTileLibrary.wall_ancient.hashset.Count + TopTileLibrary.wall_wild.hashset.Count + TopTileLibrary.wall_green.hashset.Count + TopTileLibrary.wall_iron.hashset.Count + TopTileLibrary.wall_light.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_water_bomb",
			locale_getter = () => getPowerLocale("water_bomb"),
			path_icon = "ui/Icons/iconWaterBomb",
			long_action = (StatisticsAsset _) => TopTileLibrary.water_bomb.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
		add(new StatisticsAsset
		{
			id = "world_statistics_grey_goo",
			locale_getter = () => getPowerLocale("grey_goo"),
			path_icon = "ui/Icons/iconGreygoo",
			long_action = (StatisticsAsset _) => TileLibrary.grey_goo.hashset.Count,
			is_world_statistics = true,
			world_stats_tabs = WorldStatsTabs.General
		});
	}
}
