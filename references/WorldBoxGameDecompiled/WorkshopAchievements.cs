using System;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Ugc;
using UnityEngine;

internal class WorkshopAchievements
{
	internal static void checkAchievements()
	{
		SteamSDK.steamInitialized.Then((Action)delegate
		{
			countUsersWorkshopMaps();
		}).Catch((Action<Exception>)delegate(Exception err)
		{
			Debug.Log((object)"Error happened while getting users maps");
			Debug.Log((object)err);
		});
	}

	internal static async Task countUsersWorkshopMaps()
	{
		Query val = Query.ItemsReadyToUse;
		val = ((Query)(ref val)).WhereUserPublished(default(SteamId));
		Query tQuery = ((Query)(ref val)).WithTag("World");
		int tTotalVotes = 0;
		int tTotalCount = 1;
		int tTotalFound = 0;
		int tPage = 1;
		while (tTotalCount > tTotalFound)
		{
			ResultPage? val2 = await ((Query)(ref tQuery)).GetPageAsync(tPage++);
			if (!val2.HasValue)
			{
				continue;
			}
			tTotalCount = val2.Value.TotalCount;
			tTotalFound += val2.Value.ResultCount;
			ResultPage value = val2.Value;
			foreach (Item entry in ((ResultPage)(ref value)).Entries)
			{
				Item current = entry;
				tTotalVotes += (int)((Item)(ref current)).VotesUp;
			}
		}
		if (tTotalCount > World.world.game_stats.data.workshopUploads)
		{
			World.world.game_stats.data.workshopUploads = tTotalCount;
		}
		AchievementLibrary.checkSteamMapUploads();
	}
}
