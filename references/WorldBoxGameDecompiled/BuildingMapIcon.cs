using UnityEngine;

public class BuildingMapIcon
{
	private BuildingColorPixel[][] _tex;

	private BuildingColorPixel _clear_color_pixel;

	private int _width;

	private int _height;

	public BuildingMapIcon(Sprite sprite)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		_clear_color_pixel = new BuildingColorPixel(Toolbox.clear, Toolbox.clear, Toolbox.clear);
		base._002Ector();
		_width = ((Texture)sprite.texture).width;
		_height = ((Texture)sprite.texture).height;
		_tex = new BuildingColorPixel[_height][];
		for (int i = 0; i < _height; i++)
		{
			BuildingColorPixel[] array = new BuildingColorPixel[_width];
			for (int j = 0; j < _width; j++)
			{
				Color32 val = Color32.op_Implicit(sprite.texture.GetPixel(j, i));
				if (val.a == 0)
				{
					array[j] = _clear_color_pixel;
					continue;
				}
				Color val2 = Toolbox.makeDarkerColor(Color32.op_Implicit(val), 0.9f);
				Color val3 = Toolbox.makeDarkerColor(Color32.op_Implicit(val), 0.6f);
				array[j] = new BuildingColorPixel(val, Color32.op_Implicit(val2), Color32.op_Implicit(val3));
			}
			_tex[i] = array;
		}
	}

	internal Color32 getColor(int pX, int pY, Building pBuilding)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		if (pX >= _width || pY >= _height)
		{
			return Toolbox.clear;
		}
		BuildingColorPixel buildingColorPixel = _tex[pY][pX];
		Color32 val = buildingColorPixel.color;
		bool flag = false;
		ColorAsset color = pBuilding.kingdom.getColor();
		if (color != null)
		{
			if (Toolbox.areColorsEqual(val, Toolbox.color_magenta_0))
			{
				val = color.k_color_0;
				flag = true;
			}
			else if (Toolbox.areColorsEqual(val, Toolbox.color_magenta_1))
			{
				val = color.k_color_1;
				flag = true;
			}
			else if (Toolbox.areColorsEqual(val, Toolbox.color_magenta_2))
			{
				val = color.k_color_2;
				flag = true;
			}
			else if (Toolbox.areColorsEqual(val, Toolbox.color_magenta_3))
			{
				val = color.k_color_3;
				flag = true;
			}
			else if (Toolbox.areColorsEqual(val, Toolbox.color_magenta_4))
			{
				val = color.k_color_4;
				flag = true;
			}
		}
		if (pBuilding.asset.has_get_map_icon_color && Toolbox.areColorsEqual(val, Toolbox.color_map_icon_green))
		{
			val = pBuilding.asset.get_map_icon_color(pBuilding);
			flag = true;
		}
		if (!flag)
		{
			if (pBuilding.isAbandoned())
			{
				val = buildingColorPixel.color_abandoned;
			}
			else if (pBuilding.isRuin())
			{
				val = buildingColorPixel.color_ruin;
			}
		}
		return val;
	}
}
