using System;

[Serializable]
public class BehaviourTaskTester : BehaviourTaskBase<BehaviourActionTester>
{
	protected override bool has_locales => false;

	protected override string locale_key_prefix => "task_tester";
}
