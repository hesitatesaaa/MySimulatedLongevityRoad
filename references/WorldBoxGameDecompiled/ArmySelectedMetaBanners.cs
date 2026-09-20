public class ArmySelectedMetaBanners : ArmyMetaBanners, ISelectedTabBanners<Army>
{
	public void update(Army pArmy)
	{
		meta_object = pArmy;
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
		return base.visible_banners;
	}
}
