using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyWindow : WindowMetaGeneric<Army, ArmyData>
{
	[SerializeField]
	private Image _race_top_left;

	[SerializeField]
	private Image _race_top_right;

	public override MetaType meta_type => MetaType.Army;

	protected override Army meta_object => SelectedMetas.selected_army;

	protected override void showTopPartInformation()
	{
		base.showTopPartInformation();
		Army army = meta_object;
		if (army != null)
		{
			_race_top_left.sprite = army.getSpriteIcon();
			_race_top_right.sprite = army.getSpriteIcon();
		}
	}

	internal override void showStatsRows()
	{
		Army army = meta_object;
		if (army != null)
		{
			tryShowPastNames();
			showStatRow("founded", army.getFoundedDate(), MetaType.None, -1L, "iconAge");
			showStatRow("males", army.countMales(), MetaType.None, -1L, "iconMale");
			showStatRow("females", army.countFemales(), MetaType.None, -1L, "iconFemale");
			showStatRow("deaths", army.getTotalDeaths(), MetaType.None, -1L, "iconDead");
			showStatRow("kills", army.getTotalKills(), MetaType.None, -1L, "iconKills");
			tryToShowActor("captain", -1L, null, army.getCaptain(), "iconCaptain");
			tryShowPastCaptains();
			tryToShowMetaCity("village", -1L, null, army.getCity());
			tryToShowMetaKingdom("kingdom", -1L, null, army.getKingdom());
		}
	}

	private void tryShowPastCaptains()
	{
		List<LeaderEntry> past_captains = meta_object.data.past_captains;
		if (past_captains != null && past_captains.Count > 1)
		{
			showStatRow("past_captains", meta_object.data.past_captains.Count, MetaType.None, -1L, "iconCaptain", "past_rulers", getTooltipPastCaptains);
		}
	}

	private TooltipData getTooltipPastCaptains()
	{
		return new TooltipData
		{
			tip_name = "past_captains",
			meta_type = MetaType.Army,
			past_rulers = new ListPool<LeaderEntry>(meta_object.data.past_captains)
		};
	}
}
