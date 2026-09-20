using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class LocalizedTextPrice : MonoBehaviour
{
	public static string price_current = "???";

	public static string price_old = string.Empty;

	public static string discount = string.Empty;

	public Text text_old_price;

	public Text text_current_price;

	public GameObject discount_bg;

	public Text text_percent;

	private const string IN_APP_ID = "premium";

	internal void updateText(bool pCheckText = true)
	{
		if (!string.IsNullOrEmpty(discount))
		{
			showDiscount(discount);
		}
		string text = "";
		if ((Object)(object)InAppManager.instance != (Object)null)
		{
			InAppManager instance = InAppManager.instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				IStoreController controller = instance.controller;
				obj = ((controller != null) ? controller.products : null);
			}
			if (obj != null)
			{
				Product val = InAppManager.instance.controller.products.WithID("premium");
				if (val != null)
				{
					text = val.metadata.localizedPriceString;
				}
				goto IL_007a;
			}
		}
		text = price_current;
		goto IL_007a;
		IL_007a:
		text_current_price.text = text;
		if (!string.IsNullOrEmpty(price_old))
		{
			text_old_price.text = price_old;
			((Component)text_old_price).gameObject.SetActive(true);
		}
	}

	private void showDiscount(string pString)
	{
		text_percent.text = pString;
		discount_bg.gameObject.SetActive(true);
	}

	private void setDefault()
	{
		discount_bg.gameObject.SetActive(false);
		((Component)text_current_price).gameObject.SetActive(true);
		text_current_price.text = "??";
		((Component)text_old_price).gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		setDefault();
		updateText();
	}
}
