using System.Collections.Generic;
using UnityEngine;

public static class SmoothLoader
{
	public const float DEFAULT_TIMER_VALUE = 0.001f;

	public const int MAX_LOG_LENGTH = 40;

	private static int _index = 0;

	private static float _current_timer = 0f;

	private static bool _skip_frame = false;

	private static int _added = 0;

	private static bool _started = false;

	private static bool _has_actions = false;

	private static List<MapLoaderContainer> _actions = new List<MapLoaderContainer>();

	public static string latest_called_id = string.Empty;

	public static string latest_time = string.Empty;

	private static MapLoaderContainer _last_action;

	private static bool _toggled_console = false;

	private static int _last_i = -1;

	private static bool logsEnabled => !Config.disable_loading_logs;

	public static void prepare()
	{
		_actions.Clear();
		_index = 0;
		_started = false;
		finish();
		latest_called_id = string.Empty;
		latest_time = string.Empty;
		PlayerConfig.checkSettings();
		PreloadHelpers.init();
		add(delegate
		{
			ScrollWindow.hideAllEvent(pWithAnimation: false);
			CanvasMain.instance.setMainUiEnabled(pEnabled: false);
		}, "Close windows", pSkipFrame: true, 0.01f);
		add(delegate
		{
			QuantumSpriteManager.hideAll();
		}, "Quantum Clean", pSkipFrame: true, 0.01f);
	}

	public static void add(MapLoaderAction pAction, string pId, bool pSkipFrame = false, float pNewWaitTimerValue = 0.001f, bool pToEnd = false)
	{
		MapLoaderContainer item = new MapLoaderContainer(pAction, pId, pDebugLog: true, pNewWaitTimerValue);
		_has_actions = true;
		if (_started && !pToEnd)
		{
			if (pSkipFrame)
			{
				_actions.Insert(_index + 1 + _added++, new MapLoaderContainer(skipFrame, "skipFrame", pDebugLog: false));
			}
			_actions.Insert(_index + 1 + _added++, item);
		}
		else
		{
			if (pSkipFrame)
			{
				_actions.Add(new MapLoaderContainer(skipFrame, "skipFrame", pDebugLog: false));
			}
			_actions.Add(item);
		}
	}

	public static void setWaitTimer()
	{
		float num = 0.001f;
		if (_last_action != null)
		{
			num = _last_action.new_timer_value;
		}
		if (num > _current_timer)
		{
			_current_timer = num;
		}
	}

	public static void skipFrame()
	{
		_skip_frame = true;
	}

	public static bool isLoading()
	{
		return _has_actions;
	}

	private static void finish()
	{
		_last_action = null;
		_has_actions = false;
	}

	public static void update(float pElapsed)
	{
		if (!_has_actions)
		{
			return;
		}
		if (_current_timer > 0f)
		{
			_current_timer -= pElapsed;
			return;
		}
		if (_actions.Count == 0)
		{
			finish();
			return;
		}
		if (_last_i == _index)
		{
			openConsole();
			finish();
			MapBox.instance.startTheGame(pForceGenerate: true);
			return;
		}
		_last_i = _index;
		_skip_frame = false;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		doActions();
		while (Time.realtimeSinceStartup - realtimeSinceStartup < 0.1f && !_skip_frame && _actions.Count != 0)
		{
			doActions();
		}
		setWaitTimer();
	}

	private static void openConsole()
	{
		if ((Object)(object)World.world != (Object)null && (Object)(object)World.world.console != (Object)null && !_toggled_console)
		{
			_toggled_console = true;
			World.world.console.Show();
		}
	}

	private static void doActions()
	{
		if (!_started)
		{
			Bench.bench("SmoothLoader", "loading", pForce: true);
		}
		_started = true;
		_added = 0;
		MapLoaderContainer mapLoaderContainer = _actions[_index];
		MapLoaderAction action = mapLoaderContainer.action;
		_last_action = mapLoaderContainer;
		Bench.bench(mapLoaderContainer.id, "loading", pForce: true);
		action();
		Bench.benchEnd(mapLoaderContainer.id, "loading", pSaveCounter: false, 0L, pForce: true);
		setWaitTimer();
		checkDebugText();
		_index++;
		if (_index > _actions.Count - 1)
		{
			_actions.Clear();
			double num = Bench.benchEnd("SmoothLoader", "loading", pSaveCounter: false, 0L, pForce: true);
			if (logsEnabled)
			{
				int num2 = ++_index;
				Debug.Log((object)(fillRight(fillLeft(num2.ToString(), 3, '0') + ": Loading finished", 40) + " = " + num.ToString("F4")));
			}
		}
	}

	private static void checkDebugText()
	{
		if (_last_action == null || !_last_action.debug_log)
		{
			return;
		}
		latest_time = Bench.getBenchResultAsDouble(_last_action.id, "loading", pAverage: false).ToString("F4");
		latest_called_id = fillLeft(_index.ToString(), 3, '0') + ": " + _last_action.id;
		if (logsEnabled)
		{
			string text = fillRight(latest_called_id, 40) + " = " + latest_time;
			double benchResultAsDouble = Bench.getBenchResultAsDouble(_last_action.id, "loading", pAverage: false);
			if (benchResultAsDouble > 1.0)
			{
				text = "<color=red>" + text + "</color>";
			}
			else if (benchResultAsDouble > 0.5)
			{
				text = "<color=yellow>" + text + "</color>";
			}
			Debug.Log((object)text);
		}
	}

	private static string fillLeft(string pString, int pSize = 1, char pFill = ' ')
	{
		return Toolbox.fillLeft(pString, pSize, pFill);
	}

	private static string fillRight(string pString, int pSize = 1, char pFill = ' ')
	{
		return Toolbox.fillRight(pString, pSize, pFill);
	}
}
