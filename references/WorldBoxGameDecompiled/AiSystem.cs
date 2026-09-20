using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using ai.behaviours;

public abstract class AiSystem<TSimObject, TJob, TTask, TAction, TCondition> where TJob : JobAsset<TCondition, TSimObject> where TTask : BehaviourTaskBase<TAction> where TAction : BehaviourActionBase<TSimObject> where TCondition : BehaviourBaseCondition<TSimObject>
{
	public AssetLibrary<TJob> jobs_library;

	public AssetLibrary<TTask> task_library;

	private List<SingleAction<TTask, TAction>> _single_actions;

	internal int action_index;

	internal int restarts;

	internal int task_index;

	private int[] _random_tasks = new int[0];

	public TJob job;

	internal TTask task;

	internal TAction action;

	private double _timestamp_task_start;

	protected readonly TSimObject ai_object;

	public GetNextJobID next_job_delegate;

	public JobAction clear_action_delegate;

	private TaskSwitchAction _task_switch_action;

	private string _scheduled_task_id;

	public AiSystem(TSimObject pObject)
	{
		ai_object = pObject;
		next_job_delegate = nextJobDefault;
	}

	public void scheduleTask(string pTaskID)
	{
		_scheduled_task_id = pTaskID;
	}

	public void addSingleTask(string pID)
	{
		TTask pTask = task_library.get(pID);
		if (_single_actions == null)
		{
			_single_actions = new List<SingleAction<TTask, TAction>>();
		}
		SingleAction<TTask, TAction> singleAction = new SingleAction<TTask, TAction>(pTask);
		_single_actions.Add(singleAction);
		singleAction.reset();
	}

	private void updateNewBehJob()
	{
		if (_scheduled_task_id != null)
		{
			setTask(_scheduled_task_id);
			_scheduled_task_id = null;
			return;
		}
		if (job == null)
		{
			string text = next_job_delegate();
			setJob(text);
		}
		if (task_index >= job.tasks.Count)
		{
			task_index = 0;
		}
		TaskContainer<TCondition, TSimObject> nextTask = getNextTask(job);
		if (nextTask.has_conditions)
		{
			if (checkConditionsForTask(nextTask))
			{
				setTask(nextTask.id);
			}
			else
			{
				setTask("nothing");
			}
		}
		else
		{
			setTask(nextTask.id);
		}
	}

	private TaskContainer<TCondition, TSimObject> getNextTask(TJob pJob)
	{
		List<TaskContainer<TCondition, TSimObject>> tasks = pJob.tasks;
		if (pJob.random)
		{
			if (task_index == 0 && _random_tasks.Length != tasks.Count)
			{
				_random_tasks = new int[tasks.Count];
				for (int i = 0; i < _random_tasks.Length; i++)
				{
					_random_tasks[i] = i;
				}
				_random_tasks.Shuffle();
			}
			return tasks[_random_tasks[task_index++]];
		}
		return tasks[task_index++];
	}

	private bool checkConditionsForTask(TaskContainer<TCondition, TSimObject> pTaskContainer)
	{
		if (pTaskContainer.conditions.Count == 0)
		{
			Debug.LogError((object)"TOO MANY COOKS");
		}
		foreach (var (val2, flag2) in pTaskContainer.conditions)
		{
			if (val2.check(ai_object) != flag2)
			{
				return false;
			}
		}
		return true;
	}

	public void subscribeToTaskSwitch(TaskSwitchAction pAction)
	{
		_task_switch_action = (TaskSwitchAction)Delegate.Combine(_task_switch_action, pAction);
	}

	public virtual void setTask(string pTaskId, bool pClean = true, bool pCleanJob = false, bool pForceAction = false)
	{
		if (pClean)
		{
			clearBeh();
		}
		if (pCleanJob)
		{
			job = null;
			task_index = 0;
			clearAction();
		}
		task = task_library.get(pTaskId);
		action_index = 0;
		restarts = 0;
		_timestamp_task_start = World.world.getCurWorldTime();
		if (pForceAction)
		{
			setAction(task.get(action_index));
		}
		_task_switch_action?.Invoke();
	}

	protected virtual void setAction(TAction pAction)
	{
		action = pAction;
	}

	private void clearAction()
	{
		action = null;
	}

	public void restartJob()
	{
		action_index = 0;
		task_index = 0;
		clearAction();
	}

	internal void clearBeh()
	{
		if (clear_action_delegate != null)
		{
			clear_action_delegate();
		}
	}

	public void clearJob()
	{
		job = null;
		task_index = 0;
	}

	public virtual void setJob(string pJobID)
	{
		job = jobs_library.get(pJobID);
		task_index = 0;
	}

	public void updateSingleTasks(float pElapsed)
	{
		if (_single_actions == null)
		{
			return;
		}
		for (int i = 0; i < _single_actions.Count; i++)
		{
			SingleAction<TTask, TAction> singleAction = _single_actions[i];
			singleAction.timer -= pElapsed;
			if (singleAction.timer <= 0f)
			{
				singleAction.task.list[0].startExecute(ai_object);
				singleAction.reset();
			}
		}
	}

	internal void update()
	{
		if (Bench.bench_ai_enabled)
		{
			if (task != null)
			{
				_ = task.id;
			}
			double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
			run();
			double pValue = Time.realtimeSinceStartupAsDouble - realtimeSinceStartupAsDouble;
			if (task != null)
			{
				task.rate_counter_calls.registerEvent();
				task.rate_counter_performance.registerEvent(pValue);
			}
		}
		else
		{
			run();
		}
	}

	public void decisionRun()
	{
		run();
	}

	private void run()
	{
		if (task == null)
		{
			updateNewBehJob();
			if (task == null)
			{
				return;
			}
		}
		if (action_index >= task.list.Count)
		{
			setTaskBehFinished();
			return;
		}
		setAction(task.get(action_index));
		BehResult behResult;
		if (Bench.bench_ai_enabled)
		{
			_ = action.id;
			double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
			behResult = action.startExecute(ai_object);
			double pValue = Time.realtimeSinceStartupAsDouble - realtimeSinceStartupAsDouble;
			if (action != null)
			{
				action.rate_counter_calls.registerEvent();
				action.rate_counter_performance.registerEvent(pValue);
			}
		}
		else
		{
			behResult = action.startExecute(ai_object);
		}
		if (task == null)
		{
			return;
		}
		switch (behResult)
		{
		case BehResult.Continue:
			action_index++;
			break;
		case BehResult.Stop:
			setTaskBehFinished();
			break;
		case BehResult.StepBack:
			action_index--;
			if (action_index < 0)
			{
				action_index = 0;
			}
			break;
		case BehResult.RestartTask:
			action_index = 0;
			restarts++;
			break;
		case BehResult.ImmediateRun:
			run();
			break;
		case BehResult.RepeatStep:
		case BehResult.Skip:
		case BehResult.ActiveTaskReturn:
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool hasTask()
	{
		return task != null;
	}

	public void setTaskBehFinished()
	{
		task = null;
		action_index = -1;
		clearAction();
	}

	protected virtual void debugLogAction()
	{
	}

	protected virtual void debugLogActionResult(BehResult pResult)
	{
	}

	protected string getActionID(TAction pAction)
	{
		string text = pAction?.GetType().ToString();
		if (text != null)
		{
			text = text.Replace("ai.behaviours.", "");
		}
		return text;
	}

	public void debug(DebugTool pTool)
	{
		string text = getActionID(action);
		if (text != null)
		{
			text = text.Replace("ai.behaviours.", "");
		}
		pTool.setText("job:", (job == null) ? "-" : job.id, 0f, pShowBar: false, 0L);
		string text2;
		if (task_index + 1 < job?.tasks.Count)
		{
			if (job.random)
			{
				text2 = job?.tasks[_random_tasks[task_index + 1]].id;
				text2 += " (R)";
			}
			else
			{
				text2 = job?.tasks[task_index + 1].id;
				text2 += " (S)";
			}
		}
		else
		{
			text2 = "-";
		}
		pTool.setText("next task:", text2, 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText(": task:", task?.id, 0f, pShowBar: false, 0L);
		pTool.setText(": task index:", task_index + "/" + job?.tasks.Count, 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText(":: action:", text, 0f, pShowBar: false, 0L);
		pTool.setText(":: action index:", action_index + "/" + task?.list.Count, 0f, pShowBar: false, 0L);
		pTool.setSeparator();
	}

	public static string nextJobDefault()
	{
		return null;
	}

	internal virtual void reset()
	{
		jobs_library = null;
		task_library = null;
		_single_actions = null;
		action_index = 0;
		task_index = 0;
		restarts = 0;
		job = null;
		task = null;
		action = null;
		next_job_delegate = null;
		clear_action_delegate = null;
		_task_switch_action = null;
	}

	public string getTaskTime()
	{
		return Date.formatSeconds(World.world.getWorldTimeElapsedSince(_timestamp_task_start));
	}
}
