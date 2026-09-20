using Steamworks.Ugc;
using UnityEngine;

public class WorkshopMapData
{
	public string main_path;

	public string preview_image_path;

	public Sprite sprite_small_preview;

	public MapMetaData meta_data_map;

	public WorkshopMapMetaData meta_data_workshop;

	public Item workshop_item;

	public static WorkshopMapData currentMapToWorkshop()
	{
		WorkshopMapData workshopMapData = new WorkshopMapData();
		string text = SaveManager.generateWorkshopPath();
		SavedMap savedMap = SaveManager.saveWorldToDirectory(text);
		workshopMapData.meta_data_map = savedMap.getMeta();
		workshopMapData.preview_image_path = text + "preview.png";
		workshopMapData.main_path = text;
		return workshopMapData;
	}
}
