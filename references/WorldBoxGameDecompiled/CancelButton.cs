using UnityEngine;
using UnityEngine.UI;

public class CancelButton : MonoBehaviour
{
	public Image powerIcon;

	public bool goUp;

	public bool goDown;

	private bool _going_down;

	private bool _going_up;

	private RectTransform _rect;

	private float _timer;

	private const float Y_TOP_TARGET = 90f;

	private void Awake()
	{
		_rect = ((Component)this).GetComponent<RectTransform>();
	}

	public void setIconFrom(PowerButton pButton)
	{
		if (pButton.godPower != null && !((Object)(object)pButton.icon == (Object)null))
		{
			powerIcon.sprite = pButton.icon.sprite;
		}
	}

	private void Update()
	{
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		if (goDown != _going_down)
		{
			_going_down = goDown;
			_timer = 0f;
			if (goDown)
			{
				_timer = 0.95f;
			}
		}
		if (goUp != _going_up)
		{
			_going_up = goUp;
			_timer = -1f;
		}
		if (_timer < 1f)
		{
			_timer += Time.deltaTime / 2f;
			_timer = Mathf.Clamp(_timer, 0f, 1f);
			float num = (_going_down ? iTween.easeInOutCirc(0f, -90f, _timer) : ((!_going_up) ? iTween.easeInOutCirc(_rect.anchoredPosition.y, 0f, _timer) : iTween.easeInQuart(0f, 90f, _timer)));
			_rect.anchoredPosition = Vector2.op_Implicit(new Vector3(_rect.anchoredPosition.x, num));
		}
	}
}
