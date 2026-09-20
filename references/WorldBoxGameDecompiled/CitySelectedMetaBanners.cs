public class CitySelectedMetaBanners : CityMetaBanners, ISelectedTabBanners<City>
{
	public void update(City pCity)
	{
		meta_object = pCity;
		clear();
		foreach (MetaBannerElement banner in banners)
		{
			if (banner.check())
			{
				metaBannerShow(banner);
			}
		}
	}

	protected override void OnEnable()
	{
	}

	public int countVisibleBanners()
	{
		return visible_banners;
	}
}
