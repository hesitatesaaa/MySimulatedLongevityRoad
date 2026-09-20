using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonClickMaptemplate : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__2_1;

		internal void _003CAwake_003Eb__2_1()
		{
			if (InputHelpers.mouseSupported)
			{
				Tooltip.hideTooltip();
			}
		}
	}

	private Button _button;

	private MapGenTemplate _template;

	private void Awake()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		string name = ((Object)((Component)this).transform).name;
		_button = ((Component)this).GetComponent<Button>();
		((UnityEvent)_button.onClick).AddListener(new UnityAction(click));
		if (Input.mousePresent)
		{
			_button.OnHover((UnityAction)delegate
			{
				if (InputHelpers.mouseSupported)
				{
					showTooltip();
				}
			});
			Button button = _button;
			object obj = _003C_003Ec._003C_003E9__2_1;
			if (obj == null)
			{
				UnityAction val = delegate
				{
					if (InputHelpers.mouseSupported)
					{
						Tooltip.hideTooltip();
					}
				};
				_003C_003Ec._003C_003E9__2_1 = val;
				obj = (object)val;
			}
			button.OnHoverOut((UnityAction)obj);
		}
		_template = AssetManager.map_gen_templates.get(name);
		((Component)((Component)this).transform.Find("preview_icon")).GetComponent<Image>().sprite = SpriteTextureLoader.getSprite(_template.path_icon);
	}

	private void showTooltip()
	{
		Tooltip.show(((Component)_button).gameObject, "normal", new TooltipData
		{
			tip_name = _template.getLocaleID(),
			tip_description = _template.getDescriptionID()
		});
	}

	public void click()
	{
		if (!InputHelpers.mouseSupported)
		{
			if (!Tooltip.isShowingFor(((Component)_button).gameObject))
			{
				showTooltip();
				return;
			}
			Tooltip.hideTooltipNow();
		}
		Config.current_map_template = _template.id;
		ScrollWindow.showWindow("new_world_templates_2");
	}
}
