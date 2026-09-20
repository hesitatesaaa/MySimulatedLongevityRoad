using System;
using System.Collections.Generic;

[Serializable]
public abstract class JobAsset<TCondition, TSimObject> : Asset where TCondition : BehaviourBaseCondition<TSimObject>
{
	public bool random;

	public List<TaskContainer<TCondition, TSimObject>> tasks = new List<TaskContainer<TCondition, TSimObject>>();

	public void addCondition(TCondition pCondition, bool pExpectedResult = true)
	{
		tasks[tasks.Count - 1].addCondition(pCondition, pExpectedResult);
	}

	public void addTask(string pTask)
	{
		TaskContainer<TCondition, TSimObject> taskContainer = new TaskContainer<TCondition, TSimObject>();
		taskContainer.id = pTask;
		tasks.Add(taskContainer);
	}
}
