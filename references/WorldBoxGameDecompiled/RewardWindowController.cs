using UnityEngine;
using UnityEngine.UI;

public class RewardWindowController : MonoBehaviour
{
	public GameObject watchVideoButton;

	public GameObject waitTimeElement;

	public Text textElement;

	private void Update()
	{
		double nextAdTimestamp = PlayerConfig.instance.data.nextAdTimestamp;
		double num = Epoch.Current();
		nextAdTimestamp -= num;
		if (Config.isEditor && Config.editor_test_rewards_from_ads)
		{
			PlayerConfig.instance.data.nextAdTimestamp = -1.0;
			nextAdTimestamp = 0.0;
		}
		if (nextAdTimestamp > 0.0)
		{
			watchVideoButton.SetActive(false);
			waitTimeElement.SetActive(true);
			textElement.text = Toolbox.formatTimer((float)nextAdTimestamp);
		}
		else
		{
			watchVideoButton.SetActive(true);
			waitTimeElement.SetActive(false);
		}
	}
}
