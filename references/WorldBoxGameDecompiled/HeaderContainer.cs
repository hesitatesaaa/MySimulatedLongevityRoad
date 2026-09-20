using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeaderContainer : MonoBehaviour, ILayoutController
{
	public RectTransform header_transform;

	public RectTransform content_transform;

	public RectTransform runes_container;

	public VerticalLayoutGroup content;

	private VerticalLayoutGroup _vertical_layout_group;

	private LayoutElement _layout_element;

	private int _default_top_padding;

	private RectOffset _default_padding;

	private void Awake()
	{
		if ((Object)(object)content == (Object)null)
		{
			content = ((Component)content_transform).GetComponent<VerticalLayoutGroup>();
		}
		_vertical_layout_group = ((Component)this).GetComponent<VerticalLayoutGroup>();
		_default_padding = ((LayoutGroup)_vertical_layout_group).padding;
		_layout_element = ((Component)this).GetComponent<LayoutElement>();
		_default_top_padding = ((LayoutGroup)content).padding.top;
	}

	public void SetLayoutVertical()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		if (!Application.isPlaying)
		{
			return;
		}
		int num = _default_top_padding;
		if (!hasAnyElementActive())
		{
			if (_layout_element.preferredHeight != 0f)
			{
				((LayoutGroup)_vertical_layout_group).padding = new RectOffset(0, 0, 0, 0);
				_layout_element.preferredHeight = 0f;
				LayoutRebuilder.ForceRebuildLayoutImmediate(header_transform);
			}
		}
		else
		{
			if (_layout_element.preferredHeight >= 0f)
			{
				((LayoutGroup)_vertical_layout_group).padding = _default_padding;
				_layout_element.preferredHeight = -1f;
				LayoutRebuilder.ForceRebuildLayoutImmediate(header_transform);
			}
			int num2 = num;
			Rect rect = header_transform.rect;
			num = num2 + (int)((Rect)(ref rect)).height;
		}
		if (((LayoutGroup)content).padding.top != num)
		{
			((LayoutGroup)content).padding.top = num;
			LayoutRebuilder.ForceRebuildLayoutImmediate(content_transform);
			((MonoBehaviour)this).StartCoroutine(toggleRunes());
		}
	}

	public void SetLayoutHorizontal()
	{
	}

	private IEnumerator toggleRunes()
	{
		yield return null;
		bool flag = hasAnyElementActive();
		((Component)runes_container).gameObject.SetActive(flag);
		if (flag)
		{
			RectTransform obj = runes_container;
			float x = ((Transform)runes_container).localPosition.x;
			Rect rect = header_transform.rect;
			((Transform)obj).localPosition = Vector2.op_Implicit(new Vector2(x, 0f - ((Rect)(ref rect)).height));
		}
	}

	private bool hasAnyElementActive()
	{
		for (int i = 0; i < ((Component)this).transform.childCount; i++)
		{
			if (((Component)((Component)this).transform.GetChild(i)).gameObject.activeInHierarchy)
			{
				return true;
			}
		}
		return false;
	}
}
