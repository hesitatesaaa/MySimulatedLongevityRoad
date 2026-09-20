using ai.behaviours;

public class TesterBehRequireGroundRatio : BehaviourActionTester
{
	private float _ratio;

	private RequireCondition _cond;

	public TesterBehRequireGroundRatio(float pRatio, RequireCondition pCondition = RequireCondition.AtLeast)
	{
		_ratio = pRatio;
		_cond = pCondition;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		float num = BehaviourActionBase<AutoTesterBot>.world.islands_calculator.realGroundRatio();
		switch (_cond)
		{
		case RequireCondition.AtLeast:
			if (!(num >= _ratio))
			{
				break;
			}
			goto IL_004a;
		case RequireCondition.AtMost:
			if (!(num <= _ratio))
			{
				break;
			}
			goto IL_004a;
		case RequireCondition.Exactly:
			{
				if (num != _ratio)
				{
					break;
				}
				goto IL_004a;
			}
			IL_004a:
			return BehResult.Continue;
		}
		pObject.wait = 1.5f;
		return BehResult.Stop;
	}
}
