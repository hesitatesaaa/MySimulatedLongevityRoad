using System;
using UnityEngine;
using UnityEngine.Networking;

public class ButtonEmail : MonoBehaviour
{
	public void SendEmail()
	{
		string text = "supworldbox@gmail.com";
		string text2 = convert("WorldBox Feedback ( " + Application.version + " )");
		string text3 = convert("Yo!\r\n");
		Application.OpenURL("mailto:" + text + "?subject=" + text2 + "&body=" + text3);
		Analytics.LogEvent("clicked_send_email");
	}

	public void SendEmailLogs()
	{
		string text = "supworldbox+errors@gmail.com";
		string text2 = convert("WorldBox Error Logs ( " + Application.version + " )");
		string text3 = convert("Please take a look at this error :\r\n" + LogHandler.log.Substring(Math.Max(0, LogHandler.log.Length - 4000)));
		Application.OpenURL("mailto:" + text + "?subject=" + text2 + "&body=" + text3);
		Analytics.LogEvent("clicked_send_error_email");
	}

	private string convert(string url)
	{
		return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
	}
}
