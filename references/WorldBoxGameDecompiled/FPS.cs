using System.Collections.Generic;
using UnityEngine;

public static class FPS
{
	private static float _delta_time;

	private static float _frame_count;

	private static float _update_rate = 3f;

	private static int _fps;

	private const int DEBUG_FPS_THRESHOLD = 25;

	private static List<int> _last_fps = new List<int>(5);

	public static int fps => _fps;

	public static string getFPS()
	{
		if (_fps >= 60)
		{
			return "<color=#75D53A>" + _fps + "</color>";
		}
		if (_fps >= 30)
		{
			return "<color=#F4E700>" + _fps + "</color>";
		}
		return "<color=#DB2920>" + _fps + "</color>";
	}

	public static void update()
	{
		_delta_time += Time.unscaledDeltaTime;
		_frame_count++;
		if (_delta_time > 1f / _update_rate)
		{
			_fps = (int)(_frame_count / _delta_time);
			_delta_time = 0f;
			_frame_count = 0f;
		}
	}

	public static void debug_update()
	{
		if (Config.game_loaded && !SmoothLoader.isLoading() && _last_fps.Count < 5)
		{
			if (_fps >= 25)
			{
				_last_fps.Clear();
			}
			_last_fps.Add(_fps);
			if (Config.editor_mastef && _last_fps.Count >= 5)
			{
				DebugConfig.createTool(AssetManager.debug_tool_library.get("Benchmark All").id, 30);
				DebugConfig.createTool(AssetManager.debug_tool_library.get("Benchmark Actors").id, 200);
			}
		}
	}
}
