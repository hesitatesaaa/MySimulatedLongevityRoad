using System;
using System.Collections.Generic;
using RSG;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

internal static class SteamAchievements
{
	private static Promise initialized;

	private static HashSet<string> achievements_hashset;

	public static void InitAchievements()
	{
		SteamSDK.steamInitialized.Then((Action)delegate
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			foreach (Achievement achievement in SteamUserStats.Achievements)
			{
				Achievement current = achievement;
				if (((Achievement)(ref current)).State)
				{
					unlockAchievement(((Achievement)(ref current)).Identifier);
					if (!AchievementLibrary.isUnlocked(((Achievement)(ref current)).Identifier))
					{
						Debug.Log((object)("Was unlocked in Steam already, unlocking in the game: " + ((Achievement)(ref current)).Identifier));
						AchievementLibrary.unlock(((Achievement)(ref current)).Identifier);
					}
				}
				if (!((Achievement)(ref current)).State && AchievementLibrary.isUnlocked(((Achievement)(ref current)).Identifier))
				{
					Debug.Log((object)("Was not unlocked in Steam yet, unlocking: " + ((Achievement)(ref current)).Identifier));
					TriggerAchievement(((Achievement)(ref current)).Identifier);
				}
			}
			initialized.Resolve();
		}).Catch((Action<Exception>)delegate(Exception err)
		{
			Debug.Log((object)"Error happened while getting Steam Achievement");
			Debug.Log((object)err);
			initialized.Reject(new Exception("Steam Achievements not available"));
		});
	}

	public static void TriggerAchievement(string id)
	{
		if (isSteamAchievementUnlocked(id))
		{
			return;
		}
		initialized.Then((Action)delegate
		{
			if (!isSteamAchievementUnlocked(id))
			{
				Debug.Log((object)("Unlocking in Steam: " + id));
				Achievement val = default(Achievement);
				((Achievement)(ref val))._002Ector(id);
				((Achievement)(ref val)).Trigger(true);
				unlockAchievement(id);
			}
		});
	}

	public static void unlockAchievement(string pName)
	{
		achievements_hashset.Add(pName);
	}

	public static bool isSteamAchievementUnlocked(string pName)
	{
		return achievements_hashset.Contains(pName);
	}

	static SteamAchievements()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		initialized = new Promise();
		achievements_hashset = new HashSet<string>();
	}
}
