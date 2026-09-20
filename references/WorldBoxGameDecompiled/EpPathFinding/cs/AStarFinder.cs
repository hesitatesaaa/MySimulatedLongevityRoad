using System;
using System.Collections.Generic;
using C5;

namespace EpPathFinding.cs;

public static class AStarFinder
{
	private static int _last_global_region_id;

	public static bool result_split_path;

	private static IntervalHeap<Node> _open_list = new IntervalHeap<Node>();

	public static void backTracePath(Node pNode, List<WorldTile> pSavePath, bool pEndToStartPath = false)
	{
		pSavePath.Clear();
		pSavePath.Add(pNode.tile);
		while ((object)pNode.parent != null)
		{
			pNode = pNode.parent;
			pSavePath.Add(pNode.tile);
		}
		if (!pEndToStartPath)
		{
			pSavePath.Reverse();
		}
	}

	public static void FindPath(AStarParam pParam, List<WorldTile> pSavePath)
	{
		_open_list.Clear();
		Node node;
		Node node2;
		if (pParam.end_to_start_path)
		{
			node = pParam.EndNode;
			node2 = pParam.StartNode;
		}
		else
		{
			node = pParam.StartNode;
			node2 = pParam.EndNode;
		}
		StaticGrid staticGrid = (StaticGrid)pParam.SearchGrid;
		DiagonalMovement diagonalMovement = pParam.DiagonalMovement;
		float weight = pParam.weight;
		bool boat = pParam.boat;
		node.startToCurNodeLen = 0f;
		node.heuristicStartToEndLen = 0f;
		_open_list.Add(node);
		node.isOpened = true;
		result_split_path = false;
		_last_global_region_id = 0;
		while (((CollectionValueBase<Node>)_open_list).Count != 0 && (pParam.max_open_list == -1 || ((CollectionValueBase<Node>)_open_list).Count <= pParam.max_open_list))
		{
			Node node3 = _open_list.DeleteMin();
			node3.isClosed = true;
			staticGrid.addToClosed(node3);
			if (node3 == node2)
			{
				backTracePath(node2, pSavePath, pParam.end_to_start_path);
				break;
			}
			WorldTile tile = node3.tile;
			if (pParam.use_global_path_lock)
			{
				if (tile.region.region_path_id < _last_global_region_id && tile.region.region_path_id != -1)
				{
					node3.isClosed = true;
					staticGrid.addToClosed(node3);
					continue;
				}
				if (tile.region.region_path_id > _last_global_region_id)
				{
					_last_global_region_id = tile.region.region_path_id;
				}
			}
			WorldTile[] array = ((diagonalMovement == DiagonalMovement.Never) ? tile.neighbours : ((!isObstaclesAround(tile)) ? tile.neighboursAll : tile.neighbours));
			foreach (WorldTile worldTile in array)
			{
				Node node4 = staticGrid.m_nodes[worldTile.x][worldTile.y];
				if (node4.isClosed)
				{
					continue;
				}
				TileTypeBase type = worldTile.Type;
				if (!pParam.block && type.block)
				{
					continue;
				}
				if (pParam.use_global_path_lock)
				{
					if (worldTile.region.region_path_id < _last_global_region_id && worldTile.region.region_path_id != -1)
					{
						node3.isClosed = true;
						staticGrid.addToClosed(node3);
						continue;
					}
					if (worldTile.region.region_path_id > _last_global_region_id)
					{
						_last_global_region_id = worldTile.region.region_path_id;
					}
					if (!worldTile.region.used_by_path_lock)
					{
						node3.isClosed = true;
						staticGrid.addToClosed(node3);
						continue;
					}
				}
				if (boat)
				{
					if (!worldTile.isGoodForBoat())
					{
						continue;
					}
				}
				else
				{
					if (!pParam.lava && type.lava)
					{
						continue;
					}
					if ((!pParam.block || !type.block) && (!pParam.lava || !type.lava))
					{
						if (pParam.ground && pParam.ocean)
						{
							if (!type.ground && !type.ocean)
							{
								continue;
							}
						}
						else if ((pParam.ground && !type.ground) || (pParam.ocean && !type.ocean))
						{
							continue;
						}
					}
				}
				float num = 1f;
				if (pParam.roads && type.road)
				{
					num = 0.01f;
				}
				if (_last_global_region_id >= 4 && staticGrid.closed_list_count > 10)
				{
					result_split_path = true;
					backTracePath(node3, pSavePath, pParam.end_to_start_path);
					return;
				}
				float num2 = node3.startToCurNodeLen + num * ((node4.x - node3.x == 0 || node4.y - node3.y == 0) ? 1f : 1.414f);
				if (node4.isOpened && !(num2 < node4.startToCurNodeLen))
				{
					continue;
				}
				node4.startToCurNodeLen = num2;
				if (!node4.heuristicCurNodeToEndLen.HasValue)
				{
					if (pParam.roads)
					{
						node4.heuristicCurNodeToEndLen = Heuristic.Euclidean(Math.Abs(node4.x - node2.x), Math.Abs(node4.y - node2.y)) * weight;
					}
					else
					{
						node4.heuristicCurNodeToEndLen = (float)(Math.Abs(node4.x - node2.x) + Math.Abs(node4.y - node2.y)) * weight;
					}
				}
				node4.heuristicStartToEndLen = node4.startToCurNodeLen + node4.heuristicCurNodeToEndLen.Value;
				node4.parent = node3;
				if (!node4.isOpened)
				{
					_open_list.Add(node4);
					node4.isOpened = true;
					staticGrid.addToClosed(node4);
				}
			}
		}
	}

	private static bool isObstaclesAround(WorldTile pTile)
	{
		return pTile.hasWallsAround();
	}
}
