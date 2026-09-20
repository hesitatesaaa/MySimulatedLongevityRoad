namespace ai.behaviours;

public class BehCityActorCheckAttack : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		TileZone target_attack_zone = pActor.city.target_attack_zone;
		City city = pActor.city.target_attack_zone.city;
		if (!isAttackingZoneAvailable(pActor, target_attack_zone, city))
		{
			return BehResult.Stop;
		}
		if (pActor.current_tile.zone.city != target_attack_zone.city)
		{
			pActor.beh_tile_target = target_attack_zone.tiles.GetRandom();
			return BehResult.Continue;
		}
		Building buildingOfType = city.getBuildingOfType("type_watch_tower", pCountOnlyFinished: false, pRandom: false, pOnlyFreeTile: false, pActor.current_island);
		if (buildingOfType != null)
		{
			pActor.beh_tile_target = buildingOfType.current_tile.region.tiles.GetRandom();
			return BehResult.Continue;
		}
		TileZone[] neighbours_all = pActor.current_tile.zone.neighbours_all;
		foreach (TileZone tileZone in neighbours_all)
		{
			if (tileZone.city == city)
			{
				WorldTile random = tileZone.tiles.GetRandom();
				if (random.isSameIsland(pActor.current_tile))
				{
					pActor.beh_tile_target = random;
					return BehResult.Continue;
				}
			}
		}
		foreach (TileZone zone in city.zones)
		{
			WorldTile random2 = zone.tiles.GetRandom();
			if (random2.isSameIsland(pActor.current_tile))
			{
				pActor.beh_tile_target = random2;
				return BehResult.Continue;
			}
		}
		return BehResult.Stop;
	}

	private bool isAttackingZoneAvailable(Actor pActor, TileZone pAttackZone, City pAttackCity)
	{
		if (pActor.army.isGroupInCityAndHaveLeader() && !pActor.city.isOkToSendArmy())
		{
			return false;
		}
		if (pAttackCity == null)
		{
			return false;
		}
		if (pAttackZone == null)
		{
			return false;
		}
		if (!pAttackZone.centerTile.isSameIsland(pActor.current_tile))
		{
			return false;
		}
		return true;
	}
}
