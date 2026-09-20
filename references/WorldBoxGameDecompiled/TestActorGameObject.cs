using System.Collections.Generic;
using UnityEngine;

public class TestActorGameObject : MonoBehaviour
{
	public Sprite sprite;

	public float pos_x;

	public float pos_y;

	public float scale_x = 1f;

	public float scale_y = 1f;

	private List<Sprite> sprites = new List<Sprite>();

	private SpriteRenderer spriteRenderer;

	public void create(List<Sprite> pSprites)
	{
		spriteRenderer = ((Component)this).GetComponent<SpriteRenderer>();
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
		applyUnity();
	}

	private void applyUnity()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		spriteRenderer.sprite = sprite;
		((Component)this).transform.position = new Vector3(pos_x, pos_y, 0f);
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
