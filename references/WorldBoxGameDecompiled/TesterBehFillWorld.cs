using System.Collections.Generic;
using ai.behaviours;

public class TesterBehFillWorld : BehaviourActionTester
{
	private static List<TileType> tiles = new List<TileType>();

	private static List<TopTileType> top_tiles = new List<TopTileType>();

	private string type;

	public TesterBehFillWorld(string pType)
	{
		type = pType;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		if (tiles.Count == 0)
		{
			foreach (TileType item in AssetManager.tiles.list)
			{
				if (item.can_be_autotested)
				{
					tiles.Add(item);
				}
			}
			foreach (TopTileType item2 in AssetManager.top_tiles.list)
			{
				if (item2.can_be_autotested)
				{
					top_tiles.Add(item2);
				}
			}
		}
		TileType tileType = null;
		TopTileType topTileType = null;
		if (type == "random")
		{
			topTileType = top_tiles.GetRandom();
			tileType = ((!topTileType.is_biome) ? tiles.GetRandom() : (Randy.randomBool() ? TileLibrary.soil_high : TileLibrary.soil_low));
		}
		else
		{
			tileType = AssetManager.tiles.get(type);
		}
		for (int i = 0; i < 3; i++)
		{
			WorldTile[] array = BehaviourActionBase<AutoTesterBot>.world.map_chunk_manager.chunks.GetRandom().tiles;
			int num = array.Length;
			for (int j = 0; j < num; j++)
			{
				WorldTile pTile = array[j];
				MapAction.terraformMain(pTile, tileType, TerraformLibrary.destroy_no_flash);
				if (topTileType != null)
				{
					MapAction.terraformTop(pTile, topTileType, TerraformLibrary.destroy_no_flash);
				}
			}
		}
		return base.execute(pObject);
	}
}
