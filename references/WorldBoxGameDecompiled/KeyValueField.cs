using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class KeyValueField : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	public Color odd_color;

	public Color even_color;

	public Color highlight_color;

	public Image background;

	public Image icon;

	public Image icon_secondary;

	public Text name_text;

	public Text value;

	public bool auto_odd_even_coloring;

	public UnityAction on_hover_value;

	public UnityAction on_hover_value_out;

	public UnityAction on_click_value;

	private Color _not_highlight_color;

	private LocalizedText _name_text;

	private LocalizedText _value;

	private bool _check_language;

	private void Awake()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		Button component = ((Component)value).GetComponent<Button>();
		if (Input.mousePresent)
		{
			component.OnHover((UnityAction)delegate
			{
				if (InputHelpers.mouseSupported)
				{
					UnityAction obj = on_hover_value;
					if (obj != null)
					{
						obj.Invoke();
					}
				}
			});
			component.OnHoverOut((UnityAction)delegate
			{
				if (InputHelpers.mouseSupported)
				{
					UnityAction obj = on_hover_value_out;
					if (obj != null)
					{
						obj.Invoke();
					}
				}
			});
		}
		((UnityEvent)component.onClick).AddListener((UnityAction)delegate
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			if (!InputHelpers.mouseSupported && !Tooltip.isShowingFor(this))
			{
				UnityAction obj = on_hover_value;
				if (obj != null)
				{
					obj.Invoke();
				}
				EventSystem.current.SetSelectedGameObject(((Component)this).gameObject);
			}
			else
			{
				setBackgroundColor(_not_highlight_color);
				UnityAction obj2 = on_click_value;
				if (obj2 != null)
				{
					obj2.Invoke();
				}
			}
		});
		_name_text = ((Component)name_text).GetComponent<LocalizedText>();
		_value = ((Component)value).GetComponent<LocalizedText>();
		_check_language = (Object)(object)_name_text != (Object)null || (Object)(object)_value != (Object)null;
	}

	private void OnEnable()
	{
		checkLanguage();
		if (auto_odd_even_coloring)
		{
			checkOddEvenColor(((Component)this).transform.GetActiveSiblingIndex());
		}
	}

	private void OnDisable()
	{
		on_hover_value = null;
		on_hover_value_out = null;
		on_click_value = null;
	}

	private void checkLanguage()
	{
		if (_check_language)
		{
			_name_text?.checkSpecialLanguages();
			_value?.checkSpecialLanguages();
		}
	}

	public void checkOddEvenColor(int pIndex)
	{
		if (pIndex % 2 != 0)
		{
			setEvenColor();
		}
		else
		{
			setOddColor();
		}
	}

	public void OnPointerEnter(PointerEventData pData)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (InputHelpers.mouseSupported)
		{
			_not_highlight_color = ((Graphic)background).color;
			setHighlightColor();
		}
	}

	public void OnPointerExit(PointerEventData pData)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (InputHelpers.mouseSupported)
		{
			setBackgroundColor(_not_highlight_color);
		}
	}

	public void OnSelect(BaseEventData pEventData)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!InputHelpers.mouseSupported)
		{
			_not_highlight_color = ((Graphic)background).color;
			setHighlightColor();
		}
	}

	public void OnDeselect(BaseEventData pEventData)
	{
		if (!InputHelpers.mouseSupported)
		{
			setNotHighlightColor();
		}
	}

	public void setEvenColor()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		setBackgroundColor(even_color);
	}

	public void setOddColor()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		setBackgroundColor(odd_color);
	}

	public void setHighlightColor()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		setBackgroundColor(highlight_color);
	}

	public void setNotHighlightColor()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		setBackgroundColor(_not_highlight_color);
	}

	private void setBackgroundColor(Color pColor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)background).color = pColor;
	}

	public void setMetaForTooltip(MetaType pMetaType, long pMetaId, string pTooltipId = null, TooltipDataGetter pData = null)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		on_hover_value = null;
		on_hover_value_out = new UnityAction(Tooltip.hideTooltip);
		on_click_value = null;
		if (!pMetaType.isNone())
		{
			MetaTypeAsset tAsset = AssetManager.meta_type_library.getAsset(pMetaType);
			on_hover_value = (UnityAction)delegate
			{
				tAsset.stat_hover(pMetaId, (MonoBehaviour)(object)this);
			};
			on_click_value = (UnityAction)delegate
			{
				tAsset.stat_click(pMetaId, (MonoBehaviour)(object)this);
			};
		}
		else if (!string.IsNullOrEmpty(pTooltipId))
		{
			on_hover_value = (UnityAction)delegate
			{
				Tooltip.show(this, pTooltipId, pData());
			};
		}
	}

	public KeyValueField()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		odd_color = Toolbox.makeColor("#000000", 0f);
		even_color = Toolbox.makeColor("#30322B");
		highlight_color = Toolbox.makeColor("#111111");
		_check_language = true;
		((MonoBehaviour)this)._002Ector();
	}
}
