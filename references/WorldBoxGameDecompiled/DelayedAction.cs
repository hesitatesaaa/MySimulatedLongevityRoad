using System;

public class DelayedAction
{
	public readonly Action action;

	private bool _game_speed_affected;

	private float _timer;

	public DelayedAction(Action pAction, float pDelay, bool pGameSpeedAffected)
	{
		action = pAction;
		_game_speed_affected = pGameSpeedAffected;
		_timer = pDelay;
	}

	public bool update(float pElapsed, float pDeltaTime)
	{
		if (_game_speed_affected)
		{
			_timer -= pElapsed;
		}
		else
		{
			_timer -= pDeltaTime;
		}
		if (_timer > 0f)
		{
			return false;
		}
		action();
		return true;
	}
}
