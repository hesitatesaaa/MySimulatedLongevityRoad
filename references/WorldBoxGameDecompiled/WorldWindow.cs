using System.Collections.Generic;

public class WorldWindow : TabbedWindow, IInterestingPeopleWindow
{
	public NameInput nameInput;

	public InterestingPeopleTab interesting_people;

	protected override void create()
	{
		base.create();
		nameInput.addListener(applyInputName);
	}

	private void applyInputName(string pInput)
	{
		if (!string.IsNullOrEmpty(pInput))
		{
			World.world.map_stats.name = pInput;
		}
	}

	private void OnEnable()
	{
		if (World.world.map_stats != null)
		{
			nameInput.setText(World.world.map_stats.name);
		}
	}

	public IEnumerable<Actor> getInterestingUnitsList()
	{
		return World.world.units;
	}
}
