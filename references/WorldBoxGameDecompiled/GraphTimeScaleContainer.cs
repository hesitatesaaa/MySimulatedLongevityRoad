using System;
using System.Collections.Generic;
using UnityEngine;
using db;

public class GraphTimeScaleContainer : MonoBehaviour
{
	public GraphTimeScale current_scale;

	private List<GraphTimeScale> _available_time_scales = new List<GraphTimeScale>();

	private GraphController _controller;

	public void calcBounds()
	{
		if ((Object)(object)_controller == (Object)null)
		{
			_controller = ((Component)this).GetComponentInParent<GraphController>();
		}
		_available_time_scales.Clear();
		_available_time_scales.Add(GraphTimeScale.year_10);
		foreach (NanoObject @object in _controller.getObjects())
		{
			using ListPool<GraphTimeScale> listPool = DBGetter.getTimeScales(@object);
			foreach (ref GraphTimeScale item in listPool)
			{
				GraphTimeScale current = item;
				if (!_available_time_scales.Contains(current))
				{
					_available_time_scales.Add(current);
				}
			}
		}
		bool active = _available_time_scales.Count > 1;
		ButtonGraphScalePlusMinus[] componentsInChildren = ((Component)this).GetComponentsInChildren<ButtonGraphScalePlusMinus>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Component)componentsInChildren[i]).gameObject.SetActive(active);
		}
	}

	public bool resetTimeScale()
	{
		calcBounds();
		if (!_available_time_scales.Contains(current_scale))
		{
			current_scale = _available_time_scales.Last();
			return true;
		}
		return false;
	}

	public void setTimeScale(GraphTimeScale pScale)
	{
		current_scale = pScale;
	}

	public ListPool<GraphTimeScale> sharedTimeScales()
	{
		ListPool<GraphTimeScale> listPool = new ListPool<GraphTimeScale>((GraphTimeScale[])Enum.GetValues(typeof(GraphTimeScale)));
		foreach (NanoObject @object in _controller.getObjects())
		{
			ListPool<GraphTimeScale> tAvailableTimeScales = DBGetter.getTimeScales(@object);
			try
			{
				listPool.RemoveAll((GraphTimeScale tScale) => !tAvailableTimeScales.Contains(tScale));
			}
			finally
			{
				if (tAvailableTimeScales != null)
				{
					((IDisposable)tAvailableTimeScales).Dispose();
				}
			}
		}
		return listPool;
	}

	public bool randomizeTimeScale()
	{
		if (_available_time_scales.Count < 2)
		{
			return false;
		}
		using ListPool<GraphTimeScale> listPool = sharedTimeScales();
		if (listPool.Count == 0)
		{
			return false;
		}
		if (listPool.Count > 2)
		{
			listPool.Shift();
		}
		GraphTimeScale random = listPool.GetRandom();
		if (random != current_scale)
		{
			current_scale = random;
			return true;
		}
		return false;
	}

	public void timeScaleMinus()
	{
		int num = (int)current_scale;
		if (num > 0)
		{
			current_scale = (GraphTimeScale)(num - 1);
		}
		else
		{
			current_scale = (GraphTimeScale)(_available_time_scales.Count - 1);
		}
	}

	public void timeScalePlus()
	{
		int num = (int)current_scale;
		if (num < _available_time_scales.Count - 1)
		{
			current_scale = (GraphTimeScale)(num + 1);
		}
		else
		{
			current_scale = GraphTimeScale.year_10;
		}
	}

	public string getIndexString()
	{
		if (_available_time_scales.Count == 0)
		{
			return "";
		}
		return " (" + (int)(current_scale + 1) + "/" + _available_time_scales.Count + ")";
	}

	public GraphTimeScale getCurrentScale()
	{
		return current_scale;
	}
}
