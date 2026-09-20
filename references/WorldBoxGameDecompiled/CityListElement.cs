using UnityEngine;
using UnityEngine.UI;

public class CityListElement : WindowListElementBase<City, CityData>
{
	public Text text_name;

	public CountUpOnClick population;

	public CountUpOnClick army;

	public CountUpOnClick zones;

	[SerializeField]
	private CountUpOnClick _loyalty;

	public CountUpOnClick age;

	public UiUnitAvatarElement avatarLoader;

	public CityBanner city_banner;

	[SerializeField]
	private GameObject _icon_capital;

	[SerializeField]
	private CityLoyaltyElement _loyalty_element;

	internal override void show(City pCity)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		base.show(pCity);
		avatarLoader.show(pCity.leader);
		_loyalty_element.setCity(pCity);
		text_name.text = pCity.name;
		((Graphic)text_name).color = pCity.kingdom.getColor().getColorText();
		population.setValue(pCity.getPopulationPeople());
		army.setValue(pCity.countWarriors());
		zones.setValue(pCity.zones.Count);
		int loyalty = pCity.getLoyalty(pForceRecalc: true);
		_loyalty.setValue(loyalty);
		if (loyalty < 0)
		{
			((Graphic)_loyalty.getText()).color = Toolbox.color_negative_RGBA;
		}
		else
		{
			((Graphic)_loyalty.getText()).color = Toolbox.color_positive_RGBA;
		}
		age.setValue(pCity.getAge());
		toggleCapital(pCity.isCapitalCity());
	}

	protected override void initMonoFields()
	{
	}

	protected override void loadBanner()
	{
		city_banner.load(meta_object);
	}

	protected override void tooltipAction()
	{
		Tooltip.show(this, "city", new TooltipData
		{
			city = meta_object
		});
	}

	private void toggleCapital(bool pState)
	{
		GameObject icon_capital = _icon_capital;
		if (icon_capital != null)
		{
			icon_capital.SetActive(pState);
		}
	}

	protected override ActorAsset getActorAsset()
	{
		return meta_object.getActorAsset();
	}
}
