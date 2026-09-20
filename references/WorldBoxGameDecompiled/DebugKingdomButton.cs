using UnityEngine;
using UnityEngine.UI;

public class DebugKingdomButton : MonoBehaviour
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private Image _image;

	internal KingdomAsset kingdom_asset;

	[SerializeField]
	private Image _discrepancy_bad;

	[SerializeField]
	private Image _discrepancy_have;

	[SerializeField]
	private Image _discrepancy_normal;

	public Image image => _image;

	public void setAsset(KingdomAsset pAsset)
	{
		kingdom_asset = pAsset;
		_image.sprite = kingdom_asset.getSprite();
		setupTooltip();
		if (kingdom_asset.assets_discrepancies_bad != null)
		{
			((Component)_discrepancy_have).gameObject.SetActive(true);
		}
		else
		{
			((Component)_discrepancy_have).gameObject.SetActive(false);
		}
	}

	public void checkSelected(KingdomAsset pAssetMain)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		((Component)_discrepancy_bad).gameObject.SetActive(false);
		((Component)_discrepancy_normal).gameObject.SetActive(false);
		if (kingdom_asset == pAssetMain)
		{
			((Graphic)image).color = Color.white;
			return;
		}
		if (kingdom_asset.assets_discrepancies != null && kingdom_asset.assets_discrepancies.Contains(pAssetMain.id))
		{
			((Component)_discrepancy_normal).gameObject.SetActive(true);
		}
		if (pAssetMain.assets_discrepancies_bad != null && pAssetMain.assets_discrepancies_bad.Contains(kingdom_asset.id))
		{
			((Component)_discrepancy_bad).gameObject.SetActive(true);
		}
		if (pAssetMain.isFoe(kingdom_asset))
		{
			((Graphic)image).color = new Color(0.2f, 0.2f, 0.2f);
		}
		else
		{
			((Graphic)image).color = Color.white;
		}
	}

	public void setupTooltip()
	{
		TipButton tipButton = default(TipButton);
		if (((Component)this).TryGetComponent<TipButton>(ref tipButton))
		{
			tipButton.hoverAction = showTooltip;
		}
	}

	private void showTooltip()
	{
		Tooltip.show(((Component)this).gameObject, "debug_kingdom_assets", new TooltipData
		{
			kingdom_asset = kingdom_asset
		});
	}

	public static void getTooltipDescription(KingdomAsset pAsset, out string pDescription, out string pDescription2)
	{
		pDescription = string.Empty;
		pDescription2 = string.Empty;
		if (pAsset.list_tags.Count > 0)
		{
			pDescription += "--- OWN TAGS ---\n".ColorHex(ColorStyleLibrary.m.color_text_grey_dark);
			foreach (string list_tag in pAsset.list_tags)
			{
				pDescription += (list_tag + "\n").ColorHex(ColorStyleLibrary.m.color_text_grey);
			}
		}
		if (pAsset.friendly_tags.Count > 0)
		{
			pDescription += "--- FRIENDLY TAGS ---\n".ColorHex(ColorStyleLibrary.m.color_text_grey_dark);
			foreach (string friendly_tag in pAsset.friendly_tags)
			{
				pDescription += (friendly_tag + "\n").ColorHex("#43FF43");
			}
		}
		if (pAsset.enemy_tags.Count > 0)
		{
			pDescription += "#--- ENEMY TAGS ---\n".ColorHex(ColorStyleLibrary.m.color_text_grey_dark);
			foreach (string enemy_tag in pAsset.enemy_tags)
			{
				pDescription += (enemy_tag + "\n").ColorHex("#FB2C21");
			}
		}
		if (pAsset.assets_discrepancies == null || pAsset.assets_discrepancies.Count <= 0)
		{
			return;
		}
		pDescription2 = $"!! Discrepancies {pAsset.assets_discrepancies.Count}!!\n".ColorHex("#D85BC5");
		int num = 0;
		foreach (string assets_discrepancy in pAsset.assets_discrepancies)
		{
			if (assets_discrepancy.Contains(pAsset.id) || pAsset.id.Contains(assets_discrepancy))
			{
				pDescription2 += assets_discrepancy.ColorHex("#FB2C21");
			}
			else
			{
				pDescription2 += assets_discrepancy;
			}
			if (pDescription2.Length > 150)
			{
				int num2 = pAsset.assets_discrepancies.Count - num;
				pDescription2 += $" and {num2} more...!!!".ColorHex("#8CFF99");
				break;
			}
			if (num < pAsset.assets_discrepancies.Count - 1)
			{
				pDescription2 += ", ";
			}
			num++;
		}
	}
}
