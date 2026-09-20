using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace LayoutGroupExt;

[DisallowMultipleComponent]
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public abstract class LayoutGroupExtended : LayoutGroup
{
	[SerializeField]
	public float moveDuration = 0.15f;

	[Tooltip("Will position the n items immediately, animating the next ones.")]
	[SerializeField]
	public int delayItems = 1;

	private Dictionary<RectTransform, TweenerCore<Vector2, Vector2, VectorOptions>> RectPositionXTweens = new Dictionary<RectTransform, TweenerCore<Vector2, Vector2, VectorOptions>>();

	private Dictionary<RectTransform, TweenerCore<Vector2, Vector2, VectorOptions>> RectPositionYTweens = new Dictionary<RectTransform, TweenerCore<Vector2, Vector2, VectorOptions>>();

	private static List<RectTransform> _to_remove = new List<RectTransform>();

	internal List<RectTransform> m_Children = new List<RectTransform>();

	internal Dictionary<int, List<RectTransform>> m_Axis = new Dictionary<int, List<RectTransform>>
	{
		{
			0,
			new List<RectTransform>()
		},
		{
			1,
			new List<RectTransform>()
		}
	};

	internal Vector2[] m_Positions = (Vector2[])(object)new Vector2[0];

	internal RectTransform[] m_Sort = (RectTransform[])(object)new RectTransform[0];

	internal Dictionary<RectTransform, Vector2> m_Grid_Positions = new Dictionary<RectTransform, Vector2>();

	internal Dictionary<RectTransform, Vector2> m_Grid_Anchors = new Dictionary<RectTransform, Vector2>();

	private int _skip_frame = -1;

	private static RectTransform _highlighter_prefab;

	private ObjectPoolGenericMono<RectTransform> _pool_highlighter;

	protected void SetChildAlongAxis(RectTransform rect, int axis, float pos)
	{
		if (!((Object)(object)rect == (Object)null))
		{
			SetChildAlongAxisWithScale(rect, axis, pos, 1f);
		}
	}

	public override void CalculateLayoutInputHorizontal()
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		if (_skip_frame == Time.frameCount)
		{
			((LayoutGroup)this).SetDirty();
			return;
		}
		bool flag = ((LayoutGroup)this).rectChildren.Count == 0;
		((LayoutGroup)this).rectChildren.Clear();
		List<Component> list = CollectionPool<List<Component>, Component>.Get();
		for (int i = 0; i < ((Transform)((LayoutGroup)this).rectTransform).childCount; i++)
		{
			if ((Application.isPlaying & flag) && ((LayoutGroup)this).rectChildren.Count == delayItems)
			{
				_skip_frame = Time.frameCount;
				((LayoutGroup)this).SetDirty();
				break;
			}
			Transform child = ((Transform)((LayoutGroup)this).rectTransform).GetChild(i);
			RectTransform val = (RectTransform)(object)((child is RectTransform) ? child : null);
			if ((Object)(object)val == (Object)null || !((Component)val).gameObject.activeInHierarchy)
			{
				continue;
			}
			((Component)val).GetComponents(typeof(ILayoutIgnorer), list);
			if (list.Count == 0)
			{
				((LayoutGroup)this).rectChildren.Add(val);
				continue;
			}
			foreach (ILayoutIgnorer item in list)
			{
				ILayoutIgnorer val2 = item;
				if (!val2.ignoreLayout && ((Behaviour)(MonoBehaviour)val2).enabled)
				{
					((LayoutGroup)this).rectChildren.Add(val);
					break;
				}
			}
		}
		CollectionPool<List<Component>, Component>.Release(list);
		((DrivenRectTransformTracker)(ref base.m_Tracker)).Clear();
	}

	protected void SetChildAlongAxisWithScale(RectTransform rect, int axis, float pos, float scaleFactor)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rect == (Object)null))
		{
			((DrivenRectTransformTracker)(ref base.m_Tracker)).Add((Object)(object)this, rect, (DrivenTransformProperties)(0xF00 | ((axis == 0) ? 2 : 4)));
			rect.anchorMin = Vector2.up;
			rect.anchorMax = Vector2.up;
			if (!m_Grid_Anchors.TryGetValue(rect, out var value) || !Application.isPlaying)
			{
				value = rect.anchoredPosition;
				m_Grid_Anchors[rect] = value;
			}
			Vector2 val;
			float num3;
			if (axis != 0)
			{
				float num = 0f - pos;
				val = rect.sizeDelta;
				float num2 = ((Vector2)(ref val))[axis];
				val = rect.pivot;
				num3 = num - num2 * (1f - ((Vector2)(ref val))[axis]) * scaleFactor;
			}
			else
			{
				val = rect.sizeDelta;
				float num4 = ((Vector2)(ref val))[axis];
				val = rect.pivot;
				num3 = pos + num4 * ((Vector2)(ref val))[axis] * scaleFactor;
			}
			((Vector2)(ref value))[axis] = num3;
			SetPosition(rect, value, axis);
		}
	}

	protected void SetChildAlongAxis(RectTransform rect, int axis, float pos, float size)
	{
		if (!((Object)(object)rect == (Object)null))
		{
			SetChildAlongAxisWithScale(rect, axis, pos, size, 1f);
		}
	}

	protected void SetChildAlongAxisWithScale(RectTransform rect, int axis, float pos, float size, float scaleFactor)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rect == (Object)null))
		{
			((DrivenRectTransformTracker)(ref base.m_Tracker)).Add((Object)(object)this, rect, (DrivenTransformProperties)(0xF00 | ((axis == 0) ? 4098 : 8196)));
			rect.anchorMin = Vector2.up;
			rect.anchorMax = Vector2.up;
			Vector2 sizeDelta = rect.sizeDelta;
			((Vector2)(ref sizeDelta))[axis] = size;
			rect.sizeDelta = sizeDelta;
			if (!m_Grid_Anchors.TryGetValue(rect, out var value) || !Application.isPlaying)
			{
				value = rect.anchoredPosition;
				m_Grid_Anchors[rect] = value;
			}
			Vector2 pivot;
			float num2;
			if (axis != 0)
			{
				float num = 0f - pos;
				pivot = rect.pivot;
				num2 = num - size * (1f - ((Vector2)(ref pivot))[axis]) * scaleFactor;
			}
			else
			{
				pivot = rect.pivot;
				num2 = pos + size * ((Vector2)(ref pivot))[axis] * scaleFactor;
			}
			((Vector2)(ref value))[axis] = num2;
			SetPosition(rect, value, axis);
		}
	}

	public void SetPosition(RectTransform rect, Vector2 pos, int axis)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Expected O, but got Unknown
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Expected O, but got Unknown
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Expected O, but got Unknown
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Expected O, but got Unknown
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		if (!Application.isPlaying)
		{
			rect.anchoredPosition = pos;
			return;
		}
		if (!m_Axis[axis].Contains(rect))
		{
			if (m_Axis[axis].Count >= delayItems)
			{
				Vector2 val = Vector2.zero;
				float num = float.MaxValue;
				for (int num2 = m_Axis[axis].Count - 1; num2 >= 0; num2--)
				{
					Vector2 val2 = m_Axis[axis][num2].anchoredPosition;
					if (val2 == Vector2.zero)
					{
						val2 = m_Grid_Anchors[m_Axis[axis][num2]];
					}
					float num3 = Vector2.Distance(val2, pos);
					if (num3 < num)
					{
						num = num3;
						val = val2;
					}
				}
				Vector2 val3 = val - pos;
				if (Mathf.Abs(val3.y) > Mathf.Abs(val3.x))
				{
					rect.anchoredPosition = val + new Vector2(0f, 1f);
				}
				else
				{
					rect.anchoredPosition = val - new Vector2(1f, 0f);
				}
			}
			else
			{
				rect.anchoredPosition = pos;
			}
			m_Axis[axis].Add(rect);
			if (!m_Children.Contains(rect))
			{
				m_Children.Add(rect);
			}
		}
		m_Grid_Anchors[rect] = pos;
		Vector2 anchoredPosition = rect.anchoredPosition;
		rect.anchoredPosition = pos;
		m_Grid_Positions[rect] = Vector2.op_Implicit(((Transform)rect).position);
		rect.anchoredPosition = anchoredPosition;
		if (m_Children.Count != m_Positions.Length)
		{
			m_Positions = (Vector2[])(object)new Vector2[m_Children.Count];
			m_Sort = (RectTransform[])(object)new RectTransform[m_Children.Count];
		}
		m_Children.Sort((RectTransform a, RectTransform b) => ((Transform)a).GetSiblingIndex().CompareTo(((Transform)b).GetSiblingIndex()));
		for (int num4 = 0; num4 < m_Children.Count; num4++)
		{
			Vector2 val4 = m_Grid_Positions[m_Children[num4]];
			m_Positions[num4] = val4;
			m_Sort[num4] = m_Children[num4];
		}
		Dictionary<RectTransform, TweenerCore<Vector2, Vector2, VectorOptions>> dict;
		TweenerCore<Vector2, Vector2, VectorOptions> tween;
		switch (axis)
		{
		default:
			return;
		case 0:
		{
			dict = RectPositionXTweens;
			if (dict.TryGetValue(rect, out var value2) && TweenExtensions.IsActive((Tween)(object)value2))
			{
				if (Mathf.Approximately(value2.endValue.x, pos.x))
				{
					return;
				}
				TweenExtensions.Kill((Tween)(object)value2, false);
			}
			if (Mathf.Approximately(rect.anchoredPosition.x, pos.x))
			{
				return;
			}
			tween = DOTweenModuleUI.DOAnchorPosX(rect, pos.x, moveDuration, false);
			break;
		}
		case 1:
		{
			dict = RectPositionYTweens;
			if (dict.TryGetValue(rect, out var value) && TweenExtensions.IsActive((Tween)(object)value))
			{
				if (Mathf.Approximately(value.endValue.y, pos.y))
				{
					return;
				}
				TweenExtensions.Kill((Tween)(object)value, false);
			}
			if (Mathf.Approximately(rect.anchoredPosition.y, pos.y))
			{
				return;
			}
			tween = DOTweenModuleUI.DOAnchorPosY(rect, pos.y, moveDuration, false);
			break;
		}
		}
		TweenerCore<Vector2, Vector2, VectorOptions> obj = tween;
		((Tween)obj).onKill = (TweenCallback)Delegate.Combine((Delegate?)(object)((Tween)obj).onKill, (Delegate?)(TweenCallback)delegate
		{
			if (dict.ContainsKey(rect) && dict[rect] == tween)
			{
				dict.Remove(rect);
			}
		});
		TweenerCore<Vector2, Vector2, VectorOptions> obj2 = tween;
		((Tween)obj2).onComplete = (TweenCallback)Delegate.Combine((Delegate?)(object)((Tween)obj2).onComplete, (Delegate?)(TweenCallback)delegate
		{
			LayoutRebuilder.MarkLayoutForRebuild(((LayoutGroup)this).rectTransform);
			dict.Remove(rect);
		});
		dict[rect] = tween;
	}

	protected override void OnEnable()
	{
		((LayoutGroup)this).OnEnable();
		ScrollWindow.addCallbackShow(setDirty);
		ScrollWindow.addCallbackShowFinished(setDirty);
	}

	protected override void OnDisable()
	{
		((LayoutGroup)this).OnDisable();
		ScrollWindow.removeCallbackShow(setDirty);
		ScrollWindow.removeCallbackShowFinished(setDirty);
		_skip_frame = -1;
		m_Axis[0].Clear();
		m_Axis[1].Clear();
		m_Children.Clear();
		m_Grid_Positions.Clear();
		m_Grid_Anchors.Clear();
		((LayoutGroup)this).rectChildren.Clear();
		using ListPool<TweenerCore<Vector2, Vector2, VectorOptions>> listPool = new ListPool<TweenerCore<Vector2, Vector2, VectorOptions>>(RectPositionXTweens.Count + RectPositionYTweens.Count);
		listPool.AddRange(RectPositionXTweens.Values);
		listPool.AddRange(RectPositionYTweens.Values);
		RectPositionXTweens.Clear();
		RectPositionYTweens.Clear();
		foreach (ref TweenerCore<Vector2, Vector2, VectorOptions> item in listPool)
		{
			TweenExtensions.Kill((Tween)(object)item, false);
		}
	}

	private void LateUpdate()
	{
		foreach (RectTransform child in m_Children)
		{
			if (!((LayoutGroup)this).rectChildren.Contains(child) || !((Component)child).gameObject.activeInHierarchy)
			{
				_to_remove.Add(child);
			}
		}
		foreach (RectTransform item in _to_remove)
		{
			m_Children.Remove(item);
			m_Axis[0].Remove(item);
			m_Axis[1].Remove(item);
			m_Grid_Positions.Remove(item);
			m_Grid_Anchors.Remove(item);
		}
		_to_remove.Clear();
	}

	private void setDirty(string pWindowName)
	{
		((LayoutGroup)this).SetDirty();
	}

	private void DebugInit()
	{
		if ((Object)(object)_highlighter_prefab == (Object)null)
		{
			_highlighter_prefab = Object.Instantiate<RectTransform>(Resources.Load<RectTransform>("ui/selector"));
		}
		if (_pool_highlighter == null)
		{
			_pool_highlighter = new ObjectPoolGenericMono<RectTransform>(_highlighter_prefab, ((Component)this).transform);
		}
		_pool_highlighter.clear();
	}

	protected unsafe virtual void Update()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		if (!Application.isPlaying)
		{
			return;
		}
		if (!DebugConfig.isOn(DebugOption.ShowLayoutGroupGrid))
		{
			_pool_highlighter?.clear();
			return;
		}
		DebugInit();
		for (int i = 0; i < m_Positions.Length; i++)
		{
			Vector2 val = m_Positions[i];
			RectTransform next = _pool_highlighter.getNext();
			((Transform)next).localScale = ((Transform)m_Children[0]).localScale;
			((Graphic)((Component)((Transform)next).GetChild(0)).GetComponent<Image>()).color = new Color(1f, 0f, 0f, 0.25f);
			GameObject gameObject = ((Component)next).gameObject;
			string text = i.ToString();
			Vector2 val2 = val;
			((Object)gameObject).name = "m_positions " + text + " " + ((object)(*(Vector2*)(&val2))/*cast due to constrained. prefix*/).ToString();
			((Transform)next).position = Vector2.op_Implicit(val);
		}
		for (int j = 0; j < m_Sort.Length; j++)
		{
			RectTransform key = m_Sort[j];
			Vector2 val3 = m_Grid_Anchors[key];
			RectTransform next2 = _pool_highlighter.getNext();
			((Transform)next2).localScale = ((Transform)m_Children[0]).localScale;
			((Graphic)((Component)((Transform)next2).GetChild(0)).GetComponent<Image>()).color = new Color(0f, 1f, 0f, 0.25f);
			GameObject gameObject2 = ((Component)next2).gameObject;
			string text2 = j.ToString();
			Vector2 val2 = val3;
			((Object)gameObject2).name = "m_Grid_Anchors " + text2 + " " + ((object)(*(Vector2*)(&val2))/*cast due to constrained. prefix*/).ToString();
			next2.anchoredPosition = val3;
		}
		for (int k = 0; k < m_Sort.Length; k++)
		{
			RectTransform key2 = m_Sort[k];
			Vector2 val4 = m_Grid_Positions[key2];
			RectTransform next3 = _pool_highlighter.getNext();
			((Transform)next3).localScale = ((Transform)m_Children[0]).localScale;
			((Graphic)((Component)((Transform)next3).GetChild(0)).GetComponent<Image>()).color = new Color(0f, 0f, 1f, 0.25f);
			GameObject gameObject3 = ((Component)next3).gameObject;
			string text3 = k.ToString();
			Vector2 val2 = val4;
			((Object)gameObject3).name = "m_Grid_Positions " + text3 + " " + ((object)(*(Vector2*)(&val2))/*cast due to constrained. prefix*/).ToString();
			((Transform)next3).position = Vector2.op_Implicit(val4);
		}
	}
}
