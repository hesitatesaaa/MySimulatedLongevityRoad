using UnityEngine.UI;

public class AllianceWindow : WindowMetaGeneric<Alliance, AllianceData>
{
	public NameInput mottoInput;

	public StatBar bar_experience;

	public override MetaType meta_type => MetaType.Alliance;

	protected override Alliance meta_object => SelectedMetas.selected_alliance;

	protected override void initNameInput()
	{
		base.initNameInput();
		mottoInput.addListener(applyInputMotto);
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
		Alliance alliance = meta_object;
		if (alliance != null)
		{
			mottoInput.setText(alliance.getMotto());
			((Graphic)mottoInput.textField).color = alliance.getColor().getColorText();
		}
	}

	internal override void showStatsRows()
	{
		Alliance alliance = meta_object;
		tryShowPastNames();
		showStatRow("founded", alliance.getFoundedDate(), MetaType.None, -1L, "iconAge");
		tryToShowActor("alliance_founder", alliance.data.founder_actor_id, alliance.data.founder_actor_name, null, "actor_traits/iconStupid");
		tryToShowMetaKingdom("alliance_founder_kingdom", alliance.data.founder_kingdom_id, alliance.data.founder_kingdom_name);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		mottoInput.inputField.DeactivateInputField();
	}
}
