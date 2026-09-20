using System.IO;
using UnityEngine;

public static class TestMaps
{
	private static bool _initialized = false;

	private static string[] _maps;

	private static int _index = -1;

	public static void init()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		using ListPool<string> listPool = new ListPool<string>();
		string[] files = Directory.GetFiles("test_maps", "*.wbox", SearchOption.AllDirectories);
		listPool.AddRange(files);
		string[] files2 = Directory.GetFiles("test_maps", "*.json", SearchOption.AllDirectories);
		listPool.AddRange(files2);
		listPool.RemoveAll((string p) => p.Contains("debug"));
		_maps = listPool.ToArray();
		_index = Toolbox.loopIndex(Randy.randomInt(0, _maps.Length * 100), _maps.Length);
	}

	public static void loadMap(int pIndex)
	{
		string text = _maps[pIndex];
		Debug.Log((object)$"Loading map: {text} ({_index + 1}/{_maps.Length})");
		string directoryName = Path.GetDirectoryName(text);
		directoryName = SaveManager.folderPath(directoryName);
		World.world.save_manager.loadWorld(directoryName);
	}

	public static void loadNextMap()
	{
		init();
		_index = Toolbox.loopIndex(_index + 1, _maps.Length);
		loadMap(_index);
	}

	public static void loadPrevMap()
	{
		init();
		_index = Toolbox.loopIndex(_index - 1, _maps.Length);
		loadMap(_index);
	}
}
