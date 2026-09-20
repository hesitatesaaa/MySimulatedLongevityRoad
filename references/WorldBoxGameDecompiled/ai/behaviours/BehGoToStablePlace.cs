using System.Collections.Generic;
using UnityEngine;

namespace ai.behaviours;

public class BehGoToStablePlace : BehaviourActionActor
{
	private static MapRegion best_region;

	private static int best_fast_dist = int.MaxValue;

	internal static List<KeyValuePair<int, MapRegion>> bestRegions = new List<KeyValuePair<int, MapRegion>>(4);

	internal static WorldTile best_tile = null;

	private const int MAX_DISTANCE = 15;

	public override BehResult execute(Actor pActor)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		TileIsland island = pActor.current_tile.region.island;
		if (island.isGoodIslandForActor(pActor))
		{
			return BehResult.Stop;
		}
		best_region = null;
		best_fast_dist = int.MaxValue;
		best_tile = null;
		best_region = findIslandNearby(pActor);
		if (best_region != null)
		{
			pActor.beh_tile_target = best_region.tiles.GetRandom();
			best_tile = pActor.beh_tile_target;
			return BehResult.Continue;
		}
		bestRegions.Clear();
		Vector2Int pos = pActor.current_tile.pos;
		for (int i = 0; i < BehaviourActionBase<Actor>.world.islands_calculator.islands.Count; i++)
		{
			TileIsland tileIsland = BehaviourActionBase<Actor>.world.islands_calculator.islands[i];
			if (!checkIsland(island, tileIsland, pActor))
			{
				continue;
			}
			selectBorderRegionsForComparison(tileIsland, island, out var pOut, out var pOut2);
			MapRegion mapRegion = null;
			int num = int.MaxValue;
			foreach (MapRegion item in pOut)
			{
				if (pOut2.Contains(item))
				{
					int x = ((Vector2Int)(ref pos)).x;
					int y = ((Vector2Int)(ref pos)).y;
					Vector2Int pos2 = item.tiles[0].pos;
					int x2 = ((Vector2Int)(ref pos2)).x;
					pos2 = item.tiles[0].pos;
					int num2 = Toolbox.SquaredDist(x, y, x2, ((Vector2Int)(ref pos2)).y);
					if (num2 < num)
					{
						num = num2;
						mapRegion = item;
					}
				}
			}
			if (mapRegion == null)
			{
				continue;
			}
			MapRegion mapRegion2 = mapRegion;
			List<WorldTile> edgeTiles = mapRegion2.getEdgeTiles();
			if (edgeTiles.Count == 0)
			{
				continue;
			}
			float num3 = Toolbox.DistTile(pActor.current_tile, edgeTiles.GetRandom());
			if (bestRegions.Count > 0 && (float)(bestRegions[0].Key + 15) < num3)
			{
				continue;
			}
			if (bestRegions.Count < 4)
			{
				bestRegions.Add(new KeyValuePair<int, MapRegion>((int)num3, mapRegion2));
				continue;
			}
			bestRegions.Sort((KeyValuePair<int, MapRegion> keyValuePair, KeyValuePair<int, MapRegion> keyValuePair2) => keyValuePair.Key.CompareTo(keyValuePair2.Key));
			if ((float)bestRegions[3].Key > num3)
			{
				bestRegions[3] = new KeyValuePair<int, MapRegion>((int)num3, mapRegion2);
			}
		}
		bestRegions.RemoveAll((KeyValuePair<int, MapRegion> keyValuePair) => keyValuePair.Key - 15 > bestRegions[0].Key);
		if (Randy.randomChance(0.8f) && bestRegions.Count > 0)
		{
			pActor.beh_tile_target = bestRegions.GetRandom().Value.tiles.GetRandom();
		}
		else
		{
			MapRegion mapRegion3 = ((!Randy.randomChance(0.5f)) ? Randy.getRandom(pActor.current_tile.region.neighbours) : pActor.current_tile.region);
			if (mapRegion3 != null)
			{
				pActor.beh_tile_target = Randy.getRandom(mapRegion3.tiles);
			}
		}
		if (!DebugConfig.isOn(DebugOption.ShowSwimToIslandLogic))
		{
			bestRegions.Clear();
		}
		else
		{
			best_tile = pActor.beh_tile_target;
		}
		return BehResult.Continue;
	}

	private static MapRegion findIslandNearby(Actor pActor)
	{
		(MapChunk[], int) allChunksFromTile = Toolbox.getAllChunksFromTile(pActor.current_tile);
		MapChunk[] item = allChunksFromTile.Item1;
		int item2 = allChunksFromTile.Item2;
		TileIsland island = pActor.current_tile.region.island;
		for (int i = 0; i < item2; i++)
		{
			MapChunk mapChunk = item[i];
			for (int j = 0; j < mapChunk.regions.Count; j++)
			{
				MapRegion mapRegion = mapChunk.regions[j];
				if (!checkIsland(island, mapRegion.island, pActor))
				{
					continue;
				}
				List<WorldTile> edgeTiles = mapRegion.getEdgeTiles();
				if (edgeTiles.Count != 0)
				{
					WorldTile closestTile = Toolbox.getClosestTile(edgeTiles, pActor.current_tile);
					int num = Toolbox.SquaredDistTile(pActor.current_tile, closestTile);
					if (num < best_fast_dist)
					{
						best_region = mapRegion;
						best_fast_dist = num;
					}
				}
			}
		}
		return best_region;
	}

	private static bool checkIsland(TileIsland pCurrentIsland, TileIsland pIsland, Actor pActor)
	{
		if (pCurrentIsland == pIsland)
		{
			return false;
		}
		if (!pIsland.isGoodIslandForActor(pActor))
		{
			return false;
		}
		if (!((pCurrentIsland.getTileCount() <= pIsland.getTileCount()) ? pCurrentIsland.isConnectedWith(pIsland) : pIsland.isConnectedWith(pCurrentIsland)))
		{
			return false;
		}
		if (pIsland.insideRegionEdges.Count == 0)
		{
			return false;
		}
		return true;
	}

	private static void selectBorderRegionsForComparison(TileIsland pIsland1, TileIsland pIsland2, out HashSet<MapRegion> pOut1, out HashSet<MapRegion> pOut2)
	{
		if (pIsland1.outsideRegionEdges.Count + pIsland2.insideRegionEdges.Count < pIsland1.insideRegionEdges.Count + pIsland2.outsideRegionEdges.Count)
		{
			if (pIsland1.outsideRegionEdges.Count > pIsland2.insideRegionEdges.Count)
			{
				pOut1 = pIsland2.insideRegionEdges;
				pOut2 = pIsland1.outsideRegionEdges;
			}
			else
			{
				pOut2 = pIsland2.insideRegionEdges;
				pOut1 = pIsland1.outsideRegionEdges;
			}
		}
		else if (pIsland1.insideRegionEdges.Count > pIsland2.outsideRegionEdges.Count)
		{
			pOut1 = pIsland2.outsideRegionEdges;
			pOut2 = pIsland1.insideRegionEdges;
		}
		else
		{
			pOut2 = pIsland2.outsideRegionEdges;
			pOut1 = pIsland1.insideRegionEdges;
		}
	}
}
