using UnityEngine;

public class UploadedMapDeleteProgressWindow : MonoBehaviour
{
	public GameObject deletingOverlay;

	private void OnEnable()
	{
		deletingOverlay.SetActive(false);
	}

	public void confirmDeletion()
	{
		deletingOverlay.SetActive(true);
	}
}
