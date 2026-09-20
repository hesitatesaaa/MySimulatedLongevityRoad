using Beebyte.Obfuscator;

[Skip]
public class MapSizeLibrary : AssetLibrary<MapSizeAsset>
{
	public const string tiny = "tiny";

	public const string small = "small";

	public const string standard = "standard";

	public const string large = "large";

	public const string huge = "huge";

	public const string gigantic = "gigantic";

	public const string titanic = "titanic";

	public const string iceberg = "iceberg";

	private static string[] _mapSizes;

	public static string[] getSizes()
	{
		return _mapSizes;
	}

	public override void init()
	{
		base.init();
		add(new MapSizeAsset
		{
			id = "tiny",
			size = 2,
			path_icon = "actor_traits/iconTiny"
		});
		add(new MapSizeAsset
		{
			id = "small",
			size = 3,
			path_icon = "iconAntBlack"
		});
		add(new MapSizeAsset
		{
			id = "standard",
			size = 4,
			path_icon = "iconTileSand"
		});
		add(new MapSizeAsset
		{
			id = "large",
			size = 5,
			path_icon = "iconTileSoil",
			show_warning = true
		});
		add(new MapSizeAsset
		{
			id = "huge",
			size = 6,
			path_icon = "iconTileHighSoil",
			show_warning = true
		});
		add(new MapSizeAsset
		{
			id = "gigantic",
			size = 7,
			path_icon = "iconTileMountains",
			show_warning = true
		});
		add(new MapSizeAsset
		{
			id = "titanic",
			size = 8,
			path_icon = "iconTitanic",
			show_warning = true
		});
		add(new MapSizeAsset
		{
			id = "iceberg",
			size = 9,
			path_icon = "iconIceberg",
			show_warning = true
		});
	}

	public override void linkAssets()
	{
		base.linkAssets();
		convertToOldFormat();
	}

	private void convertToOldFormat()
	{
		_mapSizes = new string[list.Count];
		for (int i = 0; i < _mapSizes.Length; i++)
		{
			_mapSizes[i] = list[i].id;
		}
	}

	public static int getSize(string pId)
	{
		return AssetManager.map_sizes.get(pId)?.size ?? 2;
	}

	public static MapSizeAsset getPresetAsset(int pMapSize)
	{
		foreach (MapSizeAsset item in AssetManager.map_sizes.list)
		{
			if (item.size == pMapSize)
			{
				return item;
			}
		}
		return null;
	}

	public static string getPreset(int pMapSize)
	{
		foreach (MapSizeAsset item in AssetManager.map_sizes.list)
		{
			if (item.size == pMapSize)
			{
				return item.id;
			}
		}
		return null;
	}

	public static bool isSizeValid(int pMapSize)
	{
		if (getPresetAsset(pMapSize) == null)
		{
			return false;
		}
		if (pMapSize > getSize(Config.maxMapSize))
		{
			return false;
		}
		return true;
	}

	public override void editorDiagnosticLocales()
	{
		foreach (MapSizeAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
		}
		base.editorDiagnosticLocales();
	}
}
