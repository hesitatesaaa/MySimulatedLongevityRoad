using UnityEngine;
using ai.behaviours;

public class TesterBehWaitFor : BehaviourActionTester
{
	private string _type;

	private int _amount;

	public TesterBehWaitFor(string pType, int pAmount)
	{
		_type = pType;
		_amount = pAmount;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		MetaTypeAsset metaTypeAsset = AssetManager.meta_type_library.get(_type);
		if (metaTypeAsset == null)
		{
			Debug.LogError((object)("TesterBehWaitFor: No asset found for type: " + _type));
			return BehResult.Stop;
		}
		int num = 0;
		foreach (NanoObject item in metaTypeAsset.get_list())
		{
			_ = item;
			num++;
			if (num >= _amount)
			{
				return BehResult.Continue;
			}
		}
		pObject.wait = 1.5f;
		return BehResult.RepeatStep;
	}
}
