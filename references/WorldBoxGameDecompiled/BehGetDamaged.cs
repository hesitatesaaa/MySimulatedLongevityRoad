using ai.behaviours;

public class BehGetDamaged : BehaviourActionActor
{
	private int _damage;

	private AttackType _attackType;

	public BehGetDamaged(int pDamage, AttackType pAttackType)
	{
		_damage = pDamage;
		_attackType = pAttackType;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.getHit(_damage, pFlash: true, _attackType);
		if (pActor.hasHealth())
		{
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
