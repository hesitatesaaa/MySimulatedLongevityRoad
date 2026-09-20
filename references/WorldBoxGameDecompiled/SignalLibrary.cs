public class SignalLibrary : AssetLibrary<SignalAsset>
{
	public override void post_init()
	{
		base.post_init();
		foreach (Achievement tAchievement in AssetManager.achievements.list)
		{
			if (!tAchievement.has_signal)
			{
				SignalAsset signal = add(new SignalAsset
				{
					id = tAchievement.id + "_signal",
					action_achievement = tAchievement.check,
					ban_check_action = (object _) => tAchievement.isUnlocked()
				});
				tAchievement.setSignal(signal);
			}
		}
		foreach (SignalAsset item in list)
		{
			if (item.action != null)
			{
				item.has_action = true;
			}
			if (item.action_achievement != null)
			{
				item.has_action_achievement = true;
			}
			if (item.ban_check_action != null)
			{
				item.has_ban_check_action = true;
				if (item.ban_check_action())
				{
					item.ban();
				}
			}
		}
	}
}
