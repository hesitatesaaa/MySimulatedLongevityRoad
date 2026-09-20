using UnityEngine;

public class KingdomSelectedContainerCities : SelectedElementBase<CityBanner>
{
	[SerializeField]
	private CityBanner _banner_prefab;

	private void Awake()
	{
		_pool = new ObjectPoolGenericMono<CityBanner>(_banner_prefab, _grid);
		((Component)_grid).gameObject.AddOrGetComponent<TraitsGrid>();
	}

	public void update(NanoObject pNano)
	{
		refresh(pNano);
	}

	protected override void refresh(NanoObject pNano)
	{
		clear();
		foreach (City city in ((Kingdom)pNano).getCities())
		{
			addBanner(city);
		}
	}

	private void addBanner(City pCity)
	{
		_pool.getNext().load(pCity);
	}
}
