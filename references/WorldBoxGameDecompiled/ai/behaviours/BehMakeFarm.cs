namespace ai.behaviours;

public class BehMakeFarm : BehCityActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		if (!pActor.beh_tile_target.Type.can_be_farm)
		{
			return BehResult.Stop;
		}
		if (pActor.beh_tile_target.hasBuilding() && !pActor.beh_tile_target.building.canRemoveForFarms())
		{
			return BehResult.Stop;
		}
		MapAction.terraformTop(pActor.beh_tile_target, TopTileLibrary.field);
		MusicBox.playSound("event:/SFX/CIVILIZATIONS/MakeFarmField", pActor.beh_tile_target, pGameViewOnly: true, pVisibleOnly: true);
		pActor.addLoot(SimGlobals.m.coins_for_field);
		return BehResult.Continue;
	}
}
