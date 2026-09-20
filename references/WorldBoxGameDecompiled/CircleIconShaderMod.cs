using UnityEngine;

public class CircleIconShaderMod : MonoBehaviour
{
	public Material prefab_radial_fill;

	private Material _instance_material;

	public SpriteRenderer sprite_renderer_with_mat;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		_instance_material = new Material(prefab_radial_fill);
		((Renderer)sprite_renderer_with_mat).material = _instance_material;
	}

	public void setShaderVal(float pVal)
	{
		if (!((Object)(object)sprite_renderer_with_mat == (Object)null))
		{
			float num = Mathf.PingPong(pVal, 1f);
			_instance_material.SetFloat("_FillAmount", num);
		}
	}
}
