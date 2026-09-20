using UnityEngine;

public class ButtonTranslate : MonoBehaviour
{
	public void openLink()
	{
		Analytics.LogEvent("click_translate");
		Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSeL8sirqSFbHa_dHipgu-2QiRSNHqEn2l7ApodM8qD5xm010A/viewform");
	}
}
