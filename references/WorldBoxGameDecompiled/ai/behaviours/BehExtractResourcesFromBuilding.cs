namespace ai.behaviours;

public class BehExtractResourcesFromBuilding : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_building_target = true;
		check_building_target_non_usable = true;
	}

	public override BehResult execute(Actor pActor)
	{
		BuildingAsset asset = pActor.beh_building_target.asset;
		pActor.beh_building_target.extractResources(pActor);
		if (asset.resources_given != null)
		{
			foreach (ResourceContainer item in asset.resources_given)
			{
				int num = item.amount;
				if (asset.building_type == BuildingType.Building_Mineral && pActor.hasTrait("miner") && Randy.randomBool())
				{
					num++;
				}
				pActor.addToInventory(item.id, num);
			}
		}
		return BehResult.Continue;
	}
}
