using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace tools.debug;

public class DebugMap
{
	public static void makeDebugMap()
	{
		createDebugButtons();
		WorldTile[] tiles_list = World.world.tiles_list;
		for (int i = 0; i < tiles_list.Length; i++)
		{
			MapAction.terraformTile(tiles_list[i], TileLibrary.soil_low, TopTileLibrary.grass_low, TerraformLibrary.destroy);
		}
		int num = 10;
		int num2 = 10;
		int num3 = 0;
		int count = AssetManager.buildings.list.Count;
		while (num3 < count)
		{
			BuildingAsset buildingAsset = AssetManager.buildings.list[num3];
			if (buildingAsset.id.Contains("!"))
			{
				num3++;
				continue;
			}
			num3++;
			num += 20;
			if (num > 200)
			{
				num = 10;
				num2 += 10;
			}
			Building building = World.world.buildings.addBuilding(buildingAsset, World.world.GetTile(num, num2));
			building.kingdom = World.world.kingdoms_wild.get("nature");
			building.updateBuild(10000);
			if (!building.asset.docks)
			{
				continue;
			}
			foreach (WorldTile tile in building.tiles)
			{
				MapAction.terraformMain(tile, TileLibrary.shallow_waters, TerraformLibrary.flash);
			}
		}
		Config.paused = true;
	}

	private static void debugConstructionZone()
	{
		foreach (Building building in World.world.buildings)
		{
			building.debugConstructions();
		}
	}

	private static void debugNextFrame()
	{
	}

	private static void debugRuins()
	{
	}

	public static void createDebugButtons()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		Button obj = makeNewButton("debug_next_frame", "iconBuildings");
		((UnityEvent)obj.onClick).AddListener(new UnityAction(debugNextFrame));
		((Component)obj).GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -20f);
		Button obj2 = makeNewButton("debug_ruins", "iconDemolish");
		((UnityEvent)obj2.onClick).AddListener(new UnityAction(debugRuins));
		((Component)obj2).GetComponent<RectTransform>().anchoredPosition = new Vector2(100f, -20f);
		Button obj3 = makeNewButton("debug_construction", "iconBucket");
		((UnityEvent)obj3.onClick).AddListener(new UnityAction(debugConstructionZone));
		((Component)obj3).GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, -20f);
	}

	private static Button makeNewButton(string pName, string pIcon)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		Button obj = Object.Instantiate<Button>((Button)Resources.Load("ui/PrefabWorldBoxButton", typeof(Button)), ((Component)World.world.canvas).transform);
		((Object)((Component)obj).transform).name = pName;
		((Component)obj).transform.parent = ((Component)World.world.canvas).transform;
		Sprite sprite = (Sprite)Resources.Load("ui/Icons/" + pIcon, typeof(Sprite));
		((Component)((Component)obj).transform.Find("Icon")).GetComponent<Image>().sprite = sprite;
		return obj;
	}
}
