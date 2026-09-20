using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllianceListElement : WindowListElementBase<Alliance, AllianceData>
{
	public Text text_name;

	public CountUpOnClick age;

	public CountUpOnClick population;

	public CountUpOnClick warriors;

	public CountUpOnClick villages;

	public CountUpOnClick kingdoms;

	public Text level;

	public KingdomBanner prefabMiniKingdomBanner;

	public GameObject grid;

	private ObjectPoolGenericMono<KingdomBanner> pool_mini_banners;

	internal override void show(Alliance pAlliance)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		base.show(pAlliance);
		text_name.text = meta_object.name;
		((Graphic)text_name).color = meta_object.getColor().getColorText();
		age.setValue(meta_object.getAge());
		population.setValue(meta_object.countPopulation());
		warriors.setValue(meta_object.countWarriors());
		villages.setValue(meta_object.countCities());
		kingdoms.setValue(meta_object.countKingdoms());
		showKingdomBanners(meta_object.kingdoms_list);
	}

	public void showKingdomBanners(List<Kingdom> pList)
	{
		if (pool_mini_banners == null)
		{
			pool_mini_banners = new ObjectPoolGenericMono<KingdomBanner>(prefabMiniKingdomBanner, grid.transform);
		}
		pool_mini_banners.clear();
		foreach (Kingdom p in pList)
		{
			KingdomBanner next = pool_mini_banners.getNext();
			next.load(p);
			((Behaviour)((Component)next).GetComponentInChildren<RotateOnHover>()).enabled = false;
		}
	}

	protected override void tooltipAction()
	{
		Tooltip.show(this, "alliance", new TooltipData
		{
			alliance = meta_object
		});
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		pool_mini_banners?.clear();
	}
}
