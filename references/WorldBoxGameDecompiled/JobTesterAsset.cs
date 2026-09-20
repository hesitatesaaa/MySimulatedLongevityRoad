using System;

[Serializable]
public class JobTesterAsset : JobAsset<BehaviourTesterCondition, AutoTesterBot>
{
	public bool manual_test;
}
