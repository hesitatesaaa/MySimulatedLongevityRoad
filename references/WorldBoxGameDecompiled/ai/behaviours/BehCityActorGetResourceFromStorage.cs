namespace ai.behaviours;

public class BehCityActorGetResourceFromStorage : BehCityActor
{
	private string _resource_id;

	private int _amount;

	public BehCityActorGetResourceFromStorage(string pResourceId, int pAmount)
	{
		_resource_id = pResourceId;
		_amount = pAmount;
	}

	public override BehResult execute(Actor pActor)
	{
		City city = pActor.city;
		if (!city.hasStorages())
		{
			return BehResult.Stop;
		}
		if (city.getResourcesAmount(_resource_id) < _amount)
		{
			return BehResult.Stop;
		}
		city.takeResource(_resource_id, _amount);
		pActor.addToInventory(_resource_id, _amount);
		return BehResult.Continue;
	}
}
