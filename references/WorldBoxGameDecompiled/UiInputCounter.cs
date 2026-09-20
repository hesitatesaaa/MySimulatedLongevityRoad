using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UiInputCounter : MonoBehaviour
{
	public InputField nameText;

	private void Start()
	{
		((UnityEvent<string>)(object)nameText.onValueChanged).AddListener((UnityAction<string>)delegate
		{
			textChanged();
		});
	}

	private void OnEnable()
	{
		if (Config.game_loaded)
		{
			textChanged();
		}
	}

	public void textChanged()
	{
		((Component)this).GetComponent<Text>().text = nameText.text.Length + " / " + nameText.characterLimit;
	}
}
