using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonResource : MonoBehaviour
{
	public Text textAmount;

	public ResourceAsset asset;

	public static float scaleTime = 0.1f;

	private void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		Button component = ((Component)this).GetComponent<Button>();
		((UnityEvent)component.onClick).AddListener(new UnityAction(showTooltip));
		component.OnHover(new UnityAction(showHoverTooltip));
		component.OnHoverOut(new UnityAction(Tooltip.hideTooltip));
	}

	internal void load(ResourceAsset pAsset, int pAmount)
	{
		asset = pAsset;
		if (asset != null)
		{
			((Component)this).GetComponent<Image>().sprite = pAsset.getSpriteIcon();
			textAmount.text = pAmount.ToString() ?? "";
		}
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
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		string tooltip = asset.tooltip;
		Tooltip.show(this, tooltip, new TooltipData
		{
			resource = asset
		});
		((Component)this).transform.localScale = new Vector3(1f, 1f, 1f);
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, 0.8f, scaleTime), (Ease)26);
	}

	private void OnDestroy()
	{
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
	}
}
