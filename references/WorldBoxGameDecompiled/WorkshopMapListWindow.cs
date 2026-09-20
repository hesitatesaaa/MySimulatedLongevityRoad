using System;
using System.Collections.Generic;
using System.IO;
using Steamworks.Ugc;
using UnityEngine;

public class WorkshopMapListWindow : MonoBehaviour
{
	public WorkshopMapElement elementPrefab;

	private Dictionary<string, Sprite> cached_sprites = new Dictionary<string, Sprite>();

	private List<WorkshopMapElement> elements = new List<WorkshopMapElement>();

	public Transform transformContent;

	private float _timer;

	private bool _no_items;

	private Queue<Item> _showQueue = new Queue<Item>();

	private void OnEnable()
	{
		if (!Config.game_loaded)
		{
			return;
		}
		_timer = 0.3f;
		foreach (WorkshopMapElement element in elements)
		{
			Object.Destroy((Object)(object)((Component)element).gameObject);
		}
		elements.Clear();
		SteamSDK.steamInitialized.Then((Action)delegate
		{
			prepareList();
		}).Catch((Action<Exception>)delegate(Exception err)
		{
			Debug.LogError((object)err);
			ErrorWindow.errorMessage = "Error happened while connecting to Steam Workshop:\n" + err.Message.ToString();
			ScrollWindow.get("error_with_reason").clickShow();
		});
	}

	private void OnDisable()
	{
		_showQueue.Clear();
	}

	private void Update()
	{
		if (_timer > 0f)
		{
			_timer -= Time.deltaTime;
		}
		else
		{
			_timer = 0.015f;
			showNextItemFromQueue();
		}
		if (_no_items)
		{
			_no_items = false;
			ScrollWindow.showWindow("steam_workshop_empty");
		}
	}

	private async void prepareList()
	{
		List<Item> list = await WorkshopMaps.listWorkshopMaps();
		if (list.Count > 0)
		{
			foreach (Item item in list)
			{
				_showQueue.Enqueue(item);
			}
			AchievementLibrary.checkSteamMapDownloads(list.Count);
		}
		else
		{
			_no_items = true;
		}
	}

	private void showNextItemFromQueue()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (_showQueue.Count != 0)
		{
			Item pSteamworksItem = _showQueue.Dequeue();
			renderMapElement(pSteamworksItem);
		}
	}

	private WorkshopMapData loadMapDataFromStorage(Item pSteamworksItem)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		string text = SaveManager.generatePngSmallPreviewPath(((Item)(ref pSteamworksItem)).Directory);
		WorkshopMapData workshopMapData = new WorkshopMapData();
		workshopMapData.main_path = ((Item)(ref pSteamworksItem)).Directory;
		workshopMapData.workshop_item = pSteamworksItem;
		if (!string.IsNullOrEmpty(text) && File.Exists(text))
		{
			if (cached_sprites.ContainsKey(text))
			{
				workshopMapData.sprite_small_preview = cached_sprites[text];
			}
			else
			{
				try
				{
					byte[] array = File.ReadAllBytes(text);
					Texture2D val = new Texture2D(32, 32);
					((Texture)val).anisoLevel = 0;
					((Texture)val).filterMode = (FilterMode)0;
					if (ImageConversion.LoadImage(val, array))
					{
						workshopMapData.sprite_small_preview = Sprite.Create(val, new Rect(0f, 0f, 32f, 32f), new Vector2(0.5f, 0.5f));
						cached_sprites.Add(text, workshopMapData.sprite_small_preview);
					}
				}
				catch (Exception)
				{
				}
			}
		}
		MapMetaData metaFor = SaveManager.getMetaFor(((Item)(ref pSteamworksItem)).Directory);
		bool flag = false;
		if (!string.IsNullOrWhiteSpace(((Item)(ref pSteamworksItem)).Title) && metaFor.mapStats.name != ((Item)(ref pSteamworksItem)).Title)
		{
			metaFor.mapStats.name = ((Item)(ref pSteamworksItem)).Title;
			flag = true;
		}
		if (metaFor.mapStats.description != ((Item)(ref pSteamworksItem)).Description)
		{
			metaFor.mapStats.description = ((Item)(ref pSteamworksItem)).Description;
			flag = true;
		}
		if (flag)
		{
			SaveManager.saveMetaIn(((Item)(ref pSteamworksItem)).Directory, metaFor);
		}
		workshopMapData.meta_data_map = metaFor;
		return workshopMapData;
	}

	private void renderMapElement(Item pSteamworksItem)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		WorkshopMapElement workshopMapElement = Object.Instantiate<WorkshopMapElement>(elementPrefab, transformContent);
		elements.Add(workshopMapElement);
		WorkshopMapData pData = loadMapDataFromStorage(pSteamworksItem);
		workshopMapElement.load(pData);
	}
}
