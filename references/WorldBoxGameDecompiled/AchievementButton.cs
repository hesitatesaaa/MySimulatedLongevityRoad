using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AchievementButton : MonoBehaviour
{
	private Achievement _achievement;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _background_completed;

	[SerializeField]
	private Image _background_legendary;

	[SerializeField]
	private GameObject _background_default;

	[SerializeField]
	private GameObject _icon_medal;

	public void Load(Achievement pAchievement)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		_achievement = pAchievement;
		Sprite icon = _achievement.getIcon();
		if ((Object)(object)icon != (Object)null)
		{
			_icon.sprite = icon;
			if (!AchievementLibrary.isUnlocked(_achievement))
			{
				((Graphic)_icon).color = Color.black;
				_background_default.SetActive(true);
				((Behaviour)((Component)_background_completed).GetComponent<Image>()).enabled = false;
				_icon_medal.SetActive(false);
			}
		}
		if (pAchievement.unlocks_something)
		{
			((Component)_background_legendary).gameObject.SetActive(true);
		}
		else
		{
			((Component)_background_legendary).gameObject.SetActive(false);
		}
		((Object)this).name = _achievement.id;
	}

	private void Start()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		((Component)this).transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
		Button component = ((Component)this).GetComponent<Button>();
		((UnityEvent)component.onClick).AddListener(new UnityAction(showTooltip));
		component.OnHover(new UnityAction(showHoverTooltip));
		component.OnHoverOut(new UnityAction(Tooltip.hideTooltip));
	}

	private void showHoverTooltip()
	{
		if (Config.tooltips_active)
		{
			showTooltip();
		}
	}

	private void showTooltip()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Tooltip.show(this, "achievement", new TooltipData
		{
			achievement = _achievement
		});
		((Component)this).transform.localScale = new Vector3(1f, 1f, 1f);
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, 0.8f, 0.1f), (Ease)26);
	}
}
