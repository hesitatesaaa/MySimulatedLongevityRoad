using System.Collections.Generic;

public class SettingsWindow : TabbedWindow
{
	internal List<OptionButton> buttons = new List<OptionButton>();

	public void resetToDefault()
	{
		foreach (OptionButton button in buttons)
		{
			OptionAsset option_asset = button.option_asset;
			if (option_asset.type == OptionType.Bool)
			{
				PlayerConfig.setOptionBool(option_asset.id, option_asset.default_bool);
			}
			else if (option_asset.type == OptionType.String)
			{
				PlayerConfig.setOptionString(option_asset.id, option_asset.default_string);
			}
			else if (option_asset.type == OptionType.Int)
			{
				PlayerConfig.setOptionInt(option_asset.id, option_asset.default_int);
			}
			if (Config.isMobile && option_asset.override_bool_mobile)
			{
				PlayerConfig.setOptionBool(option_asset.id, option_asset.default_bool_mobile);
			}
		}
		updateAllElements(pCallCallbacks: true);
	}

	public void updateAllElements(bool pCallCallbacks = false)
	{
		foreach (OptionButton button in buttons)
		{
			button.updateElements(pCallCallbacks);
		}
	}

	private void OnDisable()
	{
		if (OptionButton.player_config_dirty)
		{
			OptionButton.player_config_dirty = false;
			PlayerConfig.saveData();
		}
	}
}
