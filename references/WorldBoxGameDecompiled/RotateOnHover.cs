using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RotateOnHover : MonoBehaviour
{
	private const float TILT_ANGLE = 60f;

	private const float TILT_SPEED = 15f;

	public List<Shadow> shadows;

	private bool _is_hovering;

	private Vector3 _original_rotation;

	private Vector2 _original_pivot;

	private List<Vector2> _original_shadows;

	private RectTransform _rect_transform;

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		_rect_transform = ((Component)this).GetComponent<RectTransform>();
		_original_rotation = ((Transform)_rect_transform).eulerAngles;
		_original_pivot = _rect_transform.pivot;
		_original_shadows = new List<Vector2>();
		foreach (Shadow shadow in shadows)
		{
			_original_shadows.Add(shadow.effectDistance);
		}
	}

	private void Start()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		Button componentInParent = ((Component)this).GetComponentInParent<Button>();
		if (!((Object)(object)componentInParent == (Object)null))
		{
			componentInParent.OnHover((UnityAction)delegate
			{
				_is_hovering = true;
			});
			componentInParent.OnHoverOut((UnityAction)delegate
			{
				_is_hovering = false;
			});
		}
	}

	private void OnEnable()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		_is_hovering = false;
		((Transform)_rect_transform).eulerAngles = _original_rotation;
		_rect_transform.pivot = _original_pivot;
		for (int i = 0; i < shadows.Count; i++)
		{
			shadows[i].effectDistance = _original_shadows[i];
		}
	}

	private void OnDisable()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_is_hovering = false;
		((Transform)_rect_transform).eulerAngles = Vector3.zero;
	}

	private void Update()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		if (!_is_hovering)
		{
			float num = 15f * Time.deltaTime * 0.5f;
			_rect_transform.pivot = Vector2.Lerp(_rect_transform.pivot, _original_pivot, num);
			for (int i = 0; i < shadows.Count; i++)
			{
				shadows[i].effectDistance = Vector2.Lerp(shadows[i].effectDistance, _original_shadows[i], num);
			}
			((Transform)_rect_transform).rotation = Quaternion.Lerp(((Transform)_rect_transform).rotation, Quaternion.Euler(_original_rotation), num);
			return;
		}
		Vector2 val = Vector2.op_Implicit(((Transform)_rect_transform).InverseTransformPoint(Input.mousePosition));
		float x = val.x;
		Rect rect = _rect_transform.rect;
		float num2 = Mathf.Clamp(x / ((Rect)(ref rect)).width, -0.5f, 0.5f);
		float y = val.y;
		rect = _rect_transform.rect;
		float num3 = Mathf.Clamp(y / ((Rect)(ref rect)).height, -0.5f, 0.5f);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(_original_rotation.x - num3 * 60f, _original_rotation.y + num2 * 60f, _original_rotation.z);
		Vector2 effectDistance = default(Vector2);
		((Vector2)(ref effectDistance))._002Ector(num2 * 4f, num3 * 4f);
		foreach (Shadow shadow in shadows)
		{
			shadow.effectDistance = effectDistance;
		}
		_rect_transform.pivot = new Vector2(_original_pivot.x - num2 * 0.1f, _original_pivot.y - num3 * 0.1f);
		((Transform)_rect_transform).rotation = Quaternion.Lerp(((Transform)_rect_transform).rotation, Quaternion.Euler(val2), 15f * Time.deltaTime);
	}
}
