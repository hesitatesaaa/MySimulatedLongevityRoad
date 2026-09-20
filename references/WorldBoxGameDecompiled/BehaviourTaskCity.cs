using System;

[Serializable]
public class BehaviourTaskCity : BehaviourTaskBase<BehaviourActionCity>
{
	protected override string locale_key_prefix => "task_city";

	protected override bool has_locales => false;

	public void executeAllActionsForCity(City pCity)
	{
		for (int i = 0; i < list.Count; i++)
		{
			list[i].startExecute(pCity);
		}
	}
}
