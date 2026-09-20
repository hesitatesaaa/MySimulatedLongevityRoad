using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class ThreadHelper : MonoBehaviour
{
	public static ThreadHelper instance = null;

	private static List<Action> threadEventsQueue = new List<Action>();

	private static volatile bool threadEventsQueueEmpty = true;

	private static int _main_thread_id = -1;

	[ThreadStatic]
	private static bool? _is_main_thread;

	public static void Initialize()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		if (!IsActive())
		{
			GameObject val = new GameObject("MainThreadExecuter")
			{
				hideFlags = (HideFlags)61
			};
			Object.DontDestroyOnLoad((Object)val);
			instance = val.AddComponent<ThreadHelper>();
		}
	}

	public static bool IsActive()
	{
		return (Object)(object)instance != (Object)null;
	}

	public void Awake()
	{
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
	}

	public static void ExecuteInUpdate(Action action)
	{
		lock (threadEventsQueue)
		{
			threadEventsQueue.Add(action);
			threadEventsQueueEmpty = false;
		}
	}

	public static void InvokeInUpdate(UnityEvent eventParam)
	{
		ExecuteInUpdate(delegate
		{
			eventParam.Invoke();
		});
	}

	public void Update()
	{
		if (threadEventsQueueEmpty)
		{
			return;
		}
		List<Action> list = new List<Action>();
		lock (threadEventsQueue)
		{
			list.AddRange(threadEventsQueue);
			threadEventsQueue.Clear();
			threadEventsQueueEmpty = true;
		}
		foreach (Action item in list)
		{
			if (item.Target != null)
			{
				item();
			}
		}
	}

	public void OnDisable()
	{
		instance = null;
	}

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void initMainThread()
	{
		_main_thread_id = Thread.CurrentThread.ManagedThreadId;
	}

	public static bool isMainThread()
	{
		if (!_is_main_thread.HasValue)
		{
			_is_main_thread = Thread.CurrentThread.ManagedThreadId == _main_thread_id;
		}
		return _is_main_thread.Value;
	}
}
