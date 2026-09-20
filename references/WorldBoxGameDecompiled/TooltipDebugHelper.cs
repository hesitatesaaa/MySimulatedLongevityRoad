using System;
using UnityEngine;
using UnityEngine.UI;

public class TooltipDebugHelper
{
	private static GameObject _debug_canvas;

	public static void checkCreate()
	{
		if (DebugConfig.isOn(DebugOption.DebugTooltipUI))
		{
			MapBox.on_world_loaded = (Action)Delegate.Combine(MapBox.on_world_loaded, new Action(loadButtons));
			HotkeyAsset cancel = HotkeyLibrary.cancel;
			cancel.just_pressed_action = (HotkeyAction)Delegate.Combine(cancel.just_pressed_action, new HotkeyAction(killButtons));
		}
	}

	public static void killButtons(HotkeyAsset pAsset)
	{
		Object.Destroy((Object)(object)_debug_canvas);
		_debug_canvas = null;
	}

	public static void loadButtons()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		_debug_canvas = new GameObject("Canvas Debug", new Type[1] { typeof(RectTransform) });
		RectTransform component = _debug_canvas.GetComponent<RectTransform>();
		((Transform)component).SetParent(((Component)CanvasMain.instance.canvas_ui).transform, true);
		component.anchorMin = new Vector2(0f, 0f);
		component.anchorMax = new Vector2(1f, 1f);
		component.offsetMin = new Vector2(0f, 0f);
		component.offsetMax = new Vector2(0f, 0f);
		((Transform)component).localScale = new Vector3(1f, 1f, 1f);
		GridLayoutGroup obj = ((Component)(object)component).AddComponent<GridLayoutGroup>();
		obj.cellSize = new Vector2(28f, 28f);
		obj.spacing = new Vector2(2f, 2f);
		using ListPool<PowerButton> listPool = new ListPool<PowerButton>(PowerButton.power_buttons.Count + PowerButton.toggle_buttons.Count);
		listPool.AddRange(PowerButton.power_buttons);
		listPool.AddRange(PowerButton.toggle_buttons);
		for (int i = 0; i < 9; i++)
		{
			listPool.Shuffle();
			foreach (ref PowerButton item in listPool)
			{
				PowerButton current = item;
				((Component)current).gameObject.SetActive(false);
				PowerButton powerButton = Object.Instantiate<PowerButton>(current, (Transform)(object)component);
				((Object)((Component)powerButton).transform).name = ((Object)((Component)current).transform).name;
				powerButton.destroyLockIcon();
				IconRotationAnimation iconRotationAnimation = ((Component)powerButton).gameObject.AddComponent<IconRotationAnimation>();
				iconRotationAnimation.delay = Randy.randomFloat(1f, 10f);
				iconRotationAnimation.randomDelay = true;
				((Component)current).gameObject.SetActive(true);
				((Component)powerButton).gameObject.SetActive(true);
			}
		}
	}
}
