using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Assets.SimpleZip;
using Ionic.Zlib;
using Newtonsoft.Json;
using UnityEngine;
using db;

public class SaveManager : MonoBehaviour
{
	public static int currentSlot = 0;

	public static string currentSavePath = string.Empty;

	public static WorkshopMapData currentWorkshopMapData;

	private const string saveslot_base = "saves";

	private const string workshop_base = "workshop_upload";

	private const string autosaves_base = "autosaves";

	internal const string name_main_data_old = "map.json";

	internal const string name_main_data = "map.wbox";

	internal const string name_main_data_non_zip = "map.wbax";

	internal const string name_meta_data = "map.meta";

	internal const string name_stats_data = "map_stats.s3db";

	public const string name_png_preview_main = "preview.png";

	public const string name_png_preview_small = "preview_small.png";

	private SavedMap data;

	private static string persistentDataPath;

	private string _tile_id_main;

	private string _tile_id_top;

	private static bool _loading_animation_active;

	private static JsonSerializerSettings _settings => JsonHelper.read_settings;

	private static JsonSerializer _reader => JsonHelper.reader;

	private void Start()
	{
		persistentDataPath = Application.persistentDataPath;
	}

	public static void clearCurrentSelectedWorld()
	{
		currentWorkshopMapData = null;
		currentSavePath = string.Empty;
		currentSlot = 0;
	}

	public void clickSaveSlot()
	{
		try
		{
			saveToCurrentPath();
			ScrollWindow.hideAllEvent();
		}
		catch (Exception ex)
		{
			Debug.Log((object)"Error during saving");
			Debug.LogError((object)ex);
			ScrollWindow.showWindow("error_happened");
		}
	}

	public SavedMap saveToCurrentPath()
	{
		return saveWorldToDirectory(currentSavePath);
	}

	public static SavedMap saveWorldToDirectory(string pFolder, bool pCompress = true, bool pCheckFolder = true)
	{
		pFolder = folderPath(pFolder);
		if (pCheckFolder)
		{
			if (!Directory.Exists(pFolder))
			{
				Directory.CreateDirectory(pFolder);
			}
		}
		else
		{
			Directory.CreateDirectory(pFolder);
		}
		saveImagePreview(pFolder);
		return saveMapData(pFolder, pCompress);
	}

	public static SavedMap currentWorldToSavedMap()
	{
		World.world.items.diagnostic();
		SavedMap savedMap = new SavedMap();
		savedMap.create();
		return savedMap;
	}

	public static void deleteSavePath(string pPath)
	{
		if (Directory.Exists(pPath))
		{
			Directory.Delete(pPath, recursive: true);
		}
	}

	public static void deleteCurrentSave()
	{
		deleteSavePath(currentSavePath);
		FavoriteWorld.checkFavoriteWorld();
		ScrollWindow.hideAllEvent();
	}

	public static SavedMap saveMapData(string pFolder, bool pCompress = true)
	{
		SavedMap savedMap = currentWorldToSavedMap();
		pFolder = folderPath(pFolder);
		bool flag = false;
		string text = "";
		string text2 = "";
		string text3 = "Map";
		saveMetaData(savedMap.getMeta(), pFolder);
		saveStatsIn(pFolder);
		try
		{
			if (pCompress)
			{
				text = pFolder + "map.wbox";
				text2 = text + ".tmp";
				savedMap.toZip(text2);
			}
			else
			{
				text = pFolder + "map.wbax";
				text2 = text + ".tmp";
				savedMap.toJson(text2);
			}
		}
		catch (IOException ex)
		{
			if (Toolbox.IsDiskFull(ex))
			{
				WorldTip.showNow("Error saving " + text3 + " : Disk full!", pTranslate: false, "top");
			}
			else
			{
				Debug.Log((object)("Could not save " + text3 + " due to hard drive / IO Error : "));
				Debug.Log((object)ex);
				WorldTip.showNow("Error saving " + text3 + " due to IOError! Check console for details", pTranslate: false, "top");
			}
			flag = true;
		}
		catch (Exception ex2)
		{
			Debug.Log((object)("Could not save " + text3 + " due to error : "));
			Debug.Log((object)ex2);
			WorldTip.showNow("Error saving " + text3 + "! Check console for errors", pTranslate: false, "top");
			flag = true;
		}
		if (flag)
		{
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
		}
		else
		{
			Toolbox.MoveSafely(text2, text);
		}
		return savedMap;
	}

	public static void saveMetaData(MapMetaData pMetaData, string pPath)
	{
		pMetaData.prepareForSave();
		saveMetaIn(pPath, pMetaData);
	}

	public static MapMetaData getCurrentMeta()
	{
		return getMetaFor(currentSavePath);
	}

	public static MapMetaData getMetaFor(int pSlot)
	{
		return getMetaFor(getSlotSavePath(pSlot));
	}

	public static MapMetaData getMetaFor(string pFolder)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		pFolder = folderPath(pFolder);
		if (!doesSaveExist(pFolder))
		{
			return null;
		}
		string path = generateMetaPath(pFolder);
		if (!File.Exists(path))
		{
			SavedMap mapFromPath = getMapFromPath(pFolder);
			if (mapFromPath != null)
			{
				mapFromPath.check();
				saveMetaData(mapFromPath.getMeta(), pFolder);
				mapFromPath = null;
				Config.scheduleGC("getMetaFor");
			}
		}
		if (File.Exists(path))
		{
			try
			{
				using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
				using StreamReader streamReader = new StreamReader(stream);
				JsonReader val = (JsonReader)new JsonTextReader((TextReader)streamReader);
				try
				{
					return _reader.Deserialize<MapMetaData>(val) ?? throw new Exception("Meta data was null");
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning((object)ex);
				try
				{
					File.Delete(path);
				}
				catch (Exception ex2)
				{
					Debug.Log((object)ex2);
				}
				return null;
			}
		}
		return null;
	}

	public static bool loadStatsFrom(string pFolder)
	{
		if (!doesStatsDBExist(pFolder))
		{
			return false;
		}
		return DBManager.loadDBFrom(generateStatsPath(pFolder));
	}

	public static bool doesStatsDBExist(string pFolder)
	{
		pFolder = folderPath(pFolder);
		if (!doesSaveExist(pFolder))
		{
			return false;
		}
		return File.Exists(generateStatsPath(pFolder));
	}

	public static string getMapCreationTime(string pFolder)
	{
		string savePath = getSavePath(pFolder);
		if (File.Exists(savePath))
		{
			DateTime dateTime = DateTime.UtcNow.AddDays(7.0);
			DateTime lastWriteTime = File.GetLastWriteTime(savePath);
			if (lastWriteTime.Year < 2017)
			{
				return "GREG";
			}
			if (lastWriteTime > dateTime)
			{
				return "DREDD";
			}
			CultureInfo culture = LocalizedTextManager.getCulture();
			string shortDatePattern = culture.DateTimeFormat.ShortDatePattern;
			string shortTimePattern = culture.DateTimeFormat.ShortTimePattern;
			return lastWriteTime.ToString(shortDatePattern + " " + shortTimePattern, culture);
		}
		return "??";
	}

	public static void saveMetaIn(string pFolder, MapMetaData pMetaData)
	{
		string pDataPath = generateMetaPath(pFolder);
		string pStringData = pMetaData.toJson();
		Toolbox.WriteSafely("Map Meta", pDataPath, ref pStringData);
	}

	public static void saveStatsIn(string pFolder)
	{
		DBManager.saveToPath(generateStatsPath(pFolder));
	}

	public static bool doesSaveExist(string pFolder)
	{
		if (!Directory.Exists(pFolder))
		{
			return false;
		}
		if (File.Exists(pFolder + "map.wbox"))
		{
			return true;
		}
		if (File.Exists(pFolder + "map.wbax"))
		{
			return true;
		}
		if (File.Exists(pFolder + "map.json"))
		{
			return true;
		}
		return false;
	}

	public static string getSavePath(string pFolder)
	{
		string text = folderPath(pFolder);
		if (!Directory.Exists(text))
		{
			return null;
		}
		string text2 = text + "map.wbox";
		if (File.Exists(text2))
		{
			return text2;
		}
		text2 = text + "map.wbax";
		if (File.Exists(text2))
		{
			return text2;
		}
		text2 = text + "map.json";
		if (File.Exists(text2))
		{
			return text2;
		}
		return null;
	}

	public static bool slotExists(int pSlot)
	{
		if (File.Exists(getSlotPathWbox(pSlot)))
		{
			return true;
		}
		if (File.Exists(getOldSlotPath(pSlot)))
		{
			return true;
		}
		return false;
	}

	public static bool slotMetaExists(int pSlot = -1)
	{
		if (File.Exists(getMetaSlotPath(pSlot)))
		{
			return true;
		}
		return false;
	}

	public static void copyDataToClipboard()
	{
	}

	private static void saveImagePreview(string pFolder)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		Texture2D val = PreviewHelper.convertMapToTexture();
		Texture2D val2 = new Texture2D(((Texture)val).width, ((Texture)val).height);
		Graphics.CopyTexture((Texture)(object)val, (Texture)(object)val2);
		byte[] pByteData = ImageConversion.EncodeToPNG(val);
		Toolbox.WriteSafely("PNG Preview", generatePngPreviewPath(pFolder), pByteData);
		Object.Destroy((Object)(object)val);
		pByteData = null;
		TextureScale.Point(val2, 32, 32);
		byte[] pByteData2 = ImageConversion.EncodeToPNG(val2);
		Toolbox.WriteSafely("PNG Small Preview", generatePngSmallPreviewPath(pFolder), pByteData2);
		Object.Destroy((Object)(object)val2);
		pByteData2 = null;
	}

	public static int getCurrentSlot()
	{
		return currentSlot;
	}

	public static string getSlotSavePath(int pSlot)
	{
		return generateMainPath("saves") + folderPath("save" + pSlot);
	}

	public static string generateMainPath(string pFolder)
	{
		return folderPath(persistentDataPath) + folderPath(pFolder);
	}

	public static string generateAutosavesPath(string pFolder = "")
	{
		return generateMainPath("autosaves") + folderPath(pFolder);
	}

	public static string generateWorkshopPath(string pFolder = "")
	{
		return generateMainPath("workshop_upload") + folderPath(pFolder);
	}

	public static string generatePngPreviewPath(string pFolder)
	{
		return folderPath(pFolder) + "preview.png";
	}

	public static string generatePngSmallPreviewPath(string pFolder)
	{
		return folderPath(pFolder) + "preview_small.png";
	}

	public static string generateMetaPath(string pFolder)
	{
		return folderPath(pFolder) + "map.meta";
	}

	public static string generateStatsPath(string pFolder)
	{
		return folderPath(pFolder) + "map_stats.s3db";
	}

	public static string getSlotPathWbox(int pSlot)
	{
		return getSlotSavePath(pSlot) + "map.wbox";
	}

	public static string getMetaSlotPath(int pSlot)
	{
		return getSlotSavePath(pSlot) + "map.meta";
	}

	public static string getOldSlotPath(int pSlot)
	{
		return getSlotSavePath(pSlot) + "map.json";
	}

	public static string getPngSlotPath(int pSlot = -1)
	{
		return getSlotSavePath(pSlot) + "preview.png";
	}

	public static void setCurrentSlot(int pSlotID)
	{
		currentSlot = pSlotID;
		currentSavePath = generateMainPath("saves") + folderPath("save" + pSlotID);
	}

	public static void setCurrentPath(string pPath)
	{
		currentSavePath = folderPath(pPath);
	}

	public static void setCurrentPathAndId(string pPath, int pSlotID)
	{
		currentSlot = pSlotID;
		currentSavePath = folderPath(pPath);
	}

	public static string getCurrentPreviewPath()
	{
		return currentSavePath + "preview.png";
	}

	public static bool currentSlotExists()
	{
		return doesSaveExist(currentSavePath);
	}

	public static bool currentPreviewExists()
	{
		return File.Exists(currentSavePath + "preview.png");
	}

	public static bool currentMetaExists()
	{
		return File.Exists(currentSavePath + "map.meta");
	}

	internal void loadWorld()
	{
		_loading_animation_active = false;
		prepareLoading();
		if (currentWorkshopMapData != null)
		{
			loadWorld(currentWorkshopMapData.main_path, pLoadWorkshop: true);
			currentWorkshopMapData = null;
		}
		else
		{
			loadWorld(currentSavePath);
		}
	}

	internal void loadWorld(string pPath, bool pLoadWorkshop = false)
	{
		SmoothLoader.prepare();
		try
		{
			SavedMap mapFromPath = getMapFromPath(pPath, pLoadWorkshop);
			if (mapFromPath == null)
			{
				throw new Exception("Save file not found - has it been deleted?");
			}
			Debug.Log((object)"World Loaded");
			mapFromPath.check();
			Debug.Log((object)"World Laws Loaded");
			Config.scheduleGC("load world");
			loadData(mapFromPath, pPath);
		}
		catch (Exception ex)
		{
			Debug.Log((object)("Error during loading of slot " + pPath));
			try
			{
				MapMetaData metaFor = getMetaFor(pPath);
				if (metaFor != null)
				{
					Debug.Log((object)JsonUtility.ToJson((object)metaFor));
				}
				else
				{
					Debug.Log((object)"No meta data");
				}
			}
			catch (Exception ex2)
			{
				Debug.Log((object)"Failed to load meta data");
				Debug.Log((object)ex2);
			}
			Debug.LogError((object)ex);
			ScrollWindow.showWindow("error_happened");
			MapBox.instance.startTheGame(pForceGenerate: true);
		}
	}

	public static void loadMapFromResources(string pPath)
	{
		SmoothLoader.prepare();
		Object obj = Resources.Load(pPath);
		SavedMap savedMap = JsonConvert.DeserializeObject<SavedMap>(Zip.Decompress(((TextAsset)((obj is TextAsset) ? obj : null)).bytes), _settings);
		savedMap.check();
		World.world.save_manager.loadData(savedMap);
	}

	public static void loadMapFromBytes(byte[] pMapData)
	{
		SmoothLoader.prepare();
		SavedMap savedMap = JsonConvert.DeserializeObject<SavedMap>(Zip.Decompress(pMapData), _settings);
		savedMap.check();
		World.world.save_manager.loadData(savedMap);
	}

	public static SavedMap getMapFromPath(string pMainPath, bool pLoadWorkshop = false)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		if (pLoadWorkshop)
		{
			return JsonConvert.DeserializeObject<SavedMap>(Zip.Decompress(File.ReadAllBytes(folderPath(currentWorkshopMapData.main_path) + "map.wbox")), _settings);
		}
		pMainPath = folderPath(pMainPath);
		if (!Directory.Exists(pMainPath))
		{
			Debug.Log((object)("Directory does not exist : " + pMainPath));
			return null;
		}
		string text = pMainPath + "map.wbox";
		if (File.Exists(text))
		{
			fileInfo(text);
			using FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read);
			ZlibStream val = new ZlibStream((Stream)fileStream, (CompressionMode)1);
			try
			{
				using StreamReader streamReader = new StreamReader((Stream)(object)val);
				JsonReader val2 = (JsonReader)new JsonTextReader((TextReader)streamReader);
				try
				{
					return _reader.Deserialize<SavedMap>(val2);
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
		Debug.Log((object)("Does not exist : " + text));
		text = pMainPath + "map.wbax";
		if (File.Exists(text))
		{
			fileInfo(text);
			using FileStream stream = new FileStream(text, FileMode.Open, FileAccess.Read);
			using StreamReader streamReader2 = new StreamReader(stream);
			JsonReader val3 = (JsonReader)new JsonTextReader((TextReader)streamReader2);
			try
			{
				return _reader.Deserialize<SavedMap>(val3);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		Debug.Log((object)("Does not exist : " + text));
		text = pMainPath + "map.json";
		if (File.Exists(text))
		{
			fileInfo(text);
			return JsonConvert.DeserializeObject<SavedMap>(Zip.Decompress(File.ReadAllText(text)), _settings);
		}
		Debug.Log((object)("Does not exist : " + text));
		return null;
	}

	private static void fileInfo(string full_path)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(full_path);
			Debug.Log((object)("Loading          : " + fileInfo.Name));
			Debug.Log((object)("Size             : " + fileInfo.Length));
			Debug.Log((object)("Creation time    : " + fileInfo.CreationTime));
			Debug.Log((object)("Last access time : " + fileInfo.LastAccessTime));
			Debug.Log((object)("Last write time  : " + fileInfo.LastWriteTime));
			Debug.Log((object)("Folder           : " + fileInfo.Directory?.Name));
		}
		catch (Exception ex)
		{
			Debug.Log((object)"Error when getting file info for");
			Debug.Log((object)full_path);
			Debug.Log((object)ex);
		}
	}

	public void startLoadSlot()
	{
		_loading_animation_active = true;
		prepareLoading();
		World.world.transition_screen.startTransition(loadWorld);
	}

	private void prepareLoading()
	{
		ScrollWindow.hideAllEvent();
		World.world.nameplate_manager.clearAll();
	}

	private void loadTiles(string pString)
	{
		string[] array = pString.Split(',');
		int zONE_AMOUNT_X = Config.ZONE_AMOUNT_X;
		_ = Config.ZONE_AMOUNT_Y;
		if (data.saveVersion < 7)
		{
			string[] array2 = new string[World.world.tiles_list.Length];
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < array.Length; i++)
			{
				if (num2 >= 50 * zONE_AMOUNT_X)
				{
					num2 = 0;
					num3++;
					num += 14 * zONE_AMOUNT_X;
				}
				int num4 = i + num;
				if (num4 <= array2.Length)
				{
					array2[num4] = array[i];
					num2++;
				}
			}
			array = array2;
		}
		for (int j = 0; j < World.world.tiles_list.Length; j++)
		{
			WorldTile worldTile = World.world.tiles_list[j];
			if (worldTile.data.tile_id >= array.Length || array[worldTile.data.tile_id] == null)
			{
				worldTile.setTileType("deep_ocean");
				continue;
			}
			string[] array3 = array[worldTile.data.tile_id].Split(':');
			string text = ((array3.Length == 2) ? array3[1] : null);
			_tile_id_main = array3[0];
			_tile_id_top = string.Empty;
			convertOldTilesToNewOnes();
			if (string.IsNullOrEmpty(_tile_id_top))
			{
				worldTile.setTileType(_tile_id_main);
			}
			else
			{
				worldTile.setTileTypes(_tile_id_main, AssetManager.top_tiles.get(_tile_id_top));
			}
			worldTile.Height = worldTile.Type.height_min;
			if (text != null)
			{
				if (text.Contains("fire"))
				{
					worldTile.setFireData(pVal: true);
				}
				if (text.Contains("conv0"))
				{
					worldTile.data.conwayType = ConwayType.Eater;
					World.world.conway_layer.add(worldTile, "conway");
				}
				if (text.Contains("conv1"))
				{
					worldTile.data.conwayType = ConwayType.Creator;
					World.world.conway_layer.add(worldTile, "conway_inverse");
				}
			}
		}
	}

	private void convertPermafrost()
	{
		switch (_tile_id_top)
		{
		case "snow_low":
			_tile_id_top = "permafrost_low";
			break;
		case "snow_high":
			_tile_id_top = "permafrost_high";
			break;
		case "snow_sand":
		case "ice":
		case "snow_hills":
		case "snow_block":
			_tile_id_top = string.Empty;
			break;
		}
	}

	private void convertOldTilesToNewOnes()
	{
		if (_tile_id_main.Contains("road"))
		{
			_tile_id_main = "soil_low";
			_tile_id_top = "road";
		}
		switch (_tile_id_main)
		{
		case "shallow_waters_frozen":
			_tile_id_main = "shallow_waters";
			_tile_id_top = "ice";
			break;
		case "field":
			_tile_id_main = "soil_low";
			_tile_id_top = "field";
			break;
		case "fuse":
			_tile_id_main = "soil_low";
			_tile_id_top = "fuse";
			break;
		case "grass_flowers":
		case "grass":
			_tile_id_main = "soil_low";
			_tile_id_top = "grass_low";
			break;
		case "forest_flowers":
		case "forest":
			_tile_id_main = "soil_high";
			_tile_id_top = "grass_high";
			break;
		case "forest_soil":
			_tile_id_main = "soil_high";
			break;
		case "mountains_frozen":
			_tile_id_main = "mountains";
			_tile_id_top = "snow_block";
			break;
		case "hills_frozen":
			_tile_id_main = "mountains";
			_tile_id_top = "snow_block";
			break;
		case "sand":
			_tile_id_main = "sand";
			break;
		case "forest_soil_frozen":
			_tile_id_main = "soil_high";
			_tile_id_top = "permafrost_high";
			break;
		case "soil_frozen":
			_tile_id_main = "soil_low";
			_tile_id_top = "permafrost_low";
			break;
		case "sand_frozen":
			_tile_id_main = "sand";
			_tile_id_top = "snow_sand";
			break;
		case "soil":
			_tile_id_main = "soil_low";
			break;
		case "soil_creep":
			_tile_id_main = "soil_low";
			_tile_id_top = "tumor_low";
			break;
		case "sand_creep":
			_tile_id_main = "sand";
			_tile_id_top = "tumor_low";
			break;
		default:
			_tile_id_main = "soil_low";
			break;
		case "hills":
			break;
		case "mountains":
			break;
		case "lava0":
			break;
		case "lava1":
			break;
		case "lava2":
			break;
		case "lava3":
			break;
		case "deep_ocean":
			break;
		case "close_ocean":
			break;
		case "shallow_waters":
			break;
		}
	}

	private void loadTileArray(SavedMap pData)
	{
		if (pData.tileAmounts.Length == 0)
		{
			return;
		}
		int num = pData.width * pData.height * 64 * 64;
		int num2 = 0;
		for (int i = 0; i < pData.tileArray.Length; i++)
		{
			for (int j = 0; j < pData.tileArray[i].Length; j++)
			{
				_tile_id_top = string.Empty;
				_tile_id_main = pData.tileMap[pData.tileArray[i][j]] ?? "deep_ocean";
				if (_tile_id_main.Contains(":"))
				{
					string[] array = _tile_id_main.Split(':');
					_tile_id_main = array[0];
					_tile_id_top = array[1];
				}
				if (pData.saveVersion < 9)
				{
					convertOldTilesToNewOnes();
				}
				if (pData.saveVersion < 10)
				{
					convertPermafrost();
				}
				for (int k = 0; k < pData.tileAmounts[i][j]; k++)
				{
					WorldTile worldTile = World.world.tiles_list[num2++];
					if (string.IsNullOrEmpty(_tile_id_top))
					{
						worldTile.setTileType(_tile_id_main);
					}
					else
					{
						worldTile.setTileTypes(_tile_id_main, AssetManager.top_tiles.get(_tile_id_top));
					}
					worldTile.Height = worldTile.Type.height_min;
				}
			}
		}
		while (num2 + 1 < num)
		{
			World.world.tiles_list[num2++].setTileType("deep_ocean");
		}
	}

	private void loadConway(List<int> conv0, List<int> conv1)
	{
		if (conv0.Count != 0 || conv1.Count != 0)
		{
			for (int i = 0; i < conv0.Count; i++)
			{
				World.world.tiles_list[conv0[i]].data.conwayType = ConwayType.Eater;
				World.world.conway_layer.add(World.world.tiles_list[conv0[i]], "conway");
			}
			for (int j = 0; j < conv1.Count; j++)
			{
				World.world.tiles_list[conv1[j]].data.conwayType = ConwayType.Creator;
				World.world.conway_layer.add(World.world.tiles_list[conv1[j]], "conway_inverse");
			}
		}
	}

	private void loadFrozen(List<int> pTileList)
	{
		if (pTileList.Count != 0)
		{
			for (int i = 0; i < pTileList.Count; i++)
			{
				int num = pTileList[i];
				World.world.tiles_list[num].freeze(10);
			}
		}
	}

	private void loadFire(List<int> pTileList)
	{
		if (pTileList.Count != 0)
		{
			for (int i = 0; i < pTileList.Count; i++)
			{
				int num = pTileList[i];
				World.world.tiles_list[num].setFireData(pVal: true);
			}
		}
	}

	public void loadData(SavedMap pData, string pPath = null)
	{
		data = pData;
		Debug.Log((object)("Save Version " + data.saveVersion));
		SaveConverter.convert(data);
		World.world.addClearWorld(pData.width, pData.height);
		SmoothLoader.add(delegate
		{
			ScrollWindow.hideAllEvent();
		}, "Hiding All Windows");
		SmoothLoader.add(delegate
		{
			World.world.setMapSize(pData.width, pData.height);
			World.world.hotkey_tabs_data = pData.hotkey_tabs_data;
			World.world.map_stats = pData.mapStats;
			World.world.map_stats.load();
			World.world.world_laws = pData.worldLaws;
			checkWorldLawsLoad();
			AssetManager.gene_library.regenerateBasicDNACodesWithLifeSeed(pData.mapStats.life_dna);
			World.world.era_manager.loadAge();
		}, "Setting Map Size");
		int num;
		if (!Config.disable_db)
		{
			if (!string.IsNullOrEmpty(pPath))
			{
				num = ((!doesStatsDBExist(pPath)) ? 1 : 0);
				if (num == 0)
				{
					SmoothLoader.add(delegate
					{
						if (!loadStatsFrom(pPath))
						{
							DBManager.createDB();
						}
					}, "Loading Stats DB");
					goto IL_0138;
				}
			}
			else
			{
				num = 1;
			}
			SmoothLoader.add(delegate
			{
				DBManager.createDB();
			}, "Creating Stats DB");
			goto IL_0138;
		}
		goto IL_016d;
		IL_0138:
		SmoothLoader.add(delegate
		{
			DBTables.checkTablesOK(pDropTable: true);
		}, "Checking Stats DB");
		DBTables.createOrMigrateTablesLoader((byte)num != 0);
		goto IL_016d;
		IL_016d:
		WindowPreloader.addWaitForWindowResources();
		if (pData.saveVersion < 8)
		{
			if (pData.saveVersion == 0)
			{
				SmoothLoader.add(loadAncientTiles, "LOADING ANCIENT TILES");
			}
			SmoothLoader.add(delegate
			{
				loadTiles(pData.tileString);
			}, "Loading Very Old Tiles. Like super old. Maybe you should like, re-save your world?");
		}
		else if (pData.saveVersion > 7)
		{
			SmoothLoader.add(delegate
			{
				loadTileArray(pData);
			}, "Loading Tiles");
			SmoothLoader.add(delegate
			{
				loadFrozen(pData.frozen_tiles);
			}, "Loading Frozen");
			SmoothLoader.add(delegate
			{
				loadFire(pData.fire);
			}, "Loading Fires");
			SmoothLoader.add(delegate
			{
				loadConway(pData.conwayEater, pData.conwayCreator);
			}, "Loading Conway");
		}
		SmoothLoader.add(loadSubspecies, "Loading Subspecies");
		SmoothLoader.add(loadFamilies, "Loading Families");
		SmoothLoader.add(loadLanguages, "Loading Languages");
		SmoothLoader.add(loadReligions, "Loading Religions");
		SmoothLoader.add(loadItems, "Loading Items");
		SmoothLoader.add(loadBooks, "Loading Books");
		SmoothLoader.add(loadCultures, "Loading Cultures");
		SmoothLoader.add(loadClans, "Loading Clans");
		SmoothLoader.add(loadKingdoms, "Loading Kingdoms");
		SmoothLoader.add(loadCities, "Loading Cities");
		SmoothLoader.add(loadWars, "Loading Wars");
		SmoothLoader.add(loadArmies, "Loading Armies");
		SmoothLoader.add(loadAlliances, "Loading Alliances");
		SmoothLoader.add(loadPlots, "Loading Plots");
		SmoothLoader.add(loadActors, "Finish Loading Actors");
		SmoothLoader.add(randomDecisionCooldowns, "Set random Decision Cooldowns");
		SmoothLoader.add(loadActorLovers, "Loading Lovers");
		SmoothLoader.add(loadArmyCaptain, "Loading Army Captains");
		SmoothLoader.add(loadPlotAuthors, "Loading Plot Authors");
		SmoothLoader.add(delegate
		{
			SaveConverter.checkOldCityZones(data);
		}, "Check Old City Zones");
		SmoothLoader.add(loadBuildings, "Loading Buildings");
		SmoothLoader.add(delegate
		{
			World.world.checkSimManagerLists();
		}, "Check Meta List Stuff");
		SmoothLoader.add(setHomeBuildings, "Set Home Buildings");
		SmoothLoader.add(loadCivs02, "Loading Civs");
		SmoothLoader.add(loadLeaders, "Loading Leaders");
		SmoothLoader.add(loadDiplomacy, "Loading Diplomacy");
		World.world.addUnloadResources();
		SmoothLoader.add(delegate
		{
			World.world.map_chunk_manager.allDirty();
		}, "Map Chunk Manager (1/2)");
		SmoothLoader.add(delegate
		{
			World.world.map_chunk_manager.update(0f, pForce: true);
		}, "Map Chunk Manager (2/2)");
		SmoothLoader.add(loadBoatStates, "Loading Boats. Ahoy Ahoy");
		SmoothLoader.add(delegate
		{
			World.world.cleanUpWorld();
		}, "Cleaning Up The World", pSkipFrame: true);
		SmoothLoader.add(delegate
		{
			foreach (Subspecies subspecy in World.world.subspecies)
			{
				subspecy.checkPhenotypeColor();
			}
		}, "Checking Phenotypes", pSkipFrame: true);
		SmoothLoader.add(delegate
		{
			World.world.redrawTiles();
		}, "Drawing Up The World", pSkipFrame: true);
		SmoothLoader.add(delegate
		{
			World.world.preloadRenderedSprites();
		}, "Preload rendered sprites...", pSkipFrame: false, 0.2f);
		SmoothLoader.add(delegate
		{
			World.world.finishMakingWorld();
		}, "Tidying Up The World", pSkipFrame: true);
		SmoothLoader.add(delegate
		{
			World.world.lastGC();
		}, "Rewriting The World", pSkipFrame: true);
		World.world.addLoadAutoTester();
		World.world.addKillAllUnits();
		World.world.addLoadWorldCallbacks();
		SmoothLoader.add(delegate
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			if (data.camera_pos_x != 0f && data.camera_pos_y != 0f)
			{
				((Component)World.world.camera).transform.position = Vector2.op_Implicit(new Vector2(data.camera_pos_x, data.camera_pos_y));
				MoveCamera.instance.forceZoom(data.camera_zoom);
				MoveCamera.instance.skipResetZoom();
			}
			data = null;
			World.world.finishingUpLoading();
		}, "Finishing up...", pSkipFrame: false, 0.2f);
	}

	private void checkWorldLawsLoad()
	{
		foreach (WorldLawAsset item in AssetManager.world_laws_library.list)
		{
			item.on_world_load?.Invoke();
		}
	}

	private void loadBoatStates()
	{
		foreach (Actor unit in World.world.units)
		{
			if (unit.data.transportID.hasValue())
			{
				Actor actor = World.world.units.get(unit.data.transportID);
				if (actor != null)
				{
					unit.embarkInto(actor.getSimpleComponent<Boat>());
					unit.setTask("sit_inside_boat");
					unit.ai.update();
				}
			}
		}
	}

	private void loadDiplomacy()
	{
		World.world.diplomacy.loadFromSave(data.relations);
	}

	private void loadLeaders()
	{
		foreach (City city in World.world.cities)
		{
			city.loadLeader();
		}
	}

	private void loadCivs02()
	{
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			kingdom.load2();
		}
	}

	private void setHomeBuildings()
	{
		foreach (Actor unit in World.world.units)
		{
			long homeBuildingID = unit.data.homeBuildingID;
			if (!homeBuildingID.hasValue())
			{
				continue;
			}
			Building building = World.world.buildings.get(homeBuildingID);
			if (building != null && building.isUsable())
			{
				if (unit.asset.is_boat)
				{
					building.component_docks.addBoatToDock(unit);
				}
				else if (building.component_unit_spawner != null)
				{
					building.component_unit_spawner.setUnitFromHere(unit);
				}
				else
				{
					unit.setHomeBuilding(building);
				}
			}
		}
	}

	private void loadBuildings()
	{
		World.world.buildings.loadFromSave(data.buildings);
	}

	private void loadActors()
	{
		World.world.units.loadFromSave(data.actors_data);
		data.actors_data.Clear();
	}

	private void randomDecisionCooldowns()
	{
		foreach (Actor unit in World.world.units)
		{
			unit.setupRandomDecisionCooldowns();
		}
	}

	private void loadPlotAuthors()
	{
		foreach (Plot plot in World.world.plots)
		{
			plot.loadAuthors();
		}
	}

	private void loadActorLovers()
	{
		foreach (Actor unit in World.world.units)
		{
			if (unit.data.lover.hasValue())
			{
				Actor actor = World.world.units.get(unit.data.lover);
				if (actor != null)
				{
					unit.setLover(actor);
				}
			}
		}
	}

	private void loadActorsOld(int startIndex = 0, int pAmount = 0)
	{
	}

	private void loadCities()
	{
		if (data.cities != null)
		{
			World.world.cities.loadFromSave(data.cities);
		}
	}

	private void loadWars()
	{
		if (data.wars != null)
		{
			World.world.wars.loadFromSave(data.wars);
		}
	}

	private void loadPlots()
	{
		if (data.plots != null)
		{
			World.world.plots.loadFromSave(data.plots);
		}
	}

	private void loadAlliances()
	{
		if (data.alliances != null)
		{
			World.world.alliances.loadFromSave(data.alliances);
		}
	}

	private void loadClans()
	{
		if (data.clans != null)
		{
			World.world.clans.loadFromSave(data.clans);
		}
	}

	private void loadKingdoms()
	{
		if (data.kingdoms != null)
		{
			World.world.kingdoms.loadFromSave(data.kingdoms);
		}
	}

	private void loadCultures()
	{
		if (data.cultures != null)
		{
			World.world.cultures.loadFromSave(data.cultures);
		}
	}

	private void loadBooks()
	{
		if (data.books != null)
		{
			World.world.books.loadFromSave(data.books);
		}
	}

	private void loadSubspecies()
	{
		if (data.subspecies != null)
		{
			World.world.subspecies.loadFromSave(data.subspecies);
		}
	}

	private void loadLanguages()
	{
		if (data.languages != null)
		{
			World.world.languages.loadFromSave(data.languages);
		}
	}

	private void loadReligions()
	{
		if (data.religions != null)
		{
			World.world.religions.loadFromSave(data.religions);
		}
	}

	private void loadItems()
	{
		if (data.items != null)
		{
			World.world.items.loadFromSave(data.items);
		}
	}

	private void loadFamilies()
	{
		if (data.families != null)
		{
			World.world.families.loadFromSave(data.families);
		}
	}

	private void loadArmies()
	{
		if (data.armies != null)
		{
			World.world.armies.loadFromSave(data.armies);
		}
	}

	private void loadArmyCaptain()
	{
		foreach (Army army in World.world.armies)
		{
			army.loadDataCaptains();
		}
	}

	private void loadAncientTiles()
	{
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		for (int i = 0; i < data.tiles.Count; i++)
		{
			if (i > 0)
			{
				stringBuilderPool.Append(",");
			}
			WorldTileData worldTileData = data.tiles[i];
			stringBuilderPool.Append(worldTileData.type);
		}
		data.tileString = stringBuilderPool.ToString();
		data.tiles = new List<WorldTileData>();
	}

	public static string folderPath(string pFolder)
	{
		if (string.IsNullOrEmpty(pFolder))
		{
			return string.Empty;
		}
		string text = Path.DirectorySeparatorChar.ToString();
		string value = Path.AltDirectorySeparatorChar.ToString();
		if (!pFolder.EndsWith(text) && !pFolder.EndsWith(value))
		{
			pFolder += text;
		}
		return pFolder;
	}

	public static bool isLoadingSaveAnimationActive()
	{
		return _loading_animation_active;
	}
}
