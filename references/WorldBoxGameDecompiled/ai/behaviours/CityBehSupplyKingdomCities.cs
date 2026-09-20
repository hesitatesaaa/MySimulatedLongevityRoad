namespace ai.behaviours;

public class CityBehSupplyKingdomCities : BehaviourActionCity
{
	public override BehResult execute(City pCity)
	{
		if (!pCity.hasCulture())
		{
			return BehResult.Stop;
		}
		if (pCity.kingdom.countCities() == 1)
		{
			return BehResult.Stop;
		}
		if (pCity.data.timer_supply > 0f)
		{
			return BehResult.Stop;
		}
		if (!pCity.hasStockpiles())
		{
			return BehResult.Stop;
		}
		Building randomStockpile = pCity.getRandomStockpile();
		if (randomStockpile == null)
		{
			return BehResult.Stop;
		}
		using ListPool<CityStorageSlot> listPool = new ListPool<CityStorageSlot>();
		foreach (CityStorageSlot slot in randomStockpile.resources.getSlots())
		{
			if (slot.amount > slot.asset.supply_bound_give)
			{
				listPool.Add(slot);
			}
		}
		if (listPool.Count == 0)
		{
			return BehResult.Stop;
		}
		foreach (City item in pCity.kingdom.getCities().LoopRandom())
		{
			if (item == pCity)
			{
				continue;
			}
			foreach (CityStorageSlot item2 in listPool.LoopRandom())
			{
				if (item.getResourcesAmount(item2.id) < item2.asset.supply_bound_take)
				{
					shareResource(pCity, item, item2);
					updateSupplyTimer(pCity);
					return BehResult.Continue;
				}
			}
		}
		return BehResult.Continue;
	}

	private void updateSupplyTimer(City pCity)
	{
		float num = 30f;
		if (pCity.hasLeader())
		{
			num *= pCity.leader.stats["multiplier_supply_timer"];
		}
		if (num < 10f)
		{
			num = 10f;
		}
		pCity.data.timer_supply = num;
	}

	private void shareResource(City pCity, City pTargetCity, CityStorageSlot pSlot)
	{
		pCity.takeResource(pSlot.id, pSlot.asset.supply_give);
		pTargetCity.addResourcesToRandomStockpile(pSlot.id, pSlot.asset.supply_give);
	}
}
