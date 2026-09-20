using life.taxi;

namespace ai.behaviours;

public class BehTaxiCheck : BehCitizenActionCity
{
	public override BehResult execute(Actor pActor)
	{
		WorldTile tile = pActor.city.getTile();
		if (tile == null)
		{
			return BehResult.Stop;
		}
		bool flag = false;
		if (pActor.isCitizenJob("attacker"))
		{
			if (!pActor.current_tile.isSameIsland(tile) && (!pActor.city.hasAttackZoneOrder() || !pActor.city.target_attack_zone.centerTile.isSameIsland(pActor.current_tile)))
			{
				flag = true;
			}
		}
		else if (!pActor.current_tile.isSameIsland(tile))
		{
			flag = true;
		}
		if (!flag)
		{
			return BehResult.Stop;
		}
		TaxiManager.newRequest(pActor, tile);
		return BehResult.Continue;
	}
}
