using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PossessionUI : MonoBehaviour
{
	[SerializeField]
	private UiMover _mover;

	[SerializeField]
	private GameObject _inner;

	public static PossessionUI instance;

	private Text _text_backstep;

	private Text _text_dash;

	private Text _text_steal;

	private Text _text_yell;

	private Text _text_talk;

	private Text _text_walk;

	private Text _text_attack;

	private Text _text_special;

	private Text _text_jump;

	private Text _text_kick;

	private bool _planned_state;

	private bool _state_change_planned;

	private void Awake()
	{
		instance = this;
		_text_backstep = ((Component)((Component)this).transform.FindRecursive("backstep")).GetComponent<Text>();
		_text_dash = ((Component)((Component)this).transform.FindRecursive("dash")).GetComponent<Text>();
		_text_steal = ((Component)((Component)this).transform.FindRecursive("steal")).GetComponent<Text>();
		_text_yell = ((Component)((Component)this).transform.FindRecursive("yell")).GetComponent<Text>();
		_text_talk = ((Component)((Component)this).transform.FindRecursive("talk")).GetComponent<Text>();
		_text_walk = ((Component)((Component)this).transform.FindRecursive("walk")).GetComponent<Text>();
		_text_attack = ((Component)((Component)this).transform.FindRecursive("attack")).GetComponent<Text>();
		_text_special = ((Component)((Component)this).transform.FindRecursive("special")).GetComponent<Text>();
		_text_jump = ((Component)((Component)this).transform.FindRecursive("jump")).GetComponent<Text>();
		_text_kick = ((Component)((Component)this).transform.FindRecursive("kick")).GetComponent<Text>();
		setText(_text_backstep, "possession_tip_backstep", "CONTROL");
		setText(_text_dash, "possession_tip_dash", "SHIFT");
		setText(_text_steal, "possession_tip_steal", "Q");
		setText(_text_yell, "possession_tip_yell", "F");
		setText(_text_talk, "possession_tip_talk", "T");
		setText(_text_walk, "possession_tip_walk", "WASD/ARROWS");
		setText(_text_attack, "possession_tip_attack", "LEFT CLICK");
		setText(_text_special, "possession_tip_special", "MIDDLE CLICK");
		setText(_text_jump, "possession_tip_jump", "SPACE");
		setText(_text_kick, "possession_tip_kick", "RIGHT CLICK");
		toggleInstance(pState: false);
	}

	private void Update()
	{
		if (_state_change_planned)
		{
			_state_change_planned = false;
			if (_mover.visible != _planned_state)
			{
				toggleInstance(_planned_state);
			}
		}
	}

	private void setText(Text pTextField, string pLocaleID, string pKey)
	{
		string text = LocalizedTextManager.getText(pLocaleID);
		text = Toolbox.coloredString(text, "white");
		pKey = Toolbox.coloredString(pKey, "#F3961F");
		pTextField.text = text + " --> [ " + pKey + " ]";
	}

	public static void toggle(bool pState)
	{
		instance._state_change_planned = true;
		instance._planned_state = pState;
	}

	private void toggleInstance(bool pState)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		if (pState)
		{
			instance._inner.SetActive(true);
		}
		instance._mover.setVisible(pState, pNow: false, (TweenCallback)delegate
		{
			if (!pState && !instance._mover.visible)
			{
				instance._inner.SetActive(false);
			}
		});
	}
}
