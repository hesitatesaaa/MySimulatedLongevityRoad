using System.Collections.Generic;
using UnityEngine;

namespace ai.behaviours;

public class BehFindTileForCity : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (!BehaviourActionBase<Actor>.world.city_zone_helper.city_place_finder.hasPossibleZones())
		{
			return BehResult.Stop;
		}
		TileZone tileZone = null;
		float num = float.MaxValue;
		List<TileZone> zones = BehaviourActionBase<Actor>.world.city_zone_helper.city_place_finder.zones;
		Vector3 posV = pActor.current_tile.posV3;
		TileZone tileZone2 = null;
		for (int i = 0; i < zones.Count; i++)
		{
			TileZone tileZone3 = zones[i];
			float num2 = Toolbox.SquaredDistVec3(posV, tileZone3.centerTile.posV3);
			if (!(num <= num2) && tileZone3.tiles[0].isSameIsland(pActor.current_tile) && tileZone3.isGoodForNewCity())
			{
				num = num2;
				tileZone = tileZone3;
			}
		}
		if (tileZone != null)
		{
			pActor.beh_tile_target = tileZone.tiles.GetRandom();
			return BehResult.Continue;
		}
		if (tileZone2 != null)
		{
			pActor.beh_tile_target = tileZone2.tiles.GetRandom();
			return BehResult.Continue;
		}
		TileIsland randomIslandGround = BehaviourActionBase<Actor>.world.islands_calculator.getRandomIslandGround();
		if (randomIslandGround == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = randomIslandGround.getRandomTile();
		return BehResult.Continue;
	}
}
