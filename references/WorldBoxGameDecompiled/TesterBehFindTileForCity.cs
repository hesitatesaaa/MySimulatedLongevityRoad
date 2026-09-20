using ai.behaviours;

public class TesterBehFindTileForCity : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		TileZone random = BehaviourActionBase<AutoTesterBot>.world.zone_calculator.zones.GetRandom();
		for (int i = 0; i < 100; i++)
		{
			if (random.isGoodForNewCity())
			{
				pObject.beh_tile_target = random.centerTile;
				return BehResult.Continue;
			}
			random = BehaviourActionBase<AutoTesterBot>.world.zone_calculator.zones.GetRandom();
		}
		return base.execute(pObject);
	}
}
