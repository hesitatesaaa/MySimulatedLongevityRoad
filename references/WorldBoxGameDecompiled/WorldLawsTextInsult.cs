using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class WorldLawsTextInsult : MonoBehaviour
{
	[SerializeField]
	private Transform _follow_object;

	[SerializeField]
	private RectTransform _size_parent;

	[SerializeField]
	private Text _text;

	private static float _global_wait_timeout;

	private const float RARE_INSULT_CHANCE = 0.005f;

	private float _wait_time;

	private Tweener _text_tweener;

	private string[] _insults_rare = new string[14]
	{
		"UPDATE?", "WHEN", "GEB", "BRE", "REBR", "MODERN?", "HELP", "CAKE", "BRURSE", "MAXIM",
		"MASTEF", "HUGO", "NIKON", "JECO"
	};

	public static void removeInsultTimeout()
	{
		_global_wait_timeout = 0f;
	}

	public void Update()
	{
		if (shouldInsultNow())
		{
			if (_wait_time > 0f)
			{
				_wait_time -= Time.deltaTime;
			}
			else if (_global_wait_timeout > 0f && !isTweening())
			{
				_global_wait_timeout -= Time.deltaTime;
			}
			else if (!isTweening())
			{
				startNewTween();
			}
		}
	}

	private void startNewTween()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		killTweens();
		if (WorldLawLibrary.world_law_cursed_world.isEnabled())
		{
			_global_wait_timeout = 0.6f + Randy.randomFloat(0f, 2f);
		}
		else
		{
			_global_wait_timeout = 2f + Randy.randomFloat(0f, 3f);
		}
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(0f, 0f, Randy.randomFloat(-30f, 30f));
		((Transform)_size_parent).localRotation = Quaternion.Euler(val);
		_text.text = getInsultText();
		((Component)_text).transform.position = _follow_object.position + new Vector3(0f, (float)Randy.randomInt(8, 12), 0f);
		_text.fontSize = Randy.randomInt(7, 9);
		Vector3 val2 = ((Component)_text).transform.position + new Vector3(0f, Randy.randomFloat(30f, 60f), 0f);
		_text_tweener = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(((Component)_text).transform, val2, 6f, false), (Ease)9);
		((Tween)DOTweenModuleUI.DOColor(_text, Color.white, 2f)).onComplete = new TweenCallback(doTextFade);
	}

	private string getInsultText()
	{
		if (Randy.randomChance(0.005f))
		{
			return _insults_rare.GetRandom();
		}
		return InsultStringGenerator.getRandomText();
	}

	private void doTextFade()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		((Tween)DOTweenModuleUI.DOFade(_text, 0f, 2f)).onComplete = new TweenCallback(doWait);
	}

	private bool shouldInsultNow()
	{
		if (!CursedSacrifice.isWorldReadyForCURSE())
		{
			return WorldLawLibrary.world_law_cursed_world.isEnabled();
		}
		return true;
	}

	private bool isTweening()
	{
		return TweenExtensions.IsActive((Tween)(object)_text_tweener);
	}

	private void OnEnable()
	{
		doWait();
	}

	private void doWait()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		killTweens();
		_wait_time = Randy.randomFloat(0f, 7f);
		((Graphic)_text).color = Toolbox.color_white_transparent;
	}

	private void killTweens()
	{
		Tweener text_tweener = _text_tweener;
		if (text_tweener != null)
		{
			TweenExtensions.Kill((Tween)(object)text_tweener, false);
		}
		ShortcutExtensions.DOKill((Component)(object)_text, false);
	}
}
