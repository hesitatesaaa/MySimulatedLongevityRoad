using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class KingdomAsset : Asset
{
	public bool civ;

	[DefaultValue(-1)]
	public int default_civ_color_index;

	public bool nomads;

	public bool nature;

	public bool abandoned;

	public bool concept;

	public bool always_attack_each_other;

	public bool units_always_looking_for_enemies;

	[NonSerialized]
	public HashSet<string> assets_discrepancies;

	[NonSerialized]
	public HashSet<string> assets_discrepancies_bad;

	public bool force_look_all_chunks;

	public bool mobs;

	public bool neutral;

	public bool brain;

	public bool group_miniciv;

	public bool group_minicivs_cool;

	public bool group_creeps;

	public bool group_main;

	public bool is_forced_by_trait;

	[DefaultValue("")]
	public string forced_by_trait_kingdom_id;

	[DefaultValue("")]
	public string building_attractor_id;

	public bool count_as_danger;

	public HashSet<string> friendly_tags;

	public HashSet<string> enemy_tags;

	public HashSet<string> list_tags;

	private Dictionary<int, int> _cached_enemies;

	public ColorAsset default_kingdom_color;

	public Color color_building;

	private Sprite _cached_sprite;

	public string path_icon;

	public bool show_icon;

	public bool friendship_for_everyone;

	private ColorAsset _debug_color_asset;

	[JsonIgnore]
	public ColorAsset debug_color_asset
	{
		get
		{
			if (_debug_color_asset == null)
			{
				_debug_color_asset = AssetManager.kingdom_colors_library?.list?.GetRandom();
			}
			return _debug_color_asset;
		}
		set
		{
			_debug_color_asset = value;
		}
	}

	public Sprite getSprite()
	{
		if ((Object)(object)_cached_sprite == (Object)null)
		{
			_cached_sprite = SpriteTextureLoader.getSprite(path_icon);
		}
		return _cached_sprite;
	}

	public void setIcon(string pPath)
	{
		path_icon = pPath;
		show_icon = true;
	}

	public void addTag(string pTag)
	{
		list_tags.Add(pTag);
	}

	public void addFriendlyTag(string pTag)
	{
		friendly_tags.Add(pTag);
	}

	public void addEnemyTag(string pTag)
	{
		enemy_tags.Add(pTag);
	}

	public bool isFoe(KingdomAsset pTarget)
	{
		int value = 0;
		int hashCode = pTarget.GetHashCode();
		_cached_enemies.TryGetValue(hashCode, out value);
		if (value != 0)
		{
			return value == 1;
		}
		if (nature || pTarget.nature)
		{
			_cached_enemies.Add(hashCode, -1);
			return false;
		}
		if (this == pTarget)
		{
			_cached_enemies.Add(hashCode, always_attack_each_other ? 1 : (-1));
			return always_attack_each_other;
		}
		if (enemy_tags.Count > 0 && enemy_tags.Overlaps(pTarget.list_tags))
		{
			_cached_enemies.Add(hashCode, 1);
			return true;
		}
		pTarget.list_tags.Add(pTarget.id);
		list_tags.Add(id);
		if (friendly_tags.Count > 0 && friendly_tags.Overlaps(pTarget.list_tags))
		{
			_cached_enemies.Add(hashCode, -1);
			return false;
		}
		_cached_enemies.Add(hashCode, 1);
		return true;
	}

	public void clearKingdomColor()
	{
		default_kingdom_color = null;
	}

	public KingdomAsset()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		default_civ_color_index = -1;
		forced_by_trait_kingdom_id = string.Empty;
		building_attractor_id = string.Empty;
		count_as_danger = true;
		friendly_tags = new HashSet<string>();
		enemy_tags = new HashSet<string>();
		list_tags = new HashSet<string>();
		_cached_enemies = new Dictionary<int, int>();
		color_building = Color.white;
		path_icon = "ui/Icons/iconWarning";
		base._002Ector();
	}
}
