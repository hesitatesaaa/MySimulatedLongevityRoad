using System.Collections.Generic;

public class FavoriteItemListComponent : ComponentListBase<FavoriteItemListElement, Item, ItemData, FavoriteItemListComponent>
{
	private List<NanoObject> _meta_objects = new List<NanoObject>();

	protected override MetaType meta_type => MetaType.Item;

	protected override void setupSortingTabs()
	{
		sorting_tab.tryAddButton("ui/Icons/iconAge", "sort_by_age", show, delegate
		{
			current_sort = sortByAge;
		});
		sorting_tab.tryAddButton("ui/Icons/iconKills", "sort_by_kills", show, delegate
		{
			current_sort = sortByKills;
		});
		sorting_tab.tryAddButton("ui/Icons/iconDamage", "sort_by_damage", show, delegate
		{
			current_sort = sortByDamage;
		});
		sorting_tab.tryAddButton("ui/Icons/iconArmor", "sort_by_armor", show, delegate
		{
			current_sort = sortByArmor;
		});
		sorting_tab.tryAddButton("ui/Icons/iconItemType", "sort_by_type", show, delegate
		{
			current_sort = sortByType;
		});
		sorting_tab.tryAddButton("ui/Icons/iconItemQuality", "sort_by_quality", show, delegate
		{
			current_sort = sortByQuality;
		});
		sorting_tab.tryAddButton("ui/Icons/iconCity", "sort_by_city", show, delegate
		{
			current_sort = sortByCity;
		});
		sorting_tab.tryAddButton("ui/Icons/iconHumans", "sort_by_owner", show, delegate
		{
			current_sort = sortByOwner;
		});
	}

	protected override IEnumerable<Item> getObjectsList()
	{
		_meta_objects.Clear();
		foreach (Item item in World.world.items)
		{
			if (!item.isRekt() && item.isFavorite())
			{
				_meta_objects.Add(item);
				if (item.hasCity())
				{
					_meta_objects.Add(item.getCity());
				}
				if (item.hasActor())
				{
					_meta_objects.Add(item.getActor());
				}
				yield return item;
			}
		}
	}

	public static int sortByAge(Item pItem1, Item pItem2)
	{
		return -pItem2.data.created_time.CompareTo(pItem1.data.created_time);
	}

	public static int sortByKills(Item pItem1, Item pItem2)
	{
		return pItem2.data.kills.CompareTo(pItem1.data.kills);
	}

	public static int sortByType(Item pItem1, Item pItem2)
	{
		return pItem2.getAsset().equipment_type.CompareTo(pItem1.getAsset().equipment_type);
	}

	public static int sortByQuality(Item pItem1, Item pItem2)
	{
		return pItem2.getQuality().CompareTo(pItem1.getQuality());
	}

	public static int sortByCity(Item pItem1, Item pItem2)
	{
		int num = pItem1.hasCity().CompareTo(pItem2.hasCity());
		if (num != 0)
		{
			return num;
		}
		if (pItem1.hasCity() && pItem2.hasCity())
		{
			int num2 = pItem2.getCity().kingdom.CompareTo(pItem1.getCity().kingdom);
			if (num2 != 0)
			{
				return num2;
			}
			return pItem2.getCity().name.CompareTo(pItem1.getCity().name);
		}
		return pItem2.name.CompareTo(pItem1.name);
	}

	public static int sortByOwner(Item pItem1, Item pItem2)
	{
		int num = pItem1.hasActor().CompareTo(pItem2.hasActor());
		if (num != 0)
		{
			return num;
		}
		if (pItem1.hasActor() && pItem2.hasActor())
		{
			Actor actor = pItem1.getActor();
			Actor actor2 = pItem2.getActor();
			int num2 = actor.kingdom.CompareTo(actor2.kingdom);
			if (num2 != 0)
			{
				return num2;
			}
			int num3 = actor.hasCity().CompareTo(actor2.hasCity());
			if (num3 != 0)
			{
				return num3;
			}
			if (actor.hasCity() && actor2.hasCity())
			{
				int num4 = actor.getCity().name.CompareTo(actor2.getCity().name);
				if (num4 != 0)
				{
					return num4;
				}
			}
			return pItem2.getActor().name.CompareTo(pItem1.getActor().name);
		}
		return pItem2.name.CompareTo(pItem1.name);
	}

	public static int sortByDamage(Item pItem1, Item pItem2)
	{
		return pItem2.getFullStats()["damage"].CompareTo(pItem1.getFullStats()["damage"]);
	}

	public static int sortByArmor(Item pItem1, Item pItem2)
	{
		return pItem2.getFullStats()["armor"].CompareTo(pItem1.getFullStats()["armor"]);
	}

	public override void clear()
	{
		base.clear();
		_meta_objects.Clear();
	}

	public override bool checkRefreshWindow()
	{
		foreach (NanoObject meta_object in _meta_objects)
		{
			if (meta_object.isRekt())
			{
				return true;
			}
		}
		return base.checkRefreshWindow();
	}
}
