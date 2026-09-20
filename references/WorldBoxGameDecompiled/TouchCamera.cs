using UnityEngine;

public class TouchCamera : MonoBehaviour
{
	private Vector2?[] oldTouchPositions = new Vector2?[2];

	private Vector2 oldTouchVector;

	private float oldTouchDistance;

	private Camera _camera;

	private const int orthographicSizeMin = 10;

	internal float orthographicSizeMax = 130f;

	private void Awake()
	{
		_camera = Camera.main;
	}

	private void Update()
	{
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		Touch touch;
		if (Input.touchCount == 0)
		{
			oldTouchPositions[0] = null;
			oldTouchPositions[1] = null;
		}
		else if (Input.touchCount == 1)
		{
			if (!oldTouchPositions[0].HasValue || oldTouchPositions[1].HasValue)
			{
				Vector2?[] array = oldTouchPositions;
				touch = Input.GetTouch(0);
				array[0] = ((Touch)(ref touch)).position;
				oldTouchPositions[1] = null;
				return;
			}
			touch = Input.GetTouch(0);
			Vector2 position = ((Touch)(ref touch)).position;
			Transform transform = ((Component)this).transform;
			Vector3 position2 = transform.position;
			Transform transform2 = ((Component)this).transform;
			Vector2? val = oldTouchPositions[0];
			Vector2 val2 = position;
			Vector2? val3 = (val.HasValue ? new Vector2?(val.GetValueOrDefault() - val2) : ((Vector2?)null));
			float orthographicSize = _camera.orthographicSize;
			Vector2? val4 = (val3.HasValue ? new Vector2?(val3.GetValueOrDefault() * orthographicSize) : ((Vector2?)null));
			float num = _camera.pixelHeight;
			transform.position = position2 + transform2.TransformDirection(Vector2.op_Implicit((val4.HasValue ? new Vector2?(val4.GetValueOrDefault() / num * 2f) : ((Vector2?)null)).Value));
			oldTouchPositions[0] = position;
		}
		else if (!oldTouchPositions[1].HasValue)
		{
			Vector2?[] array2 = oldTouchPositions;
			touch = Input.GetTouch(0);
			array2[0] = ((Touch)(ref touch)).position;
			Vector2?[] array3 = oldTouchPositions;
			touch = Input.GetTouch(1);
			array3[1] = ((Touch)(ref touch)).position;
			Vector2? val4 = oldTouchPositions[0];
			Vector2? val3 = oldTouchPositions[1];
			oldTouchVector = ((val4.HasValue & val3.HasValue) ? new Vector2?(val4.GetValueOrDefault() - val3.GetValueOrDefault()) : ((Vector2?)null)).Value;
			oldTouchDistance = ((Vector2)(ref oldTouchVector)).magnitude;
		}
		else
		{
			Vector2 val5 = default(Vector2);
			((Vector2)(ref val5))._002Ector((float)_camera.pixelWidth, (float)_camera.pixelHeight);
			Vector2[] array4 = new Vector2[2];
			touch = Input.GetTouch(0);
			array4[0] = ((Touch)(ref touch)).position;
			touch = Input.GetTouch(1);
			array4[1] = ((Touch)(ref touch)).position;
			Vector2[] array5 = (Vector2[])(object)array4;
			Vector2 val6 = array5[0] - array5[1];
			float magnitude = ((Vector2)(ref val6)).magnitude;
			Transform transform3 = ((Component)this).transform;
			Vector3 position3 = transform3.position;
			Transform transform4 = ((Component)this).transform;
			Vector2? val7 = oldTouchPositions[0];
			Vector2? val8 = oldTouchPositions[1];
			Vector2? val = ((val7.HasValue & val8.HasValue) ? new Vector2?(val7.GetValueOrDefault() + val8.GetValueOrDefault()) : ((Vector2?)null));
			Vector2 val2 = val5;
			Vector2? val3 = (val.HasValue ? new Vector2?(val.GetValueOrDefault() - val2) : ((Vector2?)null));
			float orthographicSize = _camera.orthographicSize;
			Vector2? val4 = (val3.HasValue ? new Vector2?(val3.GetValueOrDefault() * orthographicSize) : ((Vector2?)null));
			float num = val5.y;
			transform3.position = position3 + transform4.TransformDirection(Vector2.op_Implicit((val4.HasValue ? new Vector2?(val4.GetValueOrDefault() / num) : ((Vector2?)null)).Value));
			_camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize * (oldTouchDistance / magnitude), 10f, orthographicSizeMax);
			Transform transform5 = ((Component)this).transform;
			transform5.position -= ((Component)this).transform.TransformDirection(Vector2.op_Implicit((array5[0] + array5[1] - val5) * _camera.orthographicSize / val5.y));
			oldTouchPositions[0] = array5[0];
			oldTouchPositions[1] = array5[1];
			oldTouchVector = val6;
			oldTouchDistance = magnitude;
		}
	}
}
