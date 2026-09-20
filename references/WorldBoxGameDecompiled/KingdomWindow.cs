using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KingdomWindow : WindowMetaGeneric<Kingdom, KingdomData>, ITraitWindow<KingdomTrait, KingdomTraitButton>, IAugmentationsWindow<ITraitsEditor<KingdomTrait>>
{
	public Image raceTopIcon1;

	public Image raceTopIcon2;

	public NameInput mottoInput;

	public override MetaType meta_type => MetaType.Kingdom;

	protected override Kingdom meta_object => SelectedMetas.selected_kingdom;

	protected override void initNameInput()
	{
		base.initNameInput();
		mottoInput.addListener(applyInputMotto);
	}

	protected override bool onNameChange(string pInput)
	{
		if (!base.onNameChange(pInput))
		{
			return false;
		}
		long iD = meta_object.getID();
		string name = meta_object.data.name;
		foreach (War war in World.world.wars)
		{
			if (!war.isRekt() && war.data.started_by_kingdom_id == iD)
			{
				war.data.started_by_kingdom_name = name;
			}
		}
		foreach (Alliance alliance in World.world.alliances)
		{
			if (!alliance.isRekt() && alliance.data.founder_kingdom_id == iD)
			{
				alliance.data.founder_kingdom_name = name;
			}
		}
		foreach (Religion religion in World.world.religions)
		{
			if (!religion.isRekt() && religion.data.creator_kingdom_id == iD)
			{
				religion.data.creator_kingdom_name = name;
			}
		}
		foreach (Culture culture in World.world.cultures)
		{
			if (!culture.isRekt() && culture.data.creator_kingdom_id == iD)
			{
				culture.data.creator_kingdom_name = name;
			}
		}
		foreach (Clan clan in World.world.clans)
		{
			if (!clan.isRekt() && clan.data.founder_kingdom_id == iD)
			{
				clan.data.founder_kingdom_name = name;
			}
		}
		foreach (Language language in World.world.languages)
		{
			if (!language.isRekt() && language.data.creator_kingdom_id == iD)
			{
				language.data.creator_kingdom_name = name;
			}
		}
		foreach (Family family in World.world.families)
		{
			if (!family.isRekt() && family.data.founder_kingdom_id == iD)
			{
				family.data.founder_kingdom_name = name;
			}
		}
		foreach (Book book in World.world.books)
		{
			if (!book.isRekt() && book.data.author_kingdom_id == iD)
			{
				book.data.author_kingdom_name = name;
			}
		}
		foreach (Item item in World.world.items)
		{
			if (!item.isRekt() && item.data.creator_kingdom_id == iD)
			{
				item.data.from = name;
			}
		}
		foreach (Army army in World.world.armies)
		{
			if (!army.isRekt() && army.getKingdom() == meta_object)
			{
				army.onKingdomNameChange();
			}
		}
		return true;
	}

	private void applyInputMotto(string pInput)
	{
		if (pInput != null && meta_object != null)
		{
			meta_object.data.motto = pInput;
		}
	}

	protected override void showTopPartInformation()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		base.showTopPartInformation();
		Kingdom kingdom = meta_object;
		if (kingdom != null)
		{
			raceTopIcon1.sprite = kingdom.getSpriteIcon();
			raceTopIcon2.sprite = kingdom.getSpriteIcon();
			mottoInput.setText(kingdom.getMotto());
			((Graphic)mottoInput.textField).color = kingdom.getColor().getColorText();
		}
	}

	private void tryShowPastRulers()
	{
		List<LeaderEntry> past_rulers = meta_object.data.past_rulers;
		if (past_rulers != null && past_rulers.Count > 1)
		{
			showStatRow("past_kings", meta_object.data.past_rulers.Count, MetaType.None, -1L, "iconKingdomList", "past_rulers", getTooltipPastRulers);
		}
	}

	private TooltipData getTooltipPastRulers()
	{
		return new TooltipData
		{
			tip_name = "past_kings",
			meta_type = MetaType.Kingdom,
			past_rulers = new ListPool<LeaderEntry>(meta_object.data.past_rulers)
		};
	}

	internal override void showStatsRows()
	{
		Kingdom kingdom = meta_object;
		tryShowPastNames();
		showStatRow("founded", kingdom.getFoundedDate(), MetaType.None, -1L, "iconAge");
		tryShowPastRulers();
		tryToShowActor("king", -1L, null, kingdom.king, "iconKings");
		Actor pObject = SuccessionTool.findNextHeir(kingdom, kingdom.king);
		tryToShowActor("heir", -1L, null, pObject, "iconChildren");
		if (kingdom.hasKing())
		{
			if (kingdom.king.s_personality != null)
			{
				showStatRow("creature_statistics_personality", kingdom.king.s_personality.getTranslatedName(), MetaType.None, -1L, "actor_traits/iconStupid");
			}
			int yearsSince = Date.getYearsSince(kingdom.data.timestamp_king_rule);
			showStatRow("kingdom_statistics_king_ruled", yearsSince, MetaType.None, -1L, "iconClock");
			showStatRow("ruler_money", kingdom.king.money, "#43FF43", MetaType.None, -1L, pColorText: false, "iconMoney");
		}
		string pValue = kingdom.getTaxRateTribute().ToString("0%");
		showStatRow("tribute", pValue, "#43FF43", MetaType.None, -1L, pColorText: false, "kingdom_traits/kingdom_trait_tax_rate_tribute_high");
		tryToShowMetaSpecies("founder_species", kingdom.getFounderSpecies().id);
	}

	public override void showMetaRows()
	{
		Kingdom kingdom = meta_object;
		meta_rows_container.tryToShowMetaAlliance("alliance", -1L, null, kingdom.getAlliance());
		meta_rows_container.tryToShowMetaCity("kingdom_statistics_capital", -1L, null, kingdom.capital, "iconKingdom");
		meta_rows_container.tryToShowMetaClan("clan", -1L, null, kingdom.king?.clan);
		meta_rows_container.tryToShowMetaCulture("culture", -1L, null, kingdom.culture);
		meta_rows_container.tryToShowMetaLanguage("language", -1L, null, kingdom.language);
		meta_rows_container.tryToShowMetaReligion("religion", -1L, null, kingdom.religion);
		meta_rows_container.tryToShowMetaSubspecies("main_subspecies", -1L, null, kingdom.getMainSubspecies());
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		mottoInput.inputField.DeactivateInputField();
	}

	public void clickCapital()
	{
		SelectedMetas.selected_city = meta_object.capital;
		ScrollWindow.showWindow("city");
	}

	T IAugmentationsWindow<ITraitsEditor<KingdomTrait>>.GetComponentInChildren<T>(bool includeInactive)
	{
		return ((Component)this).GetComponentInChildren<T>(includeInactive);
	}
}
