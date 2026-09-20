using UnityEngine;

public class RateUsRemover : MonoBehaviour
{
	public void clickedRateUs()
	{
		PlayerConfig.instance.data.lastRateID = 12;
		((Component)this).gameObject.SetActive(false);
		PlayerConfig.saveData();
	}
}
