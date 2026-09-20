public class KingdomSelectedMetaBanners : KingdomMetaBanners, ISelectedTabBanners<Kingdom>
{
	public void update(Kingdom pKingdom)
	{
		meta_object = pKingdom;
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
