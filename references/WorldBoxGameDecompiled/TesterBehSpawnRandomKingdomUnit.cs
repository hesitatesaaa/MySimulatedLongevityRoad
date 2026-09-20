using ai.behaviours;

public class TesterBehSpawnRandomKingdomUnit : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		if (BehaviourActionBase<AutoTesterBot>.world.kingdoms.Count == 0)
		{
			return new TesterBehSpawnRandomCivUnit().execute(pObject);
		}
		Kingdom random = BehaviourActionBase<AutoTesterBot>.world.kingdoms.getRandom();
		if (random == null)
		{
			return BehResult.Continue;
		}
		if (!random.hasUnits())
		{
			return BehResult.Continue;
		}
		if (!random.hasCities())
		{
			return BehResult.Continue;
		}
		City random2 = random.getCities().GetRandom();
		if (random2 == null)
		{
			return BehResult.Continue;
		}
		if (!random2.hasZones())
		{
			return BehResult.Continue;
		}
		TileZone random3 = random2.zones.GetRandom();
		if (random3 == null)
		{
			return BehResult.Continue;
		}
		WorldTile random4 = random3.tiles.GetRandom();
		if (random4 == null)
		{
			return BehResult.Continue;
		}
		ActorAsset actorAsset = random.getActorAsset();
		BehaviourActionBase<AutoTesterBot>.world.units.spawnNewUnit(actorAsset.id, random4, pSpawnSound: false, pMiracleSpawn: true);
		return BehResult.Continue;
	}
}
