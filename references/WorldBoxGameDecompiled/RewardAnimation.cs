using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class RewardAnimation : MonoBehaviour
{
	public Image boxSprite;

	public GameObject rewardTexts;

	public Text Text_free_power_unlocked;

	public Text Text_free_power_tap_to_unlock;

	private IconRotationAnimation _rotation_animation;

	public GameObject rewardedPowerIcon;

	private SpriteAnimation _sprite_animation;

	internal RewardAnimationState state;

	public LocalizedText bottomButtonText;

	private Vector3 _original_pos;

	public bool quickReward;

	private Tweener _icon_move_tween;

	private Tweener _icon_scale_tween;

	private Tweener _text_tween;

	public float rewardedPowerScaleTime;

	public float moveTime1;

	public float moveTime2;

	public float moveTime3;

	public float moveTime4;

	private Transform _icon_transform;

	private Transform _text_transform;

	private void Awake()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		_icon_transform = rewardedPowerIcon.transform;
		_text_transform = rewardTexts.transform;
		_rotation_animation = ((Component)boxSprite).GetComponent<IconRotationAnimation>();
		_sprite_animation = ((Component)boxSprite).GetComponent<SpriteAnimation>();
		_sprite_animation.Awake();
		if (_original_pos == Vector3.zero)
		{
			_original_pos = _icon_transform.localPosition;
		}
	}

	public void OnEnable()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (_original_pos == Vector3.zero)
		{
			_original_pos = _icon_transform.localPosition;
		}
		bottomButtonText.key = "free_power_button_open_in";
		bottomButtonText.updateText();
		resetAnim();
	}

	private void OnDisable()
	{
		TweenExtensions.Kill((Tween)(object)_icon_scale_tween, false);
		TweenExtensions.Kill((Tween)(object)_icon_move_tween, false);
		TweenExtensions.Kill((Tween)(object)_text_tween, false);
	}

	public void resetAnim()
	{
		state = RewardAnimationState.Idle;
		_sprite_animation.resetAnim(3);
		((Behaviour)_rotation_animation).enabled = true;
		ShortcutExtensions.DOKill((Component)(object)_icon_transform, false);
		rewardedPowerIcon.SetActive(false);
		rewardTexts.SetActive(false);
		((Component)Text_free_power_unlocked).gameObject.SetActive(false);
		((Component)Text_free_power_tap_to_unlock).gameObject.SetActive(true);
	}

	private void Update()
	{
		if (quickReward && _sprite_animation.currentFrameIndex < 7)
		{
			_sprite_animation.currentFrameIndex = 7;
			showRewards(pStart: false);
			moveStageThree();
		}
		if (state == RewardAnimationState.Play || state == RewardAnimationState.Open)
		{
			_sprite_animation.update(Time.deltaTime);
			if (_sprite_animation.currentFrameIndex > 6 && state != RewardAnimationState.Open)
			{
				showRewards();
			}
		}
	}

	private void showRewards(bool pStart = true)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Expected O, but got Unknown
		state = RewardAnimationState.Open;
		rewardedPowerIcon.SetActive(true);
		TweenExtensions.Kill((Tween)(object)_text_tween, false);
		_text_transform.localScale = new Vector3(0.5f, 0.5f);
		_text_tween = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(_text_transform, new Vector3(1f, 1f, 1f), 0.3f), (Ease)27);
		rewardTexts.gameObject.SetActive(true);
		((Component)Text_free_power_unlocked).gameObject.SetActive(true);
		((Component)Text_free_power_tap_to_unlock).gameObject.SetActive(false);
		bottomButtonText.key = "get_it";
		bottomButtonText.updateText();
		ShortcutExtensions.DOKill((Component)(object)_icon_transform, false);
		_icon_transform.localPosition = _original_pos;
		_icon_transform.localScale = new Vector3(0.02f, 0.1f, 1f);
		if (pStart)
		{
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(_original_pos.x, _original_pos.y, 0f);
			val.y += 22f;
			_icon_move_tween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(_icon_transform, val, moveTime1, false), (Ease)21), new TweenCallback(moveStageTwo));
		}
		_icon_scale_tween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(_icon_transform, new Vector3(0.75f, 0.75f, 1f), rewardedPowerScaleTime), (Ease)32), new TweenCallback(scaleStageTwo));
	}

	private void moveStageTwo()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		TweenExtensions.Kill((Tween)(object)_icon_move_tween, false);
		_icon_move_tween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(_icon_transform, _original_pos, moveTime2, false), (Ease)7), new TweenCallback(moveStageThree));
	}

	private void moveStageThree()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		TweenExtensions.Kill((Tween)(object)_icon_move_tween, false);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(_original_pos.x, _original_pos.y, 1f);
		val.y += 3f;
		_icon_move_tween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(_icon_transform, val, moveTime3, false), (Ease)7), new TweenCallback(moveStageFour));
	}

	private void moveStageFour()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		TweenExtensions.Kill((Tween)(object)_icon_move_tween, false);
		_icon_move_tween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(_icon_transform, _original_pos, moveTime4, false), (Ease)7), new TweenCallback(moveStageThree));
	}

	private void scaleStageTwo()
	{
	}

	public void clickAnimation()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (_sprite_animation.currentFrameIndex <= 5)
		{
			_sprite_animation.resetAnim();
			((Behaviour)_rotation_animation).enabled = false;
			((Component)_rotation_animation).transform.localScale = new Vector3(1f, 1f, 1f);
			if (state != RewardAnimationState.Idle)
			{
				resetAnim();
			}
			state = RewardAnimationState.Play;
		}
	}

	public RewardAnimation()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_original_pos = Vector3.zero;
		rewardedPowerScaleTime = 0.45f;
		moveTime1 = 0.25f;
		moveTime2 = 0.25f;
		moveTime3 = 1.5f;
		moveTime4 = 1.5f;
		((MonoBehaviour)this)._002Ector();
	}
}
