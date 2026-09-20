using System.Collections.Generic;
using UnityEngine;

public class MapLayer : BaseMapObject
{
	public bool autoDisable;

	public bool autoDisableCheckPixels;

	public int textureID;

	protected float timer;

	protected Color colorValues;

	protected int colors_amount = 1;

	internal SpriteRenderer sprRnd;

	internal Texture2D texture;

	internal Color32[] pixels;

	internal HashSetWorldTile pixels_to_update;

	protected List<Color32> colors;

	internal HashSetWorldTile hashsetTiles;

	private int textureWidth;

	private int textureHeight;

	public bool rewriteSortingLayer = true;

	internal override void create()
	{
		base.create();
		pixels_to_update = new HashSetWorldTile();
		sprRnd = ((Component)this).gameObject.GetComponent<SpriteRenderer>();
		if (rewriteSortingLayer)
		{
			((Renderer)sprRnd).sortingLayerName = ((Renderer)((Component)World.world).GetComponent<SpriteRenderer>()).sortingLayerName;
		}
		colors = new List<Color32>();
		createColors();
	}

	protected virtual void checkAutoDisable()
	{
		if (!autoDisable)
		{
			return;
		}
		if (autoDisableCheckPixels)
		{
			if (pixels_to_update.Count > 0)
			{
				if (!((Renderer)sprRnd).enabled)
				{
					((Renderer)sprRnd).enabled = true;
				}
			}
			else if (((Renderer)sprRnd).enabled)
			{
				((Renderer)sprRnd).enabled = false;
			}
		}
		else if (hashsetTiles.Count > 0)
		{
			if (!((Renderer)sprRnd).enabled)
			{
				((Renderer)sprRnd).enabled = true;
			}
		}
		else if (((Renderer)sprRnd).enabled)
		{
			((Renderer)sprRnd).enabled = false;
		}
	}

	internal void createTextureNew()
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)texture == (Object)null || MapBox.width != textureWidth || MapBox.height != ((Texture)texture).height)
		{
			if ((Object)(object)sprRnd.sprite != (Object)null && textureWidth != 0)
			{
				Texture2DStorage.addToStorage(sprRnd.sprite, textureWidth, textureHeight);
			}
			textureWidth = MapBox.width;
			textureHeight = MapBox.height;
			sprRnd.sprite = Texture2DStorage.getSprite(textureWidth, textureHeight);
			texture = sprRnd.sprite.texture;
			textureID = ((object)texture).GetHashCode();
			int num = ((Texture)texture).height * ((Texture)texture).width;
			Color32 val = Color32.op_Implicit(Color.clear);
			pixels = (Color32[])(object)new Color32[num];
			for (int i = 0; i < num; i++)
			{
				pixels[i] = val;
			}
			updatePixels();
		}
	}

	public bool contains(WorldTile pTile)
	{
		return pixels_to_update.Contains(pTile);
	}

	internal virtual void clear()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (pixels != null)
		{
			pixels_to_update.Clear();
			Color32 val = Color32.op_Implicit(Color.clear);
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = val;
			}
			updatePixels();
		}
	}

	public void setRendererEnabled(bool pBool)
	{
		((Renderer)sprRnd).enabled = pBool;
	}

	protected void createColors()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < colors_amount; i++)
		{
			float num = ((i <= 0) ? 0f : (1f / (float)colors_amount * (float)i));
			colors.Add(Color32.op_Implicit(new Color(colorValues.r, colorValues.g, colorValues.b, num * colorValues.a)));
		}
	}

	public override void update(float pElapsed)
	{
		checkAutoDisable();
	}

	public virtual void draw(float pElapsed)
	{
		if (((Renderer)sprRnd).enabled)
		{
			UpdateDirty(pElapsed);
		}
	}

	internal void updatePixels()
	{
		texture.SetPixels32(pixels);
		texture.Apply();
	}

	protected virtual void UpdateDirty(float pElapsed)
	{
	}
}
