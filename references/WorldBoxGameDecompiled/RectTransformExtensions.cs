using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public static class RectTransformExtensions
{
	public static ListPool<RectTransform> getLayoutChildren(this RectTransform pRect)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		List<Component> list = CollectionPool<List<Component>, Component>.Get();
		ListPool<RectTransform> listPool = new ListPool<RectTransform>();
		int i = 0;
		for (int childCount = ((Transform)pRect).childCount; i < childCount; i++)
		{
			Transform child = ((Transform)pRect).GetChild(i);
			RectTransform val = (RectTransform)(object)((child is RectTransform) ? child : null);
			if ((Object)(object)val == (Object)null || !((Component)val).gameObject.activeInHierarchy)
			{
				continue;
			}
			if (!((Component)(object)val).HasComponent<ILayoutIgnorer>())
			{
				listPool.Add(val);
				continue;
			}
			((Component)val).GetComponents(typeof(ILayoutIgnorer), list);
			if (list.Count == 0)
			{
				listPool.Add(val);
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (!((ILayoutIgnorer)list[j]).ignoreLayout)
				{
					listPool.Add(val);
					break;
				}
			}
			list.Clear();
		}
		CollectionPool<List<Component>, Component>.Release(list);
		return listPool;
	}

	public static void SetLeft(this RectTransform pRectTransform, float pLeft)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		pRectTransform.offsetMin = new Vector2(pLeft, pRectTransform.offsetMin.y);
	}

	public static void SetRight(this RectTransform pRectTransform, float pRight)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		pRectTransform.offsetMax = new Vector2(0f - pRight, pRectTransform.offsetMax.y);
	}

	public static void SetTop(this RectTransform pRectTransform, float pTop)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		pRectTransform.offsetMax = new Vector2(pRectTransform.offsetMax.x, 0f - pTop);
	}

	public static void SetBottom(this RectTransform pRectTransform, float pBottom)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		pRectTransform.offsetMin = new Vector2(pRectTransform.offsetMin.x, pBottom);
	}

	public static void SetAnchor(this RectTransform pSource, AnchorPresets pAlign, float pOffsetX = 0f, float pOffsetY = 0f)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		pSource.anchoredPosition = Vector2.op_Implicit(new Vector3(pOffsetX, pOffsetY, 0f));
		switch (pAlign)
		{
		case AnchorPresets.TopLeft:
			pSource.anchorMin = new Vector2(0f, 1f);
			pSource.anchorMax = new Vector2(0f, 1f);
			break;
		case AnchorPresets.TopCenter:
			pSource.anchorMin = new Vector2(0.5f, 1f);
			pSource.anchorMax = new Vector2(0.5f, 1f);
			break;
		case AnchorPresets.TopRight:
			pSource.anchorMin = new Vector2(1f, 1f);
			pSource.anchorMax = new Vector2(1f, 1f);
			break;
		case AnchorPresets.MiddleLeft:
			pSource.anchorMin = new Vector2(0f, 0.5f);
			pSource.anchorMax = new Vector2(0f, 0.5f);
			break;
		case AnchorPresets.MiddleCenter:
			pSource.anchorMin = new Vector2(0.5f, 0.5f);
			pSource.anchorMax = new Vector2(0.5f, 0.5f);
			break;
		case AnchorPresets.MiddleRight:
			pSource.anchorMin = new Vector2(1f, 0.5f);
			pSource.anchorMax = new Vector2(1f, 0.5f);
			break;
		case AnchorPresets.BottomLeft:
			pSource.anchorMin = new Vector2(0f, 0f);
			pSource.anchorMax = new Vector2(0f, 0f);
			break;
		case AnchorPresets.BottonCenter:
			pSource.anchorMin = new Vector2(0.5f, 0f);
			pSource.anchorMax = new Vector2(0.5f, 0f);
			break;
		case AnchorPresets.BottomRight:
			pSource.anchorMin = new Vector2(1f, 0f);
			pSource.anchorMax = new Vector2(1f, 0f);
			break;
		case AnchorPresets.HorStretchTop:
			pSource.anchorMin = new Vector2(0f, 1f);
			pSource.anchorMax = new Vector2(1f, 1f);
			break;
		case AnchorPresets.HorStretchMiddle:
			pSource.anchorMin = new Vector2(0f, 0.5f);
			pSource.anchorMax = new Vector2(1f, 0.5f);
			break;
		case AnchorPresets.HorStretchBottom:
			pSource.anchorMin = new Vector2(0f, 0f);
			pSource.anchorMax = new Vector2(1f, 0f);
			break;
		case AnchorPresets.VertStretchLeft:
			pSource.anchorMin = new Vector2(0f, 0f);
			pSource.anchorMax = new Vector2(0f, 1f);
			break;
		case AnchorPresets.VertStretchCenter:
			pSource.anchorMin = new Vector2(0.5f, 0f);
			pSource.anchorMax = new Vector2(0.5f, 1f);
			break;
		case AnchorPresets.VertStretchRight:
			pSource.anchorMin = new Vector2(1f, 0f);
			pSource.anchorMax = new Vector2(1f, 1f);
			break;
		case AnchorPresets.StretchAll:
			pSource.anchorMin = new Vector2(0f, 0f);
			pSource.anchorMax = new Vector2(1f, 1f);
			break;
		case AnchorPresets.BottomStretch:
			break;
		}
	}

	public static void SetPivot(this RectTransform pSource, PivotPresets pPreset, bool pKeepPosition = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		Vector2 zero = Vector2.zero;
		switch (pPreset)
		{
		case PivotPresets.TopLeft:
			((Vector2)(ref zero))._002Ector(0f, 1f);
			break;
		case PivotPresets.TopCenter:
			((Vector2)(ref zero))._002Ector(0.5f, 1f);
			break;
		case PivotPresets.TopRight:
			((Vector2)(ref zero))._002Ector(1f, 1f);
			break;
		case PivotPresets.MiddleLeft:
			((Vector2)(ref zero))._002Ector(0f, 0.5f);
			break;
		case PivotPresets.MiddleCenter:
			((Vector2)(ref zero))._002Ector(0.5f, 0.5f);
			break;
		case PivotPresets.MiddleRight:
			((Vector2)(ref zero))._002Ector(1f, 0.5f);
			break;
		case PivotPresets.BottomLeft:
			((Vector2)(ref zero))._002Ector(0f, 0f);
			break;
		case PivotPresets.BottomCenter:
			((Vector2)(ref zero))._002Ector(0.5f, 0f);
			break;
		case PivotPresets.BottomRight:
			((Vector2)(ref zero))._002Ector(1f, 0f);
			break;
		}
		if (!pKeepPosition)
		{
			pSource.pivot = zero;
			return;
		}
		Vector3 val = Vector2.op_Implicit(pSource.pivot - zero);
		Rect rect = pSource.rect;
		((Vector3)(ref val)).Scale(Vector2.op_Implicit(((Rect)(ref rect)).size));
		((Vector3)(ref val)).Scale(((Transform)pSource).localScale);
		val = ((Transform)pSource).rotation * val;
		pSource.pivot = zero;
		((Transform)pSource).localPosition = ((Transform)pSource).localPosition - val;
	}

	public static Vector2 GetWorldCenter(this RectTransform pRectTransform)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = pRectTransform.rect;
		return Vector2.op_Implicit(((Transform)pRectTransform).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center)));
	}

	public static Rect GetWorldRect(this RectTransform pRectTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = pRectTransform.rect;
		Rect result = default(Rect);
		((Rect)(ref result)).min = Vector2.op_Implicit(((Transform)pRectTransform).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).min)));
		((Rect)(ref result)).max = Vector2.op_Implicit(((Transform)pRectTransform).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).max)));
		return result;
	}

	public static bool Overlaps(this RectTransform a, RectTransform b)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Rect val = a.WorldRect();
		return ((Rect)(ref val)).Overlaps(b.WorldRect());
	}

	public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Rect val = a.WorldRect();
		return ((Rect)(ref val)).Overlaps(b.WorldRect(), allowInverse);
	}

	public static Rect WorldRect(this RectTransform rectTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Vector2 sizeDelta = rectTransform.sizeDelta;
		float num = sizeDelta.x * ((Transform)rectTransform).lossyScale.x;
		float num2 = sizeDelta.y * ((Transform)rectTransform).lossyScale.y;
		Rect rect = rectTransform.rect;
		Vector3 val = ((Transform)rectTransform).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center));
		float num3 = val.x - num * 0.5f;
		float num4 = val.y - num2 * 0.5f;
		return new Rect(num3, num4, num, num2);
	}
}
