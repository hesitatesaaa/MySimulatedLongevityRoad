public class BehaviourActionTester : BehaviourActionBase<AutoTesterBot>
{
	public bool null_check_tile_target;

	public override bool errorsFound(AutoTesterBot pObject)
	{
		if (null_check_tile_target && pObject.beh_tile_target == null)
		{
			return true;
		}
		return base.errorsFound(pObject);
	}
}
