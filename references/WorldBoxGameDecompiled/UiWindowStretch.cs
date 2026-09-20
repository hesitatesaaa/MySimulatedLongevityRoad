using UnityEngine;
using UnityEngine.EventSystems;

public class UiWindowStretch : EventTrigger
{
	public RectTransform stretchTarget;

	private bool dragging;

	private Transform mainTransform;

	private Transform canvasContainer;

	public Vector3 posClicked;

	public Vector3 newSize;

	public Vector2 originSizeDelta;

	private void Start()
	{
	}

	public void Update()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (dragging)
		{
			Vector3 val = Vector2.op_Implicit(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
			newSize = posClicked - val;
			stretchTarget.sizeDelta = new Vector2(originSizeDelta.x - newSize.x, originSizeDelta.y + newSize.y);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!dragging)
		{
			posClicked = Vector2.op_Implicit(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
			originSizeDelta = stretchTarget.sizeDelta;
		}
		dragging = true;
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		dragging = false;
	}
}
