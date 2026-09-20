using ai.behaviours;

public class TesterBehSpawnPowerActor : TesterBehSpawnPower
{
	private int _limit;

	public TesterBehSpawnPowerActor(string pPower, int pLimit = -1)
	{
		_power = pPower;
		_limit = pLimit;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		if (_limit > -1)
		{
			GodPower godPower = AssetManager.powers.get(_power);
			if (AssetManager.actor_library.get(godPower.actor_asset_id).units.Count >= _limit)
			{
				return BehResult.Continue;
			}
		}
		return base.execute(pObject);
	}
}
