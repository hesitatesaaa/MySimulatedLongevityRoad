using ai.behaviours;

public class TesterBehSpawnRandomBomb : TesterBehSpawnPower
{
	internal static string[] events;

	public TesterBehSpawnRandomBomb()
	{
		if (events == null)
		{
			events = new string[6] { "bomb", "grenade", "napalm_bomb", "atomic_bomb", "antimatter_bomb", "czar_bomba" };
		}
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		_power = events.GetRandom();
		return base.execute(pObject);
	}
}
