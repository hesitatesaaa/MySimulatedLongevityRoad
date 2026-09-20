using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonGraphScale : MonoBehaviour
{
	public Sprite sprite_on;

	public Sprite sprite_off;

	public GraphTimeScale button_scale;

	private GraphTimeScaleContainer _main_container;

	private GraphController _graph_controller;

	private Image _image;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener(new UnityAction(setScale));
		_image = ((Component)this).GetComponent<Image>();
		_main_container = ((Component)this).GetComponentInParent<GraphTimeScaleContainer>();
		_graph_controller = ((Component)((Component)this).transform.parent.parent).GetComponentInChildren<GraphController>();
		checkSpriteStatus();
	}

	private void Update()
	{
		checkSpriteStatus();
	}

	private void checkSpriteStatus()
	{
		if (_main_container.current_scale == button_scale)
		{
			_image.sprite = sprite_on;
		}
		else
		{
			_image.sprite = sprite_off;
		}
	}

	public void setScale()
	{
		_main_container.setTimeScale(button_scale);
		_graph_controller.forceUpdateGraph();
	}
}
