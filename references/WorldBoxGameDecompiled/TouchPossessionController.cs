using System.Collections.Generic;
using UnityEngine;

public class TouchPossessionController : MonoBehaviour
{
	public static TouchPossessionController instance;

	[SerializeField]
	private GameObject _button_dash;

	[SerializeField]
	private GameObject _button_jump;

	[SerializeField]
	private GameObject _button_backstep;

	[SerializeField]
	private GameObject _button_attack;

	[SerializeField]
	private GameObject _button_kick;

	[SerializeField]
	private GameObject _button_talk;

	[SerializeField]
	private GameObject _button_swear;

	[SerializeField]
	private GameObject _button_steal;

	public List<PossessionModeButton> possession_mode_buttons = new List<PossessionModeButton>();

	[SerializeField]
	private RectTransform _rect;

	[SerializeField]
	private UltimateJoystick _right_joystick;

	private static bool _action_pressed_jump;

	private static bool _action_pressed_dash;

	private static bool _action_pressed_backstep;

	public static PossessionActionMode _current_mode;

	private void Awake()
	{
		instance = this;
	}

	private void OnEnable()
	{
		onResizeResolution(Screen.width, Screen.height);
		checkButtonGraphics();
		setMode(PossessionActionMode.Attack);
	}

	private void Update()
	{
		checkActiveButtons();
	}

	private void checkActiveButtons()
	{
		if (ControllableUnit.isControllingUnit() && !ControllableUnit.isControllingCrabzilla())
		{
			Actor controllableUnit = ControllableUnit.getControllableUnit();
			ActorAsset asset = controllableUnit.asset;
			_button_dash.gameObject.SetActive(asset.control_can_dash);
			_button_jump.gameObject.SetActive(asset.control_can_jump);
			_button_backstep.gameObject.SetActive(asset.control_can_backstep);
			_button_attack.gameObject.SetActive(!asset.skip_fight_logic);
			_button_kick.gameObject.SetActive(asset.control_can_kick);
			_button_talk.gameObject.SetActive(asset.control_can_talk && !controllableUnit.hasTrait("mute"));
			_button_swear.gameObject.SetActive(asset.control_can_swear && !controllableUnit.hasTrait("mute"));
			_button_steal.gameObject.SetActive(asset.control_can_steal);
		}
	}

	private void onResizeResolution(float pWidth, float pHeight)
	{
		_right_joystick.UpdateSizeAndPlacement(_rect);
	}

	public static bool isActionPressedJump()
	{
		return _action_pressed_jump;
	}

	public static bool isActionPressedDash()
	{
		return _action_pressed_dash;
	}

	public static bool isActionPressedBackStep()
	{
		return _action_pressed_backstep;
	}

	public static bool isSelectedActionAttack()
	{
		return isMode(PossessionActionMode.Attack);
	}

	public static bool isSelectedActionTalk()
	{
		return isMode(PossessionActionMode.Talk);
	}

	public static bool isSelectedActionSwear()
	{
		return isMode(PossessionActionMode.Swear);
	}

	public static bool isSelectedActionSteal()
	{
		return isMode(PossessionActionMode.Steal);
	}

	public static bool isSelectedActionKick()
	{
		return isMode(PossessionActionMode.Kick);
	}

	public static void pressJump()
	{
		_action_pressed_jump = true;
	}

	public static void pressDash()
	{
		_action_pressed_dash = true;
	}

	public static void pressBackStep()
	{
		_action_pressed_backstep = true;
	}

	public void selectModeAttack()
	{
		WorldTip.showNow("possession_action_mode_attack", pTranslate: true, "top");
		setMode(PossessionActionMode.Attack);
	}

	public void selectModeTalk()
	{
		WorldTip.showNow("possession_action_mode_talk", pTranslate: true, "top");
		setMode(PossessionActionMode.Talk);
	}

	public void selectModeSwear()
	{
		WorldTip.showNow("possession_action_mode_swear", pTranslate: true, "top");
		setMode(PossessionActionMode.Swear);
	}

	public void selectModeSteal()
	{
		WorldTip.showNow("possession_action_mode_steal", pTranslate: true, "top");
		setMode(PossessionActionMode.Steal);
	}

	public void selectModeKick()
	{
		WorldTip.showNow("possession_action_mode_kick", pTranslate: true, "top");
		setMode(PossessionActionMode.Kick);
	}

	private static bool isMode(PossessionActionMode pMode)
	{
		return _current_mode == pMode;
	}

	private void setMode(PossessionActionMode pMode)
	{
		_current_mode = pMode;
		checkButtonGraphics();
	}

	private void checkButtonGraphics()
	{
		foreach (PossessionModeButton possession_mode_button in possession_mode_buttons)
		{
			possession_mode_button.updateGraphics(_current_mode);
		}
	}

	private void LateUpdate()
	{
		clearActions();
	}

	private void clearActions()
	{
		_action_pressed_jump = false;
		_action_pressed_dash = false;
		_action_pressed_backstep = false;
	}
}
