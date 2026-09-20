using System;
using System.Collections.Generic;
using UnityEngine;

public static class SaveConverter
{
	private static Dictionary<string, string[]> _buildings_old_id_dictionary;

	private static long _kingdom_0;

	public static void convert(SavedMap pData)
	{
		if (pData.saveVersion == 15)
		{
			throw new Exception("saveVersion 15 is not supported");
		}
		if (pData.saveVersion < 12)
		{
			convertOldAges(pData);
		}
		if (pData.saveVersion < 15)
		{
			checkOldBuildingID(pData);
		}
		if (pData.saveVersion <= 15)
		{
			convertTo15(pData);
		}
		if (pData.saveVersion <= 16)
		{
			convertTo16(pData);
		}
		if (pData.saveVersion <= 17)
		{
			convertTo17(pData);
		}
	}

	public static long kingdomIDFixer(SavedMap pData, long pKingdomID)
	{
		if (pKingdomID != 0L)
		{
			return pKingdomID;
		}
		if (_kingdom_0 == 0L)
		{
			_kingdom_0 = pData.mapStats.id_kingdom++;
		}
		Debug.LogWarning((object)("found kingdom with id 0, changing to " + _kingdom_0));
		return _kingdom_0;
	}

	public static string assetIDFixer(string pAssetID)
	{
		if (pAssetID.StartsWith("unit_"))
		{
			pAssetID = pAssetID.Replace("unit_", "");
		}
		if (pAssetID.StartsWith("baby_"))
		{
			pAssetID = pAssetID.Replace("baby_", "");
		}
		if (pAssetID == "chick")
		{
			pAssetID = "chicken";
		}
		if (pAssetID == "skeleton_cursed")
		{
			pAssetID = "skeleton";
		}
		if (pAssetID == "whiteMage")
		{
			pAssetID = "white_mage";
		}
		if (pAssetID == "evilMage")
		{
			pAssetID = "evil_mage";
		}
		if (pAssetID == "godFinger")
		{
			pAssetID = "god_finger";
		}
		if (pAssetID == "livingPlants")
		{
			pAssetID = "living_plants";
		}
		if (pAssetID == "livingHouse")
		{
			pAssetID = "living_house";
		}
		if (pAssetID == "walker")
		{
			pAssetID = "cold_one";
		}
		if (pAssetID == "lemon_man")
		{
			pAssetID = "lemon_snail";
		}
		if (pAssetID == "lemon_boi")
		{
			pAssetID = "lemon_snail";
		}
		if (pAssetID == "enchanted_fairy")
		{
			pAssetID = "fairy";
		}
		if (pAssetID == "crystal_golem")
		{
			pAssetID = "crystal_sword";
		}
		return pAssetID;
	}

	public static void checkMaxValues(SavedMap pData)
	{
		if (pData.mapStats == null)
		{
			return;
		}
		if (pData.mapStats.id_unit <= 1 && pData.actors_data != null)
		{
			bool flag = false;
			foreach (ActorData actors_datum in pData.actors_data)
			{
				if (actors_datum.id >= pData.mapStats.id_unit)
				{
					pData.mapStats.id_unit = actors_datum.id + 1;
					flag = true;
				}
			}
			if (flag)
			{
				Debug.LogWarning((object)("increased id_unit to " + pData.mapStats.id_unit));
			}
		}
		if (pData.mapStats.id_building <= 1 && pData.buildings != null)
		{
			bool flag2 = false;
			foreach (BuildingData building in pData.buildings)
			{
				if (building.id >= pData.mapStats.id_building)
				{
					pData.mapStats.id_building = building.id + 1;
					flag2 = true;
				}
			}
			if (flag2)
			{
				Debug.LogWarning((object)("increased id_building to " + pData.mapStats.id_building));
			}
		}
		if (pData.mapStats.id_kingdom <= 1 && pData.kingdoms != null)
		{
			bool flag3 = false;
			foreach (KingdomData kingdom in pData.kingdoms)
			{
				if (kingdom.id >= pData.mapStats.id_kingdom)
				{
					pData.mapStats.id_kingdom = kingdom.id + 1;
					flag3 = true;
				}
			}
			if (flag3)
			{
				Debug.LogWarning((object)("increased id_kingdom to " + pData.mapStats.id_kingdom));
			}
		}
		if (pData.mapStats.id_city <= 1 && pData.cities != null)
		{
			bool flag4 = false;
			foreach (CityData city in pData.cities)
			{
				if (city.id >= pData.mapStats.id_city)
				{
					pData.mapStats.id_city = city.id + 1;
					flag4 = true;
				}
			}
			if (flag4)
			{
				Debug.LogWarning((object)("increased id_city to " + pData.mapStats.id_city));
			}
		}
		if (pData.mapStats.id_culture <= 1 && pData.cultures != null)
		{
			bool flag5 = false;
			foreach (CultureData culture in pData.cultures)
			{
				if (culture.id >= pData.mapStats.id_culture)
				{
					pData.mapStats.id_culture = culture.id + 1;
					flag5 = true;
				}
			}
			if (flag5)
			{
				Debug.LogWarning((object)("increased id_culture to " + pData.mapStats.id_culture));
			}
		}
		if (pData.mapStats.id_clan <= 1 && pData.clans != null)
		{
			bool flag6 = false;
			foreach (ClanData clan in pData.clans)
			{
				if (clan.id >= pData.mapStats.id_clan)
				{
					pData.mapStats.id_clan = clan.id + 1;
					flag6 = true;
				}
			}
			if (flag6)
			{
				Debug.LogWarning((object)("increased id_clan to " + pData.mapStats.id_clan));
			}
		}
		if (pData.mapStats.id_alliance <= 1 && pData.alliances != null)
		{
			bool flag7 = false;
			foreach (AllianceData alliance in pData.alliances)
			{
				if (alliance.id >= pData.mapStats.id_alliance)
				{
					pData.mapStats.id_alliance = alliance.id + 1;
					flag7 = true;
				}
			}
			if (flag7)
			{
				Debug.LogWarning((object)("increased id_alliance to " + pData.mapStats.id_alliance));
			}
		}
		if (pData.mapStats.id_war <= 1 && pData.wars != null)
		{
			bool flag8 = false;
			foreach (WarData war in pData.wars)
			{
				if (war.id >= pData.mapStats.id_war)
				{
					pData.mapStats.id_war = war.id + 1;
					flag8 = true;
				}
			}
			if (flag8)
			{
				Debug.LogWarning((object)("increased id_war to " + pData.mapStats.id_war));
			}
		}
		if (pData.mapStats.id_plot <= 1 && pData.plots != null)
		{
			bool flag9 = false;
			foreach (PlotData plot in pData.plots)
			{
				if (plot.id >= pData.mapStats.id_plot)
				{
					pData.mapStats.id_plot = plot.id + 1;
					flag9 = true;
				}
			}
			if (flag9)
			{
				Debug.LogWarning((object)("increased id_plot to " + pData.mapStats.id_plot));
			}
		}
		if (pData.mapStats.id_book <= 1 && pData.books != null)
		{
			bool flag10 = false;
			foreach (BookData book in pData.books)
			{
				if (book.id >= pData.mapStats.id_book)
				{
					pData.mapStats.id_book = book.id + 1;
					flag10 = true;
				}
			}
			if (flag10)
			{
				Debug.LogWarning((object)("increased id_book to " + pData.mapStats.id_book));
			}
		}
		if (pData.mapStats.id_subspecies <= 1 && pData.subspecies != null)
		{
			bool flag11 = false;
			foreach (SubspeciesData subspecy in pData.subspecies)
			{
				if (subspecy.id >= pData.mapStats.id_subspecies)
				{
					pData.mapStats.id_subspecies = subspecy.id + 1;
					flag11 = true;
				}
			}
			if (flag11)
			{
				Debug.LogWarning((object)("increased id_subspecies to " + pData.mapStats.id_subspecies));
			}
		}
		if (pData.mapStats.id_family <= 1 && pData.families != null)
		{
			bool flag12 = false;
			foreach (FamilyData family in pData.families)
			{
				if (family.id >= pData.mapStats.id_family)
				{
					pData.mapStats.id_family = family.id + 1;
					flag12 = true;
				}
			}
			if (flag12)
			{
				Debug.LogWarning((object)("increased id_family to " + pData.mapStats.id_family));
			}
		}
		if (pData.mapStats.id_army <= 1 && pData.armies != null)
		{
			bool flag13 = false;
			foreach (ArmyData army in pData.armies)
			{
				if (army.id >= pData.mapStats.id_army)
				{
					pData.mapStats.id_army = army.id + 1;
					flag13 = true;
				}
			}
			if (flag13)
			{
				Debug.LogWarning((object)("increased id_army to " + pData.mapStats.id_army));
			}
		}
		if (pData.mapStats.id_language <= 1 && pData.languages != null)
		{
			bool flag14 = false;
			foreach (LanguageData language in pData.languages)
			{
				if (language.id >= pData.mapStats.id_language)
				{
					pData.mapStats.id_language = language.id + 1;
					flag14 = true;
				}
			}
			if (flag14)
			{
				Debug.LogWarning((object)("increased id_language to " + pData.mapStats.id_language));
			}
		}
		if (pData.mapStats.id_religion <= 1 && pData.religions != null)
		{
			bool flag15 = false;
			foreach (ReligionData religion in pData.religions)
			{
				if (religion.id >= pData.mapStats.id_religion)
				{
					pData.mapStats.id_religion = religion.id + 1;
					flag15 = true;
				}
			}
			if (flag15)
			{
				Debug.LogWarning((object)("increased id_religion to " + pData.mapStats.id_religion));
			}
		}
		if (pData.mapStats.id_item <= 1 && pData.items != null)
		{
			bool flag16 = false;
			foreach (ItemData item in pData.items)
			{
				if (item.id >= pData.mapStats.id_item)
				{
					pData.mapStats.id_item = item.id + 1;
					flag16 = true;
				}
			}
			if (flag16)
			{
				Debug.LogWarning((object)("increased id_item to " + pData.mapStats.id_item));
			}
		}
		if (pData.mapStats.id_diplomacy > 1 || pData.relations == null)
		{
			return;
		}
		long num = pData.mapStats.id_diplomacy;
		foreach (DiplomacyRelationData relation in pData.relations)
		{
			if (relation.id < 100000000 && relation.id >= num)
			{
				num = relation.id + 1;
			}
		}
		foreach (DiplomacyRelationData relation2 in pData.relations)
		{
			if (relation2.id >= 100000000)
			{
				relation2.id = num++;
			}
		}
		pData.mapStats.id_diplomacy = num;
	}

	public static void convertTo17(SavedMap pData)
	{
		if (pData.subspecies == null)
		{
			return;
		}
		foreach (SubspeciesData subspecy in pData.subspecies)
		{
			for (int i = 0; i < subspecy.saved_traits.Count; i++)
			{
				if (subspecy.saved_traits[i] == "water_creature")
				{
					subspecy.saved_traits[i] = "aquatic";
				}
				if (subspecy.saved_traits[i] == "aquatic_adaptation")
				{
					subspecy.saved_traits[i] = "fins";
				}
			}
		}
	}

	public static void convertTo16(SavedMap pData)
	{
		if (pData.buildings != null)
		{
			foreach (BuildingData building in pData.buildings)
			{
				building.asset_id = building.asset_id.Replace("mapple_plant", "maple_plant");
				building.asset_id = building.asset_id.Replace("mapple_tree", "maple_tree");
			}
		}
		if (pData.tileMap != null)
		{
			for (int i = 0; i < pData.tileMap.Count; i++)
			{
				if (pData.tileMap[i].Contains("mapple_"))
				{
					pData.tileMap[i] = pData.tileMap[i].Replace("mapple_", "maple_");
				}
			}
		}
		if (pData.subspecies == null)
		{
			return;
		}
		foreach (SubspeciesData subspecy in pData.subspecies)
		{
			if (subspecy.biome_variant.Contains("biome_mapple"))
			{
				subspecy.biome_variant = subspecy.biome_variant.Replace("biome_mapple", "biome_maple");
			}
		}
	}

	public static void convertTo15(SavedMap pData)
	{
		_kingdom_0 = 0L;
		checkMaxValues(pData);
		if (pData.kingdoms != null)
		{
			foreach (KingdomData kingdom in pData.kingdoms)
			{
				kingdom.id = kingdomIDFixer(pData, kingdom.id);
				kingdom.original_actor_asset = assetIDFixer(kingdom.original_actor_asset);
			}
		}
		if (pData.actors_data != null)
		{
			foreach (ActorData actors_datum in pData.actors_data)
			{
				actors_datum.asset_id = assetIDFixer(actors_datum.asset_id);
				actors_datum.civ_kingdom_id = kingdomIDFixer(pData, actors_datum.civ_kingdom_id);
				if (actors_datum.profession == UnitProfession.Baby)
				{
					actors_datum.profession = UnitProfession.Nothing;
				}
				if (actors_datum.saved_traits == null)
				{
					continue;
				}
				for (int i = 0; i < actors_datum.saved_traits.Count; i++)
				{
					if (actors_datum.saved_traits[i] == "mushSpores")
					{
						actors_datum.saved_traits[i] = "mush_spores";
					}
					if (actors_datum.saved_traits[i] == "tumorInfection")
					{
						actors_datum.saved_traits[i] = "tumor_infection";
					}
				}
			}
		}
		if (pData.cities != null)
		{
			foreach (CityData city in pData.cities)
			{
				city.kingdomID = kingdomIDFixer(pData, city.kingdomID);
				city.original_actor_asset = assetIDFixer(city.original_actor_asset);
			}
		}
		if (pData.books != null)
		{
			foreach (BookData book in pData.books)
			{
				book.author_kingdom_id = kingdomIDFixer(pData, book.author_kingdom_id);
			}
		}
		if (pData.religions != null)
		{
			foreach (ReligionData religion in pData.religions)
			{
				religion.creator_kingdom_id = kingdomIDFixer(pData, religion.creator_kingdom_id);
			}
		}
		if (pData.alliances != null)
		{
			foreach (AllianceData alliance in pData.alliances)
			{
				alliance.founder_kingdom_id = kingdomIDFixer(pData, alliance.founder_kingdom_id);
				List<long> kingdoms = alliance.kingdoms;
				if (kingdoms != null && kingdoms.Contains(0L))
				{
					alliance.kingdoms[alliance.kingdoms.IndexOf(0L)] = kingdomIDFixer(pData, 0L);
				}
			}
		}
		if (pData.wars != null)
		{
			foreach (WarData war in pData.wars)
			{
				war.started_by_kingdom_id = kingdomIDFixer(pData, war.started_by_kingdom_id);
				war.main_attacker = kingdomIDFixer(pData, war.main_attacker);
				war.main_defender = kingdomIDFixer(pData, war.main_defender);
				List<long> list_attackers = war.list_attackers;
				if (list_attackers != null && list_attackers.Contains(0L))
				{
					war.list_attackers[war.list_attackers.IndexOf(0L)] = kingdomIDFixer(pData, 0L);
				}
				List<long> list_defenders = war.list_defenders;
				if (list_defenders != null && list_defenders.Contains(0L))
				{
					war.list_defenders[war.list_defenders.IndexOf(0L)] = kingdomIDFixer(pData, 0L);
				}
			}
		}
		if (pData.clans != null)
		{
			foreach (ClanData clan in pData.clans)
			{
				clan.founder_kingdom_id = kingdomIDFixer(pData, clan.founder_kingdom_id);
				clan.original_actor_asset = assetIDFixer(clan.original_actor_asset);
			}
		}
		if (pData.cultures != null)
		{
			foreach (CultureData culture in pData.cultures)
			{
				culture.creator_kingdom_id = kingdomIDFixer(pData, culture.creator_kingdom_id);
				culture.original_actor_asset = assetIDFixer(culture.original_actor_asset);
			}
		}
		if (pData.families != null)
		{
			foreach (FamilyData family in pData.families)
			{
				family.species_id = assetIDFixer(family.species_id);
				family.founder_kingdom_id = kingdomIDFixer(pData, family.founder_kingdom_id);
			}
		}
		if (pData.subspecies != null)
		{
			foreach (SubspeciesData subspecy in pData.subspecies)
			{
				subspecy.species_id = assetIDFixer(subspecy.species_id);
			}
		}
		if (pData.plots != null)
		{
			foreach (PlotData plot in pData.plots)
			{
				if (plot.plot_type_id == "stop_war")
				{
					plot.plot_type_id = "attacker_stop_war";
				}
			}
		}
		if (pData.relations == null)
		{
			return;
		}
		foreach (DiplomacyRelationData relation in pData.relations)
		{
			relation.kingdom1_id = kingdomIDFixer(pData, relation.kingdom1_id);
			relation.kingdom2_id = kingdomIDFixer(pData, relation.kingdom2_id);
		}
	}

	public static void convertOldAges(SavedMap data)
	{
		if (data.actors != null)
		{
			foreach (ActorDataObsolete actor in data.actors)
			{
				ActorData newActorData = getNewActorData(actor);
				if (newActorData != null)
				{
					data.actors_data.Add(newActorData);
				}
			}
			foreach (ActorData actors_datum in data.actors_data)
			{
				if (data.saveVersion < 11 && actors_datum.created_time < 0.0)
				{
					actors_datum.created_time = data.mapStats.world_time + actors_datum.created_time + (double)Randy.randomFloat(0f, 360f);
				}
			}
			data.actors = null;
		}
		if (data.kingdoms != null)
		{
			foreach (KingdomData kingdom in data.kingdoms)
			{
				if (kingdom.created_time < 0.0)
				{
					kingdom.created_time = data.mapStats.world_time + kingdom.created_time + (double)Randy.randomFloat(0f, 360f);
				}
			}
		}
		if (data.cities != null)
		{
			foreach (CityData city in data.cities)
			{
				if (city.created_time < 0.0)
				{
					city.created_time = data.mapStats.world_time + city.created_time + (double)Randy.randomFloat(0f, 360f);
				}
			}
		}
		if (data.cultures == null)
		{
			return;
		}
		foreach (CultureData culture in data.cultures)
		{
			if (culture.created_time == 0.0 && culture.year_obsolete > 0)
			{
				culture.created_time = data.mapStats.world_time - (double)((float)culture.year_obsolete * 60f) + (double)Randy.randomFloat(0f, 360f);
			}
		}
	}

	public static void checkOldCityZones(SavedMap pData)
	{
		if (pData.saveVersion >= 7)
		{
			return;
		}
		for (int i = 0; i < pData.buildings.Count; i++)
		{
			BuildingData buildingData = pData.buildings[i];
			City city = World.world.cities.get(buildingData.cityID);
			if (city != null)
			{
				WorldTile tile = World.world.GetTile(buildingData.mainX, buildingData.mainY);
				city.addZone(tile.zone);
			}
		}
	}

	public static void checkOldBuildingID(SavedMap pData)
	{
		if (_buildings_old_id_dictionary == null)
		{
			_buildings_old_id_dictionary = new Dictionary<string, string[]>();
			_buildings_old_id_dictionary.Add("geyserAcid", new string[1] { "geyser_acid" });
			_buildings_old_id_dictionary.Add("tree", new string[3] { "tree_green_1", "tree_green_2", "tree_green_3" });
			_buildings_old_id_dictionary.Add("mushroom", new string[1] { "mushroom_red" });
			_buildings_old_id_dictionary.Add("savanna_tree", new string[2] { "savanna_tree_1", "savanna_tree_2" });
			_buildings_old_id_dictionary.Add("savanna_tree_big", new string[2] { "savanna_tree_big_1", "savanna_tree_big_2" });
			_buildings_old_id_dictionary.Add("cacti", new string[1] { "cacti_tree" });
			_buildings_old_id_dictionary.Add("iron", new string[1] { "mineral_metals" });
			_buildings_old_id_dictionary.Add("iron_m", new string[1] { "mineral_metals" });
			_buildings_old_id_dictionary.Add("iron_s", new string[1] { "mineral_metals" });
			_buildings_old_id_dictionary.Add("gold", new string[1] { "mineral_gold" });
			_buildings_old_id_dictionary.Add("gold_m", new string[1] { "mineral_gold" });
			_buildings_old_id_dictionary.Add("gold_s", new string[1] { "mineral_gold" });
			_buildings_old_id_dictionary.Add("ore_deposit", new string[1] { "mineral_metals" });
			_buildings_old_id_dictionary.Add("ore_deposit_m", new string[1] { "mineral_metals" });
			_buildings_old_id_dictionary.Add("ore_deposit_s", new string[1] { "mineral_metals" });
			_buildings_old_id_dictionary.Add("palm", new string[1] { "palm_tree" });
			_buildings_old_id_dictionary.Add("pine", new string[1] { "pine_tree" });
			_buildings_old_id_dictionary.Add("stone", new string[1] { "mineral_stone" });
			_buildings_old_id_dictionary.Add("stone_m", new string[1] { "mineral_stone" });
			_buildings_old_id_dictionary.Add("stone_s", new string[1] { "mineral_stone" });
			_buildings_old_id_dictionary.Add("ruins_small", new string[1] { "poop" });
			_buildings_old_id_dictionary.Add("ruins_medium", new string[1] { "poop" });
			_buildings_old_id_dictionary.Add("house_human", new string[1] { "house_human_0" });
			_buildings_old_id_dictionary.Add("1house_human", new string[1] { "house_human_1" });
			_buildings_old_id_dictionary.Add("2house_human", new string[1] { "house_human_2" });
			_buildings_old_id_dictionary.Add("3house_human", new string[1] { "house_human_3" });
			_buildings_old_id_dictionary.Add("4house_human", new string[1] { "house_human_4" });
			_buildings_old_id_dictionary.Add("5house_human", new string[1] { "house_human_5" });
			_buildings_old_id_dictionary.Add("hall_human", new string[1] { "hall_human_0" });
			_buildings_old_id_dictionary.Add("1hall_human", new string[1] { "hall_human_1" });
			_buildings_old_id_dictionary.Add("2hall_human", new string[1] { "hall_human_2" });
			_buildings_old_id_dictionary.Add("windmill_human", new string[1] { "windmill_human_0" });
			_buildings_old_id_dictionary.Add("1windmill_human", new string[1] { "windmill_human_1" });
			_buildings_old_id_dictionary.Add("house_elf", new string[1] { "house_elf_0" });
			_buildings_old_id_dictionary.Add("1house_elf", new string[1] { "house_elf_1" });
			_buildings_old_id_dictionary.Add("2house_elf", new string[1] { "house_elf_2" });
			_buildings_old_id_dictionary.Add("3house_elf", new string[1] { "house_elf_3" });
			_buildings_old_id_dictionary.Add("4house_elf", new string[1] { "house_elf_4" });
			_buildings_old_id_dictionary.Add("5house_elf", new string[1] { "house_elf_5" });
			_buildings_old_id_dictionary.Add("hall_elf", new string[1] { "hall_elf_0" });
			_buildings_old_id_dictionary.Add("1hall_elf", new string[1] { "hall_elf_1" });
			_buildings_old_id_dictionary.Add("2hall_elf", new string[1] { "hall_elf_2" });
			_buildings_old_id_dictionary.Add("windmill_elf", new string[1] { "windmill_elf_0" });
			_buildings_old_id_dictionary.Add("1windmill_elf", new string[1] { "windmill_elf_1" });
			_buildings_old_id_dictionary.Add("house_orc", new string[1] { "house_orc_0" });
			_buildings_old_id_dictionary.Add("1house_orc", new string[1] { "house_orc_1" });
			_buildings_old_id_dictionary.Add("2house_orc", new string[1] { "house_orc_2" });
			_buildings_old_id_dictionary.Add("3house_orc", new string[1] { "house_orc_3" });
			_buildings_old_id_dictionary.Add("4house_orc", new string[1] { "house_orc_4" });
			_buildings_old_id_dictionary.Add("5house_orc", new string[1] { "house_orc_5" });
			_buildings_old_id_dictionary.Add("hall_orc", new string[1] { "hall_orc_0" });
			_buildings_old_id_dictionary.Add("1hall_orc", new string[1] { "hall_orc_1" });
			_buildings_old_id_dictionary.Add("2hall_orc", new string[1] { "hall_orc_2" });
			_buildings_old_id_dictionary.Add("windmill_orc", new string[1] { "windmill_orc_0" });
			_buildings_old_id_dictionary.Add("1windmill_orc", new string[1] { "windmill_orc_1" });
			_buildings_old_id_dictionary.Add("house_dwarf", new string[1] { "house_dwarf_0" });
			_buildings_old_id_dictionary.Add("1house_dwarf", new string[1] { "house_dwarf_1" });
			_buildings_old_id_dictionary.Add("2house_dwarf", new string[1] { "house_dwarf_2" });
			_buildings_old_id_dictionary.Add("3house_dwarf", new string[1] { "house_dwarf_3" });
			_buildings_old_id_dictionary.Add("4house_dwarf", new string[1] { "house_dwarf_4" });
			_buildings_old_id_dictionary.Add("5house_dwarf", new string[1] { "house_dwarf_5" });
			_buildings_old_id_dictionary.Add("hall_dwarf", new string[1] { "hall_dwarf_0" });
			_buildings_old_id_dictionary.Add("1hall_dwarf", new string[1] { "hall_dwarf_1" });
			_buildings_old_id_dictionary.Add("2hall_dwarf", new string[1] { "hall_dwarf_2" });
			_buildings_old_id_dictionary.Add("windmill_dwarf", new string[1] { "windmill_dwarf_0" });
			_buildings_old_id_dictionary.Add("1windmill_dwarf", new string[1] { "windmill_dwarf_1" });
			_buildings_old_id_dictionary.Add("0wheat", new string[1] { "wheat" });
			_buildings_old_id_dictionary.Add("1wheat", new string[1] { "wheat" });
			_buildings_old_id_dictionary.Add("2wheat", new string[1] { "wheat" });
			_buildings_old_id_dictionary.Add("3wheat", new string[1] { "wheat" });
			_buildings_old_id_dictionary.Add("4wheat", new string[1] { "wheat" });
			_buildings_old_id_dictionary.Add("wheat_0", new string[1] { "wheat" });
			_buildings_old_id_dictionary.Add("wheat_1", new string[1] { "wheat" });
			_buildings_old_id_dictionary.Add("wheat_2", new string[1] { "wheat" });
			_buildings_old_id_dictionary.Add("wheat_3", new string[1] { "wheat" });
			_buildings_old_id_dictionary.Add("wheat_4", new string[1] { "wheat" });
			_buildings_old_id_dictionary.Add("goldenBrain", new string[1] { "golden_brain" });
			_buildings_old_id_dictionary.Add("corruptedBrain", new string[1] { "corrupted_brain" });
			_buildings_old_id_dictionary.Add("flameTower", new string[1] { "flame_tower" });
			_buildings_old_id_dictionary.Add("iceTower", new string[1] { "ice_tower" });
			_buildings_old_id_dictionary.Add("superPumpkin", new string[1] { "super_pumpkin" });
		}
		if (pData.buildings == null)
		{
			return;
		}
		foreach (BuildingData building in pData.buildings)
		{
			if (_buildings_old_id_dictionary.ContainsKey(building.asset_id))
			{
				building.asset_id = _buildings_old_id_dictionary[building.asset_id].GetRandom();
			}
			if (building.state == BuildingState.None)
			{
				building.state = BuildingState.Normal;
			}
			if (building.state == BuildingState.CivKingdom)
			{
				building.state = BuildingState.Normal;
			}
			if (building.state == BuildingState.CivAbandoned)
			{
				building.state = BuildingState.Normal;
			}
		}
	}

	private static ActorData getNewActorData(ActorDataObsolete pOldData)
	{
		ActorData status = pOldData.status;
		if (string.IsNullOrEmpty(status.asset_id))
		{
			Debug.Log((object)"skipping unit because it's missing an asset_id");
			return null;
		}
		status.x = pOldData.x;
		status.y = pOldData.y;
		status.cityID = pOldData.cityID;
		List<long> saved_items = pOldData.saved_items;
		if (saved_items != null && saved_items.Count > 0)
		{
			status.saved_items = pOldData.saved_items;
		}
		status.inventory = pOldData.inventory;
		if (status.inventory.isEmpty())
		{
			status.inventory.empty();
		}
		return status;
	}
}
