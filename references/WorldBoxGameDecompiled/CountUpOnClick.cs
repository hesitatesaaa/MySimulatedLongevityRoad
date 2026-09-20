using System.Globalization;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CountUpOnClick : MonoBehaviour
{
	private const float TWEEN_DURATION = 0.45f;

	[SerializeField]
	private Text _text;

	private Tweener _cur_tween;

	private int _value;

	private string _end = "";

	private bool _value_updated;

	private void Start()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		Button val = default(Button);
		if (!((Component)this).TryGetComponent<Button>(ref val) || (Object)(object)_text == (Object)null)
		{
			((Behaviour)this).enabled = false;
		}
		else if (!_value_updated && !checkString())
		{
			((Behaviour)this).enabled = false;
		}
		else
		{
			((UnityEvent)val.onClick).AddListener(new UnityAction(countAnimation));
		}
	}

	public void setValue(int pValue, string pEnd = "")
	{
		((Behaviour)this).enabled = true;
		_value = pValue;
		_end = pEnd;
		_value_updated = true;
		_text.text = _value.ToText(4) + pEnd;
	}

	private bool checkString()
	{
		string text = _text.text;
		if (!checkIfStringIsLegit(text))
		{
			return false;
		}
		if (!int.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out _value))
		{
			((Behaviour)this).enabled = false;
			return false;
		}
		return true;
	}

	private bool checkIfStringIsLegit(string pString)
	{
		if (string.IsNullOrEmpty(pString))
		{
			return false;
		}
		if (!pString.All(char.IsDigit))
		{
			return false;
		}
		return true;
	}

	private void countAnimation()
	{
		if (_value_updated)
		{
			_value_updated = false;
		}
		checkDestroyTween();
		_cur_tween = (Tweener)(object)_text.DOUpCounter(0, _value, 0.45f, _end);
	}

	public Text getText()
	{
		return _text;
	}

	private void OnDisable()
	{
		checkDestroyTween();
	}

	private void checkDestroyTween()
	{
		TweenExtensions.Kill((Tween)(object)_cur_tween, true);
		_cur_tween = null;
	}
}
