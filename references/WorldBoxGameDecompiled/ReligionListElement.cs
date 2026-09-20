using UnityEngine.UI;

public class ReligionListElement : WindowListElementBase<Religion, ReligionData>
{
	public Text text_name;

	public CountUpOnClick text_age;

	public CountUpOnClick text_population;

	public CountUpOnClick text_renown;

	public CountUpOnClick text_villages;

	public CountUpOnClick text_kingdom;

	internal override void show(Religion pReligion)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.show(pReligion);
		text_name.text = pReligion.name;
		((Graphic)text_name).color = pReligion.getColor().getColorText();
		text_age.setValue(pReligion.getAge());
		text_population.setValue(pReligion.countUnits());
		text_villages.setValue(pReligion.countCities());
		text_kingdom.setValue(pReligion.countKingdoms());
		text_renown.setValue(pReligion.getRenown());
	}

	protected override void tooltipAction()
	{
		Tooltip.show(this, "religion", new TooltipData
		{
			religion = meta_object
		});
	}

	protected override ActorAsset getActorAsset()
	{
		return meta_object.getActorAsset();
	}
}
