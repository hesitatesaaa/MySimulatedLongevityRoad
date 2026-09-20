using UnityEngine;
using UnityEngine.UI;

public class KingdomListElement : WindowListElementBase<Kingdom, KingdomData>
{
	public CountUpOnClick textAge;

	public CountUpOnClick textPopulation;

	public CountUpOnClick textArmy;

	public CountUpOnClick textCities;

	public CountUpOnClick textHouses;

	public CountUpOnClick textZones;

	public Text kingdomName;

	public GameObject buttonCapital;

	public GameObject buttonKing;

	public UiUnitAvatarElement avatarLoader;

	internal override void show(Kingdom pKingdom)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		base.show(pKingdom);
		kingdomName.text = pKingdom.name;
		Color colorText = pKingdom.getColor().getColorText();
		((Graphic)kingdomName).color = colorText;
		avatarLoader.show(pKingdom.king);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (City city in pKingdom.getCities())
		{
			num3++;
			num += city.zones.Count;
			num2 += city.buildings.Count;
		}
		textPopulation.setValue(pKingdom.getPopulationPeople());
		textArmy.setValue(pKingdom.countTotalWarriors());
		textZones.setValue(num);
		textHouses.setValue(num2);
		textCities.setValue(num3, "/" + pKingdom.getMaxCities());
		textAge.setValue(pKingdom.getAge());
	}

	protected override void tooltipAction()
	{
		Tooltip.show(this, "kingdom", new TooltipData
		{
			kingdom = meta_object
		});
	}

	protected override ActorAsset getActorAsset()
	{
		return meta_object.getActorAsset();
	}
}
