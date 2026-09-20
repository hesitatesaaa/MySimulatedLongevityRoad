public class Beehive : BaseBuildingComponent
{
	public int honey;

	public void addHoney()
	{
		if (honey < 10)
		{
			honey++;
			if (honey == 10)
			{
				building.setHaveResourcesToCollect(pValue: true);
			}
		}
	}
}
