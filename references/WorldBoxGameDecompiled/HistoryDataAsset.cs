using System;
using ChartAndGraph;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class HistoryDataAsset : Asset, IDescriptionAsset, ILocalizedAsset
{
	public string localized_key;

	public string localized_key_description;

	public string statistics_asset;

	public string color_hex;

	public string tooltip_color_hex;

	public string path_icon;

	public bool enabled_default;

	private Material _material_point;

	private Material _material_line;

	private Material _material_gradient;

	private ChartItemEffect _hover_prefab;

	public bool average;

	public bool max;

	public bool sum;

	public GraphCategoryGroup category_group = GraphCategoryGroup.General;

	public Material getChartPointMaterial()
	{
		if ((Object)(object)_material_point == (Object)null)
		{
			_material_point = cloneMaterial("materials/graph/graph_base_point");
			_material_point.SetTexture("_MainTex", (Texture)(object)Resources.Load<Texture2D>(path_icon));
		}
		return _material_point;
	}

	public ChartItemEffect getHoverPointMaterial()
	{
		if ((Object)(object)_hover_prefab == (Object)null)
		{
			_hover_prefab = clonePrefab("Prefabs/graph/PointHover", GameObject.Find("Charts").transform);
			((Object)((Component)_hover_prefab).gameObject).name = "Hover " + id;
			((Component)_hover_prefab).GetComponent<Image>().sprite = SpriteTextureLoader.getSprite(path_icon);
			((Component)_hover_prefab).gameObject.SetActive(false);
		}
		return _hover_prefab;
	}

	public Material getChartLineMaterial()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_material_line == (Object)null)
		{
			_material_line = getChartLineMaterial(getColorMain());
		}
		return _material_line;
	}

	public static Material getChartLineMaterial(Color pColor)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Material obj = cloneMaterial("materials/graph/graph_base_line");
		obj.SetColor("_Color", pColor);
		return obj;
	}

	public Material getChartInnerFillMaterial()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_material_gradient == (Object)null)
		{
			_material_gradient = getChartInnerFillMaterial(getColorMain());
		}
		return _material_gradient;
	}

	public static Material getChartInnerFillMaterial(Color pColor)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Material obj = cloneMaterial("materials/graph/graph_base_gradient");
		Color val = pColor;
		val.a = 0.4f;
		Color val2 = pColor;
		val2.a = 0.1f;
		obj.SetColor("_ColorFrom", val);
		obj.SetColor("_ColorTo", val2);
		return obj;
	}

	public string getLocaleID()
	{
		return localized_key ?? id;
	}

	public string getDescriptionID()
	{
		string text = getLocaleID() + "_description";
		if (!string.IsNullOrEmpty(localized_key_description))
		{
			text = localized_key_description;
		}
		if (LocalizedTextManager.stringExists(text))
		{
			return text;
		}
		return getLocaleID() + "_description";
	}

	public Color getColorMain()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Toolbox.makeColor(color_hex);
	}

	private static Material cloneMaterial(string pPath)
	{
		return Object.Instantiate<Material>(Resources.Load<Material>(pPath));
	}

	private static ChartItemEffect clonePrefab(string pPath, Transform pParentTransform)
	{
		return Object.Instantiate<ChartItemEffect>(Resources.Load<ChartItemEffect>(pPath), pParentTransform);
	}
}
