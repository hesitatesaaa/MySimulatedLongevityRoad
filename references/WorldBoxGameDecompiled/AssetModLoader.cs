using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class AssetModLoader
{
	private static string path_log;

	public static void load()
	{
		path_log = Application.streamingAssetsPath + "/mod_loading_logs.log";
		File.WriteAllText(path_log, "");
		string text = Application.streamingAssetsPath + "/mods/";
		List<string> directories = getDirectories(text);
		log("# HELLO");
		log("# GOTTA LOAD MODS FAST");
		log("# LOADING MODS NOW");
		log("########");
		log("");
		log("# MAIN PATH: " + text);
		log("# TOTAL MODS: " + directories.Count);
		log("");
		for (int i = 0; i < directories.Count; i++)
		{
			string text2 = directories[i];
			log("---------START------------------------------------------------------------------------------------");
			log("## LOADING MOD N " + (i + 1));
			log(text2);
			loadMod(text2);
			log("---------FINISH-----------------------------------------------------------------------------------");
			log("");
			log("");
		}
	}

	private static void loadMod(string pPath)
	{
		string text = pPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[^1];
		log("# CHECKING MOD... " + text);
		foreach (string directory in getDirectories(pPath))
		{
			checkModAssets(directory);
		}
	}

	private static void checkModAssets(string pPath)
	{
		List<string> directories = getDirectories(pPath);
		string[] array = pPath.Split(Path.DirectorySeparatorChar);
		log("");
		string text = array[^1];
		log("## CHECKING MOD FOLDER... " + text);
		log("## SUB FOLDERS FOUND: " + directories.Count);
		log("");
		foreach (string item in directories)
		{
			checkModFolder(item, text);
		}
	}

	private static void checkModFolder(string pPath, string pType)
	{
		List<string> files = getFiles(pPath);
		string[] array = pPath.Split(Path.DirectorySeparatorChar);
		log("");
		log("# CHECKING PATH... " + array[^1]);
		log("FILES: " + files.Count);
		log("");
		foreach (string item in files)
		{
			log(item);
			if (item.Contains("json"))
			{
				loadFileJson(item, pType);
			}
			if (item.Contains("png"))
			{
				loadTexture(item);
			}
		}
	}

	private static void loadTexture(string pPath)
	{
		string text = pPath.Split(Path.DirectorySeparatorChar)[^1];
		log("# LOAD TEXTURE: " + text);
		byte[] pBytes = File.ReadAllBytes(pPath);
		string text2 = "@wb_" + text;
		log("ADDING TEXTURE... " + text2);
		SpriteTextureLoader.addSprite(text2, pBytes);
	}

	private static void loadFileJson(string pPath, string pType)
	{
		string text = pPath.Split(Path.DirectorySeparatorChar)[^1];
		log("# LOAD ASSET: " + text);
		string pData = File.ReadAllText(pPath);
		switch (pType)
		{
		default:
			_ = pType == "traits";
			break;
		case "buildings":
			loadAssetBuilding(pData);
			break;
		case "powers":
			loadAssetPowers(pData);
			break;
		case "kingdoms":
			break;
		}
	}

	private static void loadAssetActor(string pData)
	{
		ActorAsset pAsset = JsonUtility.FromJson<ActorAsset>(pData);
		AssetManager.actor_library.add(pAsset);
	}

	private static void loadAssetBuilding(string pData)
	{
		BuildingAsset pAsset = JsonUtility.FromJson<BuildingAsset>(pData);
		AssetManager.buildings.add(pAsset);
	}

	private static void loadAssetKingdom(string pData)
	{
		KingdomAsset pAsset = JsonUtility.FromJson<KingdomAsset>(pData);
		AssetManager.kingdoms.add(pAsset);
	}

	private static void loadAssetPowers(string pData)
	{
		GodPower pAsset = JsonUtility.FromJson<GodPower>(pData);
		AssetManager.powers.add(pAsset);
	}

	private static void loadAssetTraits(string pData)
	{
		ActorTrait pAsset = JsonUtility.FromJson<ActorTrait>(pData);
		AssetManager.traits.add(pAsset);
	}

	private static void log(string pLog)
	{
		File.AppendAllText(path_log, pLog + "\n");
	}

	private static List<string> getDirectories(string pPath)
	{
		List<string> list = new List<string>();
		string[] directories = Directory.GetDirectories(pPath);
		foreach (string text in directories)
		{
			if (!text.Contains(".meta"))
			{
				list.Add(text);
			}
		}
		return list;
	}

	private static List<string> getFiles(string pPath)
	{
		List<string> list = new List<string>();
		string[] files = Directory.GetFiles(pPath);
		foreach (string text in files)
		{
			if (!text.Contains(".meta"))
			{
				list.Add(text);
			}
		}
		return list;
	}
}
