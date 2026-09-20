using UnityEngine;
using UnityEngine.UI;

public class FadeInOutAnimation : MonoBehaviour
{
	private const float FADE_OUT_BOUND = 0.1f;

	private const float FADE_SPEED = 0.015f;

	private const float INTERVAL = 0.025f;

	public float alpha_max = 1f;

	private float _current_alpha;

	private float _timer = 0.025f;

	private bool _fade_out = true;

	[SerializeField]
	private Image _image;

	public void Awake()
	{
		checkInit();
	}

	public void checkInit()
	{
		_image = ((Component)this).GetComponent<Image>();
	}

	private void updateAlpha()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		_timer -= Time.deltaTime;
		if (!(_timer < 0f))
		{
			return;
		}
		_timer = 0.025f;
		if (_fade_out)
		{
			_current_alpha -= 0.015f;
			if (_current_alpha <= 0.1f)
			{
				_current_alpha = 0.1f;
				_fade_out = false;
			}
		}
		else
		{
			_current_alpha += 0.015f;
			if (_current_alpha >= alpha_max)
			{
				_current_alpha = alpha_max;
				_fade_out = true;
			}
		}
		Color color = ((Graphic)_image).color;
		color.a = _current_alpha;
		((Graphic)_image).color = color;
	}

	public void resetToFadeOut()
	{
		_current_alpha = 1f;
		_fade_out = true;
		updateAlpha();
	}

	public void resetToFadeIn()
	{
		_current_alpha = 0f;
		_fade_out = false;
		updateAlpha();
	}

	public void reset()
	{
		resetToFadeOut();
	}

	private void OnEnable()
	{
		reset();
	}

	private void Update()
	{
		updateAlpha();
	}
}
