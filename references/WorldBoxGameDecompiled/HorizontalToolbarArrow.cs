using UnityEngine;

public class HorizontalToolbarArrow : ToolbarArrow
{
	public bool is_left = true;

	protected override void Update()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		float num = iTween.easeInOutCirc(0f, hide_position.x, timer);
		if (arrow_transform.localPosition.x != num)
		{
			arrow_transform.localPosition = new Vector3(num, 0f);
		}
	}

	protected override float getScrollPosition()
	{
		return scroll_rect.horizontalNormalizedPosition;
	}

	protected override void setScrollPosition(float pValue)
	{
		scroll_rect.horizontalNormalizedPosition = pValue;
	}

	protected override float getEndPosition()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		float scrollPosition = getScrollPosition();
		float num = (float)Screen.width / CanvasMain.instance.canvas_ui.scaleFactor;
		Rect rect = scroll_rect.content.rect;
		float num2 = num / ((Rect)(ref rect)).width;
		if (is_left)
		{
			return scrollPosition - Mathf.Min(num2, 0.5f);
		}
		return scrollPosition + Mathf.Min(num2, 0.5f);
	}

	protected override void onScroll(Vector2 pVal)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Screen.width / CanvasMain.instance.canvas_ui.scaleFactor;
		should_show = true;
		Rect rect = scroll_rect.content.rect;
		if (((Rect)(ref rect)).width < num)
		{
			should_show = false;
		}
		else if (is_left)
		{
			if (getScrollPosition() > 0.1f)
			{
				should_show = true;
			}
			else
			{
				should_show = false;
			}
		}
		else if (getScrollPosition() == 1f)
		{
			should_show = false;
		}
		else if (getScrollPosition() < 0.98f)
		{
			should_show = true;
		}
		else
		{
			should_show = false;
		}
	}
}
