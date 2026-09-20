using ai.behaviours;

public class BehActorCheckZoneTarget : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		City city = pActor.city;
		TileZone zoneToClaim = BehaviourActionBase<Actor>.world.city_zone_helper.city_growth.getZoneToClaim(pActor, pActor.city);
		if (zoneToClaim == null)
		{
			return BehResult.Stop;
		}
		if (zoneToClaim.city == city)
		{
			return BehResult.Stop;
		}
		WorldTile worldTile = null;
		if (zoneToClaim.centerTile.isSameIsland(pActor.current_tile))
		{
			worldTile = zoneToClaim.centerTile;
		}
		else
		{
			foreach (WorldTile item in zoneToClaim.tiles.LoopRandom())
			{
				if (item.isSameIsland(pActor.current_tile))
				{
					worldTile = item;
					break;
				}
			}
		}
		if (worldTile == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = worldTile;
		return BehResult.Continue;
	}
}
