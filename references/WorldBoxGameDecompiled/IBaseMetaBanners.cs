using System.Collections.Generic;
using UnityEngine;

public interface IBaseMetaBanners
{
	void metaBannerShow(MetaBannerElement pAsset);

	void metaBannerHide(MetaBannerElement pAsset);

	IReadOnlyCollection<MetaBannerElement> getBanners();

	void enableClickAnimation()
	{
		foreach (MetaBannerElement banner in getBanners())
		{
			((Component)banner.banner).GetComponent<TipButton>().showOnClick = true;
		}
	}
}
