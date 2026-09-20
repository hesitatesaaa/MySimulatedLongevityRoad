using System;
using Newtonsoft.Json;
using Proyecto26;
using UnityEngine;
using UnityEngine.Purchasing;

internal static class Discounts
{
	private static ProductMetadata localPriceData;

	private static string platform;

	internal static void checkDiscounts()
	{
		try
		{
			checkPlatform();
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
						discountRequest(val.metadata);
					}
					else
					{
						Debug.Log((object)"DC:no req/prod");
					}
					return;
				}
			}
			Debug.Log((object)"DC:np");
		}
		catch (Exception ex)
		{
			Debug.Log((object)"DC:err");
			Debug.Log((object)ex);
		}
	}

	private static void discountRequest(ProductMetadata pProductMeta)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		if (platform.Length < 2 || pProductMeta == null)
		{
			return;
		}
		string text = "https://currency.superworldbox.com/discounts/" + platform + ".json?" + Toolbox.cacheBuster();
		string text2 = JsonConvert.SerializeObject((object)pProductMeta, new JsonSerializerSettings
		{
			DefaultValueHandling = (DefaultValueHandling)3
		});
		if (string.IsNullOrEmpty(text2) || text2 == "{}")
		{
			return;
		}
		RestClient.Post(text, text2).Then((Action<ResponseHelper>)delegate(ResponseHelper response)
		{
			string text3 = response.Text;
			if (!string.IsNullOrEmpty(text3) && !(text3 == "{}") && !(text3.Substring(0, 1) != "{"))
			{
				Debug.Log((object)text3);
				DiscountData discountData = JsonConvert.DeserializeObject<DiscountData>(text3);
				Debug.Log((object)"DS:Setting");
				if (!string.IsNullOrEmpty(discountData.discount) && !string.IsNullOrEmpty(discountData.price_current) && !string.IsNullOrEmpty(discountData.price_old))
				{
					LocalizedTextPrice.discount = discountData.discount;
					LocalizedTextPrice.price_current = discountData.price_current;
					LocalizedTextPrice.price_old = discountData.price_old;
					Debug.Log((object)"DS:Set");
				}
				else
				{
					Debug.Log((object)"DS:NSet");
				}
			}
		}).Catch((Action<Exception>)delegate(Exception err)
		{
			Debug.Log((object)"DS:err");
			Debug.Log((object)err.Message);
		});
	}

	private static void checkPlatform()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected I4, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		RuntimePlatform val = Application.platform;
		if ((int)val <= 11)
		{
			switch ((int)val)
			{
			default:
				if ((int)val != 11)
				{
					break;
				}
				platform = "android";
				return;
			case 2:
				platform = "pc";
				return;
			case 7:
				platform = "pc";
				return;
			case 0:
				platform = "mac";
				return;
			case 1:
				platform = "mac";
				return;
			case 8:
				platform = "ios";
				return;
			case 3:
			case 4:
			case 5:
			case 6:
				break;
			}
		}
		else
		{
			if ((int)val == 13)
			{
				platform = "linux";
				return;
			}
			if ((int)val == 16)
			{
				platform = "linux";
				return;
			}
		}
		platform = "unknown";
	}
}
