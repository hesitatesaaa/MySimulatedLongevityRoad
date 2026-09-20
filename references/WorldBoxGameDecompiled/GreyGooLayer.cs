using System.Collections.Generic;

public class GreyGooLayer : BaseModule
{
	private const float SPREAD_CHANCE = 0.05f;

	private const float REMOVE_CHANCE = 0.09f;

	private const float INTERVAL = 0.08f;

	private List<WorldTile> _to_remove = new List<WorldTile>();

	private List<WorldTile> _to_add = new List<WorldTile>();

	private bool _initiated;

	internal override void create()
	{
		base.create();
		hashset = new HashSet<WorldTile>();
	}

	internal override void clear()
	{
		base.clear();
		hashset.Clear();
		_to_remove.Clear();
		_to_add.Clear();
		_initiated = false;
	}

	public bool isActive()
	{
		return hashset.Count > 0;
	}

	private void init()
	{
		_initiated = true;
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile worldTile in tiles_list)
		{
			if (worldTile.Type.grey_goo)
			{
				add(worldTile);
			}
		}
	}

	public override void update(float pElapsed)
	{
		if (!_initiated)
		{
			init();
		}
		base.update(pElapsed);
		if (isActive() && !World.world.isPaused())
		{
			if (timer > 0f)
			{
				timer -= pElapsed;
				return;
			}
			timer = 0.08f;
			_to_remove.Clear();
			_to_add.Clear();
			updateGooTiles();
			removeFromHashset();
			addToHashset();
		}
	}

	private void updateGooTiles()
	{
		if (hashset.Count == 0)
		{
			return;
		}
		foreach (WorldTile item in hashset)
		{
			if (!item.Type.grey_goo)
			{
				_to_remove.Add(item);
				continue;
			}
			if (item.hasBuilding())
			{
				item.building.startDestroyBuilding();
			}
			if (Randy.randomChance(0.05f))
			{
				terraform(item);
				checkAroundTiles(item);
				_to_remove.Add(item);
			}
			else if (Randy.randomChance(0.05f))
			{
				checkAroundTiles(item);
			}
			else if (Randy.randomChance(0.09f) && areAroundTilesEmpty(item))
			{
				_to_remove.Add(item);
				if (item.Type.grey_goo)
				{
					terraform(item);
				}
			}
		}
	}

	private void removeFromHashset()
	{
		if (_to_remove.Count != 0)
		{
			for (int i = 0; i < _to_remove.Count; i++)
			{
				WorldTile pTile = _to_remove[i];
				remove(pTile);
			}
		}
	}

	private void addToHashset()
	{
		if (_to_add.Count != 0)
		{
			for (int i = 0; i < _to_add.Count; i++)
			{
				WorldTile pTile = _to_add[i];
				add(pTile);
			}
		}
	}

	private void checkAroundTiles(WorldTile pTile)
	{
		if (WorldLawLibrary.world_law_gaias_covenant.isEnabled())
		{
			return;
		}
		WorldTile[] neighbours = pTile.neighbours;
		foreach (WorldTile worldTile in neighbours)
		{
			TileTypeBase type = worldTile.Type;
			if (!type.grey_goo && !type.IsType("pit_deep_ocean") && (!type.IsType("deep_ocean") || worldTile.hasBuilding()))
			{
				_to_add.Add(worldTile);
			}
		}
	}

	private bool areAroundTilesEmpty(WorldTile pTile)
	{
		WorldTile[] neighbours = pTile.neighbours;
		foreach (WorldTile obj in neighbours)
		{
			TileTypeBase type = obj.Type;
			if (obj.hasBuilding())
			{
				return false;
			}
			if (!type.grey_goo && !type.considered_empty_tile)
			{
				return false;
			}
		}
		return true;
	}

	private void makeGoo(WorldTile pTile)
	{
		pTile.unfreeze(99);
		MapAction.terraformMain(pTile, TileLibrary.grey_goo);
	}

	private void terraform(WorldTile pTile)
	{
		MapAction.terraformMain(pTile, TileLibrary.pit_deep_ocean, TerraformLibrary.grey_goo);
		MusicBox.playSound("event:/SFX/DESTRUCTION/GreyGooEat", pTile, pGameViewOnly: false, pVisibleOnly: true);
	}

	public void remove(WorldTile pTile)
	{
		hashset.Remove(pTile);
	}

	public void add(WorldTile pTile)
	{
		if (!pTile.Type.considered_empty_tile && hashset.Add(pTile))
		{
			makeGoo(pTile);
		}
	}
}
