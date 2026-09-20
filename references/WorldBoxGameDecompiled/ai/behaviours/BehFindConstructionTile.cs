namespace ai.behaviours;

public class BehFindConstructionTile : BehActorBuildingTarget
{
	public override BehResult execute(Actor pActor)
	{
		pActor.beh_tile_target = pActor.beh_building_target.getConstructionTile();
		return BehResult.Continue;
	}
}
