using UnityEngine;

public class ButtonWebsite : MonoBehaviour
{
	public void openLink()
	{
		Analytics.LogEvent("open_link_website");
		Application.OpenURL("https://www.superworldbox.com/");
	}

	public void openLinkLSFLW2()
	{
		Analytics.LogEvent("open_link_lsflw2");
		Application.OpenURL("https://apps.apple.com/app/apple-store/id1397453494?pt=117120454&ct=worldbox&mt=8");
	}

	public void openPatchLog()
	{
		Analytics.LogEvent("open_link_changelog");
		Application.OpenURL("https://www.superworldbox.com/changelog");
	}
}
