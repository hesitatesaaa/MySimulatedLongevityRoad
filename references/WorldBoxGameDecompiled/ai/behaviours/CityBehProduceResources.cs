namespace ai.behaviours;

public class CityBehProduceResources : BehaviourActionCity
{
	public override bool shouldRetry(City pCity)
	{
		return false;
	}

	public override BehResult execute(City pCity)
	{
		ActorAsset actorAsset = pCity.getActorAsset();
		if (actorAsset.production == null)
		{
			return BehResult.Stop;
		}
		foreach (string item in actorAsset.production.LoopRandom())
		{
			ResourceAsset resourceAsset = AssetManager.resources.get(item);
			int resourcesAmount = pCity.getResourcesAmount(item);
			if (resourcesAmount <= resourceAsset.maximum)
			{
				int pAmount = pCity.status.population / 10 + 1;
				if (resourcesAmount < resourceAsset.produce_min)
				{
					pAmount = resourceAsset.produce_min;
				}
				tryToProduce(resourceAsset, pCity, pAmount);
			}
		}
		return BehResult.Continue;
	}

	private bool tryToProduce(ResourceAsset pAsset, City pCity, int pAmount = 1)
	{
		for (int i = 0; i < pAmount; i++)
		{
			if (pCity.getResourcesAmount(pAsset.id) == pAsset.maximum)
			{
				return false;
			}
			string[] ingredients = pAsset.ingredients;
			foreach (string pResourceID in ingredients)
			{
				if (pCity.getResourcesAmount(pResourceID) < pAsset.ingredients_amount)
				{
					return false;
				}
			}
			ingredients = pAsset.ingredients;
			foreach (string pResourceID2 in ingredients)
			{
				pCity.takeResource(pResourceID2, pAsset.ingredients_amount);
			}
			if (pCity.addResourcesToRandomStockpile(pAsset.id) <= 0)
			{
				return false;
			}
		}
		return true;
	}
}
