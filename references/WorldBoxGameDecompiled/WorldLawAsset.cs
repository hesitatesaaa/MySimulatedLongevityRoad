using System;
using System.Runtime.CompilerServices;

[Serializable]
public class WorldLawAsset : BaseAugmentationAsset, IDescription2Asset, IDescriptionAsset, ILocalizedAsset
{
	public bool default_state;

	public PlayerOptionAction on_state_change;

	public PlayerOptionAction on_state_enabled;

	public OnWorldLoadAction on_world_load;

	public string icon_path;

	public bool can_turn_off = true;

	public bool requires_premium;

	private bool _cached_enabled;

	private static WorldLaws _world_laws => World.world.world_laws;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isEnabled()
	{
		return _cached_enabled;
	}

	public bool isEnabledRaw()
	{
		return getOption().boolVal;
	}

	public PlayerOptionData getOption()
	{
		return _world_laws.dict[id];
	}

	public void updateCachedEnabled(WorldLaws pWorldLaws)
	{
		_cached_enabled = pWorldLaws.isEnabled(id);
	}

	public void toggle(bool pState)
	{
		getOption().boolVal = pState;
		_cached_enabled = pState;
	}

	public override BaseCategoryAsset getGroup()
	{
		return AssetManager.world_law_groups.get(group_id);
	}

	public override string getLocaleID()
	{
		return id + "_title";
	}

	public string getDescriptionID()
	{
		return id + "_description";
	}

	public string getDescriptionID2()
	{
		return id + "_description_2";
	}

	public string getTranslatedName()
	{
		return LocalizedTextManager.getText(getLocaleID());
	}

	public string getTranslatedDescription()
	{
		return LocalizedTextManager.getText(getDescriptionID());
	}

	public string getTranslatedDescription2()
	{
		return LocalizedTextManager.getText(getDescriptionID2());
	}
}
