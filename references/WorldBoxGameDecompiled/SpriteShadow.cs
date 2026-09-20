using UnityEngine;

public class SpriteShadow : MonoBehaviour
{
	public Vector2 offset;

	internal int z_height;

	private SpriteRenderer sprRndCaster;

	private SpriteRenderer sprRndShadow;

	private Transform transCaster;

	private Transform transShadow;

	public Color shadowColor;

	private BaseMapObject baseMapObject;

	private void Start()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		baseMapObject = ((Component)this).GetComponent<BaseMapObject>();
		transCaster = ((Component)this).transform;
		transShadow = new GameObject().transform;
		transShadow.parent = transCaster;
		((Object)((Component)transShadow).gameObject).name = "Shadow";
		transShadow.localRotation = Quaternion.identity;
		transShadow.localScale = new Vector3(1f, 0.5f);
		sprRndCaster = ((Component)this).GetComponent<SpriteRenderer>();
		sprRndShadow = ((Component)transShadow).gameObject.AddComponent<SpriteRenderer>();
		((Renderer)sprRndShadow).sharedMaterial = LibraryMaterials.instance.mat_world_object;
		sprRndShadow.color = shadowColor;
		((Renderer)sprRndShadow).sortingLayerName = ((Renderer)sprRndCaster).sortingLayerName;
		((Renderer)sprRndShadow).sortingOrder = ((Renderer)sprRndCaster).sortingOrder - 1;
	}

	private void LateUpdate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		transShadow.position = Vector2.op_Implicit(new Vector2(transCaster.position.x + offset.x, transCaster.position.y + offset.y));
		Color color = shadowColor;
		color.a = sprRndCaster.color.a * 0.5f;
		sprRndShadow.color = color;
		sprRndShadow.sprite = sprRndCaster.sprite;
		sprRndShadow.flipX = sprRndCaster.flipX;
	}

	public SpriteShadow()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		offset = new Vector2(-3f, 3f);
		((MonoBehaviour)this)._002Ector();
	}
}
