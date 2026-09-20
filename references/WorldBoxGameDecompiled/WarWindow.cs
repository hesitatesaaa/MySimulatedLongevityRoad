using UnityEngine;

public class WarWindow : WindowMetaGeneric<War, WarData>
{
	[SerializeField]
	private WindowMetaTab _button_interesting_persons_tab;

	public override MetaType meta_type => MetaType.War;

	protected override War meta_object => SelectedMetas.selected_war;

	internal override void showStatsRows()
	{
		War war = meta_object;
		tryShowPastNames();
		showStatRow("war_type", LocalizedTextManager.getText(war.getAsset().localized_war_name), MetaType.None, -1L, "iconWar");
		showStatRow("started_at", war.getFoundedDate(), MetaType.None, -1L, "iconAge");
		if (war.hasEnded())
		{
			showStatRow("war_ended_at", war.getYearEnded().ToString() ?? "", MetaType.None, -1L, "iconClose");
		}
		showStatRow("war_duration", war.getDuration().ToString() ?? "", MetaType.None, -1L, "iconClock");
		string pValue = war.data.winner.getLocaleID().Localize();
		switch (war.data.winner)
		{
		case WarWinner.Attackers:
			showStatRow("war_winner", pValue, war.getAttackersColorTextString(), MetaType.None, -1L, pColorText: true, "iconAttackRate");
			break;
		case WarWinner.Defenders:
			showStatRow("war_winner", pValue, war.getDefendersColorTextString(), MetaType.None, -1L, pColorText: true, "iconAttackRate");
			break;
		case WarWinner.Peace:
			showStatRow("war_outcome", pValue, MetaType.None, -1L, "actor_traits/iconPeaceful");
			break;
		case WarWinner.Merged:
			showStatRow("war_outcome", pValue, MetaType.None, -1L, "iconBre");
			break;
		}
		tryToShowActor("instigator", war.data.started_by_actor_id, war.data.started_by_actor_name, null, "worldrules/icon_angryvillagers");
		tryToShowMetaKingdom("instigator_from", war.data.started_by_kingdom_id, war.data.started_by_kingdom_name);
		showStatRow("kingdoms", war.countKingdoms().ToString(), MetaType.None, -1L, "iconKingdomList");
		showStatRow("villages", war.countCities().ToString(), MetaType.None, -1L, "iconVillages");
		showStatRow("deaths", war.getTotalDeaths().ToString() ?? "", MetaType.None, -1L, "iconDead");
		showStatRow("attackers_army", war.countAttackersWarriors(), MetaType.None, -1L, "iconArmyAttackers");
		showStatRow("attackers_population", war.countAttackersPopulation(), MetaType.None, -1L, "iconPopulationAttackers");
		showStatRow("attackers_deaths", war.getDeadAttackers(), MetaType.None, -1L, "iconDeathAttackers");
		showStatRow("attackers_cities", war.countAttackersCities(), MetaType.None, -1L, "iconVillages");
		showStatRow("defenders_army", war.countDefendersWarriors(), MetaType.None, -1L, "iconArmyDefenders");
		showStatRow("defenders_population", war.countDefendersPopulation(), MetaType.None, -1L, "iconPopulationDefenders");
		showStatRow("defenders_deaths", war.getDeadDefenders(), MetaType.None, -1L, "iconDeathDefenders");
		showStatRow("defenders_cities", war.countDefendersCities(), MetaType.None, -1L, "iconVillages");
		AchievementLibrary.ancient_war_of_geometry_and_evil.checkBySignal();
	}

	public override void startShowingWindow()
	{
		base.startShowingWindow();
		if (!meta_object.hasEnded())
		{
			_button_interesting_persons_tab.toggleActive(pState: true);
		}
		else
		{
			_button_interesting_persons_tab.toggleActive(pState: false);
		}
		if ((Object)(object)base.tabs.getActiveTab() == (Object)(object)_button_interesting_persons_tab && meta_object.hasEnded())
		{
			showTab(base.tabs.tab_default);
		}
	}
}
