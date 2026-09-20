using System;
using System.Collections.Generic;
using EpPathFinding.cs;
using UnityEngine;
using ai;

public class ActorMove
{
	private const float CHUNK_MULTIPLIER = 4f;

	public static ExecuteEvent goTo(Actor pActor, WorldTile pTileTarget, bool pPathOnLiquid = false, bool pWalkOnBlocks = false, bool pPathOnLava = false, int pLimitPathfindingRegions = 0)
	{
		pActor.clearOldPath();
		ActorAsset asset = pActor.asset;
		bool is_boat = asset.is_boat;
		if (!DebugConfig.isOn(DebugOption.SystemUnitPathfinding))
		{
			pActor.current_path.Add(pTileTarget);
			return ExecuteEvent.True;
		}
		if (is_boat && !pTileTarget.isGoodForBoat())
		{
			return ExecuteEvent.False;
		}
		if (pActor.isFlying())
		{
			pActor.current_path.Add(pTileTarget);
			return ExecuteEvent.True;
		}
		WorldTile current_tile = pActor.current_tile;
		bool flag = current_tile.isSameIsland(pTileTarget);
		bool flag2 = pActor.isWaterCreature();
		bool flag3 = pActor.isInLiquid();
		bool flag4 = pActor.isImmuneToFire();
		if (flag && !flag3)
		{
			pPathOnLiquid = false;
		}
		AStarParam pathfinding_param = World.world.pathfinding_param;
		pathfinding_param.resetParam();
		pathfinding_param.block = pWalkOnBlocks;
		pathfinding_param.lava = flag4;
		pathfinding_param.fire = flag4;
		if (pActor.hasStatus("burning") || current_tile.isOnFire())
		{
			pathfinding_param.fire = true;
		}
		if (current_tile.Type.lava)
		{
			pathfinding_param.lava = true;
		}
		if (pPathOnLava)
		{
			pathfinding_param.lava = true;
		}
		pathfinding_param.ocean = flag2;
		if (pPathOnLiquid && !pActor.isDamagedByOcean())
		{
			pathfinding_param.ocean = true;
		}
		else if (flag3)
		{
			pathfinding_param.ocean = true;
		}
		pathfinding_param.ground = !flag2 || asset.force_land_creature;
		if (flag2 && !pActor.isInWater())
		{
			pathfinding_param.ground = true;
		}
		pathfinding_param.boat = is_boat && current_tile.isGoodForBoat();
		pathfinding_param.limit = true;
		if (PathfinderTools.tryToGetSimplePath(current_tile, pTileTarget, pActor.current_path, asset, pathfinding_param))
		{
			int num = (int)((float)pLimitPathfindingRegions * 4f);
			List<WorldTile> lastRaycastResult = PathfinderTools.getLastRaycastResult();
			if (pLimitPathfindingRegions > 0 && lastRaycastResult.Count > num)
			{
				lastRaycastResult.RemoveRange(num, lastRaycastResult.Count - num);
				pActor.current_path.Add(lastRaycastResult.Last());
			}
			else
			{
				pActor.current_path.Add(pTileTarget);
			}
		}
		World.world.path_finding_visualiser.showPath(null, pActor.current_path);
		if (pActor.isUsingPath())
		{
			pActor.setTileTarget(pTileTarget);
			return ExecuteEvent.True;
		}
		if (!flag)
		{
			TileTypeBase type = pTileTarget.Type;
			if ((!type.ground || !pathfinding_param.ground) && (!type.ocean || !pathfinding_param.ocean) && (!type.lava || !pathfinding_param.lava) && (!type.block || !pathfinding_param.block))
			{
				return ExecuteEvent.False;
			}
			TileIsland island = current_tile.region.island;
			TileIsland island2 = pTileTarget.region.island;
			if (island2.getTileCount() < island.getTileCount())
			{
				if (!island2.isConnectedWith(island))
				{
					return ExecuteEvent.False;
				}
				pathfinding_param.end_to_start_path = true;
			}
			else if (!island.isConnectedWith(island2))
			{
				return ExecuteEvent.False;
			}
		}
		bool flag5 = DebugConfig.isOn(DebugOption.UseGlobalPathLock);
		if (flag5)
		{
			if (is_boat)
			{
				flag5 = true;
			}
			else if (!flag)
			{
				flag5 = false;
			}
		}
		PathFinderResult pathFinderResult = PathFinderResult.PathFound;
		if (flag5)
		{
			switch (World.world.region_path_finder.getGlobalPath(current_tile, pTileTarget, is_boat))
			{
			case PathFinderResult.SamePlace:
				pActor.current_path.Add(pTileTarget);
				return ExecuteEvent.True;
			case PathFinderResult.NotFound:
				return ExecuteEvent.True;
			case PathFinderResult.DifferentIslands:
				return ExecuteEvent.True;
			}
			if (World.world.region_path_finder.last_globalPath != null)
			{
				pActor.current_path_global = World.world.region_path_finder.last_globalPath;
				lockRegionsForAStarSearch(pActor, pLimitPathfindingRegions);
			}
			else
			{
				current_tile.region.used_by_path_lock = true;
				current_tile.region.region_path_id = 0;
			}
		}
		pathfinding_param.use_global_path_lock = flag5;
		WorldTile pTargetTile = pTileTarget;
		if (flag5 && pActor.current_path_global != null && pActor.current_path_global.Count > 4)
		{
			int index = 4;
			pTargetTile = pActor.current_path_global[index].getRandomTile();
		}
		World.world.calcPath(current_tile, pTargetTile, pActor.current_path);
		if (AStarFinder.result_split_path)
		{
			pActor.split_path = SplitPathStatus.Prepare;
		}
		if (flag5)
		{
			cleanRegionSearch(pActor);
		}
		if (pLimitPathfindingRegions > 0 && pActor.current_path_global != null)
		{
			pTileTarget = pActor.current_path_global.Last().getRandomTile();
		}
		pActor.setTileTarget(pTileTarget);
		return ExecuteEvent.True;
	}

	private static void lockRegionsForAStarSearch(Actor pActor, int pLimitPathfindingRegions)
	{
		int num = 0;
		List<MapRegion> current_path_global = pActor.current_path_global;
		int limitedPathfindingRegions = getLimitedPathfindingRegions(current_path_global, pLimitPathfindingRegions);
		for (int i = 0; i < limitedPathfindingRegions; i++)
		{
			MapRegion mapRegion = current_path_global[i];
			mapRegion.used_by_path_lock = true;
			mapRegion.region_path_id = num++;
		}
		if (limitedPathfindingRegions < current_path_global.Count)
		{
			current_path_global.RemoveRange(limitedPathfindingRegions, current_path_global.Count - limitedPathfindingRegions);
		}
	}

	private static int getLimitedPathfindingRegions(List<MapRegion> pPath, int pLimitPathfindingRegions)
	{
		if (pLimitPathfindingRegions <= 0)
		{
			return pPath.Count;
		}
		return Mathf.Min(pPath.Count, pLimitPathfindingRegions);
	}

	private static void cleanRegionSearch(Actor pActor)
	{
		if (pActor.current_path_global != null)
		{
			List<MapRegion> current_path_global = pActor.current_path_global;
			for (int i = 0; i < current_path_global.Count; i++)
			{
				MapRegion mapRegion = current_path_global[i];
				mapRegion.used_by_path_lock = false;
				mapRegion.region_path_id = -1;
			}
		}
		MapRegion region = pActor.current_tile.region;
		region.used_by_path_lock = false;
		region.region_path_id = -1;
	}

	public unsafe static ExecuteEvent goToCurved(Actor pActor, params WorldTile[] pTargets)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		Span<Vector3> span;
		if (pTargets.Length < 128)
		{
			int num = pTargets.Length;
			span = new Span<Vector3>(stackalloc Vector3[num], num);
		}
		else
		{
			span = (Vector3[]?)(object)new Vector3[pTargets.Length];
		}
		Span<Vector3> pPoints = span;
		for (int i = 0; i < pTargets.Length; i++)
		{
			pPoints[i] = pTargets[i].posV3;
		}
		pActor.clearOldPath();
		float num2 = 0f;
		for (int j = 1; j < pTargets.Length; j++)
		{
			WorldTile pT = pTargets[j - 1];
			WorldTile pT2 = pTargets[j];
			num2 += Toolbox.DistTile(pT, pT2);
		}
		float num3 = (int)(num2 / 4f);
		num3 = Mathf.Clamp(num3, 5f, 100f);
		for (int k = 0; (float)k < num3; k++)
		{
			Vector2 pPos = Toolbox.cubeBezierN(Toolbox.easeInOutQuart((float)(k + 1) / num3), pPoints);
			Toolbox.clampToMap(ref pPos);
			WorldTile tileSimple = World.world.GetTileSimple((int)pPos.x, (int)pPos.y);
			if (pActor.current_path.Count <= 0 || pActor.current_path.Last() != tileSimple)
			{
				pActor.current_path.Add(tileSimple);
			}
		}
		WorldTile worldTile = pTargets.Last();
		pActor.current_path.Add(worldTile);
		World.world.path_finding_visualiser.showPath(null, pActor.current_path);
		pActor.setTileTarget(worldTile);
		return ExecuteEvent.True;
	}
}
