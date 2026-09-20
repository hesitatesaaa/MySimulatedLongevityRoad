using UnityEngine;

public class UploadedMapReportWindow : MonoBehaviour
{
	public GameObject reportOverlay;

	public GameObject reportButtons;

	public GameObject reportConfirmation;

	private string reportReason = "";

	private void OnEnable()
	{
		reportOverlay.SetActive(false);
		reportButtons.SetActive(true);
		reportConfirmation.SetActive(false);
	}

	public void reportNSFW()
	{
		reportReason = "nsfw";
		confirmReport();
	}

	public void reportCrash()
	{
		reportReason = "crash";
		confirmReport();
	}

	public void reportBroken()
	{
		reportReason = "broken";
		confirmReport();
	}

	public void confirmReport()
	{
		reportButtons.SetActive(false);
		reportOverlay.SetActive(true);
		_ = reportReason;
	}
}
