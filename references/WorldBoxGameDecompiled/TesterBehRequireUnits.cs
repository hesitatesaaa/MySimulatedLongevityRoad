using ai.behaviours;

public class TesterBehRequireUnits : BehaviourActionTester
{
	private string _actor_asset_id;

	private int _amount;

	private RequireCondition _cond;

	public TesterBehRequireUnits(string pActorAssetID, int pAmount, RequireCondition pCondition = RequireCondition.AtLeast)
	{
		_actor_asset_id = pActorAssetID;
		_amount = pAmount;
		_cond = pCondition;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		int count = AssetManager.actor_library.get(_actor_asset_id).units.Count;
		switch (_cond)
		{
		case RequireCondition.AtLeast:
			if (count < _amount)
			{
				break;
			}
			goto IL_0055;
		case RequireCondition.AtMost:
			if (count > _amount)
			{
				break;
			}
			goto IL_0055;
		case RequireCondition.Exactly:
			{
				if (count != _amount)
				{
					break;
				}
				goto IL_0055;
			}
			IL_0055:
			return BehResult.Continue;
		}
		pObject.wait = 1.5f;
		return BehResult.Stop;
	}
}
