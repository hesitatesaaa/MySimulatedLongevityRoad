using UnityEngine;
using UnityEngine.UI;

public class ClanListElement : WindowListElementBase<Clan, ClanData>
{
	public Text text_name;

	public CountUpOnClick members;

	public CountUpOnClick dead;

	public CountUpOnClick age;

	public CountUpOnClick renown;

	public UiUnitAvatarElement avatarLoader;

	internal override void show(Clan pClan)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		base.show(pClan);
		Actor chief = pClan.getChief();
		if (chief.isRekt())
		{
			((Component)avatarLoader).gameObject.SetActive(false);
		}
		else
		{
			((Component)avatarLoader).gameObject.SetActive(true);
			avatarLoader.show(chief);
		}
		text_name.text = pClan.name;
		((Graphic)text_name).color = pClan.getColor().getColorText();
		members.setValue(pClan.countUnits());
		renown.setValue(pClan.getRenown());
		int pValue = pClan.getAge();
		age.setValue(pValue);
		dead.setValue((int)pClan.getTotalDeaths());
	}

	protected override void tooltipAction()
	{
		Tooltip.show(this, "clan", new TooltipData
		{
			clan = meta_object
		});
	}

	protected override ActorAsset getActorAsset()
	{
		return meta_object.getActorAsset();
	}
}
