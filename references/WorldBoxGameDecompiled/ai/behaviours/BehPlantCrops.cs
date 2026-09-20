namespace ai.behaviours;

public class BehPlantCrops : BehCityActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		if (pActor.beh_tile_target.Type == TopTileLibrary.field && !pActor.beh_tile_target.hasBuilding())
		{
			BehaviourActionBase<Actor>.world.buildings.addBuilding("wheat", pActor.beh_tile_target);
			pActor.addLoot(SimGlobals.m.coins_for_planting);
			MusicBox.playSound("event:/SFX/CIVILIZATIONS/PlantCrops", pActor.beh_tile_target, pGameViewOnly: true);
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
