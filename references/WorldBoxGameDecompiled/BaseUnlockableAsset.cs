using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class BaseUnlockableAsset : Asset, ILocalizedAsset
{
	public string path_icon;

	[DefaultValue(true)]
	public bool show_for_unlockables_ui = true;

	[DefaultValue(null)]
	public BaseStats base_stats;

	public bool unlocked_with_achievement;

	[DefaultValue(true)]
	public bool needs_to_be_explored = true;

	public string achievement_id;

	[DefaultValue(true)]
	public bool show_in_knowledge_window = true;

	[NonSerialized]
	protected Sprite cached_sprite;

	[DefaultValue(true)]
	public bool has_locales = true;

	protected GameProgressData _progress_data => GameProgress.instance?.data;

	protected virtual HashSet<string> progress_elements
	{
		get
		{
			throw new NotImplementedException(GetType().Name);
		}
	}

	public void setUnlockedWithAchievement(string pAchievementID)
	{
		unlocked_with_achievement = true;
		achievement_id = pAchievementID;
	}

	public virtual bool unlock(bool pSaveData = true)
	{
		if (progress_elements == null)
		{
			return false;
		}
		if (isAvailable())
		{
			return false;
		}
		bool flag = false;
		if (this is ActorAsset actorAsset)
		{
			string pID = (string.IsNullOrEmpty(actorAsset.base_asset_id) ? id : actorAsset.base_asset_id);
			flag = AssetManager.actor_library.get(pID).isAvailable();
		}
		progress_elements.Add(id);
		if (pSaveData)
		{
			GameProgress.saveData();
		}
		if (!unlocked_with_achievement && has_locales && !flag)
		{
			WorldTip.showNowTop("new_knowledge_gain".Localize().Replace("$knowledge$", getLocaleID().Localize()), pTranslate: false);
		}
		return true;
	}

	public bool isUnlocked()
	{
		if (isDebugUnlockedAll())
		{
			return true;
		}
		if (isCheatEnabled())
		{
			return true;
		}
		return progress_elements.Contains(id);
	}

	public bool isAvailable()
	{
		if (isDebugUnlockedAll())
		{
			return true;
		}
		if (isCheatEnabled())
		{
			return true;
		}
		if (unlocked_with_achievement)
		{
			return GameProgress.isAchievementUnlocked(achievement_id);
		}
		if (!needs_to_be_explored)
		{
			return true;
		}
		return isUnlocked();
	}

	public bool isUnlockedByPlayer()
	{
		if (unlocked_with_achievement)
		{
			return GameProgress.isAchievementUnlocked(achievement_id);
		}
		if (!needs_to_be_explored)
		{
			return true;
		}
		return progress_elements.Contains(id);
	}

	protected virtual bool isDebugUnlockedAll()
	{
		throw new NotImplementedException(GetType().Name);
	}

	public bool isCheatEnabled()
	{
		return WorldLawLibrary.world_law_cursed_world.isEnabled();
	}

	public bool ShouldSerializebase_stats()
	{
		return !base_stats.isEmpty();
	}

	public virtual string getLocaleID()
	{
		if (!has_locales)
		{
			return null;
		}
		return id;
	}

	public string getAchievementLocaleID()
	{
		return getAchievement()?.getLocaleID();
	}

	public Achievement getAchievement()
	{
		if (!unlocked_with_achievement)
		{
			return null;
		}
		return AssetManager.achievements.get(achievement_id);
	}

	public virtual Sprite getSprite()
	{
		if (cached_sprite == null)
		{
			cached_sprite = SpriteTextureLoader.getSprite(path_icon);
		}
		return cached_sprite;
	}
}
