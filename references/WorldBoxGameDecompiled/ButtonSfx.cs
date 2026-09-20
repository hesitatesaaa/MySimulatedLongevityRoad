using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSfx : MonoBehaviour
{
	private Button _button;

	private void Start()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		_button = ((Component)this).GetComponent<Button>();
		((UnityEvent)_button.onClick).AddListener(new UnityAction(playSound));
	}

	private void playSound()
	{
		SoundBox.click();
		((Behaviour)_button).enabled = false;
		((Behaviour)_button).enabled = true;
	}
}
