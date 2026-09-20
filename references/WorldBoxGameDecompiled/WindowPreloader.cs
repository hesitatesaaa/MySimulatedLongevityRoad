using System.Collections.Generic;
using UnityEngine;

public static class WindowPreloader
{
	private static bool _windows_preloaded;

	private static readonly Dictionary<string, ScrollWindow> _preloaded_windows = new Dictionary<string, ScrollWindow>();

	private static readonly List<ResourceRequest> _windows_resources_requests = new List<ResourceRequest>();

	private static readonly Dictionary<string, AsyncInstantiateOperation<ScrollWindow>> _windows_preloading_operations = new Dictionary<string, AsyncInstantiateOperation<ScrollWindow>>();

	private static readonly Dictionary<string, ListPool<GameObject>> _windows_tabs_objects = new Dictionary<string, ListPool<GameObject>>();

	private static readonly List<string> _windows_preload_list = new List<string>();

	public static void addWindowPreloadResources()
	{
		if (_windows_preloaded || !Config.preload_windows)
		{
			return;
		}
		foreach (WindowAsset item in AssetManager.window_library.list)
		{
			if (item.preload)
			{
				_windows_preload_list.Add(item.id);
			}
		}
		SmoothLoader.add(delegate
		{
			foreach (string item2 in _windows_preload_list)
			{
				preloadWindowPrefab(item2);
			}
		}, c("Preloading windows", 1, 5), pSkipFrame: true);
	}

	public static void addWaitForWindowResources()
	{
		if (!_windows_preloaded)
		{
			SmoothLoader.add(finishPreloadingWindowsResources, c("Preloading windows", 2, 5), pSkipFrame: true);
		}
	}

	private static void finishPreloadingWindowsResources()
	{
		foreach (ResourceRequest windows_resources_request in _windows_resources_requests)
		{
			if (!((AsyncOperation)windows_resources_request).isDone)
			{
				addWaitForWindowResources();
				return;
			}
		}
		addInstantiateWindows();
	}

	private static void addInstantiateWindows()
	{
		SmoothLoader.add(delegate
		{
			foreach (string item in _windows_preload_list)
			{
				prepareWindowPrefab(item);
			}
		}, c("Preloading windows", 3, 5), pSkipFrame: true);
		int num = 0;
		int count = _windows_preload_list.Count;
		foreach (string tWindowID in _windows_preload_list)
		{
			SmoothLoader.add(delegate
			{
				instantiateWindow(tWindowID);
			}, c(c("Preloading windows", 4, 5), ++num, count));
		}
		SmoothLoader.add(finishPreloadingWindows, c("Preloading windows", 5, 5), pSkipFrame: true, 0.001f, pToEnd: true);
	}

	private static void finishPreloadingWindows()
	{
		foreach (AsyncInstantiateOperation<ScrollWindow> value in _windows_preloading_operations.Values)
		{
			if (!value.isDone)
			{
				value.WaitForCompletion();
			}
		}
		foreach (string key in _windows_preloading_operations.Keys)
		{
			ScrollWindow pWindow = _windows_preloading_operations[key].Result.First();
			finishPreloadingWindow(key, pWindow);
			restoreWindowPrefab(key);
		}
		finalizeWindowsPreloading();
	}

	public static bool TryGetPreloadedWindow(string pWindowID, out ScrollWindow tScrollWindow)
	{
		if (_preloaded_windows.TryGetValue(pWindowID, out tScrollWindow))
		{
			_preloaded_windows.Remove(pWindowID);
			return true;
		}
		return false;
	}

	private static void preloadWindowPrefab(string pWindowID)
	{
		string text = ScrollWindow.checkWindowID(pWindowID);
		if (!_preloaded_windows.ContainsKey(pWindowID))
		{
			ResourceRequest item = Resources.LoadAsync("windows/" + text, typeof(ScrollWindow));
			_windows_resources_requests.Add(item);
		}
	}

	private static ScrollWindow getWindowPrefab(string pWindowID)
	{
		string text = ScrollWindow.checkWindowID(pWindowID);
		ScrollWindow scrollWindow = (ScrollWindow)(object)Resources.Load("windows/" + text, typeof(ScrollWindow));
		if ((Object)(object)scrollWindow == (Object)null)
		{
			Debug.LogError((object)("Window with id " + text + " not found!"));
			scrollWindow = (ScrollWindow)(object)Resources.Load("windows/not_found", typeof(ScrollWindow));
		}
		return scrollWindow;
	}

	private static void prepareWindowPrefab(string pWindowID)
	{
		ScrollWindow windowPrefab = getWindowPrefab(pWindowID);
		((Component)windowPrefab).gameObject.SetActive(false);
		_windows_tabs_objects[pWindowID] = ScrollWindow.disableTabsInPrefab(windowPrefab);
	}

	private static void instantiateWindow(string pWindowID)
	{
		AsyncInstantiateOperation<ScrollWindow> value = Object.InstantiateAsync<ScrollWindow>(getWindowPrefab(pWindowID), CanvasMain.instance.transformWindows);
		_windows_preloading_operations.Add(pWindowID, value);
	}

	private static void restoreWindowPrefab(string pWindowID)
	{
		ScrollWindow windowPrefab = getWindowPrefab(pWindowID);
		ScrollWindow.enableTabsInPrefab(_windows_tabs_objects[pWindowID]);
		((Component)windowPrefab).gameObject.SetActive(true);
	}

	private static void finishPreloadingWindow(string pWindowID, ScrollWindow pWindow)
	{
		((Object)((Component)pWindow).gameObject).name = pWindowID;
		pWindow.init();
		_preloaded_windows.Add(pWindowID, pWindow);
	}

	private static void finalizeWindowsPreloading()
	{
		_windows_preloaded = true;
		_windows_tabs_objects.Clear();
		_windows_preloading_operations.Clear();
		_windows_resources_requests.Clear();
		_windows_preload_list.Clear();
	}

	private static string c(string pString, int pStep, int pMax)
	{
		return $"{pString} ({pStep}/{pMax})";
	}
}
