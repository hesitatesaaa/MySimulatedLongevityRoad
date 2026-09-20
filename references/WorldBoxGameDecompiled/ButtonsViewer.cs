using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsViewer : MonoBehaviour
{
	private List<PowerButton> buttons;

	private Transform content;

	private float lastX;

	private float lastY;

	private Canvas canvas;

	private void Start()
	{
		content = ((Component)this).transform.parent;
		canvas = CanvasMain.instance.canvas_ui;
		buttons = new List<PowerButton>();
		_ = ((Component)this).transform.childCount;
		for (int i = 0; i < ((Component)this).transform.childCount; i++)
		{
			GameObject gameObject = ((Component)((Component)this).transform.GetChild(i)).gameObject;
			if (gameObject.HasComponent<PowerButton>() && gameObject.activeSelf)
			{
				buttons.Add(gameObject.GetComponent<PowerButton>());
			}
			else if (!gameObject.HasComponent<Image>() || !gameObject.activeSelf)
			{
				Object.Destroy((Object)(object)gameObject);
			}
		}
	}

	private void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (lastX == content.position.x && lastY == content.position.y)
		{
			return;
		}
		lastX = content.position.x;
		lastY = content.position.y;
		int num = 0;
		int num2 = 0;
		bool flag = false;
		for (int i = 0; i < buttons.Count; i++)
		{
			PowerButton powerButton = buttons[i];
			if (flag)
			{
				num2++;
				((Component)powerButton).gameObject.SetActive(false);
				continue;
			}
			num++;
			Vector3[] array = (Vector3[])(object)new Vector3[4];
			powerButton.rect_transform.GetWorldCorners(array);
			float num3 = Mathf.Max(new float[4]
			{
				array[0].x,
				array[1].x,
				array[2].x,
				array[3].x
			});
			float num4 = Mathf.Min(new float[4]
			{
				array[0].x,
				array[1].x,
				array[2].x,
				array[3].x
			});
			if (num3 < 0f || num4 > (float)Screen.width)
			{
				((Component)powerButton).gameObject.SetActive(false);
				if (num4 > (float)Screen.width)
				{
					flag = true;
				}
			}
			else
			{
				((Component)powerButton).gameObject.SetActive(true);
			}
		}
	}
}
