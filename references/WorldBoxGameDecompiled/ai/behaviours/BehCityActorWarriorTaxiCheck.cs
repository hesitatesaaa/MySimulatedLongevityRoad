using life.taxi;

namespace ai.behaviours;

public class BehCityActorWarriorTaxiCheck : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.current_tile.hasCity() && pActor.current_tile.zone_city.kingdom.isEnemy(pActor.kingdom))
		{
			return BehResult.Stop;
		}
		if (pActor.city.hasAttackZoneOrder() && !pActor.city.target_attack_zone.centerTile.isSameIsland(pActor.current_tile))
		{
			TaxiManager.newRequest(pActor, pActor.city.target_attack_zone.centerTile);
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
