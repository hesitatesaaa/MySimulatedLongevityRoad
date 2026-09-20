using System.Collections.Generic;

namespace ai.behaviours;

public class BehFingerFindTarget : BehFinger
{
	private const float RANDOM_CHANCE_ADD_TILE = 0.65f;

	private const float RANDOM_CHANCE_FIND_PITS = 0.95f;

	private const float RANDOM_CHANCE_FIND_WATER = 0.95f;

	private const float RANDOM_CHANCE_FIND_GROUND = 0.95f;

	private const float RANDOM_CHANCE_USE_CURRENT_ISLAND = 0.6f;

	public override BehResult execute(Actor pActor)
	{
		pActor.findCurrentTile(pCheckNeighbours: false);
		clearTargets(finger);
		if (finger.target_tiles.Count == 0)
		{
			finger.finger_target = fillRandomTiles(pActor.current_tile, finger.target_tiles);
		}
		pActor.beh_tile_target = finger.target_tiles.GetRandom();
		return BehResult.Continue;
	}

	private FingerTarget fillRandomTiles(WorldTile pTile, HashSet<WorldTile> pTargetTiles)
	{
		float num = BehaviourActionBase<Actor>.world.islands_calculator.groundIslandRatio() * 4f;
		int num2 = TileLibrary.pit_deep_ocean.hashset.Count + TileLibrary.pit_close_ocean.hashset.Count + TileLibrary.pit_shallow_waters.hashset.Count;
		if (num2 > 20 && Randy.randomChance(0.95f))
		{
			using (ListPool<WorldTile> listPool = new ListPool<WorldTile>(num2))
			{
				listPool.AddRange(TileLibrary.pit_deep_ocean.hashset);
				listPool.AddRange(TileLibrary.pit_close_ocean.hashset);
				listPool.AddRange(TileLibrary.pit_shallow_waters.hashset);
				Toolbox.sortTilesByDistance(pTile, listPool);
				listPool.Clear(10);
				WorldTile random = listPool.GetRandom();
				(MapChunk[], int) allChunksFromTile = Toolbox.getAllChunksFromTile(random);
				MapChunk[] item = allChunksFromTile.Item1;
				int item2 = allChunksFromTile.Item2;
				bool flag = false;
				for (int i = 0; i < item2; i++)
				{
					WorldTile[] tiles = item[i].tiles;
					foreach (WorldTile worldTile in tiles)
					{
						if (worldTile.Type.IsType(random.Type) && Randy.randomChance(0.65f))
						{
							pTargetTiles.Add(worldTile);
							if (pTargetTiles.Count >= 1200)
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
				return FingerTarget.Water;
			}
		}
		if (BehaviourActionBase<Actor>.world.islands_calculator.hasNonGround() && Randy.randomChance(0.95f * num))
		{
			TileIsland tileIsland;
			if (pTile.region.island.type == TileLayerType.Ocean && Randy.randomChance(0.6f))
			{
				tileIsland = pTile.region.island;
			}
			else
			{
				tileIsland = BehaviourActionBase<Actor>.world.islands_calculator.getRandomIslandNonGroundWeighted();
				if (tileIsland == null)
				{
					tileIsland = BehaviourActionBase<Actor>.world.islands_calculator.getRandomIslandNonGround(pMinRegions: false);
				}
				pTile = tileIsland.getRandomTile();
			}
			foreach (MapRegion item3 in tileIsland.regions.getSimpleList().LoopRandom())
			{
				if (pTile.region != item3 && !pTile.region.hasNeighbour(item3))
				{
					continue;
				}
				foreach (WorldTile item4 in item3.tiles.LoopRandom())
				{
					if (Randy.randomChance(0.65f))
					{
						pTargetTiles.Add(item4);
					}
				}
				if (pTargetTiles.Count >= 1200)
				{
					break;
				}
			}
			return FingerTarget.Water;
		}
		if (BehaviourActionBase<Actor>.world.islands_calculator.hasGround() && Randy.randomChance(0.95f))
		{
			TileIsland tileIsland;
			if (pTile.region.island.type == TileLayerType.Ground && Randy.randomChance(0.6f))
			{
				tileIsland = pTile.region.island;
			}
			else
			{
				tileIsland = BehaviourActionBase<Actor>.world.islands_calculator.getRandomIslandGroundWeighted();
				if (tileIsland == null)
				{
					tileIsland = BehaviourActionBase<Actor>.world.islands_calculator.getRandomIslandGround(pMinRegions: false);
				}
				pTile = tileIsland.getRandomTile();
			}
			foreach (MapRegion item5 in tileIsland.regions.getSimpleList().LoopRandom())
			{
				if (pTile.region != item5 && !pTile.region.hasNeighbour(item5))
				{
					continue;
				}
				foreach (WorldTile item6 in item5.tiles.LoopRandom())
				{
					if (Randy.randomChance(0.65f))
					{
						pTargetTiles.Add(item6);
					}
				}
				if (pTargetTiles.Count >= 1200)
				{
					break;
				}
			}
			return FingerTarget.Ground;
		}
		WorldTile randomTileWithinDistance = Toolbox.getRandomTileWithinDistance(pTile, 75);
		foreach (MapRegion item7 in randomTileWithinDistance.region.island.regions.getSimpleList().LoopRandom())
		{
			if (randomTileWithinDistance.region != item7 && !randomTileWithinDistance.region.hasNeighbour(item7))
			{
				continue;
			}
			foreach (WorldTile item8 in item7.tiles.LoopRandom())
			{
				if (Randy.randomChance(0.65f))
				{
					pTargetTiles.Add(item8);
				}
			}
			if (pTargetTiles.Count >= 1200)
			{
				break;
			}
		}
		return getFingerTarget(randomTileWithinDistance);
	}

	private static FingerTarget getFingerTarget(WorldTile pTile)
	{
		if (pTile.Type.layer_type == TileLayerType.Ocean || pTile.Type.can_be_filled_with_ocean)
		{
			return FingerTarget.Water;
		}
		return FingerTarget.Ground;
	}

	private static void clearTargets(GodFinger pFinger)
	{
		if (pFinger.finger_target == FingerTarget.None)
		{
			return;
		}
		if (pFinger.drawing_over_water)
		{
			pFinger.target_tiles.RemoveWhere((WorldTile x) => x.Type.layer_type != TileLayerType.Ocean && !x.Type.can_be_filled_with_ocean);
		}
		if (pFinger.drawing_over_ground)
		{
			pFinger.target_tiles.RemoveWhere((WorldTile x) => x.Type.layer_type != TileLayerType.Ground);
		}
	}
}
