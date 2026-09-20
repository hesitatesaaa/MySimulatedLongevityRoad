using UnityEngine;
using UnityEngine.UI;

namespace LayoutGroupExt;

[ExecuteAlways]
public abstract class HorizontalOrVerticalLayoutGroupExtended : LayoutGroupExtended
{
	[SerializeField]
	protected float m_Spacing;

	[SerializeField]
	protected bool m_ChildForceExpandWidth = true;

	[SerializeField]
	protected bool m_ChildForceExpandHeight = true;

	[SerializeField]
	protected bool m_ChildControlWidth = true;

	[SerializeField]
	protected bool m_ChildControlHeight = true;

	[SerializeField]
	protected bool m_ChildScaleWidth;

	[SerializeField]
	protected bool m_ChildScaleHeight;

	[SerializeField]
	protected bool m_ReverseArrangement;

	public float spacing
	{
		get
		{
			return m_Spacing;
		}
		set
		{
			((LayoutGroup)this).SetProperty<float>(ref m_Spacing, value);
		}
	}

	public bool childForceExpandWidth
	{
		get
		{
			return m_ChildForceExpandWidth;
		}
		set
		{
			((LayoutGroup)this).SetProperty<bool>(ref m_ChildForceExpandWidth, value);
		}
	}

	public bool childForceExpandHeight
	{
		get
		{
			return m_ChildForceExpandHeight;
		}
		set
		{
			((LayoutGroup)this).SetProperty<bool>(ref m_ChildForceExpandHeight, value);
		}
	}

	public bool childControlWidth
	{
		get
		{
			return m_ChildControlWidth;
		}
		set
		{
			((LayoutGroup)this).SetProperty<bool>(ref m_ChildControlWidth, value);
		}
	}

	public bool childControlHeight
	{
		get
		{
			return m_ChildControlHeight;
		}
		set
		{
			((LayoutGroup)this).SetProperty<bool>(ref m_ChildControlHeight, value);
		}
	}

	public bool childScaleWidth
	{
		get
		{
			return m_ChildScaleWidth;
		}
		set
		{
			((LayoutGroup)this).SetProperty<bool>(ref m_ChildScaleWidth, value);
		}
	}

	public bool childScaleHeight
	{
		get
		{
			return m_ChildScaleHeight;
		}
		set
		{
			((LayoutGroup)this).SetProperty<bool>(ref m_ChildScaleHeight, value);
		}
	}

	public bool reverseArrangement
	{
		get
		{
			return m_ReverseArrangement;
		}
		set
		{
			((LayoutGroup)this).SetProperty<bool>(ref m_ReverseArrangement, value);
		}
	}

	protected void CalcAlongAxis(int axis, bool isVertical)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		float num = ((axis == 0) ? ((LayoutGroup)this).padding.horizontal : ((LayoutGroup)this).padding.vertical);
		bool controlSize = ((axis == 0) ? m_ChildControlWidth : m_ChildControlHeight);
		bool flag = ((axis == 0) ? m_ChildScaleWidth : m_ChildScaleHeight);
		bool childForceExpand = ((axis == 0) ? m_ChildForceExpandWidth : m_ChildForceExpandHeight);
		float num2 = num;
		float num3 = num;
		float num4 = 0f;
		bool flag2 = isVertical ^ (axis == 1);
		int count = ((LayoutGroup)this).rectChildren.Count;
		for (int i = 0; i < count; i++)
		{
			RectTransform val = ((LayoutGroup)this).rectChildren[i];
			GetChildSizes(val, axis, controlSize, childForceExpand, out var min, out var preferred, out var flexible);
			if (flag)
			{
				Vector3 localScale = ((Transform)val).localScale;
				float num5 = ((Vector3)(ref localScale))[axis];
				min *= num5;
				preferred *= num5;
				flexible *= num5;
			}
			if (flag2)
			{
				num2 = Mathf.Max(min + num, num2);
				num3 = Mathf.Max(preferred + num, num3);
				num4 = Mathf.Max(flexible, num4);
			}
			else
			{
				num2 += min + spacing;
				num3 += preferred + spacing;
				num4 += flexible;
			}
		}
		if (!flag2 && ((LayoutGroup)this).rectChildren.Count > 0)
		{
			num2 -= spacing;
			num3 -= spacing;
		}
		num3 = Mathf.Max(num2, num3);
		((LayoutGroup)this).SetLayoutInputForAxis(num2, num3, num4, axis);
	}

	protected void SetChildrenAlongAxis(int axis, bool isVertical)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = ((LayoutGroup)this).rectTransform.rect;
		Vector2 val = ((Rect)(ref rect)).size;
		float num = ((Vector2)(ref val))[axis];
		bool flag = ((axis == 0) ? m_ChildControlWidth : m_ChildControlHeight);
		bool flag2 = ((axis == 0) ? m_ChildScaleWidth : m_ChildScaleHeight);
		bool childForceExpand = ((axis == 0) ? m_ChildForceExpandWidth : m_ChildForceExpandHeight);
		float alignmentOnAxis = ((LayoutGroup)this).GetAlignmentOnAxis(axis);
		bool num2 = isVertical ^ (axis == 1);
		int num3 = (m_ReverseArrangement ? (((LayoutGroup)this).rectChildren.Count - 1) : 0);
		int num4 = ((!m_ReverseArrangement) ? ((LayoutGroup)this).rectChildren.Count : 0);
		int num5 = ((!m_ReverseArrangement) ? 1 : (-1));
		Vector3 localScale;
		if (num2)
		{
			float num6 = num - (float)((axis == 0) ? ((LayoutGroup)this).padding.horizontal : ((LayoutGroup)this).padding.vertical);
			for (int i = num3; m_ReverseArrangement ? (i >= num4) : (i < num4); i += num5)
			{
				RectTransform val2 = ((LayoutGroup)this).rectChildren[i];
				GetChildSizes(val2, axis, flag, childForceExpand, out var min, out var preferred, out var flexible);
				float num7;
				if (!flag2)
				{
					num7 = 1f;
				}
				else
				{
					localScale = ((Transform)val2).localScale;
					num7 = ((Vector3)(ref localScale))[axis];
				}
				float num8 = num7;
				float num9 = Mathf.Clamp(num6, min, (flexible > 0f) ? num : preferred);
				float startOffset = ((LayoutGroup)this).GetStartOffset(axis, num9 * num8);
				if (flag)
				{
					SetChildAlongAxisWithScale(val2, axis, startOffset, num9, num8);
					continue;
				}
				val = val2.sizeDelta;
				float num10 = (num9 - ((Vector2)(ref val))[axis]) * alignmentOnAxis;
				SetChildAlongAxisWithScale(val2, axis, startOffset + num10, num8);
			}
			return;
		}
		float num11 = ((axis == 0) ? ((LayoutGroup)this).padding.left : ((LayoutGroup)this).padding.top);
		float num12 = 0f;
		float num13 = num - ((LayoutGroup)this).GetTotalPreferredSize(axis);
		if (num13 > 0f)
		{
			if (((LayoutGroup)this).GetTotalFlexibleSize(axis) == 0f)
			{
				num11 = ((LayoutGroup)this).GetStartOffset(axis, ((LayoutGroup)this).GetTotalPreferredSize(axis) - (float)((axis == 0) ? ((LayoutGroup)this).padding.horizontal : ((LayoutGroup)this).padding.vertical));
			}
			else if (((LayoutGroup)this).GetTotalFlexibleSize(axis) > 0f)
			{
				num12 = num13 / ((LayoutGroup)this).GetTotalFlexibleSize(axis);
			}
		}
		float num14 = 0f;
		if (((LayoutGroup)this).GetTotalMinSize(axis) != ((LayoutGroup)this).GetTotalPreferredSize(axis))
		{
			num14 = Mathf.Clamp01((num - ((LayoutGroup)this).GetTotalMinSize(axis)) / (((LayoutGroup)this).GetTotalPreferredSize(axis) - ((LayoutGroup)this).GetTotalMinSize(axis)));
		}
		for (int j = num3; m_ReverseArrangement ? (j >= num4) : (j < num4); j += num5)
		{
			RectTransform val3 = ((LayoutGroup)this).rectChildren[j];
			GetChildSizes(val3, axis, flag, childForceExpand, out var min2, out var preferred2, out var flexible2);
			float num15;
			if (!flag2)
			{
				num15 = 1f;
			}
			else
			{
				localScale = ((Transform)val3).localScale;
				num15 = ((Vector3)(ref localScale))[axis];
			}
			float num16 = num15;
			float num17 = Mathf.Lerp(min2, preferred2, num14);
			num17 += flexible2 * num12;
			if (flag)
			{
				SetChildAlongAxisWithScale(val3, axis, num11, num17, num16);
			}
			else
			{
				float num18 = num17;
				val = val3.sizeDelta;
				float num19 = (num18 - ((Vector2)(ref val))[axis]) * alignmentOnAxis;
				SetChildAlongAxisWithScale(val3, axis, num11 + num19, num16);
			}
			num11 += num17 * num16 + spacing;
		}
	}

	private void GetChildSizes(RectTransform child, int axis, bool controlSize, bool childForceExpand, out float min, out float preferred, out float flexible)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (!controlSize)
		{
			Vector2 sizeDelta = child.sizeDelta;
			min = ((Vector2)(ref sizeDelta))[axis];
			preferred = min;
			flexible = 0f;
		}
		else
		{
			min = LayoutUtility.GetMinSize(child, axis);
			preferred = LayoutUtility.GetPreferredSize(child, axis);
			flexible = LayoutUtility.GetFlexibleSize(child, axis);
		}
		if (childForceExpand)
		{
			flexible = Mathf.Max(flexible, 1f);
		}
	}
}
