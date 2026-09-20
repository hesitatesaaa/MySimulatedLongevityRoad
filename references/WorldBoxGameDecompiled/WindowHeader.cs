using UnityEngine;

public class WindowHeader : MonoBehaviour
{
	public void Awake()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 3f);
	}
}
