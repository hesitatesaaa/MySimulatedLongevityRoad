using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ai.behaviours;

public static class Finder
{
	private static readonly List<BaseSimObject> _list_objects = new List<BaseSimObject>(4096);

	private static MapChunk[] _chunks;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<Building> getBuildingsFromChunk(WorldTile pTile, int pChunkRadius, int pTileRadius = 0, bool pRandom = false)
	{
		int num = pTile.chunk.x - pChunkRadius;
		int num2 = pTile.chunk.y - pChunkRadius;
		int num3 = pChunkRadius * 2 + 1;
		int num4 = pChunkRadius * 2 + 1;
		int num5 = num3 * num4;
		MapChunk[] array = (_chunks = Toolbox.checkArraySize(_chunks, num5));
		MapChunkManager map_chunk_manager = World.world.map_chunk_manager;
		int tTileRadius = pTileRadius * pTileRadius;
		int num6 = 0;
		for (int i = 0; i < num3; i++)
		{
			for (int j = 0; j < num4; j++)
			{
				MapChunk mapChunk = map_chunk_manager.get(num + i, num2 + j);
				if (mapChunk == null)
				{
					num5--;
				}
				else
				{
					array[num6++] = mapChunk;
				}
			}
		}
		if (pRandom)
		{
			foreach (MapChunk item in array.LoopRandom(num5))
			{
				if (item == null)
				{
					continue;
				}
				List<Building> buildings_all = item.objects.buildings_all;
				foreach (Building item2 in buildings_all.LoopRandom())
				{
					if (item2.isAlive() && (tTileRadius == 0 || Toolbox.SquaredDistTile(item2.current_tile, pTile) <= tTileRadius))
					{
						yield return item2;
					}
				}
			}
			yield break;
		}
		foreach (MapChunk item3 in array.LoopRandom(num5))
		{
			if (item3 == null)
			{
				continue;
			}
			List<Building> tBuildings = item3.objects.buildings_all;
			int k = 0;
			for (int tLen = tBuildings.Count; k < tLen; k++)
			{
				Building building = tBuildings[k];
				if (building.isAlive() && (tTileRadius == 0 || Toolbox.SquaredDistTile(building.current_tile, pTile) <= tTileRadius))
				{
					yield return building;
				}
			}
		}
	}

	public static bool isEnemyNearOnSameIsland(Actor pActor, int pChunkRadius = 1)
	{
		foreach (Actor item in getUnitsFromChunk(pActor.current_tile, pChunkRadius))
		{
			if (pActor.isOnSameIsland(item) && item.kingdom.isEnemy(pActor.kingdom))
			{
				return true;
			}
		}
		return false;
	}

	public static bool isEnemyNearOnSameIslandAndCarnivore(Actor pActor, int pChunkRadius = 1)
	{
		foreach (Actor item in getUnitsFromChunk(pActor.current_tile, pChunkRadius))
		{
			if (pActor.isOnSameIsland(item))
			{
				if (item.isCarnivore())
				{
					return true;
				}
				if (item.kingdom.isEnemy(pActor.kingdom))
				{
					return true;
				}
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<Actor> getUnitsFromChunk(WorldTile pTile, int pChunkRadius, float pTileRadius = 0f, bool pRandom = false)
	{
		int num = pTile.chunk.x - pChunkRadius;
		int num2 = pTile.chunk.y - pChunkRadius;
		int num3 = pChunkRadius * 2 + 1;
		int num4 = pChunkRadius * 2 + 1;
		int num5 = num3 * num4;
		MapChunk[] array = (_chunks = Toolbox.checkArraySize(_chunks, num5));
		MapChunkManager map_chunk_manager = World.world.map_chunk_manager;
		float tTileRadius = pTileRadius * pTileRadius;
		int num6 = 0;
		for (int i = 0; i < num3; i++)
		{
			for (int j = 0; j < num4; j++)
			{
				MapChunk mapChunk = map_chunk_manager.get(num + i, num2 + j);
				if (mapChunk == null)
				{
					num5--;
				}
				else
				{
					array[num6++] = mapChunk;
				}
			}
		}
		if (pRandom)
		{
			foreach (MapChunk item in array.LoopRandom(num5))
			{
				if (item == null)
				{
					continue;
				}
				List<Actor> units_all = item.objects.units_all;
				foreach (Actor item2 in units_all.LoopRandom())
				{
					if (item2.isAlive() && (tTileRadius == 0f || !((float)Toolbox.SquaredDistTile(item2.current_tile, pTile) > tTileRadius)))
					{
						yield return item2;
					}
				}
			}
			yield break;
		}
		foreach (MapChunk item3 in array.LoopRandom(num5))
		{
			if (item3 == null)
			{
				continue;
			}
			List<Actor> tUnits = item3.objects.units_all;
			int k = 0;
			for (int tLen = tUnits.Count; k < tLen; k++)
			{
				Actor actor = tUnits[k];
				if (actor.isAlive() && (tTileRadius == 0f || !((float)Toolbox.SquaredDistTile(actor.current_tile, pTile) > tTileRadius)))
				{
					yield return actor;
				}
			}
		}
	}

	public static List<BaseSimObject> getAllObjectsInChunks(WorldTile pTile, int pTileRadius = 3)
	{
		List<BaseSimObject> list_objects = _list_objects;
		list_objects.Clear();
		fillAllObjectsFromChunk(pTile.chunk, pTile, pTileRadius, list_objects);
		MapChunk[] neighbours = pTile.chunk.neighbours;
		for (int i = 0; i < neighbours.Length; i++)
		{
			fillAllObjectsFromChunk(neighbours[i], pTile, pTileRadius, list_objects);
		}
		return list_objects;
	}

	private static void fillAllObjectsFromChunk(MapChunk pChunk, WorldTile pTile, int pTileRadius, List<BaseSimObject> pListObjects)
	{
		int num = pTileRadius * pTileRadius;
		List<long> kingdoms = pChunk.objects.kingdoms;
		for (int i = 0; i < kingdoms.Count; i++)
		{
			long pKingdom = kingdoms[i];
			List<Actor> units = pChunk.objects.getUnits(pKingdom);
			for (int j = 0; j < units.Count; j++)
			{
				BaseSimObject baseSimObject = units[j];
				if (baseSimObject.isAlive() && (pTileRadius == 0 || Toolbox.SquaredDistTile(baseSimObject.current_tile, pTile) <= num))
				{
					pListObjects.Add(baseSimObject);
				}
			}
			List<Building> buildings = pChunk.objects.getBuildings(pKingdom);
			for (int k = 0; k < buildings.Count; k++)
			{
				BaseSimObject baseSimObject2 = buildings[k];
				if (baseSimObject2.isAlive() && (pTileRadius == 0 || Toolbox.SquaredDistTile(baseSimObject2.current_tile, pTile) <= num))
				{
					pListObjects.Add(baseSimObject2);
				}
			}
		}
	}

	internal static IEnumerable<Actor> findSpeciesAroundTileChunk(WorldTile pTile, string pUnitID)
	{
		foreach (Actor item in getUnitsFromChunk(pTile, 1))
		{
			if (!(item.a.asset.id != pUnitID))
			{
				yield return item;
			}
		}
	}

	public static Building getClosestBuildingFrom(Actor pActor, IReadOnlyCollection<Building> pBuildingList)
	{
		return getClosestBuildingFrom(pActor.current_tile, pBuildingList);
	}

	public static Building getClosestBuildingFrom(WorldTile pTile, IReadOnlyCollection<Building> pBuildingList)
	{
		Building result = null;
		float num = float.MaxValue;
		foreach (Building pBuilding in pBuildingList)
		{
			if (!pBuilding.isRekt() && pBuilding.current_tile.isSameIsland(pTile))
			{
				float num2 = Toolbox.SquaredDistTile(pBuilding.current_tile, pTile);
				if (num2 < num)
				{
					result = pBuilding;
					num = num2;
				}
			}
		}
		return result;
	}

	public static void clear()
	{
		_list_objects.Clear();
	}

	public static WorldTile findTileInChunk(WorldTile pTile, TileFinderType pTileType)
	{
		var (array, pLength) = Toolbox.getAllChunksFromTile(pTile);
		foreach (MapChunk item in array.LoopRandom(pLength))
		{
			foreach (MapRegion item2 in item.regions.LoopRandom())
			{
				foreach (WorldTile item3 in item2.tiles.LoopRandom())
				{
					switch (pTileType)
					{
					case TileFinderType.FreeTile:
						if (!item3.isSameIsland(pTile) || item3.hasBuilding() || !item3.Type.ground)
						{
							continue;
						}
						break;
					case TileFinderType.Sand:
						if (!item3.Type.sand)
						{
							continue;
						}
						break;
					case TileFinderType.Water:
						if (item3.isTargeted() || !item3.Type.ocean)
						{
							continue;
						}
						break;
					case TileFinderType.Grass:
						if (!item3.isSameIsland(pTile) || item3.isTargeted() || !item3.Type.grass || item3.hasBuilding())
						{
							continue;
						}
						break;
					case TileFinderType.Dirt:
						if (!item3.isSameIsland(pTile) || item3.isTargeted() || !item3.Type.can_be_farm || item3.hasBuilding())
						{
							continue;
						}
						break;
					case TileFinderType.Biome:
						if (!item3.isSameIsland(pTile) || item3.isTargeted() || !item3.Type.is_biome || item3.hasBuilding())
						{
							continue;
						}
						break;
					default:
						if (!item3.isSameIsland(pTile))
						{
							continue;
						}
						break;
					}
					return item3;
				}
			}
		}
		return null;
	}
}
