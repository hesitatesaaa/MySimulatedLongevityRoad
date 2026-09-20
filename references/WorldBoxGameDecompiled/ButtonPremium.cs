using UnityEngine;

public class ButtonPremium : MonoBehaviour
{
	public void clickPremium()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		PlayerConfig.setFirebaseProp("clicked_buy_premium", "yes");
		Analytics.LogEvent("clicked_buy_premium");
		if ((int)Application.internetReachability == 0)
		{
			ScrollWindow.showWindow("premium_purchase_error");
		}
		else
		{
			InAppManager.instance.buyPremium();
		}
	}
}
