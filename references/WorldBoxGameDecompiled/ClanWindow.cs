using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanWindow : WindowMetaGeneric<Clan, ClanData>, ITraitWindow<ClanTrait, ClanTraitButton>, IAugmentationsWindow<ITraitsEditor<ClanTrait>>
{
	public NameInput nameInput;

	public NameInput mottoInput;

	public override MetaType meta_type => MetaType.Clan;

	protected override Clan meta_object => SelectedMetas.selected_clan;

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
		foreach (Culture culture in World.world.cultures)
		{
			if (!culture.isRekt() && culture.data.creator_clan_id == iD)
			{
				culture.data.creator_clan_name = name;
			}
		}
		foreach (Religion religion in World.world.religions)
		{
			if (!religion.isRekt() && religion.data.creator_clan_id == iD)
			{
				religion.data.creator_clan_name = name;
			}
		}
		foreach (Language language in World.world.languages)
		{
			if (!language.isRekt() && language.data.creator_clan_id == iD)
			{
				language.data.creator_clan_name = name;
			}
		}
		foreach (Book book in World.world.books)
		{
			if (!book.isRekt() && book.data.author_clan_id == iD)
			{
				book.data.author_clan_name = name;
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
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		base.showTopPartInformation();
		Clan clan = meta_object;
		if (clan != null)
		{
			mottoInput.setText(clan.getMotto());
			((Graphic)mottoInput.textField).color = clan.getColor().getColorText();
		}
	}

	internal override void showStatsRows()
	{
		Clan clan = meta_object;
		tryShowPastNames();
		showStatRow("founded", clan.getFoundedDate(), MetaType.None, -1L, "iconAge");
		tryToShowActor("clan_founder", clan.data.founder_actor_id, clan.data.founder_actor_name, null, "actor_traits/iconStupid");
		tryToShowActor("clan_chief_title", -1L, null, clan.getChief(), "iconClan");
		tryShowPastChiefs();
		Actor nextChief = clan.getNextChief();
		tryToShowActor("clan_heir", -1L, null, nextChief, "iconClanList");
		tryToShowMetaCulture("culture", -1L, null, clan.getClanCulture());
		tryToShowMetaKingdom("origin", clan.data.founder_kingdom_id, clan.data.founder_kingdom_name);
		tryToShowMetaCity("birthplace", clan.data.founder_city_id, clan.data.founder_city_name);
		tryToShowMetaSubspecies("original_subspecies", clan.data.creator_subspecies_id, clan.data.creator_subspecies_name);
		tryToShowMetaSpecies("species", clan.data.creator_species_id);
	}

	private void tryShowPastChiefs()
	{
		List<LeaderEntry> past_chiefs = meta_object.data.past_chiefs;
		if (past_chiefs != null && past_chiefs.Count > 1)
		{
			showStatRow("past_chiefs", meta_object.data.past_chiefs.Count, MetaType.None, -1L, "iconCaptain", "past_rulers", getTooltipPastChiefs);
		}
	}

	private TooltipData getTooltipPastChiefs()
	{
		return new TooltipData
		{
			tip_name = "past_chiefs",
			meta_type = MetaType.Clan,
			past_rulers = new ListPool<LeaderEntry>(meta_object.data.past_chiefs)
		};
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		mottoInput.inputField.DeactivateInputField();
	}

	public void debugClearExpLevel()
	{
		OnEnable();
	}

	T IAugmentationsWindow<ITraitsEditor<ClanTrait>>.GetComponentInChildren<T>(bool includeInactive)
	{
		return ((Component)this).GetComponentInChildren<T>(includeInactive);
	}
}
