using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class TaskContainer<TCondition, TSimObject> where TCondition : BehaviourBaseCondition<TSimObject>
{
	[JsonProperty(Order = -1)]
	public string id;

	public Dictionary<TCondition, bool> conditions;

	public bool has_conditions;

	public void addCondition(TCondition pCondition, bool pResult)
	{
		if (conditions == null)
		{
			conditions = new Dictionary<TCondition, bool>();
		}
		conditions.Add(pCondition, pResult);
		has_conditions = true;
	}
}
