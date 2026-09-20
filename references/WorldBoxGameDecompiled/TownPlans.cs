using UnityEngine;

public static class TownPlans
{
	public static bool isInPassableRingMap(TileZone pZone, TileZone pCenterZone = null)
	{
		TileZone mapCenterZone = World.world.zone_calculator.getMapCenterZone();
		return isInPassableRing(pZone, mapCenterZone);
	}

	public static bool isInPassableRing(TileZone pZone, TileZone pCityCenterZone)
	{
		float num = Toolbox.Dist(pZone.x, pZone.y, pCityCenterZone.x, pCityCenterZone.y);
		float num2 = 1f;
		float num3 = 1f;
		float num4 = num2 + num3;
		return num % num4 >= num2;
	}

	public static bool isPassableCross(TileZone pZone, TileZone pCityZone)
	{
		int num = Mathf.Abs(pCityZone.x - pZone.x);
		int num2 = Mathf.Abs(pCityZone.y - pZone.y);
		if (num > 1 && num2 > 1)
		{
			return false;
		}
		return true;
	}

	public static bool isPassableLineHorizontal(TileZone pZone, TileZone _ = null)
	{
		return pZone.y % 2 == 0;
	}

	public static bool isPassableLineVertical(TileZone pZone, TileZone _ = null)
	{
		return pZone.x % 2 == 0;
	}

	public static bool isPassableDiagonal(TileZone pZone, TileZone _ = null)
	{
		if ((pZone.x + pZone.y) % 3 != 0)
		{
			return true;
		}
		return false;
	}

	public static bool isPassableDiamond(TileZone pZone, TileZone _ = null)
	{
		if ((pZone.x + pZone.y) % 2 != 0)
		{
			return true;
		}
		return false;
	}

	public static bool isPassableDiamondCluster(TileZone pZone, TileZone _ = null)
	{
		return (pZone.x / 2 + pZone.y / 2) % 2 == 0;
	}

	public static bool isPassableHoneycomb(TileZone pZone, TileZone _ = null)
	{
		int num = ((pZone.y % 2 == 0) ? 2 : 0);
		return (pZone.x + num) % 4 == 0;
	}

	public static bool isPassableBrickHorizontal(TileZone pZone, TileZone _ = null)
	{
		if (!isPassableLineVertical(pZone))
		{
			return false;
		}
		if (!isPassableDiagonal(pZone))
		{
			return false;
		}
		return true;
	}

	public static bool isPassableBrickVertical(TileZone pZone, TileZone _ = null)
	{
		if (!isPassableLineHorizontal(pZone))
		{
			return false;
		}
		if (!isPassableDiagonal(pZone))
		{
			return false;
		}
		return true;
	}

	public static bool isPassableLatticeSmall(TileZone pZone, TileZone _ = null)
	{
		return isPassableLattice(pZone, 2, 1);
	}

	public static bool isPassableLatticeMedium(TileZone pZone, TileZone _ = null)
	{
		return isPassableLattice(pZone, 3, 1);
	}

	public static bool isPassableLatticeBig(TileZone pZone, TileZone _ = null)
	{
		return isPassableLattice(pZone, 4, 2);
	}

	public static bool isPassableMadmanLabyrinth(TileZone pZone, TileZone _ = null)
	{
		float num = 0.7f;
		float num2 = 0.4f;
		return Mathf.PerlinNoise((float)pZone.x * num, (float)pZone.y * num) > num2;
	}

	private static bool isPassableLattice(TileZone pZone, int pSpacing, int pWidth)
	{
		bool num = pZone.x % pSpacing < pWidth;
		bool flag = pZone.y % pSpacing < pWidth;
		return num | flag;
	}

	private static bool isPassableClusters(TileZone pZone, int pSpacing, int pWidth)
	{
		bool num = pZone.x % pSpacing < pWidth;
		bool flag = pZone.y % pSpacing < pWidth;
		return !(num | flag);
	}

	public static bool isPassableClustersSmall(TileZone pZone, TileZone _ = null)
	{
		return isPassableClusters(pZone, 3, 1);
	}

	public static bool isPassableClustersMedium(TileZone pZone, TileZone _ = null)
	{
		return isPassableClusters(pZone, 4, 1);
	}

	public static bool isPassableClustersBig(TileZone pZone, TileZone _ = null)
	{
		return isPassableClusters(pZone, 5, 1);
	}

	public static bool debugVisualizeZone(TileZone pZone, TileZone pCursorZone = null)
	{
		DebugVariables instance = DebugVariables.instance;
		if ((Object)(object)instance == (Object)null)
		{
			return false;
		}
		bool result = true;
		if (instance.layout_cross && !isPassableCross(pZone, pCursorZone))
		{
			result = false;
		}
		if (instance.layout_ring && !isInPassableRing(pZone, pCursorZone))
		{
			result = false;
		}
		if (instance.layout_lines_horizontal && !isPassableLineHorizontal(pZone))
		{
			result = false;
		}
		if (instance.layout_lines_vertical && !isPassableLineVertical(pZone))
		{
			result = false;
		}
		if (instance.layout_diagonal && !isPassableDiagonal(pZone))
		{
			result = false;
		}
		if (instance.layout_diamond && !isPassableDiamond(pZone))
		{
			result = false;
		}
		if (instance.layout_diamond_cluster && !isPassableDiamondCluster(pZone))
		{
			result = false;
		}
		if (instance.layout_lattice_small && !isPassableLatticeSmall(pZone))
		{
			result = false;
		}
		if (instance.layout_lattice_medium && !isPassableLatticeMedium(pZone))
		{
			result = false;
		}
		if (instance.layout_lattice_big && !isPassableLatticeBig(pZone))
		{
			result = false;
		}
		if (instance.layout_clusters_small && !isPassableClustersSmall(pZone))
		{
			result = false;
		}
		if (instance.layout_clusters_medium && !isPassableClustersMedium(pZone))
		{
			result = false;
		}
		if (instance.layout_clusters_big && !isPassableClustersBig(pZone))
		{
			result = false;
		}
		if (instance.layout_map_ring && !isInPassableRingMap(pZone))
		{
			result = false;
		}
		if (instance.layout_honeycomb && !isPassableHoneycomb(pZone))
		{
			result = false;
		}
		if (instance.layout_brick_horizontal && !isPassableBrickHorizontal(pZone))
		{
			result = false;
		}
		if (instance.layout_brick_vertical && !isPassableBrickVertical(pZone))
		{
			result = false;
		}
		if (instance.layout_madman_labyrinth && !isPassableMadmanLabyrinth(pZone))
		{
			result = false;
		}
		return result;
	}
}
