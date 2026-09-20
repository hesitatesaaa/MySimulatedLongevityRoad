using DG.Tweening;
using UnityEngine;

public class SantaAnimation : BaseMapObject
{
	public float shakeX = 2f;

	public float shakeY = 0.3f;

	private Tween shakeTween;

	private Vector3 tStr;

	private Santa santa;

	private SpriteRenderer spriteRenderer;

	internal override void create()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		base.create();
		tStr = new Vector3(shakeX, shakeY);
		shakeTween = (Tween)(object)ShortcutExtensions.DOShakePosition(((Component)this).transform, 0.5f, tStr, 10, 90f, false, false, (ShakeRandomnessMode)0);
		santa = ((Component)((Component)this).transform.parent).GetComponent<Santa>();
		spriteRenderer = ((Component)this).GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (santa.alive)
		{
			((Renderer)spriteRenderer).sharedMaterial = santa.current_material;
		}
		else
		{
			((Renderer)spriteRenderer).sharedMaterial = LibraryMaterials.instance.mat_world_object;
		}
		if (!World.world.isPaused() && !shakeTween.active)
		{
			shakeTween = (Tween)(object)ShortcutExtensions.DOShakePosition(((Component)this).transform, 0.5f, tStr, 10, 90f, false, false, (ShakeRandomnessMode)0);
		}
	}
}
