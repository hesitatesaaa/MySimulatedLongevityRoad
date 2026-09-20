using UnityEngine;
using UnityEngine.UI;

public class SubspeciesListElement : WindowListElementBase<Subspecies, SubspeciesData>
{
	public Text text_name;

	public CountUpOnClick text_age;

	public CountUpOnClick text_population;

	public CountUpOnClick text_children;

	public CountUpOnClick text_deaths;

	public CountUpOnClick text_family;

	[SerializeField]
	private Text _subspecies_name;

	internal override void show(Subspecies pSubspecies)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.show(pSubspecies);
		text_name.text = pSubspecies.name;
		((Graphic)text_name).color = pSubspecies.getColor().getColorText();
		text_age.setValue(pSubspecies.getAge());
		text_population.setValue(pSubspecies.countUnits());
		text_deaths.setValue((int)pSubspecies.getTotalDeaths());
		text_children.setValue(pSubspecies.countChildren());
		text_family.setValue(pSubspecies.countCurrentFamilies());
		string translatedName = pSubspecies.getActorAsset().getTranslatedName();
		_subspecies_name.text = translatedName;
	}

	protected override void tooltipAction()
	{
		Tooltip.show(this, "subspecies", new TooltipData
		{
			subspecies = meta_object
		});
	}

	protected override ActorAsset getActorAsset()
	{
		return meta_object.getActorAsset();
	}
}
