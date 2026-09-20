using System.Collections.Generic;
using UnityEngine;

public class AchievementWindow : MonoBehaviour
{
	public AchievementGroup achievementGroupPrefab;

	private List<AchievementGroup> _elements = new List<AchievementGroup>();

	public Transform transformContent;

	public StatBar achievementBar;

	private void OnEnable()
	{
		showList();
	}

	internal void showList()
	{
		if (!Config.game_loaded)
		{
			return;
		}
		for (int i = 0; i < _elements.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)_elements[i]).gameObject);
		}
		_elements.Clear();
		foreach (AchievementGroupAsset item in AssetManager.achievement_groups.list)
		{
			showElement(item);
		}
		updateTotalBar();
	}

	private void updateTotalBar()
	{
		int count = AssetManager.achievements.list.Count;
		int num = AchievementLibrary.countUnlocked();
		achievementBar.setBar(num, count, "/" + count.ToText());
	}

	private void showElement(AchievementGroupAsset pAchievementGroup)
	{
		AchievementGroup achievementGroup = Object.Instantiate<AchievementGroup>(achievementGroupPrefab, transformContent);
		achievementGroup.showGroup(pAchievementGroup);
		_elements.Add(achievementGroup);
	}
}
