using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NameInput : MonoBehaviour
{
	public InputField inputField;

	public Text textField;

	private string LastValue;

	public bool can_be_empty;

	public bool is_onomastics;

	private Outline _outline;

	private void Start()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		textField.horizontalOverflow = (HorizontalWrapMode)0;
		if (is_onomastics)
		{
			inputField.onValidateInput = new OnValidateInput(validateOnomastics);
		}
		else
		{
			inputField.onValidateInput = new OnValidateInput(validate);
		}
	}

	private char validate(string pText, int pCharIndex, char pAddedChar)
	{
		if (pAddedChar == '<' || pAddedChar == '>')
		{
			return '\0';
		}
		return pAddedChar;
	}

	private char validateOnomastics(string pText, int pCharIndex, char pAddedChar)
	{
		char c = pAddedChar;
		bool flag = char.IsLetter(c);
		bool flag2 = char.IsWhiteSpace(c);
		bool flag3 = c == '\'';
		bool flag4 = pText.Length == 0;
		if (!(flag | flag2 | flag3))
		{
			return '\0';
		}
		if (flag4)
		{
			return char.ToUpper(c);
		}
		char num = pText[pText.Length - 1];
		bool flag5 = char.IsLetter(num);
		bool flag6 = char.IsWhiteSpace(num);
		bool flag7 = num == '\'';
		if (flag)
		{
			if (flag5)
			{
				c = char.ToLower(c);
			}
			else if (flag6)
			{
				c = char.ToUpper(c);
			}
		}
		else if (flag2)
		{
			if (flag6)
			{
				return '\0';
			}
		}
		else if (flag3 && flag7)
		{
			return '\0';
		}
		return c;
	}

	public void addListener(UnityAction<string> pAction)
	{
		((UnityEvent<string>)(object)inputField.onValueChanged).AddListener(pAction);
	}

	private void OnEnable()
	{
		((UnityEvent<string>)(object)inputField.onEndEdit).AddListener((UnityAction<string>)checkInput);
	}

	private void OnDisable()
	{
		((UnityEventBase)inputField.onEndEdit).RemoveAllListeners();
		if ((Object)(object)_outline != (Object)null)
		{
			((Behaviour)_outline).enabled = false;
		}
	}

	public void SetOutline()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_outline == (Object)null)
		{
			_outline = ((Component)inputField).gameObject.AddOrGetComponent<Outline>();
		}
		((Behaviour)_outline).enabled = true;
		Color color = ((Graphic)textField).color;
		Color effectColor = default(Color);
		((Color)(ref effectColor))._002Ector(color.r, color.g, color.b, 0.2f);
		((Shadow)_outline).effectColor = effectColor;
	}

	private void checkInput(string pInput)
	{
		if (string.IsNullOrWhiteSpace(pInput) && !can_be_empty)
		{
			inputField.text = LastValue;
		}
		else
		{
			LastValue = pInput;
		}
	}

	public void setText(string pText)
	{
		textField.text = pText;
		inputField.text = pText;
		LastValue = pText;
	}
}
