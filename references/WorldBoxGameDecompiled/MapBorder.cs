using System.Collections.Generic;
using UnityEngine;

public class MapBorder : BaseEffect
{
	private int currentState;

	private WorldTimer updateTimer;

	private WorldTimer alphaTimer;

	private int curWidth;

	private int curHeight;

	internal override void create()
	{
		base.create();
		updateTimer = new WorldTimer(0.12f, updateEffect);
		alphaTimer = new WorldTimer(0.02f, updateAlpha);
	}

	internal void generateTexture()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		if (curWidth == MapBox.width && curHeight == MapBox.height)
		{
			return;
		}
		curWidth = MapBox.width;
		curHeight = MapBox.height;
		SpriteRenderer component = ((Component)this).gameObject.GetComponent<SpriteRenderer>();
		Texture2D val = new Texture2D(curWidth, curHeight, (TextureFormat)4, false);
		((Texture)val).filterMode = (FilterMode)0;
		((Object)val).name = "MapBorder_" + curWidth + "x" + curHeight;
		int num = ((Texture)val).height * ((Texture)val).width;
		Color32[] array = (Color32[])(object)new Color32[num];
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		int num2 = 0;
		int num3 = 0;
		list2.Clear();
		num2 = 0;
		num3 = 0;
		for (int i = 0; i < num; i++)
		{
			if (num3 == 0 && !list.Contains(i))
			{
				list2.Add(i);
			}
			num2++;
			if (num2 >= curWidth)
			{
				num2 = 0;
				num3++;
			}
		}
		list.AddRange(list2);
		list2.Clear();
		num2 = 0;
		num3 = 0;
		for (int j = 0; j < num; j++)
		{
			if (num2 == curWidth - 1 && !list.Contains(j))
			{
				list2.Add(j);
			}
			num2++;
			if (num2 >= curWidth)
			{
				num2 = 0;
				num3++;
			}
		}
		list.AddRange(list2);
		list2.Clear();
		num2 = 0;
		num3 = 0;
		for (int k = 0; k < num; k++)
		{
			if (num3 == curHeight - 1 && !list.Contains(k))
			{
				list2.Add(k);
			}
			num2++;
			if (num2 >= curWidth)
			{
				num2 = 0;
				num3++;
			}
		}
		list.AddRange(list2);
		list2.Clear();
		num2 = 0;
		num3 = 0;
		for (int l = 0; l < num; l++)
		{
			if (num2 == 0 && !list.Contains(l))
			{
				list2.Add(l);
			}
			num2++;
			if (num2 >= curWidth)
			{
				num2 = 0;
				num3++;
			}
		}
		list2.Reverse();
		list.AddRange(list2);
		int num4 = 0;
		for (int m = 0; m < list.Count; m++)
		{
			int num5 = list[m];
			if (num4 == 0 || num4 == 1 || num4 == 2)
			{
				array[num5] = Color32.op_Implicit(Color.white);
				num4++;
			}
			else
			{
				num4 = 0;
			}
		}
		component.sprite = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f), 1f);
		val.SetPixels32(array);
		val.Apply();
		((Component)this).gameObject.transform.localPosition = new Vector3((float)(curWidth / 2), (float)(curHeight / 2));
	}

	private void Update()
	{
		updateTimer.update();
		alphaTimer.update();
	}

	private void updateAlpha()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)World.world.selected_buttons.selectedButton == (Object)null)
		{
			alpha -= 0.02f;
			if (alpha < 0f)
			{
				alpha = 0f;
			}
		}
		else
		{
			alpha += 0.02f;
			if (alpha > 0.42f)
			{
				alpha = 0.42f;
			}
		}
		if (sprite_renderer.color.a != alpha)
		{
			setAlpha(alpha);
		}
	}

	private void updateEffect()
	{
		if (alpha != 0f)
		{
			currentState++;
			if (currentState > 3)
			{
				currentState = 0;
			}
			switch (currentState)
			{
			case 0:
				sprite_renderer.flipX = false;
				sprite_renderer.flipY = false;
				break;
			case 1:
				sprite_renderer.flipX = true;
				sprite_renderer.flipY = false;
				break;
			case 2:
				sprite_renderer.flipX = true;
				sprite_renderer.flipY = true;
				break;
			case 3:
				sprite_renderer.flipX = false;
				sprite_renderer.flipY = true;
				break;
			}
		}
	}
}
