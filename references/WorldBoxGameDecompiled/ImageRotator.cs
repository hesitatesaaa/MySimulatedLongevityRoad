using UnityEngine;

public class ImageRotator : MonoBehaviour
{
	public float rotation_speed = 70f;

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.Rotate(Vector3.forward * rotation_speed * Time.deltaTime, (Space)1);
	}
}
