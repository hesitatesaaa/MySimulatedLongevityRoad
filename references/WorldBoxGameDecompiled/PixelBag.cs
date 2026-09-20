using UnityEngine;

public class PixelBag
{
	public readonly int texture_rect_width;

	public readonly int texture_rect_height;

	public readonly Pixel[] arr_pixels_normal;

	public readonly Pixel[] arr_pixels_light;

	public readonly Pixel[] arr_pixels_k1_0;

	public readonly Pixel[] arr_pixels_k1_1;

	public readonly Pixel[] arr_pixels_k1_2;

	public readonly Pixel[] arr_pixels_k1_3;

	public readonly Pixel[] arr_pixels_k1_4;

	public readonly Pixel[] arr_pixels_k2_0;

	public readonly Pixel[] arr_pixels_k2_1;

	public readonly Pixel[] arr_pixels_k2_2;

	public readonly Pixel[] arr_pixels_k2_3;

	public readonly Pixel[] arr_pixels_k2_4;

	public readonly Pixel[] arr_pixels_phenotype_shade_0;

	public readonly Pixel[] arr_pixels_phenotype_shade_1;

	public readonly Pixel[] arr_pixels_phenotype_shade_2;

	public readonly Pixel[] arr_pixels_phenotype_shade_3;

	private ListPool<Pixel> _pixels_normal;

	private ListPool<Pixel> _pixels_light;

	private ListPool<Pixel> _pixels_k1_0;

	private ListPool<Pixel> _pixels_k1_1;

	private ListPool<Pixel> _pixels_k1_2;

	private ListPool<Pixel> _pixels_k1_3;

	private ListPool<Pixel> _pixels_k1_4;

	private ListPool<Pixel> _pixels_k2_0;

	private ListPool<Pixel> _pixels_k2_1;

	private ListPool<Pixel> _pixels_k2_2;

	private ListPool<Pixel> _pixels_k2_3;

	private ListPool<Pixel> _pixels_k2_4;

	private ListPool<Pixel> _pixels_phenotype_shade_0;

	private ListPool<Pixel> _pixels_phenotype_shade_1;

	private ListPool<Pixel> _pixels_phenotype_shade_2;

	private ListPool<Pixel> _pixels_phenotype_shade_3;

	public PixelBag(Sprite pSpriteSource, bool pCheckPhenotypes, bool pCheckLights)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		Texture2D texture = pSpriteSource.texture;
		Rect rect = pSpriteSource.rect;
		int width = ((Texture)texture).width;
		texture_rect_width = (int)((Rect)(ref rect)).width;
		texture_rect_height = (int)((Rect)(ref rect)).height;
		int num = (int)((Rect)(ref rect)).x;
		int num2 = (int)((Rect)(ref rect)).y;
		Color32[] pixels = texture.GetPixels32();
		for (int i = 0; i < texture_rect_width; i++)
		{
			for (int j = 0; j < texture_rect_height; j++)
			{
				int num3 = i + num;
				int num4 = j + num2;
				int num5 = num3 + num4 * width;
				Color32 val = pixels[num5];
				if (val.a != 0)
				{
					checkAndSavePixel(val, i, j, pCheckPhenotypes, pCheckLights);
				}
			}
		}
		arr_pixels_normal = _pixels_normal?.ToArray();
		arr_pixels_light = _pixels_light?.ToArray();
		arr_pixels_k1_0 = _pixels_k1_0?.ToArray();
		arr_pixels_k1_1 = _pixels_k1_1?.ToArray();
		arr_pixels_k1_2 = _pixels_k1_2?.ToArray();
		arr_pixels_k1_3 = _pixels_k1_3?.ToArray();
		arr_pixels_k1_4 = _pixels_k1_4?.ToArray();
		arr_pixels_k2_0 = _pixels_k2_0?.ToArray();
		arr_pixels_k2_1 = _pixels_k2_1?.ToArray();
		arr_pixels_k2_2 = _pixels_k2_2?.ToArray();
		arr_pixels_k2_3 = _pixels_k2_3?.ToArray();
		arr_pixels_k2_4 = _pixels_k2_4?.ToArray();
		arr_pixels_phenotype_shade_0 = _pixels_phenotype_shade_0?.ToArray();
		arr_pixels_phenotype_shade_1 = _pixels_phenotype_shade_1?.ToArray();
		arr_pixels_phenotype_shade_2 = _pixels_phenotype_shade_2?.ToArray();
		arr_pixels_phenotype_shade_3 = _pixels_phenotype_shade_3?.ToArray();
		clearLists();
	}

	private void clearLists()
	{
		_pixels_normal?.Dispose();
		_pixels_light?.Dispose();
		_pixels_k1_0?.Dispose();
		_pixels_k1_1?.Dispose();
		_pixels_k1_2?.Dispose();
		_pixels_k1_3?.Dispose();
		_pixels_k1_4?.Dispose();
		_pixels_k2_0?.Dispose();
		_pixels_k2_1?.Dispose();
		_pixels_k2_2?.Dispose();
		_pixels_k2_3?.Dispose();
		_pixels_k2_4?.Dispose();
		_pixels_phenotype_shade_0?.Dispose();
		_pixels_phenotype_shade_1?.Dispose();
		_pixels_phenotype_shade_2?.Dispose();
		_pixels_phenotype_shade_3?.Dispose();
		_pixels_normal = null;
		_pixels_light = null;
		_pixels_k1_0 = null;
		_pixels_k1_1 = null;
		_pixels_k1_2 = null;
		_pixels_k1_3 = null;
		_pixels_k1_4 = null;
		_pixels_k2_0 = null;
		_pixels_k2_1 = null;
		_pixels_k2_2 = null;
		_pixels_k2_3 = null;
		_pixels_k2_4 = null;
		_pixels_phenotype_shade_0 = null;
		_pixels_phenotype_shade_1 = null;
		_pixels_phenotype_shade_2 = null;
		_pixels_phenotype_shade_3 = null;
	}

	private void checkAndSavePixel(Color32 pColor, int pX, int pY, bool pCheckPhenotypes, bool pCheckLights)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		Pixel item = new Pixel(pX, pY, pColor);
		if (Toolbox.areColorsEqual(pColor, Toolbox.color_magenta_0))
		{
			if (_pixels_k1_0 == null)
			{
				_pixels_k1_0 = new ListPool<Pixel>();
			}
			_pixels_k1_0.Add(item);
			return;
		}
		if (Toolbox.areColorsEqual(pColor, Toolbox.color_magenta_1))
		{
			if (_pixels_k1_1 == null)
			{
				_pixels_k1_1 = new ListPool<Pixel>();
			}
			_pixels_k1_1.Add(item);
			return;
		}
		if (Toolbox.areColorsEqual(pColor, Toolbox.color_magenta_2))
		{
			if (_pixels_k1_2 == null)
			{
				_pixels_k1_2 = new ListPool<Pixel>();
			}
			_pixels_k1_2.Add(item);
			return;
		}
		if (Toolbox.areColorsEqual(pColor, Toolbox.color_magenta_3))
		{
			if (_pixels_k1_3 == null)
			{
				_pixels_k1_3 = new ListPool<Pixel>();
			}
			_pixels_k1_3.Add(item);
			return;
		}
		if (Toolbox.areColorsEqual(pColor, Toolbox.color_magenta_4))
		{
			if (_pixels_k1_4 == null)
			{
				_pixels_k1_4 = new ListPool<Pixel>();
			}
			_pixels_k1_4.Add(item);
			return;
		}
		if (Toolbox.areColorsEqual(pColor, Toolbox.color_teal_0))
		{
			if (_pixels_k2_0 == null)
			{
				_pixels_k2_0 = new ListPool<Pixel>();
			}
			_pixels_k2_0.Add(item);
			return;
		}
		if (Toolbox.areColorsEqual(pColor, Toolbox.color_teal_1))
		{
			if (_pixels_k2_1 == null)
			{
				_pixels_k2_1 = new ListPool<Pixel>();
			}
			_pixels_k2_1.Add(item);
			return;
		}
		if (Toolbox.areColorsEqual(pColor, Toolbox.color_teal_2))
		{
			if (_pixels_k2_2 == null)
			{
				_pixels_k2_2 = new ListPool<Pixel>();
			}
			_pixels_k2_2.Add(item);
			return;
		}
		if (Toolbox.areColorsEqual(pColor, Toolbox.color_teal_3))
		{
			if (_pixels_k2_3 == null)
			{
				_pixels_k2_3 = new ListPool<Pixel>();
			}
			_pixels_k2_3.Add(item);
			return;
		}
		if (Toolbox.areColorsEqual(pColor, Toolbox.color_teal_4))
		{
			if (_pixels_k2_4 == null)
			{
				_pixels_k2_4 = new ListPool<Pixel>();
			}
			_pixels_k2_4.Add(item);
			return;
		}
		if (pCheckPhenotypes)
		{
			if (Toolbox.areColorsEqual(pColor, Toolbox.color_phenotype_green_0))
			{
				if (_pixels_phenotype_shade_0 == null)
				{
					_pixels_phenotype_shade_0 = new ListPool<Pixel>();
				}
				_pixels_phenotype_shade_0.Add(item);
				return;
			}
			if (Toolbox.areColorsEqual(pColor, Toolbox.color_phenotype_green_1))
			{
				if (_pixels_phenotype_shade_1 == null)
				{
					_pixels_phenotype_shade_1 = new ListPool<Pixel>();
				}
				_pixels_phenotype_shade_1.Add(item);
				return;
			}
			if (Toolbox.areColorsEqual(pColor, Toolbox.color_phenotype_green_2))
			{
				if (_pixels_phenotype_shade_2 == null)
				{
					_pixels_phenotype_shade_2 = new ListPool<Pixel>();
				}
				_pixels_phenotype_shade_2.Add(item);
				return;
			}
			if (Toolbox.areColorsEqual(pColor, Toolbox.color_phenotype_green_3))
			{
				if (_pixels_phenotype_shade_3 == null)
				{
					_pixels_phenotype_shade_3 = new ListPool<Pixel>();
				}
				_pixels_phenotype_shade_3.Add(item);
				return;
			}
		}
		if (pCheckLights && Toolbox.areColorsEqual(pColor, Toolbox.color_light))
		{
			if (_pixels_light == null)
			{
				_pixels_light = new ListPool<Pixel>();
			}
			_pixels_light.Add(item);
		}
		else
		{
			if (_pixels_normal == null)
			{
				_pixels_normal = new ListPool<Pixel>();
			}
			_pixels_normal.Add(item);
		}
	}
}
