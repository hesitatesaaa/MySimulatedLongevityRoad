using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnfoldButton : MonoBehaviour
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private Text _text;

	private UnfoldAction _action;

	public int offset;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)_button.onClick).AddListener((UnityAction)delegate
		{
			_action?.Invoke();
		});
	}

	public void setData(int pCount, int pOffset)
	{
		offset = pOffset;
		setText(pCount.ToString());
	}

	public void setCallback(UnfoldAction pCallback)
	{
		_action = pCallback;
	}

	public void setText(string pText)
	{
		_text.text = pText;
	}

	public void clear()
	{
		offset = 0;
	}

	public Button getButton()
	{
		return _button;
	}
}
