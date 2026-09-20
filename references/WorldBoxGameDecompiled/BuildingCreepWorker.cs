using System;
using UnityEngine;

public class BuildingCreepWorker : IDisposable
{
	private int steps_max;

	private WorldTile cur_tile;

	private ActorDirection cur_direction;

	private BuildingCreepHUB _parent;

	private int _total_step_counter;

	private BiomeAsset _this_creep_biome;

	private int _direction_step_amount;

	public BuildingCreepWorker(BuildingCreepHUB pParent)
	{
		_parent = pParent;
		_this_creep_biome = AssetManager.biome_library.get(_parent.building.asset.grow_creep_type);
		steps_max = _parent.building.asset.grow_creep_steps_max;
	}

	public void update()
	{
		if (cur_tile == null)
		{
			_total_step_counter = 0;
			cur_tile = _parent.building.current_tile;
			cur_direction = Randy.getRandom(Toolbox.directions);
		}
		checkRandomDirectionChange();
		updateMovement(cur_tile);
		if (_total_step_counter > steps_max)
		{
			cur_tile = null;
		}
	}

	private void checkRandomDirectionChange()
	{
		if (_parent.building.asset.grow_creep_random_new_direction)
		{
			if (_direction_step_amount >= _parent.building.asset.grow_creep_steps_before_new_direction)
			{
				cur_direction = Randy.getRandom(Toolbox.directions);
				_direction_step_amount = 0;
			}
			_direction_step_amount++;
		}
	}

	private void creepFlash(int pVal = 15)
	{
		if (_parent.building.asset.grow_creep_flash)
		{
			World.world.flash_effects.flashPixel(cur_tile, pVal);
		}
		if (_parent.building.asset.grow_creep_redraw_tile)
		{
			World.world.redrawRenderedTile(cur_tile);
		}
	}

	private void updateMovement(WorldTile pNextTile)
	{
		cur_tile = pNextTile;
		if (canPlaceWorkerOn(cur_tile))
		{
			makeCreep(cur_tile);
			creepFlash();
			_total_step_counter++;
		}
		else if (cur_tile.Type.biome_asset == _this_creep_biome)
		{
			creepFlash();
			pNextTile = getNextRandomTile(cur_tile);
			if (pNextTile == null)
			{
				cur_tile = null;
			}
			else if (canPlaceWorkerOn(pNextTile))
			{
				cur_tile = pNextTile;
			}
			else if (pNextTile.Type.biome_asset == _this_creep_biome)
			{
				cur_tile = pNextTile;
			}
			else if (pNextTile.Type.biome_asset != _this_creep_biome)
			{
				creepFlash(30);
				cur_tile = pNextTile;
			}
			else if (pNextTile.getCreepTileRank() == TileRank.Nothing)
			{
				pNextTile = cur_tile;
				cur_direction = Randy.getRandom(Toolbox.directions);
			}
		}
		else
		{
			cur_tile = null;
		}
	}

	private bool canPlaceWorkerOn(WorldTile pTile)
	{
		if (pTile.getCreepTileRank() == TileRank.Nothing)
		{
			return false;
		}
		if (pTile.Type.creep && pTile.Type.biome_asset == _this_creep_biome)
		{
			return false;
		}
		return true;
	}

	private void makeCreep(WorldTile pTile)
	{
		TopTileType tile = AssetManager.biome_library.get(_parent.building.asset.grow_creep_type).getTile(pTile);
		if (tile != null)
		{
			MapAction.terraformTop(pTile, tile, TerraformLibrary.flash);
		}
	}

	private WorldTile getNextRandomTile(WorldTile pTile)
	{
		return _parent.building.asset.grow_creep_movement_type switch
		{
			CreepWorkerMovementType.Direction => getDirectionTile(pTile, _parent.building.asset.grow_creep_direction_random_position), 
			CreepWorkerMovementType.RandomNeighbour => pTile.neighbours.GetRandom(), 
			CreepWorkerMovementType.RandomNeighbourAll => pTile.neighboursAll.GetRandom(), 
			_ => pTile.neighboursAll.GetRandom(), 
		};
	}

	private WorldTile getDirectionTile(WorldTile pTile, bool pAddRandom = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int pos = pTile.pos;
		int num = ((Vector2Int)(ref pos)).x;
		pos = pTile.pos;
		int num2 = ((Vector2Int)(ref pos)).y;
		switch (cur_direction)
		{
		case ActorDirection.Up:
			if (pAddRandom)
			{
				num += Randy.randomInt(-1, 2);
			}
			num2++;
			break;
		case ActorDirection.Right:
			num++;
			if (pAddRandom)
			{
				num2 += Randy.randomInt(-1, 2);
			}
			break;
		case ActorDirection.Down:
			if (pAddRandom)
			{
				num += Randy.randomInt(-1, 2);
			}
			num2--;
			break;
		case ActorDirection.Left:
			num--;
			if (pAddRandom)
			{
				num2 += Randy.randomInt(-1, 2);
			}
			break;
		}
		return World.world.GetTile(num, num2);
	}

	public void Dispose()
	{
		_parent = null;
		cur_tile = null;
		_this_creep_biome = null;
	}
}
