namespace ai.behaviours;

public class BehGoToBuildingTarget : BehActorBuildingTarget
{
	private bool _path_on_water;

	public BehGoToBuildingTarget(bool pPathOnWater = false)
	{
		_path_on_water = pPathOnWater;
	}

	public override BehResult execute(Actor pActor)
	{
		goToBuilding(pActor);
		return BehResult.Continue;
	}

	internal void goToBuilding(Actor pActor)
	{
		WorldTile current_tile = pActor.beh_building_target.current_tile;
		pActor.goTo(current_tile, _path_on_water);
	}
}
