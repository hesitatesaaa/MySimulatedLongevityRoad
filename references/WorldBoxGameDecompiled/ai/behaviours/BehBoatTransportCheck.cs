namespace ai.behaviours;

public class BehBoatTransportCheck : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		checkHomeDocks(pActor);
		if (boat.hasPassengers())
		{
			WorldTile worldTile = null;
			if (boat.countPassengers() > 5 && pActor != null && pActor.city?.hasAttackZoneOrder() == true)
			{
				worldTile = pActor.city.target_attack_zone.centerTile;
			}
			if (worldTile == null)
			{
				worldTile = pActor?.city?.getTile();
			}
			if (worldTile != null)
			{
				boat.taxi_target = worldTile;
				pActor.beh_tile_target = worldTile;
				return forceTask(pActor, "boat_transport_go_unload", pClean: false);
			}
			return BehResult.Stop;
		}
		return forceTask(pActor, "boat_transport_check_taxi");
	}
}
