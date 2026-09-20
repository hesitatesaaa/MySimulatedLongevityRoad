using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class BannerGeneric<TMetaObject, TData> : BannerBase where TMetaObject : CoreSystemObject<TData> where TData : BaseSystemData
{
	protected TMetaObject meta_object;

	private bool _created;

	protected Image part_background;

	protected Image part_icon;

	protected Image part_frame;

	public bool enable_default_click = true;

	[SerializeField]
	private bool _enable_customize_click;

	public bool enable_tab_show_click;

	protected TData data => meta_object.data;

	protected virtual string tooltip_id
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	private void Start()
	{
		create();
	}

	private void create()
	{
		if (!_created)
		{
			_created = true;
			setupParts();
			setupClick();
			setupTooltip();
		}
	}

	protected virtual void setupClick()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		if (!enable_default_click && !_enable_customize_click && !enable_tab_show_click)
		{
			return;
		}
		Button componentInChildren = ((Component)this).GetComponentInChildren<Button>();
		if (!((Object)(object)componentInChildren == (Object)null))
		{
			if (enable_default_click)
			{
				((UnityEvent)componentInChildren.onClick).AddListener(new UnityAction(clickAction));
			}
			else if (_enable_customize_click)
			{
				((UnityEvent)componentInChildren.onClick).AddListener(new UnityAction(clickCustomize));
			}
			else if (enable_tab_show_click)
			{
				((UnityEvent)componentInChildren.onClick).AddListener(new UnityAction(clickShowTab));
			}
		}
	}

	protected virtual void clickAction()
	{
		if (!meta_object.hasDied())
		{
			if (!InputHelpers.mouseSupported)
			{
				switchOnDoubleTap();
			}
			else
			{
				showMetaWindow();
			}
		}
	}

	private void switchOnDoubleTap()
	{
		if (!Tooltip.isShowingFor(this))
		{
			tooltipAction();
		}
		else
		{
			showMetaWindow();
		}
	}

	private void showMetaWindow()
	{
		base.meta_type_asset.set_selected(meta_object);
		string window_name = base.meta_type_asset.window_name;
		if (ScrollWindow.isCurrentWindow(window_name))
		{
			ScrollWindow.get(window_name).showSameWindow();
		}
		else
		{
			ScrollWindow.showWindow(window_name);
		}
	}

	protected virtual void clickCustomize()
	{
		string customize_window_id = base.meta_asset.customize_window_id;
		if (customize_window_id == string.Empty)
		{
			Debug.LogError((object)("var " + customize_window_id + " is not set!"));
		}
		else
		{
			ScrollWindow.showWindow(customize_window_id);
		}
	}

	private void clickShowTab()
	{
		if (HotkeyLibrary.isHoldingControlForSelection())
		{
			clickAction();
			return;
		}
		base.meta_type_asset.selectAndInspect(meta_object);
		Tooltip.blockTooltips(0.5f);
		Tooltip.hideTooltipNow();
	}

	protected virtual void setupParts()
	{
		loadPartBackground();
		loadPartFrame();
		loadPartIcon();
	}

	protected virtual void loadPartFrame()
	{
		Transform obj = ((Component)this).transform.FindRecursive("Frame");
		part_frame = ((obj != null) ? ((Component)obj).GetComponent<Image>() : null);
	}

	protected virtual void loadPartBackground()
	{
		Transform obj = ((Component)this).transform.FindRecursive("Background");
		part_background = ((obj != null) ? ((Component)obj).GetComponent<Image>() : null);
	}

	protected virtual void loadPartIcon()
	{
		Transform obj = ((Component)this).transform.FindRecursive("Icon");
		part_icon = ((obj != null) ? ((Component)obj).GetComponent<Image>() : null);
	}

	protected virtual void setupBanner()
	{
	}

	public override void load(NanoObject pObject)
	{
		setMetaObject(pObject);
		create();
		setupBanner();
	}

	public override NanoObject GetNanoObject()
	{
		return meta_object;
	}

	protected virtual void setupTooltip()
	{
		TipButton tipButton = default(TipButton);
		if (!((Component)this).TryGetComponent<TipButton>(ref tipButton))
		{
			return;
		}
		tipButton.setHoverAction(delegate
		{
			if (InputHelpers.mouseSupported)
			{
				tooltipAction();
			}
		});
	}

	protected virtual void tooltipAction()
	{
		Tooltip.show(this, tooltip_id, getTooltipData());
	}

	protected virtual TooltipData getTooltipData()
	{
		CustomDataContainer<bool> customDataContainer = new CustomDataContainer<bool>();
		customDataContainer["tab_banner"] = enable_tab_show_click;
		return new TooltipData
		{
			custom_data_bool = customDataContainer
		};
	}

	public override void showTooltip()
	{
		tooltipAction();
	}

	protected void setMetaObject(NanoObject pObject)
	{
		meta_object = (TMetaObject)pObject;
	}

	internal virtual void normalize()
	{
		if (base.meta_asset.option_1_editable)
		{
			int num = base.meta_asset.option_1_count();
			if (base.option_1 < 0)
			{
				base.option_1 += num;
			}
			if (base.option_1 > num - 1)
			{
				base.option_1 -= num;
			}
			base.option_1 = Mathf.Clamp(base.option_1, 0, num - 1);
		}
		if (base.meta_asset.option_2_editable)
		{
			int num2 = base.meta_asset.option_2_count();
			if (base.option_2 < 0)
			{
				base.option_2 += num2;
			}
			if (base.option_2 > num2 - 1)
			{
				base.option_2 -= num2;
			}
			base.option_2 = Mathf.Clamp(base.option_2, 0, num2 - 1);
		}
		if (base.meta_asset.color_editable)
		{
			int num3 = base.meta_asset.color_count();
			if (base.color < 0)
			{
				base.color += num3;
			}
			if (base.color > num3 - 1)
			{
				base.color -= num3;
			}
			base.color = Mathf.Clamp(base.color, 0, num3 - 1);
		}
	}

	internal virtual void updateColor()
	{
		ColorAsset pColor = base.meta_asset.color_library().list[base.color];
		if (meta_object.updateColor(pColor))
		{
			base.meta_asset.on_new_color();
		}
	}
}
