using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UiAutoTesterButton : MonoBehaviour
{
	public Sprite button_on;

	public Sprite button_off;

	public Text text;

	public Button button;

	private string _tester_name;

	public void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)button.onClick).AddListener(new UnityAction(click));
		_tester_name = ((Object)((Component)this).gameObject.transform).name;
	}

	public void Start()
	{
		_tester_name = ((Object)((Component)this).gameObject.transform).name;
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
		bool flag = true;
		string text2 = name;
		for (int i = 0; i < text2.Length; i++)
		{
			char c = text2[i];
			if (flag)
			{
				c = char.ToUpper(c);
				flag = false;
			}
			if (num == 0)
			{
				text += c;
			}
			else
			{
				if (c == '_')
				{
					c = ' ';
					flag = true;
				}
				text += c;
			}
			num++;
		}
		this.text.text = text;
	}

	public void click()
	{
		AssetManager.loadAutoTester();
		if (World.world.auto_tester.active_tester == _tester_name)
		{
			World.world.auto_tester.toggleAutoTester();
		}
		else
		{
			World.world.auto_tester.create(_tester_name);
			((Component)World.world.auto_tester).gameObject.SetActive(true);
		}
		checkButtonGraphics();
		ScrollWindow.hideAllEvent();
	}

	private void checkButtonGraphics()
	{
		if (World.world.auto_tester.active && World.world.auto_tester.active_tester == _tester_name)
		{
			((Component)button).GetComponent<Image>().sprite = button_on;
		}
		else
		{
			((Component)button).GetComponent<Image>().sprite = button_off;
		}
	}
}
