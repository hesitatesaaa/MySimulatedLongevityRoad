using System;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class KnowledgeAsset : Asset, ILocalizedAsset
{
	[DefaultValue(true)]
	public bool show_in_knowledge_window = true;

	public string path_icon;

	public string path_icon_easter_egg;

	public string button_prefab_path;

	public string window_id;

	public KnowledgeButtonLoader load_button;

	public ButtonTipLoader tip_button_loader;

	public ButtonTooltipLoader show_tooltip;

	public LibraryGetter get_library;

	public SpriteGetter get_asset_sprite;

	public OnKnowledgeIconClick click_icon_action = showWindow;

	private Sprite _cache_icon;

	private ILibraryWithUnlockables _cache_library;

	public string getLocaleID()
	{
		return "knowledge_" + id;
	}

	public Sprite getIcon()
	{
		if (_cache_icon == null)
		{
			_cache_icon = SpriteTextureLoader.getSprite(path_icon);
		}
		return _cache_icon;
	}

	public int countTotal()
	{
		return get_library().countTotalKnowledge();
	}

	public int countUnlockedByPlayer()
	{
		return get_library().countUnlockedByPlayer();
	}

	private static void showWindow(KnowledgeAsset pAsset)
	{
		ScrollWindow.showWindow(pAsset.window_id);
	}
}
