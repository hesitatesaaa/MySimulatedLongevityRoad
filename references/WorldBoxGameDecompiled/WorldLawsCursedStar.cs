using UnityEngine;
using UnityEngine.UI;

public class WorldLawsCursedStar : MonoBehaviour
{
	[SerializeField]
	private Image _empty_star;

	[SerializeField]
	private Image _filled_star;

	[SerializeField]
	private Sprite _filled_star_sprite;

	[SerializeField]
	private Sprite _egg_sprite;

	private bool _filled;

	public void setStarsTransparency(float pValue)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		float a = 1f - pValue;
		Color color = ((Graphic)_empty_star).color;
		color.a = a;
		((Graphic)_empty_star).color = color;
		color.a = pValue;
		((Graphic)_filled_star).color = color;
	}

	public void setColorMultiplyAlphaBoth(Color pColor, float pValue)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (pValue < 0f)
		{
			pValue = 0f;
		}
		pColor.a = ((Graphic)_empty_star).color.a * pValue;
		((Graphic)_empty_star).color = pColor;
		pColor.a = ((Graphic)_filled_star).color.a * pValue;
		((Graphic)_filled_star).color = pColor;
	}

	public void toggleEgg(bool pState)
	{
		if (pState)
		{
			_filled_star.sprite = _egg_sprite;
		}
		else
		{
			_filled_star.sprite = _filled_star_sprite;
		}
	}

	public void toggleFilled(bool pState)
	{
		_filled = pState;
	}

	public bool isFilled()
	{
		return _filled;
	}
}
