using System.Collections.Generic;
using EpPathFinding.cs;
using UnityEngine;

public class PathfinderTools
{
	private static readonly List<WorldTile> _raycast_result = new List<WorldTile>();

	public static List<WorldTile> raycast(WorldTile pFrom, WorldTile pTarget, float pMod = 0.99f)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		List<WorldTile> raycast_result = _raycast_result;
		raycast_result.Clear();
		float num = Toolbox.DistTile(pFrom, pTarget) * pMod;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(pFrom.posV3.x, pFrom.posV3.y);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(pTarget.posV3.x, pTarget.posV3.y);
		WorldTile worldTile = null;
		for (int i = 0; (float)i <= num; i++)
		{
			Vector2 val3 = Vector2.Lerp(val, val2, (float)i / num);
			int pX = Mathf.FloorToInt(val3.x);
			int pY = Mathf.FloorToInt(val3.y);
			WorldTile tile = World.world.GetTile(pX, pY);
			if (tile != null && tile != worldTile)
			{
				raycast_result.Add(tile);
				worldTile = tile;
			}
		}
		if (worldTile != pTarget)
		{
			raycast_result.Add(pTarget);
		}
		return raycast_result;
	}

	public static List<WorldTile> raycast(Vector2 pFrom, Vector2 pTarget, float pMod = 0.99f)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		List<WorldTile> raycast_result = _raycast_result;
		raycast_result.Clear();
		float num = Toolbox.DistVec2Float(pFrom, pTarget) * pMod;
		WorldTile worldTile = null;
		for (int i = 0; (float)i <= num; i++)
		{
			Vector2 val = Vector2.Lerp(pFrom, pTarget, (float)i / num);
			int pX = Mathf.FloorToInt(val.x);
			int pY = Mathf.FloorToInt(val.y);
			WorldTile tile = World.world.GetTile(pX, pY);
			if (tile != null && tile != worldTile)
			{
				raycast_result.Add(tile);
				worldTile = tile;
			}
		}
		if (Vector2Int.op_Implicit(worldTile.pos) != pTarget)
		{
			WorldTile tile2 = World.world.GetTile((int)pTarget.x, (int)pTarget.y);
			if (tile2 != null)
			{
				raycast_result.Add(tile2);
			}
		}
		return raycast_result;
	}

	public static bool tryToGetSimplePath(WorldTile pFrom, WorldTile pTargetTile, List<WorldTile> pPathToFill, ActorAsset pAsset, AStarParam pParam, int pTileLimit = 0)
	{
		raycast(pFrom, pTargetTile);
		List<WorldTile> raycast_result = _raycast_result;
		int i = 0;
		for (int count = raycast_result.Count; i < count; i++)
		{
			WorldTile worldTile = raycast_result[i];
			if (!pParam.block && worldTile.Type.block)
			{
				return false;
			}
			if (!pParam.lava && worldTile.Type.lava)
			{
				return false;
			}
			if (!pParam.fire && worldTile.isOnFire())
			{
				return false;
			}
			if (!pParam.ocean && worldTile.Type.ocean)
			{
				return false;
			}
			if (!pParam.ground && worldTile.Type.ground)
			{
				return false;
			}
			if (pParam.boat && !worldTile.isGoodForBoat())
			{
				return false;
			}
			if (worldTile.hasWallsAround())
			{
				return false;
			}
		}
		return true;
	}

	public static WorldTile raycastTileForUnitToEmbark(WorldTile pFromGround, WorldTile pTargetOcean)
	{
		List<WorldTile> list = raycast(pTargetOcean, pFromGround);
		WorldTile result = null;
		TileIsland island = pFromGround.region.island;
		for (int i = 0; i < list.Count; i++)
		{
			WorldTile worldTile = list[i];
			if (worldTile.region.island == island)
			{
				result = worldTile;
				break;
			}
		}
		list.Clear();
		return result;
	}

	public static List<WorldTile> getLastRaycastResult()
	{
		return _raycast_result;
	}

	public static WorldTile raycastTileForUnitLandingFromOcean(WorldTile pFromOcean, WorldTile pTargetGround)
	{
		List<WorldTile> list = raycast(pFromOcean, pTargetGround);
		WorldTile result = null;
		TileIsland island = pTargetGround.region.island;
		for (int i = 0; i < list.Count; i++)
		{
			WorldTile worldTile = list[i];
			if (worldTile.region.island == island)
			{
				result = worldTile;
				break;
			}
		}
		list.Clear();
		return result;
	}
}
