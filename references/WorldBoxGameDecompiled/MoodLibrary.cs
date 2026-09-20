public class MoodLibrary : AssetLibrary<MoodAsset>
{
	public override void init()
	{
		base.init();
		add(new MoodAsset
		{
			id = "sad",
			icon = "iconMoodSad"
		});
		t.base_stats["multiplier_speed"] = -0.2f;
		t.base_stats["multiplier_diplomacy"] = -0.1f;
		t.base_stats["loyalty_mood"] = -5f;
		t.base_stats["opinion"] = -5f;
		add(new MoodAsset
		{
			id = "normal",
			icon = "iconMoodNormal"
		});
		add(new MoodAsset
		{
			id = "happy",
			icon = "iconMoodHappy"
		});
		t.base_stats["multiplier_speed"] = 0.1f;
		t.base_stats["multiplier_diplomacy"] = 0.1f;
		t.base_stats["loyalty_mood"] = 10f;
		t.base_stats["opinion"] = 10f;
		add(new MoodAsset
		{
			id = "angry",
			icon = "iconMoodAngry"
		});
		t.base_stats["multiplier_speed"] = 0.1f;
		t.base_stats["multiplier_diplomacy"] = -0.3f;
		t.base_stats["loyalty_mood"] = -15f;
		t.base_stats["opinion"] = -15f;
		add(new MoodAsset
		{
			id = "dark"
		});
		t.base_stats["loyalty_mood"] = -20f;
		t.base_stats["opinion"] = -20f;
	}
}
