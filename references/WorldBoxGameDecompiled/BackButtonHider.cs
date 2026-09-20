using UnityEngine;

internal class BackButtonHider : MonoBehaviour
{
	private void OnEnable()
	{
		if (WindowHistory.hasHistory())
		{
			((Component)this).gameObject.SetActive(true);
		}
		else
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
