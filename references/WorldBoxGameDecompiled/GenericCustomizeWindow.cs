using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GenericCustomizeWindow<TMetaObject, TData, TBanner> : MonoBehaviour where TMetaObject : MetaObject<TData> where TData : MetaObjectData where TBanner : BannerGeneric<TMetaObject, TData>
{
	private bool _created;

	protected Text counter_option_1;

	protected Text counter_option_2;

	protected Text counter_color;

	protected Image image_banner_option_1;

	protected Image image_banner_option_2;

	protected LocalizedText title;

	protected LocalizedText title_option_1;

	protected LocalizedText title_option_2;

	protected LocalizedText title_color;

	protected Transform banner_area;

	protected Image icon_banner;

	protected Image icon_top;

	protected Transform option_1;

	protected Transform option_2;

	protected Transform colors;

	protected Transform colors_parent;

	public TBanner banner;

	private List<ColorElement> _color_elements = new List<ColorElement>();

	protected virtual TMetaObject meta_object
	{
		get
		{
			throw new NotImplementedException("meta_object is not set");
		}
	}

	protected TData data => meta_object.data;

	private MetaCustomizationAsset meta_asset => AssetManager.meta_customization_library.getAsset(meta_type);

	protected virtual MetaType meta_type
	{
		get
		{
			throw new NotImplementedException("meta_type is not set");
		}
	}

	private void OnEnable()
	{
		loadBanner();
		apply();
		int color = banner.color;
		clickColorElement(_color_elements[color], color);
	}

	public int getChangeValue()
	{
		int result = 1;
		if (HotkeyLibrary.many_mod.isHolding())
		{
			result = 5;
		}
		return result;
	}

	protected virtual void apply()
	{
		updateBanner();
		loadBanner();
		updateSelection();
	}

	protected virtual void loadBanner()
	{
		banner.load(meta_object);
	}

	protected virtual void updateColors()
	{
		updateColorsBanner();
	}

	protected virtual void updateColorsBanner()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		ColorAsset color = meta_object.getColor();
		((Graphic)image_banner_option_1).color = color.getColorMainSecond();
		if (meta_asset.option_2_color_editable)
		{
			((Graphic)image_banner_option_2).color = color.getColorBanner();
		}
	}

	private void Awake()
	{
		create();
	}

	private void create()
	{
		if (!_created)
		{
			_created = true;
			setupParts();
			setupButtons();
			setupBanner();
			setupTexts();
			setupImages();
		}
	}

	protected virtual void setupParts()
	{
		Transform val = ((Component)this).transform.FindRecursive("Background").Find("Title");
		title = ((Component)val).GetComponent<LocalizedText>();
		option_1 = ((Component)this).transform.FindRecursive("Option 1");
		option_2 = ((Component)this).transform.FindRecursive("Option 2");
		colors = ((Component)this).transform.FindRecursive("Colors");
		colors_parent = colors.FindRecursive("Colors BG");
		Transform pTransform = ((Component)this).transform.FindRecursive("Banner");
		banner_area = pTransform.FindRecursive("BannerArea");
		icon_banner = ((Component)pTransform.FindRecursive("Icon")).GetComponent<Image>();
		icon_top = ((Component)((Component)this).transform.FindRecursive("Cat")).GetComponent<Image>();
		counter_option_1 = ((Component)option_1.FindRecursive("Counter")).GetComponent<Text>();
		counter_option_2 = ((Component)option_2.FindRecursive("Counter")).GetComponent<Text>();
		counter_color = ((Component)colors.FindRecursive("Counter")).GetComponent<Text>();
		title_option_1 = ((Component)option_1.FindRecursive("Title")).GetComponent<LocalizedText>();
		title_option_2 = ((Component)option_2.FindRecursive("Title")).GetComponent<LocalizedText>();
		title_color = ((Component)colors.FindRecursive("Title")).GetComponent<LocalizedText>();
		image_banner_option_1 = ((Component)option_1.FindRecursive("Image")).GetComponent<Image>();
		image_banner_option_2 = ((Component)option_2.FindRecursive("Image")).GetComponent<Image>();
		((Component)option_1).gameObject.SetActive(meta_asset.option_1_editable);
		((Component)option_2).gameObject.SetActive(meta_asset.option_2_editable);
		((Component)colors).gameObject.SetActive(meta_asset.color_editable);
	}

	protected virtual void setupButtons()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Expected O, but got Unknown
		((UnityEvent)((Component)option_1.FindRecursive("Left")).GetComponent<Button>().onClick).AddListener(new UnityAction(option1Left));
		((UnityEvent)((Component)option_1.FindRecursive("Right")).GetComponent<Button>().onClick).AddListener(new UnityAction(option1Right));
		((UnityEvent)((Component)option_2.FindRecursive("Left")).GetComponent<Button>().onClick).AddListener(new UnityAction(option2Left));
		((UnityEvent)((Component)option_2.FindRecursive("Right")).GetComponent<Button>().onClick).AddListener(new UnityAction(option2Right));
		((UnityEvent)((Component)((Component)this).transform.FindRecursive("Randomize")).GetComponent<Button>().onClick).AddListener(new UnityAction(randomize));
		ColorElement color_element_prefab = ((Component)this).GetComponentInParent<CustomizeWindow>().color_element_prefab;
		for (int i = 0; i < meta_asset.color_count(); i++)
		{
			int tIndex = i;
			ColorAsset colorByIndex = meta_asset.color_library().getColorByIndex(tIndex);
			Color colorMainSecond = colorByIndex.getColorMainSecond();
			Color pInner = Color32.op_Implicit(colorByIndex.getColorBorderInsideAlpha32());
			ColorElement tColorElement = Object.Instantiate<ColorElement>(color_element_prefab, colors_parent);
			_color_elements.Add(tColorElement);
			tColorElement.setColor(colorMainSecond, pInner);
			tColorElement.index = i;
			tColorElement.asset = meta_asset;
			tColorElement.setAction((UnityAction)delegate
			{
				clickColorElement(tColorElement, tIndex);
			});
			((Component)tColorElement).gameObject.GetComponent<TipButton>().setHoverAction(tColorElement.showTooltip, pAddAnimation: false);
		}
	}

	protected virtual void setupBanner()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		TBanner val = Resources.Load<TBanner>(meta_asset.banner_prefab_id);
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogWarning((object)("Banner prefab for " + meta_asset.banner_prefab_id + " could not be found"));
		}
		banner = Object.Instantiate<TBanner>(val);
		banner.enable_default_click = false;
		((Component)banner).transform.localScale = Vector3.one;
		((Component)banner).transform.SetParent(banner_area);
		LayoutElement obj = ((Component)banner).gameObject.AddComponent<LayoutElement>();
		obj.ignoreLayout = true;
		((Behaviour)obj).enabled = false;
		((Component)banner).gameObject.AddComponent<DragSnapElement>().fly_back_parent = ((Component)banner).transform.FindParentWithName("Viewport");
		RectTransform component = ((Component)banner).GetComponent<RectTransform>();
		component.SetAnchor(AnchorPresets.MiddleCenter);
		((Transform)component).localScale = Vector3.one;
	}

	protected virtual void setupTexts()
	{
		title.setKeyAndUpdate(meta_asset.title_locale);
		if (meta_asset.option_1_editable)
		{
			title_option_1.setKeyAndUpdate(meta_asset.option_1_locale);
		}
		if (meta_asset.option_2_editable)
		{
			title_option_2.setKeyAndUpdate(meta_asset.option_2_locale);
		}
		if (meta_asset.color_editable)
		{
			title_color.setKeyAndUpdate(meta_asset.color_locale);
		}
	}

	protected virtual void setupImages()
	{
		icon_banner.sprite = SpriteTextureLoader.getSprite("ui/Icons/" + meta_asset.icon_banner);
		icon_top.sprite = SpriteTextureLoader.getSprite("ui/Icons/" + meta_asset.icon_creature);
	}

	protected virtual void updateCounters()
	{
		if (meta_asset.option_1_editable)
		{
			counter_option_1.text = meta_asset.option_1_get() + 1 + "/" + meta_asset.option_1_count();
		}
		if (meta_asset.option_2_editable)
		{
			counter_option_2.text = meta_asset.option_2_get() + 1 + "/" + meta_asset.option_2_count();
		}
		if (meta_asset.color_editable)
		{
			counter_color.text = meta_asset.color_get() + 1 + "/" + meta_asset.color_count();
		}
	}

	protected virtual void updateBanner()
	{
		banner.normalize();
		banner.updateColor();
	}

	protected virtual void updateSelection()
	{
		updateCounters();
		updateColors();
		onBannerChange();
	}

	protected virtual void onBannerChange()
	{
	}

	public void randomize()
	{
		meta_object.generateBanner();
		ColorAsset random = meta_asset.color_library().list.GetRandom();
		banner.color = meta_asset.color_library().list.IndexOf(random);
		reselectAllColors();
		_color_elements[banner.color].setSelected(pSelected: true);
		apply();
	}

	public void option1Left()
	{
		ref TBanner reference = ref banner;
		int num = reference.option_1 - getChangeValue();
		reference.option_1 = num;
		apply();
	}

	public void option1Right()
	{
		ref TBanner reference = ref banner;
		int num = reference.option_1 + getChangeValue();
		reference.option_1 = num;
		apply();
	}

	public void option2Left()
	{
		ref TBanner reference = ref banner;
		int num = reference.option_2 - getChangeValue();
		reference.option_2 = num;
		apply();
	}

	public void option2Right()
	{
		ref TBanner reference = ref banner;
		int num = reference.option_2 + getChangeValue();
		reference.option_2 = num;
		apply();
	}

	private void colorSet(int pIndex)
	{
		banner.color = pIndex;
		apply();
	}

	private void reselectAllColors()
	{
		foreach (ColorElement color_element in _color_elements)
		{
			color_element.setSelected(pSelected: false);
		}
	}

	private void clickColorElement(ColorElement pElement, int pIndex)
	{
		reselectAllColors();
		colorSet(pIndex);
		pElement.setSelected(pSelected: true);
	}
}
