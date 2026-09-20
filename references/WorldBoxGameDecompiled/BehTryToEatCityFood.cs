using ai.behaviours;

public class BehTryToEatCityFood : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		City city = pActor.city;
		if (!city.hasSuitableFood(pActor.subspecies))
		{
			return BehResult.Stop;
		}
		ResourceAsset foodItem = city.getFoodItem(pActor.subspecies, pActor.data.favorite_food);
		bool flag = !pActor.isFoodFreeForThisPerson();
		if (foodItem != null)
		{
			if (flag && !pActor.hasEnoughMoney(foodItem.money_cost))
			{
				return BehResult.Stop;
			}
			eatFood(pActor, city, foodItem, flag);
			if (pActor.hasTrait("gluttonous"))
			{
				foodItem = city.getFoodItem(pActor.subspecies, pActor.data.favorite_food);
				if (foodItem != null && flag && pActor.hasEnoughMoney(foodItem.money_cost))
				{
					eatFood(pActor, city, foodItem, pNeedToPay: true);
				}
			}
		}
		return BehResult.Continue;
	}

	private void eatFood(Actor pActor, City pCity, ResourceAsset pFoodItem, bool pNeedToPay)
	{
		if (pNeedToPay)
		{
			pActor.spendMoney(pFoodItem.money_cost);
		}
		pCity.eatFoodItem(pFoodItem.id);
		pActor.consumeFoodResource(pFoodItem);
	}
}
