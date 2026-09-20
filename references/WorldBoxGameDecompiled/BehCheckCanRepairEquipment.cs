using ai.behaviours;

public class BehCheckCanRepairEquipment : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasEquipment())
		{
			return BehResult.Stop;
		}
		bool flag = false;
		foreach (ActorEquipmentSlot item in pActor.equipment)
		{
			if (item.getItem().needRepair())
			{
				int num = (int)((float)item.getItem().getAsset().cost_gold * SimGlobals.m.item_repair_cost_multiplier);
				if (pActor.money >= num)
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
