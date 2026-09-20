using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonGraphScalePlusMinus : MonoBehaviour
{
	public ButtonGraphScaleType button_scale_type;

	private GraphTimeScaleContainer _main_container;

	private GraphController _graph_controller;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener(new UnityAction(setScale));
		_main_container = ((Component)this).GetComponentInParent<GraphTimeScaleContainer>();
		_graph_controller = ((Component)((Component)this).transform.parent.parent).GetComponentInChildren<GraphController>();
	}

	public void setScale()
	{
		if (button_scale_type == ButtonGraphScaleType.Plus)
		{
			_main_container.timeScaleMinus();
		}
		else
		{
			_main_container.timeScalePlus();
		}
		_graph_controller.forceUpdateGraph();
	}
}
