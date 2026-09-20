using System;
using System.Collections.Generic;

public class DelayedActionsManager
{
	private List<DelayedAction> _delayed_actions = new List<DelayedAction>();

	private List<DelayedAction> _to_remove = new List<DelayedAction>();

	public void update(float pElapsed, float pDeltaTime)
	{
		for (int i = 0; i < _delayed_actions.Count; i++)
		{
			DelayedAction delayedAction = _delayed_actions[i];
			if (delayedAction.update(pElapsed, pDeltaTime))
			{
				_to_remove.Add(delayedAction);
			}
		}
		for (int j = 0; j < _to_remove.Count; j++)
		{
			DelayedAction item = _to_remove[j];
			_delayed_actions.Remove(item);
		}
		_to_remove.Clear();
	}

	public static void addAction(Action pAction, float pDelay, bool pGameSpeed = true)
	{
		MapBox.instance.delayed_actions_manager.addActionInstance(pAction, pDelay, pGameSpeed);
	}

	private void addActionInstance(Action pAction, float pDelay, bool pGameSpeed = true)
	{
		DelayedAction item = new DelayedAction(pAction, pDelay, pGameSpeed);
		_delayed_actions.Add(item);
	}

	public void clear()
	{
		_delayed_actions.Clear();
		_to_remove.Clear();
	}
}
