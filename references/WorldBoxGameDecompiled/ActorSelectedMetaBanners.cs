public class ActorSelectedMetaBanners : UnitMetaBanners, ISelectedTabBanners<Actor>
{
	public void update(Actor pActor)
	{
		setActor(pActor);
		clear();
		foreach (MetaBannerElement banner in _banners)
		{
			if (banner.check())
			{
				metaBannerShow(banner);
			}
		}
	}

	protected override void checkSetActor()
	{
	}

	protected override void OnEnable()
	{
	}

	protected override void checkSetWindow()
	{
	}

	public int countVisibleBanners()
	{
		return base.visible_banners;
	}
}
