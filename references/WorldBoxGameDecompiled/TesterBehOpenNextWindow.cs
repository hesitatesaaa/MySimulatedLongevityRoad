using System.Collections.Generic;
using UnityEngine;
using ai.behaviours;

public class TesterBehOpenNextWindow : BehaviourActionTester
{
	private int _current_window;

	private bool _only_meta;

	private bool _random;

	private List<WindowAsset> _windows;

	public TesterBehOpenNextWindow(bool pOnlyMeta = false, bool pRandom = false)
	{
		_only_meta = pOnlyMeta;
		_random = pRandom;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		if (_windows == null)
		{
			_windows = AssetManager.window_library.getTestableWindows();
			if (_only_meta)
			{
				_windows = _windows.FindAll((WindowAsset pWindow) => pWindow.meta_type_asset != null);
				_windows = _windows.FindAll((WindowAsset pWindow) => !pWindow.id.EndsWith("_customize"));
			}
		}
		if (_random)
		{
			_current_window = Random.Range(0, _windows.Count);
		}
		else
		{
			_current_window = Toolbox.loopIndex(_current_window + 1, _windows.Count);
		}
		WindowAsset windowAsset = _windows[_current_window];
		if (_only_meta && windowAsset.meta_type_asset == null)
		{
			return BehResult.RepeatStep;
		}
		if (windowAsset.meta_type_asset != null)
		{
			NanoObject nanoObject = windowAsset.meta_type_asset.get_selected();
			if (nanoObject == null || !nanoObject.isAlive())
			{
				return BehResult.RepeatStep;
			}
		}
		Config.debug_window_stats.setCurrent(windowAsset.id);
		ScrollWindow.get(windowAsset.id).show("right", "right", pSkipAnimation: true);
		pObject.wait = 0.1f;
		return BehResult.Continue;
	}
}
