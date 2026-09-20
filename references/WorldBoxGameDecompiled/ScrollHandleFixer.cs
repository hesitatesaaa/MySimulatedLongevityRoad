using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScrollHandleFixer : MonoBehaviour, ILayoutSelfController, ILayoutController
{
	private const float MIN_SIZE = 0.05f;

	[SerializeField]
	private Scrollbar _bar;

	private bool _bar_updating;

	private void Awake()
	{
		((UnityEvent<float>)(object)_bar.onValueChanged).AddListener((UnityAction<float>)delegate
		{
			if (!_bar_updating && !(_bar.size > 0.05f))
			{
				_bar_updating = true;
				_bar.size = 0.05f;
				_bar_updating = false;
			}
		});
	}

	private void Update()
	{
		checkBarSize();
	}

	private void LateUpdate()
	{
		checkBarSize();
	}

	public void SetLayoutHorizontal()
	{
		checkBarSize();
	}

	public void SetLayoutVertical()
	{
		checkBarSize();
	}

	private void checkBarSize()
	{
		if (!(_bar.size > 0.05f))
		{
			_bar.size = 0.05f;
		}
	}
}
