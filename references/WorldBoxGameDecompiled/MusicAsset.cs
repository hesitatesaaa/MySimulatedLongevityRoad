using System;

[Serializable]
public class MusicAsset : Asset
{
	public int need_amount;

	public bool marker_same = true;

	public string marker_different = string.Empty;

	public bool disable_param_after_start = true;

	public float min_zoom = 70f;

	public int min_tiles_to_play;

	public bool is_environment;

	public bool is_param;

	public bool is_faction;

	public bool is_unit_param;

	public MusicAssetDelegate special_delegate_units;

	public string fmod_path;

	public bool mini_map_only;

	public bool civilization;

	[NonSerialized]
	public MusicBoxContainerTiles container_tiles;

	[NonSerialized]
	public TileTypeBase[] tile_types;

	public string[] tile_type_strings;

	public FmodAction action;

	public MusicLayerPriority priority;

	public void setTileTypes(params string[] pTileTypes)
	{
		tile_type_strings = pTileTypes;
	}
}
