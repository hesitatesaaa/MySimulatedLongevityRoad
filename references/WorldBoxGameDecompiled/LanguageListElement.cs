using UnityEngine.UI;

public class LanguageListElement : WindowListElementBase<Language, LanguageData>
{
	public Text text_name;

	public CountUpOnClick text_age;

	public CountUpOnClick text_population;

	public CountUpOnClick text_books;

	public CountUpOnClick text_villages;

	public CountUpOnClick text_kingdom;

	internal override void show(Language pLanguage)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.show(pLanguage);
		text_name.text = pLanguage.name;
		((Graphic)text_name).color = pLanguage.getColor().getColorText();
		text_age.setValue(pLanguage.getAge());
		text_population.setValue(pLanguage.countUnits());
		text_villages.setValue(pLanguage.countCities());
		text_kingdom.setValue(pLanguage.countKingdoms());
		text_books.setValue(pLanguage.books.count());
	}

	protected override void tooltipAction()
	{
		Tooltip.show(this, "language", new TooltipData
		{
			language = meta_object
		});
	}

	protected override ActorAsset getActorAsset()
	{
		return meta_object.getActorAsset();
	}
}
