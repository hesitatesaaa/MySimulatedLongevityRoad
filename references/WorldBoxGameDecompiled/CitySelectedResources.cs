public class CitySelectedResources : UICityResources
{
	public void update(City pCity)
	{
		meta_object = pCity;
		showResources();
	}

	protected override void OnEnable()
	{
	}

	protected override void onListChange()
	{
		if (base.city != null)
		{
			base.onListChange();
		}
	}
}
