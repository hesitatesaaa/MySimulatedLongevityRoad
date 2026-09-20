using System.Collections.Generic;

public class RoadsCalculator : BaseModule
{
	public List<TileIsland> islands;

	private int current_island;

	private bool dirty = true;

	internal override void create()
	{
		base.create();
		islands = new List<TileIsland>();
		hashset = new HashSet<WorldTile>();
	}

	public void setDirty(WorldTile pTile)
	{
		if (pTile.road_island != null)
		{
			pTile.road_island.dirty = true;
		}
		if (pTile.Type.road)
		{
			hashset.Add(pTile);
		}
		else
		{
			hashset.Remove(pTile);
			pTile.road_island = null;
		}
		dirty = true;
	}

	internal override void clear()
	{
		base.clear();
		hashset.Clear();
		islands.Clear();
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		if (dirty)
		{
			dirty = false;
			RecalculateIslands();
		}
	}

	private void RecalculateIslands()
	{
		int num = 0;
		while (num < islands.Count)
		{
			TileIsland tileIsland = islands[num];
			if (tileIsland.dirty)
			{
				islands.RemoveAt(num);
				foreach (WorldTile tiles_road in tileIsland.tiles_roads)
				{
					tiles_road.road_island = null;
				}
			}
			else
			{
				num++;
			}
		}
		foreach (WorldTile item in hashset)
		{
			if (item.road_island == null)
			{
				TileIsland tileIsland = new TileIsland(current_island++);
				islands.Add(tileIsland);
				CalcIsland(item, tileIsland);
			}
		}
	}

	private void CalcIsland(WorldTile pTile, TileIsland pIsland)
	{
		List<WorldTile> list = new List<WorldTile> { pTile };
		while (list.Count > 0)
		{
			WorldTile worldTile = list[0];
			list.RemoveAt(0);
			worldTile.road_island = pIsland;
			pIsland.tiles_roads.Add(worldTile);
			for (int i = 0; i < worldTile.neighboursAll.Length; i++)
			{
				WorldTile worldTile2 = worldTile.neighboursAll[i];
				if (worldTile2.road_island == null && worldTile2.Type.road)
				{
					worldTile2.road_island = pIsland;
					list.Add(worldTile2);
				}
			}
		}
	}
}
