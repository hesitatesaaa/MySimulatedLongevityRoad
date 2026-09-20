using UnityEngine;
using UnityEngine.UI;

public class ToggleIcon : MonoBehaviour
{
	public Sprite spriteON;

	public Sprite spriteOFF;

	private Image image;

	private void Awake()
	{
		image = ((Component)this).GetComponent<Image>();
	}

	internal void updateIcon(bool pEnabled)
	{
		if ((Object)(object)image == (Object)null)
		{
			image = ((Component)this).GetComponent<Image>();
		}
		if (pEnabled)
		{
			image.sprite = spriteON;
		}
		else
		{
			image.sprite = spriteOFF;
		}
	}

	internal void updateIconMultiToggle(bool pActive, bool pEnabled)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)image == (Object)null)
		{
			image = ((Component)this).GetComponent<Image>();
		}
		if (pActive)
		{
			((Component)image).gameObject.SetActive(true);
		}
		else
		{
			((Component)image).gameObject.SetActive(false);
		}
		if (pActive)
		{
			((Graphic)image).color = Color.white;
		}
		else if (pEnabled)
		{
			((Graphic)image).color = Color32.op_Implicit(Toolbox.color_grey);
		}
		else
		{
			((Graphic)image).color = Color.white;
		}
		if (pEnabled)
		{
			image.sprite = spriteON;
		}
		else
		{
			image.sprite = spriteOFF;
		}
	}
}
