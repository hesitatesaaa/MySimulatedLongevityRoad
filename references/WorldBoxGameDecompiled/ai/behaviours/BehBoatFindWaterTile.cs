namespace ai.behaviours;

public class BehBoatFindWaterTile : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		WorldTile randomTileForBoat = ActorTool.getRandomTileForBoat(pActor);
		pActor.beh_tile_target = randomTileForBoat;
		return BehResult.Continue;
	}
}
