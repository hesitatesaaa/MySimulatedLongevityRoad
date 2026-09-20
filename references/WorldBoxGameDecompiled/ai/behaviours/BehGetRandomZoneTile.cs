namespace ai.behaviours;

public class BehGetRandomZoneTile : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		MapChunk randomChunkFromTile = Toolbox.getRandomChunkFromTile(pActor.current_tile);
		if (randomChunkFromTile != null)
		{
			pActor.beh_tile_target = randomChunkFromTile.tiles.GetRandom();
		}
		return BehResult.Continue;
	}
}
