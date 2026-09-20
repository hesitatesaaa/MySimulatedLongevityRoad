using UnityEngine;

public class WorkshopOpenSteamWorkshop : MonoBehaviour
{
	public static string fileID;

	public void playWorkShopMap()
	{
		Application.OpenURL($"steam://url/SteamWorkshopPage/{1206560u}");
	}

	public void openWorkShopAgreement()
	{
		Application.OpenURL("steam://url/CommunityFilePage/" + fileID);
		((Component)this).gameObject.SetActive(false);
	}
}
