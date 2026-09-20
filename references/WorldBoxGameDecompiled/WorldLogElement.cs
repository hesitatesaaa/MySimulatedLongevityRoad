using UnityEngine;
using UnityEngine.UI;

public class WorldLogElement : MonoBehaviour
{
	public Text date;

	public Text description;

	public Image icon;

	public GameObject locate;

	public GameObject follow;

	public WorldLogMessage message;

	public void showMessage(WorldLogMessage pMessage)
	{
		message = pMessage;
		date.text = "y:" + Date.getYear(message.timestamp) + ", m:" + Date.getMonth(message.timestamp);
		string formatedText = message.getFormatedText(description);
		bool active = message.hasLocation();
		if (message.hasFollowLocation())
		{
			follow.SetActive(true);
			locate.SetActive(false);
		}
		else
		{
			follow.SetActive(false);
			locate.SetActive(active);
		}
		description.text = formatedText ?? "";
		((Component)description).GetComponent<LocalizedText>().checkTextFont();
		string path_icon = message.getAsset().path_icon;
		if (!string.IsNullOrEmpty(path_icon))
		{
			Sprite sprite = SpriteTextureLoader.getSprite(path_icon);
			icon.sprite = sprite;
		}
		else
		{
			((Component)icon).gameObject.SetActive(false);
		}
		((Component)description).GetComponent<LocalizedText>().checkSpecialLanguages();
	}

	public void clickLocate()
	{
		message.jumpToLocation();
	}
}
