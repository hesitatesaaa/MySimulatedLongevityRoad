using UnityEngine;

public class ButtonVersionUpdate : MonoBehaviour
{
	public void openLink()
	{
		Analytics.LogEvent("open_link_version");
		Application.OpenURL("https://www.superworldbox.com/");
	}
}
