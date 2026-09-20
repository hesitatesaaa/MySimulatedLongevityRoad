using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ai.behaviours;

public class BehBurnTumorTiles : BehaviourActionActor
{
	private static List<WorldTile> tiles = new List<WorldTile>();

	public override BehResult execute(Actor pActor)
	{
		if (!pActor.current_tile.Type.ground)
		{
			return BehResult.Stop;
		}
		WorldTile worldTile = null;
		List<WorldTile> list = tiles;
		checkRegion(pActor.current_tile.region, list);
		if (list.Count != 0)
		{
			worldTile = list.GetRandom();
		}
		else
		{
			List<MapRegion> neighbours = pActor.current_tile.region.neighbours;
			for (int i = 0; i < neighbours.Count; i++)
			{
				checkRegion(neighbours[i], list);
				if (list.Count != 0)
				{
					worldTile = list.GetRandom();
					break;
				}
			}
		}
		list.Clear();
		if (worldTile == null)
		{
			return BehResult.Stop;
		}
		AssetManager.spells.get("cast_fire").action?.Invoke(pActor, null, worldTile);
		pActor.doCastAnimation();
		return BehResult.Continue;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void checkRegion(MapRegion pRegion, List<WorldTile> pTiles)
	{
		List<WorldTile> list = pRegion.tiles;
		for (int i = 0; i < list.Count; i++)
		{
			WorldTile worldTile = list[i];
			if (worldTile.Type.creep)
			{
				pTiles.Add(worldTile);
			}
		}
	}
}
