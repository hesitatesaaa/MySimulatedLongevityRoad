namespace ai.behaviours;

public class BehVerifierAttackZone : BehCitizenActionCity
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.city == null)
		{
			return BehResult.Stop;
		}
		TileZone target_attack_zone = pActor.city.target_attack_zone;
		if (!pActor.city.hasAttackZoneOrder())
		{
			return BehResult.Stop;
		}
		City city = pActor.city.target_attack_zone.city;
		if (city == null)
		{
			return BehResult.Stop;
		}
		if (target_attack_zone == null)
		{
			return BehResult.Stop;
		}
		if (pActor.kingdom.isEnemy(city.kingdom))
		{
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
