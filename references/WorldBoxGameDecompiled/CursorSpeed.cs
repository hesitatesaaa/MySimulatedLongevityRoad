using UnityEngine;

public class CursorSpeed
{
	private Vector2 _lastFramePos;

	private Vector2 _curFramePos;

	private float difference;

	public float speed;

	public float fmod_speed;

	public void update()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetMouseButton(0))
		{
			((Vector2)(ref _lastFramePos)).Set(_curFramePos.x, _curFramePos.y);
			Vector3 mousePosition = Input.mousePosition;
			((Vector2)(ref _curFramePos)).Set(mousePosition.x, mousePosition.y);
			difference = Toolbox.DistVec2Float(_curFramePos, _lastFramePos) / 2f;
			if (difference > speed)
			{
				speed = difference;
			}
		}
		speed = speed * 0.95f - 1f;
		if (speed < 0f)
		{
			speed = 0f;
		}
		fmod_speed = (int)speed;
		if (fmod_speed > 100f)
		{
			fmod_speed = 100f;
		}
	}

	public void debug(DebugTool pTool)
	{
		pTool.setText("difference", difference, 0f, pShowBar: false, 0L);
		pTool.setText("speed", speed, 0f, pShowBar: false, 0L);
		pTool.setText("fmod_speed", fmod_speed, 0f, pShowBar: false, 0L);
	}
}
