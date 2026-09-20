using System;
using System.Collections.Generic;

[Serializable]
public class AchievementGroupAsset : BaseCategoryAsset, ILocalizedAsset
{
	[NonSerialized]
	public List<Achievement> achievements_list = new List<Achievement>();

	public override string getLocaleID()
	{
		return "achievement_group_" + id;
	}
}
