namespace ai.behaviours;

public class BehCityActorFertilizeCrop : BehActorUsableBuildingTarget
{
	public override BehResult execute(Actor pActor)
	{
		Building beh_building_target = pActor.beh_building_target;
		if (beh_building_target.component_wheat.isMaxLevel())
		{
			return BehResult.Stop;
		}
		if (pActor.inventory.getResource("fertilizer") == 0)
		{
			return BehResult.Stop;
		}
		pActor.takeFromInventory("fertilizer", 1);
		beh_building_target.component_wheat.growFull();
		pActor.addLoot(SimGlobals.m.coins_for_fertilize);
		return BehResult.Continue;
	}
}
