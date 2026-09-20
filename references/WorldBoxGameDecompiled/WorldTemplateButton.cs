using System;
using UnityEngine;
using UnityEngine.UI;

public class WorldTemplateButton : MonoBehaviour
{
	public Image icon;

	public Text counter;

	public Text text;

	public PowerButton button_left;

	public PowerButton button_right;

	public PowerButton button_switch;

	public Action eventLeft;

	public Action eventRight;

	public Color color_enabled;

	public Color color_disabled;

	private MapGenTemplate _template => AssetManager.map_gen_templates.get(Config.current_map_template);

	private MapGenSettingsAsset settings_asset => AssetManager.map_gen_settings.get(((Object)((Component)this).transform).name);

	private void OnEnable()
	{
		updateCounter();
	}

	public void clickSwitch()
	{
		if (settings_asset == null)
		{
			Debug.LogError((object)("Forgot to setup gen button - " + ((Object)((Component)this).transform).name));
			return;
		}
		settings_asset.action_switch(settings_asset);
		updateCounter();
	}

	public void clickLeft()
	{
		if (settings_asset == null)
		{
			Debug.LogError((object)("Forgot to setup gen button - " + ((Object)((Component)this).transform).name));
			return;
		}
		if (settings_asset.decrease == null)
		{
			Debug.LogError((object)("Forgot to setup gen button DECREASE - " + ((Object)((Component)this).transform).name));
			return;
		}
		settings_asset.decrease(settings_asset);
		updateCounter();
	}

	public void clickRight()
	{
		if (settings_asset == null)
		{
			Debug.LogError((object)("Forgot to setup gen button - " + ((Object)((Component)this).transform).name));
			return;
		}
		if (settings_asset.increase == null)
		{
			Debug.LogError((object)("Forgot to setup gen button INCREASE - " + ((Object)((Component)this).transform).name));
			return;
		}
		settings_asset.increase(settings_asset);
		updateCounter();
	}

	public void updateCounter()
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		int num = settings_asset.action_get();
		((Component)text).GetComponent<LocalizedText>().setKeyAndUpdate(settings_asset.getLocaleID());
		if (!settings_asset.is_switch)
		{
			counter.text = num.ToString();
		}
		if (num == 0)
		{
			((Graphic)text).color = color_disabled;
			((Graphic)counter).color = color_disabled;
			((Graphic)icon).color = color_disabled;
		}
		else
		{
			((Graphic)text).color = color_enabled;
			((Graphic)counter).color = color_enabled;
			((Graphic)icon).color = color_enabled;
		}
		if (settings_asset.is_switch)
		{
			if (num == 1)
			{
				((Component)button_switch).GetComponent<CanvasGroup>().alpha = 1f;
				((Component)((Component)button_switch).transform.Find("Text")).GetComponent<LocalizedText>().setKeyAndUpdate("short_on");
				button_switch.icon.sprite = SpriteTextureLoader.getSprite("ui/icons/IconOn");
			}
			else
			{
				((Component)button_switch).GetComponent<CanvasGroup>().alpha = 0.8f;
				((Component)((Component)button_switch).transform.Find("Text")).GetComponent<LocalizedText>().setKeyAndUpdate("short_off");
				button_switch.icon.sprite = SpriteTextureLoader.getSprite("ui/icons/IconOff");
			}
		}
	}
}
