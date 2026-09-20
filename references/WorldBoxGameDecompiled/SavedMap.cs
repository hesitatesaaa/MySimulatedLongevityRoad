using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Ionic.Zlib;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Scripting;

[Serializable]
public class SavedMap
{
	public int saveVersion;

	public int width;

	public int height;

	public HotkeyTabsData hotkey_tabs_data;

	public float camera_pos_x;

	public float camera_pos_y;

	public float camera_zoom;

	public MapStats mapStats;

	public WorldLaws worldLaws;

	public string tileString;

	public List<string> tileMap = new List<string>();

	public int[][] tileArray;

	public int[][] tileAmounts;

	public List<int> fire = new List<int>();

	public List<int> conwayEater = new List<int>();

	public List<int> conwayCreator = new List<int>();

	public List<int> frozen_tiles = new List<int>();

	public List<WorldTileData> tiles = new List<WorldTileData>();

	public List<CityData> cities = new List<CityData>();

	[Preserve]
	[Obsolete]
	public List<ActorDataObsolete> actors;

	public List<ActorData> actors_data = new List<ActorData>();

	public List<BuildingData> buildings = new List<BuildingData>();

	public List<KingdomData> kingdoms = new List<KingdomData>();

	public List<ClanData> clans = new List<ClanData>();

	public List<AllianceData> alliances = new List<AllianceData>();

	public List<WarData> wars = new List<WarData>();

	public List<PlotData> plots = new List<PlotData>();

	public List<DiplomacyRelationData> relations = new List<DiplomacyRelationData>();

	public List<CultureData> cultures = new List<CultureData>();

	public List<BookData> books = new List<BookData>();

	public List<SubspeciesData> subspecies = new List<SubspeciesData>();

	public List<LanguageData> languages = new List<LanguageData>();

	public List<ReligionData> religions = new List<ReligionData>();

	public List<FamilyData> families = new List<FamilyData>();

	public List<ArmyData> armies = new List<ArmyData>();

	public List<ItemData> items = new List<ItemData>();

	public SavedMap()
	{
		width = Config.ZONE_AMOUNT_X_DEFAULT;
		height = Config.ZONE_AMOUNT_Y_DEFAULT;
	}

	public void check()
	{
		if (worldLaws == null)
		{
			worldLaws = new WorldLaws();
		}
		if (mapStats == null)
		{
			mapStats = new MapStats();
		}
		if (hotkey_tabs_data == null)
		{
			hotkey_tabs_data = new HotkeyTabsData();
		}
		if (tileMap == null)
		{
			tileMap = new List<string>();
		}
		if (fire == null)
		{
			fire = new List<int>();
		}
		if (conwayEater == null)
		{
			conwayEater = new List<int>();
		}
		if (conwayCreator == null)
		{
			conwayCreator = new List<int>();
		}
		if (frozen_tiles == null)
		{
			frozen_tiles = new List<int>();
		}
		if (tiles == null)
		{
			tiles = new List<WorldTileData>();
		}
		if (cities == null)
		{
			cities = new List<CityData>();
		}
		if (actors_data == null)
		{
			actors_data = new List<ActorData>();
		}
		if (buildings == null)
		{
			buildings = new List<BuildingData>();
		}
		if (kingdoms == null)
		{
			kingdoms = new List<KingdomData>();
		}
		if (clans == null)
		{
			clans = new List<ClanData>();
		}
		if (alliances == null)
		{
			alliances = new List<AllianceData>();
		}
		if (wars == null)
		{
			wars = new List<WarData>();
		}
		if (plots == null)
		{
			plots = new List<PlotData>();
		}
		if (relations == null)
		{
			relations = new List<DiplomacyRelationData>();
		}
		if (cultures == null)
		{
			cultures = new List<CultureData>();
		}
		if (books == null)
		{
			books = new List<BookData>();
		}
		if (subspecies == null)
		{
			subspecies = new List<SubspeciesData>();
		}
		if (languages == null)
		{
			languages = new List<LanguageData>();
		}
		if (religions == null)
		{
			religions = new List<ReligionData>();
		}
		if (families == null)
		{
			families = new List<FamilyData>();
		}
		if (armies == null)
		{
			armies = new List<ArmyData>();
		}
		if (items == null)
		{
			items = new List<ItemData>();
		}
		worldLaws.check();
	}

	public void init()
	{
		worldLaws = new WorldLaws();
		worldLaws.init(pUpdateCaches: false);
	}

	public int getTileMapID(string pTileString)
	{
		if (!tileMap.Contains(pTileString))
		{
			tileMap.Add(pTileString);
		}
		return tileMap.IndexOf(pTileString);
	}

	public void create()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		init();
		width = Config.ZONE_AMOUNT_X;
		height = Config.ZONE_AMOUNT_Y;
		camera_pos_x = ((Component)World.world.camera).transform.position.x;
		camera_pos_y = ((Component)World.world.camera).transform.position.y;
		camera_zoom = MoveCamera.instance.getTargetZoom();
		saveVersion = Config.WORLD_SAVE_VERSION;
		hotkey_tabs_data = World.world.hotkey_tabs_data;
		mapStats = World.world.map_stats;
		worldLaws = World.world.world_laws;
		mapStats.population = World.world.units.Count;
		items = World.world.items.save();
		books = World.world.books.save();
		subspecies = World.world.subspecies.save();
		families = World.world.families.save();
		armies = World.world.armies.save();
		languages = World.world.languages.save();
		religions = World.world.religions.save();
		cultures = World.world.cultures.save();
		kingdoms = World.world.kingdoms.save();
		clans = World.world.clans.save();
		alliances = World.world.alliances.save();
		wars = World.world.wars.save();
		plots = World.world.plots.save();
		relations = World.world.diplomacy.save();
		cities = World.world.cities.save();
		if (tileMap == null)
		{
			check();
		}
		tileMap.Clear();
		fire.Clear();
		conwayEater.Clear();
		conwayCreator.Clear();
		frozen_tiles.Clear();
		using ListPool<int[]> listPool = new ListPool<int[]>();
		using ListPool<int[]> listPool2 = new ListPool<int[]>();
		string text = string.Empty;
		int num = 0;
		int num2 = 0;
		int num3 = width * 64;
		listPool.Add(new int[num3]);
		listPool2.Add(new int[num3]);
		int num4 = 0;
		for (int num5 = 0; num5 < World.world.tiles_list.Length; num5++)
		{
			WorldTile worldTile = World.world.tiles_list[num5];
			string wholeTileIDForSave = getWholeTileIDForSave(worldTile);
			Vector2Int pos;
			if (!(wholeTileIDForSave != text))
			{
				int num6 = num2;
				pos = worldTile.pos;
				if (num6 == ((Vector2Int)(ref pos)).y)
				{
					goto IL_036d;
				}
			}
			if (num > 0)
			{
				listPool2[num2][num4] = num;
				listPool[num2][num4++] = getTileMapID(text);
				num = 0;
			}
			text = wholeTileIDForSave;
			int num7 = num2;
			pos = worldTile.pos;
			if (num7 != ((Vector2Int)(ref pos)).y)
			{
				listPool[num2] = Toolbox.resizeArray(listPool[num2], num4);
				listPool2[num2] = Toolbox.resizeArray(listPool2[num2], num4);
				pos = worldTile.pos;
				num2 = ((Vector2Int)(ref pos)).y;
				listPool.Add(new int[num3]);
				listPool2.Add(new int[num3]);
				num4 = 0;
			}
			goto IL_036d;
			IL_036d:
			num++;
			if (worldTile.isOnFire())
			{
				fire.Add(worldTile.data.tile_id);
			}
			if (worldTile.data.conwayType == ConwayType.Eater)
			{
				conwayEater.Add(worldTile.data.tile_id);
			}
			if (worldTile.data.conwayType == ConwayType.Creator)
			{
				conwayCreator.Add(worldTile.data.tile_id);
			}
			if (worldTile.data.frozen)
			{
				frozen_tiles.Add(worldTile.data.tile_id);
			}
		}
		if (num > 0)
		{
			listPool2[num2][num4] = num;
			listPool[num2][num4++] = getTileMapID(text);
			listPool[num2] = Toolbox.resizeArray(listPool[num2], num4);
			listPool2[num2] = Toolbox.resizeArray(listPool2[num2], num4);
		}
		tileArray = listPool.ToArray();
		tileAmounts = listPool2.ToArray();
		foreach (Actor unit in World.world.units)
		{
			if (unit.isAlive() && !unit.asset.skip_save)
			{
				unit.prepareForSave();
				ActorData data = unit.data;
				actors_data.Add(data);
			}
		}
		foreach (Building building in World.world.buildings)
		{
			if (building.data.state != BuildingState.Removed)
			{
				building.prepareForSave();
				buildings.Add(building.data);
			}
		}
	}

	private string getWholeTileIDForSave(WorldTile pTile)
	{
		if (pTile.top_type == null)
		{
			return pTile.main_type.id;
		}
		return pTile.main_type.id + ":" + pTile.top_type.id;
	}

	public void toJson(string pFilePath)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		if (worldLaws == null)
		{
			create();
		}
		try
		{
			using FileStream stream = new FileStream(pFilePath, FileMode.Create, FileAccess.Write);
			using StreamWriter streamWriter = new StreamWriter(stream)
			{
				NewLine = "\n"
			};
			JsonWriter val = (JsonWriter)new JsonTextWriter((TextWriter)streamWriter);
			try
			{
				JsonHelper.writer.Serialize(val, (object)this);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
			throw;
		}
		Config.scheduleGC("toJson");
	}

	public string toJson(bool pBeautify = false)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (worldLaws == null)
		{
			create();
		}
		string text = "";
		try
		{
			JsonSerializerSettings val = new JsonSerializerSettings
			{
				DefaultValueHandling = (DefaultValueHandling)3,
				Formatting = (Formatting)0
			};
			if (pBeautify)
			{
				val.Formatting = (Formatting)1;
			}
			text = JsonConvert.SerializeObject((object)this, val);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
		if (string.IsNullOrEmpty(text) || text.Length < 20)
		{
			throw new Exception("Error while creating json ( empty string < 20 )");
		}
		return text;
	}

	public void toZip(string pFilePath)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		using FileStream fileStream = new FileStream(pFilePath, FileMode.Create, FileAccess.Write);
		ZlibStream val = new ZlibStream((Stream)fileStream, (CompressionMode)0, (CompressionLevel)9);
		try
		{
			using StreamWriter streamWriter = new StreamWriter((Stream)(object)val);
			JsonWriter val2 = (JsonWriter)new JsonTextWriter((TextWriter)streamWriter);
			try
			{
				JsonHelper.writer.Serialize(val2, (object)this);
				Config.scheduleGC("toZip");
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public byte[] toZip()
	{
		return ZlibStream.CompressString(toJson());
	}

	public MapMetaData getMeta()
	{
		MapMetaData mapMetaData = new MapMetaData();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		Dictionary<long, bool> dictionary = CollectionPool<Dictionary<long, bool>, KeyValuePair<long, bool>>.Get();
		if (subspecies != null)
		{
			foreach (SubspeciesData subspecy in subspecies)
			{
				if (subspecy.saved_traits.Contains("prefrontal_cortex"))
				{
					dictionary.Add(subspecy.id, value: true);
				}
			}
		}
		if (actors_data != null)
		{
			foreach (ActorData actors_datum in actors_data)
			{
				if (!AssetManager.actor_library.has(actors_datum.asset_id))
				{
					continue;
				}
				if (actors_datum.favorite)
				{
					num3++;
				}
				if (actors_datum.civ_kingdom_id != -1)
				{
					num++;
					continue;
				}
				if (dictionary.ContainsKey(actors_datum.subspecies))
				{
					num++;
					continue;
				}
				ActorAsset actorAsset = AssetManager.actor_library.get(actors_datum.asset_id);
				if (actorAsset != null)
				{
					if (actorAsset.civ)
					{
						num++;
					}
					else
					{
						num2++;
					}
				}
			}
		}
		CollectionPool<Dictionary<long, bool>, KeyValuePair<long, bool>>.Release(dictionary);
		int num4 = 0;
		int num5 = 0;
		if (items != null)
		{
			foreach (ItemData item in items)
			{
				if (AssetManager.items.has(item.asset_id))
				{
					if (item.favorite)
					{
						num5++;
					}
					num4++;
				}
			}
		}
		mapMetaData.saveVersion = saveVersion;
		mapMetaData.width = width;
		mapMetaData.height = height;
		mapMetaData.mapStats = mapStats;
		mapMetaData.cities = cities.Count;
		mapMetaData.units = actors_data?.Count ?? 0;
		mapMetaData.population = num;
		mapMetaData.mobs = num2;
		mapMetaData.deaths = World.world.map_stats.deaths;
		mapMetaData.favorites = num3;
		mapMetaData.favorite_items = num5;
		mapMetaData.equipment = num4;
		mapMetaData.books = books.Count;
		mapMetaData.wars = wars.Count;
		mapMetaData.alliances = alliances.Count;
		mapMetaData.families = families.Count;
		mapMetaData.clans = clans.Count;
		mapMetaData.cultures = cultures.Count;
		mapMetaData.religions = religions.Count;
		mapMetaData.languages = languages.Count;
		mapMetaData.subspecies = subspecies.Count;
		mapMetaData.cursed = WorldLawLibrary.world_law_cursed_world.isEnabled();
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		foreach (BuildingData building in buildings)
		{
			if (AssetManager.buildings.has(building.asset_id))
			{
				if (building.cityID.hasValue())
				{
					num6++;
				}
				if (AssetManager.buildings.get(building.asset_id).flora)
				{
					num8++;
				}
				num7++;
			}
		}
		mapMetaData.buildings = num6;
		mapMetaData.structures = num7;
		mapMetaData.kingdoms = kingdoms.Count;
		mapMetaData.vegetation = num8;
		return mapMetaData;
	}

	[OnDeserializing]
	private void OnDeserializingMethod(StreamingContext context)
	{
		LongJsonConverter.reset();
	}
}
