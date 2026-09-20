using ai.behaviours;

public class BehCityActorGetRandomBorderTile : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.city.hasZones())
		{
			return BehResult.Stop;
		}
		if (pActor.city.border_zones.Count == 0)
		{
			return BehResult.Stop;
		}
		WorldTile random = pActor.city.border_zones.GetRandom().tiles.GetRandom();
		if (!random.Type.ground)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = random;
		return BehResult.Continue;
	}
}
