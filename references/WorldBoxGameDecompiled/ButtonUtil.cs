using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonUtil : MonoBehaviour
{
	private Button _button;

	public void ResetState()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		if ((Object)(object)_button == (Object)null)
		{
			_button = ((Component)this).GetComponent<Button>();
			((UnityEvent)_button.onClick).AddListener(new UnityAction(playSound));
		}
		((Behaviour)_button).enabled = false;
		((Behaviour)_button).enabled = true;
	}

	private void playSound()
	{
		SoundBox.click();
	}
}
