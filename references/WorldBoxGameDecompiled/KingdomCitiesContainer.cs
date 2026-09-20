using System.Collections;
using UnityEngine;

public class KingdomCitiesContainer : KingdomElement
{
	private ObjectPoolGenericMono<CityListElement> _pool_elements;

	[SerializeField]
	private CityListElement _prefab;

	protected override void Awake()
	{
		_pool_elements = new ObjectPoolGenericMono<CityListElement>(_prefab, ((Component)this).transform);
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		using ListPool<City> tListCities = new ListPool<City>(base.kingdom.getCities());
		track_objects.AddRange(tListCities);
		tListCities.Sort(ComponentListBase<CityListElement, City, CityData, CityListComponent>.sortByPopulation);
		for (int i = 0; i < tListCities.Count; i++)
		{
			City tCity = tListCities[i];
			if (!tCity.isCapitalCity())
			{
				yield return (object)new WaitForSecondsRealtime(0.025f);
				showCityElement(tCity);
			}
		}
	}

	private void showCityElement(City pCity)
	{
		_pool_elements.getNext().show(pCity);
	}

	protected override void clear()
	{
		_pool_elements.clear();
		base.clear();
	}
}
