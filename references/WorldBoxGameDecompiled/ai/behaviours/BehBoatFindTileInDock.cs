namespace ai.behaviours;

public class BehBoatFindTileInDock : BehBoat
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		check_building_target_non_usable = true;
		null_check_building_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		if (!pActor.beh_building_target.isCiv())
		{
			return BehResult.Stop;
		}
		WorldTile oceanTileInSameOcean = pActor.beh_building_target.component_docks.getOceanTileInSameOcean(pActor.current_tile);
		if (oceanTileInSameOcean == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = oceanTileInSameOcean;
		return BehResult.Continue;
	}
}
