using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace LayoutGroupExt;

[AddComponentMenu("Layout/Grid Layout Group ( Extended )", 152)]
public class GridLayoutGroupExtended : LayoutGroupExtended
{
	public enum Corner
	{
		UpperLeft,
		UpperRight,
		LowerLeft,
		LowerRight
	}

	public enum Axis
	{
		Horizontal,
		Vertical
	}

	public enum Constraint
	{
		Flexible,
		FixedColumnCount,
		FixedRowCount
	}

	private TweenerCore<Vector3, Vector3, VectorOptions>[] _axis_tween;

	[SerializeField]
	protected Corner m_StartCorner;

	[SerializeField]
	protected Axis m_StartAxis;

	[SerializeField]
	protected Vector2 m_CellSize;

	[SerializeField]
	protected Vector2 m_Spacing;

	[SerializeField]
	protected Constraint m_Constraint;

	[SerializeField]
	protected int m_ConstraintCount;

	public Corner startCorner
	{
		get
		{
			return m_StartCorner;
		}
		set
		{
			((LayoutGroup)this).SetProperty<Corner>(ref m_StartCorner, value);
		}
	}

	public Axis startAxis
	{
		get
		{
			return m_StartAxis;
		}
		set
		{
			((LayoutGroup)this).SetProperty<Axis>(ref m_StartAxis, value);
		}
	}

	public Vector2 cellSize
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_CellSize;
		}
		set
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			((LayoutGroup)this).SetProperty<Vector2>(ref m_CellSize, value);
		}
	}

	public Vector2 spacing
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_Spacing;
		}
		set
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			((LayoutGroup)this).SetProperty<Vector2>(ref m_Spacing, value);
		}
	}

	public Constraint constraint
	{
		get
		{
			return m_Constraint;
		}
		set
		{
			((LayoutGroup)this).SetProperty<Constraint>(ref m_Constraint, value);
		}
	}

	public int constraintCount
	{
		get
		{
			return m_ConstraintCount;
		}
		set
		{
			((LayoutGroup)this).SetProperty<int>(ref m_ConstraintCount, Mathf.Max(1, value));
		}
	}

	protected GridLayoutGroupExtended()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		_axis_tween = new TweenerCore<Vector3, Vector3, VectorOptions>[2];
		m_CellSize = new Vector2(100f, 100f);
		m_Spacing = Vector2.zero;
		m_ConstraintCount = 2;
		base._002Ector();
	}

	public override void CalculateLayoutInputHorizontal()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		base.CalculateLayoutInputHorizontal();
		int num = 0;
		int num2 = 0;
		if (m_Constraint == Constraint.FixedColumnCount)
		{
			num = (num2 = m_ConstraintCount);
		}
		else if (m_Constraint == Constraint.FixedRowCount)
		{
			num = (num2 = Mathf.CeilToInt((float)((LayoutGroup)this).rectChildren.Count / (float)m_ConstraintCount - 0.001f));
		}
		else
		{
			num = 1;
			num2 = Mathf.CeilToInt(Mathf.Sqrt((float)((LayoutGroup)this).rectChildren.Count));
		}
		((LayoutGroup)this).SetLayoutInputForAxis((float)((LayoutGroup)this).padding.horizontal + (cellSize.x + spacing.x) * (float)num - spacing.x, (float)((LayoutGroup)this).padding.horizontal + (cellSize.x + spacing.x) * (float)num2 - spacing.x, -1f, 0);
	}

	public override void CalculateLayoutInputVertical()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		if (m_Constraint == Constraint.FixedColumnCount)
		{
			num = Mathf.CeilToInt((float)((LayoutGroup)this).rectChildren.Count / (float)m_ConstraintCount - 0.001f);
		}
		else if (m_Constraint == Constraint.FixedRowCount)
		{
			num = m_ConstraintCount;
		}
		else
		{
			Rect rect = ((LayoutGroup)this).rectTransform.rect;
			float width = ((Rect)(ref rect)).width;
			int num2 = Mathf.Max(1, Mathf.FloorToInt((width - (float)((LayoutGroup)this).padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
			num = Mathf.CeilToInt((float)((LayoutGroup)this).rectChildren.Count / (float)num2);
		}
		float num3 = (float)((LayoutGroup)this).padding.vertical + (cellSize.y + spacing.y) * (float)num - spacing.y;
		TweenLayoutInputForAxis(num3, num3, -1f, 1);
	}

	private void TweenLayoutInputForAxis(float totalMin, float totalPreferred, float totalFlexible, int axis)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (!Application.isPlaying)
		{
			((LayoutGroup)this).SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
			return;
		}
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(totalMin, totalPreferred, totalFlexible);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(((LayoutGroup)this).GetTotalMinSize(axis), ((LayoutGroup)this).GetTotalPreferredSize(axis), ((LayoutGroup)this).GetTotalFlexibleSize(axis));
		if (val == val2)
		{
			return;
		}
		TweenerCore<Vector3, Vector3, VectorOptions> val3 = _axis_tween[axis];
		if (TweenExtensions.IsActive((Tween)(object)val3))
		{
			if (val3.endValue == val)
			{
				return;
			}
			TweenExtensions.Kill((Tween)(object)val3, false);
		}
		_axis_tween[axis] = DOPreferredSize(val, moveDuration * 0.5f, axis);
	}

	private TweenerCore<Vector3, Vector3, VectorOptions> DOPreferredSize(Vector3 endValue, float duration, int axis)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		TweenerCore<Vector3, Vector3, VectorOptions> obj = DOTween.To((DOGetter<Vector3>)delegate
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(((LayoutGroup)this).GetTotalMinSize(axis), ((LayoutGroup)this).GetTotalPreferredSize(axis), ((LayoutGroup)this).GetTotalFlexibleSize(axis));
		}, (DOSetter<Vector3>)delegate(Vector3 x)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			((LayoutGroup)this).SetLayoutInputForAxis(x.x, x.y, x.z, axis);
		}, endValue, duration);
		TweenSettingsExtensions.OnUpdate<TweenerCore<Vector3, Vector3, VectorOptions>>(obj, (TweenCallback)delegate
		{
			((LayoutGroup)this).SetDirty();
		});
		TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(obj, (TweenCallback)delegate
		{
			((LayoutGroup)this).SetDirty();
		});
		TweenSettingsExtensions.SetTarget<TweenerCore<Vector3, Vector3, VectorOptions>>(obj, (object)this);
		return obj;
	}

	protected override void OnDisable()
	{
		TweenExtensions.Kill((Tween)(object)_axis_tween[0], false);
		TweenExtensions.Kill((Tween)(object)_axis_tween[1], false);
		base.OnDisable();
	}

	public override void SetLayoutHorizontal()
	{
		SetCellsAlongAxis(0);
	}

	public override void SetLayoutVertical()
	{
		SetCellsAlongAxis(1);
	}

	private void SetCellsAlongAxis(int axis)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		int count = ((LayoutGroup)this).rectChildren.Count;
		if (axis == 0)
		{
			for (int i = 0; i < count; i++)
			{
				RectTransform val = ((LayoutGroup)this).rectChildren[i];
				((DrivenRectTransformTracker)(ref ((LayoutGroup)this).m_Tracker)).Add((Object)(object)this, val, (DrivenTransformProperties)16134);
				val.anchorMin = Vector2.up;
				val.anchorMax = Vector2.up;
				val.sizeDelta = cellSize;
			}
			return;
		}
		Rect rect = ((LayoutGroup)this).rectTransform.rect;
		float x = ((Rect)(ref rect)).size.x;
		rect = ((LayoutGroup)this).rectTransform.rect;
		float y = ((Rect)(ref rect)).size.y;
		int num = 1;
		int num2 = 1;
		if (m_Constraint == Constraint.FixedColumnCount)
		{
			num = m_ConstraintCount;
			if (count > num)
			{
				num2 = count / num + ((count % num > 0) ? 1 : 0);
			}
		}
		else if (m_Constraint != Constraint.FixedRowCount)
		{
			num = ((!(cellSize.x + spacing.x <= 0f)) ? Mathf.Max(1, Mathf.FloorToInt((x - (float)((LayoutGroup)this).padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x))) : int.MaxValue);
			num2 = ((!(cellSize.y + spacing.y <= 0f)) ? Mathf.Max(1, Mathf.FloorToInt((y - (float)((LayoutGroup)this).padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y))) : int.MaxValue);
		}
		else
		{
			num2 = m_ConstraintCount;
			if (count > num2)
			{
				num = count / num2 + ((count % num2 > 0) ? 1 : 0);
			}
		}
		int num3 = (int)startCorner % 2;
		int num4 = (int)startCorner / 2;
		int num5;
		int num6;
		int num7;
		if (startAxis == Axis.Horizontal)
		{
			num5 = num;
			num6 = Mathf.Clamp(num, 1, count);
			num7 = ((m_Constraint != Constraint.FixedRowCount) ? Mathf.Clamp(num2, 1, Mathf.CeilToInt((float)count / (float)num5)) : Mathf.Min(num2, count));
		}
		else
		{
			num5 = num2;
			num7 = Mathf.Clamp(num2, 1, count);
			num6 = ((m_Constraint != Constraint.FixedColumnCount) ? Mathf.Clamp(num, 1, Mathf.CeilToInt((float)count / (float)num5)) : Mathf.Min(num, count));
		}
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector((float)num6 * cellSize.x + (float)(num6 - 1) * spacing.x, (float)num7 * cellSize.y + (float)(num7 - 1) * spacing.y);
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector(((LayoutGroup)this).GetStartOffset(0, val2.x), ((LayoutGroup)this).GetStartOffset(1, val2.y));
		int num8 = 0;
		if (count > m_ConstraintCount && Mathf.CeilToInt((float)count / (float)num5) < m_ConstraintCount)
		{
			num8 = m_ConstraintCount - Mathf.CeilToInt((float)count / (float)num5);
			num8 += Mathf.FloorToInt((float)num8 / ((float)num5 - 1f));
			if (count % num5 == 1)
			{
				num8++;
			}
		}
		for (int j = 0; j < count; j++)
		{
			int num9;
			int num10;
			if (startAxis == Axis.Horizontal)
			{
				if (m_Constraint == Constraint.FixedRowCount && count - j <= num8)
				{
					num9 = 0;
					num10 = m_ConstraintCount - (count - j);
				}
				else
				{
					num9 = j % num5;
					num10 = j / num5;
				}
			}
			else if (m_Constraint == Constraint.FixedColumnCount && count - j <= num8)
			{
				num9 = m_ConstraintCount - (count - j);
				num10 = 0;
			}
			else
			{
				num9 = j / num5;
				num10 = j % num5;
			}
			if (num3 == 1)
			{
				num9 = num6 - 1 - num9;
			}
			if (num4 == 1)
			{
				num10 = num7 - 1 - num10;
			}
			RectTransform rect2 = ((LayoutGroup)this).rectChildren[j];
			float y2 = val3.y;
			Vector2 val4 = cellSize;
			float num11 = ((Vector2)(ref val4))[1];
			val4 = spacing;
			float pos = y2 + (num11 + ((Vector2)(ref val4))[1]) * (float)num10;
			val4 = cellSize;
			SetChildAlongAxis(rect2, 1, pos, ((Vector2)(ref val4))[1]);
			RectTransform rect3 = ((LayoutGroup)this).rectChildren[j];
			float x2 = val3.x;
			val4 = cellSize;
			float num12 = ((Vector2)(ref val4))[0];
			val4 = spacing;
			float pos2 = x2 + (num12 + ((Vector2)(ref val4))[0]) * (float)num9;
			val4 = cellSize;
			SetChildAlongAxis(rect3, 0, pos2, ((Vector2)(ref val4))[0]);
		}
	}
}
