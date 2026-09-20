using UnityEngine;
using UnityEngine.UI;

public class LocusDot : MonoBehaviour
{
	[SerializeField]
	private Image _status;

	internal Image status => _status;

	public void colorDot(Color pColor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)_status).color = pColor;
	}

	public void colorDot(char pGeneticCode)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		colorDot(NucleobaseHelper.getColor(pGeneticCode));
	}
}
