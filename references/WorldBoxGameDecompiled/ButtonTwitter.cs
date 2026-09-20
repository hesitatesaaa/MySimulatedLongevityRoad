using UnityEngine;

public class ButtonTwitter : MonoBehaviour
{
	public void openLink()
	{
		Analytics.LogEvent("open_link_twitter");
		Application.OpenURL("http://twitter.com/mixamko");
	}
}
