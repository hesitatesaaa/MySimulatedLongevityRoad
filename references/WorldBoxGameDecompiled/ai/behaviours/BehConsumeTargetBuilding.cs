namespace ai.behaviours;

public class BehConsumeTargetBuilding : BehActorUsableBuildingTarget
{
	public override BehResult execute(Actor pActor)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (pActor.beh_building_target.asset.type == "type_fruits")
		{
			if (pActor.beh_building_target.hasResourcesToCollect())
			{
				pActor.beh_building_target.extractResources(pActor);
				pActor.addNutritionFromEating(pActor.beh_building_target.asset.nutrition_restore, pSetMaxNutrition: false, pSetJustAte: true);
				pActor.countConsumed();
			}
		}
		else if (pActor.beh_building_target.isAlive())
		{
			pActor.beh_building_target.startDestroyBuilding();
			pActor.addNutritionFromEating(pActor.beh_building_target.asset.nutrition_restore, pSetMaxNutrition: false, pSetJustAte: true);
			pActor.countConsumed();
		}
		WorldTile current_tile = pActor.beh_building_target.current_tile;
		pActor.punchTargetAnimation(current_tile.posV3, pFlip: false);
		return BehResult.Continue;
	}
}
