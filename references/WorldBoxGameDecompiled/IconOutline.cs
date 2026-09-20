using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconOutline : MonoBehaviour
{
	private static Dictionary<string, Sprite> _cached_textures = new Dictionary<string, Sprite>();

	private Image _image;

	public Image parent_image;

	private void Awake()
	{
		checkInit();
	}

	private void checkInit()
	{
		if (!((Object)(object)_image != (Object)null))
		{
			_image = ((Component)this).GetComponent<Image>();
			((Component)this).gameObject.AddComponent<FadeInOutAnimation>();
		}
	}

	public unsafe void show(ContainerItemColor pContainer)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		checkInit();
		((Component)this).gameObject.SetActive(true);
		Color color = pContainer.color;
		color.a = 1f;
		((Graphic)_image).color = color;
		string key = ((object)parent_image.sprite.texture).GetHashCode() + "_" + ((object)(*(Color*)(&color))/*cast due to constrained. prefix*/).GetHashCode();
		Sprite val = null;
		if (_cached_textures.ContainsKey(key))
		{
			val = _cached_textures[key];
		}
		else
		{
			val = generateSprite();
			_cached_textures.Add(key, val);
		}
		_image.sprite = val;
	}

	private Sprite generateSprite()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		int width = ((Texture)parent_image.sprite.texture).width;
		int height = ((Texture)parent_image.sprite.texture).height;
		Texture2D val = new Texture2D(width, height);
		Color val2 = default(Color);
		((Color)(ref val2))._002Ector(1f, 1f, 1f, 0f);
		for (int i = 0; i < ((Texture)val).width; i++)
		{
			for (int j = 0; j < ((Texture)val).height; j++)
			{
				val.SetPixel(i, j, val2);
			}
		}
		makePixels(-1, -1, val);
		makePixels(1, 1, val);
		makePixels(1, -1, val);
		makePixels(-1, 1, val);
		makePixels(1, 0, val);
		makePixels(-1, 0, val);
		makePixels(0, 1, val);
		makePixels(0, -1, val);
		val.Apply();
		((Texture)val).filterMode = (FilterMode)0;
		((Object)val).name = "IconOutline";
		Rect val3 = default(Rect);
		((Rect)(ref val3))._002Ector(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height);
		Vector2 val4 = default(Vector2);
		((Vector2)(ref val4))._002Ector(0.5f, 0.5f);
		return Sprite.Create(val, val3, val4, 1f);
	}

	private void makePixels(int pOffsetX, int pOffsetY, Texture2D pTexture)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ((Texture)pTexture).width; i++)
		{
			for (int j = 0; j < ((Texture)pTexture).height; j++)
			{
				if (parent_image.sprite.texture.GetPixel(i, j).a != 0f)
				{
					int num = i + pOffsetX;
					int num2 = j + pOffsetY;
					if (num >= 0 && num <= ((Texture)pTexture).width && num2 >= 0 && num2 <= ((Texture)pTexture).height)
					{
						Color pixel = pTexture.GetPixel(num, num2);
						pixel.a += 0.3f;
						pTexture.SetPixel(num, num2, pixel);
					}
				}
			}
		}
	}
}
