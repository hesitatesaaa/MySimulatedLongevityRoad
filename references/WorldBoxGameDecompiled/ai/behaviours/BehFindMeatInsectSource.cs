namespace ai.behaviours;

public class BehFindMeatInsectSource : BehFindMeatSource
{
	public BehFindMeatInsectSource(bool pCheckForFactions = true)
		: base(MeatTargetType.Insect, pCheckForFactions)
	{
	}
}
