namespace ai.behaviours;

public class BehFindMeatSameSpeciesSource : BehFindMeatSource
{
	public BehFindMeatSameSpeciesSource(bool pCheckForFactions)
		: base(MeatTargetType.MeatSameSpecies, pCheckForFactions)
	{
	}
}
