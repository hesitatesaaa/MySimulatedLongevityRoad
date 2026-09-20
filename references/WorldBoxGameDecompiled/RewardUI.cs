using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
	public Image powerSprite;

	public Text text;

	public Text text_description;

	public Text window_title;

	public Text free_power_unlocked;

	public List<PowerButton> rewardPowers;

	public RewardAnimation rewardAnimation;

	internal void setRewardInfo(List<PowerButton> pButtons)
	{
		rewardPowers = pButtons;
		nextReward();
	}

	internal bool hasRewards()
	{
		if (rewardPowers != null)
		{
			return rewardPowers.Count > 0;
		}
		return false;
	}

	internal PowerButton popLowestReward()
	{
		int num = 10000;
		int index = 0;
		int num2 = 0;
		foreach (PowerButton rewardPower in rewardPowers)
		{
			if ((int)rewardPower.godPower.rank < num)
			{
				index = num2;
				num = (int)rewardPower.godPower.rank;
			}
			num2++;
		}
		PowerButton result = rewardPowers[index];
		rewardPowers.RemoveAt(index);
		return result;
	}

	internal void nextReward()
	{
		if (hasRewards())
		{
			PowerButton powerButton = popLowestReward();
			powerSprite.sprite = powerButton.icon.sprite;
			((Component)text).GetComponent<LocalizedText>().setKeyAndUpdate(powerButton.godPower.getLocaleID());
			((Component)text_description).gameObject.SetActive(true);
			((Component)text_description).GetComponent<LocalizedText>().setKeyAndUpdate(powerButton.godPower.getDescriptionID());
			if (powerButton.godPower.id == "clock")
			{
				((Component)window_title).GetComponent<LocalizedText>().key = "free_hourglass_title";
				((Component)free_power_unlocked).GetComponent<LocalizedText>().key = "free_hourglass_unlocked";
				rewardAnimation.quickReward = true;
			}
			else
			{
				((Component)window_title).GetComponent<LocalizedText>().key = "free_power";
				((Component)free_power_unlocked).GetComponent<LocalizedText>().key = "free_power_unlocked";
				rewardAnimation.quickReward = false;
			}
			PlayerConfig.instance.data.lastReward = powerButton.godPower.id;
			((Component)window_title).GetComponent<LocalizedText>().updateText();
			((Component)free_power_unlocked).GetComponent<LocalizedText>().updateText();
		}
	}

	public void bottomButtonClick()
	{
		if (rewardAnimation.state == RewardAnimationState.Open)
		{
			if (hasRewards())
			{
				rewardAnimation.resetAnim();
				nextReward();
			}
			else
			{
				((Component)this).GetComponent<ButtonEvent>().hideRewardWindowAndHighlightPower();
			}
		}
		else if (rewardAnimation.state == RewardAnimationState.Idle)
		{
			rewardAnimation.clickAnimation();
		}
	}

	internal void setRewardInfo(string pSpritePath, string pText)
	{
		powerSprite.sprite = SpriteTextureLoader.getSprite("ui/Icons/" + pSpritePath);
		((Component)text).GetComponent<LocalizedText>().key = pText;
		((Component)text).GetComponent<LocalizedText>().updateText();
		((Component)text_description).gameObject.SetActive(false);
		((Component)window_title).GetComponent<LocalizedText>().key = "free_saveslots_title";
		((Component)window_title).GetComponent<LocalizedText>().updateText();
		((Component)free_power_unlocked).GetComponent<LocalizedText>().key = "free_saveslots_unlocked";
		((Component)free_power_unlocked).GetComponent<LocalizedText>().updateText();
		rewardAnimation.quickReward = true;
	}
}
