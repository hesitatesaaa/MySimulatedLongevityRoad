using System.Collections.Generic;
using UnityEngine;

namespace ai.behaviours;

public class BehCityActorFindClosestFire : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		WorldTile current_tile = pActor.current_tile;
		WorldTile closestTileOnFireFromZones = getClosestTileOnFireFromZones(pActor);
		if (closestTileOnFireFromZones == null)
		{
			return BehResult.Stop;
		}
		if (pActor.is_visible)
		{
			pActor.spawnSlashYell(Vector2Int.op_Implicit(closestTileOnFireFromZones.pos));
		}
		WorldTile worldTile = raycastTileForUnitToFightFire(current_tile, closestTileOnFireFromZones);
		if (worldTile == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = worldTile;
		return BehResult.Continue;
	}

	private static WorldTile getClosestTileOnFireFromZones(Actor pActor)
	{
		WorldTile current_tile = pActor.current_tile;
		TileZone current_zone = pActor.current_zone;
		WorldTile closestTileOnFire = getClosestTileOnFire(current_zone.tiles, current_tile);
		if (closestTileOnFire == null)
		{
			foreach (TileZone item in current_zone.neighbours_all.LoopRandom())
			{
				if (item.isZoneOnFire())
				{
					closestTileOnFire = getClosestTileOnFire(item.tiles, current_tile);
					if (closestTileOnFire != null)
					{
						return closestTileOnFire;
					}
				}
			}
		}
		return closestTileOnFire;
	}

	private static WorldTile getClosestTileOnFire(WorldTile[] pArray, WorldTile pTarget)
	{
		WorldTile result = null;
		int num = pArray.Length;
		int num2 = int.MaxValue;
		for (int i = 0; i < num; i++)
		{
			WorldTile worldTile = pArray[i];
			int num3 = Toolbox.SquaredDist(pTarget.x, pTarget.y, worldTile.x, worldTile.y);
			if (num3 < num2 && worldTile.isOnFire())
			{
				num2 = num3;
				result = worldTile;
			}
		}
		return result;
	}

	public static WorldTile raycastTileForUnitToFightFire(WorldTile pActorTile, WorldTile pTargetFire)
	{
		if (pActorTile == pTargetFire)
		{
			return pActorTile;
		}
		List<WorldTile> list = PathfinderTools.raycast(pTargetFire, pActorTile);
		WorldTile result = null;
		float num = float.MaxValue;
		for (int i = 0; i < list.Count; i++)
		{
			WorldTile worldTile = list[i];
			float num2 = Toolbox.SquaredDist(worldTile.x, worldTile.y, pTargetFire.x, pTargetFire.y);
			if (!(num2 < 4f) && !(num2 >= num))
			{
				num = num2;
				result = worldTile;
			}
		}
		list.Clear();
		return result;
	}
}
