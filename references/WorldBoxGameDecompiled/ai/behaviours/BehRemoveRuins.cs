namespace ai.behaviours;

public class BehRemoveRuins : BehActorBuildingTarget
{
	public override BehResult execute(Actor pActor)
	{
		BuildingAsset asset = pActor.beh_building_target.asset;
		switch (asset.building_type)
		{
		case BuildingType.Building_Tree:
			if (asset.hasResourceGiven("wood"))
			{
				pActor.addToInventory("wood", 1);
			}
			break;
		case BuildingType.Building_Civ:
			if (asset.cost.wood > 0)
			{
				pActor.addToInventory("wood", 1);
			}
			if (asset.cost.stone > 0)
			{
				pActor.addToInventory("stone", 1);
			}
			break;
		}
		pActor.beh_building_target.startDestroyBuilding();
		pActor.addLoot(SimGlobals.m.coins_for_cleaning);
		return BehResult.Continue;
	}
}
