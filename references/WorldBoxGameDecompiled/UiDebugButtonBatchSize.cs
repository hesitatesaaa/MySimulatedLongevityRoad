using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UiDebugButtonBatchSize : MonoBehaviour
{
	[SerializeField]
	private Text _text;

	[SerializeField]
	private Button _button;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)_button.onClick).AddListener(new UnityAction(click));
	}

	public void click()
	{
		ParallelHelper.moveDebugBatchSize();
		updateText();
	}

	private void OnEnable()
	{
		updateText();
	}

	private void updateText()
	{
		_text.text = ParallelHelper.DEBUG_BATCH_SIZE.ToString();
	}
}
