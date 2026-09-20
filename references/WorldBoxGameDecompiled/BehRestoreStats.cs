using ai.behaviours;

public class BehRestoreStats : BehaviourActionActor
{
	private readonly float _health;

	private readonly float _mana;

	public BehRestoreStats(float pHealth = 0f, float pMana = 0f)
	{
		_health = pHealth;
		_mana = pMana;
	}

	public override BehResult execute(Actor pActor)
	{
		if (_health != 0f)
		{
			pActor.restoreHealthPercent(_health);
		}
		if (_mana != 0f)
		{
			pActor.restoreManaPercent(_mana);
		}
		return BehResult.Continue;
	}
}
