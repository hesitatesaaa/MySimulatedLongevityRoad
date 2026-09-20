using UnityEngine;

public class ButtonVote : MonoBehaviour
{
	public void openLink()
	{
		Analytics.LogEvent("click_vote");
		if (Config.isAndroid)
		{
			Application.OpenURL("https://play.google.com/store/apps/details?id=com.mkarpenko.worldbox");
		}
		else if (Config.isIos)
		{
			Application.OpenURL("https://itunes.apple.com/app/id1450941371");
		}
	}
}
