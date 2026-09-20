namespace ai.behaviours;

public class BehRandomSwim : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		BehaviourActionActor.possible_moves.Clear();
		WorldTile[] neighboursAll = pActor.current_tile.neighboursAll;
		foreach (WorldTile worldTile in neighboursAll)
		{
			if (worldTile.Type.liquid)
			{
				BehaviourActionActor.possible_moves.Add(worldTile);
			}
		}
		if (BehaviourActionActor.possible_moves.Count > 0)
		{
			WorldTile random = BehaviourActionActor.possible_moves.GetRandom();
			BehaviourActionActor.possible_moves.Clear();
			pActor.moveTo(random);
			pActor.setTileTarget(random);
		}
		return BehResult.Continue;
	}
}
