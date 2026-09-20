using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
	public Sprite pause;

	public Sprite play;

	private Image icon;

	private void Start()
	{
		icon = ((Component)((Component)this).transform.Find("Icon")).GetComponent<Image>();
	}

	private void Update()
	{
		updateSprite();
	}

	internal void press()
	{
		Config.paused = !Config.paused;
		if (Config.paused)
		{
			WorldTip.instance.setText("game_paused");
		}
		else
		{
			WorldTip.instance.setText("game_unpaused");
		}
	}

	private void updateSprite()
	{
		if (Config.paused)
		{
			icon.sprite = play;
		}
		else
		{
			icon.sprite = pause;
		}
	}
}
