namespace ai.behaviours;

public class BehTeleportHome : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasCity())
		{
			return BehResult.Stop;
		}
		City city = pActor.getCity();
		if (!city.hasZones())
		{
			return BehResult.Stop;
		}
		TileZone random = city.zones.GetRandom();
		if (random == null)
		{
			return BehResult.Stop;
		}
		WorldTile random2 = random.tiles.GetRandom();
		ActionLibrary.teleportEffect(pActor, random2);
		pActor.cancelAllBeh();
		pActor.spawnOn(random2);
		pActor.doCastAnimation();
		return BehResult.Continue;
	}
}
