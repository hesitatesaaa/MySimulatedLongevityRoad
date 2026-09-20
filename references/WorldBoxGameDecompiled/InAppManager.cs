using System;
using System.Runtime.CompilerServices;
using Beebyte.Obfuscator;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

[ObfuscateLiterals]
public class InAppManager : MonoBehaviour, IDetailedStoreListener, IStoreListener
{
	public static InAppManager instance;

	private static bool _initialized = false;

	private IAppleExtensions _apple;

	private IGooglePlayStoreExtensions _googleplay;

	internal IStoreController controller;

	private IExtensionProvider extensions;

	internal static CrossPlatformValidator validator;

	public static bool last_availableToPurchase;

	public static string last_transactionID;

	public static bool last_hasReceipt;

	public static bool last_tValidPurchase;

	public static bool last_tPurchasePending;

	internal static bool googleAccount = true;

	private static ConfigurationBuilder _builder;

	public static bool restore_ui_buffering;

	public static string restore_message;

	public static string validator_message;

	private IAppleExtensions apple
	{
		get
		{
			if (_apple == null)
			{
				_apple = extensions.GetExtension<IAppleExtensions>();
			}
			return _apple;
		}
	}

	private IGooglePlayStoreExtensions googleplay
	{
		get
		{
			if (_googleplay == null)
			{
				_googleplay = extensions.GetExtension<IGooglePlayStoreExtensions>();
			}
			return _googleplay;
		}
	}

	private void Start()
	{
		Debug.Log((object)"InAppManager::Start");
		if ((Object)(object)instance != (Object)null)
		{
			Debug.LogError((object)"Multiple in-app managers have been instantiated.");
			return;
		}
		instance = this;
		if (PlayerConfig.instance == null || PlayerConfig.instance.data.pPossible0507)
		{
			activatePrem();
			Debug.Log((object)"InAppManager::End");
		}
	}

	private static bool checkGoogleAccount()
	{
		if (!googleAccount)
		{
			Debug.Log((object)"google account missing");
			ErrorWindow.errorMessage = "A Google Account is missing or you're not logged in with one.";
			ScrollWindow.get("error_with_reason").clickShow();
		}
		return googleAccount;
	}

	private static void debugPremium()
	{
	}

	private void InitializePurchasing(int pWhere = -1)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		Debug.Log((object)$"InitializePurchasing {pWhere}");
		if (!IsInitialized() && !_initialized)
		{
			_initialized = true;
			instance = this;
			_builder = ConfigurationBuilder.Instance((IPurchasingModule)(object)StandardPurchasingModule.Instance(), Array.Empty<IPurchasingModule>());
			_builder.logUnavailableProducts = true;
			ConfigurationBuilder builder = _builder;
			IDs val = new IDs();
			val.Add("premium", new string[1] { "GooglePlay" });
			val.Add("premium", new string[1] { "AppleAppStore" });
			builder.AddProduct("premium", (ProductType)1, val);
			try
			{
				validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
				Debug.Log((object)"validator assigned");
				validator_message = null;
			}
			catch (NotImplementedException ex)
			{
				Debug.Log((object)"validator not assigned");
				Debug.LogError((object)("Cross Platform Validator Not Implemented: " + ex.Message));
				validator_message = "Validator not implemented";
			}
			catch (Exception ex2)
			{
				Debug.Log((object)"validator not assigned");
				Debug.LogError((object)("Validator Exception: " + ex2.Message));
				validator_message = ex2.Message;
			}
			UnityPurchasing.Initialize((IDetailedStoreListener)(object)this, _builder);
		}
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		this.controller = controller;
		this.extensions = extensions;
		apple.RegisterPurchaseDeferredListener((Action<Product>)OnAskToBuy);
		checkPremium();
		checkWorldnet();
		Discounts.checkDiscounts();
	}

	public void checkPremium()
	{
		bool flag = true;
		bool flag2 = false;
		Product val = controller.products.WithID("premium");
		Debug.Log((object)"[cp]");
		Debug.Log((object)("[avl] " + val.availableToPurchase));
		Debug.Log((object)("[txi] " + val.transactionID));
		Debug.Log((object)("[hrc] " + val.hasReceipt));
		last_tValidPurchase = flag;
		last_tPurchasePending = flag2;
		Config.lockGameControls = false;
		if (!flag & flag2)
		{
			Debug.Log((object)"[nvp] pp");
			return;
		}
		Debug.Log((object)("[vp] 3 " + flag));
		Debug.Log((object)("[hr] " + val.hasReceipt));
		if ((flag && val.hasReceipt) || PlayerConfig.instance.data.premium)
		{
			Debug.Log((object)"[phr]");
			activatePrem();
		}
		else
		{
			Debug.Log((object)("[vp] 4 " + flag));
			Debug.Log((object)("[hr] " + val.hasReceipt));
			Debug.Log((object)("[hp] " + PlayerConfig.instance.data.premium));
		}
		Debug.Log((object)"[cpd]");
	}

	public void checkWorldnet()
	{
	}

	public static void consumePremium()
	{
	}

	private void OnAskToBuy(Product item)
	{
		Debug.Log((object)("Purchase deferred: " + item.definition.id));
		Config.lockGameControls = false;
	}

	public string getDebugInfo()
	{
		Product val = controller.products.WithID("premium");
		return string.Concat(string.Concat("" + "hasReceipt: " + val.hasReceipt, "\nreceipt: ", val.receipt), "\nprem? ", PlayerConfig.instance.data.premium.ToString());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void activatePrem(bool pShowWindowUnlocked = false)
	{
		Debug.Log((object)"[ap]");
		PlayerConfig.instance.data.premium = true;
		PlayerConfig.saveData();
		Config.hasPremium = true;
		PremiumElementsChecker.checkElements();
		PlayerConfig.setFirebaseProp("have_premium", "yes");
		if (pShowWindowUnlocked)
		{
			if (!PlayerConfig.instance.data.tutorialFinished)
			{
				ScrollWindow.queueWindow("premium_unlocked");
			}
			else
			{
				ScrollWindow.hideAllEvent(pWithAnimation: false);
				ScrollWindow.showWindow("premium_unlocked");
			}
		}
		debugPremium();
	}

	private void activateSub(bool pShowWindowUnlocked = false)
	{
		if (pShowWindowUnlocked && PlayerConfig.instance.data.tutorialFinished)
		{
			ScrollWindow.hideAllEvent(pWithAnimation: false);
			ScrollWindow.showWindow("worldnet_sub");
		}
	}

	public unsafe void OnInitializeFailed(InitializationFailureReason error)
	{
		Debug.Log((object)("Cannot initialize IAP system: " + ((object)(*(InitializationFailureReason*)(&error))/*cast due to constrained. prefix*/).ToString()));
		Config.lockGameControls = false;
	}

	public unsafe void OnInitializeFailed(InitializationFailureReason error, string message)
	{
		Debug.Log((object)("Cannot initialize IAP system: " + ((object)(*(InitializationFailureReason*)(&error))/*cast due to constrained. prefix*/).ToString()));
		Debug.Log((object)message);
		Config.lockGameControls = false;
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		Config.lockGameControls = true;
		object obj;
		if (args == null)
		{
			obj = null;
		}
		else
		{
			Product purchasedProduct = args.purchasedProduct;
			if (purchasedProduct == null)
			{
				obj = null;
			}
			else
			{
				ProductDefinition definition = purchasedProduct.definition;
				obj = ((definition != null) ? definition.id : null);
			}
		}
		Debug.Log((object)("Purchase OK: " + (string?)obj));
		Debug.Log((object)("Args: " + (object)((args != null) ? args.purchasedProduct : null)));
		object obj2;
		if (args == null)
		{
			obj2 = null;
		}
		else
		{
			Product purchasedProduct2 = args.purchasedProduct;
			if (purchasedProduct2 == null)
			{
				obj2 = null;
			}
			else
			{
				ProductDefinition definition2 = purchasedProduct2.definition;
				obj2 = ((definition2 != null) ? definition2.id : null);
			}
		}
		if ((string?)obj2 == "premium")
		{
			Debug.Log((object)"[pp] process premium");
			return ProcessPremium(args);
		}
		object obj3;
		if (args == null)
		{
			obj3 = null;
		}
		else
		{
			Product purchasedProduct3 = args.purchasedProduct;
			if (purchasedProduct3 == null)
			{
				obj3 = null;
			}
			else
			{
				ProductDefinition definition3 = purchasedProduct3.definition;
				obj3 = ((definition3 != null) ? definition3.id : null);
			}
		}
		if ((string?)obj3 == "worldnet")
		{
			Debug.Log((object)"[pw] process worldnet");
			return ProcessWorldnet(args);
		}
		Debug.Log((object)"[pn] process nothing to be done");
		return (PurchaseProcessingResult)0;
	}

	public PurchaseProcessingResult ProcessPremium(PurchaseEventArgs args)
	{
		bool flag = true;
		bool flag2 = false;
		Debug.Log((object)"[prp]");
		if (!flag & flag2)
		{
			Debug.Log((object)"[np] 3");
			Config.lockGameControls = false;
			return (PurchaseProcessingResult)1;
		}
		object obj;
		if (args == null)
		{
			obj = null;
		}
		else
		{
			Product purchasedProduct = args.purchasedProduct;
			if (purchasedProduct == null)
			{
				obj = null;
			}
			else
			{
				ProductDefinition definition = purchasedProduct.definition;
				obj = ((definition != null) ? definition.id : null);
			}
		}
		string text = (string)obj;
		Debug.Log((object)("[vp] 2 " + flag));
		Debug.Log((object)("[lc] " + text));
		if (flag && string.Equals(text, "premium", StringComparison.OrdinalIgnoreCase))
		{
			activatePrem(pShowWindowUnlocked: true);
			Debug.Log((object)$"[ppp] '{text}'");
		}
		else
		{
			Debug.Log((object)"[np] 4");
		}
		Config.lockGameControls = false;
		debugPremium();
		return (PurchaseProcessingResult)0;
	}

	public PurchaseProcessingResult ProcessWorldnet(PurchaseEventArgs args)
	{
		bool flag = true;
		bool flag2 = false;
		PlayerConfig.instance.data.worldnet = "";
		if (!flag & flag2)
		{
			Debug.Log((object)"purchase pending");
			Config.lockGameControls = false;
			return (PurchaseProcessingResult)1;
		}
		Debug.Log((object)"check if valid");
		Config.lockGameControls = false;
		if (flag && string.Equals(args.purchasedProduct.definition.id, "worldnet", StringComparison.OrdinalIgnoreCase))
		{
			Debug.Log((object)"valid!");
			setWorldnetSubscription(args.purchasedProduct.transactionID);
			activateSub(pShowWindowUnlocked: true);
			Debug.Log((object)$"ProcessPurchase: PASS. Product: '{args.purchasedProduct.definition.id}'");
		}
		Debug.Log((object)"we are here");
		if (Config.lockGameControls)
		{
			Debug.Log((object)"lockgamecontrosl locked");
		}
		else
		{
			Debug.Log((object)"lockgamecontrosl not locked");
		}
		return (PurchaseProcessingResult)0;
	}

	public void setWorldnetSubscription(string pTransactionID)
	{
		PlayerConfig.instance.data.worldnet = pTransactionID;
		PlayerConfig.saveData();
	}

	public unsafe void OnPurchaseFailed(Product i, PurchaseFailureReason p)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)("[D] WORLDBOX PURCHASE FAILED  " + ((object)(*(PurchaseFailureReason*)(&p))/*cast due to constrained. prefix*/).ToString()));
		Config.lockGameControls = false;
		_ = 4;
		ScrollWindow.showWindow("premium_purchase_error");
		InitializePurchasing(50);
	}

	public void OnPurchaseFailed(Product i, PurchaseFailureDescription p)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)("[X] WORLDBOX PURCHASE FAILED  " + p.message));
		OnPurchaseFailed(i, p.reason);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void checkPremiumReceipt(string pReceipt, ref bool validPurchase, ref bool purchasePending)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void checkWorldnetReceipt(string pReceipt, ref bool validPurchase, ref bool purchasePending)
	{
	}

	public bool buyPremium()
	{
		activatePrem(pShowWindowUnlocked: true);
		Config.lockGameControls = false;
		return true;
	}

	public bool buyWorldNet()
	{
		return false;
	}

	private bool BuyProductID(string productId)
	{
		if (IsInitialized())
		{
			Product val = controller.products.WithID(productId);
			if (val == null)
			{
				return false;
			}
			if (val.availableToPurchase)
			{
				Debug.Log((object)$"Purchasing product asychronously: '{val.definition.id}'");
				Config.lockGameControls = true;
				controller.InitiatePurchase(val);
				if (ScrollWindow.windowLoaded("premium_menu") && ScrollWindow.isCurrentWindow("premium_menu"))
				{
					ScrollWindow.get("premium_menu").clickHide();
				}
				return true;
			}
			ScrollWindow.showWindow(productId + "_purchase_error");
			Debug.Log((object)"BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
			Config.lockGameControls = false;
			return false;
		}
		ScrollWindow.showWindow(productId + "_purchase_error");
		Debug.Log((object)"BuyProductID FAIL. Not initialized.");
		Config.lockGameControls = false;
		return false;
	}

	public void RestorePurchases()
	{
		restore_message = "Restoring...";
		restore_ui_buffering = true;
		if (Config.isEditor)
		{
			restore_ui_buffering = false;
		}
		if (!IsInitialized())
		{
			InitializePurchasing(66);
			Debug.Log((object)"RestorePurchases FAIL. Not initialized.");
			restore_message = "IAP not initialized, failed to restore purchases.";
		}
	}

	private bool IsInitialized()
	{
		if (controller != null)
		{
			return extensions != null;
		}
		return false;
	}
}
