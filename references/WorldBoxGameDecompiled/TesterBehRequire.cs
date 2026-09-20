using UnityEngine;
using ai.behaviours;

public class TesterBehRequire : BehaviourActionTester
{
	private string _type;

	private int _amount;

	private RequireCondition _cond;

	public TesterBehRequire(string pType, int pAmount, RequireCondition pCondition = RequireCondition.AtLeast)
	{
		_type = pType;
		_amount = pAmount;
		_cond = pCondition;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		MetaTypeAsset metaTypeAsset = AssetManager.meta_type_library.get(_type);
		if (metaTypeAsset == null)
		{
			Debug.LogError((object)("TesterBehRequire: No asset found for type: " + _type));
			return BehResult.Stop;
		}
		int num = 0;
		foreach (NanoObject item in metaTypeAsset.get_list())
		{
			_ = item;
			num++;
		}
		switch (_cond)
		{
		case RequireCondition.AtLeast:
			if (num < _amount)
			{
				break;
			}
			goto IL_0099;
		case RequireCondition.AtMost:
			if (num > _amount)
			{
				break;
			}
			goto IL_0099;
		case RequireCondition.Exactly:
			{
				if (num != _amount)
				{
					break;
				}
				goto IL_0099;
			}
			IL_0099:
			return BehResult.Continue;
		}
		pObject.wait = 1.5f;
		return BehResult.Stop;
	}
}
