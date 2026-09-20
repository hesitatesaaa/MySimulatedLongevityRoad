using Steamworks.Ugc;
using UnityEngine;

public class WorkshopHelpers : MonoBehaviour
{
	public const string color_own_map = "#3DDEFF";

	public const string color_other_map = "#FF9B1C";

	public void openCurrentMapInWorkshop()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Application.OpenURL("steam://url/CommunityFilePage/" + ((object)((Item)(ref SaveManager.currentWorkshopMapData.workshop_item)).Id/*cast due to constrained. prefix*/).ToString());
	}

	public void openUploadWorld()
	{
		SaveManager.clearCurrentSelectedWorld();
		ScrollWindow.showWindow("steam_workshop_upload_world");
	}

	public void openBrowseWorlds()
	{
		SaveManager.clearCurrentSelectedWorld();
		ScrollWindow.showWindow("steam_workshop_browse");
	}
}
