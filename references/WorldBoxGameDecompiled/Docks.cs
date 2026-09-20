using System.Collections.Generic;
using UnityPools;

public class Docks : BaseBuildingComponent
{
	public ListPool<WorldTile> tiles_ocean;

	private Dictionary<string, int> _boat_types;

	internal override void create(Building pBuilding)
	{
		base.create(pBuilding);
		tiles_ocean = new ListPool<WorldTile>();
		_boat_types = UnsafeCollectionPool<Dictionary<string, int>, KeyValuePair<string, int>>.Get();
	}

	public TileIsland getIsland()
	{
		if (building.hasCity())
		{
			return building.city.getTile()?.region.island;
		}
		return null;
	}

	public WorldTile getOceanTileInSameOcean(WorldTile pTile)
	{
		foreach (WorldTile item in tiles_ocean.LoopRandom())
		{
			if (item.isSameIsland(pTile))
			{
				return item;
			}
		}
		return null;
	}

	public bool hasOceanTiles()
	{
		recalculateOceanTiles();
		return tiles_ocean.Count > 0;
	}

	public void recalculateOceanTiles()
	{
		tiles_ocean.Clear();
		WorldTile tile = World.world.GetTile(building.current_tile.x - 4, building.current_tile.y);
		if (tile != null && tile.isGoodForBoat())
		{
			tiles_ocean.Add(tile);
		}
		tile = World.world.GetTile(building.current_tile.x + 5, building.current_tile.y);
		if (tile != null && tile.isGoodForBoat())
		{
			tiles_ocean.Add(tile);
		}
		tile = World.world.GetTile(building.current_tile.x, building.current_tile.y - 4);
		if (tile != null && tile.isGoodForBoat())
		{
			tiles_ocean.Add(tile);
		}
		tile = World.world.GetTile(building.current_tile.x, building.current_tile.y + 7);
		if (tile != null && tile.isGoodForBoat())
		{
			tiles_ocean.Add(tile);
		}
		if (tiles_ocean.Count == 0)
		{
			building.startDestroyBuilding();
		}
	}

	public bool isDockGood()
	{
		if (tiles_ocean.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < tiles_ocean.Count; i++)
		{
			if (!tiles_ocean[i].Type.ocean)
			{
				return false;
			}
		}
		return true;
	}

	private bool ifStayingOnGround()
	{
		for (int i = 0; i < building.tiles.Count; i++)
		{
			if (building.tiles[i].Type.ground)
			{
				return true;
			}
		}
		return false;
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		if (ifStayingOnGround())
		{
			building.getHit(1000f);
		}
		if (building.hasCity() && building.city.buildings.Count == 1)
		{
			building.getHit(1000f);
		}
	}

	public int countBoatTypes(string pType)
	{
		_boat_types.TryGetValue(pType, out var value);
		return value;
	}

	public bool isFull(string pType)
	{
		if (countBoatTypes(pType) >= 1)
		{
			return true;
		}
		return false;
	}

	public bool isOverfilled(string pType)
	{
		if (countBoatTypes(pType) > 1)
		{
			return true;
		}
		return false;
	}

	public Actor buildBoatFromHere(City pCity)
	{
		ActorAsset randomBoatAssetToBuild = building.asset.getRandomBoatAssetToBuild(pCity);
		if (randomBoatAssetToBuild == null)
		{
			return null;
		}
		if (countBoatTypes(randomBoatAssetToBuild.boat_type) >= 1)
		{
			return null;
		}
		if (!pCity.hasEnoughResourcesFor(randomBoatAssetToBuild.cost))
		{
			return null;
		}
		if (tiles_ocean.Count == 0)
		{
			recalculateOceanTiles();
			return null;
		}
		WorldTile random = tiles_ocean.GetRandom();
		if (!random.region.island.goodForDocks())
		{
			return null;
		}
		Actor actor = World.world.units.createNewUnit(randomBoatAssetToBuild.id, random);
		if (actor == null)
		{
			return null;
		}
		addBoatToDock(actor);
		pCity.spendResourcesForBuildingAsset(randomBoatAssetToBuild.cost);
		return actor;
	}

	public void clearBoatCounter()
	{
		_boat_types.Clear();
	}

	public void increaseBoatCounter(Actor pActor)
	{
		int num = countBoatTypes(pActor.asset.boat_type);
		num = (_boat_types[pActor.asset.boat_type] = num + 1);
	}

	public void addBoatToDock(Actor pBoat)
	{
		pBoat.setHomeBuilding(building);
		pBoat.joinCity(building.city);
		increaseBoatCounter(pBoat);
	}

	public override void Dispose()
	{
		base.Dispose();
		tiles_ocean.Dispose();
		tiles_ocean = null;
		_boat_types.Clear();
		UnsafeCollectionPool<Dictionary<string, int>, KeyValuePair<string, int>>.Release(_boat_types);
		_boat_types = null;
	}
}
