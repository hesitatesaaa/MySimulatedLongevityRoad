using UnityEngine;
using UnityEngine.UI;

public class PossessionModeButton : MonoBehaviour
{
	public PossessionActionMode mode;

	[SerializeField]
	private Image _image_icon;

	[SerializeField]
	private Image _image_background;

	public void updateGraphics(PossessionActionMode pCurrentSelectedMode)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (mode == pCurrentSelectedMode)
		{
			((Graphic)_image_icon).color = Color.white;
			((Graphic)_image_background).color = Color.white;
		}
		else
		{
			((Graphic)_image_icon).color = new Color(0.3f, 0.3f, 0.3f, 1f);
			((Graphic)_image_background).color = Color.gray;
		}
	}
}
