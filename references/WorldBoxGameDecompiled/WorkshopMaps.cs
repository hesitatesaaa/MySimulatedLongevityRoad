using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RSG;
using Steamworks;
using Steamworks.Data;
using Steamworks.Ugc;
using UnityEngine;

public static class WorkshopMaps
{
	internal static WorkshopUploadProgress uploadProgressTracker = new WorkshopUploadProgress();

	internal static float uploadProgress = 0f;

	public static PublishedFileId uploaded_file_id;

	internal static List<Item> foundMaps = new List<Item>();

	public static bool workshopAvailable()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (SteamSDK.steamInitialized == null)
		{
			return false;
		}
		if ((int)SteamSDK.steamInitialized.CurState == 2)
		{
			return true;
		}
		return false;
	}

	internal unsafe static Promise uploadMap()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		Promise promise = new Promise();
		uploadProgress = 0f;
		WorkshopMapData workshopMapData = (SaveManager.currentWorkshopMapData = WorkshopMapData.currentMapToWorkshop());
		MapMetaData meta_data_map = workshopMapData.meta_data_map;
		if (SaveManager.currentWorkshopMapData == null)
		{
			promise.Reject(new Exception("Missing world data"));
			return promise;
		}
		if (!MapSizeLibrary.isSizeValid(meta_data_map.width))
		{
			promise.Reject(new Exception("Not a valid world size!"));
			return promise;
		}
		if (meta_data_map.width != meta_data_map.height)
		{
			promise.Reject(new Exception("Not a square world!"));
			return promise;
		}
		MapMetaData meta_data_map2 = workshopMapData.meta_data_map;
		string name = meta_data_map2.mapStats.name;
		string description = meta_data_map2.mapStats.description;
		if (string.IsNullOrWhiteSpace(name))
		{
			promise.Reject(new Exception("Give your world a name!"));
			return promise;
		}
		if (string.IsNullOrWhiteSpace(description))
		{
			promise.Reject(new Exception("Give your world a description!"));
			return promise;
		}
		string main_path = workshopMapData.main_path;
		string preview_image_path = workshopMapData.preview_image_path;
		Editor newCommunityFile = Editor.NewCommunityFile;
		Editor val = ((Editor)(ref newCommunityFile)).WithTag("World");
		if (!string.IsNullOrWhiteSpace(name))
		{
			val = ((Editor)(ref val)).WithTitle(name);
		}
		if (!string.IsNullOrWhiteSpace(description))
		{
			val = ((Editor)(ref val)).WithDescription(description);
		}
		if (!string.IsNullOrWhiteSpace(preview_image_path))
		{
			val = ((Editor)(ref val)).WithPreviewFile(preview_image_path);
		}
		if (!string.IsNullOrWhiteSpace(main_path))
		{
			val = ((Editor)(ref val)).WithContent(main_path);
		}
		val = ((Editor)(ref val)).WithFriendsOnlyVisibility();
		uploadProgressTracker = new WorkshopUploadProgress();
		((Editor)(ref val)).SubmitAsync((IProgress<float>)uploadProgressTracker).ContinueWith(delegate(Task<PublishResult> taskResult)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Invalid comparison between Unknown and I4
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			if (taskResult.Status == TaskStatus.RanToCompletion)
			{
				PublishResult result = taskResult.Result;
				if (!((PublishResult)(ref result)).Success)
				{
					Debug.LogError((object)"Error when uploading Workshop world");
				}
				if (result.NeedsWorkshopAgreement)
				{
					Debug.Log((object)"w: Needs Workshop Agreement");
					WorkshopUploadingWorldWindow.needsWorkshopAgreement = true;
					WorkshopOpenSteamWorkshop.fileID = ((object)(*(PublishedFileId*)(&result.FileId))/*cast due to constrained. prefix*/).ToString();
				}
				if ((int)result.Result != 1)
				{
					Debug.LogError((object)result.Result);
					promise.Reject(new Exception("Something went wrong: " + ((object)(*(Result*)(&result.Result))/*cast due to constrained. prefix*/).ToString()));
				}
				else
				{
					uploaded_file_id = result.FileId;
					World.world.game_stats.data.workshopUploads++;
					WorkshopAchievements.checkAchievements();
					promise.Resolve();
				}
			}
			else
			{
				promise.Reject(taskResult.Exception.GetBaseException());
			}
		}, TaskScheduler.FromCurrentSynchronizationContext());
		return promise;
	}

	internal static async Task<List<Item>> listWorkshopMaps(bool pOrder = false, bool pByFriends = false)
	{
		Query val = Query.ItemsReadyToUse;
		val = ((Query)(ref val)).WhereUserSubscribed(default(SteamId));
		Query q = ((Query)(ref val)).WithTag("World");
		if (pByFriends)
		{
			q = ((Query)(ref q)).CreatedByFriends();
		}
		q = ((!pOrder) ? ((Query)(ref q)).SortByCreationDateAsc() : ((Query)(ref q)).SortByCreationDate());
		foundMaps.Clear();
		int num = 1;
		int totalFound = 0;
		int tPage = 1;
		while (num > totalFound)
		{
			ResultPage? val2 = await ((Query)(ref q)).GetPageAsync(tPage++);
			if (!val2.HasValue)
			{
				break;
			}
			num = val2.Value.TotalCount;
			totalFound += val2.Value.ResultCount;
			Debug.Log((object)$"w: This page has {val2.Value.ResultCount} results");
			ResultPage value = val2.Value;
			foreach (Item entry in ((ResultPage)(ref value)).Entries)
			{
				Item current = entry;
				Debug.Log((object)("w: Entry: " + ((Item)(ref current)).Title));
				if (((Item)(ref current)).IsInstalled && !((Item)(ref current)).IsDownloadPending && !((Item)(ref current)).IsDownloading)
				{
					if (!filesPresent(current))
					{
						Debug.Log((object)"w: Incomplete files for Workshop Item, skipped");
					}
					else
					{
						foundMaps.Add(current);
					}
				}
			}
			Debug.Log((object)val2.Value.ResultCount);
			Debug.Log((object)val2.Value.TotalCount);
		}
		return foundMaps;
	}

	internal static bool filesPresent(Item pEntry)
	{
		if (!Directory.Exists(((Item)(ref pEntry)).Directory))
		{
			return false;
		}
		string[] files = Directory.GetFiles(((Item)(ref pEntry)).Directory);
		Debug.Log((object)("w: " + ((Item)(ref pEntry)).Directory + " with " + files.Length + " Files"));
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		string[] array = files;
		foreach (string text in array)
		{
			if (text.Contains("map.wbox"))
			{
				flag = true;
			}
			else if (text.Contains("map.meta"))
			{
				flag4 = true;
			}
			else if (text.Contains("preview.png"))
			{
				flag2 = true;
			}
			else if (text.Contains("preview_small.png"))
			{
				flag3 = true;
			}
		}
		if (!flag)
		{
			Debug.Log((object)"w: Missing Map");
		}
		if (!flag2)
		{
			Debug.Log((object)"w: Missing Preview");
		}
		if (!flag3)
		{
			Debug.Log((object)"w: Missing PreviewSmall");
		}
		if (!flag4)
		{
			Debug.Log((object)"w: Missing Meta");
		}
		if (!flag4 || !flag || !flag2 || !flag3)
		{
			return false;
		}
		return true;
	}
}
