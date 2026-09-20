using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyMetaBanners : ArmyElement, IBaseMetaBanners
{
	[SerializeField]
	private CityBanner _banner_city;

	[SerializeField]
	private AllianceBanner _banner_alliance;

	[SerializeField]
	private KingdomBanner _banner_kingdom;

	protected List<MetaBannerElement> banners = new List<MetaBannerElement>();

	private const float DELAY = 0.025f;

	private int _visible_banners;

	public int visible_banners => _visible_banners;

	protected override void Awake()
	{
		base.Awake();
		banners.Add(new MetaBannerElement
		{
			banner = _banner_kingdom,
			check = () => base.army.hasKingdom(),
			nano = () => base.army.getKingdom()
		});
		banners.Add(new MetaBannerElement
		{
			banner = _banner_alliance,
			check = () => base.army.hasKingdom() && base.army.getKingdom().hasAlliance(),
			nano = () => base.army.getKingdom().getAlliance()
		});
		banners.Add(new MetaBannerElement
		{
			banner = _banner_city,
			check = () => base.army.hasCity(),
			nano = () => base.army.getCity()
		});
		((IBaseMetaBanners)this).enableClickAnimation();
	}

	protected override IEnumerator showContent()
	{
		banners.Sort((MetaBannerElement x, MetaBannerElement y) => ((Component)x.banner).transform.GetSiblingIndex().CompareTo(((Component)y.banner).transform.GetSiblingIndex()));
		yield return (object)new WaitForSecondsRealtime(0.025f);
		foreach (MetaBannerElement banner in banners)
		{
			if (banner.check())
			{
				track_objects.Add(banner.nano());
				metaBannerShow(banner);
			}
		}
	}

	protected override void clear()
	{
		base.clear();
		_visible_banners = 0;
		foreach (MetaBannerElement banner in banners)
		{
			metaBannerHide(banner);
		}
	}

	public void metaBannerShow(MetaBannerElement pAsset)
	{
		((Component)pAsset.banner).gameObject.SetActive(true);
		pAsset.banner.load(pAsset.nano());
		_visible_banners++;
	}

	public void metaBannerHide(MetaBannerElement pAsset)
	{
		if (((Component)pAsset.banner).gameObject.activeSelf)
		{
			((Component)pAsset.banner).gameObject.SetActive(false);
		}
	}

	public IReadOnlyCollection<MetaBannerElement> getBanners()
	{
		return banners;
	}
}
