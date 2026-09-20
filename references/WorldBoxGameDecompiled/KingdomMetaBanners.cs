using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingdomMetaBanners : KingdomElement, IBaseMetaBanners
{
	[SerializeField]
	private CityBanner _banner_city;

	[SerializeField]
	private CultureBanner _banner_culture;

	[SerializeField]
	private AllianceBanner _banner_alliance;

	[SerializeField]
	private LanguageBanner _banner_language;

	[SerializeField]
	private ReligionBanner _banner_religion;

	[SerializeField]
	private ClanBanner _banner_clan;

	[SerializeField]
	private SubspeciesBanner _banner_subspecies;

	protected List<MetaBannerElement> banners = new List<MetaBannerElement>();

	private const float DELAY = 0.025f;

	protected int visible_banners;

	protected override void Awake()
	{
		base.Awake();
		banners.Add(new MetaBannerElement
		{
			banner = _banner_city,
			check = () => base.kingdom.hasCapital(),
			nano = () => base.kingdom.capital
		});
		banners.Add(new MetaBannerElement
		{
			banner = _banner_clan,
			check = () => base.kingdom.getKingClan() != null,
			nano = () => base.kingdom.getKingClan()
		});
		banners.Add(new MetaBannerElement
		{
			banner = _banner_alliance,
			check = () => base.kingdom.hasAlliance(),
			nano = () => base.kingdom.getAlliance()
		});
		banners.Add(new MetaBannerElement
		{
			banner = _banner_language,
			check = () => base.kingdom.hasLanguage(),
			nano = () => base.kingdom.getLanguage()
		});
		banners.Add(new MetaBannerElement
		{
			banner = _banner_culture,
			check = () => base.kingdom.hasCulture(),
			nano = () => base.kingdom.getCulture()
		});
		banners.Add(new MetaBannerElement
		{
			banner = _banner_religion,
			check = () => base.kingdom.hasReligion(),
			nano = () => base.kingdom.getReligion()
		});
		banners.Add(new MetaBannerElement
		{
			banner = _banner_subspecies,
			check = () => base.kingdom.getMainSubspecies() != null,
			nano = () => base.kingdom.getMainSubspecies()
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
		foreach (MetaBannerElement banner in banners)
		{
			metaBannerHide(banner);
		}
		visible_banners = 0;
	}

	public void metaBannerShow(MetaBannerElement pAsset)
	{
		((Component)pAsset.banner).gameObject.SetActive(true);
		pAsset.banner.load(pAsset.nano());
		visible_banners++;
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
