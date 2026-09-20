public class BuildingBiomeFoodProducer : BaseBuildingComponent
{
	private const float FOOD_INTERVAL = 90f;

	private float timer = 90f;

	public override void update(float pElapsed)
	{
		if (building.city == null || !building.isUsable())
		{
			return;
		}
		if (timer > 0f)
		{
			timer -= pElapsed;
			return;
		}
		timer = 90f;
		WorldTile random = building.tiles.GetRandom();
		string food_resource = random.Type.food_resource;
		if (string.IsNullOrEmpty(food_resource))
		{
			food_resource = random.main_type.food_resource;
		}
		if (!string.IsNullOrEmpty(food_resource) && building.city.getResourcesAmount(food_resource) < 10)
		{
			building.city.addResourcesToRandomStockpile(food_resource);
		}
	}
}
