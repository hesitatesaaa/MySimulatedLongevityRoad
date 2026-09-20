using System.Collections.Generic;
using LayoutGroupExt;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FlexibleOneRowGrid : MonoBehaviour, ILayoutController
{
	public bool debug;

	public int bonus_spacing_x;

	private RectTransform _grid_rect;

	private GridLayoutGroup _grid;

	private GridLayoutGroupExtended _grid_extended;

	private bool _is_extended;

	private bool _initialized;

	private void Awake()
	{
		init();
	}

	private void init()
	{
		if (!_initialized)
		{
			_initialized = true;
			if (((Component)(object)this).HasComponent<GridLayoutGroup>())
			{
				_grid = ((Component)this).GetComponent<GridLayoutGroup>();
				_grid_rect = ((Component)_grid).GetComponent<RectTransform>();
			}
			else
			{
				_grid_extended = ((Component)this).GetComponent<GridLayoutGroupExtended>();
				_grid_rect = ((Component)_grid_extended).GetComponent<RectTransform>();
				_is_extended = true;
			}
		}
	}

	public void SetLayoutHorizontal()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if (debug || Application.isPlaying)
		{
			init();
			float num = (_is_extended ? _grid_extended.cellSize.x : _grid.cellSize.x);
			Rect rect = _grid_rect.rect;
			float width = ((Rect)(ref rect)).width;
			float num2 = calculateChildren();
			float num3 = 0f;
			float num4 = num * num2 + (float)bonus_spacing_x * (num2 - 1f);
			if (num4 < width)
			{
				num3 = bonus_spacing_x;
			}
			else
			{
				num4 = num * num2;
				num3 = (width - num4) / (num2 - 1f);
			}
			if (_is_extended)
			{
				_grid_extended.spacing = new Vector2(num3, 0f);
			}
			else
			{
				_grid.spacing = new Vector2(num3, 0f);
			}
		}
	}

	public float calculateChildren()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		List<Component> list = CollectionPool<List<Component>, Component>.Get();
		int num = 0;
		int i = 0;
		for (int childCount = ((Transform)_grid_rect).childCount; i < childCount; i++)
		{
			Transform child = ((Transform)_grid_rect).GetChild(i);
			RectTransform val = (RectTransform)(object)((child is RectTransform) ? child : null);
			if ((Object)(object)val == (Object)null || !((Component)val).gameObject.activeInHierarchy)
			{
				continue;
			}
			if (!((Component)(object)val).HasComponent<ILayoutIgnorer>())
			{
				num++;
				continue;
			}
			((Component)val).GetComponents(typeof(ILayoutIgnorer), list);
			for (int j = 0; j < list.Count; j++)
			{
				if (!((ILayoutIgnorer)list[j]).ignoreLayout)
				{
					num++;
					break;
				}
			}
			list.Clear();
		}
		CollectionPool<List<Component>, Component>.Release(list);
		return num;
	}

	public void SetLayoutVertical()
	{
	}
}
