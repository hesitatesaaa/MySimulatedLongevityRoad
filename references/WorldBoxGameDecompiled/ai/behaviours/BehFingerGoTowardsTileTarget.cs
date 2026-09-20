namespace ai.behaviours;

public class BehFingerGoTowardsTileTarget : BehFinger
{
	private int _tile_range;

	public BehFingerGoTowardsTileTarget(int pRadiusTileRange = 25)
	{
		_tile_range = pRadiusTileRange;
	}

	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		WorldTile randomTileWithinDistance = Toolbox.getRandomTileWithinDistance(pActor.current_tile, _tile_range);
		WorldTile randomTileWithinDistance2 = Toolbox.getRandomTileWithinDistance(randomTileWithinDistance, _tile_range);
		WorldTile randomTileWithinDistance3 = Toolbox.getRandomTileWithinDistance(pActor.beh_tile_target, _tile_range);
		WorldTile randomTileWithinDistance4 = Toolbox.getRandomTileWithinDistance(randomTileWithinDistance3, _tile_range);
		if (ActorMove.goToCurved(pActor, pActor.current_tile, randomTileWithinDistance, randomTileWithinDistance2, randomTileWithinDistance4, randomTileWithinDistance3) == ExecuteEvent.False)
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
