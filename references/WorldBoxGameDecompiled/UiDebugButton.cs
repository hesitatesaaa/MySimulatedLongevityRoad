using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UiDebugButton : MonoBehaviour
{
	public Sprite button_on;

	public Sprite button_off;

	public Text text;

	public Image iconOn;

	public Button button;

	private DebugOption _debug_option;

	public void Awake()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		string name = ((Object)((Component)this).gameObject.transform).name;
		try
		{
			_debug_option = (DebugOption)Enum.Parse(typeof(DebugOption), name);
		}
		catch (Exception)
		{
			Debug.LogError((object)("THERE'S NO DEBUG OPTION CALLED " + name));
			throw;
		}
		((UnityEvent)button.onClick).AddListener(new UnityAction(click));
	}

	public void Start()
	{
		text.text = ((Object)((Component)((Component)this).transform).gameObject).name;
		checkButtonGraphics();
	}

	private void OnEnable()
	{
		checkButtonGraphics();
	}

	private void OnValidate()
	{
		string name = ((Object)((Component)this).gameObject.transform).name;
		string text = "";
		int num = 0;
		string text2 = name;
		for (int i = 0; i < text2.Length; i++)
		{
			char c = text2[i];
			if (num == 0)
			{
				text += c;
			}
			else
			{
				if (char.IsUpper(c))
				{
					text += " ";
				}
				text += c;
			}
			num++;
		}
		this.text.text = text;
	}

	public void click()
	{
		DebugConfig.switchOption(_debug_option);
		checkButtonGraphics();
	}

	private void checkButtonGraphics()
	{
		if (DebugConfig.isOn(_debug_option))
		{
			((Component)button).GetComponent<Image>().sprite = button_on;
		}
		else
		{
			((Component)button).GetComponent<Image>().sprite = button_off;
		}
	}
}
