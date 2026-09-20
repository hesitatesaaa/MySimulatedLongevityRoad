using UnityEngine;

public class LoadingScreenSheepAnimation : MonoBehaviour
{
	internal static float angle;

	private void Update()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		angle += Time.deltaTime * 20f;
		((Component)this).transform.localEulerAngles = new Vector3(0f, 0f, angle);
	}
}
