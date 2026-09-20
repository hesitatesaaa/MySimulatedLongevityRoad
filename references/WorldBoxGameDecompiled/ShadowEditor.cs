using UnityEngine;

public class ShadowEditor : MonoBehaviour
{
	public static ShadowEditor instance;

	public bool isEnabled;

	public Vector2 shadow_bound;

	public float shadow_distortion;

	public ShadowEditor()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		shadow_bound = new Vector2(0.5f, 0.14f);
		shadow_distortion = 0.08f;
		((MonoBehaviour)this)._002Ector();
	}
}
