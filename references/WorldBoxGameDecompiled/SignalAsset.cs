using System;

[Serializable]
public class SignalAsset : Asset
{
	public SignalAction action;

	public bool has_action;

	public AchievementCheck action_achievement;

	public bool has_action_achievement;

	public SignalBanCheckAction ban_check_action;

	public bool has_ban_check_action;

	private bool _banned;

	public bool isBanned()
	{
		return _banned;
	}

	public bool ban()
	{
		return _banned = true;
	}

	public bool unban()
	{
		return _banned = false;
	}
}
