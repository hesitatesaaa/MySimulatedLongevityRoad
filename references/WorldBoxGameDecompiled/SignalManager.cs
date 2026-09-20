using System.Collections.Generic;

public class SignalManager
{
	public static SignalManager instance;

	private Dictionary<SignalAsset, object> _signals = new Dictionary<SignalAsset, object>();

	public SignalManager()
	{
		instance = this;
	}

	public static void add(SignalAsset pSignal, object pObject = null)
	{
		if (!pSignal.isBanned())
		{
			instance.plan(pSignal, pObject);
		}
	}

	private void plan(SignalAsset pSignal, object pObject = null)
	{
		_signals.TryAdd(pSignal, pObject);
	}

	public void update()
	{
		if (_signals.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<SignalAsset, object> signal in _signals)
		{
			SignalAsset key = signal.Key;
			object value = signal.Value;
			if (key.has_action)
			{
				key.action(value);
			}
			if (key.has_action_achievement)
			{
				key.action_achievement(value);
			}
			if (key.has_ban_check_action && key.ban_check_action(value))
			{
				key.ban();
			}
		}
		_signals.Clear();
	}
}
