using System.Collections.Generic;
using UnityEngine;

public class WorldLawsCursedStars : MonoBehaviour
{
	private const int STARS_COUNT = 88;

	private const float ATTRACTION_SPEED_MIN = 0.05f;

	private const float ATTRACTION_SPEED_MAX = 0.3f;

	private const float ROTATION_SPEED_MIN = 0.05f;

	private const float ROTATION_SPEED_MAX = 0.5f;

	private const float RADIUS_MULTIPLIER = 1.25f;

	private const float MOUSE_AVOIDANCE_RADIUS = 40f;

	private const float MOUSE_AVOIDANCE_POWER = 15f;

	private const float ALPHA_FIX = 0.1f;

	private const float ALPHA_START = 8.4f;

	private static readonly Color OUTER_COLOR;

	private static readonly Color CENTER_COLOR;

	private float _attraction_speed;

	private float _rotation_speed;

	[SerializeField]
	private RectTransform _stars_parent;

	[SerializeField]
	private WorldLawsCursedStar _star_prefab;

	private float _angle;

	private Vector3 _center;

	private readonly List<WorldLawsCursedStar> _stars = new List<WorldLawsCursedStar>();

	private readonly List<float> _offset_indexes = new List<float>();

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_center = ((Transform)_stars_parent).localPosition;
		for (int i = 0; i < 88; i++)
		{
			WorldLawsCursedStar item = Object.Instantiate<WorldLawsCursedStar>(_star_prefab, (Transform)(object)_stars_parent);
			_stars.Add(item);
			_offset_indexes.Add(i);
		}
		updateStarsPositions();
	}

	private void OnEnable()
	{
		float curseProgressRatio = CursedSacrifice.getCurseProgressRatio();
		_rotation_speed = Mathf.Lerp(0.05f, 0.5f, curseProgressRatio);
		_attraction_speed = Mathf.Lerp(0.05f, 0.3f, curseProgressRatio);
	}

	private void Update()
	{
		updateStarsPositions();
	}

	private void updateStarsPositions()
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		if (_stars.Count == 0)
		{
			return;
		}
		float curseProgressRatio = CursedSacrifice.getCurseProgressRatio();
		_angle += _rotation_speed * Time.deltaTime;
		for (int i = 0; i < _stars.Count; i++)
		{
			WorldLawsCursedStar worldLawsCursedStar = _stars[i];
			Transform transform = ((Component)worldLawsCursedStar).transform;
			if (_offset_indexes[i] <= 0f)
			{
				_offset_indexes[i] += 87f;
				worldLawsCursedStar.toggleEgg(CursedSacrifice.isLatestWasEgg());
				worldLawsCursedStar.toggleFilled(Randy.randomChance(curseProgressRatio));
			}
			else
			{
				_offset_indexes[i] -= _attraction_speed;
			}
			if (worldLawsCursedStar.isFilled())
			{
				worldLawsCursedStar.setStarsTransparency(1f);
			}
			else
			{
				worldLawsCursedStar.setStarsTransparency(0f);
			}
			float num = _offset_indexes[i];
			float num2 = (float)i + _angle;
			Vector3 center = _center;
			center.x += Mathf.Cos(num2) * num * 1.25f;
			center.y += Mathf.Sin(num2) * num * 1.25f;
			transform.localPosition = center;
			mouseAvoidance(transform, num);
			float num3 = normalizedDistanceFromCenter(num);
			float num4 = Mathf.Lerp(0.5f, 1f, num3);
			transform.localScale = new Vector3(num4, num4);
			colorize(worldLawsCursedStar, num);
		}
	}

	private void mouseAvoidance(Transform pTransform, float pIndex)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_stars_parent, Vector2.op_Implicit(Input.mousePosition), (Camera)null, ref val);
		float num = Mathf.Min(Vector2.Distance(Vector2.op_Implicit(pTransform.localPosition), val), 40f);
		float num2 = 1f - num / 40f;
		float num3 = 15f * num2;
		Vector3 val2 = pTransform.localPosition - Vector2.op_Implicit(val);
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		float num4 = Mathf.Max(normalizedDistanceFromCenter(pIndex), 0.2f);
		pTransform.localPosition += new Vector3(num3 * normalized.x, num3 * normalized.y) * num4;
	}

	private void colorize(WorldLawsCursedStar pStar, float pIndex)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		float num = normalizedDistanceFromCenter(pIndex);
		Color pColor = Toolbox.blendColor(OUTER_COLOR, CENTER_COLOR, num * 1.35f);
		float pValue = 8.4f - (pIndex + 1f) / 8.8f;
		pStar.setColorMultiplyAlphaBoth(pColor, pValue);
	}

	private float normalizedDistanceFromCenter(float pIndex)
	{
		return (pIndex + 1f) / 88f;
	}

	static WorldLawsCursedStars()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		OUTER_COLOR = Toolbox.makeColor("#FFAA00");
		CENTER_COLOR = Toolbox.makeColor("#8B00FF");
	}
}
