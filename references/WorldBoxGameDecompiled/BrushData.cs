using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BrushData : Asset, ILocalizedAsset
{
	[DefaultValue(1)]
	public int size;

	[DefaultValue(1)]
	public int drops;

	public BrushGroup group;

	public bool show_in_brush_window;

	public int width;

	public int height;

	public int sqr_size;

	public bool auto_size;

	public bool continuous;

	public bool fast_spawn;

	public string localized_key;

	public BrushPixelData[] pos;

	public BrushGenerateAction generate_action;

	public Vector2 ui_scale;

	public Vector2 ui_size;

	[NonSerialized]
	private Sprite _sprite;

	public void setupImage(Image pSprite)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		pSprite.sprite = getSprite();
		Vector2 val = ui_scale;
		Vector2 val2 = ui_size;
		if (height < 28)
		{
			((Vector2)(ref val2))._002Ector((float)width, (float)height);
		}
		((Graphic)pSprite).rectTransform.sizeDelta = new Vector2(val2.x, val2.y);
		((Component)pSprite).transform.localScale = new Vector3(val.x, val.y, 1f);
	}

	public Sprite getSprite()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_sprite != (Object)null)
		{
			return _sprite;
		}
		Texture2D val = new Texture2D(width, height, (TextureFormat)4, false)
		{
			filterMode = (FilterMode)0,
			wrapMode = (TextureWrapMode)1
		};
		Color[] array = (Color[])(object)new Color[width * height];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Color.clear;
		}
		val.SetPixels(array);
		Color white = Color.white;
		int num = 0;
		int num2 = 0;
		BrushPixelData[] array2 = pos;
		for (int j = 0; j < array2.Length; j++)
		{
			BrushPixelData brushPixelData = array2[j];
			if (brushPixelData.x < num)
			{
				num = brushPixelData.x;
			}
			if (brushPixelData.y < num2)
			{
				num2 = brushPixelData.y;
			}
		}
		array2 = pos;
		for (int j = 0; j < array2.Length; j++)
		{
			BrushPixelData brushPixelData2 = array2[j];
			val.SetPixel(brushPixelData2.x - num, brushPixelData2.y - num2, white);
		}
		val.Apply(false, true);
		Rect val2 = default(Rect);
		((Rect)(ref val2))._002Ector(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height);
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector(0f, 0f);
		_sprite = Sprite.Create(val, val2, val3, 1f);
		((Object)_sprite).name = id;
		return _sprite;
	}

	public string getLocaleID()
	{
		return localized_key;
	}

	public BrushData()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		size = 1;
		drops = 1;
		ui_scale = new Vector2(1f, 1f);
		ui_size = new Vector2(28f, 28f);
		base._002Ector();
	}
}
