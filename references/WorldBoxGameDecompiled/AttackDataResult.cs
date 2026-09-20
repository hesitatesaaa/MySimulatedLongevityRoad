public struct AttackDataResult
{
	public long deflected_by_who_id;

	public ApplyAttackState state;

	public static AttackDataResult Continue => new AttackDataResult(ApplyAttackState.Continue, -1L);

	public static AttackDataResult Miss => new AttackDataResult(ApplyAttackState.Miss, -1L);

	public static AttackDataResult Hit => new AttackDataResult(ApplyAttackState.Hit, -1L);

	public static AttackDataResult Block => new AttackDataResult(ApplyAttackState.Block, -1L);

	public AttackDataResult(ApplyAttackState pState, long pDeflectedByWhoId = -1L)
	{
		state = pState;
		deflected_by_who_id = pDeflectedByWhoId;
	}
}
