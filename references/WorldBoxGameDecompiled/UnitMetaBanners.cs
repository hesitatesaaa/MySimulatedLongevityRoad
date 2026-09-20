using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMetaBanners : UnitElement, IBaseMetaBanners
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
	private KingdomBanner _banner_kingdom;

	[SerializeField]
	private SubspeciesBanner _banner_subspecies;

	[SerializeField]
	private FamilyBanner _banner_family;

	[SerializeField]
	private PlotBanner _banner_plot;

	[SerializeField]
	private ArmyBanner _banner_army;

	protected List<MetaBannerElement> _banners = new List<MetaBannerElement>();

	private const float DELAY = 0.025f;

	private int _visible_banners;

	public int visible_banners => _visible_banners;

	protected override void Awake()
	{
		base.Awake();
		_banners.Add(new MetaBannerElement
		{
			banner = _banner_kingdom,
			check = () => actor.isKingdomCiv(),
			nano = () => actor.kingdom
		});
		_banners.Add(new MetaBannerElement
		{
			banner = _banner_clan,
			check = () => actor.hasClan(),
			nano = () => actor.clan
		});
		_banners.Add(new MetaBannerElement
		{
			banner = _banner_alliance,
			check = () => actor.kingdom.hasAlliance(),
			nano = () => actor.kingdom.getAlliance()
		});
		_banners.Add(new MetaBannerElement
		{
			banner = _banner_language,
			check = () => actor.hasLanguage(),
			nano = () => actor.language
		});
		_banners.Add(new MetaBannerElement
		{
			banner = _banner_culture,
			check = () => actor.hasCulture(),
			nano = () => actor.culture
		});
		_banners.Add(new MetaBannerElement
		{
			banner = _banner_religion,
			check = () => actor.hasReligion(),
			nano = () => actor.religion
		});
		_banners.Add(new MetaBannerElement
		{
			banner = _banner_subspecies,
			check = () => actor.hasSubspecies(),
			nano = () => actor.subspecies
		});
		_banners.Add(new MetaBannerElement
		{
			banner = _banner_family,
			check = () => actor.hasFamily(),
			nano = () => actor.family
		});
		_banners.Add(new MetaBannerElement
		{
			banner = _banner_plot,
			check = () => actor.hasPlot(),
			nano = () => actor.plot
		});
		_banners.Add(new MetaBannerElement
		{
			banner = _banner_city,
			check = () => actor.hasCity(),
			nano = () => actor.getCity()
		});
		_banners.Add(new MetaBannerElement
		{
			banner = _banner_army,
			check = () => actor.hasArmy(),
			nano = () => actor.army
		});
		((IBaseMetaBanners)this).enableClickAnimation();
	}

	protected override IEnumerator showContent()
	{
		_banners.Sort((MetaBannerElement x, MetaBannerElement y) => ((Component)x.banner).transform.GetSiblingIndex().CompareTo(((Component)y.banner).transform.GetSiblingIndex()));
		yield return (object)new WaitForSecondsRealtime(0.025f);
		foreach (MetaBannerElement banner in _banners)
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
		foreach (MetaBannerElement banner in _banners)
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
		return _banners;
	}
}
