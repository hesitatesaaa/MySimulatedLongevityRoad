namespace ai.behaviours;

public class BehGetTargetBuildingMainTile : BehActorBuildingTarget
{
	public override BehResult execute(Actor pActor)
	{
		pActor.beh_tile_target = pActor.beh_building_target.current_tile;
		return BehResult.Continue;
	}
}
