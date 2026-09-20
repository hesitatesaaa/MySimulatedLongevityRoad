using Steamworks.Ugc;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopInfoIcons : MonoBehaviour
{
	public Text favorites;

	public Text upvotes;

	public Text comments;

	public Text subscription;

	private void OnEnable()
	{
		if (Config.game_loaded)
		{
			WorkshopMapData currentWorkshopMapData = SaveManager.currentWorkshopMapData;
			if (currentWorkshopMapData != null)
			{
				favorites.text = ((Item)(ref currentWorkshopMapData.workshop_item)).NumFavorites.ToString();
				upvotes.text = ((Item)(ref currentWorkshopMapData.workshop_item)).VotesUp.ToString();
				comments.text = ((Item)(ref currentWorkshopMapData.workshop_item)).NumComments.ToString();
				subscription.text = ((Item)(ref currentWorkshopMapData.workshop_item)).NumSubscriptions.ToString();
			}
		}
	}
}
