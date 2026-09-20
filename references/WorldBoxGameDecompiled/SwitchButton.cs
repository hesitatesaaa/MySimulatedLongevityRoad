using UnityEngine;
using UnityEngine.UI;

public class SwitchButton : MonoBehaviour
{
	public Color color_on;

	public Color color_off;

	public Text text;

	public Image icon;

	public void setEnabled(bool pValue)
	{
		if (pValue)
		{
			((Component)this).GetComponent<CanvasGroup>().alpha = 1f;
		}
		else
		{
			((Component)this).GetComponent<CanvasGroup>().alpha = 0.5f;
		}
	}

	public SwitchButton()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		color_on = Color.white;
		color_off = Color.gray;
		((MonoBehaviour)this)._002Ector();
	}
}
