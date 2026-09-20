using UnityEngine;
using UnityEngine.EventSystems;

public class DebugUiDrag : EventTrigger
{
	private bool dragging;

	private Transform mainTransform;

	private Transform canvasContainer;

	private DebugTool _tool;

	private Canvas _canvas;

	private void Start()
	{
		_tool = ((Component)((Component)this).transform).GetComponentInParent<DebugTool>();
		_canvas = ((Component)((Component)this).transform).GetComponentInParent<Canvas>();
		mainTransform = ((Component)_tool).transform;
		canvasContainer = mainTransform.parent;
	}

	public void Update()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (dragging)
		{
			Vector3 position = Vector2.op_Implicit(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
			mainTransform.SetParent((Transform)null, true);
			mainTransform.SetParent(canvasContainer, true);
			Vector2 sizeDelta = ((Component)_tool).GetComponent<RectTransform>().sizeDelta;
			position.x += sizeDelta.x / 2f - 75f;
			position.y += 20f;
			mainTransform.position = position;
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		dragging = true;
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		dragging = false;
	}
}
