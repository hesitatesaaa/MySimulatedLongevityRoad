using System;

[Serializable]
public class MapGenSettingsAsset : Asset, ILocalizedAsset
{
	public bool is_switch;

	public int min_value;

	public int max_value;

	public MapGenSettingsDelegateBool allowed_check;

	public MapGenSettingsDelegate increase;

	public MapGenSettingsDelegate decrease;

	public MapGenSettingsDelegateSwitch action_switch;

	public MapGenSettingsDelegateGet action_get;

	public MapGenSettingsDelegateSet action_set;

	public string getLocaleID()
	{
		return id;
	}
}
