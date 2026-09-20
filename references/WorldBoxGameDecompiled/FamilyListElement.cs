using UnityEngine;
using UnityEngine.UI;

public class FamilyListElement : WindowListElementBase<Family, FamilyData>
{
	public Text text_name;

	public CountUpOnClick text_age;

	public CountUpOnClick text_population;

	public CountUpOnClick text_adults;

	public CountUpOnClick text_children;

	public CountUpOnClick text_dead;

	[SerializeField]
	private Text _collective_term;

	internal override void show(Family pFamily)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.show(pFamily);
		text_name.text = pFamily.name;
		((Graphic)text_name).color = pFamily.getColor().getColorText();
		text_age.setValue(pFamily.getAge());
		text_population.setValue(pFamily.countUnits());
		text_adults.setValue(pFamily.countAdults());
		text_children.setValue(pFamily.countChildren());
		text_dead.setValue((int)pFamily.getTotalDeaths());
		string text = LocalizedTextManager.getText(pFamily.getActorAsset().getCollectiveTermID());
		_collective_term.text = text;
	}

	protected override void tooltipAction()
	{
		Tooltip.show(this, "family", new TooltipData
		{
			family = meta_object
		});
	}

	protected override ActorAsset getActorAsset()
	{
		return meta_object.getActorAsset();
	}
}
