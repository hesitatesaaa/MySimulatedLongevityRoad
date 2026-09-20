using UnityEngine;

public class NotchMover : MonoBehaviour
{
	private float originalTopPosition;

	private RectTransform rectTransform;

	private Canvas _canvas;

	private void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		originalTopPosition = rectTransform.anchoredPosition.y;
		_canvas = ((Component)((Component)this).gameObject.transform).GetComponentInParent<Canvas>();
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		float num = Screen.height;
		Rect safeArea = Screen.safeArea;
		if (num != ((Rect)(ref safeArea)).height && !((Object)(object)_canvas == (Object)null))
		{
			float num2 = Screen.height;
			safeArea = Screen.safeArea;
			float num3 = (num2 - ((Rect)(ref safeArea)).height) / _canvas.scaleFactor;
			rectTransform.anchoredPosition = Vector2.op_Implicit(new Vector3(rectTransform.anchoredPosition.x, originalTopPosition - num3));
		}
	}
}
