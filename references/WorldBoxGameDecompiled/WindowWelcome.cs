using UnityEngine;
using UnityEngine.UI;

public class WindowWelcome : MonoBehaviour
{
	[SerializeField]
	private Transform _content;

	private LocalizedText top_text;

	private LocalizedText text_tip;

	public void Awake()
	{
		if (Config.isMobile)
		{
			text_tip = ((Component)((Component)this).transform.FindRecursive("Text Mobile")).GetComponent<LocalizedText>();
		}
		else
		{
			text_tip = ((Component)((Component)this).transform.FindRecursive("Text PC")).GetComponent<LocalizedText>();
		}
		top_text = ((Component)((Component)this).transform.FindRecursive("Text Main")).GetComponent<LocalizedText>();
	}

	public void nextTip()
	{
		text_tip.setKeyAndUpdate(LoadingScreen.getTipID());
	}

	private void Update()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (Config.MODDED || Config.experimental_mode)
		{
			top_text.setKeyAndUpdate("mods_warning");
			((Graphic)((Component)top_text).GetComponent<Text>()).color = Toolbox.color_red;
		}
		if (Time.frameCount % 30 == 0)
		{
			tldrCheck();
		}
	}

	private void tldrCheck()
	{
		if (((Component)_content).GetComponentsInChildren<UiCreature>().Length <= 0)
		{
			AchievementLibrary.tldr.check();
		}
	}
}
