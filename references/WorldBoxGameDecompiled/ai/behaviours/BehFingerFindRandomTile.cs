namespace ai.behaviours;

public class BehFingerFindRandomTile : BehFinger
{
	private int _range;

	public BehFingerFindRandomTile(int pRange = 75)
	{
		_range = pRange;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.findCurrentTile(pCheckNeighbours: false);
		pActor.beh_tile_target = Toolbox.getRandomTileWithinDistance(pActor.current_tile, _range);
		return BehResult.Continue;
	}
}
