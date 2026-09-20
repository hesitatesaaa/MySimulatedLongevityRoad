using Steamworks;
using Steamworks.Ugc;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopMapElement : MonoBehaviour
{
	private WorkshopMapData data;

	public Image image;

	public Text textName;

	public Text textKingdoms;

	public Text textCities;

	public Text textPopulation;

	public Text textMobs;

	public Text textUpvotes;

	public Text textComments;

	public Image mainBackground;

	public Image ayeIcon;

	public unsafe void load(WorkshopMapData pData)
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		data = pData;
		textName.text = data.meta_data_map.mapStats.name;
		textKingdoms.text = data.meta_data_map.kingdoms.ToString();
		textCities.text = data.meta_data_map.cities.ToString();
		textPopulation.text = data.meta_data_map.population.ToString();
		textMobs.text = data.meta_data_map.mobs.ToString();
		textUpvotes.text = ((Item)(ref data.workshop_item)).VotesUp.ToString();
		textComments.text = ((Item)(ref data.workshop_item)).NumComments.ToString();
		image.sprite = data.sprite_small_preview;
		Friend owner = ((Item)(ref data.workshop_item)).Owner;
		if (((object)(*(SteamId*)(&owner.Id))/*cast due to constrained. prefix*/).ToString() == Config.steam_id)
		{
			((Graphic)textName).color = Toolbox.makeColor("#3DDEFF");
			((Component)ayeIcon).gameObject.SetActive(true);
		}
		else
		{
			((Graphic)textName).color = Toolbox.makeColor("#FF9B1C");
			((Component)ayeIcon).gameObject.SetActive(false);
		}
		((Object)((Component)this).gameObject).name = "WorkshopMapElement " + data.meta_data_map.mapStats.name;
	}

	public void clickWorkshopMap()
	{
		SaveManager.currentWorkshopMapData = data;
		ScrollWindow.showWindow("steam_workshop_play_world");
	}
}
