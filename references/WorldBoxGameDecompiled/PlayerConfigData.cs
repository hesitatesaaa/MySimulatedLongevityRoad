using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Scripting;

[Serializable]
[Preserve]
public class PlayerConfigData
{
	[DefaultValue(5)]
	public int nextReward = 5;

	[DefaultValue("")]
	public string powerReward = "";

	[DefaultValue("")]
	public string lastReward = "";

	[DefaultValue(-1.0)]
	public double nextAdTimestamp = -1.0;

	public List<RewardedPower> rewardedPowers = new List<RewardedPower>();

	public List<PlayerOptionData> list = new List<PlayerOptionData>();

	[Preserve]
	[Obsolete("use GameProgressData.achievements instead")]
	public List<string> achievements = new List<string>();

	[Preserve]
	[Obsolete("use GameProgressData.unlocked_traits instead")]
	public List<string> unlocked_traits = new List<string>();

	public List<string> trait_editor_gamma = new List<string>();

	[DefaultValue(RainState.Add)]
	public RainState trait_editor_gamma_state;

	public List<string> trait_editor_omega = new List<string>();

	[DefaultValue(RainState.Add)]
	public RainState trait_editor_omega_state;

	public List<string> trait_editor_delta = new List<string>();

	[DefaultValue(RainState.Add)]
	public RainState trait_editor_delta_state;

	public List<string> equipment_editor = new List<string>();

	[DefaultValue(RainState.Add)]
	public RainState equipment_editor_state;

	[DefaultValue(-1)]
	public int favorite_world = -1;

	internal string worldnet = "";

	public bool premium;

	public bool valCheck2025;

	public bool magicCheck2025;

	public bool fireworksCheck2025;

	public int saveVersion = 1;

	public int lastRateID;

	public bool tutorialFinished;

	[DefaultValue(true)]
	public bool pPossible0507 = true;

	public bool premiumDisabled;

	public bool clearDebugOnStart;

	public bool testAds;

	public void initData()
	{
		PlayerConfig.dict.Clear();
		foreach (OptionAsset item in AssetManager.options_library.list)
		{
			if (item.id[0] != '_')
			{
				PlayerOptionData playerOptionData = new PlayerOptionData(item.id);
				if (item.type == OptionType.Bool)
				{
					playerOptionData.boolVal = item.default_bool;
				}
				else if (item.type == OptionType.String)
				{
					playerOptionData.stringVal = item.default_string;
				}
				else if (item.type == OptionType.Int)
				{
					playerOptionData.intVal = item.default_int;
				}
				if (Config.isMobile && item.override_bool_mobile)
				{
					playerOptionData.boolVal = item.default_bool_mobile;
				}
				add(playerOptionData);
			}
		}
	}

	public PlayerOptionData get(string pKey)
	{
		foreach (PlayerOptionData item in list)
		{
			if (string.Equals(pKey, item.name))
			{
				return item;
			}
		}
		return null;
	}

	public PlayerOptionData add(PlayerOptionData pData)
	{
		foreach (PlayerOptionData item in list)
		{
			if (string.Equals(pData.name, item.name))
			{
				PlayerConfig.dict.Add(item.name, item);
				return item;
			}
		}
		list.Add(pData);
		PlayerConfig.dict.Add(pData.name, pData);
		return pData;
	}

	public string toJson()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		using StringBuilderPool stringBuilderPool = new StringBuilderPool(8192);
		using StringWriter stringWriter = new StringWriter(stringBuilderPool.string_builder, CultureInfo.InvariantCulture);
		JsonTextWriter val = new JsonTextWriter((TextWriter)stringWriter);
		try
		{
			JsonHelper.writer.Serialize((JsonWriter)(object)val, (object)this, typeof(PlayerConfigData));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return stringWriter.ToString();
	}
}
