using System;

[Serializable]
public class NameplateAsset : Asset
{
	public string path_sprite;

	public NameplateAction action;

	public NameplateBase action_main;

	public MetaType map_mode;

	public int padding_left = 12;

	public int padding_top;

	public int padding_right = 18;

	public float banner_only_mode_scale = 2f;

	public bool overlap_for_fluid_mode;

	public int max_nameplate_count = 100;
}
