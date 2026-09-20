using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CultureListElement : WindowListElementBase<Culture, CultureData>
{
	public Text name;

	public CountUpOnClick textFollowers;

	public CountUpOnClick textCities;

	public CountUpOnClick textRenown;

	public CountUpOnClick textAge;

	public CountUpOnClick textBooks;

	internal override void show(Culture pCulture)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.show(pCulture);
		name.text = pCulture.data.name;
		((Graphic)name).color = pCulture.getColor().getColorText();
		textAge.setValue(pCulture.getAge());
		textFollowers.setValue(pCulture.countUnits());
		textRenown.setValue(pCulture.getRenown());
		textCities.setValue(pCulture.countCities());
		textBooks.setValue(pCulture.books.count());
	}

	protected override void OnDisable()
	{
		ShortcutExtensions.DOKill((Component)(object)textFollowers, false);
		ShortcutExtensions.DOKill((Component)(object)textCities, false);
		ShortcutExtensions.DOKill((Component)(object)textRenown, false);
		ShortcutExtensions.DOKill((Component)(object)textAge, false);
		ShortcutExtensions.DOKill((Component)(object)textBooks, false);
		base.OnDisable();
	}

	protected override void tooltipAction()
	{
		Tooltip.show(this, "culture", new TooltipData
		{
			culture = meta_object
		});
	}

	protected override ActorAsset getActorAsset()
	{
		return meta_object.getActorAsset();
	}
}
