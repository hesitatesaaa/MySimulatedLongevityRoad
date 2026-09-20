using System.Collections.Generic;

public class ActorDebugAssetsComponent : BaseDebugAssetsComponent<ActorAsset, ActorDebugAssetElement, ActorAssetElementPlace>
{
	protected override List<ActorAsset> getAssetsList()
	{
		return AssetManager.actor_library.list;
	}

	protected override void init()
	{
		sorting_tab.addButton("ui/Icons/iconHealth", "sort_by_health", base.setDataResorted, delegate
		{
			list_assets_sorted = list_assets_sorting;
			list_assets_sorted.Sort(sortByHealth);
			checkReverseSort();
		});
		sorting_tab.addButton("ui/Icons/iconDamage", "sort_by_damage", base.setDataResorted, delegate
		{
			list_assets_sorted = list_assets_sorting;
			list_assets_sorted.Sort(sortByDamage);
			checkReverseSort();
		});
		sorting_tab.addButton("ui/Icons/iconSpeed", "sort_by_speed", base.setDataResorted, delegate
		{
			list_assets_sorted = list_assets_sorting;
			list_assets_sorted.Sort(sortBySpeed);
			checkReverseSort();
		});
		sorting_tab.addButton("ui/Icons/iconAge", "sort_by_lifespan", base.setDataResorted, delegate
		{
			list_assets_sorted = list_assets_sorting;
			list_assets_sorted.Sort(sortByLifespan);
			checkReverseSort();
		});
		base.init();
	}

	private int sortByHealth(ActorAsset pObject1, ActorAsset pObject2)
	{
		return -pObject1.getStatsForOverview()["health"].CompareTo(pObject2.getStatsForOverview()["health"]);
	}

	private int sortByDamage(ActorAsset pObject1, ActorAsset pObject2)
	{
		return -pObject1.getStatsForOverview()["damage"].CompareTo(pObject2.getStatsForOverview()["damage"]);
	}

	private int sortBySpeed(ActorAsset pObject1, ActorAsset pObject2)
	{
		return -pObject1.getStatsForOverview()["speed"].CompareTo(pObject2.getStatsForOverview()["speed"]);
	}

	private int sortByLifespan(ActorAsset pObject1, ActorAsset pObject2)
	{
		return -pObject1.getStatsForOverview()["lifespan"].CompareTo(pObject2.getStatsForOverview()["lifespan"]);
	}

	protected override List<ActorAsset> getListCivsSort()
	{
		bool flag = sorting_tab.getCurrentButton().getState() == SortButtonState.Up;
		List<ActorAsset> list = new List<ActorAsset>();
		foreach (ActorAsset assets in getAssetsList())
		{
			if (assets.civ == flag)
			{
				list.Add(assets);
			}
		}
		return list;
	}
}
