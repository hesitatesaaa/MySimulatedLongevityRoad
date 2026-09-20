using UnityEngine;
using UnityEngine.UI;

public class ButtonSocial : MonoBehaviour
{
	[SerializeField]
	private SocialType _social_type;

	[SerializeField]
	private Text _text;

	private void Awake()
	{
		switch (_social_type)
		{
		case SocialType.Discord:
			_text.text = 560 + "k+";
			break;
		case SocialType.Facebook:
			_text.text = 82 + "k+";
			break;
		case SocialType.Twitter:
			_text.text = 56 + "k+";
			break;
		case SocialType.Reddit:
			_text.text = 140 + "k+";
			break;
		}
	}

	public void openFacebook()
	{
		Analytics.LogEvent("open_link_facebook");
		Application.OpenURL("https://www.facebook.com/superworldbox");
	}

	public void openTwitter()
	{
		Analytics.LogEvent("open_link_twitter");
		Application.OpenURL("http://twitter.com/mixamko");
	}

	public void openDiscord()
	{
		Analytics.LogEvent("open_link_discord");
		Application.OpenURL("https://discordapp.com/invite/worldbox");
		AchievementLibrary.social_network.check();
	}

	public void openLinkReddit()
	{
		Analytics.LogEvent("open_link_reddit");
		Application.OpenURL("https://www.reddit.com/r/worldbox");
	}

	public void openLinkMoonBox()
	{
		Analytics.LogEvent("open_link_moonbox");
		if (Config.isIos)
		{
			Application.OpenURL("https://bit.ly/moonbox_wb_ap");
		}
		else
		{
			Application.OpenURL("https://bit.ly/moonbox_wb");
		}
	}

	public void openLinkSteam()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Analytics.LogEvent("open_link_steam");
		Application.OpenURL(string.Concat(string.Concat($"https://store.steampowered.com/app/{1206560u}/" + "?utm_source=game_bar", "&utm_campaign=get_wishlists"), "&utm_medium=", ((object)Application.platform/*cast due to constrained. prefix*/).ToString()));
	}
}
