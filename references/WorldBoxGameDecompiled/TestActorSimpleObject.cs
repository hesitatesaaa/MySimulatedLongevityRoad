using System.Collections.Generic;
using UnityEngine;

public class TestActorSimpleObject
{
	public Sprite sprite;

	public float pos_x;

	public float pos_y;

	public float scale_x = 1f;

	public float scale_y = 1f;

	private List<Sprite> sprites = new List<Sprite>();

	public void create(List<Sprite> pSprites)
	{
		sprites = pSprites;
		randomRespawn();
		setRandomSprite();
	}

	public void randomRespawn()
	{
		WorldTile random = World.world.tiles_list.GetRandom();
		pos_x = random.x;
		pos_y = random.y;
	}

	public void update(float pElapsed)
	{
		randomMove(pElapsed);
	}

	private void randomMove(float pElapsed)
	{
		pos_x += Randy.randomFloat(-1f, 1f) * pElapsed * 6f;
		pos_y += Randy.randomFloat(-1f, 1f) * pElapsed * 6f;
	}

	private void setRandomSprite()
	{
		sprite = sprites.GetRandom();
	}
}
