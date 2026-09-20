using System;
using System.Runtime.CompilerServices;
using Discord;
using Proyecto26;
using UnityEngine;

public class DiscordTracker : MonoBehaviour, IRichTracker
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UpdateActivityHandler _003C_003E9__10_0;

		public static UpdateActivityHandler _003C_003E9__23_0;

		public static UpdateActivityHandler _003C_003E9__24_0;

		internal void _003CStart_003Eb__10_0(Result pRes)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			if ((int)pRes != 0)
			{
				Debug.Log((object)"Disabling Discord");
				Debug.Log((object)pRes);
				Object.Destroy((Object)(object)_instance);
			}
		}

		internal void _003CtrackActivity_003Eb__23_0(Result _)
		{
		}

		internal void _003CupdateDetails_003Eb__24_0(Result _)
		{
		}
	}

	private const long DISCORD_GAME_ID = 816251591299432468L;

	private const ulong DISCORD_FLAGS = 1uL;

	private static Discord _discord;

	private static ActivityManager _activity_manager;

	private bool _initiated;

	private static DiscordTracker _instance;

	private static Activity _activity;

	private static bool _have_user = false;

	private static int _user_tries = 10;

	private static float _timer = 10f;

	private void Start()
	{
		//IL_00fa: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		if (_initiated)
		{
			return;
		}
		_initiated = true;
		bool flag = false;
		try
		{
			_instance = this;
			_discord = new Discord(816251591299432468L, 1uL);
			_activity_manager = _discord.GetActivityManager();
			_activity = new Activity
			{
				State = LocalizedTextManager.getText("discord_browsing"),
				Assets = 
				{
					LargeImage = "worldboxlogo"
				},
				Assets = 
				{
					LargeText = "WorldBox"
				},
				Timestamps = 
				{
					Start = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds
				},
				Instance = true
			};
			ActivityManager activity_manager = _activity_manager;
			if (activity_manager != null)
			{
				Activity activity = _activity;
				object obj = _003C_003Ec._003C_003E9__10_0;
				if (obj == null)
				{
					UpdateActivityHandler val = delegate(Result pRes)
					{
						//IL_0000: Unknown result type (might be due to invalid IL or missing references)
						//IL_000d: Unknown result type (might be due to invalid IL or missing references)
						if ((int)pRes != 0)
						{
							Debug.Log((object)"Disabling Discord");
							Debug.Log((object)pRes);
							Object.Destroy((Object)(object)_instance);
						}
					};
					_003C_003Ec._003C_003E9__10_0 = val;
					obj = (object)val;
				}
				activity_manager.UpdateActivity(activity, (UpdateActivityHandler)obj);
			}
		}
		catch (ResultException ex)
		{
			Debug.Log((object)"Disabling Discord Integration (Discord not running, or game not run as Administrator)");
			Debug.Log((object)ex);
			flag = true;
		}
		catch (Exception ex2)
		{
			Debug.Log((object)"Disabling Discord Integration (Discord not running, or game not run as Administrator)");
			Debug.Log((object)ex2);
			flag = true;
		}
		if (flag)
		{
			Object.Destroy((Object)(object)_instance);
		}
	}

	private static void tryGetUser()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			_user_tries--;
			User currentUser = _discord.GetUserManager().GetCurrentUser();
			string text = currentUser.Id.ToString();
			if (!string.IsNullOrEmpty(text))
			{
				Config.discordId = text;
				RestClient.DefaultRequestHeaders["wb-dsc"] = text;
				_have_user = true;
				Debug.Log((object)("D:" + Config.discordId));
			}
			else
			{
				Debug.Log((object)"D:nf");
			}
			string username = currentUser.Username;
			if (!string.IsNullOrEmpty(username))
			{
				Config.discordName = username;
			}
			string discriminator = currentUser.Discriminator;
			if (!string.IsNullOrEmpty(discriminator))
			{
				Config.discordDiscriminator = discriminator;
			}
			VersionCheck.checkVersion();
		}
		catch (Exception)
		{
			Debug.Log((object)"D:F");
		}
	}

	private void Update()
	{
		if (!_initiated)
		{
			return;
		}
		try
		{
			_discord.RunCallbacks();
		}
		catch (Exception ex)
		{
			Debug.Log((object)"Disabling Discord");
			Debug.Log((object)ex);
			Object.Destroy((Object)(object)_instance);
			return;
		}
		if (_timer > 0f)
		{
			_timer -= Time.deltaTime;
			return;
		}
		_timer = 10f;
		try
		{
			if (!_have_user && _user_tries > 0)
			{
				tryGetUser();
			}
			updateDetails(PowerTracker.activeStat);
		}
		catch (Exception ex2)
		{
			Debug.Log((object)"Disabling Discord");
			Debug.Log((object)ex2);
			Object.Destroy((Object)(object)_instance);
		}
	}

	private void OnDisable()
	{
		Discord discord = _discord;
		if (discord != null)
		{
			discord.Dispose();
		}
	}

	private void OnDestroy()
	{
		_instance = null;
		_activity_manager = null;
		PowerTracker.discordTracker = null;
	}

	public void trackViewing(string pString)
	{
		if ((Object)(object)_instance == (Object)null)
		{
			return;
		}
		if (pString != "" && LocalizedTextManager.stringExists(pString))
		{
			pString = LocalizedTextManager.getText("discord_viewing").Replace("$window$", LocalizedTextManager.getText(pString));
		}
		else
		{
			if (pString != "")
			{
				Debug.Log((object)("Missing translation for " + pString));
			}
			pString = LocalizedTextManager.getText("discord_browsing");
		}
		trackActivity(pString);
	}

	public void trackWatching()
	{
		if (!((Object)(object)_instance == (Object)null))
		{
			trackActivity(LocalizedTextManager.getText("discord_watching"));
		}
	}

	public void trackUsing(string pPower)
	{
		if (!((Object)(object)_instance == (Object)null))
		{
			trackActivity(LocalizedTextManager.getText("discord_using").Replace("$power$", LocalizedTextManager.getText(pPower)));
		}
	}

	public void updateUsing(int pAmount, string pPower = "")
	{
		trackActivity(LocalizedTextManager.getText(pPower) + " (" + pAmount + ")");
	}

	public void inspectKingdom(string pKingdom)
	{
		trackActivity(LocalizedTextManager.getText("village_statistics_kingdom") + ": " + pKingdom);
	}

	public void inspectVillage(string pVillage)
	{
		trackActivity(LocalizedTextManager.getText("village") + ": " + pVillage);
	}

	public void inspectUnit(string pUnit)
	{
		trackActivity("inspect".Localize() + ": " + pUnit);
	}

	public void spectatingUnit(string pUnit)
	{
		trackActivity(LocalizedTextManager.getText("tip_following_unit").Replace("$name$", pUnit));
	}

	public void trackActivity(string pState = "")
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		if ((Object)(object)_instance == (Object)null)
		{
			return;
		}
		_activity.State = pState;
		ActivityManager activity_manager = _activity_manager;
		if (activity_manager != null)
		{
			Activity activity = _activity;
			object obj = _003C_003Ec._003C_003E9__23_0;
			if (obj == null)
			{
				UpdateActivityHandler val = delegate
				{
				};
				_003C_003Ec._003C_003E9__23_0 = val;
				obj = (object)val;
			}
			activity_manager.UpdateActivity(activity, (UpdateActivityHandler)obj);
		}
	}

	public void updateDetails(StatisticsAsset pStat)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		if ((Object)(object)_instance == (Object)null)
		{
			return;
		}
		string localeID = pStat.getLocaleID();
		if (!string.IsNullOrEmpty(localeID))
		{
			_activity.Details = LocalizedTextManager.getText(localeID) + ": " + pStat.last_value;
		}
		else
		{
			_activity.Details = pStat.last_value;
		}
		ActivityManager activity_manager = _activity_manager;
		if (activity_manager != null)
		{
			Activity activity = _activity;
			object obj = _003C_003Ec._003C_003E9__24_0;
			if (obj == null)
			{
				UpdateActivityHandler val = delegate
				{
				};
				_003C_003Ec._003C_003E9__24_0 = val;
				obj = (object)val;
			}
			activity_manager.UpdateActivity(activity, (UpdateActivityHandler)obj);
		}
	}
}
