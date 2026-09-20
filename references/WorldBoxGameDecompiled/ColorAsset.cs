using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ColorAsset : Asset
{
	private static int _create_last_index_id = 1000;

	public int index_id;

	public string color_main;

	public string color_main_2;

	public string color_banner;

	public string color_text;

	public bool favorite;

	[NonSerialized]
	public Color32 k_color_0;

	[NonSerialized]
	public Color32 k_color_1;

	[NonSerialized]
	public Color32 k_color_2;

	[NonSerialized]
	public Color32 k_color_3;

	[NonSerialized]
	public Color32 k_color_4;

	[NonSerialized]
	public Color32 k2_color_0;

	[NonSerialized]
	public Color32 k2_color_1;

	[NonSerialized]
	public Color32 k2_color_2;

	[NonSerialized]
	public Color32 k2_color_3;

	[NonSerialized]
	public Color32 k2_color_4;

	private Color32 _color_main_32;

	private Color32 _color_main_second_32;

	private Color32 _color_unit_32;

	private Color32 _color_border_inside_alpha_32;

	private Color _color_main;

	private Color _color_main_second;

	private Color _color_text;

	private Color _color_minimap_element;

	private Color _color_border_out_capture;

	private Color _color_banner;

	private static readonly List<ColorAsset> _all_colors_list = new List<ColorAsset>();

	private static readonly Dictionary<string, ColorAsset> _all_colors_dict = new Dictionary<string, ColorAsset>();

	public const byte ALPHA_BORDER_INSIDE_BYTE = 170;

	private bool _initialized;

	private Material _material_line;

	private Material _material_gradient;

	public static List<ColorAsset> getAllColorsList()
	{
		return _all_colors_list;
	}

	public ColorAsset()
	{
	}

	public static bool isColorAssetExists(string pColorMain)
	{
		return _all_colors_dict.ContainsKey(pColorMain);
	}

	public static ColorAsset getExistingColorAsset(string pColorMain)
	{
		_all_colors_dict.TryGetValue(pColorMain, out var value);
		return value;
	}

	public static ColorAsset tryMakeNewColorAsset(string pColorMain)
	{
		_all_colors_dict.TryGetValue(pColorMain, out var value);
		if (value == null)
		{
			return new ColorAsset(pColorMain);
		}
		return value;
	}

	private ColorAsset(string pColorMain)
	{
		setMainHexColors(pColorMain, pColorMain, pColorMain);
		index_id = _create_last_index_id++;
		saveToGlobalList(this);
	}

	public static void saveToGlobalList(ColorAsset pAsset, bool pMustBeGlobal = false)
	{
		if (isColorAssetExists(pAsset.color_main))
		{
			if (pMustBeGlobal)
			{
				Debug.LogError((object)("ColorAsset with same <b>color_main</b> already exists in global list: " + pAsset.id + " " + pAsset.index_id + " " + pAsset.color_main));
			}
		}
		else
		{
			_all_colors_list.Add(pAsset);
			_all_colors_dict.Add(pAsset.color_main, pAsset);
		}
	}

	private void setMainHexColors(string pColorMain, string pColorMain2, string pColorBanner)
	{
		color_main = pColorMain;
		color_main_2 = pColorMain2;
		color_banner = pColorBanner;
		color_text = pColorMain;
	}

	public void setEditorColors(Color pMain, Color pMain2, Color pBanner, Color pText)
	{
		initColor();
	}

	public void initColor()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized)
		{
			_initialized = true;
			_color_main = Toolbox.makeColor(color_main);
			_color_main_32 = Color32.op_Implicit(Toolbox.makeColor(color_main));
			_color_main_second = Toolbox.makeColor(color_main_2);
			_color_main_second_32 = Color32.op_Implicit(Toolbox.makeColor(color_main_2));
			_color_text = Toolbox.makeColor(color_text);
			_color_banner = Toolbox.makeColor(color_banner);
			Color val = Color32.op_Implicit(_color_main_32);
			_color_border_inside_alpha_32 = Color32.op_Implicit(new Color(val.r, val.g, val.b));
			_color_border_inside_alpha_32.a = 170;
			Color32 val2 = default(Color32);
			((Color32)(ref val2))._002Ector((byte)30, (byte)30, (byte)30, byte.MaxValue);
			_color_border_out_capture = new Color(_color_main_second.r, _color_main_second.g, _color_main_second.b, 0.8f);
			_color_unit_32 = Color32.op_Implicit(Color.Lerp(_color_main_second, Color.white, 0.3f));
			_color_unit_32.a = byte.MaxValue;
			k_color_0 = Color32.op_Implicit(_color_text);
			k_color_0 = checkIfColorTooDark(k_color_0);
			_color_minimap_element = Color32.op_Implicit(Color32.Lerp(k_color_0, Color32.op_Implicit(Color.white), 0.2f));
			k_color_1 = Color32.Lerp(k_color_0, val2, 0.13f);
			k_color_2 = Color32.Lerp(k_color_0, val2, 0.35000002f);
			k_color_3 = Color32.Lerp(k_color_0, val2, 0.51f);
			k_color_4 = Color32.Lerp(k_color_0, val2, 0.65999997f);
			k2_color_0 = _color_main_32;
			k2_color_0 = checkIfColorTooDark(k2_color_0);
			k2_color_1 = Color32.Lerp(k2_color_0, val2, 0.13f);
			k2_color_2 = Color32.Lerp(k2_color_0, val2, 0.35000002f);
			k2_color_3 = Color32.Lerp(k2_color_0, val2, 0.51f);
			k2_color_4 = Color32.Lerp(k2_color_0, val2, 0.65999997f);
		}
	}

	public Material getChartLineMaterial()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_material_line == (Object)null)
		{
			_material_line = cloneMaterial("materials/graph/graph_base_line");
			Color colorText = getColorText();
			_material_line.SetColor("_Color", colorText);
		}
		return _material_line;
	}

	public Material getChartInnerFillMaterial()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_material_gradient == (Object)null)
		{
			_material_gradient = cloneMaterial("materials/graph/graph_base_gradient");
			Color colorText = getColorText();
			colorText.a = 0.4f;
			Color colorText2 = getColorText();
			colorText2.a = 0.1f;
			_material_gradient.SetColor("_ColorFrom", colorText);
			_material_gradient.SetColor("_ColorTo", colorText2);
		}
		return _material_gradient;
	}

	private Material cloneMaterial(string pPath)
	{
		return Object.Instantiate<Material>(Resources.Load<Material>(pPath));
	}

	private Color32 checkIfColorTooDark(Color32 pColor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (pColor.r < 128 && pColor.g < 128 && pColor.b < 128)
		{
			pColor.r += 50;
			pColor.g += 50;
			pColor.b += 50;
		}
		return pColor;
	}

	private Color32 getDarkerColor(Color32 pColor, byte pValue)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		pColor.r += pValue;
		pColor.g += pValue;
		pColor.b += pValue;
		return pColor;
	}

	public Color32 getColorUnit32()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _color_unit_32;
	}

	public Color32 getColorMain32()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _color_main_32;
	}

	public Color32 getColorBorderInsideAlpha32()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _color_border_inside_alpha_32;
	}

	public Color32 getColorMainSecond32()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _color_main_second_32;
	}

	public Color getColorMainSecond()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _color_main_second;
	}

	public Color getColorMain()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _color_main;
	}

	public Color getColorText()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _color_text;
	}

	public ref Color getColorTextRef()
	{
		return ref _color_text;
	}

	public Color getColorMinimapElements()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _color_minimap_element;
	}

	public Color getColorBorderOut_capture()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _color_border_out_capture;
	}

	public Color getColorBanner()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _color_banner;
	}
}
