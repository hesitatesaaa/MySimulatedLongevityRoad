using System;
using UnityEngine;

namespace ai.behaviours;

[Serializable]
public class BehaviourTaskActor : BehaviourTaskBase<BehaviourActionActor>
{
	public bool move_from_block;

	public bool ignore_fight_check;

	public bool in_combat;

	public string force_hand_tool = string.Empty;

	public bool flag_boat_related;

	public bool diet;

	public bool cancellable_by_reproduction;

	public bool cancellable_by_socialize;

	public bool is_fireman;

	public string path_icon = "ui/Icons/iconWarning";

	public bool show_icon;

	public float speed_multiplier = 1f;

	[NonSerialized]
	public UnitHandToolAsset cached_hand_tool_asset;

	private Sprite _cached_sprite;

	protected override string locale_key_prefix => "task_unit";

	public Sprite getSprite()
	{
		if ((Object)(object)_cached_sprite == (Object)null)
		{
			_cached_sprite = SpriteTextureLoader.getSprite(path_icon);
			if ((Object)(object)_cached_sprite == (Object)null)
			{
				Debug.LogError((object)("No sprite found for " + path_icon));
			}
		}
		return _cached_sprite;
	}

	public void setIcon(string pPath)
	{
		path_icon = pPath;
		show_icon = true;
	}
}
