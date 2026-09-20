using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityPools;

public class MetaRepresentationContainerBase : StatsRowsContainer
{
	[SerializeField]
	protected MetaType _meta_type;

	[SerializeField]
	private LocalizedText _title;

	[SerializeField]
	private Image _background;

	[SerializeField]
	private Image _prefab_bar;

	[SerializeField]
	private LayoutElement _layout_element;

	protected MetaRepresentationAsset asset;

	protected override void init()
	{
		base.init();
		asset = AssetManager.meta_representation_library.getAsset(_meta_type);
		((Component)_prefab_bar).gameObject.SetActive(false);
		_title.setKeyAndUpdate(asset.getLocaleID());
	}

	protected override void showStats()
	{
		int pTotal = 0;
		bool pAny = false;
		Dictionary<IMetaObject, int> dictionary = UnsafeCollectionPool<Dictionary<IMetaObject, int>, KeyValuePair<IMetaObject, int>>.Get();
		fillDict(ref pTotal, ref pAny, dictionary);
		int num = pTotal;
		foreach (KeyValuePair<IMetaObject, int> item in dictionary.OrderByDescending((KeyValuePair<IMetaObject, int> p) => p.Value))
		{
			IMetaObject key = item.Key;
			int value = item.Value;
			num -= value;
			string pValue = amountWithPercent(value, pTotal);
			string pIconPath = asset.icon_getter(key);
			string pIconSecondaryPath = (asset.show_species_icon ? key.getActorAsset().icon : null);
			string name = key.name;
			name += Toolbox.coloredGreyPart(value, key.getColor().color_text);
			KeyValueField pField = showStatRowTwoIcons(name, pValue, key.getColor().color_text, asset.meta_type, key.getID(), pColorText: true, pIconPath, pIconSecondaryPath, null, null, pLocalize: false);
			showBar(pField, value, pTotal, key.getColor().color_text);
		}
		checkShowNone(pAny, num, pTotal);
		UnsafeCollectionPool<Dictionary<IMetaObject, int>, KeyValuePair<IMetaObject, int>>.Release(dictionary);
		_layout_element.ignoreLayout = !pAny;
		((Behaviour)_background).enabled = pAny;
		((Component)_title).gameObject.SetActive(pAny);
	}

	protected virtual void fillDict(ref int pTotal, ref bool pAny, Dictionary<IMetaObject, int> pDict)
	{
		throw new NotImplementedException();
	}

	protected virtual void checkShowNone(bool pAny, int pNone, int pTotal)
	{
		throw new NotImplementedException();
	}

	protected void showBar(KeyValueField pField, int pAmount, int pTotal, string pColorHex)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		float num = ((pTotal > 0) ? ((float)pAmount / (float)pTotal) : 0f);
		Transform obj = ((Component)pField).transform.Find("gen_percent_bar");
		Image val = ((obj != null) ? ((Component)obj).GetComponent<Image>() : null);
		if ((Object)(object)val == (Object)null)
		{
			val = Object.Instantiate<GameObject>(((Component)_prefab_bar).gameObject, ((Component)pField).transform).GetComponent<Image>();
			((Component)val).gameObject.SetActive(true);
			((Object)val).name = "gen_percent_bar";
		}
		float num2 = 100f * num * 0.5f;
		Vector2 sizeDelta = default(Vector2);
		((Vector2)(ref sizeDelta))._002Ector(num2, 8.5f);
		((Component)val).GetComponent<RectTransform>().sizeDelta = sizeDelta;
		((Component)val).GetComponent<RectTransform>().anchoredPosition = new Vector2(-2f, 0f);
		((Component)val).transform.SetAsFirstSibling();
		Color color = Toolbox.makeColor(pColorHex);
		color.a = 0.4f;
		((Graphic)val).color = color;
	}

	protected string amountWithPercent(int pAmount, int pTotal)
	{
		float pFloat = ((pTotal > 0) ? ((float)pAmount / (float)pTotal * 100f) : 0f);
		if (pTotal == pAmount)
		{
			pFloat = 100f;
		}
		return pFloat.ToText() + "%";
	}

	internal KeyValueField showStatRowTwoIcons(string pId, object pValue, string pColor, MetaType pMetaType = MetaType.None, long pMetaId = -1L, bool pColorText = false, string pIconPath = null, string pIconSecondaryPath = null, string pTooltipId = null, TooltipDataGetter pTooltipData = null, bool pLocalize = true)
	{
		KeyValueField keyValueField = showStatRow(pId, pValue, pColor, pMetaType, pMetaId, pColorText, pIconPath, pTooltipId, pTooltipData, pLocalize);
		bool flag = !string.IsNullOrEmpty(pIconSecondaryPath);
		if (flag)
		{
			Sprite sprite = SpriteTextureLoader.getSprite("ui/Icons/" + pIconSecondaryPath);
			keyValueField.icon_secondary.sprite = sprite;
		}
		((Component)keyValueField.icon_secondary).gameObject.SetActive(flag);
		return keyValueField;
	}

	public void setMetaType(MetaType pType)
	{
		_meta_type = pType;
	}
}
