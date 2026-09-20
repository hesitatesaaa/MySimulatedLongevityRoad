using UnityEngine;
using UnityEngine.UI;

public class WorldTemplatesWindow : MonoBehaviour
{
	public Text text_hi;

	public Text text_size_warning;

	public Image icon_1;

	public Image icon_2;

	public Image preview_template;

	public Transform container_buttons;

	public GameObject reset_button;

	public CustomButtonSwitch switch_button;

	private void Awake()
	{
		switch_button.click_increase = increaseSize;
		switch_button.click_decrease = decreaseSize;
	}

	public void increaseSize()
	{
		int num = MapSizeLibrary.getSizes().IndexOf(Config.customMapSize);
		num++;
		if (num > MapSizeLibrary.getSizes().Length - 1)
		{
			num = 0;
		}
		Config.customMapSize = MapSizeLibrary.getSizes()[num];
	}

	public void decreaseSize()
	{
		int num = MapSizeLibrary.getSizes().IndexOf(Config.customMapSize);
		num--;
		if (num < 0)
		{
			num = MapSizeLibrary.getSizes().Length - 1;
		}
		Config.customMapSize = MapSizeLibrary.getSizes()[num];
	}

	private void Update()
	{
		MapSizeAsset mapSizeAsset = AssetManager.map_sizes.get(Config.customMapSize);
		if (mapSizeAsset.show_warning)
		{
			((Component)text_hi).gameObject.SetActive(false);
			((Component)text_size_warning).gameObject.SetActive(true);
		}
		else
		{
			((Component)text_hi).gameObject.SetActive(true);
			((Component)text_size_warning).gameObject.SetActive(false);
		}
		icon_1.sprite = mapSizeAsset.getIconSprite();
		icon_2.sprite = mapSizeAsset.getIconSprite();
	}

	private void OnEnable()
	{
		MapGenTemplate mapGenTemplate = AssetManager.map_gen_templates.get(Config.current_map_template);
		preview_template.sprite = SpriteTextureLoader.getSprite(mapGenTemplate.path_icon);
		checkButtons();
		if (mapGenTemplate.show_reset_button)
		{
			reset_button.SetActive(true);
		}
		else
		{
			reset_button.SetActive(false);
		}
	}

	public void resetTemplate()
	{
		MapGenTemplate pAsset = AssetManager.map_gen_templates.get(Config.current_map_template);
		AssetManager.map_gen_templates.resetTemplateValues(pAsset);
		checkButtons();
	}

	private void checkButtons()
	{
		MapGenTemplate pAsset = AssetManager.map_gen_templates.get(Config.current_map_template);
		for (int i = 0; i < container_buttons.childCount; i++)
		{
			WorldTemplateButton component = ((Component)container_buttons.GetChild(i)).gameObject.GetComponent<WorldTemplateButton>();
			if (!((Object)(object)component == (Object)null))
			{
				string name = ((Object)component).name;
				if (AssetManager.map_gen_settings.get(name).allowed_check(pAsset))
				{
					((Component)component).gameObject.SetActive(true);
					component.updateCounter();
				}
				else
				{
					((Component)component).gameObject.SetActive(false);
				}
			}
		}
	}
}
