using ai.behaviours;

public class BehClaimZoneForCityActorBorder : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		return tryClaimZone(pActor);
	}

	public static BehResult tryClaimZone(Actor pActor)
	{
		TileZone zone = pActor.current_tile.zone;
		City city = pActor.city;
		WorldTile tile = city.getTile();
		if (tile == null)
		{
			return BehResult.Stop;
		}
		if (!city.isZoneToClaimStillGood(pActor, zone, tile))
		{
			return BehResult.Stop;
		}
		bool flag = pActor.hasCultureTrait("expansionists") || DebugConfig.isOn(DebugOption.CityFastZonesGrowth);
		bool num = zone.city != null && zone.city != city;
		city.addZone(zone);
		if (num)
		{
			flag = false;
		}
		if (flag)
		{
			TileZone[] neighbours_all = zone.neighbours_all;
			foreach (TileZone tileZone in neighbours_all)
			{
				if (!tileZone.hasCity() && tileZone.centerTile.isSameIsland(tile) && city.isZoneToClaimStillGood(pActor, tileZone, tile))
				{
					city.addZone(tileZone);
				}
			}
		}
		pActor.addLoot(SimGlobals.m.coins_for_zone);
		return BehResult.Continue;
	}
}
