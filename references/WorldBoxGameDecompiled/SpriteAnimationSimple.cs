using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimationSimple : MonoBehaviour
{
	[SerializeField]
	public float _time_between_frames = 0.1f;

	private Image _renderer;

	[SerializeField]
	public Sprite[] _frames;

	private EffectParticlesCursorDelegate _action_finish;

	private int _frame_index_current;

	private float _next_frame_time;

	private void Awake()
	{
		_renderer = ((Component)this).GetComponent<Image>();
	}

	public void setActionFinish(EffectParticlesCursorDelegate pAction)
	{
		_action_finish = pAction;
	}

	public void resetAnim()
	{
		_frame_index_current = 0;
		_next_frame_time = _time_between_frames;
		updateFrame();
	}

	internal virtual void update(float pElapsed)
	{
		if (_next_frame_time > 0f)
		{
			_next_frame_time -= pElapsed;
			if (_next_frame_time > 0f)
			{
				return;
			}
		}
		_next_frame_time = _time_between_frames;
		_frame_index_current++;
		if (_frame_index_current >= _frames.Length)
		{
			if (_action_finish != null)
			{
				_action_finish((MonoBehaviour)(object)this);
			}
		}
		else
		{
			updateFrame();
		}
	}

	private void updateFrame()
	{
		Sprite sprite = _frames[_frame_index_current];
		_renderer.sprite = sprite;
	}

	public void setFrames(Sprite[] pFrames)
	{
		_frames = pFrames;
	}
}
