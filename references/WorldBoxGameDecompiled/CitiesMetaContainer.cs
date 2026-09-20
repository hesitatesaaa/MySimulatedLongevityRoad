using System;
using System.Collections.Generic;

public class CitiesMetaContainer : ListMetaContainer<CityListElement, City, CityData>
{
	protected override IEnumerable<City> getMetaList()
	{
		return getMeta().getCities();
	}

	protected override Comparison<City> getSorting()
	{
		return ComponentListBase<CityListElement, City, CityData, CityListComponent>.sortByPopulation;
	}
}
