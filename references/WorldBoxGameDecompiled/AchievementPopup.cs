using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPopup : MonoBehaviour
{
	private static AchievementPopup _instance;

	[SerializeField]
	private Image _icon_left;

	[SerializeField]
	private Image _icon_right;

	[SerializeField]
	private Text _popup_text;

	[SerializeField]
	private Text _popup_description;

	[SerializeField]
	private AchievementGoodie _goodie_prefab;

	[SerializeField]
	private Transform _goodies_parent;

	private ObjectPoolGenericMono<AchievementGoodie> _goodie_pool;

	private Tweener _tween;

	private void Awake()
	{
		_instance = this;
		hide();
	}

	internal static void show(string pAchievementID)
	{
		_instance.showByID(pAchievementID);
	}

	internal static void show(Achievement pAchievement)
	{
		_instance.showByID(pAchievement.id);
	}

	private void Update()
	{
		World.world.spawnCongratulationFireworks();
	}

	internal void showByID(string pAchievementID)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		if (_tween != null && ((Tween)_tween).active)
		{
			return;
		}
		((Component)this).gameObject.SetActive(true);
		checkPool();
		Achievement achievement = AssetManager.achievements.get(pAchievementID);
		Sprite icon = achievement.getIcon();
		if ((Object)(object)icon != (Object)null)
		{
			_icon_left.sprite = icon;
			_icon_right.sprite = icon;
		}
		((Component)_popup_text).GetComponent<LocalizedText>().setKeyAndUpdate(achievement.getLocaleID());
		((Component)_popup_description).GetComponent<LocalizedText>().setKeyAndUpdate(achievement.getDescriptionID());
		float num = Screen.height;
		Rect safeArea = Screen.safeArea;
		float num2 = (num - ((Rect)(ref safeArea)).height) / CanvasMain.instance.canvas_ui.scaleFactor;
		_tween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(((Component)this).transform, 0f - num2, 1f, false), (Ease)27), 0.2f), new TweenCallback(tweenHide));
		if (!achievement.unlocks_something)
		{
			return;
		}
		foreach (BaseUnlockableAsset unlock_asset in achievement.unlock_assets)
		{
			if (unlock_asset.show_for_unlockables_ui)
			{
				_goodie_pool.getNext().load(unlock_asset, pUnlocked: true);
			}
		}
	}

	public void forceHide()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		if (_tween != null)
		{
			TweenExtensions.Kill((Tween)(object)_tween, false);
		}
		_tween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(((Component)this).transform, 200f, 0.5f, false), (Ease)27), new TweenCallback(hide));
	}

	private void tweenHide()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		_tween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(((Component)this).transform, 200f, 1f, false), 4f), (Ease)27), new TweenCallback(hide));
	}

	private void hide()
	{
		((Component)this).gameObject.SetActive(false);
		_goodie_pool?.clear();
	}

	private void checkPool()
	{
		if (_goodie_pool == null)
		{
			_goodie_pool = new ObjectPoolGenericMono<AchievementGoodie>(_goodie_prefab, _goodies_parent);
		}
	}
}
