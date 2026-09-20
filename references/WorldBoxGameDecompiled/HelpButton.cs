using UnityEngine;

public class HelpButton : MonoBehaviour
{
	public void clickHelp()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		string stringVal = PlayerConfig.dict["language"].stringVal;
		Analytics.LogEvent("open_help");
		string text = "";
		text = (((int)Application.platform != 11) ? ("https://support.apple.com/" + stringVal + "-" + stringVal + "/HT203005") : ("https://support.google.com/googleplay/answer/1050566?hl=" + stringVal));
		Application.OpenURL(text);
	}
}
