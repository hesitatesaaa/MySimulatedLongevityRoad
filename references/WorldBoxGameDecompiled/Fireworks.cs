using UnityEngine;

public class Fireworks : BaseEffect
{
	internal override void spawnOnTile(WorldTile pTile)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		float pScale = Randy.randomFloat(0.3f, 1f);
		prepare(pTile, pScale);
		if (Randy.randomBool())
		{
			loadSprites("effects/fireworks1");
		}
		else
		{
			loadSprites("effects/fireworks2");
		}
		sprite_renderer.flipX = Randy.randomBool();
		Color color = new Color
		{
			a = 1f,
			r = Randy.randomFloat(0f, 1f),
			b = Randy.randomFloat(0f, 1f),
			g = Randy.randomFloat(0f, 1f)
		};
		sprite_renderer.color = color;
		float num = Randy.randomFloat(-15f, 15f);
		((Component)this).transform.localEulerAngles = new Vector3(0f, 0f, num);
	}

	private void loadSprites(string pPath)
	{
		Sprite[] spriteList = SpriteTextureLoader.getSpriteList(pPath);
		sprite_animation.frames = spriteList;
	}
}
