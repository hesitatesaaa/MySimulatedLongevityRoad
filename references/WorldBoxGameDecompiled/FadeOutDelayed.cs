using UnityEngine;

public class FadeOutDelayed : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _group;

	[SerializeField]
	private float _duration;

	[SerializeField]
	private float _delay;

	[SerializeField]
	[Range(0f, 1f)]
	private float _max_alpha = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _min_alpha;

	private float _time_left;

	private float _delay_time_left;

	private void OnEnable()
	{
		reset();
	}

	private void Update()
	{
		_delay_time_left -= Time.deltaTime;
		if (!(_delay_time_left > 0f) && !(_time_left <= 0f))
		{
			_time_left -= Time.deltaTime;
			_group.alpha = Mathf.Lerp(_min_alpha, _max_alpha, _time_left / _duration);
		}
	}

	private void reset()
	{
		_delay_time_left = _delay;
		_time_left = _duration;
		_group.alpha = _max_alpha;
	}
}
