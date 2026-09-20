using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementGroup : MonoBehaviour
{
	public AchievementButton achievementButtonPrefab;

	private List<AchievementButton> _elements = new List<AchievementButton>();

	public Text title;

	public Text counter;

	public Transform transformContent;

	public void showGroup(AchievementGroupAsset pAchievementGroup)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		((Component)title).GetComponent<LocalizedText>().setKeyAndUpdate(pAchievementGroup.getLocaleID());
		((Graphic)title).color = pAchievementGroup.getColor();
		if (pAchievementGroup.achievements_list.Count <= 0)
		{
			return;
		}
		int num = 0;
		foreach (Achievement item in pAchievementGroup.achievements_list)
		{
			AchievementButton achievementButton = Object.Instantiate<AchievementButton>(achievementButtonPrefab, transformContent);
			achievementButton.Load(item);
			if (AchievementLibrary.isUnlocked(item))
			{
				num++;
			}
			_elements.Add(achievementButton);
		}
		counter.text = num + " / " + pAchievementGroup.achievements_list.Count;
	}
}
