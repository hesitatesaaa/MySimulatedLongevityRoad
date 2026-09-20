using System;
using Beebyte.Obfuscator;

[Serializable]
[ObfuscateLiterals]
public class AchievementGroupLibrary : AssetLibrary<AchievementGroupAsset>
{
	public override void init()
	{
		base.init();
		add(new AchievementGroupAsset
		{
			id = "creation",
			color = "#68B3FF"
		});
		add(new AchievementGroupAsset
		{
			id = "worlds",
			color = "#BAFFC2"
		});
		add(new AchievementGroupAsset
		{
			id = "civilizations",
			color = "#BAF0F4"
		});
		add(new AchievementGroupAsset
		{
			id = "creatures",
			color = "#42FF61"
		});
		add(new AchievementGroupAsset
		{
			id = "destruction",
			color = "#FF6B86"
		});
		add(new AchievementGroupAsset
		{
			id = "nature",
			color = "#BAFFC2"
		});
		add(new AchievementGroupAsset
		{
			id = "experiments",
			color = "#FF8F44"
		});
		add(new AchievementGroupAsset
		{
			id = "collection",
			color = "#46DCE3"
		});
		add(new AchievementGroupAsset
		{
			id = "exploration",
			color = "#EFCB00"
		});
		add(new AchievementGroupAsset
		{
			id = "forbidden",
			color = "#C98CFF"
		});
		add(new AchievementGroupAsset
		{
			id = "miscellaneous",
			color = "#B4C4C0"
		});
	}

	public override void linkAssets()
	{
		foreach (Achievement item in AssetManager.achievements.list)
		{
			dict[item.group].achievements_list.Add(item);
		}
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (AchievementGroupAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
		}
	}
}
