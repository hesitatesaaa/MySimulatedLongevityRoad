using ai.behaviours;

public class BehFindRandomFarTile : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		MapRegion mapRegion = pActor.current_tile.region;
		for (int i = 0; i < 5; i++)
		{
			if (mapRegion.neighbours.Count == 0)
			{
				break;
			}
			mapRegion = mapRegion.neighbours.GetRandom();
		}
		if (mapRegion.tiles.Count > 0)
		{
			pActor.beh_tile_target = mapRegion.tiles.GetRandom();
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
