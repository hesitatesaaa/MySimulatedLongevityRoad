using System.Collections.Generic;

public class CitiesBannersContainer : BannersMetaContainer<CityBanner, City, CityData>
{
	protected override IEnumerable<City> getMetaList(IMetaObject pMeta)
	{
		return pMeta.getCities();
	}
}
