public class BattleContainer
{
	public float timer = 1f;

	public float timeout = 1f;

	public float timer_animation = 0.05f;

	public int frame;

	private int _deaths_civs;

	private int _deaths_mobs;

	public WorldTile tile;

	public int getDeathsTotal()
	{
		return _deaths_civs + _deaths_mobs;
	}

	public void increaseDeaths(Actor pActor)
	{
		if (pActor.isSapient())
		{
			_deaths_civs++;
		}
		else
		{
			_deaths_mobs++;
		}
	}

	public bool isRendered()
	{
		if (_deaths_civs <= 3)
		{
			return _deaths_mobs > 6;
		}
		return true;
	}
}
