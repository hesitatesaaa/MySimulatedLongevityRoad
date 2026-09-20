using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StatIconDarkToggle : MonoBehaviour
{
	private Color _original_color;

	private Image _background;

	private const int INDEX_MAX = 3;

	private const float SHADE_FACTOR = 0.5f;

	private int _switched_index;

	private void changeColor()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_background == (Object)null))
		{
			_switched_index++;
			if (_switched_index >= 3)
			{
				_switched_index = 0;
			}
			float num = 1f - (float)_switched_index / 3f * 0.5f;
			Color color = default(Color);
			((Color)(ref color))._002Ector(_original_color.r * num, _original_color.g * num, _original_color.b * num, _original_color.a);
			((Graphic)_background).color = color;
		}
	}

	private void Awake()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		((UnityEvent)((Component)this).gameObject.AddOrGetComponent<Button>().onClick).AddListener(new UnityAction(click));
		_background = ((Component)this).GetComponent<Image>();
		if ((Object)(object)_background != (Object)null)
		{
			_original_color = ((Graphic)_background).color;
		}
		else
		{
			_original_color = Color.white;
		}
	}

	private void click()
	{
		changeColor();
	}
}
