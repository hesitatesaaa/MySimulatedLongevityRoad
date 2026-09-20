using ai.behaviours;

public class BehRepairEquipment : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		bool flag = false;
		foreach (ActorEquipmentSlot item2 in pActor.equipment)
		{
			if (item2.isEmpty())
			{
				continue;
			}
			Item item = item2.getItem();
			if (item.needRepair())
			{
				int pCost = (int)((float)item2.getItem().getAsset().cost_gold * SimGlobals.m.item_repair_cost_multiplier);
				if (pActor.hasEnoughMoney(pCost))
				{
					pActor.spendMoney(pCost);
					item.fullRepair();
					flag = true;
				}
			}
		}
		if (flag)
		{
			pActor.setStatsDirty();
		}
		return BehResult.Continue;
	}
}
