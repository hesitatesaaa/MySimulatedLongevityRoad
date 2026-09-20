using UnityEngine;

public class LoadingScreenOut : MonoBehaviour
{
	public CanvasGroup canvasGroup;

	private void Update()
	{
		CanvasGroup obj = canvasGroup;
		obj.alpha -= Time.deltaTime * 2f;
		if (canvasGroup.alpha <= 0f)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
