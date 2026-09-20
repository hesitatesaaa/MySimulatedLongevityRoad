using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UiCreature : MonoBehaviour
{
	public bool doFall;

	public bool doRotate;

	public bool doScale;

	public bool doFly;

	public bool doPlayPunch;

	public bool changeParent;

	public string doSfx;

	private Tweener tweener_scale;

	private Tweener tweener_rotation;

	private Tweener tweener_move;

	private Vector3 _init_scale;

	internal bool dropped;

	public string achievement;

	private Vector3 _initial_pos;

	private Quaternion _initial_rotation;

	private Transform _original_parent;

	private bool _forced_complete;

	private void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		_original_parent = ((Component)this).transform.parent;
		_init_scale = ((Component)this).transform.localScale;
		_initial_pos = ((Component)this).transform.localPosition;
		_initial_rotation = ((Component)this).transform.rotation;
	}

	private void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!((Component)this).gameObject.HasComponent<Button>())
		{
			((UnityEvent)((Component)this).gameObject.AddComponent<Button>().onClick).AddListener(new UnityAction(click));
		}
	}

	private void killTweens(bool pComplete = false)
	{
		if (pComplete)
		{
			_forced_complete = true;
		}
		TweenExtensions.Kill((Tween)(object)tweener_scale, pComplete);
		TweenExtensions.Kill((Tween)(object)tweener_rotation, pComplete);
		TweenExtensions.Kill((Tween)(object)tweener_move, pComplete);
		_forced_complete = false;
	}

	internal void resetPosition()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		killTweens();
		dropped = false;
		((Component)this).transform.rotation = _initial_rotation;
		((Component)this).transform.localPosition = _initial_pos;
		((Component)this).transform.localScale = _init_scale;
		((Component)this).gameObject.SetActive(true);
	}

	public void click()
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		if (dropped)
		{
			return;
		}
		killTweens();
		if (((Component)(object)this).HasComponent<HoveringIcon>())
		{
			((Component)this).GetComponent<HoveringIcon>().clear();
		}
		if (((Component)(object)this).HasComponent<LivingIcon>())
		{
			((Component)this).GetComponent<LivingIcon>().kill();
		}
		if (!string.IsNullOrEmpty(achievement))
		{
			AchievementLibrary.unlock(achievement);
		}
		if (doPlayPunch)
		{
			MusicBox.playSound("event:/SFX/OTHER/Punch");
		}
		if (doSfx != "none" && !string.IsNullOrEmpty(doSfx) && doSfx.Contains("event:"))
		{
			MusicBox.playSound(doSfx);
		}
		if (doScale)
		{
			Vector3 localScale = _init_scale * 1.2f;
			((Component)this).transform.localScale = localScale;
			tweener_scale = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, _init_scale, 0.3f), (Ease)27);
		}
		if (doFall)
		{
			fall();
		}
		if (doFly)
		{
			flyAway();
		}
		if (doRotate)
		{
			if (Randy.randomBool())
			{
				tweener_rotation = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DORotate(((Component)this).transform, new Vector3(0f, 0f, Randy.randomFloat(90f, 180f)), 1f, (RotateMode)0), (Ease)9);
			}
			else
			{
				tweener_rotation = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DORotate(((Component)this).transform, new Vector3(0f, 0f, Randy.randomFloat(-180f, -90f)), 1f, (RotateMode)0), (Ease)9);
			}
		}
	}

	private void flyAway()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		dropped = true;
		if (changeParent)
		{
			((Component)this).transform.parent = ((Component)CanvasMain.instance.canvas_tooltip).transform;
		}
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(((Component)this).transform.position.x + Randy.randomFloat(-200f, 200f), 1000f, 0f);
		tweener_move = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(((Component)this).transform, val, 0.6f, false), (Ease)5), new TweenCallback(completeFly));
	}

	private void fall()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		dropped = true;
		if (changeParent)
		{
			((Component)this).transform.SetParent(((Component)CanvasMain.instance.canvas_tooltip).transform);
		}
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(((Component)this).transform.position.x + Randy.randomFloat(-4f, 4f), ((Component)this).transform.position.y - (float)Screen.height, 0f);
		tweener_move = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(((Component)this).transform, val, 0.6f, false), (Ease)5), new TweenCallback(completeFall));
	}

	private void completeFly()
	{
		((Component)this).transform.SetParent(_original_parent);
		((Component)this).gameObject.SetActive(false);
	}

	private void completeFall()
	{
		if (_forced_complete)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		((Component)this).transform.SetParent(_original_parent);
		MusicBox.playSound("event:/SFX/HIT/HitStone");
		((Component)this).gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		resetPosition();
	}

	private void OnDisable()
	{
		killTweens(pComplete: true);
	}

	public UiCreature()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		doScale = true;
		changeParent = true;
		doSfx = "none";
		_init_scale = Vector3.one;
		achievement = "";
		((MonoBehaviour)this)._002Ector();
	}
}
