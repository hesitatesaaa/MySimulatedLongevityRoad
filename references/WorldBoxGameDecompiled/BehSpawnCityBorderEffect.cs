using ai.behaviours;

public class BehSpawnCityBorderEffect : BehaviourActionActor
{
	private int _amount;

	public BehSpawnCityBorderEffect(int pAmount = 1)
	{
		_amount = pAmount;
	}

	public override BehResult execute(Actor pActor)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		_ = pActor.current_tile.zone;
		for (int i = 0; i < _amount; i++)
		{
			WorldTile random = pActor.current_tile.neighbours.GetRandom();
			EffectsLibrary.spawnAt("fx_new_border", random.posV, 0.25f);
		}
		return BehResult.Continue;
	}
}
