using System;

[Serializable]
public class BehaviourTaskKingdom : BehaviourTaskBase<BehaviourActionKingdom>
{
	protected override string locale_key_prefix => "task_kingdom";

	protected override bool has_locales => false;
}
