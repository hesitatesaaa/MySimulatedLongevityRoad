using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WindowMetaElementBase : MonoBehaviour, IShouldRefreshWindow
{
	protected readonly List<NanoObject> track_objects = new List<NanoObject>();

	protected virtual void Awake()
	{
		clearInitial();
		clear();
	}

	protected virtual void OnEnable()
	{
		clear();
		((MonoBehaviour)this).StartCoroutine(showContent());
	}

	public virtual void refresh()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			((MonoBehaviour)this).StopAllCoroutines();
			clear();
			((MonoBehaviour)this).StartCoroutine(showContent());
		}
	}

	protected virtual IEnumerator showContent()
	{
		yield break;
	}

	protected virtual void OnDisable()
	{
		clear();
	}

	protected virtual void clear()
	{
		track_objects.Clear();
	}

	protected virtual void clearInitial()
	{
	}

	public virtual bool checkRefreshWindow()
	{
		foreach (NanoObject track_object in track_objects)
		{
			if (track_object.isRekt())
			{
				return true;
			}
		}
		return false;
	}
}
