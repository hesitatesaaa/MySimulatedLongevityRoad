using UnityEngine;

public class WindowBackground : MonoBehaviour
{
	private CanvasGroup group;

	private void Start()
	{
		group = ((Component)this).GetComponent<CanvasGroup>();
	}

	private void Update()
	{
		if (ScrollWindow.isWindowActive() && group.alpha < 1f)
		{
			CanvasGroup obj = group;
			obj.alpha += Time.deltaTime * 5f;
		}
		else if (!ScrollWindow.isWindowActive() && group.alpha > 0f)
		{
			CanvasGroup obj2 = group;
			obj2.alpha -= Time.deltaTime * 5f;
		}
	}
}
